using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;

namespace NetBcnModule.Services.Queries
{
    /// <summary>
    /// Service class to handle all queries from the XML configuration
    /// </summary>
    public class QueriesService : IQueriesService
    {
        private readonly DatabaseService _aresDatabaseService;
        private readonly DatabaseService _romssDatabaseService;
        private readonly DatabaseService _bcnDatabaseService;

        public QueriesService(ILoggingService loggingService)
        {
            _aresDatabaseService = new DatabaseService(DatabaseTarget.DBARES, loggingService);
            _romssDatabaseService = new DatabaseService(DatabaseTarget.DBAROMSS, loggingService);
            _bcnDatabaseService = new DatabaseService(DatabaseTarget.DBBCN, loggingService);
        }

        #region AORA Queries

        /// <summary>
        /// Query to get inventory data from AORA
        /// </summary>
        public async Task<IEnumerable<AoraInventoryModel>> GetAoraInventoryAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT ROW_NUMBER() OVER (PARTITION BY DVRMASS.TIME_TAKEN ORDER BY rtrim(PLOUNITS.TAG)) nbRN
                , DVRMASS.TIME_TAKEN dtInventario
                , PLOUNITS.DBINDEX idRecOrigen 
                , rtrim(PLOUNITS.TAG) nmRecOrigen
                , GLOFEDST.DBINDEX idProdOrigen
                , rtrim(GLOFEDST.TAG) nmProdOrigen
                , CONVERT(DECIMAL(10, 2), DORVOLUM.V_CORRCTED) vFuente	 
                , 'BLS' vUM
                , CONVERT(DECIMAL(10, 2), DORVOLUM.W_CORRCTED) wFuente     
                , 'TM' wUM
                from DORVOLUM
                join GLOFEDST on GLOFEDST.DBINDEX = DORVOLUM.IND2FEDSTK
                join PLOUNITS on PLOUNITS.DBINDEX = DORVOLUM.IND2VESSEL
                join DLORECON on DLORECON.DBINDEX = DORVOLUM.IND2PERIOD
                join DVRMASS on DVRMASS.IND2INSTR = DORVOLUM.IND2INSTR and SUBSTRING(DVRMASS.TIME_TAKEN,0,11) = SUBSTRING(DLORECON.TIME_BEG,0,11)
                where DVRMASS.TIME_TAKEN = @ConsultaIni";

            return await _aresDatabaseService.QueryAsync<AoraInventoryModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get movement data from AORA
        /// </summary>
        public async Task<IEnumerable<AoraMovementModel>> GetAoraMovementsAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT ROW_NUMBER() OVER (PARTITION BY CONVERT(DATE, DLOEVENT.TIME_BEG) ORDER BY PLOINSTR.TAG) nbRN
                , rtrim(PLOINSTR.TAG) Tag
                , DLOEVENT.TIME_BEG dtMovIni
                , DLOEVENT.TIME_END dtMovFin
                , P_EMISOR.DBINDEX idRecOrigen
                , rtrim(P_EMISOR.TAG) nmRecOrigen
                , GLOFEDST.DBINDEX idProdOrigen
                , rtrim(GLOFEDST.TAG) nmProdOrigen
                , P_RECEPTOR.DBINDEX idRecDestino
                , rtrim(P_RECEPTOR.TAG) nmRecDestino
                , ISNULL(G_RECEPTOR.DBINDEX, GLOFEDST.DBINDEX) idProdDestino
                , ISNULL(rtrim(G_RECEPTOR.TAG), rtrim(GLOFEDST.TAG)) nmProdDestino
                , CONVERT(DECIMAL(10, 2), DOREVENT.V_CORRCTED) vFuente
                , CONVERT(DECIMAL(10, 2), (DOREVENT.V_CORRCTED + DOREVENT.V_ADJ_SYS + DOREVENT.V_ADJ_USER)) vReconciliado
                , 'BLS' AS vUM
                , CONVERT(DECIMAL(10, 2), DOREVENT.W_CORRCTED) wFuente
                , CONVERT(DECIMAL(10, 2), (DOREVENT.W_CORRCTED + DOREVENT.W_ADJ_SYS + DOREVENT.W_ADJ_USER)) wReconciliado
                , 'TM' AS wUM
                from DVRINTGM
                join PLOINSTR on PLOINSTR.DBINDEX = DVRINTGM.IND2INSTR
                join PCOIINTM on PCOIINTM.IND2OBJECT = PLOINSTR.DBINDEX 
                join DOREVENT on DOREVENT.IND2INSTR = PLOINSTR.DBINDEX
                join DLOEVENT on DLOEVENT.IND2INSTR = PLOINSTR.DBINDEX
                join PCOPPERM on PCOPPERM.IND2OBJECT = PCOIINTM.IND2SOURCE
                join PLOUNITS as P_EMISOR on P_EMISOR.DBINDEX = PCOPPERM.IND2SOURCE
                join PLOUNITS as P_RECEPTOR on P_RECEPTOR.DBINDEX = PCOPPERM.IND2DESTIN
                join GLOFEDST on GLOFEDST.DBINDEX = DOREVENT.IND2FEDSTK
                left join PCOIMASS as PC_EMISOR on PC_EMISOR.IND2SOURCE = P_EMISOR.DBINDEX and PC_EMISOR.TIME_END = '2038-01-01 00:00:00'
                left join DVRMASS as D_EMISOR on D_EMISOR.IND2INSTR = PC_EMISOR.IND2OBJECT and CONVERT(DATE,D_EMISOR.TIME_TAKEN) = CONVERT(DATE,DVRINTGM.TIME_TAKEN)
                left join GLOFEDST AS G_EMISOR on G_EMISOR.DBINDEX = D_EMISOR.IND2FEDSTK
                left join PCOIMASS as PC_RECEPTOR on PC_RECEPTOR.IND2SOURCE = P_RECEPTOR.DBINDEX and PC_RECEPTOR.TIME_END = '2038-01-01 00:00:00'
                left join DVRMASS as D_RECEPTOR on D_RECEPTOR.IND2INSTR = PC_RECEPTOR.IND2OBJECT and CONVERT(DATE,D_RECEPTOR.TIME_TAKEN) = CONVERT(DATE,DVRINTGM.TIME_TAKEN)
                left join GLOFEDST AS G_RECEPTOR on G_RECEPTOR.DBINDEX = D_RECEPTOR.IND2FEDSTK
                where 1 = 1
                AND rtrim(P_RECEPTOR.TAG) <> 'DC Perdidas Emisione'
                AND CHARINDEX('-', rtrim(PLOINSTR.TAG)) = 0
                AND ((DOREVENT.V_CORRCTED + DOREVENT.V_ADJ_SYS + DOREVENT.V_ADJ_USER) + (DOREVENT.W_CORRCTED + DOREVENT.W_ADJ_SYS + DOREVENT.W_ADJ_USER)) > 0 
                AND DLOEVENT.TIME_END BETWEEN @ConsultaIni AND @ConsultaFin";

            return await _aresDatabaseService.QueryAsync<AoraMovementModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get flow data from AORA
        /// </summary>
        public async Task<IEnumerable<AoraFlowModel>> GetAoraFlowsAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT ROW_NUMBER() OVER (PARTITION BY DVRFLOWM.TIME_TAKEN ORDER BY PLOINSTR.TAG, DVRFLOWM.TIME_TAKEN) nbRN
                , DVRFLOWM.TIME_TAKEN dtFlujo
                , rtrim(PLOINSTR.TAG) Tag
                , P_EMISOR.DBINDEX  idRecOrigen     
                , rtrim(P_EMISOR.TAG) nmRecOrigen     
                , P_RECEPTOR.DBINDEX idRecDestino
                , rtrim(P_RECEPTOR.TAG) nmRecDestino
                , GLOFEDST.DBINDEX as idProdOrigen
                , rtrim(GLOFEDST.TAG) nmProdOrigen
                , CONVERT(DECIMAL(18, 4), DOREVENT.V_CORRCTED) vFuente
                , CONVERT(DECIMAL(18, 4), (DOREVENT.V_CORRCTED + DOREVENT.V_ADJ_SYS + DOREVENT.V_ADJ_USER)) vReconciliado
                , 'BLS' AS vUM
                , CONVERT(DECIMAL(18, 4), DOREVENT.W_CORRCTED) wFuente
                , CONVERT(DECIMAL(18, 4), (DOREVENT.W_CORRCTED + DOREVENT.W_ADJ_SYS + DOREVENT.W_ADJ_USER)) wReconciliado
                , 'TM' AS wUM
                from DVRFLOWM
                join PLOINSTR on PLOINSTR.DBINDEX = DVRFLOWM.IND2INSTR
                join PCOIFLOM on PCOIFLOM.IND2OBJECT = PLOINSTR.DBINDEX and PCOIFLOM.TIME_END = '2038-01-01 00:00:00'
                join PLOPIPES on PLOPIPES.DBINDEX = PCOIFLOM.IND2SOURCE
                join DOREVENT on DOREVENT.IND2INSTR = PLOINSTR.DBINDEX
                join DLORECON on DLORECON.DBINDEX = DOREVENT.IND2PERIOD and DVRFLOWM.TIME_TAKEN = DLORECON.TIME_BEG
                join PCOPPERM on PCOPPERM.IND2OBJECT = PLOPIPES.DBINDEX and PCOPPERM.TIME_END = '2038-01-01 00:00:00'
                join PLOUNITS as P_EMISOR on P_EMISOR.DBINDEX = PCOPPERM.IND2SOURCE
                join PLOUNITS as P_RECEPTOR on P_RECEPTOR.DBINDEX = PCOPPERM.IND2DESTIN
                join GLOFEDST on GLOFEDST.DBINDEX = DVRFLOWM.IND2FEDSTK
                WHERE 1 = 1
                AND ((DOREVENT.V_CORRCTED + DOREVENT.V_ADJ_SYS + DOREVENT.V_ADJ_USER) +
                     (DOREVENT.W_CORRCTED + DOREVENT.W_ADJ_SYS + DOREVENT.W_ADJ_USER)) > 0 
                AND DVRFLOWM.TIME_TAKEN BETWEEN @ConsultaIni AND @ConsultaFin";

            return await _aresDatabaseService.QueryAsync<AoraFlowModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        #endregion

        #region ROMSS Queries

        /// <summary>
        /// Query to get inventory data from ROMSS
        /// </summary>
        public async Task<IEnumerable<RomssInventoryModel>> GetRomssInventoryAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT CONVERT(VARCHAR(30), DATEADD(minute, -1, a.ATD_TIMESTAMP), 20) dtInventario
                , IIF(Tc.T_ID IS NULL, 'No', 'Si') boInvFoto
                , IIF(Tc.T_ID IS NULL, 'BLS', Tc.UM_ID) umInvFoto
                , ISNULL(a.T_ID,'No Data') nbAlmacen
                , a.C_ID nbProducto
                , ISNULL(CONVERT(DECIMAL(5, 1), a.ATD_API60), 0) nbAPI60
                , CONVERT(DECIMAL(15, 2), a.ATD_NSV) TotalNSV
                , CAST((a.ATD_AVAILWEIGHT * ((100-isnull(hd.HD_SW, 0))/100)) / ((((141.5*0.999016)/(131.5+a.ATD_API60))*0.158987294928)/0.90718474) AS DECIMAL(15, 2)) BombeableNSV
                , CAST(a.ATD_NSV - IIF(a.ATD_AVAILWEIGHT < 0, 0, (a.ATD_AVAILWEIGHT * ((100-isnull(hd.HD_SW, 0))/100))/ ((((141.5*0.999016)/(131.5+a.ATD_API60))*0.158987294928)/0.90718474)) AS DECIMAL(15, 2)) RemanenteNSV
                , 'BLS' vUM
                , CAST(a.ATD_NSW*0.90718474 AS DECIMAL(15, 2)) TotalNSW
                , CAST(IIF(a.ATD_AVAILWEIGHT <0, 0, a.ATD_AVAILWEIGHT * ((100-isnull(hd.HD_SW, 0))/100))*0.90718474 AS DECIMAL(15, 2)) PumpableNSW
                , CAST((a.ATD_NSW - IIF(a.ATD_AVAILWEIGHT <0, 0, a.ATD_AVAILWEIGHT * ((100-isnull(hd.HD_SW, 0))/100)))*0.90718474 AS DECIMAL(15, 2)) RemanenteNSW            
                , 'TM' wUM
                , CASE WHEN hd.HD_SHIP_AUTH = 1 THEN 'Si' ELSE 'No' END boVoBo
                , ISNULL(CAST(ISNULL(Tb.idUltMuestra, Tb1.idUltMuestra) AS VARCHAR(30)) , '') nbMuestra
                , CONVERT(VARCHAR(30), ISNULL(Tb.dtUltMuestra, Tb1.dtUltMuestra), 20) dtMuestra
                FROM offsite.offsite.acc_tank_data a
                INNER JOIN ( SELECT h.T_ID, h.HD_TIMESTAMP, h.HD_TNK_STAT, h.HD_SHIP_AUTH, h.HD_AVAIL_GSV, h.HD_MIN_GSW, h.HD_SW, h.HD_NSV, h.HD_GSW, HD_LEVEL_STAT
                                     , hd_min_gsv, hd_max_gsv, h.HD_TOV, hd_vcf_tc, HD_NOTE, HD_NOTE2, HD_AVAIL
                                     , HD_API60
                            FROM offsite.offsite.hist_data h 
                            WHERE 1 = 1
                            AND h.HD_SET_ID = 0
                            AND h.HD_TNK_STAT <> 6
                            AND h.hd_timestamp = @ConsultaIni
                           ) hd ON (hd.hd_timestamp=a.atd_timestamp and hd.t_id=a.t_id)
                LEFT JOIN ( SELECT a.T_ID, a.C_ID, c.UM_ID
                            FROM (SELECT T_ID, C_ID FROM offsite.offsite.acc_tank_data WHERE ATD_SET_ID = 0 AND ATD_TIMESTAMP = @ConsultaIni) a 
                            INNER JOIN ( SELECT C_ID, UPPER(SUBSTRING(C_SPIRAL_ID, IIF(SUBSTRING(C_SPIRAL_ID, 22, 1) ='_', 27, 26), CHARINDEX('_PTAS', C_SPIRAL_ID)-IIF(SUBSTRING(C_SPIRAL_ID, 22, 1) ='_', 27, 26))) UM_ID
                                         FROM offsite.offsite.COMPONENT WHERE SUBSTRING(C_SPIRAL_ID, 1, 8) LIKE 'ENT%') c on (a.C_ID = c.C_ID)
                            ) Tc ON (Tc.T_ID = a.T_ID AND Tc.C_ID = a.C_ID)
                LEFT JOIN ( SELECT x.E_ID, x.C_ID, x.idUltMuestra, x.dtUltMuestra 
                            FROM ( SELECT s.LS_UNIT_ID E_ID, s.C_ID, s.LS_ID idUltMuestra, s.LS_TIMESTAMP dtUltMuestra 			                   
                                            , ROW_NUMBER() OVER (PARTITION by s.LS_UNIT_ID, s.C_ID ORDER by s.LS_TIMESTAMP DESC, s.LS_ID desc) Item
                                   FROM offsite.offsite.LAB_SAMPLES s
                                   INNER JOIN (SELECT * FROM offsite.offsite.EQUIPMENT WHERE ET_ID = 'TANK') e ON (e.E_ID = s.LS_UNIT_ID)
                                   WHERE 1 = 1
                                   AND s.LS_TIMESTAMP <= @ConsultaIni
                                   AND s.LS_TIMESTAMP >= DATEADD(MONTH, -10, @ConsultaIni)                                                     
                                  ) x
                            WHERE x.Item = 1 
                            ) Tb on (Tb.E_ID = a.t_id AND Tb.C_ID = a.C_ID)
                LEFT JOIN ( SELECT E_ID, eqh_labref as idUltMuestra, eqh_labq_time as dtUltMuestra 
                        FROM ( SELECT qh.E_ID, qh.eqh_labref, qh.eqh_labq_time, qh.EQ_SRC 
                                , ROW_NUMBER() OVER (PARTITION by qh.E_ID ORDER by qh.eqh_labq_time DESC, qh.eqh_labref desc) as ROW_ID 
                           FROM  offsite.offsite.equip_qual_hist qh
                                   INNER JOIN ( SELECT e.E_ID 
                                                FROM (SELECT * FROM offsite.offsite.EQUIPMENT WHERE ET_ID = 'TANK' AND E_AVSTAT = 0) e
                                                LEFT JOIN  offsite.offsite.LAB_SAMPLES s ON (e.E_ID = s.LS_UNIT_ID)
                                                WHERE 1 = 1
                                                AND e.ET_ID = 'TANK'
                                                AND s.LS_UNIT_ID IS NULL
                                                GROUP BY e.E_ID) e ON (e.E_ID = qh.E_ID)
                                   WHERE 1 = 1                
                                   AND qh.E_ID is not null 
                                   AND eqh_labref is not null 
                                   AND qh.eqh_labq_time <= getdate() 
                                   AND qh.eqh_labq_time >= DATEADD(MONTH, -40, getdate()) 
                                   )tmp 
                        WHERE row_id = 1 ) Tb1 on (Tb1.E_ID = a.t_id)
                WHERE 1 = 1
                AND a.ATD_SET_ID = 0
                AND a.ATD_TIMESTAMP = @ConsultaIni
                UNION ALL
                SELECT CONVERT(VARCHAR(30), DATEADD(minute, -1, a.AEC_TIMESTAMP), 20) dtInventario	   
                        , 'No' boInvFoto
                        , 'BLS' umInvFoto
                        , ISNULL(a.E_ID,'No Data') nbAlmacen
                        , ISNULL(a.C_ID,'No Data') nbProducto
                        , a.AEC_DENS15 nbAPI60
                        , CONVERT(DECIMAL(15, 2), a.AEC_NSV) TotalNSV
                        , CONVERT(DECIMAL(15, 2), a.AEC_NSV) BombeableNSV
                        , 0 RemanenteNSV
                        , 'BLS' vUM
                        , CAST(0.90718474*a.AEC_NSW AS DECIMAL(15, 2)) TotalNSW            			
                        , CAST(0.90718474*a.AEC_NSW AS DECIMAL(15, 2))PumpableNSW
                        , 0 RemanenteNSW
                        , 'TM' wUM
                        , 'No' boVoBo
                        , NULL nbMuestra
                        , NULL dtMuestra 
                FROM OFFSITE.OFFSITE.ACC_EQ_CONTENTS a
                INNER JOIN offsite.offsite.EQUIPMENT ae on (ae.C_ID = a.C_ID AND a.E_ID = ae.E_ID) 
                WHERE 1 = 1
                AND ae.E_IS_MAB = 1 
                AND a.AEC_NSV > 0 
                AND a.AEC_TIMESTAMP = @ConsultaIni";

            return await _romssDatabaseService.QueryAsync<RomssInventoryModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get movement data from ROMSS
        /// </summary>
        public async Task<IEnumerable<RomssMovementModel>> GetRomssMovementsAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT Ta.Tag
                , CONVERT(VARCHAR(20), IIF(CONVERT(VARCHAR(8), Ta.dtMovIni, 108) = '00:00:00', DATEADD(minute, 1, Ta.dtMovIni), Ta.dtMovIni), 120) dtMovIni
                , CONVERT(VARCHAR(20), IIF(CONVERT(VARCHAR(8), Ta.dtMovFin, 108) = '00:00:00', DATEADD(second, -1, Ta.dtMovFin), Ta.dtMovFin), 120) dtMovFin
                , Ta.tpCategoria
                , Ta.idRecOrigen
                , Ta.tpRecOrigen
                , Ta.idProdOrigen
                , Ta.idRecDestino
                , Ta.tpRecDestino
                , Ta.idProdDestino	
                , Ta.vFuente
                , Ta.vUM
                , Ta.wFuente
                , Ta.wUM
                , Ta.API
                , Ta.nbMuestra
                , Ta.numPedido
                , Ta.posPedido
                , Ta.uomPedido
                , Ta.cantPedido
                , 1 idMovSigno
                FROM ( SELECT DISTINCT mvi.OM_ID Tag
                             , IIF (mvi.RN = 1, IIF(mvi.AOM_ACT_START_DT < @ConsultaIni, CAST(DATEADD(DAY, -1, mvi.AOM_FINISH_DT) AS DATE), mvi.AOM_ACT_START_DT), mvf.AOM_FINISH_DT) dtMovIni
                             , mvi.AOM_ACT_FINISH_DT dtMovFin
                             , mvi.O_CAT_TYPE tpCategoria
                             , mvi.E_ID_FROM idRecOrigen
                             , (CASE WHEN eo.ET_ID IN ('TANK', 'TANKS') THEN 'ALMACEN OPERATIVO' 
                                     WHEN eo.ET_ID IN ('UNITS') THEN 'UNIDAD DE PROCESO'			     
                                     ELSE 'RECURSO ROMSS'
                                END) tpRecOrigen
                             , mvi.C_ID idProdOrigen
                             , mvi.E_ID_TO idRecDestino
                             , (CASE WHEN ed.ET_ID IN ('TANK', 'TANKS') THEN 'ALMACEN OPERATIVO' 
                                     WHEN ed.ET_ID IN ('UNITS') THEN 'UNIDAD DE PROCESO'			     
                                     ELSE 'RECURSO ROMSS'
                                END) tpRecDestino
                             , ISNULL(AT_C_ID2, mvi.C_ID) idProdDestino
                             , CONVERT(DECIMAL(10, 2), mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0)) vFuente
                             , 'BLS' vUM
                             , CONVERT(DECIMAL(10, 2), 0.90718474*(mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0))) wFuente 
                             , 'TM' wUM
                             , CONVERT(DECIMAL(10, 1), (CASE WHEN (mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0)) > 0 THEN ((141.5*0.999016) / (((mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0)) / (mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0))) * 6.28981)) - 131.5 ELSE ISNULL(omv.OM_MV_AVG_APIDENS_SYS, 0) END)) API
                             , ISNULL(mvi.AOM_SAMPLE_NR, '') nbMuestra
                             , '' numPedido
                             , '' posPedido
                             , 'N/A' uomPedido
                             , 0 cantPedido
                      FROM ( SELECT ROW_NUMBER() OVER (PARTITION BY OM_ID ORDER BY AOM_FINISH_DT) RN, a.*
                             FROM offsite.offsite.ACC_ORDER_MV a
                             WHERE 1 = 1 
                             AND ((AOM_ACT_START_DT >= @ConsultaIni AND AOM_ACT_START_DT <= @ConsultaFin) OR 
                                  (AOM_FINISH_DT >= @ConsultaIni AND AOM_FINISH_DT <= @ConsultaFin))
                            ) mvi
                      LEFT JOIN ( SELECT ROW_NUMBER() OVER (PARTITION BY OM_ID ORDER BY AOM_FINISH_DT) RN, a.*
                                  FROM offsite.offsite.ACC_ORDER_MV a
                                  WHERE 1 = 1 
                                  AND ((AOM_ACT_START_DT >= @ConsultaIni AND AOM_ACT_START_DT <= @ConsultaFin) OR 
                                       (AOM_FINISH_DT >= @ConsultaIni AND AOM_FINISH_DT <= @ConsultaFin))
                                 ) mvf ON (mvi.OM_ID = mvf.OM_ID AND mvi.RN = mvf.RN+1) 
                      INNER JOIN offsite.offsite.EQUIPMENT eo ON (eo.E_ID = mvi.E_ID_FROM)
                      INNER JOIN offsite.offsite.EQUIPMENT ed ON (ed.E_ID = mvi.E_ID_TO)
                      LEFT JOIN offsite.offsite.ORDERS o ON (o.O_ID = mvi.O_ID)
                      LEFT JOIN offsite.offsite.ORDER_MV omv ON (omv.OM_ID = mvi.OM_ID)
                      LEFT JOIN ( SELECT ROW_NUMBER() OVER(PARTITION BY AT_TIMESTAMP, OM_ID ORDER BY AT_TRANSDATE DESC) AS nbRN, * 
                                  FROM offsite.offsite.ACC_TRANS
                                  WHERE 1 = 1 ) t ON (t.OM_ID = mvi.OM_ID AND t.nbRN = 1)
                      WHERE 1 = 1
                      AND mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0) + mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0) <> 0
                      AND IIF (mvi.RN = 1, IIF(mvi.AOM_ACT_START_DT < @ConsultaIni, CAST(DATEADD(DAY, -1, mvi.AOM_FINISH_DT) AS DATE), mvi.AOM_ACT_START_DT), mvf.AOM_FINISH_DT) >= @ConsultaIni
                      AND mvi.AOM_FINISH_DT <= @ConsultaFin
                      AND (ISNULL(o.O_RELEASE_NR, '') = '' OR o.O_RELEASE_NR LIKE 'TAS%')
                      ) Ta
                      UNION ALL 
                      SELECT a.AT_ID Tag
                             , CONVERT(VARCHAR(20), a.AT_TRANSDATE, 120) dtMovIni
                             , CONVERT(VARCHAR(20), a.AT_TRANSDATE, 120) dtMovFin
                             , 'PRIORI' tpCategoria
                             , a.E_ID idRecOrigen
                             , 'ALMACEN OPERATIVO' tpRecOrigen 	
                             , a.C_ID idProdOrigen	     
                             , a.E_ID idRecDestino
                             , 'ALMACEN OPERATIVO' tpRecDestino 
                             , a.AT_C_ID2 idProdDestino	     
                             , CAST(a.AT_NSV AS DECIMAL(15, 3)) vFuente
                             , 'BLS' vUM
                             , CAST(0.90718474*a.AT_NSW AS DECIMAL(15, 3)) wFuente
                             , 'TM' wUM
                             , CONVERT(DECIMAL(10, 1), IIF(abs(a.AT_NSV) = 0 or abs(a.AT_NSW) = 0, 0, ((141.5*0.999016) / (((0.90718474*abs(a.AT_NSW)) / abs(a.AT_NSV)) * 6.28981)) - 131.5)) API
                             , '' nbMuestra
                             , '' numPedido
                             , '' posPedido
                             , 'N/A' uomPedido
                             , 0 cantPedido
                             , 1 idMovSigno
                FROM offsite.offsite.ACC_TRANS a
                WHERE 1 = 1
                AND a.AT_TYPE = 10
                AND a.AT_TIMESTAMP >= @ConsultaIni
                AND a.AT_TIMESTAMP <= @ConsultaFin
                AND (abs(a.AT_NSV) + abs(a.AT_NSW)) > 0
                UNION ALL
                SELECT CONCAT(CONVERT(VARCHAR(10), EOMONTH(@ConsultaIni), 112), '-', Ta.E_ID) Tag
                     , CONVERT(DATETIME, EOMONTH(@ConsultaIni), 120) dtMovIni
                     , DATEADD(second, 86399, CONVERT(DATETIME, EOMONTH(@ConsultaIni), 120)) dtMovFin	
                     , 'PIEMS' tpCategoria
                     , Ta.E_ID idRecOrigen
                     , 'ALMACEN OPERATIVO' tpRecOrigen
                     , a.C_ID idProdOrigen
                     , 'PERDIDA IDENTIFICADA' idRecDestino
                     , 'RECURSO PERDIDA' tpRecDestino
                     , a.C_ID idProdDestino
                     , Ta.vFuente
                     , 'BLS' vUM
                     , Ta.wFuente
                     , 'TM' wUM
                     , CONVERT(DECIMAL(10, 1), IIF(abs(Ta.vFuente) = 0 or abs(Ta.wFuente) = 0, 0, ((141.5*0.999016) / (((0.90718474*abs(Ta.wFuente)) / abs(Ta.vFuente)) * 6.28981)) - 131.5)) API	
                     , '' nbMuestra	
                     , '' numPedido	
                     , '' posPedido	
                     , 'N/A' uomPedido	
                     , 0 cantPedido
                     , 1 idMovSigno
                FROM ( SELECT t.E_ID
                             , CONVERT(DECIMAL(10, 2), SUM(ABS(t.AT_NSV))) vFuente
                             , CONVERT(DECIMAL(10, 2), SUM(ABS(t.AT_NSW*0.90718474))) wFuente
                             , MAX(t.AT_ID) AT_ID
                       FROM OFFSITE.offsite.ACC_TRANS t 
                       WHERE 1 = 1
                       AND t.AT_TYPE = 14  
                       AND t.AT_TIMESTAMP >= DATEADD(day, 1, convert(DATETIME, EOMONTH(@ConsultaIni, -1), 120))
                       AND t.AT_TIMESTAMP < DATEADD(day, 1, EOMONTH(@ConsultaIni))
                       GROUP BY t.E_ID
                       ) Ta 
                INNER JOIN OFFSITE.offsite.ACC_TRANS a ON (Ta.AT_ID = a.AT_ID)
                WHERE (Ta.vFuente + Ta.wFuente) <> 0
                UNION ALL
                SELECT mvi.OM_ID Tag
                     , IIF (mvi.RN = 1, IIF(mvi.AOM_ACT_START_DT < @ConsultaIni, CAST(DATEADD(DAY, -1, mvi.AOM_FINISH_DT) AS DATE), mvi.AOM_ACT_START_DT), mvf.AOM_FINISH_DT) dtMovIni
                     , mvi.AOM_ACT_FINISH_DT dtMovFin
                     , mvi.O_CAT_TYPE tpCategoria
                     , mvi.E_ID_FROM idRecOrigen
                     , (CASE WHEN eo.ET_ID IN ('TANK', 'TANKS') THEN 'ALMACEN OPERATIVO' 
                             WHEN eo.ET_ID IN ('UNITS') THEN 'UNIDAD DE PROCESO'			     
                             ELSE 'RECURSO ROMSS' END) tpRecOrigen
                     , (CASE WHEN ed.ET_ID IN ('TANK', 'TANKS', 'UNITS') THEN ed.C_ID ELSE mvi.C_ID END) idProdOrigen
                     , mvi.E_ID_TO idRecDestino
                     , (CASE WHEN ed.ET_ID IN ('TANK', 'TANKS') THEN 'ALMACEN OPERATIVO' 
                             WHEN ed.ET_ID IN ('UNITS') THEN 'UNIDAD DE PROCESO'			     
                             ELSE 'RECURSO ROMSS' END) tpRecDestino
                     , (CASE WHEN ISNULL(a.AT_TYPE, 0) IN (3, 4) THEN a.AT_C_ID2 ELSE mvi.C_ID END) idProdDestino			 
                     , CONVERT(DECIMAL(10, 2), mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0)) vFuente
                     , 'BLS' vUM
                     , CONVERT(DECIMAL(10, 2), 0.90718474*(mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0))) wFuente 
                     , 'TM' wUM
                     , CONVERT(DECIMAL(10, 1), (CASE WHEN (mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0)) > 0 THEN ((141.5*0.999016) / (((mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0)) / (mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0))) * 6.28981)) - 131.5 ELSE ISNULL(omv.OM_MV_AVG_APIDENS_SYS, 0) END)) API
                     , ISNULL(mvi.AOM_SAMPLE_NR, '') nbMuestra
                     , o.O_RELEASE_NR numPedido
                     , m.SAP_POSITION_ID posPedido
                     , IIF(UPPER(u.SOURCE_UOM) = 'BBLS', 'BLS', u.SOURCE_UOM) uomPedido
                     , CONVERT(DECIMAL(15, 2), IIF(UPPER(u.TARGET_UOM) = 'BBLS', (isnull(mvi.AOM_REPORT_NSV, 0) - isnull(mvf.AOM_REPORT_NSV, 0)), (isnull(mvi.AOM_REPORT_NSW, 0) - isnull(mvf.AOM_REPORT_NSW, 0)))/u.FACTOR) cantPedido
                     , 1 idMovSigno
                FROM ( SELECT ROW_NUMBER() OVER (PARTITION BY OM_ID ORDER BY AOM_FINISH_DT) RN, a.*
                       FROM offsite.offsite.ACC_ORDER_MV a
                       WHERE 1 = 1 
                       AND ((AOM_ACT_START_DT >= @ConsultaIni AND AOM_ACT_START_DT <= @ConsultaFin) OR 
                            (AOM_FINISH_DT >= @ConsultaIni AND AOM_FINISH_DT <= @ConsultaFin))
                      ) mvi
                LEFT JOIN ( SELECT ROW_NUMBER() OVER (PARTITION BY OM_ID ORDER BY AOM_FINISH_DT) RN, a.*
                            FROM offsite.offsite.ACC_ORDER_MV a
                            WHERE 1 = 1 
                            AND ((AOM_ACT_START_DT >= @ConsultaIni AND AOM_ACT_START_DT <= @ConsultaFin) OR 
                                 (AOM_FINISH_DT >= @ConsultaIni AND AOM_FINISH_DT <= @ConsultaFin))
                            ) mvf ON (mvi.OM_ID = mvf.OM_ID AND mvi.RN = mvf.RN+1) 
                INNER JOIN offsite.offsite.EQUIPMENT eo ON (eo.E_ID = mvi.E_ID_FROM)
                INNER JOIN offsite.offsite.EQUIPMENT ed ON (ed.E_ID = mvi.E_ID_TO)
                LEFT JOIN offsite.offsite.ACC_TRANS a ON a.om_id = mvi.OM_ID
                LEFT JOIN offsite.offsite.ORDERS o ON (o.O_ID = mvi.O_ID)
                LEFT JOIN offsite.offsite.ORDER_MV omv ON (omv.OM_ID = mvi.OM_ID)
                INNER JOIN BARRANCA.SAP.SAP_MOVEMENT m ON (m.SAP_ORDER_ID = o.O_RELEASE_NR)
                INNER JOIN BARRANCA.sap.SAP_UOM_CONVERSION u ON (m.SAP_UOM = u.SOURCE_UOM AND u.ORIGINATOR = 'SAP')
                INNER JOIN barranca.intg.parameters_category pc ON (pc.O_CAT_TYPE = mvi.O_CAT_TYPE)
                WHERE 1 = IIF (pc.SOURCE_TYPE = 'Compras', 
                               IIF( IIF (mvi.RN = 1, IIF(mvi.AOM_ACT_START_DT < @ConsultaIni, CAST(DATEADD(DAY, -1, mvi.AOM_FINISH_DT) AS DATE), mvi.AOM_ACT_START_DT), mvF.AOM_FINISH_DT) >= CONVERT(DATETIME, CONCAT(CONVERT(VARCHAR(10), m.SAP_STARTTIME, 120), ' 00:00:00'))
                                    AND IIF (mvi.RN = 1, IIF(mvi.AOM_ACT_START_DT < @ConsultaIni, CAST(DATEADD(DAY, -1, mvi.AOM_FINISH_DT) AS DATE), mvi.AOM_ACT_START_DT), mvF.AOM_FINISH_DT) <= CONVERT(DATETIME, CONCAT(CONVERT(VARCHAR(10), m.SAP_ENDTIME, 120), ' 23:59:59'))
                               , 1, 0), 1)     
                AND pc.REPORT_NAME = 'RepIntROMSS_ARES'
                AND pc.TAG_ID = 'Pedidos'
                AND omv.MSS_STATUS <> 'DELETED'
                AND ((mvi.AOM_REPORT_NSW - isnull(mvf.AOM_REPORT_NSW, 0)) + (mvi.AOM_REPORT_NSV - isnull(mvf.AOM_REPORT_NSV, 0))) > 0
                AND mvi.MSS_STATUS = 'CLOSED'";

            return await _romssDatabaseService.QueryAsync<RomssMovementModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        #endregion

        #region BCN Queries

        /// <summary>
        /// Query to get inventory data from BCN
        /// </summary>
        public async Task<IEnumerable<BcnInventoryModel>> GetBcnInventoryAsync(DateTime consultaIni, int idCaso = 4)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, a.nbRecFuente, a.nmRecProducto, a.nmRecAlmacen, a.boVoBoAlmacen, a.boFotoInventario, a.idUMFotoInventario, a.nbAPI60, a.CantVolTotal, a.CantVolBombeable, a.CantVolRemanente, a.idUMVolumen, a.CantMasTotal, a.CantMasBombeable, a.CantMasRemanente, a.idUMMasa, a.nbMuestra, a.dtMuestra, a.dtCargado, a.nmUsrAuditoria, a.nmEstado
                FROM BCN.Inventarios_vw a
                WHERE 1 = 1 AND a.idCaso = @IdCaso AND a.dtInventario = @ConsultaIni
                ORDER BY a.idRecProducto, a.nmRecAlmacen";

            return await _bcnDatabaseService.QueryAsync<BcnInventoryModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get consolidated inventory balance data from BCN (using qryGETINVENTARIOSBCN)
        /// </summary>
        public async Task<IEnumerable<BcnInventoryModel>> GetBcnConsolidatedInventoryBalanceAsync(DateTime consultaIni, int idCaso = 5)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, a.nbRecFuente, a.nmRecProducto, a.nmRecAlmacen, a.boVoBoAlmacen, a.boFotoInventario, a.idUMFotoInventario, a.nbAPI60, a.CantVolTotal, a.CantVolBombeable, a.CantVolRemanente, a.idUMVolumen, a.CantMasTotal, a.CantMasBombeable, a.CantMasRemanente, a.idUMMasa, a.nbMuestra, a.dtMuestra, a.dtCargado, a.nmUsrAuditoria, a.nmEstado
                FROM BCN.Inventarios_vw a
                WHERE 1 = 1 AND a.idCaso = @IdCaso AND a.dtInventario = @ConsultaIni
                ORDER BY a.idRecProducto, a.nmRecAlmacen";

            return await _bcnDatabaseService.QueryAsync<BcnInventoryModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get movement data from BCN
        /// </summary>
        public async Task<IEnumerable<BcnMovementModel>> GetBcnMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento = "", int idCaso = 4)
        {
            var sql = @"
                SELECT m.nbMovimientoTag, m.tpMovimientoCls, m.dtMovimientoIni, m.dtMovimientoFin, m.nmRecOrigen, m.nbProdOrigen, m.nmProdOrigen, m.nmRecDestino, m.nbProdDestino, m.vlCantVolFuente, m.vlCantVolReconciliado, m.vlCantVolConciliado, m.idUMCantVol, m.vlCantMasFuente, m.vlCantMasReconciliado, m.vlCantMasConciliado, m.idUMCantMas, m.nbAPI60, m.nbMuestra, m.numPedido, m.posPedido, m.idUMPedido, m.nmEstado, m.dtCargado, m.nmUsrAuditoria
                FROM BCN.Movimientos_vw m
                WHERE 1 = 1
                AND m.idCaso = @IdCaso
                AND m.dtMovimientoIni >= @ConsultaIni 
                AND m.dtMovimientoFin <= @ConsultaFin";

            if (!string.IsNullOrEmpty(filtroMovimiento))
            {
                sql += " AND m.tpMovimientoCls = @FiltroMovimiento";
                return await _bcnDatabaseService.QueryAsync<BcnMovementModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin, FiltroMovimiento = filtroMovimiento });
            }

            return await _bcnDatabaseService.QueryAsync<BcnMovementModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get consolidated movement data from BCN
        /// </summary>
        public async Task<IEnumerable<BcnMovementModel>> GetBcnConsolidatedMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento = "", int idCaso = 5)
        {
            var sql = @"
                SELECT m.nbMovimientoTag, m.tpMovimientoCls, m.dtMovimientoIni, m.dtMovimientoFin, m.nmRecOrigen, m.nbProdOrigen, m.nmProdOrigen, m.nmRecDestino, m.nbProdDestino, m.vlCantVolFuente, m.vlCantVolReconciliado, m.vlCantVolConciliado, m.idUMCantVol, m.vlCantMasFuente, m.vlCantMasReconciliado, m.vlCantMasConciliado, m.idUMCantMas, m.nbAPI60, m.nbMuestra, m.numPedido, m.posPedido, m.idUMPedido, m.nmEstado, m.dtCargado, m.nmUsrAuditoria
                FROM BCN.Movimientos_vw m
                WHERE 1 = 1
                AND m.idCaso = @IdCaso
                AND m.dtMovimientoIni >= @ConsultaIni 
                AND m.dtMovimientoFin <= @ConsultaFin";

            if (!string.IsNullOrEmpty(filtroMovimiento))
            {
                sql += " AND m.tpMovimientoCls = @FiltroMovimiento";
                return await _bcnDatabaseService.QueryAsync<BcnMovementModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin, FiltroMovimiento = filtroMovimiento });
            }

            return await _bcnDatabaseService.QueryAsync<BcnMovementModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get inventory photo from BCN
        /// </summary>
        public async Task<IEnumerable<BcnInventoryPhotoModel>> GetBcnInventoryPhotoAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, a.nbRecFuente, a.nmRecProducto, a.nmRecAlmacen, a.boVoBoAlmacen
                                     , (CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolTotal * cum.vlFactor ELSE a.CantMasTotal * cum.vlFactor END) CantTotal
                                     , (CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END) CantBombeableLU
                                     , (CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END) CantBombeableCC
                                     , (CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END) CantRemanente
                                     , (CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END) + (CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END) CantBloqueada
                                     , a.idUMFotoInventario
                                     , a.nbMuestra
                                     , a.dtMuestra
                                FROM BCN.Inventarios_vw a
                                INNER JOIN bcn.cumconversion cum ON (a.idUMFotoInventario = cum.idUMDestino AND cum.nmSistema = 'FOTOINV') 
                                WHERE 1 = 1
                                AND a.idCaso = 4
                                AND a.bofotoInventario = 'Si'
                                and a.dtInventario = @ConsultaIni";

            return await _bcnDatabaseService.QueryAsync<BcnInventoryPhotoModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get HPI (Provisional Inventory Enablement) movements from ARES
        /// </summary>
        public async Task<IEnumerable<HpiMovementModel>> GetHpiMovementsAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT b.IDMOVIMIENTOSAP Tag
                     , CONVERT(DATETIME2(0), b.FECHAENVIO) dtMovIni
                     , CONVERT(DATETIME2(0), b.FECHAENVIO) dtMovFin
                     , IIF(a.TRANSACTION_ID = 521, 'HPI', 'DPI') tpCategoria
                     , b.TANQUE idRecOrigen
	                 , 'ALMACEN OPERATIVO' tpRecOrigen
                     , a.C_ID idProdOrigen
	                 , b.TANQUE idRecDestino
	                 , 'ALMACEN OPERATIVO' tpRecDestino
	                 , a.C_ID idProdDestino
	                 , a.PumpableNSVbbls vFuente
                     , 'BLS' vUM
	                 , a.PumpableNSWton wFuente
	                 , 'TM' wUM
	                 , a.API60 API
	                 , '' nbMuestra	
	                 , '' numPedido	
	                 , '' posPedido	
	                 , 'N/A' uomPedido	
	                 , 0 cantPedido
	                 , IIF(a.TRANSACTION_ID = 521, 1, -1) idMovSigno	
                FROM XTHABILITACIONINVENTARIOROMSS a 
                INNER JOIN XTHABILITACIONINVENTARIOENVIOSSAP b ON (a.t_id = b.TANQUE AND a.C_DESC = b.PRODUCTO AND CONVERT(DATETIME2(0), a.FECHAENVIO) = CONVERT(DATETIME2(0), b.FECHAENVIO))
                where 1 = 1 
                AND a.MARCADOENVIO IS NOT NULL 
                AND CONVERT(DATE, b.FECHAENVIO) = CONVERT(DATE, @ConsultaIni, 120)";

            return await _aresDatabaseService.QueryAsync<HpiMovementModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get consolidated balance from BCN
        /// </summary>
        public async Task<IEnumerable<BcnConsolidatedBalanceModel>> GetBcnConsolidatedBalanceAsync(DateTime consultaIni, DateTime consultaFin, string tpMovimiento)
        {
            var sql = @"
                SELECT a.idRecurso, a.nbRecurso, a.nmRecurso, a.UM UMBalance
                     , ISNULL(InvIni.InvVolIni, 0) InvIniVol
	                 , ISNULL(MovEnt.vlVolEnt, 0) vlVolEntVol
	                 , ISNULL(MovSal.vlVolSal, 0) vlVolSalVol
                     , ISNULL(InvFin.InvVolFin, 0) InvFinVol 
                     , ((ISNULL(InvFin.InvVolFin, 0) + ISNULL(MovSal.vlVolSal, 0)) - (ISNULL(InvIni.InvVolIni, 0) + ISNULL(MovEnt.vlVolEnt, 0))) vlDesbalanceVol
	                 , 'BLS' UMVol
                     , ISNULL(InvIni.InvMasIni, 0) InvIniMas
	                 , ISNULL(MovEnt.vlMasEnt, 0) vlVolEntMas
	                 , ISNULL(MovSal.vlMasSal, 0) vlVolSalMas
                     , ISNULL(InvFin.InvMasFin, 0) InvFinMas
                     , ((ISNULL(InvFin.InvMasFin, 0) + ISNULL(MovSal.vlMasSal, 0)) - (ISNULL(InvIni.InvMasIni, 0) + ISNULL(MovEnt.vlMasEnt, 0))) vlDesbalanceMas
	                 , 'TM' UMMas
                FROM bcn.jerarquiarecursos_vw a
                LEFT JOIN (SELECT idRecAlmacen, CantVolTotal InvVolIni, CantMasTotal InvMasIni FROM bcn.mInventarios WHERE idCaso = 5 AND dtInventario = DATEADD(MINUTE, -1, @ConsultaIni) ) InvIni ON  InvIni.idRecAlmacen = a.idRecurso
                LEFT JOIN (SELECT idRecAlmacen, CantVolTotal InvVolFin, CantMasTotal InvMasFin FROM bcn.mInventarios WHERE idCaso = 5 AND dtInventario = DATEADD(SECOND, -59, @ConsultaFin) ) InvFin ON  InvFin.idRecAlmacen = a.idRecurso
                LEFT JOIN (SELECT idRecDestino, SUM(vlCantVolConciliado) vlVolEnt, SUM(vlCantMasConciliado) vlMasEnt FROM bcn.mMovimientos WHERE idCaso = 5 AND dtMovimientoIni >= @ConsultaIni AND dtMovimientoFin <= @ConsultaFin GROUP BY idRecDestino) MovEnt ON MovEnt.idRecDestino = a.idRecurso
                LEFT JOIN (SELECT idRecOrigen, SUM(vlCantVolConciliado) vlVolSal, SUM(vlCantMasConciliado) vlMasSal FROM bcn.mMovimientos WHERE idCaso = 5 AND dtMovimientoIni >= @ConsultaIni AND dtMovimientoFin <= @ConsultaFin GROUP BY idRecOrigen) MovSal ON MovSal.idRecOrigen = a.idRecurso
                WHERE 1 = 1
                AND a.Clase = 'BALANCE CONSOLIDADO'
                AND a.tpRecurso = @TpMovimiento
                ORDER BY a.Orden, a.nmRecurso";

            return await _bcnDatabaseService.QueryAsync<BcnConsolidatedBalanceModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin, TpMovimiento = tpMovimiento });
        }

        /// <summary>
        /// Query to get consolidated inventory photo from BCN
        /// </summary>
        public async Task<IEnumerable<BcnConsolidatedInventoryPhotoModel>> GetBcnConsolidatedInventoryPhotoAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, mp.nmRecurso nmProducto
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolTotal * cum.vlFactor ELSE a.CantMasTotal * cum.vlFactor END)) CantTotal
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END)) CantBombeableLU
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END)) CantBombeableCC
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END)) CantRemanente
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END) + (CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END)) CantBloqueada
                                     , a.idUMFotoInventario
                                FROM BCN.Inventarios_vw a
                                INNER JOIN bcn.cumconversion cum ON (a.idUMFotoInventario = cum.idUMDestino AND cum.nmSistema = 'FOTOINV') 
								INNER JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'PRODUCTO LOGISTICO') mp ON (mp.nbRecurso = a.nbRecSAP)
                                WHERE 1 = 1
                                AND a.idCaso = 4
                                AND a.bofotoInventario = 'Si'
                                and a.dtInventario = @ConsultaIni
                                GROUP BY a.dtInventario, a.nbRecSAP, mp.nmRecurso, a.idUMFotoInventario";

            return await _bcnDatabaseService.QueryAsync<BcnConsolidatedInventoryPhotoModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get BCN Balance Rule data
        /// </summary>
        public async Task<IEnumerable<BcnBalanceRuleModel>> GetBcnBalanceRuleAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, mp.nmRecurso nmProducto
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolTotal * cum.vlFactor ELSE a.CantMasTotal * cum.vlFactor END)) CantTotal
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'Si' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END)) CantBombeableLU
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END)) CantBombeableCC
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END)) CantRemanente
                                     , SUM((CASE WHEN cum.idUMDestino = 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantVolBombeable * cum.vlFactor 
                                             WHEN cum.idUMDestino <> 'BLS' AND a.boVoBoAlmacen = 'No' THEN a.CantMasBombeable * cum.vlFactor
                                             ELSE 0 END) + (CASE WHEN cum.idUMDestino = 'BLS' THEN a.CantVolRemanente * cum.vlFactor ELSE a.CantMasRemanente * cum.vlFactor END)) CantBloqueada
                                     , a.idUMFotoInventario
                                FROM BCN.Inventarios_vw a
                                INNER JOIN bcn.cumconversion cum ON (a.idUMFotoInventario = cum.idUMDestino AND cum.nmSistema = 'FOTOINV') 
								INNER JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'PRODUCTO LOGISTICO') mp ON (mp.nbRecurso = a.nbRecSAP)
                                WHERE 1 = 1
                                AND a.idCaso = 4
                                AND a.bofotoInventario = 'Si'
                                and a.dtInventario = @ConsultaIni
                                GROUP BY a.dtInventario, a.nbRecSAP, mp.nmRecurso, a.idUMFotoInventario";

            return await _bcnDatabaseService.QueryAsync<BcnBalanceRuleModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get BCN Balance Difference data
        /// </summary>
        public async Task<IEnumerable<BcnBalanceDifferenceModel>> GetBcnBalanceDifferenceAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT m.nbMovimientoTag, m.tpMovimientoCls, m.dtMovimientoIni, m.dtMovimientoFin, m.nmRecOrigen, m.nbProdOrigen, m.nmProdOrigen, m.nmRecDestino, m.nbProdDestino, m.vlCantVolFuente, m.vlCantVolReconciliado, m.vlCantVolConciliado, m.idUMCantVol, m.vlCantMasFuente, m.vlCantMasReconciliado, m.vlCantMasConciliado, m.idUMCantMas, m.nbAPI60, m.nbMuestra, m.numPedido, m.posPedido, m.idUMPedido, m.nmEstado, m.dtCargado, m.nmUsrAuditoria
                     , (m.vlCantVolFuente - m.vlCantVolConciliado) vlDiferenciaVol
                     , (m.vlCantMasFuente - m.vlCantMasConciliado) vlDiferenciaMas
                     , CASE WHEN (m.vlCantVolFuente - m.vlCantVolConciliado) <> 0 OR (m.vlCantMasFuente - m.vlCantMasConciliado) <> 0 THEN 'DIFERENCIA' ELSE 'CONCILIADO' END tpDiferencia
                FROM BCN.Movimientos_vw m
                WHERE 1 = 1
                AND m.idCaso = 5
                AND m.dtMovimientoIni >= @ConsultaIni 
                AND m.dtMovimientoFin <= @ConsultaFin
                ORDER BY m.dtMovimientoIni DESC, m.nbMovimientoTag";

            return await _bcnDatabaseService.QueryAsync<BcnBalanceDifferenceModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get logistic balance
        /// </summary>
        public async Task<IEnumerable<LogisticBalanceModel>> GetLogisticBalanceAsync(string nbCeLo, DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT a.*, ISNULL(InvI.vlContable, 0) InvIni
                        , ISNULL(MovEnt.vlContable, 0) vlEntradas
                        , ISNULL(MovSal.vlContable, 0) vlSalidas
                        , ISNULL(InvF.vlContable, 0) InvFin
	                    , (ISNULL(InvF.vlContable, 0)) + ISNULL(MovSal.vlContable, 0) - (ISNULL(InvI.vlContable, 0) + ISNULL(MovEnt.vlContable, 0)) vlDesbalance

                FROM BCN.getBalanceLogistico (@NbCeLo) a 

                LEFT JOIN ( SELECT i.nbCenLog, i.nbAlmLog, i.nbMaterial, i.idUM, i.vlContable
                            FROM bcn.mIntInventario i
			                WHERE dtContabilizacion = DATEADD(MINUTE, -1, @ConsultaIni) ) InvI ON InvI.nbCenLog = a.nbCenLog AND InvI.nbAlmLog = a.nbAlmLog AND InvI.nbMaterial = a.nbRecurso

                LEFT JOIN ( SELECT m.nbCenLogDestino nbCenLog, m.nbAlmLogDestino nbAlmLog, m.nbProdLogDestino nbMaterial, m.idUM, sum(m.vlContable) vlContable
                            FROM bcn.mIntMovLogistico m
			                WHERE m.nbCenLogDestino <> '' AND dtContabilizacion = @ConsultaIni
			                GROUP BY m.nbCenLogDestino, m.nbAlmLogDestino, m.nbProdLogDestino, m.idUM) MovEnt ON MovEnt.nbCenLog = a.nbCenLog AND MovEnt.nbAlmLog = a.nbAlmLog AND MovEnt.nbMaterial = a.nbRecurso

                LEFT JOIN ( SELECT i.nbCenLog, i.nbAlmLog, i.nbMaterial, i.idUM, i.vlContable
                            FROM bcn.mIntInventario i
			                WHERE dtContabilizacion = DATEADD(MINUTE, -1, @ConsultaFin) ) InvF ON InvF.nbCenLog = a.nbCenLog AND InvF.nbAlmLog = a.nbAlmLog AND InvF.nbMaterial = a.nbRecurso
                LEFT JOIN ( SELECT m.nbCenLogOrigen nbCenLog, m.nbAlmLogOrigen nbAlmLog, m.nbProdLogOrigen nbMaterial, m.idUM, sum(m.vlContable) vlContable
                            FROM bcn.mIntMovLogistico m
			                WHERE m.nbCenLogOrigen <> '' AND dtContabilizacion = @ConsultaIni
			                GROUP BY m.nbCenLogOrigen, m.nbAlmLogOrigen, m.nbProdLogOrigen, m.idUM) MovSal ON MovSal.nbCenLog = a.nbCenLog AND MovSal.nbAlmLog = a.nbAlmLog AND MovSal.nbMaterial = a.nbRecurso
                ORDER BY a.NivelJrq";

            return await this._bcnDatabaseService.QueryAsync<LogisticBalanceModel>(sql, new { NbCeLo = nbCeLo, ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get logistic inventory for web services
        /// </summary>
        public async Task<IEnumerable<WsLogisticInventoryModel>> GetWsLogisticInventoryAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT a.dtContabilizacion
                     , BCN.getRecursoNm(a.idRecurso) nmRecAlmacen
                     , BCN.getRecursoNm(a.idProducto) nmRecProducto
                     , a.nbCenLog
                     , a.nbAlmLog
                     , a.nbMaterial
                     , a.vlContable
                     , a.idUM
                     , a.nmUsrAuditoria
                     , a.dtUsrAuditoria
                FROM BCN.mIntInventario a
                WHERE 1 = 1
                AND a.dtContabilizacion >= @ConsultaIni 
                AND a.dtContabilizacion <= @ConsultaFin";

            return await this._bcnDatabaseService.QueryAsync<WsLogisticInventoryModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        #endregion

        #region Web Service Queries

        /// <summary>
        /// Query to get cost data for web services
        /// </summary>
        public async Task<IEnumerable<WsCostModel>> GetWsCostsAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT c.idRegCosto
                , (CASE WHEN c.tpObjCosto IN ('CO PLANTA', 'CO POOL') THEN 'COSTOS_' ELSE 'VOLTOTAL_' END) + CAST(c.idRegCosto as nvarchar) idMsgCostos
                , c.dtContabilizacion
                , c.txMovimiento
                , c.tpObjCosto
                , c.idObjCosto
                , (CASE WHEN c.tpObjCosto IN ('CO PLANTA', 'CO POOL') THEN c.idObjCosto ELSE '' END) ObjPlantaPool
                , (CASE WHEN c.tpObjCosto = 'CO COLECTOR' THEN c.idObjCosto ELSE '' END) ObjColector
                , (CASE WHEN c.tpObjCosto = 'CO VOL TOTAL' THEN c.idObjCosto ELSE '' END) ObjVolTotal  
                , c.idValEstadistico ObjEstadistico
                , c.nmProducto
                , c.vlContabilizado vlContable
                , c.idUM
                FROM BCN.mIntCostos c
                WHERE 1 = 1  
                AND txProcesamiento IS NULL
                AND c.dtContabilizacion >= @ConsultaIni 
                AND c.dtContabilizacion <= @ConsultaFin";

            return await _bcnDatabaseService.QueryAsync<WsCostModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        /// <summary>
        /// Query to get logistic movement data for web services
        /// </summary>
        public async Task<IEnumerable<WsLogisticMovementModel>> GetWsLogisticMovementsAsync(DateTime consultaIni, DateTime consultaFin, string filtroMovimiento = "")
        {
            var sql = @"
                SELECT 100000 + ml.idRegMovLogistico idRegMovLogistico
                , ml.dtContabilizacion
                , ml.idMovimientoReg
                , m.dtMovimientoIni
                , m.dtMovimientoFin
                , m.tpMovimientoCls tpMovimiento
                , m.nmRecOrigen
                , m.nmProdOrigen
                , m.nmRecDestino
                , m.nmProdDestino
                , ml.nbMovimientoCls
                , cm.nmMovimientoCls
                , cm.nbGM
                , cm.tpInventario
                , ml.numPedido
                , ml.posPedido
                , ml.idUMPedido      
                , ml.nbCenLogOrigen
                , ml.nbAlmLogOrigen 
                , ISNULL(ro.nmRecurso, '') nmAlmLogOrigen
                , (CASE WHEN LEN(ISNULL(ro.nmRecurso, ''))> 0 THEN m.nmProdOrigen ELSE '' END) nmProdLogOrigen
                , ml.nbProdLogOrigen 
                , ml.nbCenLogDestino
                , ISNULL(rd.nmRecurso, '') nmAlmLogDestino
                , (CASE WHEN LEN(ISNULL(rd.nmRecurso, ''))> 0 THEN m.nmProdDestino ELSE '' END) nmProdLogDestino
                , ml.nbAlmLogDestino
                , ml.nbProdLogDestino
                , ml.vlContable
                , ml.idUM 
                , ml.idCentroCosto
                , (CASE WHEN ml.vlAtrCalidad IS NULL THEN '' ELSE ml.idAtrCalidad END)  idAtrCalidad  
                , (CASE WHEN ml.vlAtrCalidad IS NULL THEN '' ELSE CAST(ROUND(ml.vlAtrCalidad, 3) AS VARCHAR(15)) END) vlAtrCalidad  
                , (CASE WHEN ml.vlAtrCalidad IS NULL THEN '' ELSE ml.idUMAtrCalidad END) idUMAtrCalidad
                , (CASE WHEN ml.vlQCI IS NULL THEN '' ELSE CAST(ml.vlQCI AS VARCHAR(15)) END) vlQCI  
                , (CASE WHEN ml.vlQCI IS NULL THEN '' ELSE ml.idUMQCI END) idUMQCI
                , (CASE WHEN ml.vlQCI IS NULL THEN '' ELSE 'U' END) upQCI
                , (CASE WHEN ml.vlAtrCalidad IS NULL THEN '{}' ELSE '{""Attribute"": {""PropertyQualityID"": """", ""PropertyQuality"": ""' + ml.idAtrCalidad + '"", ""NumberValue"": ""' + CAST(ROUND(ml.vlAtrCalidad, 3) AS VARCHAR(15)) + '"", ""TextValue"": ""Density Liquid"", ""Uom"": ""' + ml.idUMAtrCalidad + '""}}' END)  txAtrCalidad
                , (CASE WHEN ml.vlQCI IS NULL THEN '{}' 
                ELSE (CASE WHEN ml.numPedido <> '' THEN '{""QUANTITY"": {""VALUE"": ""' + CAST(ml.vlContable AS VARCHAR(15)) + '"", ""UOM"": ""' + ml.idUM + '"", ""MODIFYVALUE"": ""U""}}'
                    ELSE '{""QUANTITY"": {""VALUE"": ""' + CAST(ml.vlQCI AS VARCHAR(15)) + '"", ""UOM"": ""' + ml.idUMQCI + '"", ""MODIFYVALUE"": ""U""}}' END) END) txQCI
                , ml.idPropiedad
                , ml.dtProcesamiento
                , ml.txProcesamiento
            FROM BCN.mIntMovLogistico ml
            INNER JOIN BCN.cclasemovimientos cm ON (ml.nbMovimientoCls = cm.nbMovimientoCls)
            INNER JOIN BCN.Movimientos_vw m ON (ml.idMovimientoReg = m.idMovimientoReg)
            LEFT JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'ALMACEN LOGISTICO') ro ON (ro.nbRecurso = (ml.nbCenLogOrigen + ':' + ml.nbAlmLogOrigen))
            LEFT JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'PRODUCTO LOGISTICO') rop ON (rop.nbRecurso = ml.nbProdLogOrigen)
            LEFT JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'ALMACEN LOGISTICO') rd ON (rd.nbRecurso = (ml.nbCenLogDestino + ':' + ml.nbAlmLogDestino))
            LEFT JOIN (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'PRODUCTO LOGISTICO') rdp ON (rdp.nbRecurso = ml.nbProdLogDestino)
            WHERE 1 = 1
            AND ml.dtContabilizacion >= @ConsultaIni 
            AND ml.dtContabilizacion <= @ConsultaFin
            [FiltroMovimiento]";

            sql = sql.Replace("[FiltroMovimiento]", filtroMovimiento);

            var results = await _bcnDatabaseService.QueryAsync<WsLogisticMovementModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });

            return results;
        }

        #endregion

        #region BCN Comparison Queries

        /// <summary>
        /// Query to get BCN inventory comparison data - Option 06
        /// </summary>
        public async Task<IEnumerable<BcnInventoryComparisonModel>> GetBcnInventoryComparisonAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, a.nmRecProducto, a.nmRecAlmacen, a.boVoBoAlmacen, a.boFotoInventario, a.idUMFotoInventario, a.nbAPI60, a.CantVolTotal, a.CantVolBombeable, a.CantVolRemanente, a.idUMVolumen, a.CantMasTotal, a.CantMasBombeable, a.CantMasRemanente, a.idUMMasa, a.nbMuestra, a.dtMuestra, a.dtCargado, a.nmUsrAuditoria, a.nmEstado
                FROM BCN.Inventarios_vw a
                WHERE 1 = 1 AND a.idCaso = 4 AND a.dtInventario = @ConsultaIni
                ORDER BY a.idRecProducto, a.nmRecAlmacen";

            return await _bcnDatabaseService.QueryAsync<BcnInventoryComparisonModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get BCN cost comparison data - Option 07
        /// </summary>
        public async Task<IEnumerable<BcnCostComparisonModel>> GetBcnCostComparisonAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT c.idRegCosto, c.dtContabilizacion, c.txMovimiento, c.tpObjCosto, c.idObjCosto, c.idValEstadistico, c.nmProducto, c.vlContabilizado, c.idUM
                FROM BCN.mIntCostos c
                WHERE 1 = 1
                AND c.dtContabilizacion >= @ConsultaIni
                AND c.dtContabilizacion <= @ConsultaFin
                ORDER BY c.dtContabilizacion DESC";

            return await _bcnDatabaseService.QueryAsync<BcnCostComparisonModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        #endregion

        #region BCN Queries to view datatable

        /// <summary>
        /// Query to get detailed inventory data from BCN inventario operativo table
        /// </summary>
        public async Task<IEnumerable<BcnInventoryDetailModel>> GetBcnInventoryDetailAsync(DateTime consultaIni, int idCaso)
        {
            var sql = @"
                SELECT a.dtInventario, a.nbRecSAP, a.nbRecFuente, a.nmRecProducto, a.nmRecAlmacen, a.boVoBoAlmacen, a.boFotoInventario, a.idUMFotoInventario, a.nbAPI60, a.CantVolTotal, a.CantVolBombeable, a.CantVolRemanente, a.idUMVolumen, a.CantMasTotal, a.CantMasBombeable, a.CantMasRemanente, a.idUMMasa, a.nbMuestra, a.dtMuestra, a.dtCargado, a.nmUsrAuditoria, a.nmEstado
                FROM BCN.Inventarios_vw a
                WHERE 1 = 1 AND a.idCaso = @IdCaso AND a.dtInventario = @ConsultaIni
                ORDER BY a.idRecProducto, a.nmRecAlmacen";

            return await _bcnDatabaseService.QueryAsync<BcnInventoryDetailModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get movement data from BCN movimientos view
        /// </summary>
        public async Task<IEnumerable<BcnMovementViewModel>> GetBcnMovementViewAsync(DateTime consultaIni, DateTime consultaFin, int idCaso = 4, string filtroMovimiento = "", string tpMovimiento = "")
        {
            var sql = @"
                SELECT m.nbMovimientoTag, m.tpMovimientoCls, m.dtMovimientoIni, m.dtMovimientoFin, m.nmRecOrigen, m.nbProdOrigen, m.nmProdOrigen, m.nmRecDestino, m.nbProdDestino, m.vlCantVolFuente, m.vlCantVolReconciliado, m.vlCantVolConciliado, m.idUMCantVol, m.vlCantMasFuente, m.vlCantMasReconciliado, m.vlCantMasConciliado, m.idUMCantMas, m.nbAPI60, m.nbMuestra, m.numPedido, m.posPedido, m.idUMPedido, m.nmEstado, m.dtCargado, m.nmUsrAuditoria
                     , (CASE WHEN m.idCaso = 5 THEN 
                         CONVERT(NVARCHAR(MAX),
                           (SELECT b.idRecOrigen, b.nbMovimientoTag Tag, a.nbMultiplicador Signo, b.nmRecOrigen, b.idProdOrigen, b.nmProdOrigen, b.idRecDestino, b.nmRecDestino, b.idProdDestino, b.nmProdDestino, b.vlCantVolFuente, b.vlCantVolReconciliado, b.vlCantVolConciliado, b.idUMCantVol 
                            FROM BCN.appReglaConsolidacion a 
                            INNER JOIN BCN.movimientos_vw b on (a.idFuente = b.idFuente AND a.idCaso = b.idCaso AND a.tpMovimientoCls = b.tpMovimientoCls AND ISNULL(a.idRecOrigen, b.idRecOrigen) = b.idRecOrigen AND ISNULL(a.idProdOrigen, b.idProdOrigen) = b.idProdOrigen AND ISNULL(a.idRecDestino, b.idRecDestino) = b.idRecDestino AND ISNULL(a.idProdDestino, b.idProdDestino) = b.idProdDestino) 
                            WHERE 1 = 1 
                            AND b.idCaso = 4 -- Caso Operativo 
                            AND b.dtMovimientoIni >= m.dtMovimientoIni 
                            AND b.dtMovimientoFin <= m.dtMovimientoFin 
                            AND a.nbMAPTag = m.nbMovimientoTag 
                            FOR XML PATH('reg'), root('Movimientos'))) 
                       ELSE '' END) txXML
                FROM BCN.Movimientos_vw m
                WHERE 1 = 1
                AND m.idCaso = @IdCaso
                AND m.dtMovimientoIni >= @ConsultaIni 
                AND m.dtMovimientoFin <= @ConsultaFin
                AND (CASE WHEN @FiltroMovimiento = '=' THEN 
                           CASE WHEN m.tpMovimientoCls = @TpMovimiento THEN 1 ELSE 0 END
                     WHEN @FiltroMovimiento = '<>' THEN 
                           CASE WHEN m.tpMovimientoCls <> @TpMovimiento THEN 1 ELSE 0 END
                     ELSE 1 END) = 1";

            return await _bcnDatabaseService.QueryAsync<BcnMovementViewModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin, FiltroMovimiento = filtroMovimiento, TpMovimiento = tpMovimiento });
        }

        #region ARES Processing Review Queries

        /// <summary>
        /// Query to get ARES Logistic Movement Processing Review - Option 04
        /// </summary>
        public async Task<IEnumerable<AresLogisticProcessingReviewModel>> GetAresLogisticProcessingReviewAsync(DateTime consultaIni)
        {
            var sql = @"
                SELECT idMovimiento
                , 'Documento: ' + DOCUMENTORESPUESTA txProcesamiento
                , ISNULL(REPLACE(JSON_VALUE(JSONRECIBIDO, '$.ProcessingTime'), 'T', ' '), '') dtProcesamiento
                , dtContabilizacion
                , Estado
                FROM XTMOVIMIENTOSSAPBIC 
                WHERE Estado = 'OK'  
                AND dtContabilizacion = @ConsultaIni";

            return await _aresDatabaseService.QueryAsync<AresLogisticProcessingReviewModel>(sql, new { ConsultaIni = consultaIni });
        }

        /// <summary>
        /// Query to get ARES Cost Movement Processing Review - Option 05
        /// </summary>
        public async Task<IEnumerable<AresCostProcessingReviewModel>> GetAresCostProcessingReviewAsync(DateTime consultaIni, DateTime consultaFin)
        {
            var sql = @"
                SELECT tpObjCostos
                , idObjCosto
                , idValEstadistico
                , dtContabilizacion
                , 'Documento: ' + DOCUMENTORESPUESTA txProcesamiento
                , FECHAENVIO dtProcesamiento
                , Estado
                FROM XTCOSTOSSAPBIC 
                WHERE Estado = 'OK' 
                AND dtContabilizacion >= @ConsultaIni 
                AND dtContabilizacion <= @ConsultaFin";

            return await _aresDatabaseService.QueryAsync<AresCostProcessingReviewModel>(sql, new { ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }

        #endregion

        #endregion

        /// <summary>
        /// Query to get BCN Balance Operativo data - Option 08
        /// Based on Python query BALANCEOPER
        /// </summary>
        public async Task<IEnumerable<BcnBalanceOperativoModel>> GetBcnBalanceOperativoAsync(DateTime consultaIni, DateTime consultaFin, int idCaso = 4)
        {
            var sql = @"
                SELECT ROW_NUMBER() OVER (ORDER BY InvFin.idRecProducto, a.nmRecurso) as NbRN
                     , a.idRecurso
                     , InvFin.nbRecSAP as NbRecurso
                     , a.nmRecurso 
                     , '' as UMBalance
                     , BCN.getRecursoNm(InvIni.idRecProducto) as NmProductoIni
                     , BCN.getRecursoNm(InvFin.idRecProducto) as NmProductoFin
                     , ISNULL(InvIni.InvVolIni, 0) as InvIniVol
                     , ISNULL(MovEnt.vlVolEnt, 0) as VlEntVol
                     , ISNULL(MovSal.vlVolSal, 0) as VlSalVol
                     , ISNULL(InvFin.InvVolFin, 0) as InvFinVol
                     , ((ISNULL(InvFin.InvVolFin, 0) + ISNULL(MovSal.vlVolSal, 0)) - (ISNULL(InvIni.InvVolIni, 0) + ISNULL(MovEnt.vlVolEnt, 0))) as VlDesbalanceVol
                     , 'BLS' as UMVol
                     , ISNULL(InvIni.InvMasIni, 0) as InvIniMas
                     , ISNULL(MovEnt.vlMasEnt, 0) as VlEntMas
                     , ISNULL(MovSal.vlMasSal, 0) as VlSalMas
                     , ISNULL(InvFin.InvMasFin, 0) as InvFinMas
                     , ((ISNULL(InvFin.InvMasFin, 0) + ISNULL(MovSal.vlMasSal, 0)) - (ISNULL(InvIni.InvMasIni, 0) + ISNULL(MovEnt.vlMasEnt, 0))) as VlDesbalanceMas
                     , 'TM' as UMMas
                     , (CASE WHEN ISNULL(InvFin.InvVolFin, 0) = 0 THEN 0 
                           ELSE (6.29600604148465*((0.90718474*InvFin.InvMasFin)/InvFin.InvVolFin)) END) as SGInvFin
                     , (((CASE WHEN ISNULL(InvFin.InvVolFin, 0) = 0 THEN 0 ELSE (6.29600604148465*((0.90718474*InvFin.InvMasFin)/InvFin.InvVolFin)) END)*0.9991657784-0.001199407795)*0.1589872949) as FC
                FROM (SELECT * FROM BCN.mrecursos WHERE tpRecurso = 'ALMACEN OPERATIVO') a
                LEFT JOIN (SELECT idRecAlmacen, dtInventario, idUMFotoInventario, CantVolTotal as InvVolIni, CantMasTotal as InvMasIni, nbAPI60, idRecProducto, nbRecSAP 
                          FROM BCN.Inventarios_vw 
                          WHERE idCaso = @IdCaso AND dtInventario = DATEADD(MINUTE, -1, @ConsultaIni)) InvIni 
                          ON InvIni.idRecAlmacen = a.idRecurso
                LEFT JOIN (SELECT idRecAlmacen, dtInventario, idUMFotoInventario, CantVolTotal as InvVolFin, CantMasTotal as InvMasFin, nbAPI60, idRecProducto, nbRecSAP 
                          FROM BCN.Inventarios_vw 
                          WHERE idCaso = @IdCaso AND dtInventario = DATEADD(SECOND, -59, @ConsultaFin)) InvFin 
                          ON InvFin.idRecAlmacen = a.idRecurso
                LEFT JOIN (SELECT idRecDestino, SUM(vlCantVolConciliado) as vlVolEnt, SUM(vlCantMasConciliado) as vlMasEnt 
                          FROM BCN.mMovimientos 
                          WHERE idCaso = @IdCaso AND dtMovimientoIni >= @ConsultaIni AND dtMovimientoFin <= @ConsultaFin 
                          GROUP BY idRecDestino) MovEnt ON MovEnt.idRecDestino = a.idRecurso
                LEFT JOIN (SELECT idRecOrigen, SUM(vlCantVolConciliado) as vlVolSal, SUM(vlCantMasConciliado) as vlMasSal 
                          FROM BCN.mMovimientos 
                          WHERE idCaso = @IdCaso AND dtMovimientoIni >= @ConsultaIni AND dtMovimientoFin <= @ConsultaFin 
                          GROUP BY idRecOrigen) MovSal ON MovSal.idRecOrigen = a.idRecurso
                WHERE 1 = 1
                AND InvFin.idRecProducto IS NOT NULL 
                ORDER BY InvFin.idRecProducto, a.nmRecurso";

            return await _bcnDatabaseService.QueryAsync<BcnBalanceOperativoModel>(sql, new { IdCaso = idCaso, ConsultaIni = consultaIni, ConsultaFin = consultaFin });
        }
    }
}
