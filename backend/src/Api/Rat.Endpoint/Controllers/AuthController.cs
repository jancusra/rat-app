using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rat.Contracts.Models.User;
using Rat.Domain.Exceptions;
using Rat.Framework.Authentication;

namespace Test.Endpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public partial class AuthController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;

        public AuthController(
            ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Create JWT token / authenticate user by email and password
        /// </summary>
        /// <param name="model">inserted email and password</param>
        /// <returns>exception or OK result</returns>
        /// <exception cref="InvalidInputRequestDataException"></exception>
        [HttpPost]
        public virtual async Task<IActionResult> Authenticate([FromBody]LoginDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
            {
                throw new InvalidInputRequestDataException();
            }

            await _tokenManager.CreateTokenAsync(model.Email, model.Password);

            return Ok();
        }

        /// <summary>
        /// Logout the currently logged in user
        /// </summary>
        /// <returns>OK result</returns>
        [HttpPost]
        public virtual async Task<IActionResult> Logout()
        {
            await _tokenManager.DeactivateCurrentTokenAsync();

            return Ok();
        }
    }
}
