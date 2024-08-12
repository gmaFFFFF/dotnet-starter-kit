namespace RedStar.Amounts.Тесты;

public class AmountТесты {
    public static UnitType деньгиРус = new("деньгиРус");
    public static UnitType деньгиUsa = new("деньгиUsa");
    public static Unit рубль = new("рубль", "₽", деньгиРус);
    public static Unit копейка = new("копейка", "коп.", .01 * рубль);
    public static Unit доллар = new("доллар", "$", деньгиUsa);
    public static Unit цент = new("цент", "c.", .01 * доллар);

    static AmountТесты() {
        UnitManager.RegisterUnits(typeof(AmountТесты));
    }

    [Fact]
    public void МожноСуммироватьРазныеЕдиницы() {
        Amount[] кошелёк = {
            Amount.Parse("1 ₽"),
            new(1000, "копейка"),
            Amount.Parse("1000 $"),
            new(1, "цент"),
            new(5, "рубль")
        };
        Dictionary<Unit, Amount> теор = new() {
            [копейка] = new Amount(1600, "копейка"),
            [цент] = new Amount(100001, "цент")
        };

        кошелёк.СуммируйПоЕдИзм().Should().BeEquivalentTo(теор);
    }
}