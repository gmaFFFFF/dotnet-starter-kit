using System.Linq.Expressions;
using static DotNext.Linq.Expressions.ExpressionBuilder;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

public static class ПсевдоЗначимыйТипПлагиатор {
    /// <summary>
    ///     Создаёт копию объекта с изменением отдельных его свойств с помощью инициализатора
    /// </summary>
    /// <param name="чужой">Объект, которые необходимо плагиатить</param>
    /// <param name="иницализатор">
    ///     <see cref="Expression" /> создания объекта тип <typeparamref name="Т" />
    ///     с инициализацией тех полей, которые необходимо изменить:
    ///     <code>чужой.Плагиать(() => new T(null) {Член1 = "Измененное значение1", Член2 = "Измененное значение2"});</code>
    ///     или <see cref="Expression" /> создания анонимного типа с инициализацией тех полей, которые необходимо изменить:
    ///     <code>чужой.Плагиать(() => new {Член1 = "Измененное значение1", Член2 = "Измененное значение2"});</code>
    /// </param>
    /// <returns>измененная копия объекта</returns>
    /// <remarks>конкретный тип <typeparamref name="Т" /> и параметры конструктора не учитываются</remarks>
    public static object Плагиать<Т>(this ПсевдоЗначимыйТип чужой, Expression<Func<Т>> иницализатор)
        where Т : class {
        var телоПриведено = иницализатор.Convert<object>();
        var иницализаторПриведен = Expression.Lambda<Func<object>>(телоПриведено, иницализатор.Parameters);

        return new Плагиатор(чужой).Плагиать(иницализаторПриведен);
    }
}