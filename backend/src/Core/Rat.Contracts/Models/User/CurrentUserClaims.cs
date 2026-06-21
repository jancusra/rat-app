namespace Rat.Contracts.Models.User
{
    /// <summary>
    /// Represents data about the currently logged in user
    /// </summary>
    public partial class CurrentUserClaims
    {
        /// <summary>
        /// User database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Has current user administrator rights
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}
