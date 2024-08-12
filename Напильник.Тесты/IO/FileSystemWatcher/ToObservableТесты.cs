using AutoFixture;
using AutoFixture.Dsl;
using AutoFixture.Xunit2;
using Microsoft.Reactive.Testing;
using Moq;
using Xunit.Abstractions;

namespace System.IO.Тесты;

public class ToObservableТесты {
    public ToObservableТесты(ITestOutputHelper output) {
        _output = output;
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
        var измененФайлы = FileSystemEventChange.CreateMany();
        var удаленФайлы = FileSystemEventDelete.CreateMany();
        var переименованФайлы = FileSystemEventRename.CreateMany();
        var события = new[] { новыеФайлы, измененФайлы, удаленФайлы, переименованФайлы }.SelectMany(x => x);

        var testObserver = _планировщикТестовый.CreateObserver<FileSystemEventArgs>();


        // Действие
        _планировщикТестовый.AdvanceBy(100);
        var наблюдаемый = Fsw.ToObservable();

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
            .Select(m => m.Value.Exception)
            .FirstOrDefault()
            .Should().Be(exc);
    }

    #region Инфраструктурные поля

    private readonly Fixture _оснастка = new();
    private readonly Mock<ИFileSystemWatcherОбёртка> _fswФиктивный = new();
    private readonly TestScheduler _планировщикТестовый = new();
    private ИFileSystemWatcherОбёртка Fsw { get; }
    private readonly ITestOutputHelper _output;

    #endregion
}