using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class KdfParams
    {
        // ReSharper disable once StringLiteralTypo
        [JsonPropertyName("dklen")]
        public int Dklen { get; set; }

        [JsonPropertyName("salt")]
        public string Salt { get; set; }
    }
}