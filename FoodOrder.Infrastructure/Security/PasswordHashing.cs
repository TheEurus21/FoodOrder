using System.Security.Cryptography;
using System.Text;

namespace FoodOrder.Halpers
{
    public static class PasswordHashing
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            var combined = Encoding.UTF8.GetBytes(password + salt);

            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(combined);

            return (Convert.ToBase64String(hashBytes), salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var combined = Encoding.UTF8.GetBytes(password + storedSalt);
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(combined);

            return Convert.ToBase64String(hashBytes) == storedHash;
        }
    }
}
