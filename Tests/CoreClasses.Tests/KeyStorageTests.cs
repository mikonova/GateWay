using System;
using System.Collections.Generic;
using System.Text;
using CoreClasses;

namespace CoreClasses.Tests
{
    public class KeyStorageTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly KeyStorage _storage;

        public KeyStorageTests()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _storage = new KeyStorage(_testRoot);
        }

        [Fact]
        public void KeysExist_WhenNoKeys_ShouldReturnFalse()
        {
            Assert.False(_storage.KeysExist());
        }

        [Fact]
        public void SaveAndLoad_PublicKey_ShouldReturnSameKey()
        {
            var publicKey = new byte[] { 1, 2, 3, 4, 5 };
            _storage.SavePublicKey(publicKey);

            var loaded = _storage.LoadPublicKey();
            Assert.Equal(publicKey, loaded);
        }

        [Fact]
        public void SaveAndLoad_PrivateKey_ShouldReturnSameKey()
        {
            var privateKey = new byte[] { 10, 20, 30, 40, 50 };
            _storage.SavePrivateKey(privateKey);

            var loaded = _storage.LoadPrivateKey();
            Assert.Equal(privateKey, loaded);
        }

        [Fact]
        public void KeysExist_AfterSavingBoth_ShouldReturnTrue()
        {
            _storage.SavePublicKey(new byte[] { 1, 2, 3 });
            _storage.SavePrivateKey(new byte[] { 4, 5, 6 });

            Assert.True(_storage.KeysExist());
        }

        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, recursive: true);
        }
    }
}
