namespace gmafffff.СтартовыйНабор.DDD.МодельПредметнойОбласти.Тесты;

public class СравнивательАтрибутовПоЗначениюТесты {
    /// <summary>
    ///     Несколько атрибутов <see cref="InlineDataAttribute" /> использованы чтобы несколько раз вызвать
    ///     генерацию случайных значений
    /// </summary>
    [Theory]
    [InlineData]
    [InlineData]
    [InlineData]
    [InlineData]
    [InlineData]
    [InlineData]
    [InlineData]
    [InlineData]
    public void АтрибутМожноСравнивать() {
        var сортированный = ТестовыйНабор.ПеремешанныеАтрибуты.ToArray();
        Assert.False(сортированный.SequenceEqual(ТестовыйНабор.Атрибуты));
        Array.Sort(сортированный);
        Assert.True(сортированный.SequenceEqual(ТестовыйНабор.Атрибуты));
    }
}