namespace Rat.Domain.EntityTypes
{
    /// <summary>
    /// Specify entity with a name (usually required)
    /// </summary>
    public partial interface INamed
    {
        public string Name { get; set; }
    }
}
