namespace Rat.Contracts.Models.User
{
    /// <summary>
    /// Represents a simple login model
    /// </summary>
    public partial class LoginDto
    {
        /// <summary>
        /// Inserted user email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Inserted user password
        /// </summary>
        public string Password { get; set; }
    }
}
