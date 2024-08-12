namespace System.IO;

/// <summary>
///     Реализация <see cref="ИFileSystemWatcherОбёртка" /> с помощью <see cref="FileSystemWatcher" />
/// </summary>
/// <remarks>
///     Класс, потребляющий события файловой системы, будет хранить у себя ссылку типа
///     <see cref="ИFileSystemWatcherОбёртка" />, а в конструктор ему можно пробрасывать данную реализацию,
///     или при тестировании заглушку4LVMvDP2OC9MMiHRBlKa
/// </remarks>
public class FileSystemWatcherОбёртка : FileSystemWatcher, ИFileSystemWatcherОбёртка { }

/// <summary>
///     Интерфейс обеспечивает возможность тестирования компонентов, завязанных на <see cref="FileSystemWatcher" />
/// </summary>
public interface ИFileSystemWatcherОбёртка : IDisposable {
    NotifyFilters NotifyFilter { get; set; }
    bool EnableRaisingEvents { get; set; }
    string Filter { get; set; }
    bool IncludeSubdirectories { get; set; }
    int InternalBufferSize { get; set; }
    string Path { get; set; }

    event FileSystemEventHandler? Changed;
    event FileSystemEventHandler? Created;
    event FileSystemEventHandler? Deleted;
    event ErrorEventHandler? Error;
    event RenamedEventHandler? Renamed;
}