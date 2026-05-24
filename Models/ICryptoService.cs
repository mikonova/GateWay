namespace CoreClasses;

public interface ICryptoService
{
    KeyPair GenerateKeys();
    byte[] Encrypt(byte[] message, byte[] myPrivateKey, byte[] recipientPublicKey);
    byte[] Decrypt(byte[] encrypted, byte[] myPrivateKey, byte[] senderPublicKey);
}
