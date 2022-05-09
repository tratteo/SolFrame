namespace Solnet.Wallet.Utilities
{
    public class Keypair
    {
        public string publicKey;

        public string privateKey;

        public byte[] publicKeyByte;

        public byte[] privateKeyByte;

        public Keypair()
        { }

        public Keypair(byte[] publicKey, byte[] privateKey)
        {
            publicKeyByte = publicKey;
            privateKeyByte = privateKey;

            this.publicKey = Encoders.Base58.EncodeData(publicKey);
            this.privateKey = Encoders.Base58.EncodeData(privateKey);
        }
    }
}