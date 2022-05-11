using SolFrame.Cryptography;
using SolFrame.LocalStore;
using SolFrame.Statics;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Rpc;
using Solnet.Wallet;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame
{
    public class WalletMono : MonoBehaviour
    {
        //immage  serialize
        //TODO : implement wallet loading from digest cryptography
        // Start is called before the first frame update
        private AccountData localStoreData;

        public bool IsLoaded { get; private set; }

        public bool HasCache { get; private set; }

        public Account Account { get; private set; }

        public bool Load(string password)
        {
            if (!HasCache)
            {
                return false;
            }
            if (IsLoaded)
            {
                return true;
            }
            if (password is null || password == string.Empty)
            {
                return false;
            }
            if (Crypto.AESDecryptAndVerifySignature(localStoreData.DigestAES256.X2StringToBytes(), Crypto.SHA256Of(password), out var plainText))
            {
                var acc = GetAccountFromPrivateKey(plainText);
                if (acc is null)
                {
                    Debug.Log("Entered an invalid Key Pair");
                    return false;
                }
                Account = acc;
                IsLoaded = true;
                return true;
            }
            else
            {
                Debug.Log("Couldn't load KeyPair");
                return false;
            }
        }

        public bool Store(string privateKey, string password)
        {
            if (password is null || password == string.Empty)
            {
                return false;
            }
            if (privateKey is null || privateKey == string.Empty)
            {
                return false;
            }

            var bytes = Encoding.Unicode.GetBytes(privateKey);

            var digest = Crypto.AESEncryptAndSign(bytes, Crypto.SHA256Of(password));
            localStoreData = new AccountData(digest.BytesToX2String());

            var acc = GetAccountFromPrivateKey(bytes);
            if (acc is null)
            {
                Debug.Log("Entered an invalid Key Pair");
                return false;
            }
            Account = acc;
            IsLoaded = true;
            SaveManager.SaveJson(localStoreData, AccountData.Location);
            return true;
        }

        //public async Task<TokenAccount[]> GetOwnedTokenAccounts(string walletPubKey, string tokenMintPubKey, string tokenProgramPublicKey)
        //{
        //    var result = await RpcClient.GetTokenAccountsByOwnerAsync(walletPubKey, tokenMintPubKey, tokenProgramPublicKey);
        //    if (result.Result != null && result.Result.Value != null)
        //    {
        //        return result.Result.Value;
        //    }
        //    return null;
        //}
        /// <summary> Generates an Account using the private key in <byte[]> and check its validity </summary> <param
        /// name="plainPrivateKey"></param> <returns>The generated Account, null if it doesn't</returns>
        private Account GetAccountFromPrivateKey(byte[] plainPrivateKey)
        {
            var privKey = new PrivateKey(Encoding.Unicode.GetString(plainPrivateKey));
            var pubKey = new PublicKey(privKey.KeyBytes[32..]);
            //checks if the account is an existing one
            return !pubKey.IsOnCurve() ? null : new Account(privKey.Key, pubKey.Key);
        }

        private void Start()
        {
            IsLoaded = false;
            HasCache = SaveManager.TryLoadJson(AccountData.Location, out localStoreData);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}