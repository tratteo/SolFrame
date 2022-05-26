using SolFrame.Utils;
using Solnet.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace SolFrame.UI
{
    /// <summary>
    ///   Responsible for displaying information about the <see cref="Wallet"/>
    /// </summary>
    public class WalletDisplay : MonoBehaviour
    {
        [SerializeField] private Wallet target;
        [SerializeField] private List<string> filterMints;

        /// <summary>
        ///   Bind the instance of a <see cref="Wallet"/> to be displayed
        /// </summary>
        /// <param name="wallet"> </param>
        public void Bind(Wallet wallet)
        {
            target = wallet;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!target.IsTokenWalletLoaded)
            {
                target.TokenWalletLoaded += UpdateUI;
                return;
            }
            target.TokenWalletLoaded -= UpdateUI;
            var filterList = target.TokenWallet.TokenAccounts();
            var accounts = new List<TokenWalletAccount>();
            if (filterMints.Count <= 0)
            {
                accounts.AddRange(filterList.WhichAreAssociatedTokenAccounts());
            }
            else
            {
                foreach (var mint in filterMints)
                {
                    var ata = filterList.WithMint(mint).AssociatedTokenAccount();
                    if (ata is not null)
                    {
                        accounts.Add(ata);
                    }
                }
            }
            foreach (var ata in accounts)
            {
                this.LogObj(ata.TokenMint);
            }
        }

        private void Start()
        {
            if (target)
            {
                Bind(target);
            }
        }
    }
}