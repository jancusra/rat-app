using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Contracts.Models.User;
using Rat.Domain.Entities;

namespace Rat.Services
{
    public partial interface IUserService
    {
        /// <summary>
        /// Get specific user by email address
        /// </summary>
        /// <param name="email">email address</param>
        /// <returns>user entity</returns>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>
        /// Get all users stored in the database
        /// </summary>
        /// <returns>list of all user entities</returns>
        Task<IList<User>> GetAllAsync();

        /// <summary>
        /// Determine whether a user has administration rights
        /// </summary>
        /// <param name="userId">user ID</param>
        /// <returns>bool result</returns>
        Task<bool> IsUserAdminAsync(int userId);

        /// <summary>
        /// Get current logged in user data
        /// </summary>
        /// <returns>logged in user data</returns>
        CurrentUserClaims GetCurrentUserClaims();

        /// <summary>
        /// User validation before creating a login token
        /// </summary>
        /// <param name="email">inserted login email</param>
        /// <param name="password">inserted login password</param>
        /// <returns>user entity</returns>
        Task<User> LoginUserValidationAsync(string email, string password);

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="email">insetred email</param>
        /// <param name="password">inserted password</param>
        /// <param name="passwordVerify">password for verification</param>
        /// <returns>the result of whether the user was successfully created in the database</returns>
        Task<bool> RegisterNewUserAsync(string email, string password, string passwordVerify);
    }
}
