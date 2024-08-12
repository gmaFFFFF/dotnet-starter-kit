namespace FluentResults.Ошибки;

/// <summary>
///     Аргумент не должен быть null
/// </summary>
public class АргументNullОшибка : АргументОшибка {
    public АргументNullОшибка() : this(null) { }

    public АргументNullОшибка(string? имяАргумента, string? сообщение = null)
        : base(сообщение ?? УстановитьСообщение(имяАргумента)) { }


    public АргументNullОшибка(string? имяАргумента, Error вызвана, string? сообщение = null)
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
    public АргументNullОшибка(object? аргумент, string? сообщение = null,
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
    public АргументNullОшибка(object? аргумент, Error вызвана, string? сообщение = null,
        [CallerArgumentExpression("аргумент")] string имяАргумента = "")
        : this(имяАргумента, вызвана, сообщение) { }

    [Pure]
    private static string УстановитьСообщение(string? имяАргумента) {
        const string сообщениеСтандартное = "Аргумент {0}не должен быть null";
        return string.IsNullOrEmpty(имяАргумента)
            ? string.Format(сообщениеСтандартное, "")
            : string.Format(сообщениеСтандартное, $"'{имяАргумента}' ");
    }
}