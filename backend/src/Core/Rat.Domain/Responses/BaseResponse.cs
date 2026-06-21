namespace Rat.Domain.Responses
{
    /// <summary>
    /// Defines the basic api response model
    /// </summary>
    public partial class BaseResponse
    {
        public BaseResponse()
        {
        }

        public BaseResponse(int resultCode) => ResultCode = resultCode;

        public BaseResponse(int resultCode, string resultReason)
          : this(resultCode)
          => ResultReason = resultReason;

        public int ResultCode { get; set; }

        public string ResultReason { get; set; }
    }
}
