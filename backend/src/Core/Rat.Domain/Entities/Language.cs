using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class Language : TableEntity
    {
        /// <summary>
        /// Language name
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string Name { get; set; }

        /// <summary>
        /// Language culture code by standard
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxLanguageCultureCodeLength)]
        public string LanguageCulture { get; set; }

        /// <summary>
        /// Language two letter code by ISO 639-1
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxTwoLetterLength)]
        public string TwoLetterCode { get; set; }

        /// <summary>
        /// Is language active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Language display order
        /// </summary>
        public int ItemOrder { get; set; }
    }
}
