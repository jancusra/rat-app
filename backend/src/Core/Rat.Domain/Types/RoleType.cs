namespace Rat.Domain.Types
{
    /// <summary>
    /// Defines possible user roles
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Administrators (full access)
        /// </summary>
        Administrators = 1,

        /// <summary>
        /// Moderators (almost full access)
        /// </summary>
        Moderators = 2,

        /// <summary>
        /// Read-only access (settings can't be modified)
        /// </summary>
        FullReadOnlyAccess = 3,

        /// <summary>
        /// Registered users (without access to administration)
        /// </summary>
        RegisteredUsers = 4,

        /// <summary>
        /// Web visitors (not registered users)
        /// </summary>
        Visitors = 5
    }
}
