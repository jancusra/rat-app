namespace Rat.Domain.Types
{
    /// <summary>
    /// Access type definition for the user role
    /// </summary>
    public enum AccessType
    {
        /// <summary>
        /// Full access to edit and read all settings
        /// </summary>
        FullAccess = 10,

        /// <summary>
        /// Read-only access to all settings
        /// </summary>
        ReadOnly = 20,

        /// <summary>
        /// No administration access
        /// </summary>
        NoAccess = 30
    }
}
