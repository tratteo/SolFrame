using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class CipherParams
    {
        [JsonPropertyName("iv")]
        public string Iv { get; set; }

        public CipherParams()
        {
        }

        public CipherParams(byte[] iv)
        {
            Iv = iv.ToHex();
        }
    }
}