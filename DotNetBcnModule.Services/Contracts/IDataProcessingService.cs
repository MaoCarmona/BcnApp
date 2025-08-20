using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using DotNetBcnModule.Services.Contracts;

namespace DotNetBcnModule.Services.Contracts
{
    public interface IDataProcessingService
    {
        /// <summary>
        /// Converts data records to XML format
        /// </summary>
        List<string> ConvertToXml(IEnumerable<object> dataRecords, string tagName);

        /// <summary>
        /// Processes XML items through stored procedure (replicating Python logic)
        /// </summary>
        Task<bool> ProcessXmlItemsAsync(List<string> xmlItems, string userAudit, string source, string tagQuery);

        /// <summary>
        /// Processes operational information integration
        /// 
        /// Queries that use useInitialDate parameter:
        /// - "01": AORA Inventory (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// - "04": ROMSS Inventory (useInitialDate=true: dateFrom, false: dateTo+1sec)
        /// - "06": BCN Inventory Photo (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// </summary>
        Task<IntegrationResult> ProcessOperationalInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            bool useInitialDate = false);

        /// <summary>
        /// Processes consolidated information integration
        /// 
        /// Queries that use useInitialDate parameter:
        /// - "01": BCN Consolidated Inventory (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// - "06": BCN Consolidated Inventory Photo (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// </summary>
        Task<IntegrationResult> ProcessConsolidatedInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            bool useInitialDate = false);

        /// <summary>
        /// Processes balance information integration
        /// </summary>
        Task<IntegrationResult> ProcessBalanceInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo);
    }

    /// <summary>
    /// Integration result model
    /// </summary>
    public class IntegrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> XmlItems { get; set; } = new List<string>();
        public int RecordCount { get; set; }
    }
} 