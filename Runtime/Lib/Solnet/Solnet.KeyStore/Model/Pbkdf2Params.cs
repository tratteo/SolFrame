using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class Pbkdf2Params : KdfParams
    {
        [JsonPropertyName("c")]
        public int Count { get; set; }

        [JsonPropertyName("prf")]
        public string Prf { get; set; }
    }
}