using CoreClasses;

namespace CoreClasses.Tests;

public class CryptoServiceTests
{
    private readonly CryptoService _crypto = new();

    [Fact]
    public void GenerateKeys_ShouldReturnNonEmptyKeys()
    {
        var keyPair = _crypto.GenerateKeys();

        Assert.NotNull(keyPair.PublicKey);
        Assert.NotNull(keyPair.PrivateKey);
        Assert.NotEmpty(keyPair.PublicKey);
        Assert.NotEmpty(keyPair.PrivateKey);
    }

    [Fact]
    public void GenerateKeys_TwoCalls_ShouldReturnDifferentKeys()
    {
        var keyPair1 = _crypto.GenerateKeys();
        var keyPair2 = _crypto.GenerateKeys();

        Assert.NotEqual(keyPair1.PublicKey, keyPair2.PublicKey);
        Assert.NotEqual(keyPair1.PrivateKey, keyPair2.PrivateKey);
    }

    [Fact]
    public void Encrypt_Decrypt_ShouldReturnOriginalMessage()
    {
        var alice = _crypto.GenerateKeys();
        var bob = _crypto.GenerateKeys();

        var originalMessage = "Привет, Боб!"u8.ToArray();

        var encrypted = _crypto.Encrypt(originalMessage, alice.PrivateKey, bob.PublicKey);
        var decrypted = _crypto.Decrypt(encrypted, bob.PrivateKey, alice.PublicKey);

        Assert.Equal(originalMessage, decrypted);
    }

    [Fact]
    public void Encrypt_ShouldNotReturnOriginalMessage()
    {
        var alice = _crypto.GenerateKeys();
        var bob = _crypto.GenerateKeys();

        var originalMessage = "Секретное сообщение"u8.ToArray();
        var encrypted = _crypto.Encrypt(originalMessage, alice.PrivateKey, bob.PublicKey);

        Assert.NotEqual(originalMessage, encrypted);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldThrowException()
    {
        var alice = _crypto.GenerateKeys();
        var bob = _crypto.GenerateKeys();
        var eve = _crypto.GenerateKeys();

        var message = "Секрет"u8.ToArray();
        var encrypted = _crypto.Encrypt(message, alice.PrivateKey, bob.PublicKey);

        Assert.Throws<Exception>(() =>
            _crypto.Decrypt(encrypted, eve.PrivateKey, alice.PublicKey));
    }

    [Fact]
    public void Encrypt_SameMessageTwice_ShouldReturnDifferentCiphertext()
    {
        var alice = _crypto.GenerateKeys();
        var bob = _crypto.GenerateKeys();

        var message = "Одинаковое сообщение"u8.ToArray();

        var encrypted1 = _crypto.Encrypt(message, alice.PrivateKey, bob.PublicKey);
        var encrypted2 = _crypto.Encrypt(message, alice.PrivateKey, bob.PublicKey);

        Assert.NotEqual(encrypted1, encrypted2);
    }
}