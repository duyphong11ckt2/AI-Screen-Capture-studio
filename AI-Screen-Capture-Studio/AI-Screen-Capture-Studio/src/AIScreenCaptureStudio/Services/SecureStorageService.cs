using System.Security.Cryptography;
using System.Text;
using AIScreenCaptureStudio.Services.Interfaces;

namespace AIScreenCaptureStudio.Services;

/// <summary>
/// Encrypts secrets with Windows DPAPI scoped to the current user. The
/// ciphertext is Base64 so it can live safely inside the JSON settings file.
/// </summary>
public sealed class SecureStorageService : ISecureStorageService
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("AIScreenCaptureStudio.v1");

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        var data = Encoding.UTF8.GetBytes(plainText);
        var cipher = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(cipher);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            var cipher = Convert.FromBase64String(cipherText);
            var data = ProtectedData.Unprotect(cipher, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            // Corrupted or moved to another machine/user: treat as empty.
            return string.Empty;
        }
    }
}
