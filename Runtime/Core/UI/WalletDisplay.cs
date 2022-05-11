using UnityEngine;

namespace SolFrame.UI
{
    /// <summary>
    ///   Responsible for displaying information about the <see cref="Wallet"/>
    /// </summary>
    public class WalletDisplay : MonoBehaviour
    {
        [SerializeField] private Wallet target;

        public void Bind(Wallet wallet)
        {
            target = wallet;
            //TODO update the UI
        }
    }
}