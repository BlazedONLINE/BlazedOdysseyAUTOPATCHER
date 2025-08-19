using NSec.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher.Services;

public class SecurityService : ISecurityService
{
    // Embedded public key for manifest verification
    // This should be replaced with your actual public key
    private static readonly byte[] PublicKey = Convert.FromHexString(
        "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");

    public bool VerifyManifestSignature(byte[] manifestData, byte[] signature)
    {
        try
        {
            var algorithm = SignatureAlgorithm.Ed25519;
            var publicKey = PublicKey.FromPublicKey(algorithm, out var keyCreationError);
            
            if (keyCreationError != null)
            {
                return false;
            }

            return algorithm.Verify(publicKey, manifestData, signature);
        }
        catch
        {
            return false;
        }
    }

    public bool VerifyFileHash(string filePath, string expectedHash)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var actualHash = CalculateFileHash(filePath);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public string CalculateFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        
        var buffer = new byte[81920]; // 80KB buffer
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
        }
        
        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
    }
}