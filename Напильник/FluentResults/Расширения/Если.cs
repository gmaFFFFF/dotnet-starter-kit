namespace FluentResults;

public static partial class РезультатРасширения {
    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> ошибочен,
    ///     то возвращает значение из функции без параметров "<paramref name="функцОшибка" />",
    ///     в противном случае результат выполнения функции "<paramref name="функцОк" />"
    ///     в отношении значения
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="функцОк"> функция преобразующая значение <see cref="Result{TValue}" /></param>
    /// <param name="функцОшибка">
    ///     функция, возвращающая значение,
    ///     которое может использоваться при отсутствии значения <see cref="Result{TValue}" />
    /// </param>
    /// <typeparam name="Т">тип, хранимый в <see cref="Result{TValue}" /></typeparam>
    /// <typeparam name="ТВых">
    ///     тип возврата из функции "<paramref name="функцОшибка" />"
    ///     или "<paramref name="функцОк" />
    /// </typeparam>
    /// <returns>Преобразованное значение результата или результат "<paramref name="функцОшибка" />"</returns>
    public static ТВых Если<Т, ТВых>(this Result<Т> результат, Func<Т, ТВых> функцОк, Func<ТВых> функцОшибка) {
        return результат.IsFailed ? функцОшибка.MustNotBeNull()() : функцОк.MustNotBeNull()(результат.Value);
    }

    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> ошибочен,
    ///     то выполняется процедура "<paramref name="действиеОшибка" />",
    ///     в противном процедура "<paramref name="действиеОк" />" с параметром
    ///     значения текущего <see cref="Result{TValue}" />
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="действиеОк"> процедура, принимающая значение <see cref="Result{TValue}" /> как аргумент </param>
    /// <param name="действиеОшибка">
    ///     процедура, которая может использоваться при отсутствии
    ///     значения <see cref="Result{TValue}" />
    /// </param>
    /// <typeparam name="Т">тип, хранимый в <see cref="Result{TValue}" /></typeparam>
    /// <returns></returns>
    public static void Если<Т>(this Result<Т> результат, Action<Т>? действиеОк, Action? действиеОшибка) {
        if (результат.IsFailed && действиеОшибка is not null)
            действиеОшибка();
        if (результат.IsSuccess && действиеОк is not null)
            действиеОк(результат.Value);
    }
}