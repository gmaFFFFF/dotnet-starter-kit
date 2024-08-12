namespace FluentResults.Ошибки;

/// <summary>
///     Допущена ошибка в аргументе метода
/// </summary>
public class АргументОшибка : Error {
    public АргументОшибка() : this(null) { }

    public АргументОшибка(string? имяАргумента, string? сообщение = null)
        : base(сообщение ?? УстановитьСообщение(имяАргумента)) { }

    public АргументОшибка(string? имяАргумента, Error вызвана, string? сообщение = null)
        : base(сообщение ?? УстановитьСообщение(имяАргумента), вызвана) { }

    /// <summary>
    ///     Конструктор, который не требует передачи имени аргумента с помощью оператора nameof,
    ///     вместо этого используется <see cref="CallerArgumentExpressionAttribute" />
    /// </summary>
    /// <param name="аргумент">
    ///     Аргумент вызвавший ошибку, если он имеет тип <see cref="string" />,
    ///     то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="сообщение">Пользовательское сообщение об ошибке</param>
    /// <param name="имяАргумента">заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" /></param>
    public АргументОшибка(object? аргумент, string? сообщение = null,
        [CallerArgumentExpression("аргумент")] string имяАргумента = "")
        : this(имяАргумента, сообщение) { }

    /// <summary>
    ///     Конструктор, который не требует передачи имени аргумента с помощью оператора nameof,
    ///     вместо этого используется <see cref="CallerArgumentExpressionAttribute" />
    /// </summary>
    /// <param name="аргумент">
    ///     Аргумент вызвавший ошибку, если он имеет тип <see cref="string" />,
    ///     то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="вызвана"><see cref="Error" /> Ошибка, вызвавшая текущую</param>
    /// <param name="сообщение">Пользовательское сообщение об ошибке</param>
    /// <param name="имяАргумента">заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" /></param>
    public АргументОшибка(object? аргумент, Error вызвана, string? сообщение = null,
        [CallerArgumentExpression("аргумент")] string имяАргумента = "")
        : this(имяАргумента, вызвана, сообщение) { }


    [Pure]
    private static string УстановитьСообщение(string? имяАргумента) {
        const string сообщениеСтандартное = "Ошибочный аргумент{0}";
        return string.IsNullOrEmpty(имяАргумента)
            ? string.Format(сообщениеСтандартное, "")
            : string.Format(сообщениеСтандартное, $" '{имяАргумента}'");
    }
}