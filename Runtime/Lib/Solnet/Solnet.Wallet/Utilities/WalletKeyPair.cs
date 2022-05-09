using dotnetstandard_bip39;
using System;

namespace Solnet.Wallet.Utilities
{
    public static class WalletKeyPair
    {
        public static string derivePath = "m/44'/501'/0'/0'";

        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            var arr = new byte[hex.Length >> 1];

            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            var val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string GenerateNewMnemonic()
        {
            //dotnetstandard_bip39.BIP39 p = new BIP39()
            var p = new BIP();
            var mnemonic = p.GenerateMnemonic(256, BIP39Wordlist.English);
            return mnemonic;
        }

        public static byte[] GetBIP39SeedBytes(string seed) => StringToByteArrayFastest(MnemonicToSeedHex(seed));

        public static string MnemonicToSeedHex(string seed)
        {
            var p = new BIP39();
            return p.MnemonicToSeedHex(seed, string.Empty);
        }

        public static byte[] GetBIP32SeedByte(byte[] seed)
        {
            var bip = new Ed25519Bip32(seed);
            (var key, _) = bip.DerivePath(derivePath);
            return key;
        }

        public static byte[] GenerateSeedFromMnemonic(string mnemonic) => GetBIP39SeedBytes(mnemonic);

        public static Keypair GenerateKeyPairFromMnemonic(string mnemonics)
        {
            var bip39seed = GetBIP39SeedBytes(mnemonics);

            var finalSeed = GetBIP32SeedByte(bip39seed);
            (var privateKey, var publicKey) = Ed25519Extensions.EdKeyPairFromSeed(finalSeed);

            return new Keypair(publicKey, privateKey);
        }

        public static bool CheckMnemonicValidity(string mnemonic)
        {
            var mnemonicWords = mnemonic.Split(' ');
            return mnemonicWords.Length is 12 or 24;
        }

        public static bool CheckPasswordValidity(string password) => !string.IsNullOrEmpty(password);
    }
}