using System.Text.Json;
using NMoneys.Extensions;

namespace NMoneys.Serialization.SystemTextJson.Тесты;

public class NMoneysJsonПреобразовательТесты {
    [Fact]
    public void УмеетСохранятьЗагружатьJson() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NMoneysJsonПреобразователь());

        var деньги = 123.45m.Rub();
        var json = JsonSerializer.Serialize(деньги, деньги.GetType(), options);
        var деньги2 = (Money)JsonSerializer.Deserialize(json, деньги.GetType(), options);
        деньги.Should().Be(деньги2);
    }

    [Fact]
    public void ЗнаетСтандартныйJson() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new NMoneysJsonПреобразователь());
        options.PropertyNameCaseInsensitive = true;

        var деньги = 123.45m.Rub();
        var json = @"{""amount"" : 123.45, ""currency"" : ""RUB""}";
        var деньги2 = (Money)JsonSerializer.Deserialize(json, деньги.GetType(), options);
        деньги.Should().Be(деньги2);
    }
}