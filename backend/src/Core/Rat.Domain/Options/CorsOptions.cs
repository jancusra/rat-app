namespace Rat.Domain.Options
{
    /// <summary>
    /// Represents Cross-Origin Resource Sharing (CORS) options.
    /// Needed when the frontend is served from a different origin than the API
    /// (e.g. the React app behind nginx on its own host/port) and authenticates
    /// with the cookie-based JWT: the browser only sends that cookie cross-origin
    /// when the API answers with an explicit allowed origin + AllowCredentials.
    /// </summary>
    public partial class CorsOptions
    {
        /// <summary>
        /// Origins allowed to call the API with credentials, e.g.
        /// "https://app.example.com". Comma- or semicolon-separated for multiple
        /// values. A wildcard ("*") is intentionally NOT supported because the
        /// CORS spec forbids combining it with credentialed requests.
        /// When empty, no CORS policy is applied (same-origin / reverse-proxy
        /// deployments need none).
        /// </summary>
        public string AllowedOrigins { get; set; }
    }
}
