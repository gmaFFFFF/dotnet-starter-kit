using Microsoft.EntityFrameworkCore;

namespace gmafffff.СтартовыйНабор.DDD.Инфраструктура.Данные.Тесты;

internal class ТестовыйРепоЗаказ : СущностьХранилищеEF<Заказ, Guid> {
    public ТестовыйРепоЗаказ(DbContext контекст) : base(контекст) { }
}

internal class ТестовыйРепоКлиент : СущностьХранилищеEF<Клиент, Guid> {
    public ТестовыйРепоКлиент(DbContext контекст) : base(контекст) { }
}

internal class ТестовыйРепоТовар : СущностьХранилищеEF<Товар, Guid> {
    public ТестовыйРепоТовар(DbContext контекст) : base(контекст) { }
}

internal class ТестовыйРепоПоставщик : СущностьХранилищеEF<Поставщик, Guid> {
    public ТестовыйРепоПоставщик(DbContext контекст) : base(контекст) { }
}