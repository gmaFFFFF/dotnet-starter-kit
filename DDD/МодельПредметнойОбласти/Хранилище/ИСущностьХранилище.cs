using System.Linq.Expressions;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти;

public interface ИСущностьХранилище<ТСущность, in ТИд>
    where ТСущность : class, ИСущность<ТИд>
    where ТИд : IEquatable<ТИд> {
    #region Поиск

    /// <summary>
    ///     Находит сущности, соответствующие <paramref name="предикат" />у
    /// </summary>
    /// <param name="предикат">условие для поиска сущностей. Если не задан, то вернет пустую коллекцию</param>
    /// <returns><see cref="IImmutableList{T}" />найденных сущностей</returns>
    IImmutableList<ТСущность> Найди(Expression<Func<ТСущность, bool>>? предикат = null);

    /// <summary>
    ///     Асинхронно находит сущности, соответствующие <paramref name="предикат" />у
    /// </summary>
    /// <param name="предикат">условие для поиска сущностей. Если не задан, то вернет пустую коллекцию</param>
    /// <param name="маячокОтмены"><see cref="CancellationToken" /> отмены операции</param>
    /// <returns><see cref="IImmutableList{T}" />найденных сущностей</returns>
    Task<IImmutableList<ТСущность>> НайдиАсинх(Expression<Func<ТСущность, bool>>? предикат = null,
        CancellationToken маячокОтмены = default);

    /// <summary>
    ///     Находит единственую сущность, с идентификатором <paramref name="ид" />
    /// </summary>
    /// <param name="ид">уникальный идентификатор сущности</param>
    /// <returns>найденная сущность или null, если сущность не найдена</returns>
    ТСущность? Найди(ТИд ид);

    /// <summary>
    ///     Асинхронно находит единственую сущность, с идентификатором <paramref name="ид" />
    /// </summary>
    /// <param name="ид">уникальный идентификатор сущности</param>
    /// <param name="маячокОтмены"><see cref="CancellationToken" /> отмены операции</param>
    /// <returns>найденная сущность или null, если сущность не найдена</returns>
    Task<ТСущность?> НайдиАсинх(ТИд ид, CancellationToken маячокОтмены = default);

    /// <summary>
    ///     Возвращает сущность по ключу
    /// </summary>
    /// <param name="ид">уникальный идентификатор сущности</param>
    ТСущность? this[ТИд ид] => Найди(ид);

    /// <summary>
    ///     Возвращает сущности, соответствующие предикату
    /// </summary>
    /// <param name="предикат">условие для поиска сущностей. Если не задан, то вернет пустую коллекцию</param>
    IImmutableList<ТСущность> this[Expression<Func<ТСущность, bool>>? предикат] => Найди(предикат);

    #endregion

    #region Добавление

    /// <summary>
    ///     Добавляет сущность в хранилище
    /// </summary>
    /// <param name="сущность">добавляемая сущность</param>
    void Добавь(ТСущность сущность);

    /// <summary>
    ///     Добавляет сущности в хранилище
    /// </summary>
    /// <param name="сущности">добавляемые сущности</param>
    void Добавь(IEnumerable<ТСущность> сущности);

    #endregion

    #region Удаление

    /// <summary>
    ///     Удаляет сущность из хранилища
    /// </summary>
    /// <param name="сущность">удаляемая сущность</param>
    void Удали(ТСущность сущность);


    /// <summary>
    ///     Удаляет сущность из хранилища
    /// </summary>
    /// <param name="ид">уникальный идентификатор удаляемой сущности</param>
    void Удали(ТИд ид);

    /// <summary>
    ///     Удаляет сущности из хранилища
    /// </summary>
    /// <param name="сущности">удаляемые сущности</param>
    void Удали(IEnumerable<ТСущность> сущности);

    /// <summary>
    ///     Удаляет сущности из хранилища
    /// </summary>
    /// <param name="предикат">условие для поиска удаляемых сущностей. Если не задан, то ничего не удалит</param>
    void Удали(Expression<Func<ТСущность, bool>>? предикат = null);

    /// <summary>
    ///     Асинхронно удаляет сущности из хранилища
    /// </summary>
    /// <param name="предикат">условие для поиска удаляемых сущностей. Если не задан, то ничего не удалит</param>
    /// <param name="маячокОтмены"><see cref="CancellationToken" /> отмены операции</param>
    Task УдалиАсинх(Expression<Func<ТСущность, bool>>? предикат = null, CancellationToken маячокОтмены = default);

    #endregion

    #region Характеристики

    /// <summary>
    ///     Возвращает число элементов в хранилище
    /// </summary>
    int ДайРазмер();

    /// <summary>
    ///     Асинхронно возвращает число элементов в хранилище
    /// </summary>
    Task<int> ДайРазмерАсинх(CancellationToken маячокОтмены = default);

    /// <summary>
    ///     Подсчитывает количество сущностей соответствующих <see cref="предикат" />у
    /// </summary>
    /// <param name="предикат">условие для поиска удаляемых сущностей. Если не задан, то вернёт 0</param>
    /// <returns>Число сущностей, соответствующих <see cref="предикат" />у </returns>
    int ДайРазмерПоУсловию(Expression<Func<ТСущность, bool>>? предикат = null);

    /// <summary>
    ///     Асинхронно подсчитывает количество сущностей соответствующих <see cref="предикат" />у
    /// </summary>
    /// <param name="предикат">условие для поиска удаляемых сущностей. Если не задан, то вернёт 0</param>
    /// <param name="маячокОтмены"><see cref="CancellationToken" /> отмены операции</param>
    /// <returns>Число сущностей, соответствующих <see cref="предикат" />у </returns>
    Task<int> ДайРазмерПоУсловиюАсинх(Expression<Func<ТСущность, bool>>? предикат = null,
        CancellationToken маячокОтмены = default);

    #endregion
}