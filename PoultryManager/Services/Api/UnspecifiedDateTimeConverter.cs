using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PoultryManager.Services.Api;

/// <summary>
/// Les dates de l'application sont des jours « nus » (JJ/MM/AAAA, sans heure ni fuseau).
/// On les envoie et on les lit telles quelles (« 2026-10-23T00:00:00 »), sans décalage de fuseau horaire,
/// pour qu'un jour saisi à 00:00 ne devienne jamais la veille côté serveur.
/// </summary>
public sealed class UnspecifiedDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString() ?? string.Empty;
        var value = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
