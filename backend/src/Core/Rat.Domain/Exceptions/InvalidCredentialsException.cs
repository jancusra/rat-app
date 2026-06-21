using Rat.Domain.Responses;

namespace Rat.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when authentication fails (unknown user or wrong password).
    /// Uses a neutral message so it does not reveal which part of the credentials was wrong.
    /// </summary>
    public partial class InvalidCredentialsException : BaseResponseException
    {
        public InvalidCredentialsException()
            : base(new ResponseState { Code = 11005, Message = "Invalid email or password.", HttpStatusCode = 401 })
        {
        }
    }
}
