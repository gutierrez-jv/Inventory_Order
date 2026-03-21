using System.Security.Cryptography;
using System.Text;

namespace Inventory_Order.Helpers
{
    public class SecurityHelper
    {
        private const int saltSize = 16; // 128 bits
        private const int hashSize = 32; // 256 bits
        private const int iteration = 10000;


        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: iteration,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: hashSize
            );

            byte[] hashBytes = new byte[saltSize + hashSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize);
            Array.Copy(hash, 0, hashBytes, saltSize, hashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                iterations: iteration,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: hashSize
            );

            return CryptographicOperations.FixedTimeEquals(
                hash,
                hashBytes.AsSpan(saltSize, hashSize)
            );
        }
    }
}