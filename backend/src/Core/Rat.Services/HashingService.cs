using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Rat.Domain.Types;

namespace Rat.Services
{
    /// <summary>
    /// Methods for providing secure hashing algorithms
    /// </summary>
    public partial class HashingService : IHashingService
    {
        public virtual string GenerateSalt(int size = 128)
        {
            byte[] salt = new byte[size / 8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        public virtual string GetHashByType(HashType hashType, string inputString, bool withSalt = false, string salt = null)
        {
            switch (hashType)
            {
                case HashType.Pbkdf2SHA512:
                    return GetPbkdf2Hash(inputString, salt);
                default:
                    return GetHashByAlgorithm(hashType, inputString, withSalt, salt);
            }
        }

        /// <summary>
        /// Create a hash according to a specific algorithm
        /// </summary>
        /// <param name="hashType">the type of hash</param>
        /// <param name="inputString">input string (usually password)</param>
        /// <param name="withSalt">should be hash prepared with a salt</param>
        /// <param name="salt">the hash salt</param>
        /// <returns>final hash output</returns>
        private string GetHashByAlgorithm(HashType hashType, string inputString, bool withSalt = false, string salt = null)
        {
            if (withSalt)
            {
                if (string.IsNullOrEmpty(salt))
                {
                    salt = GenerateSalt();
                }

                inputString = $"{inputString}-{salt}";
            }

            var hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(hashType.ToString());
            var hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Get Pbkdf2Hash from input string
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <param name="salt">the hash salt</param>
        /// <returns>GetPbkdf2Hash hashing output</returns>
        private string GetPbkdf2Hash(string inputString, string salt = null)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inputString,
                salt: string.IsNullOrEmpty(salt) ? Encoding.UTF8.GetBytes(GenerateSalt()) : Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 256 / 8));
        }
    }
}
