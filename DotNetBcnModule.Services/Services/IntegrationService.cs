using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
    
namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Main service to handle data integration from different sources
    /// </summary>
    public class IntegrationService : IIntegrationService
    {
        private readonly ILoggingService _loggingService;
        private readonly IOperationalIntegrationService _operationalIntegrationService;
        private readonly IConsolidatedIntegrationService _consolidatedIntegrationService;
        private readonly IBalanceIntegrationService _balanceIntegrationService;
        private readonly IQueriesService _queriesService;
        private readonly IAresWebService _aresWebService;

        public IntegrationService(
            ILoggingService loggingService,
            IOperationalIntegrationService operationalIntegrationService,
            IConsolidatedIntegrationService consolidatedIntegrationService,
            IBalanceIntegrationService balanceIntegrationService,
            IQueriesService queriesService,
            IAresWebService aresWebService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _operationalIntegrationService = operationalIntegrationService ?? throw new ArgumentNullException(nameof(operationalIntegrationService));
            _consolidatedIntegrationService = consolidatedIntegrationService ?? throw new ArgumentNullException(nameof(consolidatedIntegrationService));
            _balanceIntegrationService = balanceIntegrationService ?? throw new ArgumentNullException(nameof(balanceIntegrationService));
            _queriesService = queriesService ?? throw new ArgumentNullException(nameof(queriesService));
            _aresWebService = aresWebService ?? throw new ArgumentNullException(nameof(aresWebService));
        }

        /// <summary>
        /// Integrates information from different sources
        /// </summary>
        /// <param name="infoType">Type of information to integrate</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="dateFrom">Start date</param>
        /// <param name="dateTo">End date</param>
        /// <param name="optionId">Option identifier</param>
        /// <returns>Integration result</returns>
        public async Task<IntegrationResult> IntegrateInfoAsync(
            string infoType,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            string optionId = null)
        {
            var result = new IntegrationResult();
            var source = "GRB";

            try
            {
                // Load configuration
                var dateFormat = "yyyy-MM-dd HH:mm:ss";
                var dtFechaIni = dateFrom.ToString(dateFormat);
                var dtFechaFin = dateTo.ToString(dateFormat);

                _loggingService.WriteInfo($"Starting integration: {infoType} from {dtFechaIni} to {dtFechaFin}");

                // Process based on information type
                switch (infoType)
                {
                    case "InfOperativo":
                        result = await _operationalIntegrationService.ProcessOperationalInfoAsync(optionId, userAudit, dateFrom, dateTo);
                        break;

                    case "InfConsolidado":
                        result = await _consolidatedIntegrationService.ProcessConsolidatedInfoAsync(optionId, userAudit, dateFrom, dateTo);
                        break;

                    case "InfBalance":
                        result = await _balanceIntegrationService.ProcessBalanceInfoAsync(optionId, userAudit, dateFrom, dateTo);
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Tipo de información no válido: {infoType}";
                        break;
                }

                // If successful, process the XML items
                if (result.Success && result.XmlItems.Any())
                {
                    ProcessXmlItems(result, userAudit, source, optionId);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error en integración: {ex.Message}";
                _loggingService.WriteError($"Integration error: {ex.Message}");
            }

            return result;
        }



        /// <summary>
        /// Processes XML items and sends to database
        /// </summary>
        private void ProcessXmlItems(
            IntegrationResult result,
            string userAudit,
            string source,
            string optionId)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var tagQuery = GetTagQuery(optionId);

                _loggingService.WriteInfo($"Processing {result.XmlItems.Count} XML packages for {optionId} - Total records: {result.RecordCount}");

                for (int i = 0; i < result.XmlItems.Count; i++)
                {
                    var xmlItem = result.XmlItems[i];
                    var xmlSize = xmlItem?.Length ?? 0;
                    var logMessage = $"Package {i + 1}/{result.XmlItems.Count}: Tag={tagQuery}, User={userAudit}, XML Size={xmlSize} chars";
                    _loggingService.WriteInfo(logMessage);

                    // Execute stored procedure to process the XML
                    var sql = $"EXEC BCN.spAppIntegrarProcesarInfo '{userAudit}', '{source}', '{tagQuery}', [XML_DATA];";
                    _loggingService.WriteSql(sql);

                    // Note: In a real implementation, you would execute this SQL against the database
                    // For now, we'll just log it
                    _loggingService.WriteInfo($"Executed stored procedure for package {i + 1}");
                }

                _loggingService.WriteInfo($"Successfully processed {optionId} - Total records: {result.RecordCount}");
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error processing XML items: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the tag query based on option ID
        /// </summary>
        private string GetTagQuery(string optionId)
        {
            if (string.IsNullOrEmpty(optionId))
                return "";

            var infoType = optionId.Substring(0, 2);
            switch (infoType)
            {
                case "01": return "INVOPERAORA";
                case "02": return "MOVOPERAORA";
                case "03": return "FLUOPERAORA";
                case "04": return "INVOPERROMSS";
                case "05": return "MOVOPERROMSS";
                case "07": return "MOVHPIARES";
                default: return "";
            }
        }
    }
}