using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
//using System.Protec

namespace CoreClasses;

public class KeyStorage
{
    private readonly string _keysPath;

    public KeyStorage(string root)
    {
        _keysPath = Path.Combine(root, "keys");
        Directory.CreateDirectory(_keysPath);
    }

    public bool KeysExist()
        => File.Exists(Path.Combine(_keysPath, "public.key"))
        && File.Exists(Path.Combine(_keysPath, "private.key"));

    public void SavePublicKey(byte[] publicKey)
        => File.WriteAllBytes(Path.Combine(_keysPath, "public.key"), publicKey);

    public byte[] LoadPublicKey()
    {
        var path = Path.Combine(_keysPath, "public.key");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Публичный ключ не найден — регистрация не завершена: {path}");
        return File.ReadAllBytes(path);
    }

    public void SavePrivateKey(byte[] privateKey)
    {
        var encrypted = ProtectedData.Protect(privateKey, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(Path.Combine(_keysPath, "private.key"), encrypted);
    }

    public byte[] LoadPrivateKey()
    {
        var encrypted = File.ReadAllBytes(Path.Combine(_keysPath, "private.key"));
        return ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
    }

    public void SaveToken(string token)
    => File.WriteAllText(Path.Combine(_keysPath, "token.txt"), token);

    public string LoadToken()
        => File.ReadAllText(Path.Combine(_keysPath, "token.txt"));

    public bool TokenExists()
        => File.Exists(Path.Combine(_keysPath, "token.txt"));
}