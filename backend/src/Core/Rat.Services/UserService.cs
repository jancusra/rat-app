using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rat.Contracts.Models.User;
using Rat.Domain;
using Rat.Domain.Entities;
using Rat.Domain.Options;
using Rat.Domain.Types;

namespace Rat.Services
{
    /// <summary>
    /// Methods working with user entity and other features
    /// </summary>
    public partial class UserService : IUserService
    {
        private readonly IHashingService _hashingService;
        private readonly IRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<UserOptions> _userOptions;

        public UserService(
            IHashingService hashingService,
            IRepository repository,
            IHttpContextAccessor httpContextAccessor,
            IOptions<UserOptions> userOptions)
        {
            _hashingService = hashingService;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _userOptions = userOptions;
        }

        public virtual async Task<User> GetUserByEmailAsync(string email)
            => await _repository.Table<User>().FirstOrDefaultAsync(x => x.Email == email);

        public virtual async Task<IList<User>> GetAllAsync()
            => await _repository.GetAllAsync<User>();

        public virtual async Task<bool> IsUserAdminAsync(int userId)
            => await _repository.Table<UserUserRoleMap>().FirstOrDefaultAsync(x => x.UserId == userId && x.UserRoleId == (int)RoleType.Administrators) != null;

        public virtual CurrentUserClaims GetCurrentUserClaims()
        {
            var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity.FindFirst(CustomClaimTypes.Id);
            var emailClaim = identity.FindFirst(ClaimTypes.Email);
            var isAdminClaim = identity.FindFirst(CustomClaimTypes.IsAdmin);

            return new CurrentUserClaims
            {
                Id = idClaim != null ? Convert.ToInt32(idClaim.Value) : default(int),
                Email = emailClaim != null ? emailClaim.Value : string.Empty,
                IsAdmin = isAdminClaim != null ? Convert.ToBoolean(isAdminClaim.Value) : false
            };
        }

        public virtual async Task<User> LoginUserValidationAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);

            if (user != null)
            {
                var userPassword = await GetUserPasswordByUserIdAsync(user.Id);

                if (userPassword != null)
                {
                    var hashToValidate = _hashingService.GetHashByType((HashType)userPassword.HashTypeId, 
                        password, true, userPassword.PasswordSalt);

                    if (hashToValidate == userPassword.PasswordHash)
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        public virtual async Task<bool> RegisterNewUserAsync(string email, string password, string passwordVerify)
        {
            if (password != passwordVerify)
                return false;

            var newUser = new User 
            {
                UserGuid = Guid.NewGuid(),
                Email = email,
                IsActive = true,
                CreatedUTC = DateTime.UtcNow
            };

            await _repository.InsertAsync(newUser);

            var passwordSalt = _hashingService.GenerateSalt();
            var passwordHash = _hashingService.GetHashByType(_userOptions.Value.PasswordHashing, password, true, passwordSalt);

            await _repository.InsertAsync(new UserPassword 
            {
                UserId = newUser.Id,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                HashTypeId = (int)_userOptions.Value.PasswordHashing,
                CreatedUTC = DateTime.UtcNow
            });

            return true;
        }

        /// <summary>
        /// Get database password by user ID
        /// </summary>
        /// <param name="userId">user ID</param>
        /// <returns>the password belongs to the user</returns>
        private async Task<UserPassword> GetUserPasswordByUserIdAsync(int userId)
            => await _repository.Table<UserPassword>().FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
