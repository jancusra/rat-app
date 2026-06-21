
namespace Rat.Contracts
{
    /// <summary>
    /// Represents the model for the validation result
    /// </summary>
    public partial class ValidationEntryResult
    {
        /// <summary>
        /// Field name identifier
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Validation result report
        /// </summary>
        public string Message { get; set; }
    }
}
