namespace Rat.Contracts.Models.User
{
    /// <summary>
    /// Represents a model for user registration
    /// </summary>
    public partial class RegisterDto
    {
        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Verify inserted password
        /// </summary>
        public string PasswordVerify { get; set; }
    }
}
