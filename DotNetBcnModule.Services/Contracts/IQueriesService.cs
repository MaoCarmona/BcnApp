using NetBcnModule.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace DotNetBcnModule.Services.Contracts
{
    public interface IQueriesService
    {
        // AORA Queries
        Task<IEnumerable<AoraInventoryModel>> GetAoraInventoryAsync(DateTime consultaIni);
        Task<IEnumerable<AoraMovementModel>> GetAoraMovementsAsync(DateTime consultaIni, DateTime consultaFin);
        Task<IEnumerable<AoraFlowModel>> GetAoraFlowsAsync(DateTime consultaIni, DateTime consultaFin);

        // ROMSS Queries
        Task<IEnumerable<RomssInventoryModel>> GetRomssInventoryAsync(DateTime consultaIni);
        Task<IEnumerable<RomssMovementModel>> GetRomssMovementsAsync(DateTime consultaIni, DateTime consultaFin);

        // BCN Queries
        Task<IEnumerable<BcnInventoryModel>> GetBcnInventoryAsync(DateTime consultaIni, int idCaso);
        Task<IEnumerable<BcnInventoryDetailModel>> GetBcnInventoryDetailAsync(DateTime consultaIni, int idCaso);
        Task<IEnumerable<BcnMovementViewModel>> GetBcnMovementViewAsync(DateTime consultaIni, DateTime consultaFin, int idCaso = 4, string filtroMovimiento = "", string tpMovimiento = "");
        Task<IEnumerable<BcnBalanceOperativoModel>> GetBcnBalanceOperativoAsync(DateTime consultaIni, DateTime consultaFin, int idCaso = 4);
        Task<IEnumerable<BcnInventoryModel>> GetBcnConsolidatedInventoryBalanceAsync(DateTime consultaIni, int idCaso = 5);
        Task<IEnumerable<BcnMovementModel>> GetBcnMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento = "", int idCaso = 4);
        Task<IEnumerable<BcnMovementModel>> GetBcnConsolidatedMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento = "", int idCaso = 5);
        Task<IEnumerable<BcnInventoryPhotoModel>> GetBcnInventoryPhotoAsync(DateTime consultaIni);
        Task<IEnumerable<HpiMovementModel>> GetHpiMovementsAsync(DateTime consultaIni);
        Task<IEnumerable<BcnConsolidatedBalanceModel>> GetBcnConsolidatedBalanceAsync(DateTime consultaIni, DateTime consultaFin, string tpMovimiento);
        Task<IEnumerable<BcnConsolidatedInventoryPhotoModel>> GetBcnConsolidatedInventoryPhotoAsync(DateTime consultaIni);
        Task<IEnumerable<LogisticBalanceModel>> GetLogisticBalanceAsync(string nbCeLo, DateTime consultaIni, DateTime consultaFin);
        Task<IEnumerable<WsLogisticInventoryModel>> GetWsLogisticInventoryAsync(DateTime consultaIni, DateTime consultaFin);

        // Web Service Queries
        Task<IEnumerable<WsCostModel>> GetWsCostsAsync(DateTime consultaIni, DateTime consultaFin);
        Task<IEnumerable<WsLogisticMovementModel>> GetWsLogisticMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento);

        // ARES Processing Review Queries
        Task<IEnumerable<AresLogisticProcessingReviewModel>> GetAresLogisticProcessingReviewAsync(DateTime consultaIni);
        Task<IEnumerable<AresCostProcessingReviewModel>> GetAresCostProcessingReviewAsync(DateTime consultaIni, DateTime consultaFin);

        // BCN Comparison Queries
        Task<IEnumerable<BcnInventoryComparisonModel>> GetBcnInventoryComparisonAsync(DateTime consultaIni);
        Task<IEnumerable<BcnCostComparisonModel>> GetBcnCostComparisonAsync(DateTime consultaIni, DateTime consultaFin);

        // BCN Balance Queries
        Task<IEnumerable<BcnBalanceRuleModel>> GetBcnBalanceRuleAsync(DateTime consultaIni);
        Task<IEnumerable<BcnBalanceDifferenceModel>> GetBcnBalanceDifferenceAsync(DateTime consultaIni, DateTime consultaFin);
    }
} 