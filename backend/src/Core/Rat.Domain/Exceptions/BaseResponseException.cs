using System;
using Rat.Domain.Responses;

namespace Rat.Domain.Exceptions
{
    /// <summary>
    /// Core base api response exception
    /// </summary>
    public partial class BaseResponseException : Exception
    {
        public BaseResponseException(ResponseState responseState)
            : base(responseState.Message)
        {
            ResponseState = responseState;
        }

        public ResponseState ResponseState { get; }
    }
}
