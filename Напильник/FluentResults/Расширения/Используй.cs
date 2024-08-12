namespace FluentResults;

public static partial class РезультатРасширения {
    /// <summary>
    ///     Если текущий <see cref="Result{TValue}" /> успешен, то вызывает процедуру "<paramref name="действие" />", со
    ///     значением <see cref="Result{TValue}" /> как аргумента и возвращает текущий <see cref="Result{TValue}" />
    /// </summary>
    /// <param name="результат">текущий <see cref="Result{TValue}" /></param>
    /// <param name="действие">процедура, принимающая значение <see cref="Result{TValue}" /> как аргумент</param>
    /// <typeparam name="Т">тип, принимаемый процедурой "<paramref name="действие" />"</typeparam>
    public static Result<Т> Используй<Т>(this Result<Т> результат, Action<Т> действие) {
        if (результат.IsFailed) return результат;

        действие.MustNotBeNull();

        действие(результат.Value);
        return результат;
    }
}