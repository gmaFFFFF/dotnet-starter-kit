using System.Reactive.Concurrency;

namespace System.Reactive.Linq;

public static class ObservableОператоры {
    /// <summary>
    ///     Расщепляет последовательность по временным отрезкам.
    ///     В отличие от стандартного оператора <see cref="Observable.Buffer{TSource}(System.IObservable{TSource},TimeSpan)" />
    ///     не генерирует пустую последовательность
    /// </summary>
    /// <param name="source"></param>
    /// <param name="threshold"></param>
    /// <param name="планировщик"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    /// <remarks>
    ///     Код заимствован <see href="https://stackoverflow.com/a/35611155/6803090" />
    /// </remarks>
    public static IObservable<IList<TSource>> BufferWhenAvailable<TSource>(this IObservable<TSource> source,
        TimeSpan threshold, IScheduler планировщик) {
        return source.GroupByUntil(_ => true, _ => Observable.Timer(threshold, планировщик))
            .SelectMany(i => i.ToList());
    }

    /// <summary>
    ///     Аналог для <see cref="Quiescent{T}" />
    /// </summary>
    /// <param name="source"></param>
    /// <param name="порог"></param>
    /// <param name="maxAmount"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    /// <remarks>
    ///     Код заимствован
    ///     <see href="https://stackoverflow.com/questions/35557411/buffer-until-quiet-behavior-from-reactive" />
    /// </remarks>
    public static IObservable<IList<TSource>> BufferWithThrottle<TSource>(this IObservable<TSource> source,
        TimeSpan порог,
        int maxAmount = int.MaxValue) {
        return Observable.Create<IList<TSource>>(obs
            => source.GroupByUntil(_ => true,
                    g => g.Throttle(порог)
                        .Select(_ => Unit.Default)
                        .Merge(g.Take(maxAmount)
                            .LastAsync()
                            .Select(_ => Unit.Default)))
                .SelectMany(i => i.ToList())
                .Subscribe(obs));
    }

    /// <summary>
    ///     Буферизует наблюдаемую последовательность пока после последнего выпущенного элемента не пройдет
    ///     время <see cref="minimumInactivityPeriod" />
    /// </summary>
    /// <param name="src"></param>
    /// <param name="minimumInactivityPeriod">Максимальный период между двумя выпусками, которые попадут в один буфер</param>
    /// <param name="scheduler">Планировщик</param>
    /// <typeparam name="T">Тип наблюдаемых элементов</typeparam>
    /// <returns>Буферизованная последовательность</returns>
    /// <remarks>
    ///     Код заимствован <see href="https://introtorx.com/chapters/key-types">LINQ Operators and Composition in</see>
    /// </remarks>
    public static IObservable<IList<T>> Quiescent<T>(this IObservable<T> src,
        TimeSpan minimumInactivityPeriod, IScheduler scheduler) {
        var onOffs =
            from _ in src
            from delta in
                Observable.Return(1, scheduler)
                    .Concat(Observable.Return(-1, scheduler)
                        .Delay(minimumInactivityPeriod, scheduler))
            select delta;
        var outstanding = onOffs.Scan(0, (total, delta) => total + delta);
        var zeroCrossings = outstanding.Where(total => total == 0);
        return src.Buffer(zeroCrossings);
    }
}