using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Xunit2;
using FluentAssertions.Execution;
using Microsoft.Reactive.Testing;
using Moq;
using Xunit.Abstractions;

namespace System.IO.Тесты;

public class FileSystemWatcherToObservableТесты {
    public class ЗаглушкаДляСобытий {
        public ЗаглушкаДляСобытий(ITestOutputHelper output) {
            Fsw = _fswФиктивный.Object;
        }

        private IPostprocessComposer<FileSystemEventArgs> FileSystemEventNew
            => _оснастка.Build<FileSystemEventArgs>()
                .FromFactory<string>(название => new FileSystemEventArgs(WatcherChangeTypes.Created, ".", название));

        private IPostprocessComposer<FileSystemEventArgs> FileSystemEventChange
            => _оснастка.Build<FileSystemEventArgs>()
                .FromFactory<string>(название => new FileSystemEventArgs(WatcherChangeTypes.Changed, ".", название));

        private IPostprocessComposer<FileSystemEventArgs> FileSystemEventDelete
            => _оснастка.Build<FileSystemEventArgs>()
                .FromFactory<string>(название => new FileSystemEventArgs(WatcherChangeTypes.Deleted, ".", название));

        private IPostprocessComposer<RenamedEventArgs> FileSystemEventRename
            => _оснастка.Build<RenamedEventArgs>()
                .FromFactory<string, string>((названиеНовое, названиеСтарое)
                    => new RenamedEventArgs(WatcherChangeTypes.Renamed, ".", названиеНовое, названиеСтарое));

        private IPostprocessComposer<ErrorEventArgs> FileSystemEventError
            => _оснастка.Build<ErrorEventArgs>()
                .FromFactory<InternalBufferOverflowException>(exc => new ErrorEventArgs(exc));


        [Fact]
        public void ПолучаетСобытияФайлов() {
            // Подготовка
            var новыеФайлы = FileSystemEventNew.CreateMany();
            var игнорСобытия = FileSystemEventNew.CreateMany();
            var измененФайлы = FileSystemEventChange.CreateMany();
            var удаленФайлы = FileSystemEventDelete.CreateMany();
            var переименованФайлы = FileSystemEventRename.CreateMany();
            var события = new[] { новыеФайлы, измененФайлы, удаленФайлы, переименованФайлы }.SelectMany(x => x);

            var testObserver = _планировщикТестовый.CreateObserver<FileSystemEventArgs>();

            // Действие
            _планировщикТестовый.AdvanceBy(100);
            var наблюдаемый = Fsw.ToObservable();
            foreach (var игнор in игнорСобытия)
                _fswФиктивный.Raise(f => f.Created += null, игнор);

            _планировщикТестовый.AdvanceBy(100);
            var подписка = наблюдаемый.Subscribe(testObserver);


            foreach (var item in измененФайлы)
                _fswФиктивный.Raise(f => f.Created += null, item);

            foreach (var item in новыеФайлы)
                _fswФиктивный.Raise(f => f.Changed += null, item);

            foreach (var item in удаленФайлы)
                _fswФиктивный.Raise(f => f.Deleted += null, item);

            foreach (var item in переименованФайлы)
                _fswФиктивный.Raise(f => f.Renamed += null, item);


            _планировщикТестовый.AdvanceTo(1000L);
            подписка.Dispose();

            // Проверка
            testObserver.Messages
                .Select(m => m.Value.Value)
                .Should().BeEquivalentTo(события);
        }

        [Fact]
        public void ОтлавливаетИсключенияПриРаботе() {
            // Подготовка
            var новыеФайлы = FileSystemEventNew.CreateMany();
            var error = FileSystemEventError.Create();
            var testObserver = _планировщикТестовый.CreateObserver<FileSystemEventArgs>();

            // Действие
            _планировщикТестовый.AdvanceBy(100);
            var наблюдаемый = Fsw.ToObservable();

            _планировщикТестовый.AdvanceBy(100);
            var подписка = наблюдаемый.Subscribe(testObserver);

            foreach (var item in новыеФайлы)
                _fswФиктивный.Raise(f => f.Created += null, item);

            _fswФиктивный.Raise(f => f.Error += null, error);

            _планировщикТестовый.AdvanceTo(1000L);
            подписка.Dispose();

            // Проверка
            testObserver.Messages
                .Where(m => m.Value.HasValue)
                .Select(m => m.Value.Value)
                .Should().BeEquivalentTo(новыеФайлы);

            testObserver.Messages
                .Where(m => !m.Value.HasValue)
                .Select(m => m.Value.Exception)
                .FirstOrDefault()
                .Should().Be(error.GetException());
        }

        [Theory]
        [AutoData]
        public void ПриСтартеНаблюденияПробрасываетИсключение(ArgumentException exc) {
            // Подготовка
            _fswФиктивный.SetupSet(fsw => fsw.EnableRaisingEvents = true).Throws(exc);
            var testObserver = _планировщикТестовый.CreateObserver<FileSystemEventArgs>();

            // Действие
            _планировщикТестовый.AdvanceBy(100);
            var наблюдаемый = Fsw.ToObservable();

            _планировщикТестовый.AdvanceBy(100);
            var подписка = наблюдаемый.Subscribe(testObserver);

            _планировщикТестовый.AdvanceTo(1000L);
            подписка.Dispose();

            // Проверка
            testObserver.Messages
                .Select(m => m.Value.Exception!.InnerException)
                .FirstOrDefault()
                .Should().Be(exc);
        }

        #region Инфраструктурные поля

        private readonly Fixture _оснастка = new();
        private readonly Mock<ИFileSystemWatcherОбёртка> _fswФиктивный = new();
        private readonly TestScheduler _планировщикТестовый = new();
        private ИFileSystemWatcherОбёртка Fsw { get; }

        #endregion
    }

    public class РеальнаяФайловаяСистема : IDisposable {
        private readonly TimeSpan _задержка = TimeSpan.FromMilliseconds(100);
        private readonly TestScheduler _планировщикТестовый = new();
        private readonly ЗаглушкаФайловойСистемы _fs = new();

        private readonly ИFileSystemWatcherОбёртка _fsw = new FileSystemWatcherОбёртка();
        private readonly ITestableObserver<FileSystemEventArgs> _obs;

        public РеальнаяФайловаяСистема() {
            _fsw.Path = _fs.КорневаяПапка.FullName;
            _fsw.IncludeSubdirectories = true;
            _obs = _планировщикТестовый.CreateObserver<FileSystemEventArgs>();
        }

        public void Dispose() {
            _fs.Dispose();
        }

        [Fact]
        public async Task ПолучаетСобытияСозданияФайла() {
            // Подготовка
            var папка = _fs.ДобавьФейковуюПапку();
            var наблюдаемый = _fsw.ToObservable();

            // Действие
            using var подписка = наблюдаемый.Subscribe(_obs);
            var file = await _fs.ДобавьФейковыйФайлАсинх(папка.Name);
            await Task.Delay(_задержка);

            // Проверка
            using var _ = new AssertionScope();
            _obs.Messages.ToList()
                .Select(m => m.Value.Value)
                .Should().OnlyContain(arg => arg.FullPath == file.FullName,
                    "манипуляции происходят только с одним файлом");

            _obs.Messages
                .Select(m => m.Value.Value)
                .Should().OnlyContain(
                    arg => arg.ChangeType == WatcherChangeTypes.Created || arg.ChangeType == WatcherChangeTypes.Changed,
                    "ожидаются только события создания и изменения");

            _obs.Messages
                .Select(m => m.Value.Value)
                .Should().ContainSingle(arg => arg.ChangeType == WatcherChangeTypes.Created,
                    "должно произойти одно событие создания");

            _obs.Messages
                .Select(m => m.Value.Value)
                .Where(arg => arg.ChangeType == WatcherChangeTypes.Changed)
                .Should().HaveCountGreaterThanOrEqualTo(1, "запись осуществлялась в несколько действий");
        }

        [Fact]
        public async Task ПолучаетСобытиеУдаленияФайла() {
            // Подготовка
            await _fs.ДобавьФейковыйФайлАсинх();
            var наблюдаемый = _fsw.ToObservable();

            // Действие
            using var подписка = наблюдаемый.Subscribe(_obs);
            var file = _fs.УдалиСлучайныйФайл();
            await Task.Delay(_задержка);

            // Проверка
            _obs.Messages
                .Select(m => m.Value.Value)
                .Should().ContainSingle(
                    arg => arg.ChangeType == WatcherChangeTypes.Deleted && arg.FullPath == file.FullName,
                    "ожидается ровно 1 событие удаления");
        }

        [Fact]
        public async Task ПолучаетСобытияИзмененияФайла() {
            // Подготовка
            var папка = _fs.ДобавьФейковуюПапку();
            await _fs.ДобавьФейковыйФайлАсинх(папка.Name);
            var наблюдаемый = _fsw.ToObservable();

            // Действие
            using var подписка = наблюдаемый.Subscribe(_obs);
            var file = await _fs.ИзмениСлучайныйФайлАсинх();
            await Task.Delay(_задержка);

            // Проверка
            _obs.Messages
                .Select(m => m.Value.Value)
                .Should().Contain(arg => arg.ChangeType == WatcherChangeTypes.Changed && arg.FullPath == file.FullName,
                    "ожидаются только события изменения одного файла");
        }
    }
}