namespace Rat.Contracts.Common
{
    /// <summary>
    /// Represents additional column meta data
    /// </summary>
    public partial class ColumnMetadata : BaseEntryDto
    {
        /// <summary>
        /// Column width in [px]
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Column flex value
        /// </summary>
        public decimal Flex { get; set; }
    }
}
