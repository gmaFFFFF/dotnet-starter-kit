using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;

namespace System.IO;

public static class FileSystemWatcherРасширения {
    public static IObservable<FileSystemEventArgs> ToObservable(this ИFileSystemWatcherОбёртка @this) {
        var fswObservableWrapper = Observable.Defer(() => {
                @this.EnableRaisingEvents = true;
                return Observable.Return(@this);
            })
            .Catch((Exception exc) =>
                Observable.Throw<ИFileSystemWatcherОбёртка>(НормализуйОшибкиFileSystemWatcher(@this.Path, exc)))
            .Publish()
            .RefCount(2);

        var событияФС = fswObservableWrapper
            .SelectMany(fsw =>
                Observable.Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Created += h, h => fsw.Created -= h),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Changed += h, h => fsw.Changed -= h),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                        h => fsw.Deleted += h, h => fsw.Deleted -= h),
                    Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(
                        h => fsw.Renamed += h, h => fsw.Renamed -= h))
            )
            .Select(evt => evt.EventArgs);

        var ошибкиНаблюдения = fswObservableWrapper
            .SelectMany(fsw =>
                Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
                    h => fsw.Error += h, h => fsw.Error -= h))
            .SelectMany(evt => Observable.Throw<FileSystemEventArgs>(
                НормализуйОшибкиFileSystemWatcher(@this.Path, evt.EventArgs.GetException())));

        return событияФС
            .Merge(ошибкиНаблюдения)
            .Finally(() => {
                @this.EnableRaisingEvents = false;
                @this.Dispose();
            });
    }


    /// <summary>
    ///     Создаёт снимок файловой системы согласно параметрам ИFileSystemWatcherОбёртка и начинает следить за её изменениями
    /// </summary>
    /// <remarks>
    ///     Ограничение: Теряются события в промежутке между первичной инициализацией (получение оглавления) и запуском
    ///     наблюдателя
    /// </remarks>
    public static IObservable<IChangeSet<Exceptional<FileSystemInfo>, string>> ToObservableChangeSet(
        this ИFileSystemWatcherОбёртка @this, out Task<ISourceCache<Exceptional<FileSystemInfo>, string>> снимок) {
        var выходнойПараметр = new TaskCompletionSource<ISourceCache<Exceptional<FileSystemInfo>, string>>();
        снимок = выходнойПараметр.Task;

        return ObservableChangeSet.Create(снимокФС => {
                выходнойПараметр.SetResult(снимокФС);
                var подпискаОбщая = Disposable.Empty;

                // Снимок файловой системы
                var оглавлениеКорневойПапки = Observable
                    .Defer(() => Observable.Return(new DirectoryInfo(@this.Path)))
                    .Catch((Exception exc) =>
                        Observable.Throw<DirectoryInfo>(НормализуйОшибкиFileSystemWatcher(@this.Path, exc)))
                    .SelectMany(папка => ДайОглавлениеПапки(папка, @this.Filter, @this.IncludeSubdirectories));

                // События файловой системы
                var наблюдательФС = @this.ToObservable().Publish();
                var НаблюдательФСПодписка = Disposable.Empty;

                var ошибкиПодписка = наблюдательФС.Subscribe(_ => { },
                    ошибка => ОбработайОшибку(снимокФС, ошибка, подпискаОбщая));

                var новыеПодписка = ПричешиСобытияСоздания(
                        from событие in наблюдательФС
                        where событие.ChangeType is WatcherChangeTypes.Created
                        select событие.FullPath
                        , снимокФС)
                    .Subscribe(снимокФС.AddOrUpdate);

                var измененныеПодписка = ПричешиСобытияИзменения(
                        from событие in наблюдательФС
                        where событие.ChangeType is WatcherChangeTypes.Changed
                        select событие.FullPath)
                    .Subscribe(снимокФС.AddOrUpdate);

                var удаленныеПодписка = ПричешиСобытияУдаления(
                        from событие in наблюдательФС
                        where событие.ChangeType is WatcherChangeTypes.Deleted
                        select событие.FullPath
                        , снимокФС)
                    .Subscribe(снимокФС.RemoveKey);

                var переименованныеНовыеПодписка = ПричешиСобытияСоздания(
                        from efsi in наблюдательФС.OfType<RenamedEventArgs>()
                        select efsi.FullPath
                        , снимокФС)
                    .Subscribe(снимокФС.AddOrUpdate);

                var переименованныеУдаленныеПодписка = ПричешиСобытияУдаления(
                        from efsi in наблюдательФС.OfType<RenamedEventArgs>()
                        select efsi.OldFullPath
                        , снимокФС)
                    .Subscribe(снимокФС.RemoveKey);


                // Заполняем кэш текущими данными, а затем подписываемся на события файловой системы
                // Из-за этого будут потеряны события между получением оглавления и подпиской
                // Если подписку на наблюдательФС не привязывать к перечислению оглавления, 
                // то получим неопределённое поведение: чаще всего всё будет ок, но редко может что-то 
                // произойти.
                var существующиеПодписка = оглавлениеКорневойПапки
                    .Subscribe(снимокФС.AddOrUpdate, () => НаблюдательФСПодписка = наблюдательФС.Connect());

                подпискаОбщая = new CompositeDisposable(
                    НаблюдательФСПодписка, ошибкиПодписка,
                    существующиеПодписка, новыеПодписка, измененныеПодписка, удаленныеПодписка,
                    переименованныеНовыеПодписка, переименованныеУдаленныеПодписка);

                return подпискаОбщая;
            },
            (Func<Exceptional<FileSystemInfo>, string>)СгенерируйКлючФайла);

        void ОбработайОшибку(ISourceCache<Exceptional<FileSystemInfo>, string> снимокФС, Exception ошибка,
            IDisposable подписка) {
            var путь = (string?)ошибка.Data[ExceptionDataPathFolderKey] ?? "";
            var отмена =
                new OperationCanceledException("Наблюдение за папкой прекращено вследствие возникновения ошибки",
                    ошибка);
            снимокФС.AddOrUpdate(new Exceptional<FileSystemInfo>(отмена.ДобавитьПуть(путь)));
            подписка.Dispose();
        }

        IObservable<Exceptional<FileSystemInfo>> ПричешиСобытияСоздания(IObservable<string> событияСоздания,
            ISourceCache<Exceptional<FileSystemInfo>, string> снимокФС) {
            var поступившие = from путь in событияСоздания
                from fsi in путь.ВFileSystemInfo()
                select fsi;

            var новыеПапки =
                from efsi in поступившие
                where !efsi.HasException && efsi.Value is DirectoryInfo
                select efsi.Value as DirectoryInfo;

            // Если папка перемещена с того же диска, то внутри папки никаких событий не будет
            // поэтому нужно вручную загрузить содержимое папки
            var оглавлениеНовыхПапок =
                from папка in новыеПапки
                select ДайОглавлениеПапки(папка, @this.Filter, @this.IncludeSubdirectories,
                    CurrentThreadScheduler.Instance).ToListObservable();

            // Если папка перемещена с другого диска, то все файлы внутри папки попадут в «поступившие» и будут записаны в кэш
            // поэтому фильтруем те файлы, которые уже попали в кэш.
            var предикатУчтённые =
                from путь in новыеПапки
                let начальныйПуть = $"{путь}{Path.DirectorySeparatorChar}"
                select (Func<Exceptional<FileSystemInfo>, bool>)(efsi =>
                    СгенерируйКлючФайла(efsi).StartsWith(начальныйПуть));

            var учтённые = снимокФС
                .Connect()
                .Filter(предикатУчтённые)
                .Transform(СгенерируйКлючФайла)
                .ToCollection();

            // Небольшая задержка позволит пропустить вперёд события от FileSystemWatcher
            var неучтенныеОбъекты = оглавлениеНовыхПапок
                .Zip(учтённые,
                    (возможноНовые, существующие) =>
                        from н in возможноНовые
                        where !существующие.Contains(СгенерируйКлючФайла(н))
                        select н
                )
                .SelectMany(x => x);


            return поступившие.Merge(@this.IncludeSubdirectories
                ? неучтенныеОбъекты
                : Observable.Empty<Exceptional<FileSystemInfo>>());
        }

        IObservable<Exceptional<FileSystemInfo>> ПричешиСобытияИзменения(IObservable<string> событияИзменения) {
            var поступившие = from путь in событияИзменения
                from fsi in путь.ВFileSystemInfo()
                select fsi;

            return поступившие;
        }

        IObservable<string> ПричешиСобытияУдаления(IObservable<string> событияУдаления,
            ISourceCache<Exceptional<FileSystemInfo>, string> снимокФС) {
            // Не учитываются перемещения содержимого подкаталога.
            // Поэтому по каждому удаленному каталогу нужно убрать информацию о его вложениях
            var предикатДочерние =
                from путь in событияУдаления
                let начальныйПуть = $"{путь}{Path.DirectorySeparatorChar}"
                select (Func<Exceptional<FileSystemInfo>, bool>)(efsi =>
                    СгенерируйКлючФайла(efsi).StartsWith(начальныйПуть));


            var вложенные = снимокФС
                .Connect()
                .Filter(предикатДочерние)
                .Transform(СгенерируйКлючФайла)
                .ToCollection()
                .SelectMany(x => x);

            return событияУдаления.Merge(вложенные);
        }
    }

    public static IObservable<IChangeSet<Exceptional<FileSystemInfo>, string>> ToObservableChangeSet(
        this ИFileSystemWatcherОбёртка @this) {
        return @this.ToObservableChangeSet(out _);
    }

    public static IObservable<Exceptional<FileSystemInfo>> ДайОглавлениеПапки(DirectoryInfo папка,
        string searchPattern = "", bool рекурсивноЛи = false,
        IScheduler? планировщик = null) {
        if (!папка.Exists)
            return Observable.Return(Exceptional.From<FileSystemInfo>(НовоеDirectoryNotFoundException(папка.FullName)));

        var настройки = new EnumerationOptions {
            IgnoreInaccessible = true,
            RecurseSubdirectories = рекурсивноЛи,
            ReturnSpecialDirectories = false,
            MatchType = MatchType.Win32
        };

        var сформируйОглавление = (IScheduler планировщик) => папка
            .EnumerateFileSystemInfos(searchPattern, настройки)
            .ToExceptional()
            .ToObservable(планировщик);

        return планировщик is not null
            ? сформируйОглавление(планировщик)
            : сформируйОглавление(TaskPoolScheduler.Default);
    }

    #region Служебные

    #region Генерация ключа

    private static string СгенерируйКлючФайла(Exceptional<FileSystemInfo> fs) {
        return fs.Match(СгенериКлючСобытияФС, СгенериКлючСобытияФС);
    }

    private static string СгенериКлючСобытияФС(FileSystemInfo fsi) {
        return fsi is DirectoryInfo
            ? $"{fsi.FullName}{Path.DirectorySeparatorChar}"
            : fsi.FullName;
    }

    private static string СгенериКлючСобытияФС(Exception exc) {
        return exc.Data.Contains(ExceptionDataPathFolderKey)
            ? $"{exc.Data[ExceptionDataPathFolderKey]}:{exc.GetType().Name}"
            : $"Exceptions:{exc.GetType().Name}";
    }

    #endregion

    private static Exceptional<FileSystemInfo> ВFileSystemInfo(this string путь) {
        if (File.Exists(путь))
            try {
                return Exceptional.From<FileSystemInfo>(new FileInfo(путь));
            }
            catch (Exception ex) {
                return Exceptional.From<FileSystemInfo>(ex.ДобавитьПуть(путь));
            }

        if (Directory.Exists(путь))
            try {
                return Exceptional.From<FileSystemInfo>(new DirectoryInfo(путь));
            }
            catch (Exception ex) {
                return Exceptional.From<FileSystemInfo>(ex.ДобавитьПуть(путь));
            }

        return Exceptional.From<FileSystemInfo>(НовоеDirectoryNotFoundException(путь));
    }

    #endregion

    #region Ошибки

    private const string ExceptionDataPathFolderKey = "Path";

    private static Т ДобавитьПуть<Т>(this Т exc, string путь) where Т : Exception {
        if (!exc.Data.Contains(ExceptionDataPathFolderKey))
            exc.Data.Add(ExceptionDataPathFolderKey, $"{путь}{Path.DirectorySeparatorChar}");
        return exc;
    }

    private static DirectoryNotFoundException НовоеDirectoryNotFoundException(string путь, Exception? внутр = null) {
        return (внутр is null
                ? new DirectoryNotFoundException("Каталог для наблюдения недоступен")
                : new DirectoryNotFoundException("Каталог для наблюдения недоступен", внутр))
            .ДобавитьПуть(путь);
    }

    private static Exception НормализуйОшибкиFileSystemWatcher(string путь, Exception exc) {
        return exc switch {
            // Каталог не существует
            ArgumentException arg => НовоеDirectoryNotFoundException(путь, arg),
            DirectoryNotFoundException dnf => dnf.ДобавитьПуть(путь),
            FileNotFoundException fnf => НовоеDirectoryNotFoundException(путь, fnf),
            // Произошло слишком много файловых событий и внутренний буфер FileSystemWatcher не может с ними справится.
            InternalBufferOverflowException ovf => ovf,
            // Специфические ошибки Windows
            // Доступ к каталогу запрещён, возможно он был удален
            Win32Exception fnfWin when WinErrors.Contains(fnfWin.NativeErrorCode) => НовоеDirectoryNotFoundException(
                путь, fnfWin),
            // Неизвестное исключение
            var e => e.ДобавитьПуть(путь)
        };
    }


    #region Коды избранных файловых ошибок Windows

    private const int ErrorFileNotFound = 0x2; //The system cannot find the file specified
    private const int ErrorPathNotFound = 0x3; //The system cannot find the path specified
    private const int ErrorTooManyOpenFiles = 0x4; //The system cannot open the file
    private const int ErrorAccessDenied = 0x5; //Access is denied
    private const int ErrorEaFileCorrupt = 0x114; //The extended attribute file on the mounted file system is corrupt
    private const int ErrorNotAllowedOnSystemFile = 0x139; //Operation is not allowed on a file system internal file

    private static readonly int[] WinErrors = [
        ErrorFileNotFound, ErrorPathNotFound, ErrorTooManyOpenFiles,
        ErrorAccessDenied, ErrorEaFileCorrupt, ErrorNotAllowedOnSystemFile
    ];

    #endregion

    #endregion
}