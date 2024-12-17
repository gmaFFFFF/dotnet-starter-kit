using AutoFixture;

namespace System.IO.Тесты;

public class ЗаглушкаФайловойСистемы : IDisposable {
    private readonly EnumerationOptions _настройкаОглавления = new() {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        ReturnSpecialDirectories = false,
        MatchType = MatchType.Win32
    };

    private readonly Fixture _оснастка = new();
    public readonly DirectoryInfo КорневаяПапка = Directory.CreateTempSubdirectory("gmafffff.tests_");

    public IEnumerable<FileSystemInfo> Оглавление => КорневаяПапка.EnumerateFileSystemInfos("*", _настройкаОглавления);
    public IEnumerable<DirectoryInfo> ОглавлениеПапок => КорневаяПапка.EnumerateDirectories("*", _настройкаОглавления);
    public IEnumerable<FileInfo> ОглавлениеФайлов => КорневаяПапка.EnumerateFiles("*", _настройкаОглавления);

    public void Dispose() {
        if (КорневаяПапка.Exists)
            КорневаяПапка.Delete(true);
    }

    public async Task ИнициализироватьАсинх() {
        List<Task> init = [
            ДобавьФейковыеФайлыАсинх(),
            ДобавьФейковыеФайлыАсинх("lev2.1", 2),
            ДобавьНесколькоФейковыхПапокДаФайловАсинх(),
            ДобавьНесколькоФейковыхПапокДаФайловАсинх("lev2.1")
        ];
        await Task.WhenAll(init);
    }

    public async Task<FileInfo> ИзмениСлучайныйФайлАсинх() {
        var списокФайлов = ОглавлениеФайлов.ToArray();
        if (списокФайлов.Length == 0)
            throw new NotSupportedException("В папках нет ни одного файла");
        new Random().Shuffle(списокФайлов);
        var файл = списокФайлов[0];

        await ГарантированоЗаписатьНаДискФейковыеДанныеАсинх(файл);

        return файл;
    }

    public FileInfo УдалиСлучайныйФайл() {
        var списокФайлов = ОглавлениеФайлов.ToArray();
        if (списокФайлов.Length == 0)
            throw new NotSupportedException("В папках нет ни одного файла");
        new Random().Shuffle(списокФайлов);
        var файл = списокФайлов[0];

        файл.Delete();

        // Ждём фактического удаления
        файл.Refresh();
        while (файл.Exists) {
            Thread.Sleep(50);
            файл.Refresh();
        }

        return файл;
    }

    public IEnumerable<FileSystemInfo> УдалиСлучайнуюПапку() {
        var списокПапок = ОглавлениеПапок.ToArray();
        if (списокПапок.Length == 0)
            throw new NotSupportedException("В системе нет ни одной папки");
        new Random().Shuffle(списокПапок);
        var папка = списокПапок[0];
        var списокУдаленных = папка.EnumerateFileSystemInfos("", _настройкаОглавления).Prepend(папка);

        папка.Delete(true);

        // Ждём фактического удаления
        папка.Refresh();
        while (папка.Exists) {
            Thread.Sleep(50);
            папка.Refresh();
        }

        return списокУдаленных;
    }

    public IEnumerable<string> ПереместиСлучайнуюПапкуРядом() {
        var списокПапок = ОглавлениеПапок.ToArray();
        if (списокПапок.Length == 0)
            throw new NotSupportedException("В системе нет ни одной папки");
        new Random().Shuffle(списокПапок);
        var папка = списокПапок[0];
        var списокПеремещенных = папка.EnumerateFileSystemInfos("", _настройкаОглавления)
            .Prepend(папка).Select(x => x.FullName).ToList();

        using var фс2 = new ЗаглушкаФайловойСистемы();

        var новыйПуть = Path.Combine(фс2.КорневаяПапка.FullName, папка.Name);
        Directory.Move(папка.FullName, новыйПуть);
        папка.Refresh();
        while (папка.Exists) {
            Thread.Sleep(50);
            папка.Refresh();
        }

        return списокПеремещенных;
    }

    public async Task<IEnumerable<string>> ЗабериИзСлучайнойПапкиРядомАсинх() {
        using var фс2 = new ЗаглушкаФайловойСистемы();
        await фс2.ИнициализироватьАсинх();

        var списокПапок = фс2.ОглавлениеПапок.ToArray();
        if (списокПапок.Length == 0)
            throw new NotSupportedException("В системе нет ни одной папки");
        new Random().Shuffle(списокПапок);
        var папка = списокПапок[0];

        папка.MoveTo(Path.Combine(КорневаяПапка.FullName, папка.Name));

        var списокПеремещенных = папка.EnumerateFileSystemInfos("", _настройкаОглавления)
            .Select(x => x.FullName).Prepend(папка.FullName[..^1]);

        return списокПеремещенных;
    }

    public async Task<IEnumerable<FileSystemInfo>> ДобавьНесколькоФейковыхПапокДаФайловАсинх(
        string относительныйПуть = "",
        uint количество = 3) {
        List<Task<IEnumerable<FileSystemInfo>>> папки = [];
        for (var i = 0; i < количество; i++)
            папки.Add(ДобавьФейковуюПапкуДаФайлыАсинх(относительныйПуть));

        await Task.WhenAll(папки);

        var rez = папки.SelectMany(x => x.Result);
        return rez;
    }

    public async Task<IEnumerable<FileSystemInfo>> ДобавьФейковуюПапкуДаФайлыАсинх(string относительныйПуть = "",
        uint количество = 10) {
        var папка = ДобавьФейковуюПапку(относительныйПуть);
        var файлы = await ДобавьФейковыеФайлыАсинх(Path.GetRelativePath(КорневаяПапка.FullName, папка.FullName),
            количество);
        return файлы.Prepend(папка);
    }

    public DirectoryInfo ДобавьФейковуюПапку(string относительныйПуть = "") {
        var путь = Path.Combine(относительныйПуть, Path.GetRandomFileName().Replace(".", null));

        var новаяПапка = КорневаяПапка.CreateSubdirectory(путь);

        // Ждём фактического создания
        новаяПапка.Refresh();
        while (!новаяПапка.Exists) {
            Thread.Sleep(50);
            новаяПапка.Refresh();
        }

        return новаяПапка;
    }

    private async Task<FileInfo> ДобавьФейковыйФайлАсинх(DirectoryInfo папка) {
        var путь = Path.Combine(папка.FullName, Path.GetRandomFileName());
        var файл = new FileInfo(путь);

        await ГарантированоЗаписатьНаДискФейковыеДанныеАсинх(файл);

        return файл;
    }

    public async Task<IEnumerable<FileSystemInfo>> ДобавьФейковыеФайлыАсинх(string относительныйПуть = "",
        uint количество = 10) {
        List<Task<FileInfo>> файлы = [];
        for (var i = 0; i < количество; i++)
            файлы.Add(ДобавьФейковыйФайлАсинх(относительныйПуть));

        await Task.WhenAll(файлы);

        return файлы.Select(x => x.Result);
    }

    public async Task<FileInfo> ДобавьФейковыйФайлАсинх(string относительныйПуть = "") {
        var папкаПуть = Path.Combine(КорневаяПапка.FullName, относительныйПуть);
        var папка = new DirectoryInfo(папкаПуть);

        папка.Create();

        return await ДобавьФейковыйФайлАсинх(папка);
    }

    private async Task ГарантированоЗаписатьНаДискФейковыеДанныеАсинх(FileInfo файл) {
        using var writer = файл.Exists ? файл.AppendText() : файл.CreateText();

        // Экспериментально установлено, что 10-кратная запись по 30 символов, 
        // а затем сброс буфера помогают дождаться фактической записи в файл
        foreach (var _ in Enumerable.Range(0, 10))
            await writer.WriteLineAsync(_оснастка.CreateMany<char>(30).ToArray());
        await writer.FlushAsync();
    }
}