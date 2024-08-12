using System.Linq.Expressions;
using static DotNext.Linq.Expressions.ExpressionBuilder;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Помощник;

/// <summary>
///     Плагиатор создаёт копию объекта с изменением отдельных его свойств с помощью инициализатора
/// </summary>
/// <remarks> Чужой объект должен определять конструктор копий, а изменяемые члены поддерживать инициализацию </remarks>
public struct Плагиатор {
    /// <summary>
    ///     Объект, копию которого необходимо создать
    /// </summary>
    private readonly object _чужойОбъект;

    /// <summary>
    ///     Тип <see cref="_чужойОбъект" />
    /// </summary>
    private Type _тип => _чужойОбъект.GetType();

    /// <summary>
    ///     Объект, анализирующий Expression
    /// </summary>
    private readonly РазборИнициализацииExpression _анализатор = new();

    public Плагиатор(object чужойОбъект) {
        _чужойОбъект = чужойОбъект.MustNotBeNullReference();
        if (!КонструкторКопийЕстьЛи())
            throw new ArgumentException($"Тип {_тип.FullName} должен реализовать конструктор копий");
    }

    /// <summary>
    ///     Есть ли конструктор копий у объекта, который мы хотим плагиатить?
    /// </summary>
    /// <returns>True - конструктор копий есть, иначе - false</returns>
    [Pure]
    private bool КонструкторКопийЕстьЛи() {
        return _тип.GetConstructor(new[] { _тип }) is not null;
    }

    /// <summary>
    ///     Возвращает копию чужого объекта с изменением отдельных его свойств с помощью инициализатора
    /// </summary>
    /// <param name="иницализатор">
    ///     <see cref="Expression" /> создания объекта с инициализацией тех полей,
    ///     которые необходимо изменить:
    ///     <code>плагиатор.Плагиать(() => new T() {Член1 = "Измененное значение1", Член2 = "Измененное значение2"})</code>
    ///     или <see cref="Expression" /> создания анонимного типа с инициализацией тех полей, которые необходимо изменить:
    ///     <code>плагиатор.Плагиать(() => new {Член1 = "Измененное значение1", Член2 = "Измененное значение2"})</code>
    /// </param>
    /// <returns>изменённая копия объекта</returns>
    [Pure]
    public object Плагиать(Expression<Func<object>> иницализатор) {
        var (новыеЗначения, конструкПарам) = _анализатор.АнализируйИнициализацию(иницализатор);
        var констрКопий = _тип.New(_чужойОбъект.Const().Convert(_тип));

        Expression плагиатор = (новыеЗначения.Count, конструкПарам.Count) switch {
            (> 0, _) => Expression.MemberInit(констрКопий, новыеЗначения.Values),
            (0, > 0) => Expression.MemberInit(констрКопий, КонструкторВСписокИнициализации(конструкПарам).Values),
            _ => констрКопий
        };


        var плагиать = Expression.Lambda<Func<object>>(плагиатор).Compile();

        return плагиать();
    }

    /// <summary>
    ///     Преобразовывает список параметров конструктора в список инициализации
    /// </summary>
    /// <param name="конструкторПараметры"><see cref="Expression" /> параметры конструктора</param>
    /// <returns><see cref="Expression" /> инициализатора</returns>
    /// <exception cref="ArgumentException">
    ///     выбрасывается, если не удалось сопоставить
    ///     параметр конструктора с членом класса
    /// </exception>
    [Pure]
    public Dictionary<string, MemberAssignment> КонструкторВСписокИнициализации(
        Dictionary<string, Expression> конструкторПараметры) {
        Dictionary<string, MemberAssignment> инициализатор = new();
        const BindingFlags флагРефлексии = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public;
        var всеСвойства = _тип.GetProperties(флагРефлексии);
        var всеПоля = _тип.GetFields(флагРефлексии);

        foreach (var имяЧлена in конструкторПараметры.Keys) {
            var свойство = всеСвойства.FirstOrDefault(с => с.Name == имяЧлена);
            var поле = всеПоля.FirstOrDefault(с => с.Name == имяЧлена);
            var (член, членТип) = (свойство, поле) switch {
                (not null, _) => (свойство, свойство.PropertyType),
                (null, not null) => ((MemberInfo)поле, поле.FieldType),
                _ => throw new ArgumentException($"{_тип.FullName} не содержит член {имяЧлена}")
            };

            инициализатор[имяЧлена] = Expression.Bind(член, конструкторПараметры[имяЧлена].Convert(членТип));
        }

        return инициализатор;
    }

    /// <summary>
    ///     Шаблон "Посетитель" для прохода по дереву выражений с целью выделить
    ///     выражение инициализации и параметры конструктора
    /// </summary>
    private class РазборИнициализацииExpression : ExpressionVisitor {
        private Dictionary<string, MemberAssignment>? _инициализатор;
        private Dictionary<string, Expression>? _инициализаторAnonym;

        /// <summary>
        ///     Разбирает дерево выражений и возвращает инициализатор и параметры конструктора
        /// </summary>
        /// <param name="инициализатор"><see cref="Expression" /> для разбора</param>
        /// <returns> кортеж инициализатора и конструктора</returns>
        public РазобранаИнициализация АнализируйИнициализацию(Expression инициализатор) {
            _инициализатор = new Dictionary<string, MemberAssignment>();
            _инициализаторAnonym = new Dictionary<string, Expression>();
            Visit(инициализатор);
            return new РазобранаИнициализация(_инициализатор, _инициализаторAnonym);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
            _инициализатор![node.Member.Name] = node;
            return base.VisitMemberAssignment(node);
        }

        protected override Expression VisitNew(NewExpression node) {
            foreach (var (член, значение) in node.Members?.Zip(node.Arguments) ??
                                             Array.Empty<(MemberInfo First, Expression Second)>())
                _инициализаторAnonym![член.Name] = значение;

            return base.VisitNew(node);
        }

        /// <summary>
        ///     Тип для возврата значений из метода <see cref="РазобранаИнициализация" />
        /// </summary>
        /// <param name="Инициализатор"></param>
        /// <param name="КонструкторПараметры"></param>
        public record struct РазобранаИнициализация(
            Dictionary<string, MemberAssignment> Инициализатор,
            Dictionary<string, Expression> КонструкторПараметры);
    }
}