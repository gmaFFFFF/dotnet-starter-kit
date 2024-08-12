using System.Linq.Expressions;
using gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace gmafffff.СтартовыйНабор.DDD.Инфраструктура.Данные;

public class СущностьХранилищеEF<ТСущность, ТИд> : ИСущностьХранилищеБд<ТСущность, ТИд>
    where ТСущность : class, ИСущность<ТИд>
    where ТИд : IEquatable<ТИд> {
    #region Служебное

    protected readonly DbContext Контекст;
    protected readonly DbSet<ТСущность> Сущности;

    public Func<IQueryable<ТСущность>, IIncludableQueryable<ТСущность, object>>? АвтоЗагружаемыеСвязанныеСущности {
        get;
        set;
    }

    public СущностьХранилищеEF(DbContext контекст) {
        Контекст = контекст.MustNotBeNull();
        Сущности = Контекст.Set<ТСущность>();
    }

    /// <summary>
    ///     Находит сущность, если она загружена локально
    /// </summary>
    /// <param name="ид"></param>
    /// <returns></returns>
    [Pure]
    protected ТСущность? НайдиЛокальноПоИд(ТИд ид) {
        return Сущности.Local.FirstOrDefault(с => с.Ид != null && с.Ид.Equals(ид));
    }

    /// <summary>
    ///     Создает запрос для поиска сущностей, соответствующих <paramref name="предикат" />у
    /// </summary>
    /// <param name="предикат">условие фильтрации</param>
    /// <param name="автоЗагружаемыеСвязанныеСущности">включить связанные сущности в загрузку</param>
    /// <param name="сортировка">сортировать результат по</param>
    /// <param name="отключитьОтслеживание">не отслеживать изменения найденных сущностей</param>
    /// <param name="отключитьГлобальныеФильтры">отключить фильтры уровня модели</param>
    /// <returns><see cref="IQueryable{T}" />></returns>
    /// <remarks>Источник: https://github.com/Arch/UnitOfWork/blob/master/src/UnitOfWork/Repository.cs</remarks>
    [Pure]
    protected IQueryable<ТСущность> ПостройЗапросНаПоиск(Expression<Func<ТСущность, bool>>? предикат = null,
        Func<IQueryable<ТСущность>, IIncludableQueryable<ТСущность, object>>? автоЗагружаемыеСвязанныеСущности = null,
        Func<IQueryable<ТСущность>, IOrderedQueryable<ТСущность>>? сортировка = null,
        bool отключитьОтслеживание = false,
        bool отключитьГлобальныеФильтры = false) {
        IQueryable<ТСущность> запрос = Сущности;
        if (отключитьОтслеживание)
            запрос = запрос.AsNoTracking();
        if (автоЗагружаемыеСвязанныеСущности is not null)
            запрос = автоЗагружаемыеСвязанныеСущности(запрос);
        if (предикат is not null)
            запрос = запрос.Where(предикат);
        if (отключитьГлобальныеФильтры)
            запрос = запрос.IgnoreQueryFilters();
        if (сортировка is not null)
            запрос = сортировка(запрос);
        return запрос;
    }

    [Pure]
    protected virtual IEnumerable<ИСобытияМодели> НайдиСущностиССобытиями(bool толькоУдаляемые = false) {
        var сущностиОтсл = Контекст.ChangeTracker.Entries();
        if (толькоУдаляемые)
            сущностиОтсл = сущностиОтсл.Where(entry => entry.State == EntityState.Deleted);

        return сущностиОтсл
            .Select(entry => entry.Entity)
            .OfType<ИСобытияМодели>();
    }

    #endregion

    #region Поиск

    public virtual IImmutableList<ТСущность> Найди(Expression<Func<ТСущность, bool>>? предикат = null) {
        return предикат is null
            ? ImmutableArray<ТСущность>.Empty
            : ПостройЗапросНаПоиск(предикат, АвтоЗагружаемыеСвязанныеСущности).ToImmutableArray();
    }


    public virtual async Task<IImmutableList<ТСущность>> НайдиАсинх(Expression<Func<ТСущность, bool>>? предикат = null,
        CancellationToken маячокОтмены = default) {
        if (предикат is null)
            return ImmutableArray<ТСущность>.Empty;
        var результат = await ПостройЗапросНаПоиск(предикат, АвтоЗагружаемыеСвязанныеСущности).ToListAsync(маячокОтмены)
            .ConfigureAwait(false);
        return результат.ToImmutableArray();
    }

    public virtual ТСущность? Найди(ТИд ид) {
        return НайдиЛокальноПоИд(ид) ?? Найди(с => ид.Equals(с.Ид)).SingleOrDefault();
    }

    public virtual async Task<ТСущность?> НайдиАсинх(ТИд ид, CancellationToken маячокОтмены = default) {
        return НайдиЛокальноПоИд(ид) ??
               (await НайдиАсинх(с => ид.Equals(с.Ид), маячокОтмены).ConfigureAwait(false)).SingleOrDefault();
    }

    public virtual ТСущность? this[ТИд ид] => Найди(ид);

    public virtual IImmutableList<ТСущность> this[Expression<Func<ТСущность, bool>>? предикат] => Найди(предикат);

    #endregion

    #region Добавление

    public virtual void Добавь(ТСущность сущность) {
        Сущности.Add(сущность);
    }

    public virtual void Добавь(IEnumerable<ТСущность> сущности) {
        Сущности.AddRange(сущности);
    }

    #endregion

    #region Удаление

    public virtual void Удали(ТСущность сущность) {
        Сущности.Remove(сущность);
    }

    public virtual void Удали(ТИд ид) {
        if (НайдиЛокальноПоИд(ид) is { } сущность) {
            Удали(сущность);
            return;
        }

        if (СоздайФиктивнуюСущностьИд(ид) is { } фикт) {
            Удали(фикт);
            return;
        }

        if (Найди(ид) is { } сущностьБд)
            Удали(сущностьБд);

        ТСущность? СоздайФиктивнуюСущностьИд(ТИд ид) {
            var типИнфо = typeof(ТСущность);
            var ключПолеИмя = Контекст.Model.FindEntityType(типИнфо)?.FindPrimaryKey()?.Properties[0].Name;
            if (ключПолеИмя is null) return null;

            var конструктор = () => (ТСущность?)типИнфо.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic,
                Type.EmptyTypes)?.Invoke(null);
            var установщикИд = типИнфо.GetProperty(ключПолеИмя,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var сущностьФикт = конструктор();
            if (сущностьФикт is not null)
                установщикИд?.SetValue(сущностьФикт, ид);

            return сущностьФикт;
        }
    }

    public virtual void Удали(IEnumerable<ТСущность> сущности) {
        Сущности.RemoveRange(сущности);
    }

    public virtual void Удали(Expression<Func<ТСущность, bool>>? предикат = null) {
        Удали(ПостройЗапросНаПоиск(предикат));
    }

    public virtual async Task УдалиАсинх(Expression<Func<ТСущность, bool>>? предикат = null,
        CancellationToken маячокОтмены = default) {
        var сущности = await ПостройЗапросНаПоиск(предикат).ToArrayAsync(маячокОтмены).ConfigureAwait(false);
        if (маячокОтмены.IsCancellationRequested) return;
        Удали(сущности);
    }

    #endregion

    #region Характеристики

    public virtual int ДайРазмер() {
        return Сущности.Count();
    }

    public virtual async Task<int> ДайРазмерАсинх(CancellationToken маячокОтмены = default) {
        return await Сущности.CountAsync(маячокОтмены).ConfigureAwait(false);
    }

    public virtual int ДайРазмерПоУсловию(Expression<Func<ТСущность, bool>>? предикат = null) {
        return предикат is null ? 0 : ПостройЗапросНаПоиск(предикат).Count();
    }

    public virtual async Task<int> ДайРазмерПоУсловиюАсинх(Expression<Func<ТСущность, bool>>? предикат = null,
        CancellationToken маячокОтмены = default) {
        return предикат is null
            ? 0
            : await ПостройЗапросНаПоиск(предикат).CountAsync(маячокОтмены).ConfigureAwait(false);
    }

    #endregion

    #region Сохранение

    public virtual Result<IImmutableSet<ИСобытиеМодели>> СохраниИОтдайСобытия() {
        var сущностиССобытиями = НайдиСущностиССобытиями(true).ToArray();
        try {
            Контекст.SaveChanges();
        }
        catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        IImmutableSet<ИСобытиеМодели> события = сущностиССобытиями.AsEnumerable()
            .Concat(НайдиСущностиССобытиями())
            .SelectMany(сущность => сущность.СобытияМоделиПередатьНаПубликацию())
            .ToImmutableHashSet();

        return Result.Ok(события);
    }

    public virtual async Task<Result<IImmutableSet<ИСобытиеМодели>>> СохраниИОтдайСобытияАсинх() {
        var сущностиССобытиями = НайдиСущностиССобытиями(true).ToArray();
        try {
            await Контекст.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e) {
            return Result.Fail(new ExceptionalError(e));
        }

        IImmutableSet<ИСобытиеМодели> события = сущностиССобытиями.AsEnumerable()
            .Concat(НайдиСущностиССобытиями())
            .SelectMany(сущность => сущность.СобытияМоделиПередатьНаПубликацию())
            .ToImmutableHashSet();
        return Result.Ok(события);
    }

    #endregion
}