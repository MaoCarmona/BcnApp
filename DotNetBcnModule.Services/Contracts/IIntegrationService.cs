using NetBcnModule.Services.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBcnModule.Services.Contracts
{
    public interface IIntegrationService
    {
        /// <summary>
        /// Integrates information from different sources
        /// </summary>
        /// <param name="infoType">Type of information to integrate</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <param name="optionId">Option identifier</param>
        /// <returns>Integration result</returns>
        Task<IntegrationResult> IntegrateInfoAsync(
            string infoType,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            string optionId = null);
    }
}
