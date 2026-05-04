using System;
using System.Collections.Generic;
using System.Text;
using NSec.Cryptography;
using PublicKey = NSec.Cryptography.PublicKey;

namespace CoreClasses
{
    public class CryptoService : ICryptoService
    {
        private static readonly KeyAgreementAlgorithm _algorithm = KeyAgreementAlgorithm.X25519;
        private static readonly AeadAlgorithm _aead = AeadAlgorithm.ChaCha20Poly1305;

        public KeyPair GenerateKeys()
        {
            var key = Key.Create(_algorithm, new KeyCreationParameters
            {
                ExportPolicy = KeyExportPolicies.AllowPlaintextExport
            });

            var publicKey = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
            var privateKey = key.Export(KeyBlobFormat.RawPrivateKey);

            return new KeyPair(publicKey, privateKey);
        }

        public byte[] Encrypt(byte[] message, byte[] myPrivateKey, byte[] recipientPublicKey)
        {
            var privKey = Key.Import(_algorithm, myPrivateKey, KeyBlobFormat.RawPrivateKey);
            var pubKey = PublicKey.Import(_algorithm, recipientPublicKey, KeyBlobFormat.RawPublicKey);

            var sharedSecret = _algorithm.Agree(privKey, pubKey);

            var encryptionKey = KeyDerivationAlgorithm.HkdfSha256.DeriveKey(
                sharedSecret, null, null, _aead, new KeyCreationParameters());

            var nonce = new byte[_aead.NonceSize];
            System.Security.Cryptography.RandomNumberGenerator.Fill(nonce);

            var ciphertext = _aead.Encrypt(encryptionKey, nonce, null, message);

            return [.. nonce, .. ciphertext];
        }

        public byte[] Decrypt(byte[] encrypted, byte[] myPrivateKey, byte[] senderPublicKey)
        {
            var privKey = Key.Import(_algorithm, myPrivateKey, KeyBlobFormat.RawPrivateKey);
            var pubKey = PublicKey.Import(_algorithm, senderPublicKey, KeyBlobFormat.RawPublicKey);

            var sharedSecret = _algorithm.Agree(privKey, pubKey);

            var encryptionKey = KeyDerivationAlgorithm.HkdfSha256.DeriveKey(
                sharedSecret, null, null, _aead, new KeyCreationParameters());

            var nonce = encrypted[.._aead.NonceSize];
            var ciphertext = encrypted[_aead.NonceSize..];

            return _aead.Decrypt(encryptionKey, nonce, null, ciphertext)
                   ?? throw new Exception("Расшифровка не удалась — данные повреждены или ключ неверный");
        }
    }
}
