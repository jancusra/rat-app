namespace Rat.Domain.Types
{
    /// <summary>
    /// Custom entity entry types (not like string, bool, int etc.)
    /// </summary>
    public enum CustomEntityEntryType
    {
        /// <summary>
        /// Enum custom type
        /// </summary>
        Enum,

        /// <summary>
        /// Mapped multiselect (with a relationship to another entity)
        /// </summary>
        MappedMultiSelect
    }
}
