using Rat.Domain.Responses;

namespace Rat.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when the current user does not have sufficient
    /// administration access to perform an operation on a common entity
    /// </summary>
    public partial class AccessDeniedException : BaseResponseException
    {
        public AccessDeniedException(string entityName)
            : base(new ResponseState { Code = 11004, Message = $"Access to entity {entityName} is denied.", HttpStatusCode = 403 })
        {
        }
    }
}
