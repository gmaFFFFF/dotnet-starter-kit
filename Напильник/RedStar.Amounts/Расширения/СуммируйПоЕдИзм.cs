namespace RedStar.Amounts;

public static class AmountРасширения {
    /// <summary>
    ///     Суммирует совместимые Amounts и возвращает последовательность несовместимых Amounts
    /// </summary>
    /// <param name="ист">IEnumerable Amounts для вычисления суммы</param>
    /// <returns>
    ///     IDictionary, где
    ///     <br />значение - сумма совместимых Amounts,
    ///     <br /> ключ - наименьшая ед. изм
    /// </returns>
    /// <remarks>
    ///     На примере кошелька. У вас есть кошелёк с несколькими отделениями, в которых хранятся разные валюты.
    ///     Метод принимает на вход деньги в разных валютах и считает сколько денег будет находиться в каждом отделении
    /// </remarks>
    public static IDictionary<Unit, Amount> СуммируйПоЕдИзм(this IEnumerable<Amount> ист) {
        // Не могу придумать абстрактные названия, поэтому представим,
        // что у нас есть кошелёк с разными валютами в разных отделениях...
        Dictionary<Unit, List<Amount>> кошелёк = new();
        foreach (var монета in ист) {
            var отделение = кошелёк.Keys
                .SingleOrDefault(отд => отд.IsCompatibleTo(монета.Unit));
            if (отделение is null)
                кошелёк[монета.Unit] = new List<Amount> { монета };
            else
                кошелёк[отделение].Add(монета);
        }

        Dictionary<Unit, Amount> сумма = new();
        foreach (var отделение in кошелёк.Keys) {
            кошелёк[отделение].Sort((x, y) => ((IComparable<Unit>)x.Unit).CompareTo(y.Unit));
            сумма[кошелёк[отделение].First().Unit] = кошелёк[отделение].Sum();
        }

        return сумма;
    }
}