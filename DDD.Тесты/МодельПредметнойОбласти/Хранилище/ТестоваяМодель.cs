namespace gmafffff.СтартовыйНабор.DDD.Инфраструктура.Данные.Тесты;

public class Клиент : СущностьЛегат {
    protected Клиент() { }
    public Фио Фио { get; protected set; } = null!;
    public ICollection<Email>? Emailы { get; protected set; }

    public static Клиент СоздатьКлиента(Фио фио, params Email[] emailы) {
        return new Клиент { Фио = фио, Emailы = emailы.ToList() };
    }

    public void ОбновитьEmails(params Email[] emailы) {
        Emailы = emailы;
    }

    public void ДобавитьEmail(Email email) {
        (Emailы ??= new List<Email>()).Add(email);
        СобытияМоделиДобавить(new ДобавленEMailСобытие(this));
    }
}

public class Фио : ПсевдоЗначимыйТип {
    public Фио(string? фамилия = null, string? имя = null, string? отчество = null) {
        Фамилия = фамилия;
        Имя = имя;
        Отчество = отчество;
    }

    public string? Фамилия { get; init; }
    public string? Имя { get; init; }
    public string? Отчество { get; init; }
}

public class Email : ПсевдоЗначимыйТипПростой<string> {
    public Email(string значение) : base(значение) { }
}

public class Заказ : СущностьЛегат {
    private static int _генераторНомер;

    static Заказ() {
        ГенераторНомера = 0;
    }

    protected Заказ() { }

    public static int ГенераторНомера {
        get => ++_генераторНомер;
        set => _генераторНомер = value;
    }

    public Реквизиты Реквизиты { get; protected set; } = null!;
    public ICollection<СтрокаЗаказа>? СтрокиЗаказа { get; protected set; }
    public Клиент Клиент { get; protected set; } = null!;

    public static Заказ НовыйЗаказ(Клиент клиент) {
        return new Заказ {
            Реквизиты = new Реквизиты($"Зак-{DateTime.Today:yyyy-MM-d}-{ГенераторНомера}"),
            Клиент = клиент
        };
    }

    public void ДобавитьСтроку(СтрокаЗаказа строкаЗаказа) {
        (СтрокиЗаказа ??= new List<СтрокаЗаказа>()).Add(строкаЗаказа);
    }

    public void УдалитьСтроку(СтрокаЗаказа строкаЗаказа) {
        СтрокиЗаказа!.Remove(строкаЗаказа);
    }
}

public class Реквизиты : ПсевдоЗначимыйТип {
    public Реквизиты(string номер, DateTime? дата = null) {
        Номер = номер;
        Дата = дата ?? DateTime.Today;
    }

    public string Номер { get; protected set; }
    public DateTime? Дата { get; protected set; }
}

public class СтрокаЗаказа : ПсевдоЗначимыйТип {
    private СтрокаЗаказа(decimal цена, decimal количество) {
        Цена = цена;
        Количество = количество;
    }

    public СтрокаЗаказа(Товар товар, decimal цена, decimal количество) {
        Товар = товар;
        Цена = цена;
        Количество = количество;
    }

    public Товар Товар { get; protected set; } = null!;
    public decimal Цена { get; protected set; }
    public decimal Количество { get; protected set; }

    public decimal Стоимость {
        get => Количество * Цена;
        private set { /*Для ef core*/
        }
    }
}

public class Товар : СущностьЛегат {
    public Товар(string название, decimal цена, ТоварКатегория товарКатегория, Поставщик поставщик) {
        Название = название;
        Цена = цена;
        ТоварКатегория = товарКатегория;
        Поставщик = поставщик;
    }

    private Товар(string название, decimal цена) {
        Название = название;
        Цена = цена;
    }

    public string Название { get; protected set; }
    public decimal Цена { get; protected set; }
    public ТоварКатегория ТоварКатегория { get; protected set; } = null!;
    public Поставщик Поставщик { get; protected set; } = null!;
}

public class ТоварКатегория : Сущность<int> {
    public ТоварКатегория(string название, string? описание) {
        Название = название;
        Описание = описание;
    }

    public string Название { get; protected set; }
    public string? Описание { get; protected set; }
}

public class Поставщик : СущностьЛегат {
    public Поставщик(string название) {
        Название = название;
    }

    public string Название { get; protected set; }
    public ICollection<Email>? Emailы { get; protected set; }

    public void ОбновитьEmails(params Email[] emailы) {
        Emailы = emailы;
    }
}

public class ДобавленEMailСобытие : СобытиеМодели<Guid> {
    public ДобавленEMailСобытие(ИСущность<Guid> отправитель) : base(отправитель) { }
}