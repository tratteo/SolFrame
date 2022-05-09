using Chaos.NaCl;
using Solnet.Wallet.Utilities;
using System;
using System.Diagnostics;

namespace Solnet.Wallet
{
    /// <summary>
    ///   Implements the public key functionality.
    /// </summary>
    [DebuggerDisplay("Key = {" + nameof(Key) + "}")]
    public class PublicKey
    {
        /// <summary>
        ///   Public key length.
        /// </summary>
        public const int PublicKeyLength = 32;

        private string key;

        private byte[] keyBytes;

        /// <summary>
        ///   The key as base-58 encoded string.
        /// </summary>
        public string Key
        {
            get
            {
                if (key == null)
                {
                    Key = Encoders.Base58.EncodeData(KeyBytes);
                }
                return key;
            }
            set => key = value;
        }

        /// <summary>
        ///   The bytes of the key.
        /// </summary>
        public byte[] KeyBytes
        {
            get
            {
                if (keyBytes == null)
                {
                    KeyBytes = Encoders.Base58.DecodeData(Key);
                }
                return keyBytes;
            }
            set => keyBytes = value;
        }

        /// <summary>
        ///   Initialize the public key from the given byte array.
        /// </summary>
        /// <param name="key"> The public key as byte array. </param>
        public PublicKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != PublicKeyLength)
                throw new ArgumentException("invalid key length", nameof(key));
            KeyBytes = new byte[PublicKeyLength];
            Array.Copy(key, KeyBytes, PublicKeyLength);
        }

        /// <summary>
        ///   Initialize the public key from the given string.
        /// </summary>
        /// <param name="key"> The public key as base58 encoded string. </param>
        public PublicKey(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        ///   Conversion between a <see cref="PublicKey"/> object and the corresponding base-58 encoded public key.
        /// </summary>
        /// <param name="publicKey"> The PublicKey object. </param>
        /// <returns> The base-58 encoded public key. </returns>
        public static implicit operator string(PublicKey publicKey) => publicKey.Key;

        /// <summary>
        ///   Conversion between a base-58 encoded public key and the <see cref="PublicKey"/> object.
        /// </summary>
        /// <param name="address"> The base-58 encoded public key. </param>
        /// <returns> The PublicKey object. </returns>
        public static explicit operator PublicKey(string address) => new PublicKey(address);

        /// <summary>
        ///   Conversion between a <see cref="PublicKey"/> object and the public key as a byte array.
        /// </summary>
        /// <param name="publicKey"> The PublicKey object. </param>
        /// <returns> The public key as a byte array. </returns>
        public static implicit operator byte[](PublicKey publicKey) => publicKey.KeyBytes;

        /// <summary>
        ///   Conversion between a public key as a byte array and the corresponding <see cref="PublicKey"/> object.
        /// </summary>
        /// <param name="keyBytes"> The public key as a byte array. </param>
        /// <returns> The PublicKey object. </returns>
        public static explicit operator PublicKey(byte[] keyBytes) => new PublicKey(keyBytes);

        /// <summary>
        ///   Verify the signed message.
        /// </summary>
        /// <param name="message"> The signed message. </param>
        /// <param name="signature"> The signature of the message. </param>
        /// <returns> </returns>
        public bool Verify(byte[] message, byte[] signature) => Ed25519.Verify(signature, message, KeyBytes);

        /// <inheritdoc cref="Equals(object)"/>
        public override bool Equals(object obj) => obj is PublicKey pk && pk.Key == Key;

        /// <inheritdoc cref="ToString"/>
        public override string ToString() => Key;

        public override int GetHashCode() => base.GetHashCode();
    }
}