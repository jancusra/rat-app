using Rat.Domain.Responses;

namespace Rat.Domain.Exceptions
{
    /// <summary>
    /// Exception for non existing table entity
    /// </summary>
    public partial class NonExistingEntityException : BaseResponseException
    {
        public NonExistingEntityException(string entityName)
            : base(new ResponseState { Code = 11003, Message = $"Entity {entityName} not exists.", HttpStatusCode = 404 })
        {
        }
    }
}
