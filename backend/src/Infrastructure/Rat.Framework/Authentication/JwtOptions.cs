namespace Rat.Framework.Authentication
{
    /// <summary>
    /// Represents JWT options
    /// </summary>
    public partial class JwtOptions
    {
        public JwtOptions()
        {
            AuthorizationCookieKey = "Authorization";
        }

        /// <summary>
        /// Authorization cookie key
        /// </summary>
        public string AuthorizationCookieKey { get; }

        /// <summary>
        /// JWT secret key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Token expiration 
        /// </summary>
        public int ExpiryMinutes { get; set; }
    }
}
