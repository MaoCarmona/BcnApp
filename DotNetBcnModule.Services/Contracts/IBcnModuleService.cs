using NetBcnModule.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetBcnModule.Services.Contracts
{
    /// <summary>
    /// Comprehensive service interface for all BCN Module operations
    /// </summary>
    public interface IBcnModuleService
    {
        #region Integrar Información (Operational Integration)
        
        /// <summary>
        /// AORA: Inventario operativo
        /// </summary>
        Task<QueryResult> GetAoraInventoryAsync(DateTime? consultaIni);
        
        /// <summary>
        /// AORA: Movimientos operativo
        /// </summary>
        Task<QueryResult> GetAoraMovementsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// AORA: Flujos operativo
        /// </summary>
        Task<QueryResult> GetAoraFlowsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// ROMSS: Inventario operativo
        /// </summary>
        Task<QueryResult> GetRomssInventoryAsync(DateTime? consultaIni);
        
        /// <summary>
        /// ROMSS: Movimientos operativo
        /// </summary>
        Task<QueryResult> GetRomssMovementsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// BCN: Foto inventario operativo
        /// </summary>
        Task<QueryResult> GetBcnInventoryPhotoAsync(DateTime? consultaIni);
        
        /// <summary>
        /// BCN: Movimientos
        /// </summary>
        Task<QueryResult> GetBcnMovementsAsync(DateTime? consultaIni, DateTime? consultaFin, string filtroMovimiento = "");
        
        /// <summary>
        /// WebService: Costos
        /// </summary>
        Task<QueryResult> GetWsCostsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// WebService: Movimientos logísticos
        /// </summary>
        Task<QueryResult> GetWsLogisticMovementsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// ARES: Movimientos HFS
        /// </summary>
        Task<QueryResult> GetHpiMovementsAsync(DateTime? consultaIni);
        
        #endregion

        #region Consolidar Información (Consolidation)
        
        /// <summary>
        /// BCN: Inventarios consolidados
        /// </summary>
        Task<QueryResult> GetBcnConsolidatedInventoryAsync(DateTime? consultaIni, DateTime? consultaFin, int idCaso = 4);
        
        /// <summary>
        /// BCN: Inventarios consolidados (using qryGETINVENTARIOSBCN)
        /// </summary>
        Task<QueryResult> GetBcnConsolidatedInventoryBalanceAsync(DateTime? consultaIni, int idCaso = 5);
        
        /// <summary>
        /// BCN: Movimientos consolidados
        /// </summary>
        Task<QueryResult> GetBcnConsolidatedMovementsAsync(DateTime? consultaIni, DateTime? consultaFin, string filtroMovimiento = "", int idCaso = 5);
        
        /// <summary>
        /// BCN: Balance ALMACEN
        /// </summary>
        Task<QueryResult> GetBcnBalanceAlmacenAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "ALMACEN PRODUCTO");
        
        /// <summary>
        /// BCN: Balance POOL
        /// </summary>
        Task<QueryResult> GetBcnBalancePoolAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "POOL PRODUCTO");
        
        /// <summary>
        /// BCN: Balance UNIDAD DE PROCESO
        /// </summary>
        Task<QueryResult> GetBcnBalanceUnidadProcesoAsync(DateTime? consultaIni, DateTime? consultaFin, string tpMovimiento = "UNIDAD DE PROCESO");
        
        /// <summary>
        /// BCN: Foto inventario consolidado
        /// </summary>
        Task<QueryResult> GetBcnConsolidatedInventoryPhotoAsync(DateTime? consultaIni);
        
        #endregion

        #region Transformación Logística (Logistic Transformation)
        
        /// <summary>
        /// Movimientos logísticos
        /// </summary>
        Task<QueryResult> GetLogisticMovementsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// Movimientos de costos
        /// </summary>
        Task<QueryResult> GetCostMovementsAsync(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// Balance GRB CeLo: 2000
        /// </summary>
        Task<QueryResult> GetBalanceGrbCelo2000Async(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// Balance Reexpido CeLo: 3501
        /// </summary>
        Task<QueryResult> GetBalanceReexpidoCelo3501Async(DateTime? consultaIni, DateTime? consultaFin);
        
        /// <summary>
        /// Balance Impala CeLo: 4130
        /// </summary>
        Task<QueryResult> GetBalanceImpalaCelo4130Async(DateTime? consultaIni, DateTime? consultaFin);
        
        #endregion

        #region Envío BCN WS-ARES (ARES Web Service)
        
        /// <summary>
        /// Inventario logístico para ARES
        /// </summary>
        Task<QueryResult> GetAresLogisticInventoryAsync(List<AresInventoryPayload> inventoryData, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Movimiento logístico para ARES
        /// </summary>
        Task<QueryResult> GetAresLogisticMovementAsync(List<AresMovementPayload> movementData, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Movimiento de costos para ARES
        /// </summary>
        Task<QueryResult> GetAresCostMovementAsync(List<AresCostPayload> costData, CancellationToken cancellationToken = default);
        
        #endregion

        #region ARES Processing Review

        /// <summary>
        /// ARES: Rev. Procesamiento Logistico - Option 04
        /// </summary>
        Task<QueryResult> GetAresLogisticProcessingReviewAsync(DateTime? consultaIni);

        /// <summary>
        /// ARES: Rev. Procesamiento Costo - Option 05
        /// </summary>
        Task<QueryResult> GetAresCostProcessingReviewAsync(DateTime? consultaIni, DateTime? consultaFin);

        /// <summary>
        /// SAP ECC ECP Web Service (Python WSSAPECCECP equivalent)
        /// </summary>
        Task<QueryResult> CallSapEccEcpWebServiceAsync(string infoType, DateTime? consultaIni, DateTime? consultaFin);

        #endregion

        #region BCN Comparison Queries

        /// <summary>
        /// BCN: Comparativo Inventario - Option 06
        /// </summary>
        Task<QueryResult> GetBcnInventoryComparisonAsync(DateTime? consultaIni);

        /// <summary>
        /// BCN: Comparativo Costos - Option 07
        /// </summary>
        Task<QueryResult> GetBcnCostComparisonAsync(DateTime? consultaIni, DateTime? consultaFin);

        #endregion

        #region BCN Balance Queries

        /// <summary>
        /// BCN: Aplicar Regla de Balance - Option 07
        /// </summary>
        Task<QueryResult> GetBcnBalanceRuleAsync(DateTime? consultaIni);

        /// <summary>
        /// BCN: Diferencia Balance - Option 08
        /// </summary>
        Task<QueryResult> GetBcnBalanceDifferenceAsync(DateTime? consultaIni, DateTime? consultaFin);

        #endregion

        /// <summary>
        /// Checks if integration has been performed for the given option and date range
        /// </summary>
        Task<bool> HasIntegratedDataAsync(string option, DateTime dateFrom, DateTime dateTo);
    }

    /// <summary>
    /// Result model for queries
    /// </summary>
    public class QueryResult
    {
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
        public List<string> Columns { get; set; } = new List<string>();
        public int RecordCount => Data?.Count ?? 0;
        public bool Success => Data != null;
        public string Message { get; set; } = "Query executed successfully";
    }
} 