namespace FluentResults;

public static partial class РезультатРасширения {
    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> ошибочен,
    ///     то возвращает результат выполнения функции "<paramref name="функц" />",
    ///     иначе значение <see cref="Result{TValue}" />
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="функц">
    ///     функция возвращающая значение,
    ///     которое может использоваться вместо ошибочного <see cref="Result{TValue}" />
    /// </param>
    /// <typeparam name="Т">тип, хранимый в <see cref="Result{TValue}" /></typeparam>
    /// <returns></returns>
    public static Т Или<Т>(this Result<Т> результат, Func<Т> функц) {
        return результат.Если(_ => результат.Value, функц);
    }

    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> ошибочен,
    ///     то выполняется "<paramref name="действие" />"
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="действие">процедура, принимающая значение <see cref="Result{TValue}" /> как аргумент</param>
    /// <typeparam name="Т">тип, принимаемый процедурой "<paramref name="действие" />"</typeparam>
    public static void Или<Т>(this Result<Т> результат, Action действие) {
        if (результат.IsFailed)
            действие();
    }
}