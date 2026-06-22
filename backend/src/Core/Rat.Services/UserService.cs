using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
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
            => await _repository.Table<User>().FirstOrDefaultAsync(x => x.Email == email && !x.Deleted);

        public virtual async Task<IList<User>> GetAllAsync()
            => await _repository.GetAllAsync<User>();

        public virtual async Task<bool> IsUserAdminAsync(int userId)
            => await _repository.Table<UserUserRoleMap>().FirstOrDefaultAsync(x => x.UserId == userId && x.UserRoleId == (int)RoleType.Administrators) != null;

        public virtual async Task<AccessType> GetCurrentUserAdministrationAccessAsync()
        {
            var currentUser = GetCurrentUserClaims();

            if (currentUser.Id <= default(int))
            {
                return AccessType.NoAccess;
            }

            return await GetAdministrationAccessByUserIdAsync(currentUser.Id);
        }

        public virtual async Task<AccessType> GetAdministrationAccessByUserIdAsync(int userId)
        {
            var accessTypeIds = await (
                from map in _repository.Table<UserUserRoleMap>()
                join role in _repository.Table<UserRole>() on map.UserRoleId equals role.Id
                where map.UserId == userId && role.IsActive
                select role.DefaultAccessTypeId).ToListAsync();

            if (!accessTypeIds.Any())
            {
                return AccessType.NoAccess;
            }

            // access types are ordered by permissiveness (FullAccess = 10 < ReadOnly = 20 < NoAccess = 30),
            // so the lowest value is the most permissive access the user has across all active roles
            return (AccessType)accessTypeIds.Min();
        }

        public virtual CurrentUserClaims GetCurrentUserClaims()
        {
            // HttpContext is null outside of a request (e.g. during startup migrations or
            // background logging); return empty claims instead of throwing.
            if (!(_httpContextAccessor.HttpContext?.User?.Identity is ClaimsIdentity identity))
            {
                return new CurrentUserClaims { Email = string.Empty };
            }

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

            // blocked (inactive) users cannot authenticate, even with a valid password
            if (user != null && user.IsActive)
            {
                var userPassword = await GetUserPasswordByUserIdAsync(user.Id);

                if (userPassword != null)
                {
                    var hashToValidate = _hashingService.GetHashByType((HashType)userPassword.HashTypeId,
                        password, true, userPassword.PasswordSalt);

                    if (FixedTimeEquals(hashToValidate, userPassword.PasswordHash))
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

            var passwordSalt = _hashingService.GenerateSalt();
            var passwordHash = _hashingService.GetHashByType(_userOptions.Value.PasswordHashing, password, true, passwordSalt);

            // User and its password are inserted together so a failure on the second insert
            // cannot leave an orphaned user without a password.
            await _repository.ExecuteInTransactionAsync(async () =>
            {
                var newUser = new User
                {
                    UserGuid = Guid.NewGuid(),
                    Email = email,
                    IsActive = true,
                    CreatedUTC = DateTime.UtcNow
                };

                await _repository.InsertAsync(newUser);

                await _repository.InsertAsync(new UserPassword
                {
                    UserId = newUser.Id,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    HashTypeId = (int)_userOptions.Value.PasswordHashing,
                    CreatedUTC = DateTime.UtcNow
                });

                await _repository.InsertAsync(new UserUserRoleMap
                {
                    UserId = newUser.Id,
                    UserRoleId = (int)RoleType.RegisteredUsers
                });
            });

            return true;
        }

        /// <summary>
        /// Get database password by user ID
        /// </summary>
        /// <param name="userId">user ID</param>
        /// <returns>the password belongs to the user</returns>
        private async Task<UserPassword> GetUserPasswordByUserIdAsync(int userId)
            => await _repository.Table<UserPassword>()
                .OrderByDescending(x => x.CreatedUTC)
                .FirstOrDefaultAsync(x => x.UserId == userId);

        /// <summary>
        /// Compare two hash strings in constant time to avoid leaking information through
        /// timing differences (mitigates timing attacks on password verification).
        /// </summary>
        /// <param name="left">computed hash</param>
        /// <param name="right">stored hash</param>
        /// <returns>true when both hashes are equal</returns>
        private static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
        }
    }
}
