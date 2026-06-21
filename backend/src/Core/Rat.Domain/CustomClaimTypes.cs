namespace Rat.Domain
{
    /// <summary>
    /// Custom defined user claim types
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// User database identifier
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// Determine whether the user has administrative rights
        /// </summary>
        public const string IsAdmin = "isAdmin";
    }
}
