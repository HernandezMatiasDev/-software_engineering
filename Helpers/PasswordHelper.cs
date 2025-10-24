// En SuMejorPeso/Models/PasswordHelper.cs
using System;
using System.Security.Cryptography;

namespace SuMejorPeso.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 10000;

        /// <summary>
        /// Crea un hash y un salt para una contraseña dada.
        /// </summary>
        /// <returns>Un tuple con el Hash (string) y el Salt (string).</returns>
        public static (string Hash, string Salt) HashPassword(string password)
        {
            // 1. Crear el Salt
            byte[] saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string saltString = Convert.ToBase64String(saltBytes);

            // 2. Crear el Hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(HashSize);
                string hashString = Convert.ToBase64String(hashBytes);
                
                return (hashString, saltString);
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash y salt almacenados.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                // 1. Convertir los strings (Base64) de vuelta a bytes
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] storedHashBytes = Convert.FromBase64String(storedHash);

                // 2. Hashear la contraseña ingresada usando el salt antiguo
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] testHashBytes = pbkdf2.GetBytes(HashSize);

                    // 3. Comparar los hashes
                    return CryptographicOperations.FixedTimeEquals(storedHashBytes, testHashBytes);
                }
            }
            catch
            {
                // Si el formato del hash/salt es inválido
                return false;
            }
        }
    }
}