namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

public class СобытиеМоделиТесты {
    [Fact]
    public void СобытиеСохраняетИд() {
        var отправитель = new ОтправительСобытия();
        var событие = new ОтправительСобытия.ТестовоеСобытие(отправитель);

        отправитель.ИзХранилищаЛи().Should().BeFalse();
        событие.ОтправительИд.Should().Be(Guid.Empty);

        // Отслеживаем отправителя и его ИД до тех пор пока объект не помещен в хранилище
        отправитель.УстановитьИд(Guid.NewGuid());
        отправитель.ИзХранилищаЛи().Should().BeTrue();
        событие.ОтправительИд.Should().Be(отправитель.Ид);

        // После помещения объекта в хранилище и запроса Ид связь разрывается
        отправитель.УстановитьИд(Guid.NewGuid());
        отправитель.ИзХранилищаЛи().Should().BeTrue();
        событие.ОтправительИд.Should().NotBe(отправитель.Ид);
    }
}