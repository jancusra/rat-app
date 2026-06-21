namespace Rat.Domain.EntityTypes
{
    /// <summary>
    /// Specify a system entry (system entry usually cannot be deleted)
    /// </summary>
    public partial interface ICanBeSystem
    {
        public bool IsSystemEntry { get; set; }
    }
}
