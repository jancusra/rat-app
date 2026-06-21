using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Rat.Domain;
using Rat.Domain.Entities;
using Rat.Domain.Types;

namespace Rat.Services
{
    /// <summary>
    /// Methods working with log entity and other features
    /// </summary>
    public partial class LogService : ILogService
    {
        private readonly IRepository _repository;

        private readonly IUserService _userService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(
            IRepository repository,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual async Task<IList<Log>> GetAllAsync()
            => await _repository.GetAllAsync<Log>();

        public virtual async Task InsertLogAsync(
            LogLevelType logLevelType, string shortMessage, string fullMessage)
        {
            var currentUser = _userService.GetCurrentUserClaims();

            var log = new Log
            {
                LogLevelTypeId = (int)logLevelType,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                UserId = currentUser.Id > default(int) ? currentUser.Id : null,
                PathUrl = _httpContextAccessor.HttpContext.Request.Path,
                ReferrerUrl = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer],
                CreatedUTC = DateTime.UtcNow
            };

            await _repository.InsertAsync(log);
        }

        public virtual async Task InformationAsync(string message, Exception exception = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            await InsertLogAsync(LogLevelType.Information, message, exception?.ToString() ?? string.Empty);
        }

        public virtual async Task WarningAsync(string message, Exception exception = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            await InsertLogAsync(LogLevelType.Warning, message, exception?.ToString() ?? string.Empty);
        }

        public virtual async Task ErrorAsync(string message, Exception exception = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            await InsertLogAsync(LogLevelType.Error, message, exception?.ToString() ?? string.Empty);
        }
    }
}
