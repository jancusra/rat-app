namespace Rat.Contracts.Common
{
    /// <summary>
    /// Represents the extended base DTO entry for the entity
    /// </summary>
    public partial class EntityEntryDto : BaseEntryDto
    {
        /// <summary>
        /// Specific entity value
        /// </summary>
        public dynamic Value { get; set; }
    }
}
