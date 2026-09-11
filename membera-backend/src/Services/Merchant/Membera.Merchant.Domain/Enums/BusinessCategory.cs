using System.Text.Json.Serialization;

namespace Membera.Merchant.Domain.Enums;

// Serialized as its name (e.g. "Gym"), not the default numeric value, since
// this enum crosses the wire in both request bodies (create/update merchant)
// and response bodies (merchant profile, browse plans) and the frontend reads
// it as a string literal type.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BusinessCategory
{
    Restaurant,
    Cafe,
    Barbershop,
    BeautySalon,
    Gym,
    Other
}
