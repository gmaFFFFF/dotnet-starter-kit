namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

public class ПсевдоЗначимыйТипТесты {
    protected readonly Fixture ТестОснастка = new();

    [Fact]
    public void ПроверкаНаРавенствоРеализована() {
        var утверждение = ТестОснастка.Create<EqualityComparerAssertion>();
        утверждение.Verify(typeof(ПсевдоЗначимыйТип));
    }

    [Fact]
    public void ПроизводныйОбъектНеРавенБазовому() {
        var производБезАтрибутов = ТестОснастка
            .Build<СпециальныйАвтомобиль>()
            .OmitAutoProperties()
            .Create();
        var базовыйКласс = ТестОснастка
            .Build<Автомобиль>()
            .OmitAutoProperties()
            .Create();

        производБезАтрибутов.Equals(базовыйКласс).Should().BeFalse();
        базовыйКласс.Equals(производБезАтрибутов).Should().BeFalse();
    }

    [Fact]
    public void ДваРазныхОбъектаРавныЕслиРавныАтрибуты() {
        ТестОснастка.Register(() => new Автомобиль
            { Вместимость = 4, Кузов = "купе", Тяга = ТипТяги.Двигатель });
        var авто1 = ТестОснастка.Create<Автомобиль>();
        var авто2 = ТестОснастка.Create<Автомобиль>();

        авто1.Equals(авто2).Should().BeTrue();
        авто1.GetHashCode().Equals(авто2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void ДваРазныхОбъектаНеРавныЕслиНеРавныАтрибуты() {
        var авто1 = ТестОснастка.Create<Автомобиль>();
        var авто2 = ТестОснастка.Create<Автомобиль>();

        авто1.Equals(авто2).Should().BeFalse();
        авто1.GetHashCode().Equals(авто2.GetHashCode()).Should().BeFalse();
    }

    [Fact]
    public void ПриСравненииНеУчитываетсяРегистрСимволов() {
        var авто1 = new Автомобиль { Вместимость = 4, Кузов = "купе", Тяга = ТипТяги.Двигатель };
        var авто2 = new Автомобиль { Вместимость = 4, Кузов = "купе".ToUpper(), Тяга = ТипТяги.Двигатель };

        авто1.Equals(авто2).Should().BeTrue();
        авто1.GetHashCode().Equals(авто2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void ПростыеСущностиРавныЕслиРавныАтрибуты() {
        var епочта1 = new Email("a@example.com");
        var епочта2 = new Email("A@example.com");

        епочта1.Equals(епочта2).Should().BeTrue();
        епочта1.GetHashCode().Equals(епочта2.GetHashCode()).Should().BeTrue();
    }

    [Fact]
    public void ПростыеСущностиНеРавныЕслиНеРавныАтрибуты() {
        var епочта1 = ТестОснастка.Create<Email>();
        var епочта2 = ТестОснастка.Create<Email>();

        епочта1.Equals(епочта2).Should().BeFalse();
        епочта1.GetHashCode().Equals(епочта2.GetHashCode()).Should().BeFalse();
    }

    [Fact]
    public void ВозможноПереопределитьСпособСравненияЧленов() {
        var знач1 = new СпецифическоеСравнение("Тест");
        var знач2 = new СпецифическоеСравнение("тест");

        знач1.Equals(знач2).Should().BeFalse();
        знач1.GetHashCode().Equals(знач2.GetHashCode()).Should().BeFalse();
    }

    [Fact]
    public void ОбъектыРазныхТиповНеРавны() {
        var епочта1 = new Email("a@example.com");
        var епочта2 = new Email2("a@example.com");

        епочта1.Equals(епочта2).Should().BeFalse();
    }

    [Fact]
    public void ОбъектыСРазнымиСпискамиНеРавны() {
        var г1 = new Автоколонна(ТестовыйНабор.Атрибуты.OfType<Транспорт>());
        var г2 = new Автоколонна(ТестовыйНабор.ПеремешанныеАтрибуты.OfType<Транспорт>().SkipLast(1));
        г1.Should().NotBe(г2);
    }

    [Fact]
    public void НеСравниватьПоAttributeРаботает() {
        ИгнорируемоеСвойство об1 = new() { СвойствоИгнор = 2, СвойствоСравниваемое = 1 },
            об2 = new() { СвойствоИгнор = 1, СвойствоСравниваемое = 2 };
        (об1 == об2).Should().BeFalse();
        ((IComparable)об1).CompareTo(об2).Should().BeNegative();

        об1.СвойствоСравниваемое = об2.СвойствоСравниваемое;
        (об1 == об2).Should().BeTrue();
    }
}