using Rat.Domain.Types;

namespace Rat.Services
{
    public partial interface IHashingService
    {
        /// <summary>
        /// Generate hash salt by random number generator
        /// </summary>
        /// <param name="size">size of the salt (default - 128)</param>
        /// <returns>final hash salt</returns>
        string GenerateSalt(int size = 128);

        /// <summary>
        /// Prepare a hash according to the selected type
        /// </summary>
        /// <param name="hashType">the type of hash</param>
        /// <param name="inputString">input string (usually password)</param>
        /// <param name="withSalt">should be hash prepared with a salt</param>
        /// <param name="salt">the hash salt</param>
        /// <returns>final hash output</returns>
        string GetHashByType(HashType hashType, string inputString, bool withSalt = false, string salt = null);
    }
}
