namespace Rat.Domain.EntityTypes
{
    /// <summary>
    /// Specify a system entry (system entry usually cann't be deleted)
    /// </summary>
    public partial interface ICanBeSystem
    {
        public bool IsSystemEntry { get; set; }
    }
}
