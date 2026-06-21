namespace Rat.Contracts.Models.Entity
{
    /// <summary>
    /// Represents a model for the removal of a common entity
    /// </summary>
    public partial class DeleteEntityDto
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Entity name
        /// </summary>
        public string EntityName { get; set; }
    }
}
