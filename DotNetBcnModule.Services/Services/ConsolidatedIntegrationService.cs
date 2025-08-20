using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Queries;
using System;
using System.Threading.Tasks;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Service for handling consolidated information integration
    /// </summary>
    public class ConsolidatedIntegrationService : IConsolidatedIntegrationService
    {
        private readonly ILoggingService _loggingService;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IQueriesService _queriesService;

        public ConsolidatedIntegrationService(
            ILoggingService loggingService,
            IDataProcessingService dataProcessingService,
            IQueriesService queriesService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _queriesService = queriesService ?? throw new ArgumentNullException(nameof(queriesService));
        }

        /// <summary>
        /// Processes consolidated information integration
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <returns>Integration result</returns>
        public async Task<IntegrationResult> ProcessConsolidatedInfoAsync(
            string optionId,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo)
        {
            var result = new IntegrationResult();

            try
            {
                if (string.IsNullOrEmpty(optionId))
                {
                    result.Success = false;
                    result.Message = "ID de opción requerido para información consolidada";
                    return result;
                }

                var infoType = optionId.Substring(0, 2);
                result = await _dataProcessingService.ProcessConsolidatedInfoAsync(
                    infoType, _queriesService, userAudit, dateFrom, dateTo);

                if (result.Success)
                {
                    _loggingService.WriteInfo($"Consolidated info processed successfully: {result.RecordCount} records");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error procesando información consolidada: {ex.Message}";
                _loggingService.WriteError($"Consolidated info error: {ex.Message}");
            }

            return result;
        }
    }
} 