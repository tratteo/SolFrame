using Chaos.NaCl;
using Solnet.Wallet.Utilities;
using System;
using System.Security.Cryptography;

//using NBitcoin.DataEncoders;

namespace Solnet.Wallet
{
    public class Account
    {
        /// <summary>
        ///   The base58 encoder instance.
        /// </summary>
        //private static readonly Base58Encoder Encoder = new Base58Encoder();

        /// <summary>
        ///   Private key length.
        /// </summary>
        private const int PrivateKeyLength = 64;

        /// <summary>
        ///   Public key length.
        /// </summary>
        private const int PublicKeyLength = 32;

        /// <summary>
        ///   The private key.
        /// </summary>
        private readonly byte[] privateKey;

        /// <summary>
        ///   The public key.
        /// </summary>
        private readonly byte[] publicKey;

        /// <summary>
        ///   Get the private key encoded as base58.
        /// </summary>
        public string GetPrivateKey => Encoders.Base58.EncodeData(privateKey);

        /// <summary>
        ///   Get byte array of private key
        /// </summary>
        public byte[] GetByteArayPrivateKey => privateKey;

        /// <summary>
        ///   Get the public key encoded as base58.
        /// </summary>
        public string GetPublicKey => Encoders.Base58.EncodeData(publicKey);

        /// <summary>
        ///   Get the public key as a byte array.
        /// </summary>
        public byte[] PublicKey => publicKey;

        /// <summary>
        ///   Get the private key as a byte array.
        /// </summary>
        public byte[] PrivateKey => privateKey;

        /// <summary>
        ///   Initialize an account. Generating a random seed for the Ed25519 key pair.
        /// </summary>
        public Account()
        {
            var seed = GenerateRandomSeed();

            (privateKey, publicKey) = Ed25519Extensions.EdKeyPairFromSeed(seed);
        }

        /// <summary>
        ///   Initialize an account with the passed private and public keys.
        /// </summary>
        /// <param name="privateKey"> The private key. </param>
        /// <param name="publicKey"> The public key. </param>
        public Account(byte[] privateKey, byte[] publicKey)
        {
            if (privateKey.Length != PrivateKeyLength)
                throw new ArgumentException("invalid key length", nameof(privateKey));
            if (publicKey.Length != PublicKeyLength)
                throw new ArgumentException("invalid key length", nameof(privateKey));

            this.privateKey = privateKey;
            this.publicKey = publicKey;
        }

        /// <summary>
        ///   Verify the signed message.
        /// </summary>
        /// <param name="message"> The signed message. </param>
        /// <param name="signature"> The signature of the message. </param>
        /// <returns> </returns>
        public bool Verify(byte[] message, byte[] signature)
        {
            return Ed25519.Verify(signature, message, publicKey);
        }

        /// <summary>
        ///   Sign the data.
        /// </summary>
        /// <param name="message"> The data to sign. </param>
        /// <returns> The signature of the data. </returns>
        public byte[] Sign(byte[] message)
        {
            var signature = new byte[64];
            Ed25519.Sign(new ArraySegment<byte>(signature), new ArraySegment<byte>(message), new ArraySegment<byte>(privateKey));
            return signature;
        }

        /// <summary>
        ///   Generates a random seed for the Ed25519 key pair.
        /// </summary>
        /// <returns> The seed as byte array. </returns>
        private byte[] GenerateRandomSeed()
        {
            var bytes = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            RandomNumberGenerator.Create().GetBytes(bytes);
            return bytes;
        }
    }
}