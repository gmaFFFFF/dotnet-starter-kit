namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

public class ОтправительСобытия : СущностьЛегат {
    public void УстановитьИд(Guid ид) {
        Ид = ид;
    }

    public class ТестовоеСобытие : СобытиеМодели<Guid> {
        public ТестовоеСобытие(ИСущность<Guid> отправитель) : base(отправитель) { }
    }
}