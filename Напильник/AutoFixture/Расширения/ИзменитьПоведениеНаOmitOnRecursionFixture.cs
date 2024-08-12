namespace AutoFixture;

public static partial class AutoFixtureРасширения {
    /// <summary>
    ///     Изменяет поведение с <see cref="ThrowingRecursionBehavior" /> на <see cref="OmitOnRecursionBehavior" />
    /// </summary>
    /// <returns></returns>
    public static IFixture ИзменитьПоведениеНаOmitOnRecursionFixture(this IFixture тестОснастка) {
        тестОснастка
            .Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => тестОснастка.Behaviors.Remove(b));
        тестОснастка.Behaviors.Add(new OmitOnRecursionBehavior());
        return тестОснастка;
    }
}