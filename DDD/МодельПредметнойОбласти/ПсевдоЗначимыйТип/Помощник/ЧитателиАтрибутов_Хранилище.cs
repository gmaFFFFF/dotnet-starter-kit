namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Помощник;

/// <summary>
///     Кэширует <see cref="ЧитательАтрибута" />
/// </summary>
public class ЧитателиАтрибутов_Хранилище {
    /// <summary>
    ///     Хранилище считывателей атрибутов класса
    /// </summary>
    protected Dictionary<Type, IEnumerable<ЧитательАтрибута>> Хранилище = new();

    public ЧитателиАтрибутов_Хранилище() { }

    public ЧитателиАтрибутов_Хранилище(HashSet<Type> игнорируемыйБазовыйКласс) {
        ИгнорируемыйБазовыйКласс = игнорируемыйБазовыйКласс;
    }

    /// <summary>
    ///     Базовые классы иерархии, начиная с которых читатели атрибутов не создаются
    /// </summary>
    protected HashSet<Type> ИгнорируемыйБазовыйКласс { get; init; } = new() { typeof(object) };

    /// <summary>
    ///     Возвращает (и добавляет при необходимости) <see cref="IEnumerable{T}" /> считыватели
    ///     атрибутов (<see cref="ЧитательАтрибута" />)  типа "<paramref name="тип" />" из внутреннего хранилища
    /// </summary>
    /// <param name="тип"><see cref="Type" /> класса</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" /> <see cref="ЧитательАтрибута" />
    /// </returns>
    public Result<IEnumerable<ЧитательАтрибута>> ДайСчитывателиЗначений(Type тип) {
        if (тип is null) return Result.Fail(new АргументNullОшибка(тип));

        if (Хранилище.TryGetValue(тип, out var считыватели))
            return Result.Ok(считыватели);

        return СоздайСчитывателиЗначений(тип)
            .Используй(сч => Хранилище[тип] = сч);
    }

    /// <summary>
    ///     Создаёт <see cref="IEnumerable{T}" /> считыватели атрибутов (<see cref="ЧитательАтрибута" />)
    ///     типа "<paramref name="тип" />"
    /// </summary>
    /// <param name="тип"><see cref="Type" /> класса</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" /> <see cref="ЧитательАтрибута" />
    /// </returns>
    protected Result<IEnumerable<ЧитательАтрибута>> СоздайСчитывателиЗначений(Type тип) {
        if (тип is null) return Result.Fail(new АргументNullОшибка(тип));

        if (Хранилище.TryGetValue(тип, out var считыватели)) return Result.Ok(считыватели);

        var иерархияТипа = Сравниватель_Помощник.ДайВсюИерахиюТипов(тип).ToArray();
        var учитываемыеКлассы = иерархияТипа.TakeWhile(тек => !ИгнорируемыйБазовыйКласс.Contains(тек));

        var родитель = учитываемыеКлассы.Skip(1).FirstOrDefault();

        var атрибутыТипа = Сравниватель_Помощник
            .ДайСчитывателиПубличныхАтрибутовЭкземпляра(тип);
        var атрибутыРодителя = родитель is not null
            ? ДайСчитывателиЗначений(родитель)
                .Или(Enumerable.Empty<ЧитательАтрибута>)
            : Enumerable.Empty<ЧитательАтрибута>();

        if (учитываемыеКлассы.Any() || !атрибутыТипа.IsSuccess)
            return атрибутыТипа.Преобразуй(сч => Result.Ok(атрибутыРодителя.Concat(сч)));

        var пользовательскийАтрибут = nameof(ПсевдоЗначимыйТип.КомпонентыСравненияПользовательские);
        var считыватель = атрибутыТипа
            .Value
            .Where(читатель => читатель.ИмяАтрибута == пользовательскийАтрибут);

        return Result.Ok(считыватель);
    }
}