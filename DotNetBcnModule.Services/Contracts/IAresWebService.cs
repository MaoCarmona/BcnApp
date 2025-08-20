using NetBcnModule.Services.Models;
using NetBcnModule.Services.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBcnModule.Services.Contracts
{
    public interface IAresWebService
    {
        /// <summary>
        /// Sends data to ARES web service
        /// </summary>
        /// <param name="payload">Data payload to send</param>
        /// <param name="endpoint">ARES endpoint URL</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="transactionOrigin">Transaction origin identifier</param>
        /// <returns>Response result</returns>
        Task<AresResponse> SendToAresAsync(object payload, string endpoint, string username, string password, string transactionOrigin);

        /// <summary>
        /// Sends costs data to ARES
        /// </summary>
        /// <param name="costsData">Costs data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        Task<List<AresResponse>> SendCostsToAresAsync(List<AresCostPayload> costsData, string userAudit);

        /// <summary>
        /// Sends inventory data to ARES
        /// </summary>
        /// <param name="inventoryData">Inventory data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        Task<List<AresResponse>> SendInventoryToAresAsync(List<AresInventoryPayload> inventoryData, string userAudit);

        /// <summary>
        /// Sends logistic movement data to ARES
        /// </summary>
        /// <param name="movementData">Movement data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        Task<List<AresResponse>> SendLogisticMovementsToAresAsync(List<AresMovementPayload> movementData, string userAudit);
    }
} 