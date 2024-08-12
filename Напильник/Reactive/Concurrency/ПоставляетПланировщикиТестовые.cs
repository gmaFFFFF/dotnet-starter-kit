using System.Reactive.Concurrency;

namespace Microsoft.Reactive.Testing;

/// <summary>
///     Поставщик тестовых планировщиков
/// </summary>
public class ПоставляетПланировщикиТестовые : ИПоставляетПланировщики {
    // Планировщики доступны как тип TestScheduler
    public TestScheduler CurrentThread { get; } = new();
    public TestScheduler Dispatcher { get; } = new();
    public TestScheduler Immediate { get; } = new();
    public TestScheduler NewThread { get; } = new();
    public TestScheduler Default { get; } = new();
    public TestScheduler TaskPool { get; } = new();

    // Планировщики, возвращаемые поставщиком, должны возвращать IScheduler, 
    // но для удобства тестирования кода нужны и свойства, явно возвращающие TestScheduler,
    // поэтому предоставим явную реализацию интерфейса
    IScheduler ИПоставляетПланировщики.CurrentThread => CurrentThread;
    IScheduler ИПоставляетПланировщики.Dispatcher => Dispatcher;
    IScheduler ИПоставляетПланировщики.Immediate => Immediate;
    IScheduler ИПоставляетПланировщики.NewThread => NewThread;
    IScheduler ИПоставляетПланировщики.Default => Default;
    IScheduler ИПоставляетПланировщики.TaskPool => TaskPool;
}