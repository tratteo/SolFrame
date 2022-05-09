using Org.BouncyCastle.Security;

namespace Solnet.KeyStore.Crypto
{
    public class RandomBytesGenerator : IRandomBytesGenerator
    {
        private static readonly SecureRandom Random = new SecureRandom();

        public byte[] GenerateRandomInitializationVector() => GenerateRandomBytes(16);

        public byte[] GenerateRandomSalt() => GenerateRandomBytes(32);

        private static byte[] GenerateRandomBytes(int size)
        {
            var bytes = new byte[size];
            Random.NextBytes(bytes);
            return bytes;
        }
    }
}