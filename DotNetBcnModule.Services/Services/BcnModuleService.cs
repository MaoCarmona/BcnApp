using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Comprehensive service implementation for all BCN Module operations
    /// </summary>
    public class BcnModuleService : IBcnModuleService
    {
        private readonly IQueriesService _queriesService;
        private readonly IIntegrationService _integrationService;
        private readonly ILoggingService _loggingService;
        private readonly IAresWebService _aresWebService;

        public BcnModuleService(
            IQueriesService queriesService,
            IIntegrationService integrationService,
            ILoggingService loggingService,
            IAresWebService aresWebService)
        {
            _queriesService = queriesService ?? throw new ArgumentNullException(nameof(queriesService));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _aresWebService = aresWebService ?? throw new ArgumentNullException(nameof(aresWebService));
        }

        #region Integrar Información (Operational Integration)

        public async Task<QueryResult> GetAoraInventoryAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetAoraInventoryAsync(
                    consultaIni ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAoraInventoryAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetAoraMovementsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetAoraMovementsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAoraMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetAoraFlowsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetAoraFlowsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAoraFlowsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetRomssInventoryAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetRomssInventoryAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetRomssInventoryAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetRomssMovementsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetRomssMovementsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetRomssMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnInventoryPhotoAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetBcnInventoryPhotoAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnInventoryPhotoAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnMovementsAsync(DateTime? consultaIni, DateTime? consultaFin, string filtroMovimiento = "")
        {
            try
            {
                var data = await _queriesService.GetBcnMovementsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today, 
                    filtroMovimiento);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetWsCostsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetWsCostsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetWsCostsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetWsLogisticMovementsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetWsLogisticMovementsAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today,
                    "");
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetWsLogisticMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetHpiMovementsAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetHpiMovementsAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetHpiMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        #endregion

        #region Consolidar Información (Consolidation)

        public async Task<QueryResult> GetBcnInventoryAsync(DateTime? consultaIni, int idCaso = 4)
        {
            try
            {
                var data = await _queriesService.GetBcnInventoryAsync(consultaIni ?? DateTime.Today, idCaso);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnInventoryAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnConsolidatedInventoryAsync(DateTime? consultaIni, DateTime? consultaFin, int idCaso = 4)
        {
            try
            {
                var data = await _queriesService.GetBcnInventoryAsync(consultaIni ?? DateTime.Today, idCaso);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnConsolidatedInventoryAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnConsolidatedInventoryBalanceAsync(DateTime? consultaIni, int idCaso = 5)
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedInventoryBalanceAsync(consultaIni ?? DateTime.Today, idCaso);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnConsolidatedInventoryBalanceAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnConsolidatedMovementsAsync(DateTime? consultaIni, DateTime? consultaFin, string filtroMovimiento = "", int idCaso = 5)
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedMovementsAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today, filtroMovimiento, idCaso);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnConsolidatedMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnBalanceAlmacenAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "ALMACEN PRODUCTO")
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedBalanceAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today, 
                    tpMovimiento);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnBalanceAlmacenAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnBalancePoolAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "POOL PRODUCTO")
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedBalanceAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today, 
                    tpMovimiento);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnBalancePoolAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnBalanceUnidadProcesoAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "UNIDAD DE PROCESO")
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedBalanceAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today, 
                    tpMovimiento);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnBalanceUnidadProcesoAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnConsolidatedInventoryPhotoAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetBcnConsolidatedInventoryPhotoAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnConsolidatedInventoryPhotoAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        #endregion

        #region Transformación Logística (Logistic Transformation)

        public async Task<QueryResult> GetLogisticMovementsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetWsLogisticMovementsAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today, "");
                var dataList = data.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i].Item = i + 1;
                }
                return ConvertToQueryResult(dataList);
            }   
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetLogisticMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetCostMovementsAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetWsCostsAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);
                var dataList = data.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i].Item = i + 1;
                }
                return ConvertToQueryResult(dataList);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetCostMovementsAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBalanceGrbCelo2000Async(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetLogisticBalanceAsync("2000", consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);
                
                var dataList = data.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i].Item = i + 1;
                }
                return ConvertToQueryResult(dataList);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBalanceGrbCelo2000Async: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBalanceReexpidoCelo3501Async(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetLogisticBalanceAsync("3501", consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);

                var dataList = data.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i].Item = i + 1;
                }
                return ConvertToQueryResult(dataList);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBalanceReexpidoCelo3501Async: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBalanceImpalaCelo4130Async(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetLogisticBalanceAsync("4130", consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);

                var dataList = data.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    dataList[i].Item = i + 1;
                }
                return ConvertToQueryResult(dataList);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBalanceImpalaCelo4130Async: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        #endregion

        #region Envío BCN WS-ARES (ARES Web Service)

        public async Task<QueryResult> GetAresLogisticInventoryAsync(List<AresInventoryPayload> inventoryData, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                // Get inventory data to send to ARES
                if (inventoryData == null || !inventoryData.Any())
                {
                    return new QueryResult { Message = "No hay datos de inventario logístico para enviar a ARES" };
                }

                // Send data to ARES
                var responses = await _aresWebService.SendInventoryToAresAsync(inventoryData, "AdminBCN");
                
                // Create result with ARES responses
                var result = new QueryResult();
                result.Columns = new List<string> { "ID", "Success", "StatusCode", "Message" };
                
                foreach (var response in responses)
                {
                    result.Data.Add(new Dictionary<string, object>
                    {
                        ["ID"] = $"ARES_Inventory_{DateTime.Now:yyyyMMddHHmmss}",
                        ["Success"] = response.Success ? "Sí" : "No",
                        ["StatusCode"] = response.StatusCode,
                        ["Message"] = response.Message
                    });
                }

                var successCount = responses.Count(r => r.Success);
                var totalCount = responses.Count;
                
                result.Message = $"Enviados {successCount} de {totalCount} registros de inventario logístico a ARES";
                return result;
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("GetAresLogisticInventoryAsync operation was cancelled by user");
                return new QueryResult { Message = "Operación cancelada por el usuario" };
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAresLogisticInventoryAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetAresLogisticMovementAsync(List<AresMovementPayload> movementData, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                if(movementData == null || !movementData.Any())
                {
                    return new QueryResult { Message = "No hay datos de movimientos logísticos para enviar a ARES" };
                }

                // Send data to ARES
                var responses = await _aresWebService.SendLogisticMovementsToAresAsync(movementData, "AdminBCN");
                
                // Create result with ARES responses
                var result = new QueryResult();
                result.Columns = new List<string> { "ID", "Success", "StatusCode", "Message" };
                
                foreach (var response in responses)
                {
                    result.Data.Add(new Dictionary<string, object>
                    {
                        ["ID"] = $"ARES_Movement_{DateTime.Now:yyyyMMddHHmmss}",
                        ["Success"] = response.Success ? "Sí" : "No",
                        ["StatusCode"] = response.StatusCode,
                        ["Message"] = response.Message
                    });
                }

                var successCount = responses.Count(r => r.Success);
                var totalCount = responses.Count;
                
                result.Message = $"Enviados {successCount} de {totalCount} registros de movimientos logísticos a ARES";
                return result;
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("GetAresLogisticMovementAsync operation was cancelled by user");
                return new QueryResult { Message = "Operación cancelada por el usuario" };
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAresLogisticMovementAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetAresCostMovementAsync(List<AresCostPayload> costData, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                // Get cost data to send to ARES
                if (costData == null || !costData.Any())
                {
                    return new QueryResult { Message = "No hay datos de costos para enviar a ARES" };
                }

                // Send data to ARES
                var responses = await _aresWebService.SendCostsToAresAsync(costData, "AdminBCN");
                
                // Create result with ARES responses
                var result = new QueryResult();
                result.Columns = new List<string> { "ID", "Success", "StatusCode", "Message" };
                
                foreach (var response in responses)
                {
                    result.Data.Add(new Dictionary<string, object>
                    {
                        ["ID"] = $"ARES_Cost_{DateTime.Now:yyyyMMddHHmmss}",
                        ["Success"] = response.Success ? "Sí" : "No",
                        ["StatusCode"] = response.StatusCode,
                        ["Message"] = response.Message
                    });
                }

                var successCount = responses.Count(r => r.Success);
                var totalCount = responses.Count;
                
                result.Message = $"Enviados {successCount} de {totalCount} registros de costos a ARES";
                return result;
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("GetAresCostMovementAsync operation was cancelled by user");
                return new QueryResult { Message = "Operación cancelada por el usuario" };
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAresCostMovementAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        #endregion

        #region ARES Processing Review

        /// <summary>
        /// Get ARES Logistic Movement Processing Review - Option 04
        /// </summary>
        public async Task<QueryResult> GetAresLogisticProcessingReviewAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetAresLogisticProcessingReviewAsync(
                    consultaIni ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAresLogisticProcessingReviewAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        /// <summary>
        /// Get ARES Cost Movement Processing Review - Option 05
        /// </summary>
        public async Task<QueryResult> GetAresCostProcessingReviewAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetAresCostProcessingReviewAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetAresCostProcessingReviewAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnInventoryComparisonAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetBcnInventoryComparisonAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnInventoryComparisonAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnBalanceRuleAsync(DateTime? consultaIni)
        {
            try
            {
                var data = await _queriesService.GetBcnBalanceRuleAsync(consultaIni ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnBalanceRuleAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnBalanceDifferenceAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetBcnBalanceDifferenceAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnBalanceDifferenceAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        public async Task<QueryResult> GetBcnCostComparisonAsync(DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                var data = await _queriesService.GetBcnCostComparisonAsync(
                    consultaIni ?? DateTime.Today, 
                    consultaFin ?? DateTime.Today);
                
                return ConvertToQueryResult(data);
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in GetBcnCostComparisonAsync: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to get property value from dynamic object
        /// </summary>
        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            
            // Try dictionary access
            if (obj is Dictionary<string, object> dict && dict.ContainsKey(propertyName))
            {
                return dict[propertyName];
            }
            
            return null;
        }

        private QueryResult ConvertToQueryResult<T>(IEnumerable<T> data)
        {
            var result = new QueryResult();

            if (data == null)
                return result;

            var properties = typeof(T).GetProperties();
            result.Columns = properties.Select(p => p.Name).ToList();

            foreach (var item in data)
            {
                if (item == null)
                    continue;

                var dict = new Dictionary<string, object>();

                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(item);
                        dict[prop.Name] = value ?? DBNull.Value;
                    }
                    catch (Exception ex)
                    {
                        dict[prop.Name] = $"Error: {ex.Message}";
                    }
                }

                result.Data.Add(dict);
            }

            return result;
        }

        public async Task<bool> HasIntegratedDataAsync(string option, DateTime dateFrom, DateTime dateTo)
        {
            // For consolidation options, check if the corresponding integrated data exists
            // Consolidation options map to their required integrated data sources
            QueryResult result = null;
            switch (option)
            {
                case "01": // BCN: Inventarios consolidados -> check BCN: Inventarios integrated
                    result = await GetBcnInventoryAsync(dateFrom, 4);
                    break;
                case "02": // BCN: Movimientos consolidados -> check BCN: Movimientos integrated
                    result = await GetBcnMovementsAsync(dateFrom, dateTo);
                    break;
                case "03": // BCN: Balance ALMACEN -> check BCN: Movimientos integrated
                    result = await GetBcnConsolidatedInventoryBalanceAsync(dateFrom, 5);
                    break;
                case "04": // BCN: Balance POOL -> check BCN: Movimientos integrated
                    result = await GetBcnConsolidatedInventoryBalanceAsync(dateFrom, 5);
                    break;
                case "05": // BCN: Balance UNIDAD DE PROCESO -> check BCN: Movimientos integrated
                    result = await GetBcnMovementsAsync(dateFrom, dateTo);
                    break;
                case "06": // BCN: Foto inventario consolidado -> check BCN: Foto inventario integrated
                    result = await GetBcnConsolidatedInventoryPhotoAsync(dateFrom);
                    break;
                case "07": // BCN: Movimientos (integration) -> check BCN: Movimientos integrated
                    result = await GetBcnMovementsAsync(dateFrom, dateTo);
                    break;
                case "08": // WebService: Costos (integration) -> check WebService: Costos integrated
                    result = await GetWsCostsAsync(dateFrom, dateTo);
                    break;
                case "09": // WebService: Movimientos logísticos (integration) -> check WebService: Movimientos logísticos integrated
                    result = await GetWsLogisticMovementsAsync(dateFrom, dateTo);
                    break;
                case "10": // ARES: Movimientos HFS (integration) -> check ARES: Movimientos HFS integrated
                    result = await GetHpiMovementsAsync(dateFrom);
                    break;
                default:
                    // For unknown options, return false
                    return false;
            }
            return result != null && result.Data != null && result.Data.Count > 0;
        }

        #endregion

        #region SAP ECC ECP Web Service

        /// <summary>
        /// Call SAP ECC ECP Web Service - Python WSSAPECCECP equivalent
        /// </summary>
        /// <summary>
        /// Call SAP ECC ECP Web Service - Implementación 100% compatible con Python WSSAPECCECP
        /// </summary>
        public async Task<QueryResult> CallSapEccEcpWebServiceAsync(string infoType, DateTime? consultaIni, DateTime? consultaFin)
        {
            try
            {
                // Configuración exacta del web service SAP ECC ECP según los datos del Python
                var sapEccEcpConfig = new
                {
                    idUsr = "cons_aresapi",
                    pwUsr = "H-msQatepeK2",
                    txURL = "http://vhecppo2ci.hec.ecopetrol.com.co:50000/RESTAdapter/aora/",
                    txMetodoInventario = "comparacionInventario",
                    txMetodoCosto = "comparativoCostos",
                    arrCenLog = new[] { 
                        new { Nombre = "2000" },
                        new { Nombre = "3501" },
                        new { Nombre = "4130" }
                    },
                    arrAlmLog = new[] { 
                        new { Nombre = "M001" },
                        new { Nombre = "S001" },
                        new { Nombre = "T001" },
                        new { Nombre = "A001" },
                        new { Nombre = "A002" },
                        new { Nombre = "C002" },
                        new { Nombre = "F001" }
                    }
                };

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var fechaAuxF = consultaFin?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
                
                string endpoint;
                object payload;
                string transaccionOrigen;

                if (infoType == "INVENTARIO")
                {
                    transaccionOrigen = "Comparativo de Inventarios";
                    endpoint = sapEccEcpConfig.txURL + sapEccEcpConfig.txMetodoInventario;
                    payload = new
                    {
                        FechaConsulta = fechaAuxF,
                        SistemaOrigen = "ARESGRB",
                        CentroLogistico = sapEccEcpConfig.arrCenLog,
                        Almacen = sapEccEcpConfig.arrAlmLog
                    };
                }
                else if (infoType == "CECO")
                {
                    transaccionOrigen = "Comparativo de Costos";
                    endpoint = sapEccEcpConfig.txURL + sapEccEcpConfig.txMetodoCosto;
                    
                    // Extraer mes y año exactamente como en Python: str(IFechaFin[6:7]) y str(IFechaFin[:4])
                    var fechaFinStr = consultaFin?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
                    var periodo = fechaFinStr.Substring(5, 2); // MM
                    var anio = fechaFinStr.Substring(0, 4);   // YYYY
                    
                    payload = new
                    {
                        SistemaOrigen = "AORA",
                        Periodo = periodo,
                        Anio = anio,
                        TipoValor = 4,
                        ObjetoCostos = new[] { new { Nombre = "RF5050" } }
                    };
                }
                else
                {
                    throw new ArgumentException($"Tipo de información no válido: {infoType}");
                }

                _loggingService.WriteInfo($"{timestamp} INFO: ARES {transaccionOrigen}: calling SAP ECC ECP web service at {endpoint}");

                // Crear cliente HTTP con autenticación básica usando WebClient (nativo del framework)
                using (var webClient = new System.Net.WebClient())
                {
                    // Configurar autenticación básica exactamente como en Python
                    var credentials = Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes($"{sapEccEcpConfig.idUsr}:{sapEccEcpConfig.pwUsr}"));
                    webClient.Headers.Add("Authorization", $"Basic {credentials}");
                    
                    // Headers exactamente como en Python
                    webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
                    webClient.Headers.Add("Accept", "application/json");

                    // Serializar payload a JSON usando JavaScriptSerializer (nativo del framework)
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsonPayload = serializer.Serialize(payload);

                    _loggingService.WriteInfo($"{timestamp} INFO: Sending request to {endpoint} with payload: {jsonPayload}");

                    try
                    {
                        // Hacer la llamada HTTP POST
                        var responseBytes = await webClient.UploadDataTaskAsync(endpoint, "POST", 
                            System.Text.Encoding.UTF8.GetBytes(jsonPayload));
                        var responseContent = System.Text.Encoding.UTF8.GetString(responseBytes);

                        _loggingService.WriteInfo($"{timestamp} INFO: Response received successfully. Content length: {responseContent.Length}");
                        _loggingService.WriteInfo($"{timestamp} INFO: Response content: {responseContent}");

                        // Parsear la respuesta JSON usando JavaScriptSerializer (nativo del framework)
                        var responseData = serializer.Deserialize<Dictionary<string, object>>(responseContent);
                        
                        // Extraer los datos exactamente como en Python: varrDataAux["INVENTARIO"] o varrDataAux["CECO"]
                        if (infoType == "INVENTARIO" && responseData.ContainsKey("INVENTARIO"))
                        {
                            var inventoryArray = responseData["INVENTARIO"] as object[];
                            var inventoryData = ConvertArrayToQueryResult(inventoryArray, "InventarioBCN,InventarioECC,InventarioS4H");
                            _loggingService.WriteInfo($"{timestamp} INFO: Envio exitoso del Registro {transaccionOrigen} (estado: 200)!!!");
                            _loggingService.WriteInfo($"{timestamp} INFO: ARES {transaccionOrigen}: successful response with {inventoryData.Data?.Count ?? 0} inventory records");
                            return inventoryData;
                        }
                        else if (infoType == "CECO" && responseData.ContainsKey("CECO"))
                        {
                            var costArray = responseData["CECO"] as object[];
                            var costData = ConvertArrayToQueryResult(costArray, "Item,ID Registro Costo,Fecha Contabilización,Texto Movimiento,Tipo Objeto Costo,ID Objeto Costo,ID Valor Estadístico,Nombre Producto,UM,Valor Contabilizado,JSON Movimiento");
                            _loggingService.WriteInfo($"{timestamp} INFO: Envio exitoso del Registro {transaccionOrigen} (estado: 200)!!!");
                            _loggingService.WriteInfo($"{timestamp} INFO: ARES {transaccionOrigen}: successful response with {costData.Data?.Count ?? 0} cost records");
                            return costData;
                        }
                        else
                        {
                            _loggingService.WriteWarning($"{timestamp} WARNING: No se encontraron datos de {infoType} en la respuesta. Keys disponibles: {string.Join(", ", responseData.Keys)}");
                            return new QueryResult
                            {
                                Message = $"No se encontraron datos de {infoType} en la respuesta del web service SAP ECC ECP",
                                Data = new List<Dictionary<string, object>>(),
                                Columns = new List<string>()
                            };
                        }
                    }
                    catch (System.Net.WebException webEx)
                    {
                        // Manejo de errores exactamente como en Python
                        string errorMessage;
                        int errorCode;
                        
                        if (webEx.Status == WebExceptionStatus.ConnectFailure)
                        {
                            errorMessage = $"{timestamp} ERROR: Estado: 400 - No se pudo conectar con el servidor. Asegurese de que el servidor este en funcionamiento.";
                            errorCode = 400;
                        }
                        else if (webEx.Status == WebExceptionStatus.Timeout)
                        {
                            errorMessage = $"{timestamp} ERROR: Estado: 500 - Se agotó el tiempo de espera de la solicitud. Verifique su conexión a Internet o vuelva a intentarlo más tarde.";
                            errorCode = 500;
                        }
                        else
                        {
                            errorCode = 0;
                            errorMessage = $"{timestamp} ERROR: Estado: {errorCode} - {transaccionOrigen}";
                        }
                        
                        _loggingService.WriteError(errorMessage);
                        
                        // Intentar leer la respuesta de error
                        string errorResponse = "";
                        if (webEx.Response != null)
                        {
                            using (var errorStream = webEx.Response.GetResponseStream())
                            using (var errorReader = new System.IO.StreamReader(errorStream))
                            {
                                errorResponse = errorReader.ReadToEnd();
                                _loggingService.WriteError($"{timestamp} ERROR: Error response: {errorResponse}");
                            }
                        }
                        
                        return new QueryResult
                        {
                            Message = errorMessage,
                            Data = new List<Dictionary<string, object>>(),
                            Columns = new List<string>()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR: Error inesperado en CallSapEccEcpWebServiceAsync: {ex.Message}");
                return new QueryResult
                {
                    Message = $"Error inesperado: {ex.Message}",
                    Data = new List<Dictionary<string, object>>(),
                    Columns = new List<string>()
                };
            }
        }

        /// <summary>
        /// Convierte un array de objetos a QueryResult con las columnas especificadas
        /// Implementación 100% compatible con Python getConvertirXML
        /// </summary>
        private QueryResult ConvertArrayToQueryResult(object[] dataArray, string columnNames)
        {
            var columns = columnNames.Split(',').Select(c => c.Trim()).ToList();
            var data = new List<Dictionary<string, object>>();

            if (dataArray != null && dataArray.Length > 0)
            {
                _loggingService.WriteInfo($"ConvertArrayToQueryResult: Processing {dataArray.Length} items");
                
                for (int i = 0; i < dataArray.Length; i++)
                {
                    var item = dataArray[i];
                    var row = new Dictionary<string, object>();
                    
                    // Agregar número de item
                    row["Item"] = i + 1;
                    
                    // Procesar el item según su tipo
                    if (item is Dictionary<string, object> itemDict)
                    {
                        _loggingService.WriteInfo($"Processing dictionary item {i}: {string.Join(", ", itemDict.Keys)}");
                        
                        foreach (var column in columns)
                        {
                            if (column == "Item") continue; // Ya lo agregamos
                            
                            if (itemDict.ContainsKey(column))
                            {
                                row[column] = itemDict[column] ?? "";
                            }
                            else
                            {
                                // Buscar con diferentes variaciones del nombre (como en Python)
                                var foundKey = itemDict.Keys.FirstOrDefault(k => 
                                    string.Equals(k, column, StringComparison.OrdinalIgnoreCase) ||
                                    k.Replace(" ", "").Equals(column.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
                                
                                if (foundKey != null)
                                {
                                    row[column] = itemDict[foundKey] ?? "";
                                }
                                else
                                {
                                    row[column] = "";
                                }
                            }
                        }
                    }
                    else
                    {
                        _loggingService.WriteWarning($"Item {i} is not a dictionary: {item?.GetType()?.Name}");
                        
                        // Si no es un diccionario, intentar procesar como objeto simple
                        foreach (var column in columns)
                        {
                            if (column == "Item") continue;
                            row[column] = item?.ToString() ?? "";
                        }
                    }
                    
                    data.Add(row);
                }
            }
            else
            {
                _loggingService.WriteWarning("ConvertArrayToQueryResult: dataArray is null or empty");
            }

            var result = new QueryResult
            {
                Data = data,
                Columns = new List<string> { "Item" }.Concat(columns.Where(c => c != "Item")).ToList(),
                Message = data.Count > 0 ? $"Datos obtenidos del web service SAP ECC ECP: {data.Count} registros" : "No se obtuvieron datos del web service SAP ECC ECP"
            };
            
            _loggingService.WriteInfo($"ConvertArrayToQueryResult: Returning {result.Data.Count} rows with {result.Columns.Count} columns");
            
            return result;
        }

        #endregion 
    }
} 