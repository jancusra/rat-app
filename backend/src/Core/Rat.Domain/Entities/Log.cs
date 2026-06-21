using System;
using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    [CommonAccess]
    public partial class Log : TableEntity
    {
        /// <summary>
        /// Log level type
        /// </summary>
        public int LogLevelTypeId { get; set; }

        /// <summary>
        /// Short message
        /// </summary>
        public string ShortMessage { get; set; }

        /// <summary>
        /// Full log message
        /// </summary>
        public string FullMessage { get; set; }

        /// <summary>
        /// Binding to user if logged in
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Web address URL, where log event occurs
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxUrlLength)]
        public string PathUrl { get; set; }

        /// <summary>
        /// Web address URL as referrer, where log event occurs
        /// </summary>
        [MaxStringLength(EntityDefaults.MaxUrlLength)]
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Log event created at (UTC)
        /// </summary>
        public DateTime CreatedUTC { get; set; }
    }
}
