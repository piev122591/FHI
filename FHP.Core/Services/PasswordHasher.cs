using System;
using System.Security.Cryptography;

namespace FHP.Core.Services
{
    /// <summary>
    /// PBKDF2 password hashing — built into .NET, no external dependency. Each hash
    /// embeds its own random salt and iteration count so the work factor can be
    /// raised later without breaking old hashes.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string plainTextPassword)
        {
            if (string.IsNullOrEmpty(plainTextPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string plainTextPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(plainTextPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            string[] parts = storedHash.Split('.');
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int iterations))
                return false;

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
