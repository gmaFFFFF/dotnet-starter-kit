namespace System.Reactive.Concurrency;

/// <summary>
///     Стандартный поставщик планировщиков, который можно использовать в реальном коде
/// </summary>
public sealed class ПоставляетПланировщики : ИПоставляетПланировщики {
    public IScheduler CurrentThread => Scheduler.CurrentThread;
    public IScheduler? Dispatcher => SynchronizationContext.Current as IScheduler;
    public IScheduler Immediate => Scheduler.Immediate;
    public IScheduler NewThread => NewThreadScheduler.Default;
    public IScheduler Default => Scheduler.Default;
    public IScheduler TaskPool => TaskPoolScheduler.Default;
}