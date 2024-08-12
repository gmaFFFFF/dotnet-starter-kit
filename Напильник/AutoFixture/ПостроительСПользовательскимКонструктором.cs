using System.Globalization;
using AutoFixture.Kernel;

namespace AutoFixture;

/// <summary>
///     Создает запрошенный класс с помощью переданных параметров конструктора
/// </summary>
/// <remarks> Источник https://stackoverflow.com/a/28627595</remarks>
/// <typeparam name="Т">Тестируемый класс</typeparam>
internal sealed class ПостроительСПользовательскимКонструктором<Т> : ISpecimenBuilder {
    private readonly Dictionary<string, object?> _параметрыКонструктора = new();

    public object Create(object request, ISpecimenContext context) {
        var тип = typeof(Т);
        if (request is not SeededRequest sr || !sr.Request.Equals(тип))
            return new NoSpecimen();

        var конструктор = тип.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault();
        if (конструктор is null)
            return new NoSpecimen();

        return конструктор.Invoke(BindingFlags.CreateInstance, null, конструктор.GetParameters()
                .Select(параметр => _параметрыКонструктора.ContainsKey(параметр.Name)
                    ? _параметрыКонструктора[параметр.Name]
                    : context.Resolve(параметр.ParameterType)).ToArray(),
            CultureInfo.InvariantCulture);
    }

    public void ДобавьПараметр(string параметрИмя, object? значение) {
        _параметрыКонструктора.Add(параметрИмя, значение);
    }
}