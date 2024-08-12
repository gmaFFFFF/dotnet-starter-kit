namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

internal enum ТипТяги {
    Ручная,
    Животные,
    Двигатель
}

internal enum ТипХода {
    Железнодорожный,
    Гусеничный,
    Колесный
}

internal abstract class Транспорт : ПсевдоЗначимыйТип {
    public ТипТяги? Тяга { get; init; }
    public string? Марка { get; init; }
    public int? Вместимость { get; init; }
}

internal class Велосипед : Транспорт {
    public Велосипед(Велосипед другой) {
        ЧислоКолес = другой.ЧислоКолес;
        Вместимость = другой.Вместимость;
        Марка = другой.Марка;
        Тяга = другой.Тяга;
    }

    public Велосипед() {
        Тяга = ТипТяги.Ручная;
    }

    public byte? ЧислоКолес { get; init; }

    public void Deconstruct(out byte? числоКолес, out int? вместимость, out string? марка, out ТипТяги? тяга) {
        числоКолес = ЧислоКолес;
        вместимость = Вместимость;
        марка = Марка;
        тяга = Тяга;
    }
}

internal abstract class Машина : Транспорт {
    protected Машина() {
        Тяга = ТипТяги.Двигатель;
    }

    public ТипХода? Ход { get; init; }
    public string? Модель { get; init; }
    public int? МаксСкорость { get; init; }
}

internal class Автомобиль : Машина {
    public Автомобиль() {
        Ход = ТипХода.Колесный;
    }

    public string? Кузов { get; init; }
}

internal class Мотоцикл : Машина {
    public Мотоцикл() {
        Ход = ТипХода.Колесный;
    }

    public bool? СКоляскойЛи { get; init; }
}

internal class Поезд : Машина {
    public Поезд() {
        Ход = ТипХода.Железнодорожный;
    }

    public int? ЧислоВагонов { get; init; }
}

internal class СпециальныйАвтомобиль : Автомобиль { }

internal class Танк : СпециальныйАвтомобиль, IEnumerable {
    public float Масса;

    public Танк() {
        Ход = ТипХода.Гусеничный;
    }

    public int? МаксВыстрелов { get; init; }

    public IEnumerator GetEnumerator() {
        throw new NotImplementedException();
    }
}

internal class Email : ПсевдоЗначимыйТипПростой<string> {
    public Email(string? значение) : base(значение) { }
}

internal class Email2 : ПсевдоЗначимыйТипПростой<string> {
    public Email2(string? значение) : base(значение) { }
}

internal class СпецифическоеСравнение : ПсевдоЗначимыйТипПростой<string> {
    public СпецифическоеСравнение(string? значение) : base(значение) {
        _компонентыСравненияПользовательские = Encoding.UTF8.GetBytes(значение).Cast<object>();
    }
}

internal class Автоколонна : ПсевдоЗначимыйТипПростой<IEnumerable<Транспорт>> {
    public Автоколонна(IEnumerable<Транспорт>? значение) : base(значение) { }
}

internal class ИгнорируемоеСвойство : ПсевдоЗначимыйТип {
    [НеСравниватьПо] public int СвойствоИгнор { get; set; }

    public int СвойствоСравниваемое { get; set; }
}

internal static class ТестовыйНабор {
    public static List<ПсевдоЗначимыйТип> Атрибуты = new();

    private static readonly Random Rng = new((int)DateTime.Now.Ticks & 0x0000FFFF);

    static ТестовыйНабор() {
        Атрибуты.AddRange(new List<ПсевдоЗначимыйТип> {
            // Тяга.Ручной
            new Велосипед { Марка = "1Дружок", Вместимость = 1, ЧислоКолес = 2 },
            new Велосипед { Марка = "1Дружок", Вместимость = 1, ЧислоКолес = 4 },
            new Велосипед { Марка = "9Юпитер", Вместимость = 1, ЧислоКолес = 2 },
            // Тяга.Двигатель
            new Автомобиль { Марка = "0Ferrari", Модель = "2F1", Вместимость = 1, МаксСкорость = 350 },
            new Автомобиль { Марка = "2Жигули", Модель = "2101", Вместимость = 4, МаксСкорость = 110 },
            new Автомобиль { Марка = "2Жигули", Модель = "2140", Вместимость = 4, МаксСкорость = 120 },
            new Мотоцикл { Марка = "3Иж", Модель = "3Планета", Вместимость = 2, МаксСкорость = 80 },
            new Мотоцикл
                { Марка = "3Иж", Модель = "3Планета", Вместимость = 2, МаксСкорость = 80, СКоляскойЛи = false },
            new Мотоцикл { Марка = "3Иж", Модель = "3Планета", Вместимость = 2, МаксСкорость = 80, СКоляскойЛи = true },
            new Поезд { Марка = "4Секрет" },
            new Танк { Марка = "4Секрет" },
            new Автомобиль { Марка = "4Секрет" },
            new Мотоцикл { Марка = "4Секрет" }
        });
    }

    public static IList<ПсевдоЗначимыйТип> ПеремешанныеАтрибуты =>
        Атрибуты.Select(э => (элемент: э, индекс: Rng.Next()))
            .OrderBy(tuple => tuple.индекс)
            .Select(tuple => tuple.элемент)
            .Reverse()
            .ToArray();
}