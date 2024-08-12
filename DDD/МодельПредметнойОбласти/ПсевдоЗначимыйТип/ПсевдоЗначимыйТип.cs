using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

/// <summary>
///     Базовый класс для объектов-значений
/// </summary>
/// <remarks>
///     Производные классы должны быть неизменяемыми
/// </remarks>
public abstract partial class ПсевдоЗначимыйТип :
    IEquatable<ПсевдоЗначимыйТип>,
    IComparable,
    IComparable<ПсевдоЗначимыйТип> {
    public override string ToString() {
        var options = new JsonSerializerOptions {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };
        return JsonSerializer.Serialize(this, GetType(), options);
    }

    #region Сравнение

    /// <summary>
    ///     <see cref="IEnumerable{T}" /> значений, по которым будут сравниваться объекты значения (факультативно)
    /// </summary>
    /// <remarks>
    ///     Если не задан, то для сравнения используются все публичные поля и свойства <see cref="ПсевдоЗначимыйТип" />,
    ///     кроме отмеченных <see cref="НеСравниватьПоAttribute" />.
    ///     Можно включать любые члены класса, а также любые вычисляемые значения
    /// </remarks>
    [NotMapped] protected IEnumerable<object?>? _компонентыСравненияПользовательские;

    /// <summary>
    ///     <see cref="IEnumerable{T}" /> значений, по которым будут сравниваться объекты значения (факультативно)
    /// </summary>
    /// <remarks>
    ///     Если не задан, то для сравнения используются все публичные поля и свойства <see cref="ПсевдоЗначимыйТип" />,
    ///     кроме отмеченных <see cref="НеСравниватьПоAttribute" />
    /// </remarks>
    public IEnumerable<object?>? КомпонентыСравненияПользовательские => _компонентыСравненияПользовательские;

    /// <summary>
    ///     Вспомогательный объект для сравнения <see cref="ПсевдоЗначимыйТип" />
    /// </summary>
    /// <remarks>
    ///     Сравниватель по умолчанию игнорирует регистр строк (использует
    ///     <see cref="StringComparison.CurrentCultureIgnoreCase" />)
    /// </remarks>
    [NotMapped] protected readonly СравнивательСтандартный Сравниватель = СравнивательСтандартный.Сравниватель;

    public bool Equals(ПсевдоЗначимыйТип? other) {
        return ((IComparer<ПсевдоЗначимыйТип?>)Сравниватель).Compare(this, other) == 0;
    }

    public override bool Equals(object? obj) {
        return Equals(obj as ПсевдоЗначимыйТип);
    }

    public static bool operator ==(ПсевдоЗначимыйТип? левый, ПсевдоЗначимыйТип? правый) {
        return левый?.Equals(правый) ?? правый?.Equals(левый) ?? true;
    }

    public static bool operator !=(ПсевдоЗначимыйТип? левый, ПсевдоЗначимыйТип? правый) {
        return !(левый == правый);
    }

    int IComparable<ПсевдоЗначимыйТип>.CompareTo(ПсевдоЗначимыйТип? other) {
        return ((IComparer<ПсевдоЗначимыйТип>)Сравниватель).Compare(this, other);
    }

    int IComparable.CompareTo(object? obj) {
        return ((IComparer)Сравниватель).Compare(this, obj);
    }

    #endregion

    #region Хэш-код

    public override int GetHashCode() {
        if (_хэшКод.HasValue) return _хэшКод.Value;

        _хэшКод = Сравниватель.РассчитайХэшКод(this);
        return _хэшКод.Value;
    }

    /// <summary>
    ///     Кэшированный хэшкод
    /// </summary>
    [NotMapped] private int? _хэшКод;

    #endregion
}