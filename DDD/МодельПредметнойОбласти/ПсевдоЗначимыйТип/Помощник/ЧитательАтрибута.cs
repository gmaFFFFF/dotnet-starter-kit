namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Помощник;

/// <summary>
///     Объект для чтения значений атрибутов класса с помощью рефлексии
/// </summary>
public class ЧитательАтрибута {
    /// <summary>
    ///     Создаёт новый считыватель
    /// </summary>
    /// <param name="имяАтрибута">имя атрибута в типе</param>
    /// <param name="attributesАтрибута">пользовательские <see cref="Attribute" /> атрибута</param>
    /// <param name="дайЗначение">
    ///     <see cref="DynamicInvoker" /> из библиотеки DotNext.Reflection.
    ///     Создаётся с помощью вызова метода расширения
    ///     <see cref="Reflector.Unreflect(System.Reflection.FieldInfo,System.Reflection.BindingFlags,bool)" /> или
    ///     <see cref="Reflector.Unreflect(System.Reflection.MethodInfo)" />
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ЧитательАтрибута(string имяАтрибута, IEnumerable<Attribute>? attributesАтрибута,
        Func<object?, object?> дайЗначение) {
        ArgumentNullException.ThrowIfNull(дайЗначение);
        ИмяАтрибута = имяАтрибута.MustNotBeNull();
        ДайЗначение = target => дайЗначение(target.MustNotBeNull());
        Attributes = attributesАтрибута ?? Array.Empty<Attribute>();
    }

    /// <summary>
    ///     Имя атрибута в типе
    /// </summary>
    public string ИмяАтрибута { get; init; }

    /// <summary>
    ///     Фунуция для считывания значения атрибута класса
    /// </summary>
    /// <remarks>
    ///     Объект, у которого необходимо считать значение атрибута, передаётся в качестве параметра функции
    /// </remarks>
    public Func<object, object?> ДайЗначение { get; init; }

    /// <summary>
    ///     Пользовательские <see cref="Attribute" /> атрибута
    /// </summary>
    public IEnumerable<Attribute> Attributes { get; init; }
}