using System.Text.Json.Serialization;

namespace Domain.Enums
{
    public enum OrderSide
    {
        [JsonStringEnumMemberName("C")]
        C,

        [JsonStringEnumMemberName("V")]
        V
    }
}
