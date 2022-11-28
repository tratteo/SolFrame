
using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class Pbkdf2Params : KdfParams
    {
        [JsonPropertyName("c")]
        public int Count { get; set; }

        [JsonPropertyName("prf")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Prf { get; set; }
    }
}