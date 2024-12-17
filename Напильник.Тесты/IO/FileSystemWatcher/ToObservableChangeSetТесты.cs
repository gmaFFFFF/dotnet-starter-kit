using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using FluentAssertions.Execution;

namespace System.IO.Тесты;

public class ToObservableChangeSetТесты : IDisposable {
    private readonly TimeSpan _задержка = TimeSpan.FromMilliseconds(100);
    private readonly ЗаглушкаФайловойСистемы _fs = new();
    private readonly ИFileSystemWatcherОбёртка _fsw = new FileSystemWatcherОбёртка();

    public ToObservableChangeSetТесты() {
        _fsw.Path = _fs.КорневаяПапка.FullName;
        _fsw.IncludeSubdirectories = true;
    }

    public void Dispose() {
        _fs.Dispose();
    }

    [Fact]
    public async Task ПравильноПеречисляетОглавлениеРекурсивноАсинх() {
        // Подготовка
        await _fs.ИнициализироватьАсинх();

        // Действие
        var результат = await FileSystemWatcherРасширения
            .ДайОглавлениеПапки(_fs.КорневаяПапка, рекурсивноЛи: true)
            .Materialize()
            .ToList();

        // Проверка
        var ожидается = _fs.Оглавление
            .Select(f => f.FullName)
            .ToList();
        using var _ = new AssertionScope();

        результат
            .Should().NotContain(m => m.Kind == NotificationKind.OnError);

        результат
            .Where(m => m.Kind == NotificationKind.OnNext)
            .Select(m => m.Value.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(ожидается);
    }

    [Fact]
    public async Task ПравильноПеречисляетОглавлениеКорневойПапкиАсинх() {
        // Подготовка
        await _fs.ИнициализироватьАсинх();

        // Действие
        var результат = await FileSystemWatcherРасширения
            .ДайОглавлениеПапки(_fs.КорневаяПапка, рекурсивноЛи: false)
            .Materialize()
            .ToList();

        // Проверка
        var ожидается = _fs.Оглавление
            .Where(f => Path.GetDirectoryName(f.FullName) == _fs.КорневаяПапка.FullName)
            .Select(f => f.FullName)
            .ToList();
        using var _ = new AssertionScope();

        результат
            .Should().NotContain(m => m.Kind == NotificationKind.OnError);

        результат
            .Where(m => m.Kind == NotificationKind.OnNext)
            .Select(m => m.Value.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(ожидается);
    }

    [Fact]
    public async Task ОтслеживаетДобавлениеФайлаАсинх() {
        // Подготовка
        FileSystemInfo файл = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _1 = монитор.OnItemAdded(ф => файл = ф.Value!).Subscribe();
        монитор.Connect();

        // Действие
        var ожидается = await _fs.ДобавьФейковыйФайлАсинх();

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        файл.FullName.Should().Be(ожидается.FullName);
    }

    [Fact]
    public async Task ОтслеживаетДобавлениеПапкиАсинх() {
        // Подготовка
        FileSystemInfo папка = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _1 = монитор.OnItemAdded(ф => папка = ф.Value!).Subscribe();
        монитор.Connect();
        await Task.Delay(_задержка);

        // Действие
        var ожидается = _fs.ДобавьФейковуюПапку();
        await Task.Delay(_задержка);

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        папка.FullName.Should().Be(ожидается.FullName);
    }

    [Fact]
    public async Task ОтслеживаетУдалениеФайлаАсинх() {
        // Подготовка
        await _fs.ДобавьФейковыйФайлАсинх();
        FileSystemInfo файл = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _2 = монитор.OnItemRemoved(ф => файл = ф.Value!).Subscribe();
        монитор.Connect();

        // Действие
        var ожидается = _fs.УдалиСлучайныйФайл();
        await Task.Delay(_задержка);

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        файл.FullName.Should().Be(ожидается.FullName);
    }

    [Fact]
    public async Task ОтслеживаетУдалениеПапкиАсинх() {
        // Подготовка
        _fs.ДобавьФейковуюПапку();
        FileSystemInfo папка = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();

        using var _1 = монитор.OnItemRemoved(ф => папка = ф.Value!).Subscribe();
        using var список = монитор.AsObservableCache();

        using var _3 = монитор.Connect();


        // Действие
        await Task.Delay(_задержка); // Ждём инициализации
        var ожидается = _fs.УдалиСлучайнуюПапку().Single();
        await Task.Delay(_задержка); // Ждем прохождения события 

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        папка.FullName.Should().Be(ожидается.FullName);
    }

    [Fact]
    public async Task ОтслеживаетПеремещениеПапкиНаТотЖеДискАсинх() {
        // Подготовка
        await _fs.ДобавьНесколькоФейковыхПапокДаФайловАсинх();
        List<FileSystemInfo> перемещённые = [];
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _2 = монитор.OnItemRemoved(ф => перемещённые.Add(ф.Value!)).Subscribe();
        using var _3 = монитор.Connect();

        // Действие
        await Task.Delay(_задержка); // Ждём инициализации
        var ожидается = _fs.ПереместиСлучайнуюПапкуРядом();
        await Task.Delay(_задержка); // Ждем прохождения события 

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        перемещённые.Select(x => x.FullName).Should().BeEquivalentTo(ожидается);
    }

    [Fact]
    public async Task ОтслеживаетПеремещениеПапкиИзТогоЖеДискаАсинх() {
        // Подготовка

        List<FileSystemInfo> перемещённые = [];
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _2 = монитор.OnItemAdded(ф => перемещённые.Add(ф.Value!))
            .Subscribe();
        монитор.Connect();

        // Действие
        var ожидается = await _fs.ЗабериИзСлучайнойПапкиРядомАсинх();
        await Task.Delay(_задержка);

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        перемещённые.Select(x => x.FullName).Should().BeEquivalentTo(ожидается);
    }


    //TODO: Тест, подтверждающий, что при перемещении файлов с другого диска 
    //      не происходит двух событий add к вложенным в папку файлам. 
    //      Проблема в том, что не на каждом компе есть два диска. 
    //      А если и есть, то кто разрешил писать в произвольное место на 2?

    [Fact]
    public async Task ОтслеживаетИзменениеФайлаАсинх() {
        // Подготовка
        await _fs.ДобавьФейковыйФайлАсинх();
        FileSystemInfo файл = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _2 = монитор.OnItemUpdated((до, после) => файл = после.Value!)
            .Subscribe();
        монитор.Connect();
        await Task.Delay(_задержка);

        // Действие
        var ожидается = await _fs.ИзмениСлучайныйФайлАсинх();
        await Task.Delay(_задержка);

        // Проверка
        using var _ = new AssertionScope();
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));

        файл.FullName.Should().Be(ожидается.FullName);
    }

    [Fact]
    public async Task КорректноОбрабатываетСбойНаблюдателяАсинх() {
        // Подготовка
        Exception ошибка = null!;
        var монитор = _fsw.ToObservableChangeSet().Publish();
        var список = монитор.AsObservableCache();
        using var _1 = монитор.OnItemAdded(ф => ошибка = ф.Exception!).Subscribe();
        монитор.Connect();
        await Task.Delay(_задержка);

        // Действие
        _fs.Dispose();
        await Task.Delay(_задержка);

        // Проверка
        using var _ = new AssertionScope();
        список.Items.Should().ContainSingle(efsi => efsi.HasException);

        ошибка.Should().NotBeNull();
    }


    [Fact]
    public async Task ОтслеживаетМножествоФайловыхСобытийАсинх() {
        // Подготовка        
        await _fs.ДобавьНесколькоФейковыхПапокДаФайловАсинх();

        var список = _fsw.ToObservableChangeSet().AsObservableCache();
        await Task.Delay(_задержка);

        Func<Task>[] действия = [
            () => _fs.ДобавьНесколькоФейковыхПапокДаФайловАсинх(), () => Task.Run(() => _fs.ДобавьФейковуюПапку()),
            () => _fs.ДобавьФейковуюПапкуДаФайлыАсинх(), () => _fs.ДобавьФейковыеФайлыАсинх(),
            () => _fs.ДобавьФейковыйФайлАсинх(), () => _fs.ЗабериИзСлучайнойПапкиРядомАсинх(),
            () => _fs.ИзмениСлучайныйФайлАсинх()
        ];


        // Действие        
        new Random().Shuffle(действия);
        действия.AsParallel().ForAll(async el => await el());

        new Random().Shuffle(действия);
        действия.AsParallel().ForAll(async el => await el());

        // Проверка
        список.Items
            .Select(efsi => efsi.Value)
            .Select(f => f!.FullName)
            .Should().BeEquivalentTo(_fs.Оглавление.Select(f => f.FullName));
    }

    [Fact]
    public async Task МожноУправлятьВнутреннимСостоянием() {
        // Подготовка        
        await _fs.ИнициализироватьАсинх();

        var список = _fsw.ToObservableChangeSet(out var снимокБудущий).AsObservableCache();
        await Task.Delay(_задержка);

        var снимок = await снимокБудущий;

        // Предварительная проверка
        using var _ = new AssertionScope();
        список.Items
            .Should().Equal(снимок.Items);

        // Действие
        снимок.Clear();

        // Проверка
        список.Items.Should().BeEmpty();
    }
}