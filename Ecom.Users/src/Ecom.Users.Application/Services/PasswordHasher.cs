using Ecom.Users.Domain.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Services.Autofac.Attributes;
using System.Security.Cryptography;

namespace Ecom.Users.Application.Services;
[TransientService]

public class PasswordHasher : IPasswordHasher
{
    // PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 100,000 iterations
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 100000;
    private const char Delimiter = ':';

    public string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive a 256-bit subkey (use HMAC-SHA256)
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        // Format: {iterations}:{salt}:{hash}
        return $"{Iterations}{Delimiter}{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // Extract iteration count, salt and hash from the hashed password
        string[] parts = hashedPassword.Split(Delimiter);
        if (parts.Length != 3)
        {
            return false; // Invalid hash format
        }

        if (!int.TryParse(parts[0], out int iterations))
        {
            return false; // Invalid iteration count
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] hash = Convert.FromBase64String(parts[2]);

            // Derive a subkey from the password and compare it to the stored hash
            byte[] testHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: hash.Length);

            // Compare the hashes in constant time to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false; // Invalid salt or hash
        }
    }
}