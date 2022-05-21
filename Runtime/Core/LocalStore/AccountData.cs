using System.IO;
using UnityEngine;

namespace SolFrame.LocalStore
{
    [System.Serializable]
    public class AccountData
    {
        public static readonly string Location = $"{Application.persistentDataPath}{Path.AltDirectorySeparatorChar}wallet_account_data.json";

        [SerializeField] private string digestAES256;

        public string DigestAES256 => digestAES256;

        public AccountData(string digestAES256)
        {
            this.digestAES256 = digestAES256;
        }
    }
}