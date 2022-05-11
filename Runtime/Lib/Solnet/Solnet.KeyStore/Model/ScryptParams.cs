using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class ScryptParams : KdfParams
    {
        [JsonPropertyName("n")]
        public int N { get; set; }

        [JsonPropertyName("r")]
        public int R { get; set; }

        [JsonPropertyName("p")]
        public int P { get; set; }
    }
}