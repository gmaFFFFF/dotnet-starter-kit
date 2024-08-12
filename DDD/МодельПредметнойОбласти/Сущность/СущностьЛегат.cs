namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

/// <summary>
///     Сущность для доступа к совокупности логически связанных сущностей (агрегат)
/// </summary>
public abstract class СущностьЛегат : Сущность<Guid>, ИСобытияМодели<Guid> {
    #region События

    private HashSet<ИСобытиеМодели<Guid>>? _событияМодели;

    public IImmutableSet<ИСобытиеМодели<Guid>> СобытияМодели =>
        _событияМодели?.ToImmutableHashSet() ?? ImmutableHashSet<ИСобытиеМодели<Guid>>.Empty;

    public virtual IImmutableSet<ИСобытиеМодели<Guid>> СобытияМоделиПередатьНаПубликацию() {
        var рез = СобытияМодели;
        _событияМодели = null;
        return рез;
    }

    /// <summary>
    ///     Регистрирует новые события
    /// </summary>
    /// <param name="события">новые события</param>
    protected virtual void СобытияМоделиДобавить(params ИСобытиеМодели<Guid>[] события) {
        (_событияМодели ??= new HashSet<ИСобытиеМодели<Guid>>()).UnionWith(события);
    }

    /// <summary>
    ///     Удаляет события
    /// </summary>
    /// <param name="события">удаляемые события</param>
    protected virtual void СобытияМоделиУдалить(params ИСобытиеМодели<Guid>[] события) {
        _событияМодели?.ExceptWith(события);
    }

    #endregion
}