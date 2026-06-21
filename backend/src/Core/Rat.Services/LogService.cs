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

            // HttpContext is null when logging happens outside a request (e.g. startup migrations).
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new Log
            {
                LogLevelTypeId = (int)logLevelType,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                UserId = currentUser.Id > default(int) ? currentUser.Id : null,
                PathUrl = httpContext?.Request.Path.Value,
                ReferrerUrl = httpContext?.Request.Headers[HeaderNames.Referer].ToString(),
                CreatedUTC = DateTime.UtcNow
            };

            await _repository.InsertAsync(log);
        }

        public virtual async Task InformationAsync(string message, Exception exception = null)
            => await LogAsync(LogLevelType.Information, message, exception);

        public virtual async Task WarningAsync(string message, Exception exception = null)
            => await LogAsync(LogLevelType.Warning, message, exception);

        public virtual async Task ErrorAsync(string message, Exception exception = null)
            => await LogAsync(LogLevelType.Error, message, exception);

        /// <summary>
        /// Shared logging entry point; skips thread abort exceptions (raised on request teardown).
        /// </summary>
        /// <param name="logLevelType">log event level</param>
        /// <param name="message">the short description message</param>
        /// <param name="exception">specific exception</param>
        private async Task LogAsync(LogLevelType logLevelType, string message, Exception exception)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            await InsertLogAsync(logLevelType, message, exception?.ToString() ?? string.Empty);
        }
    }
}
