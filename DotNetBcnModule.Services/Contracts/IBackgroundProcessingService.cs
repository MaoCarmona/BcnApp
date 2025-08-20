using System;
using System.Threading;
using System.Threading.Tasks;
using NetBcnModule.Services.Models;

namespace DotNetBcnModule.Services.Contracts
{
    /// <summary>
    /// Service interface for handling background data processing operations
    /// </summary>
    public interface IBackgroundProcessingService
    {
        /// <summary>
        /// Starts operational information processing in the background
        /// </summary>
        /// <param name="option">Processing option</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that represents the background operation</returns>
        Task<IntegrationResult> StartOperationalProcessingAsync(
            string option,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts consolidated information processing in the background
        /// </summary>
        /// <param name="option">Processing option</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that represents the background operation</returns>
        Task<IntegrationResult> StartConsolidatedProcessingAsync(
            string option,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts balance information processing in the background
        /// </summary>
        /// <param name="option">Processing option</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that represents the background operation</returns>
        Task<IntegrationResult> StartBalanceProcessingAsync(
            string option,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken cancellationToken = default);
    }
} 