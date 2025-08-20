using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Queries;
using System;
using System.Threading.Tasks;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Service for handling operational information integration
    /// </summary>
    public class OperationalIntegrationService : IOperationalIntegrationService
    {
        private readonly ILoggingService _loggingService;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IQueriesService _queriesService;

        public OperationalIntegrationService(
            ILoggingService loggingService,
            IDataProcessingService dataProcessingService,
            IQueriesService queriesService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _queriesService = queriesService ?? throw new ArgumentNullException(nameof(queriesService));
        }

        /// <summary>
        /// Processes operational information integration
        /// </summary>
        /// <param name="optionId">Option identifier</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <returns>Integration result</returns>
        public async Task<IntegrationResult> ProcessOperationalInfoAsync(
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
                    result.Message = "ID de opción requerido para información operativa";
                    return result;
                }

                var infoType = optionId.Substring(0, 2);
                result = await _dataProcessingService.ProcessOperationalInfoAsync(
                    infoType, _queriesService, userAudit, dateFrom, dateTo);

                if (result.Success)
                {
                    _loggingService.WriteInfo($"Operational info processed successfully: {result.RecordCount} records");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error procesando información operativa: {ex.Message}";
                _loggingService.WriteError($"Operational info error: {ex.Message}");
            }

            return result;
        }
    }
} 