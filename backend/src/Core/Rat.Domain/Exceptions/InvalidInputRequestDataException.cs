using Rat.Domain.Responses;

namespace Rat.Domain.Exceptions
{
    /// <summary>
    /// Exception for inserting invalid data into the input
    /// </summary>
    public partial class InvalidInputRequestDataException : BaseResponseException
    {
        public InvalidInputRequestDataException()
            : base(new ResponseState { Code = 11001, Message = "Input request data are not valid.", HttpStatusCode = 400 })
        {
        }
    }
}
