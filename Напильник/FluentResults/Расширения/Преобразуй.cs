namespace FluentResults;

public static partial class РезультатРасширения {
    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> успешен, то возвращает результат функции
    ///     "<paramref name="преобразователь" />", вызванной с аргументом значения текущего <see cref="Result{TValue}" />
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="преобразователь">функция, принимающая значение <see cref="Result{TValue}" /> как аргумент</param>
    /// <typeparam name="Т">тип, принимаемый функцией "<paramref name="преобразователь" />"</typeparam>
    /// <typeparam name="ТВых">тип, возвращаемый функцией "<paramref name="преобразователь" />"</typeparam>
    /// <returns>Результат <see cref="Result{TValue}" /> выполнения функции "<paramref name="преобразователь" />"</returns>
    public static Result<ТВых> Преобразуй<Т, ТВых>(this Result<Т> результат, Func<Т, Result<ТВых>> преобразователь) {
        if (результат.IsSuccess) преобразователь.MustNotBeNull();

        return (результат.IsFailed ? результат.ToResult() : преобразователь(результат.Value))
            .WithReasons(результат.Reasons);
    }

    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> успешен, то возвращает результат функции "
    ///     <paramref name="преобразователь" />", вызванной с аргументом Value текущего <see cref="Result{TValue}" />
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="преобразователь">функция, принимающая значение <see cref="Result{TValue}" /> как аргумент</param>
    /// <typeparam name="Т">тип, принимаемый функцией "<paramref name="преобразователь" />"</typeparam>
    /// <typeparam name="ТВых">тип, возвращаемый функцией "<paramref name="преобразователь" />"</typeparam>
    /// <returns>Результат <see cref="Result{TValue}" /> выполнения функции "<paramref name="преобразователь" />"</returns>
    /// <remarks>
    ///     Если функция "<paramref name="преобразователь" />" выбросит исключение, то будет возвращен "провальный"
    ///     результат
    /// </remarks>
    public static Result<ТВых> Преобразуй<Т, ТВых>(this Result<Т> результат, Func<Т, ТВых> преобразователь) {
        if (результат.IsSuccess) преобразователь.MustNotBeNull();

        return результат.ToResult(преобразователь);
    }
}