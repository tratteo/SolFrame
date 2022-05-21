using SolFrame.Cryptography;
using SolFrame.LocalStore;
using SolFrame.Statics;
using SolFrame.Utils;
using SolmangoNET;
using Solnet.Extensions;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame
{
    /// <summary>
    ///   Base Solana wallet component.
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private bool autoRefreshTokenWallet = true;
        [SerializeField] private float refreshTime = 30F;
        [Header("Debug")]
        [SerializeField] private bool enableLogs = true;

        /// <summary>
        ///   Is the <see cref="Account"/> loaded
        /// </summary>
        public bool IsLoaded { get; private set; }

        public Account Account { get; private set; }

        public TokenWallet TokenWallet { get; private set; }

        /// <summary>
        ///   Is the <see cref="TokenWallet"/> loaded
        /// </summary>
        public bool IsTokenWalletLoaded { get; private set; }

        /// <summary>
        ///   Called when the <see cref="AccountData"/> configuration is loaded.
        ///   <para> <i> Runs on the main thread </i> </para>
        /// </summary>
        public event Action Loaded = delegate { };

        /// <summary>
        ///   Called when the <see cref="AccountData"/> configuration is unloaded
        ///   <para> <i> Runs on the main thread </i> </para>
        /// </summary>
        public event Action Unloaded = delegate { };

        /// <summary>
        ///   Called when the <see cref="Solnet.Extensions.TokenWallet"/> is loaded. The <see cref="Solnet.Extensions.TokenWallet"/> is
        ///   loaded right after the <see cref="Loaded"/> event is called but process is async.
        ///   <para> <i> Runs on the main thread </i> </para>
        /// </summary>
        public event Action TokenWalletLoaded = delegate { };

        /// <summary>
        ///   Called when the <see cref="Solnet.Extensions.TokenWallet"/> is refreshed.
        ///   <para> <i> Runs on the main thread </i> </para>
        /// </summary>
        public event Action TokenWalletRefreshed = delegate { };

        /// <summary>
        ///   Refresh the <see cref="Solnet.Extensions.TokenWallet"/>
        /// </summary>
        /// <returns> </returns>
        public async Task RefreshTokenWalletAsync()
        {
            await TokenWallet.RefreshAsync();
            MainThread.InUpdate(() => TokenWalletRefreshed?.Invoke());
        }

        private void Start()
        {
            if (SolanaEndpointManager.Instance is null)
            {
                this.LogObj("Unable to locate the SolanaEndpointManager instance", ref enableLogs, LogType.Warning);
            }
            _ = TokenWalletRefreshDaemon();
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
            if (plainPrivateKey is null) return false;
            try
            {
                var privKey = new PrivateKey(Encoding.Unicode.GetString(plainPrivateKey));

                if (privKey.KeyBytes.Length != 64) return false;
                var pubKey = new PublicKey(privKey.KeyBytes[32..]);

                // Checks if the account is an existing one
                if (!pubKey.IsValid()) return false;
                account = new Account(privKey.Key, pubKey.Key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Tokens Management

        /// <summary>
        ///   <para> Send an SPL token. </para>
        ///   <para> Sending transaction at a high rate can cause: </para>
        ///   <list>
        ///     <item>
        ///       <description> - In case of bounded rate endpoint, rejection </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         - In case the <see cref="Solnet.Rpc.Models.LatestBlockHash"/> is the same of a previous transaction, silent request dropping
        ///       </description>
        ///     </item>
        ///   </list>
        /// </summary>
        /// <param name="destination"> PublicKey of the receiver </param>
        /// <param name="tokenMint"> The token mint </param>
        /// <param name="amount"> Amount with decimals </param>
        /// <param name="resultCallback">
        ///   Callback that is called with True only if the transaction is confirmed by validators, with False in any other case
        /// </param>
        /// <returns> </returns>
        public async Task SendToken(string destination, string tokenMint, double amount = 1D, Action<bool> resultCallback = null)
        {
            // Send trasaction
            var oneOf = await Solmango.SendSplToken(SolanaEndpointManager.Instance.RpcClient, Account, destination, tokenMint, amount);

            // Catch any exception from Solmango
            if (oneOf.TryPickT1(out var ex, out var tsx))
            {
                this.LogObj($"Exception in sending token: {ex}", ref enableLogs, LogType.Error);
                MainThread.InUpdate(() => resultCallback?.Invoke(false));
            }
            else
            {
                // The transaction returned is valid but not confirmed, therefore subscribe for its confirmation
                var state = SolanaEndpointManager.Instance.StreamingRpcClient.SubscribeSignature(tsx, (state, result) =>
                {
                    if (result.Value.Error is null)
                    {
                        this.LogObj($"{tokenMint} [{amount}] -> {destination}", ref enableLogs);
                        MainThread.InUpdate(() => resultCallback?.Invoke(true));
                    }
                    else
                    {
                        this.LogObj($"Exception in confirming signature: {result.Value.Error.Type}", ref enableLogs, LogType.Exception);
                        MainThread.InUpdate(() => resultCallback?.Invoke(false));
                    }
                    // Cleanup subscription
                    _ = SolanaEndpointManager.Instance.StreamingRpcClient.UnsubscribeAsync(state);
                }, Commitment.Confirmed);
            }
        }

        #endregion Tokens Management

        #region Loading

        /// <summary>
        ///   Create a localstore of a new <see cref="AccountData"/> for the wallet in the <see cref="Application.persistentDataPath"/>. If
        ///   already existing, overwrite it
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
                this.LogObj("Entered an invalid PrivateKey", ref enableLogs, LogType.Warning);
                return false;
            }
            Account = acc;

            var digest = Crypto.AESEncryptAndSign(bytes, Crypto.SHA256Of(password));
            SaveManager.SaveJson(new AccountData(digest.BytesToX2String()), AccountData.Location);

            IsLoaded = true;
            OnLoad();
            return true;
        }

        /// <summary>
        ///   Load the existing <see cref="AccountData"/>
        /// </summary>
        /// <param name="password"> </param>
        /// <returns> <c> True </c> upon success </returns>
        public bool Load(string password)
        {
            if (IsLoaded) return true;
            if (password is null || password == string.Empty) return false;

            // Load the store data on demand (lazy init), little less performant (orders of 10E-9s) but much more solid.
            if (!SaveManager.TryLoadJson<AccountData>(AccountData.Location, out var localStoreData))
            {
                this.LogObj("Unable to load the account data", ref enableLogs, LogType.Warning);
                return false;
            }

            if (Crypto.AESDecryptAndVerifySignature(localStoreData.DigestAES256.X2StringToBytes(), Crypto.SHA256Of(password), out var plainText))
            {
                if (!TryGetAccountFromPrivateKey(plainText, out var acc))
                {
                    this.LogObj("Entered an invalid PrivateKey", ref enableLogs, LogType.Warning);
                    return false;
                }
                Account = acc;
                IsLoaded = true;
                OnLoad();
                return true;
            }
            else
            {
                this.LogObj("Couldn't load KeyPair, decryption unsuccessful", ref enableLogs, LogType.Warning);
                return false;
            }
        }

        /// <summary>
        ///   Unload the current <see cref="AccountData"/> and unsubscribe from the <see cref="Solnet.Rpc.IStreamingRpcClient"/>
        /// </summary>
        public void Unload()
        {
            TokenWallet = null;
            IsTokenWalletLoaded = false;
            IsLoaded = false;
            Account = null;
            Unloaded?.Invoke();
        }

        /// <summary>
        ///   Load the <see cref="Solnet.Extensions.TokenWallet"/>
        /// </summary>
        private async Task LoadTokenWalletAsync()
        {
            if (!IsLoaded) return;
            TokenWallet = await TokenWallet.LoadAsync(SolanaEndpointManager.Instance.RpcClient, await TokenMintResolver.LoadAsync(), Account.PublicKey);
            MainThread.InUpdate(() => TokenWalletLoaded?.Invoke());
            IsTokenWalletLoaded = true;
        }

        private void OnLoad()
        {
            _ = LoadTokenWalletAsync();
            Loaded?.Invoke();
            this.LogObj("Successfully loaded the AccoundData", ref enableLogs);
        }

        #endregion Loading

        private async Task TokenWalletRefreshDaemon()
        {
            while (true)
            {
                if (TokenWallet is not null && autoRefreshTokenWallet)
                {
                    await RefreshTokenWalletAsync();
                }
                await Task.Delay((int)(refreshTime * 1E3));
            }
        }
    }
}