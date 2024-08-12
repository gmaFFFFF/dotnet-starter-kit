using DotNext.Collections.Generic;

namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Помощник;

public static class Сравниватель_Помощник {
    // Todo: Может убрать эту глубину?
    /// <summary>
    ///     Максимальная глубина, на которую <see cref="СравнитьСписокЗначений" />
    ///     будет проходить по атрибутам элементов списков
    /// </summary>
    public static short ГлубинаСравненияСписка { get; set; } = 3;

    /// <summary>
    ///     Возвращает иерархию типов для класса с типом "<paramref name="тип" />", включая "<paramref name="тип" />"
    /// </summary>
    /// <param name="тип"><see cref="Type" /> объекта</param>
    /// <returns>
    ///     Перечисление типов, начиная с типа "<paramref name="тип" />" и заканчивая базовым типом
    /// </returns>
    [Pure]
    public static IEnumerable<Type> ДайВсюИерахиюТипов(Type тип) {
        var текущий = тип;
        while (текущий is not null) {
            yield return текущий;
            текущий = текущий.BaseType;
        }
    }

    /// <summary>
    ///     Находит ближайший в иерархии общий супертип двух типов
    /// </summary>
    /// <param name="тип1"><see cref="Type" /> первого класса</param>
    /// <param name="тип2"><see cref="Type" /> второго класса</param>
    /// <returns>
    ///     <see cref="Type" /> первого общего суперкласса
    /// </returns>
    [Pure]
    public static Result<Type> НайдиПервогоОбщегоПредка(Type тип1, Type тип2) {
        var контракт = Result.Merge(
            Result.FailIf(тип1 is null, new АргументNullОшибка(тип1)),
            Result.FailIf(тип2 is null, new АргументNullОшибка(тип2))
        );

        if (контракт.IsFailed) return контракт;

        var иерархия1 = ДайВсюИерахиюТипов(тип1!);
        var иерархия2 = ДайВсюИерахиюТипов(тип2!);

        var предок = иерархия1.FirstOrDefault(т1 => иерархия2.Contains(т1));
        return предок is null ? Result.Fail(new НеНайденОшибка()) : Result.Ok(предок);
    }

    /// <summary>
    ///     Возвращает считыватели всех (кроме унаследованных) публичных атрибутов экземпляра
    /// </summary>
    /// <param name="тип"><see cref="Type" /> класса, у которого необходимо прочитать атрибуты</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" /> считывателей (<see cref="ЧитательАтрибута" />) значений,
    ///     из атрибутов экземпляра тип "<paramref name="тип" />"
    /// </returns>
    /// <remarks>Атрибуты - это поля и свойства </remarks>
    [Pure]
    public static Result<IEnumerable<ЧитательАтрибута>> ДайСчитывателиПубличныхАтрибутовЭкземпляра(Type тип) {
        if (тип is null) return Result.Fail(new АргументNullОшибка(тип));

        const BindingFlags флагРефлексии = BindingFlags.Default //Сброс флагов
                                           | BindingFlags.Instance //Атрибуты экземпляра   
                                           | BindingFlags.Public //Публичные атрибуты
                                           | BindingFlags.DeclaredOnly; //Атрибуты без наследования

        var поляВсе = тип.GetFields(флагРефлексии)
            .Select(поле => new ЧитательАтрибута(поле.Name,
                поле.GetCustomAttributes(true).OfType<Attribute>(),
                поле.GetValue));

        var свойстваВсе = тип.GetProperties(флагРефлексии)
            .Where(с => с.CanRead)
            .Select(свойство => (метод: свойство.GetMethod, свойство))
            .Where(arg => arg.метод is not null)
            .Select(arg => new ЧитательАтрибута(arg.свойство.Name,
                arg.свойство.GetCustomAttributes(true).OfType<Attribute>(),
                arg.свойство.GetValue));

        return Result.Ok(поляВсе.Concat(свойстваВсе));
    }

    /// <summary>
    ///     Сравнивает объект '<paramref name="x" />' с объектом '<paramref name="y" />' на основе ссылок
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    [Pure]
    public static short? СравнитьПоСсылке(object? x, object? y) {
        if (ReferenceEquals(x, y)) return 0;
        return (x, y) switch {
            (null, null) => 0,
            (null, not null) => -1,
            (not null, null) => 1,
            _ => null
        };
    }

    /// <summary>
    ///     Сравнивает объект '<paramref name="x" />' с объектом '<paramref name="y" />' на основе типов объектов
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <returns>
    ///     -1 - x объект является базовым и поэтому меньше y;
    ///     0 - типы x и y равны;
    ///     1 - x объект является производным и поэтому больше y;
    ///     -1 или +1 - если объекты не находятся в непосредственном родстве, тогда сравнение производится по их имени.
    /// </returns>
    /// <remarks>
    ///     Экземпляры двух различных типов будут сравниваться по строковому представлению их имени
    /// </remarks>
    [Pure]
    public static Result<short> СравнитьПоТипу(object x, object y) {
        var контракт = Result.Merge(
            Result.FailIf(x is null, new АргументNullОшибка(x)),
            Result.FailIf(y is null, new АргументNullОшибка(y))
        );

        if (контракт.IsFailed) return контракт;

        return (x!.GetType(), y!.GetType()) switch {
            var (xt, yt) when xt == yt => Result.Ok((short)0),
            var (xt, yt) when yt.IsSubclassOf(xt) => Result.Ok((short)-1),
            var (xt, yt) when xt.IsSubclassOf(yt) => Result.Ok((short)1),
            _ => Result.Ok(СтандартизоватьРезультатСравнения(
                string.CompareOrdinal(x.GetType().FullName, y.GetType().FullName)))
        };
    }

    /// <summary>
    ///     Стандартизует результат функции сравнения
    /// </summary>
    /// <param name="вход">
    ///     &lt; 0 - объект меньше другого;
    ///     == 0 - объект равен другому;
    ///     &gt; 1 - объект больше другого.
    /// </param>
    /// <returns>
    ///     -1 - объект меньше другого;
    ///     0 - объект равен другому;
    ///     1 - объект больше другого.
    /// </returns>
    [Pure]
    public static short СтандартизоватьРезультатСравнения(int вход) {
        return вход switch {
            > 0 => 1,
            < 0 => -1,
            0 => 0
        };
    }

    /// <summary>
    ///     Пытается сравнить две строки объекта на основе  <see cref="string.Compare(string?,int,string?,int,int)" />
    /// </summary>
    /// <param name="x">Сравниваемая строка № 1</param>
    /// <param name="y">Сравниваемая строка № 2</param>
    /// <param name="типСравненияСтрок">Способ сравнения строк</param>
    /// <returns>
    ///     -1 - строка <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - строка <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - строка <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    [Pure]
    private static short ПопробуйСравнитьЗначениеСтрок(string x, string y,
        StringComparison типСравненияСтрок = StringComparison.CurrentCultureIgnoreCase) {
        return (short)string.Compare(x, y, типСравненияСтрок);
    }

    /// <summary>
    ///     Пытается сравнить два объекта, поддерживающих операторы сравнения
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    private static short? ПопробуйСравнитьЗначениеDynamic(dynamic x, dynamic y) {
        try {
            return (x, y) switch {
                var (a, b) when a < b => -1,
                var (a, b) when a > b => 1,
                var (a, b) when a == b => 0,
                _ => null
            };
        }
        catch { // Игнорируем все исключения с невозможностью сравнения}
        }

        return null;
    }

    /// <summary>
    ///     Пытается сравнить два объекта одного типа
    ///     на основе реализованного ими интерфейса <see cref="IComparable{T}" /> или <see cref="IComparable" />
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <typeparam name="Т">Тип объектов одинаковый</typeparam>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    [Pure]
    private static short? ПопробуйСравнитьЗначение<Т>(Т x, Т y) {
        return x switch {
            IComparable<Т> xСравн => СтандартизоватьРезультатСравнения((short)xСравн.CompareTo(y)),
            IComparable xСравн1 => СтандартизоватьРезультатСравнения((short)xСравн1.CompareTo(y)),
            _ => null
        };
    }

    /// <summary>
    ///     Пытается сравнить два объекта разных типов
    ///     на основе реализованных ими интерфейсов <see cref="IComparable{T}" /> или
    ///     <see cref="IComparable" />
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <typeparam name="Т1">тип первого объекта</typeparam>
    /// <typeparam name="Т2">тип второго объекта</typeparam>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    [Pure]
    private static short? ПопробуйСравнитьЗначение<Т1, Т2>(Т1 x, Т2 y) {
        try {
            if (x is IComparable<Т1> xСравн) {
                var yТ1 = (Т1)Convert.ChangeType(y, x.GetType());
                return СтандартизоватьРезультатСравнения((short)xСравн.CompareTo(yТ1));
            }
        }
        catch { // Игнорируем все исключения с невозможностью сравнения}
        }

        try {
            if (x is IComparable xСравн) {
                var yТ1 = (Т1)Convert.ChangeType(y, x.GetType());
                return СтандартизоватьРезультатСравнения((short)xСравн.CompareTo(yТ1));
            }
        }
        catch { // Игнорируем все исключения с невозможностью сравнения}
        }

        return null;
    }

    /// <summary>
    ///     Сравнивает два <see cref="IEnumerable{T}" /> атрибутов объекта на основе их значений
    /// </summary>
    /// <param name="x"><see cref="IEnumerable{T}" /> атрибутов сравниваемого объекта № 1</param>
    /// <param name="y"><see cref="IEnumerable{T}" /> атрибутов сравниваемого объекта № 2</param>
    /// <param name="глубина">Текущая глубина рекурсии, задается только самим методом</param>
    /// <param name="типСравненияСтрок">Способ сравнения строк</param>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    /// <remarks>
    ///     Максимальная глубина на которую метод будет проходить по атрибутам списков установлена
    ///     <see cref="ГлубинаСравненияСписка" />
    /// </remarks>
    public static Result<short> СравнитьСписокЗначений(IEnumerable<object?> x, IEnumerable<object?> y,
        short глубина = 0, StringComparison типСравненияСтрок = StringComparison.CurrentCultureIgnoreCase) {
        // Сравнение самих списков по ссылке
        if (СравнитьПоСсылке(x, y) is { } сравн) return Result.Ok(сравн);

        // Сравнение значений, содержащихся в списках
        var xy = x.Zip(y);
        foreach (var (first, second) in xy) {
            { // Попарное сравнение по ссылке
                var сравнСсылки = СравнитьПоСсылке(first, second);
                if (сравнСсылки == 0) continue;
                if (сравнСсылки is { } ссыл) return Result.Ok(ссыл);
            }

            { //Попарное сравнение элементов, поддерживающих операторы сравнения
                short? сравнДин = ПопробуйСравнитьЗначениеDynamic((dynamic)first!, (dynamic)second!);
                if (сравнДин == 0) continue;
                if (сравнДин is { } дин) return Result.Ok(дин);
            }

            switch (first, second) {
                //Попарное сравнение единичных элементов строк
                case (string xStr, string yStr): {
                    var сравнСтр = ПопробуйСравнитьЗначениеСтрок(xStr, yStr, типСравненияСтрок);
                    if (сравнСтр == 0) continue;
                    return Result.Ok(сравнСтр);
                }
                // Попарное сравнение внутренних списков
                case (IEnumerable<object?> п1, IEnumerable<object?> п2): {
                    short? сравнСпис = null;
                    if (глубина + 1 <= ГлубинаСравненияСписка)
                        сравнСпис = СравнитьСписокЗначений(п1, п2, (short)(глубина + 1), типСравненияСтрок)
                            .ValueOrDefault;

                    if (сравнСпис == 0) continue;
                    if (сравнСпис is { } спис) return Result.Ok(спис);
                    break;
                }
                //Попарное сравнение единичных элементов одинакового типа
                case ({ } f, { } s) when f.GetType() == s.GetType(): {
                    var сравнЗнач = ПопробуйСравнитьЗначение<object>(f, s);
                    if (сравнЗнач == 0) continue;
                    if (сравнЗнач is { } сравнЗначNN) return Result.Ok(сравнЗначNN);
                    break;
                }
                //Попарное сравнение элементов разных типов
                case ({ } f, { } s) when f.GetType() != s.GetType(): {
                    // Приведение типов могло создать ошибку, поэтому нужно убедиться,
                    // что сравнение одинаково в 2 направлениях или не поддерживается в одном из направлений 
                    var сравнЗначX = ПопробуйСравнитьЗначение<object, object>(f, s);
                    var сравнЗначY = ПопробуйСравнитьЗначение<object, object>(s, f);
                    short? оценкаСравнРазныхТипов = (сравнЗначX, сравнЗначY) switch {
                        ({ } xx, null) => xx,
                        (null, { } yy) => yy,
                        var (xx, yy) when (xx == yy && xx != 0) ||
                                          (xx == 0 && xx != yy) ||
                                          (yy == 0 && xx != yy) => null,
                        (var xx and 0, var yy) when xx == yy => 0,
                        var (xx, yy) when xx != yy => xx,
                        _ => null
                    };
                    if (оценкаСравнРазныхТипов == 0) continue;
                    if (оценкаСравнРазныхТипов is { } осртNN) return Result.Ok(осртNN);
                    break;
                }
            }

            { //Остается только сравнить элементы как строки
                var сравнЗначСтр = ПопробуйСравнитьЗначениеСтрок(x.ToString(), y.ToString(), типСравненияСтрок);
                if (сравнЗначСтр == 0) continue;
                return Result.Ok(сравнЗначСтр);
            }
        }

        // Дополнительное сравнение на длину списка
        return (x.Count(), y.Count()) switch {
            var (д1, д2) when д1 < д2 => Result.Ok((short)-1),
            var (д1, д2) when д2 < д1 => Result.Ok((short)1),
            _ => Result.Ok((short)0)
        };
    }

    /// <summary>
    ///     Сравнивает два объекта на основе <see cref="IEnumerable{T}" /> считывателей атрибутов объектов
    /// </summary>
    /// <param name="x">Сравниваемый объект № 1</param>
    /// <param name="y">Сравниваемый объект № 2</param>
    /// <param name="считыватели">
    ///     <see cref="IEnumerable{T}" /> считыватель <see cref="ЧитательАтрибута" />> атрибутов
    ///     объектов
    /// </param>
    /// <param name="типСравненияСтрок">Способ сравнения строк</param>
    /// <returns>
    ///     -1 - объект <paramref name="x" /> меньше <paramref name="y" />
    ///     0 - объект <paramref name="x" /> равен  <paramref name="y" />
    ///     1 - объект <paramref name="x" /> больше <paramref name="y" />
    /// </returns>
    public static Result<short> СравнитьОбъектыПоЗначениямАтрибутов(object x, object y,
        IEnumerable<ЧитательАтрибута> считыватели,
        StringComparison типСравненияСтрок = StringComparison.CurrentCultureIgnoreCase) {
        var контракт = Result.Merge(
            Result.FailIf(x is null, new АргументNullОшибка(x)),
            Result.FailIf(y is null, new АргументNullОшибка(y)),
            Result.FailIf(считыватели is null, new АргументNullОшибка(считыватели))
        );

        if (контракт.IsFailed)
            return контракт;

        var считывателиМассив = считыватели!.ToArray();

        var xАтрибб = считывателиМассив.Select(считыватель => считыватель.ДайЗначение(x!));
        var yАтрибб = считывателиМассив.Select(считыватель => считыватель.ДайЗначение(y!));

        return Result.Ok(СравнитьСписокЗначений(xАтрибб, yАтрибб, типСравненияСтрок: типСравненияСтрок).ValueOrDefault);
    }

    /// <summary>
    ///     Рассчитывает хэш-код объекта на основе значения его атрибутов
    /// </summary>
    /// <param name="x">Объект, у которого необходимо вычислить хэш-код</param>
    /// <param name="считыватели">
    ///     <see cref="IEnumerable{T}" /> <see cref="ЧитательАтрибута" />
    /// </param>
    /// <param name="типСравненияСтрок">Способ сравнения строк</param>
    /// <returns>Хэш-код</returns>
    [Pure]
    public static int РассчитайХэшКод(object? x, IEnumerable<ЧитательАтрибута> считыватели,
        StringComparison типСравненияСтрок = StringComparison.CurrentCultureIgnoreCase) {
        if (x is null) return 0;

        var хэш = new HashCode();
        хэш.Add(x.GetType().FullName!.GetHashCode());
        foreach (var считыватель in считыватели) {
            var значение = считыватель.ДайЗначение(x);

            switch (значение) {
                case null:
                    хэш.Add(0);
                    break;
                case string стр:
                    хэш.Add(стр.GetHashCode(типСравненияСтрок));
                    break;
                case IEnumerable<object?> переч:
                    переч.ForEach(элем => хэш.Add(элем?.GetHashCode() ?? 0));
                    break;
                default:
                    хэш.Add(значение.GetHashCode());
                    break;
            }
        }

        return хэш.ToHashCode();
    }
}