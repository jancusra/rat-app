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
            CookieSecure = true;
            CookieSameSite = "Lax";
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

        /// <summary>
        /// Mark the auth cookie as Secure (only sent over HTTPS). Should stay true;
        /// it is forced on when <see cref="CookieSameSite"/> is "None".
        /// </summary>
        public bool CookieSecure { get; set; }

        /// <summary>
        /// SameSite policy for the auth cookie: "Lax" (default), "Strict" or "None".
        /// A frontend served from a different site than the API needs "None" (which
        /// additionally requires HTTPS / Secure).
        /// </summary>
        public string CookieSameSite { get; set; }
    }
}
