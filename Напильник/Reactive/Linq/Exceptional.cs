using System.Diagnostics;

namespace System.Reactive.Linq;

/// <summary>
///     Монада для обработки исключений
/// </summary>
/// <example>
///     <code>
/// <![CDATA[
/// IObservable<Exceptional<int>> query =
///             from n in Observable.Range(0, 10)
///             from x in n.ToExceptional()
///             let a = 5 - x
///             let b = 100 / a
///             select b + 5;
/// ]]>
/// </code>
///     <code>
/// <![CDATA[
/// IObservable<Exceptional<int>> query =
///             Observable
///             .Range(0, 10)
///             .Select(x => x.ToExceptional())
///             .Select(x => 5 - x)
///             .Select(x => 100 / x)
///             .Select(x => x + 5);
/// ]]>
/// </code>
/// </example>
/// <remarks>
///     Код заимствован <see href="https://stackoverflow.com/a/67528904/6803090" />
/// </remarks>
public class Exceptional {
    public static Exceptional<Т> From<Т>(Т value) {
        return new Exceptional<Т>(value);
    }

    public static Exceptional<Т> From<Т>(Exception ex) {
        return new Exceptional<Т>(ex);
    }

    public static Exceptional<Т> From<Т>(Func<Т> factory) {
        return new Exceptional<Т>(factory);
    }

    public static IEnumerable<Exceptional<Т>> From<Т>(IEnumerable<Т> перечисление) {
        using var перечислитель = перечисление.GetEnumerator();
        var дальшеЛи = true;
        Exception? exc = null;

        while (дальшеЛи) {
            try {
                дальшеЛи = перечислитель.MoveNext();
                if (!дальшеЛи)
                    yield break;
            }
            catch (Exception e) {
                exc = e;
                break;
            }

            yield return перечислитель.Current.ToExceptional();
        }

        yield return From<Т>(exc);
    }
}

public class Exceptional<Т> {
    public Exceptional(Т value) {
        HasException = false;
        Value = value;
    }

    public Exceptional(Exception exception) {
        HasException = true;
        Exception = exception;
    }

    public Exceptional(Func<Т> factory) {
        try {
            Value = factory();
            HasException = false;
        }
        catch (Exception ex) {
            Exception = ex;
            HasException = true;
        }
    }

    public bool HasException { get; }
    public Exception? Exception { get; }
    public Т? Value { get; }

    public override string ToString() {
        Debug.Assert(!(HasException && Exception == null),
            nameof(HasException) + "== true, но " +
            nameof(Exception) + " == null");
        return HasException
            ? Exception!.GetType().Name
            : Value?.ToString() ?? "null";
    }
}

public static class ExceptionalExtensions {
    public static Exceptional<Т> ToExceptional<Т>(this Т value) {
        return Exceptional.From(value);
    }

    public static IEnumerable<Exceptional<Т>> ToExceptional<Т>(this IEnumerable<Т> value) {
        return Exceptional.From(value);
    }

    public static Exceptional<Т> ToExceptional<Т>(this Func<Т> factory) {
        return Exceptional.From(factory);
    }

    public static Exceptional<Т2> Select<Т, Т2>(this Exceptional<Т> value, Func<Т, Т2> m) {
        return value.SelectMany(t => Exceptional.From(() => m(t)));
    }

    public static Exceptional<Т2> SelectMany<Т, Т2>(this Exceptional<Т> value, Func<Т, Exceptional<Т2>> k) {
        return value.HasException ? Exceptional.From<Т2>(value.Exception!) : k(value.Value!);
    }

    public static Exceptional<Т3> SelectMany<Т, Т2, Т3>(this Exceptional<Т> value, Func<Т, Exceptional<Т2>> k,
        Func<Т, Т2, Т3> m) {
        return value.SelectMany(t => k(t).SelectMany(u => Exceptional.From(() => m(t, u))));
    }

    public static IObservable<Exceptional<Т2>> Select<Т, Т2>(this IObservable<Exceptional<Т>> source, Func<Т, Т2> m) {
        return source.Select(x => x.SelectMany(y => Exceptional.From(() => m(y))));
    }

    public static IObservable<Exceptional<Т3>> SelectMany<Т, Т2, Т3>(this IObservable<Т> source,
        Func<Т, Exceptional<Т2>> k,
        Func<Т, Т2, Т3> m) {
        return source.SelectMany(t => k(t).SelectMany(u => Exceptional.From(() => m(t, u))));
    }

    public static IObservable<Exceptional<Т2>>
        SelectMany<Т, Т2>(this IObservable<Т> source, Func<Т, Exceptional<Т2>> k) {
        return source.Select(k);
    }

    public static Т2 Match<Т, Т2>(this Exceptional<Т> value, Func<Т, Т2> ifSucc, Func<Exception, Т2> ifFail) {
        ArgumentNullException.ThrowIfNull(ifSucc, nameof(ifSucc));
        ArgumentNullException.ThrowIfNull(ifFail, nameof(ifFail));
        return value.HasException ? ifFail(value.Exception!) : ifSucc(value.Value!);
    }
}