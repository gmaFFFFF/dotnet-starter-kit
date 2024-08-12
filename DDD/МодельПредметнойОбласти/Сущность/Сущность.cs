using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

/// <summary>
///     Базовый класс для сущностей
/// </summary>
/// <typeparam name="ТИд">Тип поля идентификатора</typeparam>
public abstract class Сущность<ТИд> : ИСущность<ТИд>
    where ТИд : IEquatable<ТИд> {
    #region Конструкторы

    protected Сущность(ТИд ид) {
        Ид = ид;
    }

    protected Сущность() { }

    #endregion

    #region Хранение

    /// <summary>
    ///     Ключ, идентифицирующий сущность
    /// </summary>
    [Key]
    public ТИд? Ид { get; protected set; }

    /// <summary>
    ///     Сущность извлечена из хранилища (<c>true</c>), а не создана приложением
    /// </summary>
    /// <returns>
    ///     True - сущность извлечена из хранилища, false - сущность временная
    /// </returns>
    [Pure]
    public virtual bool ИзХранилищаЛи() {
        return Ид switch {
            //EF Core устанавливает значение int / long равным min при прикреплении(Attach) сущности к dbcontext
            (long or int) and <= 0 => false,
            Guid гуид when гуид == Guid.Empty => false,
            _ => !(Ид is null || Ид.Equals(default))
        };
    }

    #endregion

    #region Методы сравнения

    /// <summary>
    ///     Кэшированный хэшкод
    /// </summary>
    [NotMapped] private int? _хэшКод;

    [Pure]
    public override int GetHashCode() {
        if (_хэшКод.HasValue) return _хэшКод.Value;

        var хэш = new HashCode();
        хэш.Add(GetType().FullName?.GetHashCode() ?? base.GetHashCode());

        if (ИзХранилищаЛи()) хэш.Add(Ид!.GetHashCode() ^ 31); // XOR for random distribution
        // (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

        _хэшКод = хэш.ToHashCode();
        return _хэшКод.Value;
    }

    [Pure]
    public bool Equals(ИСущность<ТИд>? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;
        return ИзХранилищаЛи() && other.ИзХранилищаЛи() && Ид!.Equals(other.Ид);
    }

    [Pure]
    public override bool Equals(object? obj) {
        return Equals(obj as ИСущность<ТИд>);
    }

    [Pure]
    public static bool operator ==(Сущность<ТИд>? левый, ИСущность<ТИд>? правый) {
        return левый?.Equals(правый) ?? Equals(правый, null);
    }

    [Pure]
    public static bool operator !=(Сущность<ТИд>? левый, ИСущность<ТИд>? правый) {
        return !(левый == правый);
    }

    [Pure]
    public static bool operator ==(Сущность<ТИд>? левый, Сущность<ТИд>? правый) {
        return левый?.Equals(правый) ?? Equals(правый, null);
    }

    [Pure]
    public static bool operator !=(Сущность<ТИд>? левый, Сущность<ТИд>? правый) {
        return !(левый == правый);
    }

    #endregion
}