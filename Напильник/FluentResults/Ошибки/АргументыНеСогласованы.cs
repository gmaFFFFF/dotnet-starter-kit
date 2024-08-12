namespace FluentResults.Ошибки;

/// <summary>
///     Аргументы не согласованы
/// </summary>
public class АргументыНеСогласованы : АргументОшибка {
    public АргументыНеСогласованы() : this(null, null) { }

    public АргументыНеСогласованы(string? имяАргумента1, string? имяАргумента2, string? сообщение = null)
        : base(сообщение ?? УстановитьСообщение(имяАргумента1, имяАргумента2)) { }


    public АргументыНеСогласованы(string? имяАргумента1, string? имяАргумента2, Error вызвана, string? сообщение = null)
        : base(сообщение ?? УстановитьСообщение(имяАргумента1, имяАргумента2), вызвана) { }

    /// <summary>
    ///     Конструктор, который не требует передачи имен аргументов с помощью оператора nameof,
    ///     вместо этого используется <see cref="CallerArgumentExpressionAttribute" />
    /// </summary>
    /// <param name="аргумент1">
    ///     Первый несогласованный аргумент
    ///     <br />
    ///     Если он имеет тип <see cref="string" />, то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="аргумент2">
    ///     Второй несогласованный аргумент
    ///     <br />
    ///     Если он имеет тип <see cref="string" />, то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="сообщение">Пользовательское сообщение об ошибке</param>
    /// <param name="имяАргумента1">
    ///     заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" />
    /// </param>
    /// <param name="имяАргумента2">
    ///     заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" />
    /// </param>
    public АргументыНеСогласованы(object? аргумент1, object? аргумент2, string? сообщение = null,
        [CallerArgumentExpression("аргумент1")]
        string имяАргумента1 = "",
        [CallerArgumentExpression("аргумент2")]
        string имяАргумента2 = "")
        : this(имяАргумента1, имяАргумента2, сообщение) { }

    /// <summary>
    ///     Конструктор, который не требует передачи имени аргумента с помощью оператора nameof,
    ///     вместо этого используется <see cref="CallerArgumentExpressionAttribute" />
    /// </summary>
    /// <param name="аргумент1">
    ///     Первый несогласованный аргумент
    ///     <br />
    ///     Если он имеет тип <see cref="string" />, то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="аргумент2">
    ///     Второй несогласованный аргумент
    ///     <br />
    ///     Если он имеет тип <see cref="string" />, то осуществите его явное приведение к типу <see cref="object" />
    /// </param>
    /// <param name="вызвана"><see cref="Error" /> Ошибка, вызвавшая текущую</param>
    /// <param name="сообщение">Пользовательское сообщение об ошибке</param>
    /// <param name="имяАргумента1">
    ///     заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" />
    /// </param>
    /// <param name="имяАргумента2">
    ///     заполняется автоматически с помощью <see cref="CallerArgumentExpressionAttribute" />
    /// </param>
    public АргументыНеСогласованы(object? аргумент1, object? аргумент2, Error вызвана, string? сообщение = null,
        [CallerArgumentExpression("аргумент1")]
        string имяАргумента1 = "",
        [CallerArgumentExpression("аргумент2")]
        string имяАргумента2 = "")
        : this(имяАргумента1, имяАргумента2, вызвана, сообщение) { }

    [Pure]
    private static string УстановитьСообщение(string? имяАргумента1, string? имяАргумента2) {
        return (имяАргумента1, имяАргумента2) switch {
            (not null, not null) => $"Аргументы '{имяАргумента1}' и '{имяАргумента2}' не согласованы",
            (not null, null) => $"Аргумент '{имяАргумента1}' не согласован c другими",
            (null, not null) => $"Аргумент '{имяАргумента2}' не согласован c другими",
            _ => "Аргументы не согласованы"
        };
    }
}