using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rat.Domain.Entities;
using Rat.Domain.Types;

namespace Rat.Services
{
    public partial interface ILogService
    {
        /// <summary>
        /// Get all logs stored in the database
        /// </summary>
        /// <returns></returns>
        Task<IList<Log>> GetAllAsync();

        /// <summary>
        /// Insert log to the database
        /// </summary>
        /// <param name="logLevelType">log event level</param>
        /// <param name="shortMessage">the short description message</param>
        /// <param name="fullMessage">the full description message</param>
        Task InsertLogAsync(
            LogLevelType logLevelType, string shortMessage, string fullMessage);

        /// <summary>
        /// Log event information
        /// </summary>
        /// <param name="message">the short description message</param>
        /// <param name="exception">specific exception</param>
        Task InformationAsync(string message, Exception exception = null);

        /// <summary>
        /// Log event warning
        /// </summary>
        /// <param name="message">the short description message</param>
        /// <param name="exception">specific exception</param>
        Task WarningAsync(string message, Exception exception = null);

        /// <summary>
        /// Log event error
        /// </summary>
        /// <param name="message">the short description message</param>
        /// <param name="exception">specific exception</param>
        Task ErrorAsync(string message, Exception exception = null);
    }
}
