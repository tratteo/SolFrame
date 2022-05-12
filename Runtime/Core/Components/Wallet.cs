using SolFrame.Cryptography;
using SolFrame.LocalStore;
using SolFrame.Statics;
using SolFrame.Utils;
using Solnet.Extensions;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SolFrame
{
    /// <summary>
    ///   Base Solana wallet component
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        private readonly List<SubscriptionState> subscriptions = new List<SubscriptionState>();

        [Header("Debug")]
        [SerializeField] private bool enableLogs = true;

        public bool IsLoaded { get; private set; }

        public Account Account { get; private set; }

        public bool IsSubscribed => subscriptions.Count > 0;

        #region CachedValues

        public Cached<ulong> SolBalance { get; private set; } = new Cached<ulong>();

        public TokenWallet TokenWallet { get; private set; }

        #endregion CachedValues

        /// <summary>
        ///   Called when the <see cref="AccountData"/> configuration is loaded
        /// </summary>
        public event Action Loaded = delegate { };

        /// <summary>
        ///   Called when the <see cref="AccountData"/> configuration is unloaded
        /// </summary>
        public event Action Unloaded = delegate { };

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
            Unsubscribe();
            IsLoaded = false;
            Account = null;
            Unloaded?.Invoke();
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
        ///   Clear all the <see cref="Cached{T}"/>
        /// </summary>
        public void ClearCache()
        {
            SolBalance.Clear();
            this.LogObj("Cache cleared", ref enableLogs);
        }

        /// <summary>
        ///   Initialize all the <see cref="Cached{T}"/> in batch using the <see cref="Solnet.Rpc.SolanaRpcBatchWithCallbacks"/>
        /// </summary>
        public void InitializeCache()
        {
            SolanaEndpointManager.Instance.BatchRequest(batcher =>
                batcher.GetBalance(Account.PublicKey, Commitment.Finalized, (res, ex) =>
                    HandleCached(SolBalance, res, ex)));
            this.LogObj("Cache initialized", ref enableLogs);
            SolanaEndpointManager.Instance.FlushBatch();
            Task.Run(async () => await TokenWallet.LoadAsync(SolanaEndpointManager.Instance.RpcClient, await TokenMintResolver.LoadAsync(), Account.PublicKey));
        }

        /// <summary>
        ///   Subscribe to the <see cref="Solnet.Rpc.IStreamingRpcClient"/> if not already subscribed
        /// </summary>
        public bool Subscribe()
        {
            if (subscriptions.Count > 0) return false;
            var streamingClient = SolanaEndpointManager.Instance.StreamingRpcClient;
            subscriptions.Add(streamingClient.SubscribeAccountInfo(Account.PublicKey, AccountInfoSubscriptionHandler));
            this.LogObj("Subscription registered", ref enableLogs);
            return true;
        }

        /// <summary>
        ///   Unsubscribe from the <see cref="Solnet.Rpc.IStreamingRpcClient"/>
        /// </summary>
        public void Unsubscribe()
        {
            var tasks = new Task[subscriptions.Count];
            for (var i = 0; i < subscriptions.Count; i++)
            {
                tasks[i] = SolanaEndpointManager.Instance.StreamingRpcClient.UnsubscribeAsync(subscriptions[i]);
            }
            Task.WhenAll(tasks);
            if (subscriptions.Count > 0) this.LogObj("Unsubscription successful", ref enableLogs);
            subscriptions.Clear();
        }

        private void OnLoad()
        {
            InitializeCache();
            Loaded?.Invoke();
            this.LogObj("Account loaded", ref enableLogs);
        }

        private void Start()
        {
            if (SolanaEndpointManager.Instance is null)
            {
                this.LogObj("Unable to locate the SolanaEndpointManager instance", ref enableLogs, LogType.Warning);
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
            if (plainPrivateKey is null) return false;
            var privKey = new PrivateKey(Encoding.Unicode.GetString(plainPrivateKey));
            var pubKey = new PublicKey(privKey.KeyBytes[32..]);

            // Checks if the account is an existing one
            if (!pubKey.IsValid()) return false;
            account = new Account(privKey.Key, pubKey.Key);
            return true;
        }

        #region Cache

        private void HandleCached<T>(Cached<T> cached, ResponseValue<T> response, Exception exception)
        {
            if (exception is null) cached.Update(response.Value);
            else Debug.LogException(exception);
        }

        #endregion Cache

        #region Subscription

        private void AccountInfoSubscriptionHandler(SubscriptionState state, ResponseValue<AccountInfo> response)
        {
            SolBalance.Update(response.Value.Lamports);
        }

        #endregion Subscription
    }
}