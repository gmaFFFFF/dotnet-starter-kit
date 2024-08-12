namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

public class СущностьТесты {
    protected readonly Fixture ТестОснастка = new();

    [Fact]
    public void ПроверкаНаРавенствоРеализована() {
        var утверждение = ТестОснастка.Create<EqualityComparerAssertion>();
        утверждение.Verify(typeof(Сущность<>));
    }

    [Fact]
    public void ПроизводнаяСущностьНеРавнаБазовой() {
        var сущ1 = new Гараж(1);
        var сущ2 = new ГаражСпец(1);

        var сравн1 = сущ1.Equals(сущ2);
        var сравн2 = сущ1 == сущ2;

        сравн1.Should().BeFalse();
        сравн2.Should().BeFalse();
    }

    [Fact]
    public void СущностиСРавнымИдРавны() {
        var сущ1 = new Гараж(1);
        var сущ2 = new Гараж(1);

        var сравн1 = сущ1.Equals(сущ2);
        var сравн2 = сущ1 == сущ2;

        сравн1.Should().BeTrue();
        сравн2.Should().BeTrue();
    }

    [Fact]
    public void СущностиСРазнымиИдНеРавны() {
        var сущ1 = new Гараж(1);
        var сущ2 = new Гараж(2);

        var сравн1 = сущ1.Equals(сущ2);
        var сравн2 = сущ1 == сущ2;

        сравн1.Should().BeFalse();
        сравн2.Should().BeFalse();
    }

    [Fact]
    public void СущностиСИдПоУмолчаниюНеРавны() {
        var сущ1 = new Гараж(default(int));
        var сущ2 = new Гараж(default(int));

        var сравн1 = сущ1.Equals(сущ2);
        var сравн2 = сущ1 == сущ2;

        сравн1.Should().BeFalse();
        сравн2.Should().BeFalse();
    }

    [Fact]
    public void СущностьСПользовательскимИдентификаторомПоддерживается() {
        var guid = new Guid();
        var сущ1 = new СущностьСПользовательскимИд(new МойИдентификатор("1", guid));
        var сущ2 = new СущностьСПользовательскимИд(new МойИдентификатор("1", guid));

        var сравн1 = сущ1.Equals(сущ2);
        var сравн2 = сущ1 == сущ2;

        сравн1.Should().BeTrue();
        сравн2.Should().BeTrue();
    }

    [Fact]
    public void СравнениеСNull() {
        Гараж? сущ1 = new(1);
        Гараж? сущ2 = null;
        Гараж? сущ3 = null;

        (сущ1 == null).Should().BeFalse();
        (сущ2 == null).Should().BeTrue();
        сущ1?.Equals(null).Should().BeFalse();
        (сущ2 == сущ3).Should().BeTrue();
    }

    [Fact]
    public void СущностиСИдРавнымNullНеРавны() {
        var сущ1 = new СущностьСПользовательскимИд(null!);
        var сущ2 = new СущностьСПользовательскимИд(null!);

        (сущ1 == сущ2).Should().BeFalse();
    }
}