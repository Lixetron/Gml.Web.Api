using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gml.Web.Api.Core.Json;

/// <summary>
///     Терпимый к пустым значениям конвертер <see cref="TimeSpan"/>.
///     Дашборд при выключенной автоочистке Sentry присылает пустую строку,
///     на которой стандартный конвертер System.Text.Json роняет запрос в 400.
/// </summary>
public class LenientTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => TimeSpan.Zero,
            JsonTokenType.String => Parse(reader.GetString()),
            JsonTokenType.Number => TimeSpan.FromSeconds(reader.GetDouble()),
            _ => throw new JsonException(
                $"Неподдерживаемый формат TimeSpan: {reader.TokenType}. Ожидается строка вида \"hh:mm:ss\" или \"d.hh:mm:ss\"")
        };
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));
    }

    private static TimeSpan Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return TimeSpan.Zero;

        return TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new JsonException(
                $"Не удалось преобразовать \"{raw}\" в TimeSpan. Ожидается формат \"hh:mm:ss\" или \"d.hh:mm:ss\"");
    }
}
