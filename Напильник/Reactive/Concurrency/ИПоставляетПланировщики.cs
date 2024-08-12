namespace System.Reactive.Concurrency;

/// <summary>
///     Интерфейс, который используется для проброса тестового планировщика
/// </summary>
public interface ИПоставляетПланировщики {
    IScheduler CurrentThread { get; }
    IScheduler? Dispatcher { get; }
    IScheduler Immediate { get; }
    IScheduler NewThread { get; }
    IScheduler Default { get; }
    IScheduler TaskPool { get; }
}