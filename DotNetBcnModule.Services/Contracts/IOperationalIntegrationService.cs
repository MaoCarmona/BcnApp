using NetBcnModule.Services.Services;
using System;
using System.Threading.Tasks;

namespace DotNetBcnModule.Services.Contracts
{
    /// <summary>
    /// Service interface for handling operational information integration
    /// </summary>
    public interface IOperationalIntegrationService
    {
        /// <summary>
        /// Processes operational information integration
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <returns>Integration result</returns>
        Task<IntegrationResult> ProcessOperationalInfoAsync(
            string optionId,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo);
    }
} 