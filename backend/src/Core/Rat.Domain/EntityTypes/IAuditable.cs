using System;

namespace Rat.Domain.EntityTypes
{
    /// <summary>
    /// The auditable entity has a creation and modification date
    /// </summary>
    public partial interface IAuditable
    {
        DateTime CreatedUTC { get; set; }

        DateTime? ModifiedUTC { get; set; }
    }
}
