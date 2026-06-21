using Rat.Domain.EntityAttributes;

namespace Rat.Domain.Entities
{
    /// <summary>
    /// Represents database table
    /// </summary>
    public partial class Localization : TableEntity
    {
        /// <summary>
        /// Language ID binding
        /// </summary>
        [ForeignKey(nameof(Language))]
        public int LanguageId { get; set; }

        /// <summary>
        /// Localization name (also identifier)
        /// </summary>
        [NotNullableString]
        [MaxStringLength(EntityDefaults.MaxTypicalStringLength)]
        public string Name { get; set; }

        /// <summary>
        /// Localization translated value
        /// </summary>
        public string Value { get; set; }
    }
}
