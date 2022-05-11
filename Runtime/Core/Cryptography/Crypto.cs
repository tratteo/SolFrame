using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SolFrame.Cryptography
{
    public static class Crypto
    {
        /// <summary>
        ///   Encrypt using the AES-256 cipher the plainText using the provided key
        /// </summary>
        /// <param name="plainText"> </param>
        /// <param name="key"> </param>
        /// <returns> The encrypted cipherText </returns>
        public static byte[] AESEncryptAndSign(byte[] plainText, byte[] key)
        {
            plainText = SignWithHMACSHA256(key, plainText);

            using var aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.GenerateIV();

            // Create an encryptor to perform the stream transform.
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            //using var swEncrypt = new BinaryWriter(csEncrypt);
            using var swEncrypt = new BinaryWriter(csEncrypt);

            //Write all data to the stream.
            //swEncrypt.Write(plainText, 0, plainText.Length);
            swEncrypt.Write(plainText, 0, plainText.Length);

            swEncrypt.Close();
            var encrypted = msEncrypt.ToArray();

            csEncrypt.Close();
            msEncrypt.Close();

            // Create a bigger final cipher text to include the IV
            var finalCipher = new byte[encrypted.Length + aesAlg.IV.Length];
            Buffer.BlockCopy(aesAlg.IV, 0, finalCipher, 0, aesAlg.IV.Length);
            Buffer.BlockCopy(encrypted, 0, finalCipher, aesAlg.IV.Length, encrypted.Length);

            aesAlg.Dispose();
            return finalCipher;
        }

        /// <summary>
        ///   Decrypt using the AES-256 cipher the cipherText using the provided key
        /// </summary>
        /// <param name="cipherText"> </param>
        /// <param name="key"> </param>
        /// <param name="plainText"> Upon success, the decrypted plainText </param>
        /// <returns> True if the decryption process is successful, false otherwise </returns>
        public static bool AESDecryptAndVerifySignature(byte[] cipherText, byte[] key, out byte[] plainText)
        {
            try
            {
                using var aesAlg = Aes.Create();
                aesAlg.Key = key;

                // Retrieve the IV, prepended at the beginning of the total ciphertext
                var iv = new byte[aesAlg.IV.Length];
                Buffer.BlockCopy(cipherText, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                // Retrieve the actual ciphertext
                var cipher = new byte[cipherText.Length - aesAlg.IV.Length];
                Buffer.BlockCopy(cipherText, aesAlg.IV.Length, cipher, 0, cipher.Length);

                // Create a decryptor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new BinaryReader(csDecrypt);
                //using var srDecrypt = new BinaryReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream and place them in a string.

                var decrypted = srDecrypt.ReadBytes(cipher.Length);
                srDecrypt.Close();
                csDecrypt.Close();
                msDecrypt.Close();
                aesAlg.Dispose();
                if (VerifySignWithHMACSHA256(key, decrypted, out plainText))
                {
                    return true;
                }
                else
                {
                    decrypted = null;
                    return false;
                }
            }
            catch (Exception)
            {
                plainText = null;
                return false;
            }
        }

        /// <summary>
        ///   Calculate the SHA-256 hashcode
        /// </summary>
        /// <param name="plainText"> </param>
        /// <returns> </returns>
        public static byte[] SHA256Of(string plainText) => SHA256Of(plainText, Encoding.Unicode);

        /// <summary>
        ///   Calculate the SHA-256 hashcode
        /// </summary>
        /// <param name="plainText"> </param>
        /// <returns> </returns>
        public static byte[] SHA256Of(string plainText, Encoding encoding) => SHA256Of(encoding.GetBytes(plainText));

        /// <summary>
        ///   Calculate the SHA-256 hashcode
        /// </summary>
        /// <param name="plainText"> </param>
        /// <returns> </returns>
        public static byte[] SHA256Of(byte[] plainText) => SHA256.Create().ComputeHash(plainText);

        /// <summary>
        ///   Sign a plaintext with an HMAC using the specified key and the SHA-256 hashcode
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="plainText"> </param>
        /// <returns> The signed plaintext </returns>
        private static byte[] SignWithHMACSHA256(byte[] key, byte[] plainText)
        {
            using var hmac = new HMACSHA256(key);
            var signature = hmac.ComputeHash(plainText);
            var composed = new byte[plainText.Length + signature.Length];
            Buffer.BlockCopy(signature, 0, composed, 0, signature.Length);
            Buffer.BlockCopy(plainText, 0, composed, signature.Length, plainText.Length);
            return composed;
        }

        /// <summary>
        ///   Verify the HMAC signature of the provided ciphertext using the specified key and the SHA-256 hashcode
        /// </summary>
        /// <param name="key"> </param>
        /// <param name="digest"> </param>
        /// <param name="plainText"> Upon success, the retrieved plaintext stripped of the signature </param>
        /// <returns> True if the verification passed, false otherwise </returns>
        private static bool VerifySignWithHMACSHA256(byte[] key, byte[] digest, out byte[] plainText)
        {
            using var hmac = new HMACSHA256(key);
            var storedHmac = new byte[hmac.HashSize / 8];
            plainText = new byte[digest.Length - storedHmac.Length];
            Buffer.BlockCopy(digest, 0, storedHmac, 0, storedHmac.Length);
            Buffer.BlockCopy(digest, storedHmac.Length, plainText, 0, plainText.Length);
            var calculatedHmac = hmac.ComputeHash(plainText);

            for (var i = 0; i < calculatedHmac.Length; i++)
            {
                if (calculatedHmac[i] != storedHmac[i]) return false;
            }
            return true;
        }
    }
}