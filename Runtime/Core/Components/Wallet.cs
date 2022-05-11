using SolFrame.Cryptography;
using SolFrame.LocalStore;
using SolFrame.Statics;
using Solnet.Wallet;
using System.Text;
using UnityEngine;

namespace SolFrame
{
    /// <summary>
    ///   Base Solana wallet component
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        public bool IsLoaded { get; private set; }

        public Account Account { get; private set; }

        /// <summary>
        ///   Load the existing <see cref="AccountData"/> if existing
        /// </summary>
        /// <param name="password"> </param>
        /// <returns> <c> True </c> upon success </returns>
        public bool Load(string password)
        {
            if (IsLoaded) return true;
            if (password is null || password == string.Empty) return false;

            // Load the store data on demand (lazy init), little less performant (orders of 10E-9s) but much more solid.
            if (!SaveManager.TryLoadJson<AccountData>(AccountData.Location, out var localStoreData)) return false;

            if (Crypto.AESDecryptAndVerifySignature(localStoreData.DigestAES256.X2StringToBytes(), Crypto.SHA256Of(password), out var plainText))
            {
                if (!TryGetAccountFromPrivateKey(plainText, out var acc))
                {
                    Debug.LogWarning("Entered an invalid PrivateKey");
                    return false;
                }
                Account = acc;
                IsLoaded = true;
                return true;
            }
            else
            {
                Debug.LogWarning("Couldn't load KeyPair");
                return false;
            }
        }

        /// <summary>
        ///   Create a localstore of a new <see cref="AccountData"/> for the wallet. If already existing, overwrite it
        /// </summary>
        /// <param name="privateKey"> </param>
        /// <param name="password"> </param>
        /// <returns> <c> True </c> upon success </returns>
        public bool Store(string privateKey, string password)
        {
            if (password is null || password == string.Empty) return false;
            if (privateKey is null || privateKey == string.Empty) return false;

            var bytes = Encoding.Unicode.GetBytes(privateKey);

            // Check validity of wallet before encrypting
            if (!TryGetAccountFromPrivateKey(bytes, out var acc))
            {
                Debug.LogWarning("Entered an invalid PrivateKey");
                return false;
            }
            Account = acc;

            var digest = Crypto.AESEncryptAndSign(bytes, Crypto.SHA256Of(password));
            SaveManager.SaveJson(new AccountData(digest.BytesToX2String()), AccountData.Location);

            IsLoaded = true;
            return true;
        }

        private void Start()
        {
            if (SolanaEndpointManager.Instance is null)
            {
                Debug.LogWarning("Unable to find the SolanaEndpointManager in scene");
            }
        }

        /// <summary>
        ///   Try to retrieve a Solana <see cref="Solnet.Wallet.Account"/> from the <c> byte[] </c> of the private key and checks its validity
        /// </summary>
        /// <param name="plainPrivateKey"> </param>
        /// <param name="account"> </param>
        /// <returns> <c> True </c> if the <see cref="Solnet.Wallet.Account"/> is created successfully </returns>
        private bool TryGetAccountFromPrivateKey(byte[] plainPrivateKey, out Account account)
        {
            account = null;
            if (plainPrivateKey is null || plainPrivateKey.Length != 64) return false;
            var privKey = new PrivateKey(Encoding.Unicode.GetString(plainPrivateKey));
            var pubKey = new PublicKey(privKey.KeyBytes[32..]);

            // Checks if the account is an existing one
            if (!pubKey.IsValid()) return false;
            account = new Account(privKey.Key, pubKey.Key);
            return true;
        }
    }
}