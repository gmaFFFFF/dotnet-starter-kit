using MediatR;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

/// <summary>
///     Универсальный интерфейс события модели предметной области
/// </summary>
public interface ИСобытиеМодели : INotification {
    /// <summary>
    ///     Момент времени, когда произошло событие
    /// </summary>
    DateTime Произошло { get; init; }

    /// <summary>
    ///     Сведения о <see cref="Type" /> отправителя
    /// </summary>
    Type ОтправительТип { get; init; }

    /// <summary>
    ///     Уникальный идентификатор сущности отправителя
    /// </summary>
    object? ОтправительИд { get; init; }
}

/// <summary>
///     Параметризованный по типу идентификатора сущности интерфейс события модели предметной области
/// </summary>
/// <typeparam name="ТИд">тип уникального идентификатора сущности</typeparam>
public interface ИСобытиеМодели<out ТИд> : ИСобытиеМодели
    where ТИд : IEquatable<ТИд> {
    /// <summary>
    ///     Уникальный идентификатор сущности отправителя
    /// </summary>
    new ТИд? ОтправительИд { get; }

    #region Реализация ИСобытиеМодели

    object? ИСобытиеМодели.ОтправительИд {
        get => ОтправительИд;
        init => throw new NotImplementedException();
    }

    #endregion
}