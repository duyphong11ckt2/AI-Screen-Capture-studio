namespace AIScreenCaptureStudio.Services.Interfaces;

/// <summary>DPAPI-backed encryption for secrets such as API keys.</summary>
public interface ISecureStorageService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
