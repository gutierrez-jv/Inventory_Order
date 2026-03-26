using System.Security.Cryptography; // Provides cryptographic functions (hashing, random bytes)
using System.Text;

namespace Inventory_Order.Helpers
{
    // This helper class provides methods for securely hashing passwords and verifying them.
    public class SecurityHelper
    {
        // Size of the random salt (adds randomness to password hashing)
        private const int saltSize = 16; // 128 bits

        // Size of the generated hash (final password hash length)
        private const int hashSize = 32; // 256 bits

        // Number of iterations for hashing (higher = more secure, slower)
        private const int iteration = 10000;

        // Converts a plain password into a secure hashed string
        public static string HashPassword(string password)
        {
            // Generate a random salt (prevents identical passwords from having same hash)
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

            // Generate hash using PBKDF2 algorithm with SHA256
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password, // user password
                salt: salt, // random salt
                iterations: iteration, // number of hash iterations
                hashAlgorithm: HashAlgorithmName.SHA256, // hashing algorithm
                outputLength: hashSize // final hash length
            );

            // Combine salt + hash into one byte array
            byte[] hashBytes = new byte[saltSize + hashSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize); // copy salt into first part
            Array.Copy(hash, 0, hashBytes, saltSize, hashSize); // copy hash after salt

            // Convert combined bytes into a Base64 string for storage in database
            return Convert.ToBase64String(hashBytes);
        }

        // Checks if entered password matches the stored hashed password
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Convert stored Base64 hash back to byte array
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract the original salt from the stored hash
            byte[] salt = new byte[saltSize];
            Array.Copy(hashBytes, 0, salt, 0, saltSize);

            // Re-hash the entered password using the same salt and settings
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: enteredPassword, // password entered by user
                salt: salt, // original salt
                iterations: iteration,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: hashSize
            );

            // Compare the newly generated hash with the stored hash securely
            return CryptographicOperations.FixedTimeEquals(
                hash, // newly generated hash
                hashBytes.AsSpan(saltSize, hashSize) // stored hash portion
            );
        }
    }
}