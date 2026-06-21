using Rat.Domain.Types;

namespace Rat.Domain.Options
{
    /// <summary>
    /// Represents user option
    /// </summary>
    public partial class UserOptions
    {
        /// <summary>
        /// Configured password hashing for users
        /// </summary>
        public HashType PasswordHashing { get; set; }
    }
}
