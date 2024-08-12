namespace AutoFixture;

public static partial class AutoFixtureРасширения {
    /// <summary>
    ///     Закрепляет переданные аргументы конструктора для создания экземпляра класса
    /// </summary>
    /// <remarks> Источник https://stackoverflow.com/a/28627595</remarks>
    /// <typeparam name="Т">Тестируемый класс</typeparam>
    /// <example>
    ///     var f = new Fixture(); <br />
    ///     f.ЗакрепиАргументыКонструктораДля&lt;ПользоватТип&gt;  (new { ид = 15, родитИд = (long?)33 });
    /// </example>
    public static void ЗакрепиАргументыКонструктораДля<Т>(this IFixture тестОснастка, object параметры) {
        var построитель = new ПостроительСПользовательскимКонструктором<Т>();
        foreach (var свойство in параметры.GetType().GetProperties())
            построитель.ДобавьПараметр(свойство.Name, свойство.GetValue(параметры));

        тестОснастка.Customize<Т>(_ => построитель);
    }
}