using System.Text.Json.Serialization;

namespace Solnet.KeyStore.Model
{
    public class CryptoInfo<TKdfParams> where TKdfParams : KdfParams
    {
        [JsonPropertyName("cipher")]
        public string Cipher { get; }

        [JsonPropertyName("ciphertext")]
        public string CipherText { get; set; }

        // ReSharper disable once StringLiteralTypo
        [JsonPropertyName("cipherparams")]
        public CipherParams CipherParams { get; set; }

        [JsonPropertyName("kdf")]
        public string Kdf { get; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }

        // ReSharper disable once StringLiteralTypo
        [JsonPropertyName("kdfparams")]
        public TKdfParams Kdfparams { get; set; }

        public CryptoInfo()
        {
        }

        public CryptoInfo(string cipher, byte[] cipherText, byte[] iv, byte[] mac, byte[] salt, TKdfParams kdfParams,
            string kdfType)
        {
            Cipher = cipher;
            CipherText = cipherText.ToHex();
            Mac = mac.ToHex();
            CipherParams = new CipherParams(iv);
            Kdfparams = kdfParams;
            Kdfparams.Salt = salt.ToHex();
            Kdf = kdfType;
        }
    }
}