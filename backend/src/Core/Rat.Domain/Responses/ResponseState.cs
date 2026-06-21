namespace Rat.Domain.Responses
{
    /// <summary>
    /// Defines the status of the exception response
    /// </summary>
    public struct ResponseState
    {
        /// <summary>
        /// Application code for a response state
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// HTTP status code defined by a standard
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// Exception reason/message
        /// </summary>
        public string Message { get; set; }
    }
}
