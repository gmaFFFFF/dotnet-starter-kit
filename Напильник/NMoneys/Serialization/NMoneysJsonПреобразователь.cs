using System.Text.Json;
using System.Text.Json.Serialization;

namespace NMoneys.Serialization.SystemTextJson;

public class NMoneysJsonПреобразователь : JsonConverter<Money> {
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Ожидается StartObject токен");

        decimal? amount = null;
        CurrencyIsoCode? currencyCode = null;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Money(amount.Value, currencyCode.Value);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Ожидается имя токена");

            var имяСвойства = reader.GetString();
            reader.Read();

            switch ((имяСвойства, options.PropertyNameCaseInsensitive, имяСвойства.ToUpper())) {
                case (nameof(Money.Amount), _, _):
                case (_, true, var и) when и == nameof(Money.Amount).ToUpper():
                    amount = reader.GetDecimal();
                    break;
                case (nameof(Currency), _, _):
                case (_, true, var и) when и == nameof(Currency).ToUpper():
                    currencyCode = Enum.Parse<CurrencyIsoCode>(reader.GetString());
                    break;
                default:
                    throw new JsonException("Неизвестное имя свойства");
            }
        }

        throw new JsonException("Ожидается EndObject токен");
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(Money.Amount), value.Amount);
        writer.WriteString(nameof(Currency), Enum.GetName(value.CurrencyCode));
        writer.WriteEndObject();
    }
}