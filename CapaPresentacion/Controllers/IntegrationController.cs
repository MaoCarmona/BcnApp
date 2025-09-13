using DotNetBcnModule.Presentation.App_Start;
using DotNetBcnModule.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NetBcnModule.Services;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Queries;
using NetBcnModule.Services.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;
using System.Net;

namespace NetBcnModule.Presentation.Controllers
{
    /// <summary>
    /// Controller for handling data integration operations
    /// </summary>
    [RoutePrefix("api/integration")]
    public class IntegrationController : Controller
    {
        private readonly IBcnModuleService _bcnModuleService;
        private readonly ILoggingService _loggingService;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IQueriesService _queriesService;
        private readonly IAresWebService _aresWebService;

        // Parameterless constructor for ASP.NET MVC
        public IntegrationController()
            : this(
                DependencyInjection.GetServiceProvider().GetService<IBcnModuleService>(),
                DependencyInjection.GetServiceProvider().GetService<ILoggingService>(),
                DependencyInjection.GetServiceProvider().GetService<IDataProcessingService>(),
                DependencyInjection.GetServiceProvider().GetService<IQueriesService>(),
                DependencyInjection.GetServiceProvider().GetService<IAresWebService>())
        {
        }

        // Constructor for DI
        public IntegrationController(
            IBcnModuleService bcnModuleService,
            ILoggingService loggingService,
            IDataProcessingService dataProcessingService,
            IQueriesService queriesService,
            IAresWebService aresWebService)
        {
            _bcnModuleService = bcnModuleService ?? throw new ArgumentNullException(nameof(bcnModuleService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _queriesService = queriesService ?? throw new ArgumentNullException(nameof(queriesService));
            _aresWebService = aresWebService ?? throw new ArgumentNullException(nameof(aresWebService));
        }

        /// <summary>
        /// Normalizes date range: start date to 00:00:00 and end date to 23:59:59
        /// </summary>
        private string GetModuleTitle(string type, string option)
        {
            switch (type)
            {
                case "integrar":
                    switch (option)
                    {
                        case "01": return "AORA: Inventario operativo";
                        case "02": return "AORA: Movimientos operativos";
                        case "03": return "AORA: Flujos operativo";
                        case "04": return "ROMSS: Inventario operativo";
                        case "05": return "ROMSS: Movimientos operativos";
                        case "06": return "BCN: Foto inventario operativo";
                        case "07": return "ARES: Movimientos HPI";
                        case "08": return "BCN: Balance operativo";
                        default: return "Integrar Información";
                    }
                case "consolidar":
                    switch (option)
                    {
                        case "01": return "BCN: Inventarios";
                        case "02": return "BCN: Movimientos";
                        case "03": return "BCN: Balance ALMACEN";
                        case "04": return "BCN: Balance POOL";
                        case "05": return "BCN: Balance UNIDAD DE PROCESO";
                        case "06": return "BCN: Foto inventario";
                        case "07": return "BCN: Corregir Bal. Sig. Contrario";
                        case "08": return "BCN: Aplicar Regla de Balance";
                        case "09": return "BCN: Diferencia Balance";
                        default: return "Consolidar Información";
                    }
                case "logistica":
                    switch (option)
                    {
                        case "01": return "Movimientos logísticos";
                        case "02": return "Movimientos de costos";
                        case "03": return "Balance GRB CeLo: 2000";
                        case "04": return "Balance Reexpido CeLo: 3501";
                        case "05": return "Balance Impala CeLo: 4130";
                        default: return "Transformación Logística";
                    }
                case "ares":
                    switch (option)
                    {
                        case "01": return "BCN: Inventario logístico";
                        case "02": return "BCN: Movimiento logístico";
                        case "03": return "BCN: Movimiento de costos";
                        case "04": return "ARES: Rev. Procesamiento Logistico";
                        case "05": return "ARES: Rev. Procesamiento Costo";
                        case "06": return "BCN: Comparativo Inventario";
                        case "07": return "BCN: Comparativo Costos";
                        default: return "Envío BCN WS-ARES";
                    }
                default:
                    return "Reporte BCN Module";
            }
        }

        // private (DateTime? startDate, DateTime? endDate) NormalizeDateRange(string fechaIni, string fechaFin)
        // {
        //     DateTime? consultaIni = string.IsNullOrEmpty(fechaIni) ? (DateTime?)null : DateTime.Parse(fechaIni);
        //     DateTime? consultaFin = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.Parse(fechaFin);

        //     if (consultaIni.HasValue)
        //     {
        //         consultaIni = consultaIni.Value.Date; // Already 00:00:00
        //     }
        //     if (consultaFin.HasValue)
        //     {
        //         consultaFin = consultaFin.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        //     }

        //     return (consultaIni, consultaFin);
        // }

        /// <summary>
        /// Normalize date range for specific operations that need different date handling
        /// This replicates Python's exact behavior for different option types
        /// </summary>
        private (DateTime? startDate, DateTime? endDate) NormalizeDateRangeForOption(string fechaIni, string fechaFin, string option, bool useInitialDate, bool isConsolidated = false)
        {
            DateTime? consultaIni = string.IsNullOrEmpty(fechaIni) ? (DateTime?)null : DateTime.ParseExact(fechaIni, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime? consultaFin = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.ParseExact(fechaFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (consultaIni.HasValue)
            {
                consultaIni = consultaIni.Value.Date; // Already 00:00:00
            }
            if (consultaFin.HasValue)
            {
                consultaFin = consultaFin.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return (consultaIni, consultaFin);
        }

        /// <summary>
        /// Normalize date range for consolidar information operations with specific rules
        /// Replicates Python InfConsolidado behavior exactly
        /// </summary>
        private (DateTime? startDate, DateTime? endDate) NormalizeDateRangeForConsolidarOption(string fechaIni, string fechaFin, string option, bool useInitialDate)
        {
            DateTime? consultaIni = string.IsNullOrEmpty(fechaIni) ? (DateTime?)null : DateTime.ParseExact(fechaIni, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime? consultaFin = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.ParseExact(fechaFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (!consultaIni.HasValue || !consultaFin.HasValue)
            {
                return (consultaIni, consultaFin);
            }

            switch (option)
            {
                case "01": // BCN: Inventarios consolidados
                case "06": // BCN: Foto inventarios consolidados
                    if (useInitialDate)
                    {
                        // Python: vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
                        consultaIni = consultaIni.Value.AddMinutes(-1);
                        _loggingService.WriteInfo($"BCN Inventario Consolidado Inicial - Aplicando regla Python: {fechaIni} -> {FormatDateTime(consultaIni)}");
                    }
                    else
                    {
                        // Python: vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)
                        consultaFin = consultaFin.Value.AddSeconds(-59);
                        _loggingService.WriteInfo($"BCN Inventario Consolidado Final - Aplicando regla Python: {fechaFin} -> {FormatDateTime(consultaFin)}");
                    }
                    break;

                case "02": // BCN: Movimientos consolidados
                case "03": // BCN: Balance ALMACEN
                case "04": // BCN: Balance POOL
                case "05": // BCN: Balance UNIDAD DE PROCESO
                case "07": // BCN: Corregir Balance Signo Contrario
                case "08": // BCN: Aplicar Regla de Balance
                case "09": // BCN: Diferencia Balance
                    // Python: usar fechas tal como están, sin modificaciones
                    _loggingService.WriteInfo($"BCN Consolidado Movimientos/Balance - Aplicando regla Python: fechas sin modificar");
                    break;

                default:
                    _loggingService.WriteInfo($"Opción consolidar {option} - Sin reglas específicas, usando fechas tal como están");
                    break;
            }

            return (consultaIni, consultaFin);
        }

        /// <summary>
        /// Normalize date range for integrar information operations with specific rules
        /// 
        /// Para ROMSS (opción 04):
        /// - Si viewDataIntegrar = true (botón Visualizar): Aplica reglas de AORA
        ///   - RN01: Inventario Inicial - descontar 1 minuto a Fecha Desde
        ///   - RN02: Inventario Final - descontar 59 segundos a Fecha Final
        /// - Si viewDataIntegrar = false (botón Ejecutar): Aplica reglas específicas de ROMSS
        ///   - RN04: Inventario Inicial - usar Fecha Desde tal como está
        ///   - RN05: Inventario Final - aumentar 1 segundo a Fecha Final
        /// 
        /// Para otras opciones:
        /// RN03: AORA Movimientos/Flujos - usar fechas tal como están
        /// RN07: ROMSS Movimientos/Flujos - usar fechas tal como están
        /// </summary>
        private (DateTime? startDate, DateTime? endDate) NormalizeDateRangeForIntegrarOption(string fechaIni, string fechaFin, string option, bool useInitialDate, bool viewDataIntegrar)
        {
            DateTime? consultaIni = string.IsNullOrEmpty(fechaIni) ? (DateTime?)null : DateTime.ParseExact(fechaIni, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime? consultaFin = string.IsNullOrEmpty(fechaFin) ? (DateTime?)null : DateTime.ParseExact(fechaFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (!consultaIni.HasValue || !consultaFin.HasValue)
            {
                return (consultaIni, consultaFin);
            }

            // Aplicar reglas específicas según la opción y el tipo de fecha
            switch (option)
            {
                case "01": // AORA: Inventario operativo
                    if (useInitialDate)
                    {
                        // RN01: Inventario Inicial - descontar 1 minuto a Fecha Desde
                        consultaIni = consultaIni.Value.AddMinutes(-1);
                        _loggingService.WriteInfo($"AORA Inventario Inicial - Aplicando RN01: {fechaIni} -> {FormatDateTime(consultaIni)}");
                    }
                    else
                    {
                        // RN02: Inventario Final - descontar 59 segundos a Fecha Final
                        consultaFin = consultaFin.Value.AddSeconds(-59);
                        _loggingService.WriteInfo($"AORA Inventario Final - Aplicando RN02: {fechaFin} -> {FormatDateTime(consultaFin)}");
                    }
                    break;

                case "04": // ROMSS: Inventario operativo
                    if (viewDataIntegrar)
                     {
                         // Para visualización, ROMSS usa las reglas de AORA
                         if (useInitialDate)
                         {
                             // RN01: Inventario Inicial - descontar 1 minuto a Fecha Desde
                             consultaIni = consultaIni.Value.AddMinutes(-1);
                             _loggingService.WriteInfo($"ROMSS Inventario Inicial (Visualizar) - Aplicando RN01: {fechaIni} -> {FormatDateTime(consultaIni)}");
                         }
                         else
                         {
                             // RN02: Inventario Final - descontar 59 segundos a Fecha Final
                             consultaFin = consultaFin.Value.AddSeconds(-59);
                             _loggingService.WriteInfo($"ROMSS Inventario Final (Visualizar) - Aplicando RN02: {fechaFin} -> {FormatDateTime(consultaFin)}");
                         }
                     }
                    else
                    {
                        // Para ejecución, ROMSS usa sus reglas específicas
                        if (useInitialDate)
                        {
                            // RN04: Inventario Inicial - usar Fecha Desde tal como está
                            _loggingService.WriteInfo($"ROMSS Inventario Inicial (Ejecutar) - Aplicando RN04: {fechaIni} -> {FormatDateTime(consultaIni)}");
                        }
                        else
                        {
                            // RN05: Inventario Final - aumentar 1 segundo a Fecha Final
                            consultaFin = consultaFin.Value.AddSeconds(1);
                            _loggingService.WriteInfo($"ROMSS Inventario Final (Ejecutar) - Aplicando RN05: {fechaFin} -> {FormatDateTime(consultaFin)}");
                        }
                    }
                    break;
                    

                case "02": // AORA: Movimientos operativo
                case "03": // AORA: Flujos operativo
                    // RN03: Movimientos y Flujos - usar fechas tal como están
                    _loggingService.WriteInfo($"AORA Movimientos/Flujos - Aplicando RN03: fechas sin modificar");
                    break;


                case "05": // ROMSS: Movimientos operativo
                    // RN07: ROMSS Movimientos - sumar 1 segundo a Fecha Final (igual que Python)
                    consultaFin = consultaFin.Value.AddSeconds(1);
                    _loggingService.WriteInfo($"ROMSS Movimientos - Aplicando RN07: {fechaFin} -> {FormatDateTime(consultaFin)} (+1 segundo)");
                    break;

                case "06": // BCN: Foto inventario operativo
                    if (useInitialDate)
                    {
                        // RN01: Inventario Inicial - descontar 1 minuto a Fecha Desde
                        consultaIni = consultaIni.Value.AddMinutes(-1);
                        _loggingService.WriteInfo($"BCN Inventario Inicial - Aplicando RN01: {fechaIni} -> {FormatDateTime(consultaIni)}");
                    }
                    else
                    {
                        // RN02: Inventario Final - descontar 59 segundos a Fecha Final
                        consultaFin = consultaFin.Value.AddSeconds(-59);
                        _loggingService.WriteInfo($"BCN Inventario Final - Aplicando RN02: {fechaFin} -> {FormatDateTime(consultaFin)}");
                    }
                    break;

                case "07": // ARES: Movimientos HPI
                    // RN03: Movimientos - usar fechas tal como están
                    _loggingService.WriteInfo($"ARES Movimientos - Aplicando RN03: fechas sin modificar");
                    break;

                default:
                    _loggingService.WriteInfo($"Opción {option} - Sin reglas específicas, usando fechas tal como están");
                    break;
            }

            return (consultaIni, consultaFin);
        }

        private async Task<QueryResult> IntegrarView(string option, string fechaIni, string fechaFin, bool useInitialDate = false, bool viewDataIntegrar = false)
        {
            try
            {
                // Parse the already normalized dates - ensure they are in yyyy-MM-dd HH:mm:ss format
                var consultaIni = DateTime.ParseExact(fechaIni, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var consultaFin = DateTime.ParseExact(fechaFin, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                
                _loggingService.WriteInfo($"IntegrarView called: option={option}, fechaIni={fechaIni}, fechaFin={fechaFin}, useInitialDate={useInitialDate}, viewDataIntegrar={viewDataIntegrar}");
                _loggingService.WriteInfo($"IntegrarView - Fechas ya normalizadas: consultaIni={FormatDateTime(consultaIni)}, consultaFin={FormatDateTime(consultaFin)}");


                // Validate that we have valid dates
                if (consultaIni == default(DateTime) || consultaFin == default(DateTime))
                {
                    return new QueryResult
                    {
                        Message = "Fechas de inicio y fin son requeridas para la consulta",
                        Data = new List<Dictionary<string, object>>(),
                        Columns = new List<string>()
                    };
                }

                // Handle inventory options (01, 04, 06)
                if (option == "01" || option == "04" || option == "06")
                {
                    var inventoryDate = useInitialDate ? consultaIni : consultaFin;
                    
                    if (option == "04")
                    {
                        _loggingService.WriteInfo($"[ROMSS INVENTORY VIEW] User clicked 'Visualizar' for ROMSS Inventario Operativo");
                        _loggingService.WriteInfo($"[ROMSS INVENTORY VIEW] Date range: {FormatDateTime(consultaIni)} to {FormatDateTime(consultaIni)}");
                        _loggingService.WriteInfo($"[ROMSS INVENTORY VIEW] Using inventory date: {FormatDateTime(inventoryDate)} (useInitialDate={useInitialDate})");
                    }
                    else
                    {
                        _loggingService.WriteInfo($"BCN Inventory: using inventoryDate={inventoryDate} (useInitialDate={useInitialDate})");
                    }
                    
                    var dataResult = await _queriesService.GetBcnInventoryDetailAsync(inventoryDate, idCaso: 4);
                    var dtoList = dataResult.Select((model, index) => new BcnInventoryDetailDto
                    {
                        Item = index + 1,
                        Producto = model.NmRecProducto, 
                        Almacen = model.NmRecAlmacen,
                        FotoInv = model.BoFotoInventario,
                        VoBo = model.BoVoBoAlmacen,
                        API = model.NbAPI60 ?? 0,
                        VolumenTotal = model.CantVolTotal ?? 0,
                        VolumenBombeable = model.CantVolBombeable ?? 0,
                        VolumenRemanente = model.CantVolRemanente ?? 0,
                        UMVolumen = model.IdUMVolumen,
                        MasaTotal = model.CantMasTotal ?? 0,
                        MasaBombeable = model.CantMasBombeable ?? 0,
                        MasaRemanente = model.CantMasRemanente ?? 0,
                        UMMasa = model.IdUMMasa,
                        IdMuestra = model.NbMuestra,
                        // IdMuestraOrigen = model.IdRecProducto,
                        Estado = model.NmEstado
                    }).ToList();

                    return ConvertToQueryResult(dtoList);
                }
                // Handle movement options (02, 03, 05)
                else if ( option == "02" || option == "03" || option == "05" )
                {
                    string filtroMovimiento = option == "03" ? "=" : "<>";
                    string tpMovimiento = "LIMBAT";
                    var dataResult = await _queriesService.GetBcnMovementViewAsync(consultaIni, consultaFin, idCaso: 4, filtroMovimiento, tpMovimiento);
                    var dtoList = dataResult.Select((model, index) => {
                        var fechaFinAux = model.dtMovimientoFin;
                        if (model.dtMovimientoIni.HasValue && model.dtMovimientoFin.HasValue && model.dtMovimientoIni.Value.Date == model.dtMovimientoFin.Value.Date && model.dtMovimientoFin.Value.TimeOfDay == TimeSpan.Zero)
                        {
                            fechaFinAux = model.dtMovimientoFin.Value.Date.AddHours(23).AddMinutes(59);
                        }
                        
                        return new BcnMovementDto
                        {
                            Item = index + 1,
                            Tag = model.nbMovimientoTag,
                            TipoMov = model.tpMovimientoCls,
                            FechaInicio = FormatDateTime(model.dtMovimientoIni),
                            FechaFin = FormatDateTime(fechaFinAux),
                            RecOrigen = model.nmRecOrigen,
                            ProdOrigen = model.nmProdOrigen,
                            RecDestino = model.nmRecDestino,
                            ProdDestino = model.nmProdDestino,
                            FuenteVolumen = model.vlCantVolFuente ?? 0,
                            ReconciliadoVolumen = model.vlCantVolReconciliado ?? 0,
                            ConciliadoVolumen = model.vlCantVolConciliado ?? 0,
                            UMVolumen = model.idUMCantVol,
                            FuenteMasa = model.vlCantMasFuente ?? 0,
                            ReconciliadoMasa = model.vlCantMasReconciliado ?? 0,
                            ConciliadoMasa = model.vlCantMasConciliado ?? 0,
                            UMMasa = model.idUMCantMas,
                            API = model.nbAPI60,
                            IDMuestra = model.nbMuestra,
                            NumPedido = model.numPedido,
                            PosPedido = model.posPedido,
                            UMPedido = model.idUMPedido,
                            Estado = model.nmEstado
                        };
                    }).ToList();

                    var result = ConvertToQueryResult(dtoList);

                    if (option == "03" || option == "02" )
                    {
                        result.Columns.Remove("Producto Destino");
                        foreach (var row in result.Data)
                        {
                            row.Remove("Producto Destino");
                        }

                        var tipoMovIndex = result.Columns.IndexOf("Tipo Mov.");
                        if (tipoMovIndex != -1)
                        {
                            result.Columns[tipoMovIndex] = "Tipo Flujo";
                        }
                        foreach (var row in result.Data)
                        {
                            if (row.ContainsKey("Tipo Mov."))
                            {
                                var value = row["Tipo Mov."];
                                row.Remove("Tipo Mov.");
                                row["Tipo Flujo"] = value;
                            }
                        }
                    }

                    return result;
                }
                // Handle HPI movements option (07)
                else if (option == "07")
                {
                    var dataResult = await _queriesService.GetHpiMovementsAsync(consultaIni);
                    var dtoList = dataResult.Select((model, index) => new BcnMovementDto
                    {
                        Item = index + 1,
                        Tag = model.Tag,
                        TipoMov = model.TpCategoria,
                        FechaInicio = FormatDateTime(model.DtMovIni),
                        FechaFin = FormatDateTime(model.DtMovFin),
                        RecOrigen = model.IdRecOrigen,
                        ProdOrigen = model.IdProdOrigen,
                        RecDestino = model.IdRecDestino,
                        ProdDestino = model.IdProdDestino,
                        FuenteVolumen = model.VFuente,
                        ReconciliadoVolumen = 0, // HPI no tiene reconciliado
                        ConciliadoVolumen = 0,   // HPI no tiene conciliado
                        UMVolumen = model.VUM ?? "",
                        FuenteMasa = model.WFuente,
                        ReconciliadoMasa = 0,   
                        ConciliadoMasa = 0,   
                        UMMasa = model.WUM ?? "",
                        API = model.API.ToString() ?? "",
                        IDMuestra = model.NbMuestra ?? "",
                        NumPedido = model.NumPedido ?? "",
                        PosPedido = model.PosPedido ?? "",
                        UMPedido = model.UomPedido ?? "",
                        Estado = "PROCESADO"
                    }).ToList();

                    return ConvertToQueryResult(dtoList);
                }
                // Handle balance option (08)
                else if (option == "08")
                {
                    var dataResult = await _queriesService.GetBcnBalanceOperativoAsync(consultaIni, consultaFin, idCaso: 4);
                    var dtoList = dataResult.Select((model, index) => new BcnBalanceOperativoDto
                    {
                        Item = index + 1,
                        IdRecurso = model.IdRecurso,
                        NbRecurso = model.NbRecurso,
                        NmRecurso = model.NmRecurso,
                        UMBalance = model.UMBalance,
                        NmProductoIni = model.NmProductoIni,
                        NmProductoFin = model.NmProductoFin,
                        InvIniVol = model.InvIniVol ?? 0,
                        VlEntVol = model.VlEntVol ?? 0,
                        VlSalVol = model.VlSalVol ?? 0,
                        InvFinVol = model.InvFinVol ?? 0,
                        VlDesbalanceVol = model.VlDesbalanceVol ?? 0,
                        UMVol = model.UMVol,
                        InvIniMas = model.InvIniMas ?? 0,
                        VlEntMas = model.VlEntMas ?? 0,
                        VlSalMas = model.VlSalMas ?? 0,
                        InvFinMas = model.InvFinMas ?? 0,
                        VlDesbalanceMas = model.VlDesbalanceMas ?? 0,
                        UMMas = model.UMMas,
                        SGInvFin = model.SGInvFin ?? 0,
                        FC = model.FC ?? 0
                    }).ToList();

                    return ConvertToQueryResult(dtoList);
                }
                else
                {
                    // Invalid option
                    return new QueryResult
                    {
                        Message = $"Opción '{option}' no válida para visualización de datos integrados. Opciones válidas: 01, 02, 03, 04, 05, 06, 07, 08, 09, 10",
                        Data = new List<Dictionary<string, object>>(),
                        Columns = new List<string>()
                    };
                }
            }
            catch (System.Exception ex)
            {
                _loggingService.WriteError($"Resultado en la integración: {ex.Message}");
                return new QueryResult
                {
                    Message = $"Resultado en la integración de datos: {ex.Message}",
                    Data = new List<Dictionary<string, object>>(),
                    Columns = new List<string>()
                };
            }
        }

        /// <summary>
        /// Comprehensive dynamic query that handles all operation types
        /// 
        /// useInitialDate parameter:
        /// - true: Use fechaIni (initial date) for inventory queries
        /// - false: Use fechaFin (final date) for inventory queries (default)
        /// 
        /// This parameter affects inventory-related queries that need to choose between
        /// initial and final dates for their date filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> DynamicQuery(string option, string fechaIni, string fechaFin, string type = null, bool useInitialDate = false, CancellationToken cancellationToken = default, bool viewDataIntegrar = false)
        {
            try
            {
                _loggingService.WriteInfo($"DynamicQuery called: option={option}, type={type}, fechaIni={fechaIni}, fechaFin={fechaFin}, useInitialDate={useInitialDate}, viewDataIntegrar={viewDataIntegrar}");
                QueryResult result;

                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Handle query operations (get data) - when no type is specified
                DateTime? consultaIni, consultaFin;
                
                // Apply date normalization based on type and context
                if (viewDataIntegrar && type == "integrar")
                {
                    // For integrar visualization, use specific rules that consider viewDataIntegrar
                    (consultaIni, consultaFin) = NormalizeDateRangeForIntegrarOption(fechaIni, fechaFin, option, useInitialDate, viewDataIntegrar);
                }
                else if (type == "consolidar")
                {
                    // Use specific consolidar date rules that replicate Python behavior exactly
                    (consultaIni, consultaFin) = NormalizeDateRangeForConsolidarOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else if (type == "logistica" && (option == "03" || option == "04" || option == "05"))
                {
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else if (type == "ares" && (option == "04" || option == "05" || option == "06" || option == "07"))
                {
                    // Python: opciones 04 y 05 usan fechaIni, opciones 06 y 07 usan fechaFin
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else
                {
                    // Default normalization for other cases
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }

                // Validate dates
                if (consultaFin != null && consultaIni != null)
                {
                    if (consultaFin < consultaIni)
                    {
                        return Json(new { success = false, message = "La fecha de fin no puede ser menor a la fecha de inicio" });
                    }
                }

                if (viewDataIntegrar && type == "integrar")
                {
                    // Use the already normalized dates from above - ensure they are in standard format
                    var fechaIniFormatted = consultaIni?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                    var fechaFinFormatted = consultaFin?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                    result = await IntegrarView(option, fechaIniFormatted, fechaFinFormatted, useInitialDate, viewDataIntegrar);
                }
                else
                {
                    result = await ExecuteQueryByTypeAndOption(type, option, consultaIni, consultaFin, useInitialDate, viewDataIntegrar, cancellationToken);
                }
                // Execute query based on type and option

                // Ensure the "Item" column is always correctly numbered
                result = EnsureSequentialItemColumn(result);

                _loggingService.WriteInfo($"Result lengh data: {result.Data.Count}");
                _loggingService.WriteInfo($"Result lengh columns: {result.Columns.Count}");
                _loggingService.WriteInfo($"Result lengh recordCount: {result.RecordCount}");
                _loggingService.WriteInfo($"Result lengh message: {result.Message}");
                _loggingService.WriteInfo($"Result lengh success: {result.Success}");

                return LargeJsonResult(new
                {
                    success = result.Success,
                    columns = result.Columns,
                    data = result.Data,
                    recordCount = result.RecordCount,
                    message = result.Message
                });
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("DynamicQuery operation was cancelled by user");
                return Json(new { success = false, message = "Operación cancelada por el usuario" });
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in DynamicQuery: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Execute Envío BCN WS-ARES queries (options 01-03)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> ExecuteAresWbService(
            string option,
            string inventoryData,
            string movementData,
            string costData,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _loggingService.WriteInfo($"ExecuteAresWbService called: option={option}");
                List<AresInventoryPayload> inventoryDataDto = JsonConvert.DeserializeObject<List<AresInventoryPayload>>(inventoryData ?? "");
                List<AresMovementPayload> movementDataDto = JsonConvert.DeserializeObject<List<AresMovementPayload>>(movementData ?? "");
                List<AresCostPayload> costDataDto = JsonConvert.DeserializeObject<List<AresCostPayload>>(costData ?? "");
                _loggingService.WriteInfo($"Deserealizar objetos");
                switch (option)
                {
                    case "01": // Inventario logístico para ARES
                        if (inventoryData == null || !inventoryData.Any())
                        {
                            return Json(new QueryResult { Message = "No inventory data to send to ARES webservice" });
                        }
                        var result1 = await _bcnModuleService.GetAresLogisticInventoryAsync(inventoryDataDto, cancellationToken);
                        return LargeJsonResult(result1);
                        
                    case "02": // Movimiento logístico para ARES
                        if (movementData == null || !movementData.Any())
                        {
                            return Json(new QueryResult { Message = "No movement data to send to ARES webservice" });
                        }
                        var result2 = await _bcnModuleService.GetAresLogisticMovementAsync(movementDataDto, cancellationToken);
                        return LargeJsonResult(result2);
                        
                    case "03": // Movimiento de costos para ARES
                        if (costData == null || !costData.Any())
                        {
                            return Json(new QueryResult { Message = "No cost data to send to ARES webservice" });
                        }
                        var result3 = await _bcnModuleService.GetAresCostMovementAsync(costDataDto, cancellationToken);
                        return LargeJsonResult(result3);
                        
                    default:
                        return Json(new QueryResult { Message = $"Unknown ares option: {option}" });
                }
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("ExecuteAresWbService operation was cancelled by user");
                return Json(new QueryResult { Message = "Operación cancelada por el usuario" });
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ExecuteAresWbService: {ex.Message}");
                return Json(new QueryResult { Message = $"Error processing ARES request: {ex.Message}" });
            }
        }

        /// <summary>
        /// Consolidation endpoint that replicates Python InfConsolidado functionality
        /// </summary>
        [HttpPost]
        [Route("consolidate")]
        public async Task<ActionResult> ConsolidateData(string option, string fechaIni, string fechaFin, string userAudit = "AdminBCN", bool useInitialDate = false)
        {
            try
            {
                _loggingService.WriteInfo($"ConsolidateData called: option={option}, fechaIni={fechaIni}, fechaFin={fechaFin}, userAudit={userAudit}, useInitialDate={useInitialDate}");

                // Parse dates using specific consolidar rules - ensure they are in standard format
                var (consultaIni, consultaFin) = NormalizeDateRangeForConsolidarOption(fechaIni, fechaFin, option, useInitialDate);
                _loggingService.WriteInfo($"Consulta ini: {FormatDateTime(consultaIni)}, consulta fin: {FormatDateTime(consultaFin)}");
                // Validate dates
                if (consultaFin != null && consultaIni != null)
                {
                    if (consultaFin < consultaIni)
                    {
                        return Json(new { success = false, message = "La fecha de fin no puede ser menor a la fecha de inicio" });
                    }
                    if (consultaFin > DateTime.Today)
                    {
                        return Json(new { success = false, message = "La fecha de fin no puede ser mayor a la fecha actual" });
                    }
                    if (consultaIni > DateTime.Today)
                    {
                        return Json(new { success = false, message = "La fecha de inicio no puede ser mayor a la fecha actual" });
                    }
                }

                var dateFrom = consultaIni ?? DateTime.Today;
                var dateTo = consultaFin ?? DateTime.Today;

                _loggingService.WriteInfo($"Starting consolidation process: option={option}, dateFrom={FormatDateTime(dateFrom)}, dateTo={FormatDateTime(dateTo)}");

                // Process consolidation using DataProcessingService (replicates Python InfConsolidado logic)
                var consolidationResult = await _dataProcessingService.ProcessConsolidatedInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo, false); // Default to false for consolidation

                if (consolidationResult.Success)
                {
                    _loggingService.WriteInfo($"Consolidation successful: {consolidationResult.RecordCount} records processed");
                    
                    return Json(new
                    {
                        success = true,
                        message = $"Consolidación exitosa: {consolidationResult.RecordCount} registros procesados",
                        recordCount = consolidationResult.RecordCount,
                        xmlPackages = consolidationResult.XmlItems.Count,
                        consolidationType = GetConsolidationTypeDescription(option)
                    });
                }
                else
                {
                    _loggingService.WriteError($"Consolidation failed: {consolidationResult.Message}");
                    return Json(new
                    {
                        success = false,
                        message = $"Error en consolidación: {consolidationResult.Message}"
                    });
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ConsolidateData: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get description for consolidation type
        /// </summary>
        private string GetConsolidationTypeDescription(string option)
        {
            switch (option)
            {
                case "01": return "BCN: Inventarios consolidados";
                case "02": return "BCN: Movimientos consolidados";
                case "03": return "BCN: Balance ALMACEN";
                case "04": return "BCN: Balance POOL";
                case "05": return "BCN: Balance UNIDAD DE PROCESO";
                case "06": return "BCN: Foto inventario consolidado";
                case "07": return "BCN: Aplicar Regla de Balance";
                case "08": return "BCN: Diferencia Balance";
                default: return $"Opción desconocida: {option}";
            }
        }

        /// <summary>
        /// Execute query based on type and option number
        /// </summary>
        private async Task<QueryResult> ExecuteQueryByTypeAndOption(string type, string option, DateTime? consultaIni, DateTime? consultaFin, bool useInitialDate = false, bool viewDataIntegrar = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(type))
            {
                type = "integrar";
            }

            QueryResult result;
            switch (type.ToLower())
            {
                case "integrar":
                    result = await ExecuteIntegrarQuery(option, consultaIni, consultaFin, useInitialDate, viewDataIntegrar, cancellationToken);
                    break;
                
                case "consolidar":
                    result = await ExecuteConsolidarQuery(option, consultaIni, consultaFin, useInitialDate, cancellationToken);
                    break;
                
                case "logistica":
                    result = await ExecuteLogisticaQuery(option, consultaIni, consultaFin, useInitialDate, cancellationToken);
                    break;
                
                case "ares":
                    result = await ExecuteAresQuery(option, consultaIni, consultaFin, useInitialDate, cancellationToken);
                    break;
                
                default:
                    result = new QueryResult { Message = $"Unknown type: {type}" };
                    break;
            }

            var propertyToDisplayMapping = GetPropertyToDisplayMapping(type, option);

            // Ensure result has valid properties to avoid null pointer exceptions
            if (result.Columns == null)
            {
                result.Columns = new List<string>();
            }

            if (result.Data == null)
            {
                result.Data = new List<Dictionary<string, object>>();
            }

            result.Columns = GetCustomColumnNames(type, option, result.Columns);
            result.Data = TransformDataToMatchColumns(result.Data, propertyToDisplayMapping);
            
            return result;
        }

        /// <summary>
        /// Apply filters to the data (replicates client-side filtering logic)
        /// </summary>
        private QueryResult ApplyFiltersToData(QueryResult result, string searchTerm, string columnFiltersJson)
        {
            if (result?.Data == null || result.Data.Count == 0)
                return result;

            var filteredData = new List<Dictionary<string, object>>(result.Data);

            // Apply global search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                filteredData = filteredData.Where(row =>
                {
                    return result.Columns.Any(column =>
                    {
                        var cellValue = row.ContainsKey(column) ? row[column]?.ToString() : "";
                        return !string.IsNullOrEmpty(cellValue) && cellValue.ToLower().Contains(searchTermLower);
                    });
                }).ToList();
            }

            // Apply column-specific filters
            if (!string.IsNullOrEmpty(columnFiltersJson))
            {
                try
                {
                    var columnFilters = System.Web.Helpers.Json.Decode<Dictionary<string, string>>(columnFiltersJson);
                    
                    foreach (var filter in columnFilters)
                    {
                        var columnName = filter.Key;
                        var filterValue = filter.Value.ToLower();
                        
                        filteredData = filteredData.Where(row =>
                        {
                            var cellValue = row.ContainsKey(columnName) ? row[columnName]?.ToString() : "";
                            return !string.IsNullOrEmpty(cellValue) && cellValue.ToLower().Contains(filterValue);
                        }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.WriteError($"Error parsing column filters: {ex.Message}");
                }
            }

            return new QueryResult
            {
                Message = result.Message,
                Data = filteredData,
                Columns = result.Columns
            };
        }

        /// <summary>
        /// Remove empty columns from the data
        /// </summary>
        private QueryResult RemoveEmptyColumns(QueryResult result)
        {
            if (result?.Data == null || result.Data.Count == 0 || result.Columns == null)
                return result;

            var columnsToKeep = new List<string>();
            var columnHasData = new Dictionary<string, bool>();

            // Initialize all columns as empty
            foreach (var column in result.Columns)
            {
                columnHasData[column] = false;
            }

            // Check each row to see which columns have data
            foreach (var row in result.Data)
            {
                foreach (var column in result.Columns)
                {
                    if (row.ContainsKey(column) && row[column] != null)
                    {
                        var value = row[column].ToString();
                        if (!string.IsNullOrEmpty(value) && value.Trim() != "")
                        {
                            columnHasData[column] = true;
                        }
                    }
                }
            }

            // Keep only columns that have data
            foreach (var column in result.Columns)
            {
                if (columnHasData[column])
                {
                    columnsToKeep.Add(column);
                }
            }

            // If no columns have data, keep at least one column to avoid empty Excel
            if (columnsToKeep.Count == 0 && result.Columns.Count > 0)
            {
                columnsToKeep.Add(result.Columns[0]);
            }

            // Create new data with only the columns that have data
            var cleanedData = new List<Dictionary<string, object>>();
            foreach (var row in result.Data)
            {
                var newRow = new Dictionary<string, object>();
                foreach (var column in columnsToKeep)
                {
                    if (row.ContainsKey(column))
                    {
                        newRow[column] = row[column];
                    }
                    else
                    {
                        newRow[column] = "";
                    }
                }
                cleanedData.Add(newRow);
            }

            _loggingService.WriteInfo($"RemoveEmptyColumns - Original columns: {result.Columns.Count}, Kept columns: {columnsToKeep.Count}");
            _loggingService.WriteInfo($"RemoveEmptyColumns - Removed columns: {string.Join(", ", result.Columns.Except(columnsToKeep))}");

            return new QueryResult
            {
                Message = result.Message,
                Data = cleanedData,
                Columns = columnsToKeep
            };
        }

        /// <summary>
        /// Remove empty columns but preserve important ones like Item, Producto, Almacén, etc.
        /// </summary>
        private QueryResult RemoveEmptyColumnsPreservingImportant(QueryResult result)
        {
            if (result?.Data == null || result.Data.Count == 0 || result.Columns == null)
                return result;

            // Define important columns that should always be preserved
            var importantColumns = new HashSet<string> { "Item", "Producto", "Almacén", "Tag", "Fecha", "Estado" };
            
            var columnsToKeep = new List<string>();
            var columnHasData = new Dictionary<string, bool>();

            // Initialize all columns as empty
            foreach (var column in result.Columns)
            {
                columnHasData[column] = false;
            }

            // Check each row to see which columns have data
            foreach (var row in result.Data)
            {
                foreach (var column in result.Columns)
                {
                    if (row.ContainsKey(column) && row[column] != null)
                    {
                        var value = row[column].ToString();
                        if (!string.IsNullOrEmpty(value) && value.Trim() != "")
                        {
                            columnHasData[column] = true;
                        }
                    }
                }
            }

            // Always keep important columns, even if they appear empty
            foreach (var column in result.Columns)
            {
                if (columnHasData[column] || importantColumns.Contains(column))
                {
                    columnsToKeep.Add(column);
                }
            }

            // If no columns have data, keep at least one column to avoid empty Excel
            if (columnsToKeep.Count == 0 && result.Columns.Count > 0)
            {
                columnsToKeep.Add(result.Columns[0]);
            }

            // Create new data with only the columns that have data
            var cleanedData = new List<Dictionary<string, object>>();
            foreach (var row in result.Data)
            {
                var newRow = new Dictionary<string, object>();
                foreach (var column in columnsToKeep)
                {
                    if (row.ContainsKey(column))
                    {
                        newRow[column] = row[column];
                    }
                    else
                    {
                        newRow[column] = "";
                    }
                }
                cleanedData.Add(newRow);
            }

            _loggingService.WriteInfo($"RemoveEmptyColumnsPreservingImportant - Original columns: {result.Columns.Count}, Kept columns: {columnsToKeep.Count}");
            _loggingService.WriteInfo($"RemoveEmptyColumnsPreservingImportant - Removed columns: {string.Join(", ", result.Columns.Except(columnsToKeep))}");

            return new QueryResult
            {
                Message = result.Message,
                Data = cleanedData,
                Columns = columnsToKeep
            };
        }

        /// <summary>
        /// Validate that the data is suitable for Excel export
        /// </summary>
        private bool ValidateDataForExport(QueryResult result)
        {
            if (result?.Data == null || result.Data.Count == 0)
            {
                _loggingService.WriteWarning("ValidateDataForExport - No hay datos para validar");
                return false;
            }

            if (result.Columns == null || result.Columns.Count == 0)
            {
                _loggingService.WriteWarning("ValidateDataForExport - No hay columnas para validar");
                return false;
            }

            // Check if all rows have the same number of columns
            var expectedColumnCount = result.Columns.Count;
            for (int i = 0; i < result.Data.Count; i++)
            {
                var row = result.Data[i];
                if (row == null)
                {
                    _loggingService.WriteWarning($"ValidateDataForExport - Fila {i} es null");
                    return false;
                }

                // Ensure all expected columns exist in the row
                foreach (var column in result.Columns)
                {
                    if (!row.ContainsKey(column))
                    {
                        _loggingService.WriteWarning($"ValidateDataForExport - Fila {i} no contiene la columna '{column}'");
                        return false;
                    }
                }
            }

            _loggingService.WriteInfo($"ValidateDataForExport - Datos válidos: {result.Data.Count} filas, {result.Columns.Count} columnas");
            return true;
        }

        /// <summary>
        /// Format decimal values to always show 3 decimal places (.000 if no decimals)
        /// </summary>
        private string FormatDecimalWith3Places(object value)
        {
            if (value == null)
                return "0.000";

            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("0.000", CultureInfo.InvariantCulture);
            }
            
            if (value is double doubleValue)
            {
                return doubleValue.ToString("0.000", CultureInfo.InvariantCulture);
            }
            
            if (value is float floatValue)
            {
                return floatValue.ToString("0.000", CultureInfo.InvariantCulture);
            }
            
            if (value is int intValue)
            {
                return intValue.ToString("0.000", CultureInfo.InvariantCulture);
            }
            
            if (value is long longValue)
            {
                return longValue.ToString("0.000", CultureInfo.InvariantCulture);
            }

            // Try to parse as decimal
            if (decimal.TryParse(value.ToString(), out decimal parsedDecimal))
            {
                return parsedDecimal.ToString("0.000", CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        /// <summary>
        /// Format DateTime to standard format yyyy-MM-dd HH:mm:ss
        /// </summary>
        private string FormatDateTime(DateTime? dateTime)
        {
            if (dateTime == null)
                return "";
            return dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Format DateTime to standard format yyyy-MM-dd HH:mm:ss
        /// </summary>
        private string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Get a safe cell value for Excel export, handling nulls and special characters
        /// </summary>
        private object GetSafeCellValue(Dictionary<string, object> rowData, string columnName)
        {
            if (rowData == null || !rowData.ContainsKey(columnName))
            {
                return "";
            }

            var value = rowData[columnName];
            if (value == null)
            {
                return "";
            }

            try
            {
                // Check if this is a numeric column that should be formatted with 3 decimal places
                var numericColumns = new HashSet<string> 
                { 
                    "API", "Volumen Total", "Volumen Bombeable", "Volumen Remanente", "Masa Total", "Masa Bombeable", "Masa Remanente",
                    "Fuente Volumen", "Reconciliado Volumen", "Conciliado Volumen", "Fuente Masa", "Reconciliado Masa", "Conciliado Masa",
                    "Inv. Inicial Volumen", "Entradas Volumen", "Salidas Volumen", "Inv. Final Volumen", "Desbalance Volumen",
                    "Inv. Inicial Masa", "Entradas Masa", "Salidas Masa", "Inv. Final Masa", "Desbalance Masa",
                    "SG Inv. Final", "FC", "Valor Contable", "Valor Contabilizado", "Cantidad Total", "Cantidad Bombeable LU",
                    "Cantidad Bombeable CC", "Cantidad Bloqueada", "Inv. Inicial", "Entradas", "Salidas", "Inv. Final", "Desbalance"
                };

                if (numericColumns.Contains(columnName))
                {
                    // For numeric columns, return the actual numeric value, not a formatted string
                    if (value is decimal decimalValue)
                    {
                        return Math.Round(decimalValue, 3);
                    }
                    if (value is double doubleValue)
                    {
                        return Math.Round(doubleValue, 3);
                    }
                    if (value is float floatValue)
                    {
                        return Math.Round(floatValue, 3);
                    }
                    if (value is int intValue)
                    {
                        return intValue;
                    }
                    if (value is long longValue)
                    {
                        return longValue;
                    }
                    // Try to parse as decimal
                    if (decimal.TryParse(value.ToString(), out decimal parsedDecimal))
                    {
                        return Math.Round(parsedDecimal, 3);
                    }
                }

                var stringValue = value.ToString();
                
                // Handle special cases
                if (stringValue == "null" || stringValue == "NULL")
                {
                    return "";
                }

                // Truncate very long strings to avoid Excel issues
                if (stringValue.Length > 32000)
                {
                    _loggingService.WriteWarning($"GetSafeCellValue - Valor truncado para columna '{columnName}': {stringValue.Length} caracteres");
                    return stringValue.Substring(0, 32000) + "...";
                }

                return stringValue;
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"GetSafeCellValue - Error procesando valor para columna '{columnName}': {ex.Message}");
                return "Error";
            }
        }

        /// <summary>
        /// Get custom column names based on query type and option
        /// Always returns ALL columns from the property mapping, not just the ones that exist in the data
        /// </summary>
        private List<string> GetCustomColumnNames(string type, string option, List<string> originalColumns)
        {
            // Create a mapping dictionary for property names to display names
            var propertyToDisplayMapping = GetPropertyToDisplayMapping(type, option);
            
            // Always return ALL display names from the mapping, regardless of what's in originalColumns
            // This ensures all expected columns are present, even if they're blank
            return propertyToDisplayMapping.Values.ToList();
        }

        /// <summary>
        /// Get property to display name mapping based on query type and option
        /// </summary>
        private Dictionary<string, string> GetPropertyToDisplayMapping(string type, string option)
        {
            switch (type.ToLower())
            {
                case "integrar":
                    return GetIntegrarPropertyMapping(option);
                
                case "consolidar":
                    return GetConsolidarPropertyMapping(option);
                
                case "logistica":
                    return GetLogisticaPropertyMapping(option);
                
                case "ares":
                    return GetAresPropertyMapping(option);
                
                default:
                    return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Get property mapping for Integrar Información queries
        /// </summary>
        private Dictionary<string, string> GetIntegrarPropertyMapping(string option)
        {
            var mapping = new Dictionary<string, string>();
            
            switch (option)
            {
                case "01": // AORA: Inventario operativo
                    // Python HTML columns: Item, Producto, Almacen, Foto Inv., VoBo, API, Volumen (Total, Bombeable, Remanente, UM), Masa (Total, Bombeable, Remanente, UM), Muestra (ID, Fecha), Estado
                    // Actual data model properties from Queries.cs: NbRN, DtInventario, IdRecOrigen, NmRecOrigen, IdProdOrigen, NmProdOrigen, VFuente, VUM, WFuente, WUM
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Producto", "Producto" },
                        { "Almacen", "Almacén" },
                        { "Foto Inv.", "Foto Inv." },
                        { "VoBo", "VoBo" },
                        { "API", "API" },
                        { "Volumen Total", "Volumen Total" },
                        { "Volumen Bombeable", "Volumen Bombeable" },
                        { "Volumen Remanente", "Volumen Remanente" },
                        { "UM Volumen", "UM Volumen" },
                        { "Masa Total", "Masa Total" },
                        { "Masa Bombeable", "Masa Bombeable" },
                        { "Masa Remanente", "Masa Remanente" },
                        { "UM Masa", "UM Masa" },
                        { "ID Muestra", "ID Muestra" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "02": // AORA: Movimientos operativo
                    // Original mapping plus new fields for completeness
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tag", "Tag" },
                        { "Fecha Inicio", "Fecha Inicio" },
                        { "Fecha Fin", "Fecha Fin" },
                        { "Recurso Origen", "Recurso Origen" },
                        { "Producto Origen", "Producto Origen" },
                        { "Recurso Destino", "Recurso Destino" },
                        { "Fuente Volumen", "Volumen Fuente" },
                        { "Reconciliado Volumen", "Volumen Reconciliado" },
                        { "Conciliado Volumen", "Volumen Conciliado" },
                        { "UM Volumen", "UM Volumen" },
                        { "Fuente Masa", "Masa Fuente" },
                        { "Reconciliado Masa", "Masa Reconciliado" },
                        { "Conciliado Masa", "Masa Conciliado" },
                        { "UM Masa", "UM Masa" },
                        { "API", "API" },
                        { "ID Muestra", "ID Muestra" },
                        { "Num. Pedido", "Número Pedido" },
                        { "Pos. Pedido", "Posición Pedido" },
                        { "UM Pedido", "UM Pedido" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "03": // AORA: Flujos operativo
                    // Original mapping plus new fields for completeness
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tag", "Tag" },
                        { "Fecha Inicio", "Fecha Inicio" },
                        { "Fecha Fin", "Fecha Fin" },
                        { "Recurso Origen", "Recurso Origen" },
                        { "Producto Origen", "Producto Origen" },
                        { "Recurso Destino", "Recurso Destino" },
                        { "Fuente Volumen", "Volumen Fuente" },
                        { "Reconciliado Volumen", "Volumen Reconciliado" },
                        { "Conciliado Volumen", "Volumen Conciliado" },
                        { "UM Volumen", "UM Volumen" },
                        { "Fuente Masa", "Masa Fuente" },
                        { "Reconciliado Masa", "Masa Reconciliado" },
                        { "Conciliado Masa", "Masa Conciliado" },
                        { "UM Masa", "UM Masa" },
                        { "API", "API" },
                        { "ID Muestra", "ID Muestra" },
                        { "Num. Pedido", "Número Pedido" },
                        { "Pos. Pedido", "Posición Pedido" },
                        { "UM Pedido", "UM Pedido" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "04": // ROMSS: Inventario operativo
                    // Original mapping plus new fields for completeness
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Producto", "Producto" },
                        { "Almacen", "Almacén" },
                        { "Foto Inv.", "Foto Inv." },
                        { "VoBo", "VoBo" },
                        { "API", "API" },
                        { "Volumen Total", "Volumen Total" },
                        { "Volumen Bombeable", "Volumen Bombeable" },
                        { "Volumen Remanente", "Volumen Remanente" },
                        { "UM Volumen", "UM Volumen" },
                        { "Masa Total", "Masa Total" },
                        { "Masa Bombeable", "Masa Bombeable" },
                        { "Masa Remanente", "Masa Remanente" },
                        { "UM Masa", "UM Masa" },
                        { "ID Muestra", "ID Muestra" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "05": // ROMSS: Movimientos operativo
                    // Same structure as AORA movements
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tag", "Tag" },
                        { "Tipo Mov.", "Tipo Mov." },
                        { "Fecha Inicio", "Fecha Inicio" },
                        { "Fecha Fin", "Fecha Fin" },
                        { "Recurso Origen", "Recurso Origen" },
                        { "Producto Origen", "Producto Origen" },
                        { "Recurso Destino", "Recurso Destino" },
                        { "Producto Destino", "Producto Destino" },
                        { "Fuente Volumen", "Volumen Fuente" },
                        { "Reconciliado Volumen", "Volumen Reconciliado" },
                        { "Conciliado Volumen", "Volumen Conciliado" },
                        { "UM Volumen", "UM Volumen" },
                        { "Fuente Masa", "Masa Fuente" },
                        { "Reconciliado Masa", "Masa Reconciliado" },
                        { "Conciliado Masa", "Masa Conciliado" },
                        { "UM Masa", "UM Masa" },
                        { "API", "API" },
                        { "ID Muestra", "ID Muestra" },
                        { "Num. Pedido", "Número Pedido" },
                        { "Pos. Pedido", "Posición Pedido" },
                        { "UM Pedido", "UM Pedido" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "06": // BCN: Foto inventario operativo
                    // Same structure as other inventories
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Producto", "Producto" },
                        { "Almacen", "Almacén" },
                        { "Foto Inv.", "Foto Inv." },
                        { "VoBo", "VoBo" },
                        { "API", "API" },
                        { "Volumen Total", "Volumen Total" },
                        { "Volumen Bombeable", "Volumen Bombeable" },
                        { "Volumen Remanente", "Volumen Remanente" },
                        { "UM Volumen", "UM Volumen" },
                        { "Masa Total", "Masa Total" },
                        { "Masa Bombeable", "Masa Bombeable" },
                        { "Masa Remanente", "Masa Remanente" },
                        { "UM Masa", "UM Masa" },
                        { "ID Muestra", "ID Muestra" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "07": // ARES: Movimientos HPI
                    // HPI structure (similar to movements but simpler)
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tag", "Tag" },
                        { "Tipo Mov.", "Tipo Mov." },
                        { "Fecha Inicio", "Fecha Inicio" },
                        { "Fecha Fin", "Fecha Fin" },
                        { "Recurso Origen", "Recurso Origen" },
                        { "Producto Origen", "Producto Origen" },
                        { "Recurso Destino", "Recurso Destino" },
                        { "Producto Destino", "Producto Destino" },
                        { "Fuente Volumen", "Volumen Fuente" },
                        { "Reconciliado Volumen", "Volumen Reconciliado" },
                        { "Conciliado Volumen", "Volumen Conciliado" },
                        { "UM Volumen", "UM Volumen" },
                        { "Fuente Masa", "Masa Fuente" },
                        { "Reconciliado Masa", "Masa Reconciliado" },
                        { "Conciliado Masa", "Masa Conciliado" },
                        { "UM Masa", "UM Masa" },
                        { "API", "API" },
                        { "ID Muestra", "ID Muestra" },
                        { "Num. Pedido", "Número Pedido" },
                        { "Pos. Pedido", "Posición Pedido" },
                        { "UM Pedido", "UM Pedido" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "10": // ARES: Movimientos HFS
                    // Same structure as other movements
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tag", "Tag" },
                        { "Tipo Mov.", "Tipo Mov." },
                        { "Fecha Inicio", "Fecha Inicio" },
                        { "Fecha Fin", "Fecha Fin" },
                        { "Recurso Origen", "Recurso Origen" },
                        { "Producto Origen", "Producto Origen" },
                        { "Recurso Destino", "Recurso Destino" },
                        { "Producto Destino", "Producto Destino" },
                        { "Fuente Volumen", "Volumen Fuente" },
                        { "Reconciliado Volumen", "Volumen Reconciliado" },
                        { "Conciliado Volumen", "Volumen Conciliado" },
                        { "UM Volumen", "UM Volumen" },
                        { "Fuente Masa", "Masa Fuente" },
                        { "Reconciliado Masa", "Masa Reconciliado" },
                        { "Conciliado Masa", "Masa Conciliado" },
                        { "UM Masa", "UM Masa" },
                        { "API", "API" },
                        { "ID Muestra", "ID Muestra" },
                        { "Num. Pedido", "Número Pedido" },
                        { "Pos. Pedido", "Posición Pedido" },
                        { "UM Pedido", "UM Pedido" },
                        { "Estado", "Estado" }
                    };
                    break;
                
                case "08": // BCN: Balance operativo
                    // Python HTML columns: Item, ID, Codigo, Recurso, UM, Producto (Inicial/Final), Volumen (Inv. Inicial, Entradas, Salidas, Inv. Final, Desbalance, UM), Masa (similar estructura), SG Inv. Final, FC
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "ID", "ID" },
                        { "Código", "Código" },
                        { "Recurso", "Recurso" },
                        { "UM", "UM" },
                        { "Producto Inicial", "Producto Inicial" },
                        { "Producto Final", "Producto Final" },
                        { "Inv. Inicial Volumen", "Inv. Inicial Volumen" },
                        { "Entradas Volumen", "Entradas Volumen" },
                        { "Salidas Volumen", "Salidas Volumen" },
                        { "Inv. Final Volumen", "Inv. Final Volumen" },
                        { "Desbalance Volumen", "Desbalance Volumen" },
                        { "UM Volumen", "UM Volumen" },
                        { "Inv. Inicial Masa", "Inv. Inicial Masa" },
                        { "Entradas Masa", "Entradas Masa" },
                        { "Salidas Masa", "Salidas Masa" },
                        { "Inv. Final Masa", "Inv. Final Masa" },
                        { "Desbalance Masa", "Desbalance Masa" },
                        { "UM Masa", "UM Masa" }
                    };
                    break;
            }
            
            return mapping;
        }

        /// <summary>
        /// Get property mapping for Consolidar Información queries
        /// </summary>
        private Dictionary<string, string> GetConsolidarPropertyMapping(string option)
        {
            var mapping = new Dictionary<string, string>();
            
            switch (option)
            {
                case "01": // BCN: Inventarios
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "NmRecProducto", "Producto" },
                        { "NmRecAlmacen", "Almacén" },
                        { "BoFotoInventario", "Foto Inv." },
                        { "BoVoBoAlmacen", "VoBo" },
                        { "NbAPI60", "API" },
                        { "CantVolTotal", "Volumen Total" },
                        { "CantVolBombeable", "Volumen Bombeable" },
                        { "CantVolRemanente", "Volumen Remanente" },
                        { "IdUMVolumen", "UM Volumen" },
                        { "CantMasTotal", "Masa Total" },
                        { "CantMasBombeable", "Masa Bombeable" },
                        { "CantMasRemanente", "Masa Remanente" },
                        { "IdUMMasa", "UM Masa" },
                        // { "nbMuestra", "ID Muestra" },
                        { "NmEstado", "Estado" }
                    };
                    break;
                
                case "06": // BCN: Foto inventario consolidado
                    // Python HTML columns: Item, Producto, Almacen, Foto Inv., VoBo, API, Volumen (Total, Bombeable, Remanente, UM), Masa (Total, Bombeable, Remanente, UM), Muestra (ID, Fecha), Estado
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "DtInventario", "Fecha" },
                        { "NbRecSAP", "ID Recurso" },
                        { "NmProducto", "Producto" },
                        { "CantTotal", "Cantidad Total" },
                        { "CantBombeableLU", "Cantidad Bombeable LU" },
                        { "CantBombeableCC", "Cantidad Bombeable CC" },
                        { "CantBloqueada", "Cantidad Bloqueada" },
                        { "IdUMFotoInventario", "UM" },
                    };
                    break;
                
                case "02": // BCN: Movimientos consolidados
                    // Python HTML columns: Item, Tag, Tipo Mov., Fecha (Inicio, Fin), Origen (Recurso, Producto), Destino (Recurso, Producto), Volumen (Fuente, Reconciliado, Conciliado, UM), Masa (Fuente, Reconciliado, Conciliado, UM), API, ID Muestra, Pedido (Numero, Posicion, UM), Estado
                    mapping = new Dictionary<string, string>
                    {
                        { "nbRN", "Item" },
                        { "NbMovimientoTag", "Tag" },
                        { "TpMovimientoCls", "Tipo Mov." },
                        { "DtMovimientoIni", "Fecha Inicio" },
                        { "DtMovimientoFin", "Fecha Fin" },
                        { "NmRecOrigen", "Recurso Origen" },
                        { "NmProdOrigen", "Producto Origen" },
                        { "NmRecDestino", "Recurso Destino" },
                        { "NmProdDestino", "Producto Destino" },
                        { "VlCantVolFuente", "Volumen Fuente" },
                        { "VlCantVolReconciliado", "Volumen Reconciliado" },
                        { "VlCantVolConciliado", "Volumen Conciliado" },
                        { "IdUMCantVol", "UM Volumen" },
                        { "VlCantMasFuente", "Masa Fuente" },
                        { "VlCantMasReconciliado", "Masa Reconciliado" },
                        { "VlCantMasConciliado", "Masa Conciliado" },
                        { "IdUMCantMas", "UM Masa" },
                        { "NbAPI60", "API" },
                        { "nbMuestra", "ID Muestra" },
                        { "NumPedido", "Número Pedido" },
                        { "PosPedido", "Posición Pedido" },
                        { "IdUMPedido", "UM Pedido" },
                        { "NmEstado", "Estado" }
                    };
                    break;
                
                case "05": // BCN: Balance UNIDAD DE PROCESO
                case "04":// BCN: Balance POOL
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "IdRecurso", "ID" },
                        { "NbRecurso", "Código" },
                        { "NmRecurso", "Recurso" },
                        { "UMBalance", "UM" },
                        { "InvIniVol", "Inv. Inicial Volumen" },
                        { "VlVolEntVol", "Entradas Volumen" },
                        { "VlVolSalVol", "Salidas Volumen" },
                        { "InvFinVol", "Inv. Final Volumen" },
                        { "VlDesbalanceVol", "Desbalance Volumen" },
                        { "UMVol", "UM Volumen" },
                        { "InvIniMas", "Inv. Inicial Masa" },
                        { "VlVolEntMas", "Entradas Masa" },
                        { "VlVolSalMas", "Salidas Masa" },
                        { "InvFinMas", "Inv. Final Masa" },
                        { "VlDesbalanceMas", "Desbalance Masa" },
                        { "UMMas", "UM Masa" }
                    };
                    break; 
                case "03": // BCN: Balance ALMACEN
                    // Python HTML columns: Item, ID, Codigo, Recurso, UM, Volumen (Inv. Inicial, Entradas, Salidas, Inv. Final, Desbalance, UM), Masa (Inv. Inicial, Entradas, Salidas, Inv. Final, Desbalance, UM)
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "IdRecurso", "ID" },
                        { "NbRecurso", "Código SAP" },
                        { "NmRecurso", "Recurso" },
                        { "UMBalance", "UM" },
                        { "InvIniVol", "Inv. Inicial Volumen" },
                        { "VlVolEntVol", "Entradas Volumen" },
                        { "VlVolSalVol", "Salidas Volumen" },
                        { "InvFinVol", "Inv. Final Volumen" },
                        { "VlDesbalanceVol", "Desbalance Volumen" },
                        { "UMVol", "UM Volumen" },
                        { "InvIniMas", "Inv. Inicial Masa" },
                        { "VlVolEntMas", "Entradas Masa" },
                        { "VlVolSalMas", "Salidas Masa" },
                        { "InvFinMas", "Inv. Final Masa" },
                        { "VlDesbalanceMas", "Desbalance Masa" },
                        { "UMMas", "UM Masa" }
                    };
                    break;
                
                case "07": // BCN: Corregir Bal. Sig. Contrario
                    // Esta opción ejecuta el proceso de corrección de balance signo contrario
                    // Python ejecuta RNSIGCONTRARIO y no retorna datos para mostrar
                    break;
                
                case "08": // BCN: Aplicar Regla de Balance
                    // Esta opción solo ejecuta el proceso, no consulta datos para mostrar
                    // Python solo ejecuta REGBALANCE y no retorna datos
                    break;
                
                case "09": // BCN: Diferencia Balance
                    // Esta opción solo ejecuta el proceso, no consulta datos para mostrar
                    // Python solo ejecuta DIFBALANCE y no retorna datos
                    break;
            }
            
            return mapping;
        }

        /// <summary>
        /// Get property mapping for Transformación Logística queries
        /// </summary>
        private Dictionary<string, string> GetLogisticaPropertyMapping(string option)
        {
            var mapping = new Dictionary<string, string>();
            
            switch (option)
            {
                case "01": // Movimientos logísticos
                    // Python HTML columns: Item, Tag, Clase Movimiento (ID, Descripcion), Fecha (Inicio, Fin), Origen (Recurso, Producto), Destino (Recurso, Producto), Contabilizacion (Fecha, Valor, UM), Pedido (Numero, Posicion, UM), CeCo, Estado
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "IdRegMovLogistico", "ID Message" },
                        { "NbMovimientoCls", "Clase Movimiento" },
                        { "NmMovimientoCls", "Descripción" },
                        { "DtMovimientoIni", "Fecha Inicio" },
                        { "DtMovimientoFin", "Fecha Fin" },
                        { "NmAlmLogOrigen", "Recurso Origen" },
                        { "NmProdLogOrigen", "Producto Origen" },
                        { "NmAlmLogDestino", "Recurso Destino" },
                        { "NmProdLogDestino", "Producto Destino" },
                        { "DtContabilizacion", "Fecha" },
                        { "VlContable", "Valor" },
                        { "IdUM", "UM" },
                        { "NumPedido", "Número Pedido" },
                        { "PosPedido", "Posición Pedido" },
                        { "IdUMPedido", "UM Pedido" },
                        { "nbCentroCosto", "CeCo" },
                        { "nmEstado", "Estado" }
                    };
                    break;
                
                case "02": // Movimientos de costos
                    // This option is not implemented in Python HTML, but keeping the structure for consistency
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "IdRegCosto", "ID Registro" },
                        { "TpObjCosto", "Tipo Objeto Costo" },
                        { "IdObjCosto", "ID Objeto Costo" },
                        { "NmProducto", "Producto" },
                        { "VlContabilizado", "Valor Contabilizado" },
                        { "IdUM", "UM" },
                        { "DtContabilizacion", "Fecha Contabilización" },
                        { "NmEstado", "Estado" }
                    };
                    break;
                
                case "03": // Balance GRB CeLo: 2000
                case "04": // Balance Reexpido CeLo: 3501
                case "05": // Balance Impala CeLo: 4130
                    // Python HTML columns: Item, ID, Codigo, Recurso, Balance (Inv. Inicial, Entradas, Salidas, Inv. Final, Desbalance, UM)
                    mapping = new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "IdRecurso", "ID" },
                        { "NbRecurso", "Código" },
                        { "NmRecurso", "Recurso" },
                        { "InvIni", "Inv. Inicial" },
                        { "VlEntradas", "Entradas" },
                        { "VlSalidas", "Salidas" },
                        { "InvFin", "Inv. Final" },
                        { "VlDesbalance", "Desbalance" },
                        { "UM", "UM" }
                    };
                    break;
            }
            
            return mapping;
        }

        /// <summary>
        /// Get property mapping for Envío BCN WS-ARES queries
        /// </summary>
        private Dictionary<string, string> GetAresPropertyMapping(string option)
        {
            var mapping = new Dictionary<string, string>();
            
            switch (option)
            {
                case "01": // Inventario logístico para ARES
                    // Python HTML columns: Item, Fecha Contabilización, ID Recurso, ID Producto, ID CELO, ID Almacén, ID Material, Valor Contable, UM Contable, Usuario Auditoría, Fecha Auditoría
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "dtContabilizacion", "Fecha Contabilización" },
                        { "idRecurso", "ID Recurso" },
                        { "idProducto", "ID Producto" },
                        { "idCELO", "ID CELO" },
                        { "idALMACEN", "ID Almacén" },
                        { "idMaterial", "ID Material" },
                        { "vlContable", "Valor Contable" },
                        { "idUMContable", "UM Contable" },
                        { "idUsrAuditoria", "Usuario Auditoría" },
                        { "dtUsrAuditoria", "Fecha Auditoría" }
                    };
                    break;
                
                case "02": // Movimiento logístico para ARES
                    mapping = new Dictionary<string, string>
                    {
                        { "IDMessage", "ID Mensaje" },
                        { "dtContabilizacion", "Fecha Contabilización" },
                        { "idMovimiento", "ID Movimiento" },
                        { "dtMovimientoIni", "Fecha Inicio" },
                        { "dtMovimientoFin", "Fecha Fin" },
                        { "tpMovimiento", "Tipo Movimiento" },
                        { "clsMovimiento", "Clase Movimiento" },
                        { "TransactionCodeSAP", "Código Transacción SAP" },
                        { "StockTypeSAP", "Tipo Stock SAP" },
                        { "NumPedido", "Número Pedido" },
                        { "PosPedido", "Posición Pedido" },
                        { "idRecOrigen", "Recurso Origen" },
                        { "idProdOrigen", "Producto Origen" },
                        { "idRecDestino", "Recurso Destino" },
                        { "idProdDestino", "Producto Destino" },
                        { "idSRCCELO", "CeLo Origen" },
                        { "idSRCALMACEN", "Almacén Origen" },
                        { "idSRCMaterial", "Material Origen" },
                        { "idDSTCCELO", "CeLo Destino" },
                        { "idDSTALMACEN", "Almacén Destino" },
                        { "idDSTMaterial", "Material Destino" },
                        { "vlContable", "Valor Contable" },
                        { "idUMContable", "UM Contable" },
                        { "nbCentroCosto", "CeCo" },
                        { "txEstadoEnvio", "Estado de Envío" },
                        { "vlAtrCalidad", "Valor Atributo Calidad" },
                        { "idUMAtrCalidad", "UM Atributo Calidad" },
                        { "vlCantidaadQCI", "Valor QCI" },
                        { "idUMCantidadQCI", "UM QCI" },
                        { "txCantidadQCI", "Cantidad QCI" },
                        { "IdPropiedad", "Propiedad" },
                        { "jsonMovimientos", "Json Movimientos" },
                        { "idUsrAuditoria", "Usuario Auditoria" },
                        { "dtUsrAuditoria", "Fecha Auditoria" }
                    };
                    break;
                
                case "03": // Movimiento de costos para ARES
                    // Python HTML columns: Item, ID Mensaje, Tipo Objeto Costos, Texto Movimiento, Fecha Contabilización, ID Objeto Costo, ID Valor Estadístico, Nombre Producto, UM, Valor Contabilizado, JSON Movimiento, Usuario Auditoría, Fecha Auditoría
                    mapping = new Dictionary<string, string>
                    {
                        { "NbRN", "Item" },
                        { "idMessage", "ID Mensaje" },
                        { "tpObjCostos", "Tipo Objeto Costos" },
                        { "txMovimiento", "Texto Movimiento" },
                        { "dtContabilizacion", "Fecha Contabilización" },
                        { "idObjCosto", "ID Objeto Costo" },
                        { "idValEstadistico", "ID Valor Estadístico" },
                        { "nmProducto", "Nombre Producto" },
                        { "idUM", "UM" },
                        { "vlContabilizado", "Valor Contabilizado" },
                        { "jsMovimiento", "JSON Movimiento" },
                        { "txEstadoEnvio", "Estado de Envío" },
                        { "idUsrAuditoria", "Usuario Auditoría" },
                        { "dtUsrAuditoria", "Fecha Auditoría" }
                    };
                    break;

                case "04": // ARES: Rev. Procesamiento Logistico
                    // Python: vTagQuery = "MOVLOGISTICOOK" - consulta movimientos logísticos procesados
                    return new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "ID Movimiento", "ID Movimiento" },
                        { "Fecha Contabilización", "Fecha Contabilización" },
                        { "Texto Procesamiento", "Texto Procesamiento" }
                    };

                case "05": // ARES: Rev. Procesamiento Costo
                    // Python: vTagQuery = "MOVCOSTOOK" - consulta movimientos de costos procesados
                    return new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Tipo Objeto Costo", "Tipo Objeto Costo" },
                        { "ID Objeto Costo", "ID Objeto Costo" },
                        { "ID Valor Estadístico", "ID Valor Estadístico" },
                        { "Fecha Contabilización", "Fecha Contabilización" },
                        { "Texto Procesamiento", "Texto Procesamiento" },
                        { "Fecha Envío", "Fecha Envío" }
                    };

                case "06": // ARES: Rev. Comparativo Inventario
                    // Python: vTagQuery = "INVENTARIOSAPECC" - consulta comparativo de inventarios
                    return new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "Centro Logístico", "Centro Logístico" },
                        { "Almacén", "Almacén" },
                        { "Material", "Material" },
                        { "Inventario BCN", "Inventario BCN" },
                        { "UM BCN", "UM BCN" },
                        { "Inventario ECC", "Inventario ECC" },
                        { "UM ECC", "UM ECC" },
                        { "Diferencia BCN-ECC", "Diferencia BCN-ECC" },
                        { "Inventario S4H", "Inventario S4H" },
                        { "UM S4H", "UM S4H" }
                    };

                case "07": // ARES: Rev. Comparativo Costos
                    // Python: vTagQuery = "COSTOSAP" - consulta comparativo de costos
                    return new Dictionary<string, string>
                    {
                        { "Item", "Item" },
                        { "ID Registro Costo", "ID Registro Costo" },
                        { "Fecha Contabilización", "Fecha Contabilización" },
                        { "Texto Movimiento", "Texto Movimiento" },
                        { "Tipo Objeto Costo", "Tipo Objeto Costo" },
                        { "ID Objeto Costo", "ID Objeto Costo" },
                        { "ID Valor Estadístico", "ID Valor Estadístico" },
                        { "Nombre Producto", "Nombre Producto" },
                        { "UM", "UM" },
                        { "Valor Contabilizado", "Valor Contabilizado" },
                        { "JSON Movimiento", "JSON Movimiento" }
                    };
            }
            
            return mapping;
        }

        /// <summary>
        /// Execute Integrar Información queries (options 01-10)
        /// 
        /// Queries that use useInitialDate parameter:
        /// - "01": AORA Inventory (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// - "04": ROMSS Inventory (useInitialDate=true: dateFrom, false: dateTo+1sec)
        /// - "06": BCN Inventory Photo (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// </summary>
        private async Task<QueryResult> ExecuteIntegrarQuery(string option, DateTime? consultaIni, DateTime? consultaFin, bool useInitialDate = false, bool viewDataIntegrar = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Si viewDataIntegrar es true, usar IntegrarView para obtener los datos integrados
                if (viewDataIntegrar)
                {
                    var fechaIni = FormatDateTime(consultaIni ?? DateTime.Today);
                    var fechaFin = FormatDateTime(consultaFin ?? DateTime.Today);
                    return await IntegrarView(option, fechaIni, fechaFin, useInitialDate, viewDataIntegrar);
                }

                var userAudit = "AdminBCN"; // Default user, could be made configurable
                
                // Note: Date normalization rules are applied in DataProcessingService.cs
                // No need to apply them here to avoid duplication
                var dateFrom  = consultaIni ?? DateTime.Today;
                var dateTo = consultaFin ?? DateTime.Today;

                cancellationToken.ThrowIfCancellationRequested();

                _loggingService.WriteInfo($"Starting integration process: option={option}, dateFrom={dateFrom}, dateTo={dateTo}, useInitialDate={useInitialDate}");

                // Define options that should only return integration information (not data)
                var integrationOnlyOptions = new[] { "01", "02", "03", "04", "05", "06", "07", "10" };

                // Check if this option should only return integration information
                if (integrationOnlyOptions.Contains(option))
                {
                    // Process integration and wait for the result to get the record count    
                    var integrationResult = await _dataProcessingService.ProcessOperationalInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo, useInitialDate);

                    if (integrationResult.Success)
                    {
                        return new QueryResult
                        {
                            Message = $"Integración exitosa: {integrationResult.RecordCount} registros procesados.",
                            Data = new List<Dictionary<string, object>>(),
                            Columns = new List<string>()
                        };
                    }
                    else
                    {
                        return new QueryResult
                        {
                            Message = $"Resultado en la integración: {integrationResult.Message}",
                            Data = new List<Dictionary<string, object>>(),
                            Columns = new List<string>()
                        };
                    }
                }

                // For other options, process integration asynchronously and then query data
                // Process integration using DataProcessingService (replicates Python InfOperativo logic)
                _ = Task.Run(async () => 
                    await _dataProcessingService.ProcessOperationalInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo, useInitialDate), cancellationToken);

                // After successful integration, query the integrated data
                QueryResult dataResult;
                switch (option)
                {
                    case "01": // AORA: Inventario operativo
                        
                        var inventoryDate = useInitialDate ? dateFrom : dateTo;
                        _loggingService.WriteInfo($"AORA Inventory: using inventoryDate={FormatDateTime(inventoryDate)} (useInitialDate={useInitialDate})");
                        dataResult = await _bcnModuleService.GetAoraInventoryAsync(inventoryDate);
                        break;
                    case "04": // ROMSS: Inventario operativo

                        var romssInventoryDate = useInitialDate ? dateFrom : dateTo;
                        _loggingService.WriteInfo($"[ROMSS INVENTORY INTEGRATION] Executing ROMSS Inventario Operativo integration");
                        _loggingService.WriteInfo($"[ROMSS INVENTORY INTEGRATION] Date range: {FormatDateTime(dateFrom)} to {FormatDateTime(dateTo)}");
                        _loggingService.WriteInfo($"[ROMSS INVENTORY INTEGRATION] Using inventory date: {FormatDateTime(romssInventoryDate)} (useInitialDate={useInitialDate})");
                        dataResult = await _bcnModuleService.GetRomssInventoryAsync(romssInventoryDate);
                        break;
                    case "05": 
                        dataResult = await _bcnModuleService.GetRomssMovementsAsync(dateFrom, dateTo);
                        break;
                    case "06": // BCN: Foto inventario operativo
                        
                        var photoDate = useInitialDate ? dateFrom : dateTo;
                        _loggingService.WriteInfo($"BCN Inventory Photo: using photoDate={FormatDateTime(photoDate)} (useInitialDate={useInitialDate})");
                        dataResult = await _bcnModuleService.GetBcnInventoryPhotoAsync(photoDate);
                        break;
                    case "07": // ARES: Movimientos HPI
                        _loggingService.WriteInfo($"[ARES HPI MOVEMENTS] Executing ARES HPI Movements integration");
                        _loggingService.WriteInfo($"[ARES HPI MOVEMENTS] Using date: {FormatDateTime(dateFrom)}");
                        dataResult = await _bcnModuleService.GetHpiMovementsAsync(dateFrom);
                        break;
                    case "08": // BCN: Balance operativo
                        var balanceData = await _queriesService.GetBcnBalanceOperativoAsync(consultaIni.Value, consultaFin.Value, idCaso: 4);
                        dataResult = ConvertToQueryResult(balanceData);
                        break;
                    case "09": // WebService: Movimientos logísticos
                        dataResult = await _bcnModuleService.GetWsLogisticMovementsAsync(consultaIni, consultaFin);
                        break;
                    case "10": // WebService: Costos
                        dataResult = await _bcnModuleService.GetWsCostsAsync(consultaIni, consultaFin);
                        break;
                    default:
                        dataResult = new QueryResult { Message = $"Unknown integrar option: {option}" };
                        break;
                }

                // Combine integration success message with data result
                if (dataResult.Success)
                {
                    dataResult.Message = $"Integración exitosa: {dataResult.RecordCount} registros procesados. {dataResult.Message}";
                    return dataResult;
                }
                else
                {
                    return new QueryResult
                    {
                        Message = $"Integración exitosa: {dataResult.RecordCount} registros procesados. {dataResult.Message}",
                        Data = new List<Dictionary<string, object>>(),
                        Columns = new List<string>()
                    };
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ExecuteIntegrarQuery: {ex.Message}");
                return new QueryResult
                {
                    Message = $"Resultado en la integración: {ex.Message}",
                    Data = null,
                    Columns = new List<string>()
                };
            }
        }

        /// <summary>
        /// Execute Consolidar Información queries (options 01-06) with consolidation logic
        /// 
        /// Queries that use useInitialDate parameter:
        /// - "01": BCN Consolidated Inventory (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// - "06": BCN Consolidated Inventory Photo (useInitialDate=true: dateFrom-1min, false: dateTo-59sec)
        /// </summary>
        private async Task<QueryResult> ExecuteConsolidarQuery(string option, DateTime? consultaIni, DateTime? consultaFin, bool useInitialDate = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var userAudit = "AdminBCN"; // Default user, could be made configurable
                var dateFrom = consultaIni ?? DateTime.Today;
                var dateTo = consultaFin ?? DateTime.Today;

                _loggingService.WriteInfo($"Starting consolidation process: option={option}, dateFrom={FormatDateTime(dateFrom)}, dateTo={FormatDateTime(dateTo)}, useInitialDate={useInitialDate}");

                if (option == "07" || option == "08" || option == "09")
                {
                    var result = await _dataProcessingService.ProcessConsolidatedInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo, useInitialDate);

                    return new QueryResult
                    {
                        Message = result.Success
                            ? $"Proceso '{GetModuleTitle("consolidar", option)}' ejecutado exitosamente. Se procesaron {result.RecordCount} registros."
                            : $"Error al ejecutar el proceso '{GetModuleTitle("consolidar", option)}': {result.Message}",
                        Data = new List<Dictionary<string, object>>(),
                        Columns = new List<string>()
                    };
                }

                // Process consolidation using DataProcessingService (replicates Python logic)
                _ = Task.Run(async () => 
                    await _dataProcessingService.ProcessConsolidatedInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo, useInitialDate), cancellationToken);

                    // After successful consolidation, query the consolidated data
                    QueryResult dataResult;
                    switch (option)
                    {
                        case "01": // BCN: Inventarios consolidados
                            
                            var consolidatedInventoryDate = useInitialDate ? dateFrom : dateTo;
                            _loggingService.WriteInfo($"BCN Consolidated Inventory: using consolidatedInventoryDate={FormatDateTime(consolidatedInventoryDate)} (useInitialDate={useInitialDate})");
                            dataResult = await _bcnModuleService.GetBcnConsolidatedInventoryBalanceAsync(consolidatedInventoryDate, 5);
                            break;
                case "02": // BCN: Movimientos consolidados
                            dataResult = await _bcnModuleService.GetBcnConsolidatedMovementsAsync(consultaIni, consultaFin);
                            break;
                case "03": // BCN: Balance ALMACEN
                            dataResult = await _bcnModuleService.GetBcnBalanceAlmacenAsync(consultaIni, consultaFin);
                            break;
                case "04": // BCN: Balance POOL
                            dataResult = await _bcnModuleService.GetBcnBalancePoolAsync(consultaIni, consultaFin);
                            break;
                case "05": // BCN: Balance UNIDAD DE PROCESO
                            dataResult = await _bcnModuleService.GetBcnBalanceUnidadProcesoAsync(consultaIni, consultaFin);
                            break;
                case "06": // BCN: Foto inventario consolidado
                            
                            var consolidatedPhotoDate = useInitialDate ? dateFrom : dateTo;
                            _loggingService.WriteInfo($"BCN Consolidated Inventory Photo: using consolidatedPhotoDate={FormatDateTime(consolidatedPhotoDate)} (useInitialDate={useInitialDate})");
                            dataResult = await _bcnModuleService.GetBcnConsolidatedInventoryPhotoAsync(consolidatedPhotoDate);
                            break;
                default:
                            dataResult = new QueryResult { Message = $"Unknown consolidar option: {option}" };
                            break;
                    }

                    // Combine consolidation success message with data result
                    if (dataResult.Success)
                    {
                        dataResult.Message = $"Consolidación exitosa: {dataResult.RecordCount} registros procesados. {dataResult.Message}";
                        return dataResult;
                    }
                    else
                    {
                        return new QueryResult
                        {
                            Message = $"Consolidación exitosa: {dataResult.RecordCount} registros procesados. {dataResult.Message}",
                            Data = new List<Dictionary<string, object>>(),
                            Columns = new List<string>()
                        };
                    }
                
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ExecuteConsolidarQuery: {ex.Message}");
                return new QueryResult 
                { 
                    Message = $"Error ejecutando consolidación: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Execute Transformación Logística queries (options 01-05)
        /// </summary>
        private async Task<QueryResult> ExecuteLogisticaQuery(string option, DateTime? consultaIni, DateTime? consultaFin, bool useInitialDate = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var userAudit = "AdminBCN"; // Default user, could be made configurable
                var dateFrom = consultaIni ?? DateTime.Today;
                var dateTo = consultaFin ?? DateTime.Today;

                _loggingService.WriteInfo($"Starting balance process: option={option}, dateFrom={FormatDateTime(dateFrom)}, dateTo={FormatDateTime(dateTo)}");

                // Process balance using DataProcessingService (replicates Python InfBalance logic)
                _= Task.Run(async () => 
                    await _dataProcessingService.ProcessBalanceInfoAsync(
                        option, _queriesService, userAudit, dateFrom, dateTo
                        
                ), cancellationToken);

                    // After successful balance processing, query the balance data
                    QueryResult dataResult;
                    switch (option)
                    {
                        case "01": // Movimientos logísticos
                            
                            dataResult = await _bcnModuleService.GetLogisticMovementsAsync(consultaIni, consultaFin);
                            break;
                        case "02": // Movimientos de costos
                            dataResult = await _bcnModuleService.GetCostMovementsAsync(consultaIni, consultaFin);
                            break;
                        case "03": // Balance GRB CeLo: 2000
                            
                            dataResult = await _bcnModuleService.GetBalanceGrbCelo2000Async(consultaIni, consultaFin);
                            break;
                        case "04": // Balance Reexpido CeLo: 3501
                            
                            dataResult = await _bcnModuleService.GetBalanceReexpidoCelo3501Async(consultaIni, consultaFin);
                            break;
                        case "05": // Balance Impala CeLo: 4130
                            
                            dataResult = await _bcnModuleService.GetBalanceImpalaCelo4130Async(consultaIni, consultaFin);
                            break;
                        default:
                            dataResult = new QueryResult { Message = $"Unknown logistica option: {option}" };
                            break;
                    }

                    // Combine balance success message with data result
                    if (dataResult.Success)
                    {
                        dataResult.Message = $"Procesamiento de balance exitoso: {dataResult.RecordCount} registros procesados. {dataResult.Message}";
                        return dataResult;
                    }
                    else
                    {
                        return new QueryResult
                        {
                            Message = $"Procesamiento de balance exitoso: {dataResult.RecordCount} registros procesados. {dataResult.Message}",
                            Data = new List<Dictionary<string, object>>(),
                            Columns = new List<string>()
                        };
                    }
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ExecuteLogisticaQuery: {ex.Message}");
                return new QueryResult
                {
                    Message = $"Error en el procesamiento de balance: {ex.Message}",
                    Data = null,
                    Columns = new List<string>()
                };
            }
        }

        /// <summary>
        /// Execute ARES queries (options 01-07) - returns data for manual sending
        /// </summary>
        private async Task<QueryResult> ExecuteAresQuery(string option, DateTime? consultaIni, DateTime? consultaFin, bool useInitialDate = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                switch (option)
                {
                    case "01": // Inventario logístico para ARES
                        var inventoryData = await _queriesService.GetWsLogisticInventoryAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);
                        return ConvertInventoryToAresPayload(inventoryData);
                        
                    case "02": // Movimiento logístico para ARES
                        var movementData = await _queriesService.GetWsLogisticMovementsAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today, "AND ml.txProcesamiento NOT LIKE 'Documento:%'");
                        return ConvertMovementToAresPayload(movementData);
                        
                    case "03": // Movimiento de costos para ARES
                        var costData = await _queriesService.GetWsCostsAsync(consultaIni ?? DateTime.Today, consultaFin ?? DateTime.Today);
                        return ConvertCostToAresPayload(costData);
                        
                    case "04": // ARES: Rev. Procesamiento Logistico
                        // Python: vTagQuery = "MOVLOGISTICOOK" - consulta movimientos logísticos procesados
                        // Python usa la fecha de contabilización (fechaIni) para esta opción
                        _loggingService.WriteInfo($"ARES Logistic Review: calling SAP ECC ECP web service for inventory date={FormatDateTime(consultaIni)}");
                        var logisticReviewData = await _bcnModuleService.CallSapEccEcpWebServiceAsync("INVENTARIO", consultaIni, consultaFin);
                        return logisticReviewData;
                        
                    case "05": // ARES: Rev. Procesamiento Costo
                        // Python: vTagQuery = "MOVCOSTOOK" - consulta movimientos de costos procesados
                        // Python usa la fecha de contabilización (fechaIni) para esta opción
                        _loggingService.WriteInfo($"ARES Cost Review: calling SAP ECC ECP web service for cost period");
                        var costReviewData = await _bcnModuleService.CallSapEccEcpWebServiceAsync("CECO", consultaIni, consultaFin);
                        return costReviewData;

                    case "06": // ARES: Rev. Comparativo Inventario
                        // Python: vTagQuery = "INVENTARIOSAPECC" - consulta comparativo de inventarios
                        // Python usa la fecha de contabilización (fechaFin) para esta opción
                        _loggingService.WriteInfo($"ARES Inventory Comparison: calling SAP ECC ECP web service for inventory date={FormatDateTime(consultaFin)}");
                        var inventoryComparisonData = await _bcnModuleService.CallSapEccEcpWebServiceAsync("INVENTARIO", consultaIni, consultaFin);
                        return inventoryComparisonData;
                        
                    case "07": // ARES: Rev. Comparativo Costos
                        // Python: vTagQuery = "COSTOSAP" - consulta comparativo de costos
                        // Python usa la fecha de contabilización (fechaFin) para esta opción
                        _loggingService.WriteInfo($"ARES Cost Comparison: calling SAP ECC ECP web service for cost period");
                        var costComparisonData = await _bcnModuleService.CallSapEccEcpWebServiceAsync("CECO", consultaIni, consultaFin);
                        return costComparisonData;
                    default:
                        return new QueryResult { Message = $"Unknown ares option: {option}" };
                }
            }
            catch (OperationCanceledException)
            {
                _loggingService.WriteInfo("ExecuteAresQuery operation was cancelled by user");
                return new QueryResult { Message = "Operación cancelada por el usuario" };
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error in ExecuteAresQuery: {ex.Message}");
                return new QueryResult { Message = ex.Message };
            }
        }

        /// <summary>
        /// Convert any IEnumerable to QueryResult
        /// </summary>
        private QueryResult ConvertToQueryResult<T>(IEnumerable<T> data)
        {
            var result = new QueryResult();

            if (data == null)
                return result;

            var properties = typeof(T).GetProperties();
            result.Columns = properties.Select(p => {
                var jsonProperty = p.GetCustomAttribute<JsonPropertyAttribute>();
                return jsonProperty?.PropertyName ?? p.Name;
            }).ToList();

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
                        var jsonProperty = prop.GetCustomAttribute<JsonPropertyAttribute>();
                        var columnName = jsonProperty?.PropertyName ?? prop.Name;
                        
                        // Formatear números decimales con exactamente 3 decimales
                        if (value != null && (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?) || 
                                             prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?) ||
                                             prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?) ||
                                             prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?) ||
                                             prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?)))
                        {
                            dict[columnName] = FormatDecimalWith3Places(value);
                        }
                        else
                        {
                            dict[columnName] = value ?? DBNull.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        var jsonProperty = prop.GetCustomAttribute<JsonPropertyAttribute>();
                        var columnName = jsonProperty?.PropertyName ?? prop.Name;
                        dict[columnName] = $"Error: {ex.Message}";
                    }
                }

                result.Data.Add(dict);
            }

            return result;
        }

        /// <summary>
        /// Convert inventory data to ARES payload format
        /// </summary>
        private QueryResult ConvertInventoryToAresPayload(IEnumerable<WsLogisticInventoryModel> inventoryData)
        {
            var result = new QueryResult();
            result.Columns = new List<string> 
            { 
                "dtContabilizacion", "idRecurso", "idProducto", "idCELO", "idALMACEN", 
                "idMaterial", "vlContable", "idUMContable", "idUsrAuditoria", "dtUsrAuditoria" 
            };

            if (inventoryData == null)
                return result;

            foreach (var inventory in inventoryData)
            {
                if (inventory == null)
                    continue;

                var payload = new Dictionary<string, object>
                {
                    ["dtContabilizacion"] = FormatDateTime(inventory.DtContabilizacion),
                    ["idRecurso"] = inventory.NmRecAlmacen,
                    ["idProducto"] = inventory.NmRecProducto,
                    ["idCELO"] = inventory.NbCenLog,
                    ["idALMACEN"] = inventory.NbAlmLog,
                    ["idMaterial"] = inventory.NbMaterial,
                    ["vlContable"] = FormatDecimalWith3Places(inventory.VlContable),
                    ["idUMContable"] = inventory.IdUM,
                    ["idUsrAuditoria"] = "AdminBCN",
                    ["dtUsrAuditoria"] = FormatDateTime(DateTime.Now)
                };

                result.Data.Add(payload);
            }

            return result;
        }

        /// <summary>
        /// Convert movement data to ARES payload format
        /// </summary>
        private QueryResult ConvertMovementToAresPayload(IEnumerable<WsLogisticMovementModel> movementData)
        {
            var result = new QueryResult();
            // Columnas exactamente como las claves del payload Python
            result.Columns = new List<string>
			{
				"IDMessage","dtContabilizacion","idMovimiento","IdMovimientoReg","dtMovimientoIni","dtMovimientoFin","tpMovimiento",
				"clsMovimiento","TransactionCodeSAP","StockTypeSAP","NumPedido","PosPedido",
				"idRecOrigen","idProdOrigen","idRecDestino","idProdDestino",
				"idSRCCELO","idSRCALMACEN","idSRCMaterial","idDSTCCELO","idDSTALMACEN","idDSTMaterial",
				"vlContable","idUMContable","nbCentroCosto","txEstadoEnvio","vlAtrCalidad","idUMAtrCalidad",
				"vlCantidaadQCI","idUMCantidadQCI","txCantidadQCI","IdPropiedad","jsMovimiento","jsonMovimientos","idUsrAuditoria","dtUsrAuditoria"
			};

            if (movementData == null)
                return result;

            foreach (var movement in movementData)
            {
                if (movement == null) continue;

                var payload = new Dictionary<string, object>
                {
                    ["IDMessage"] = movement.IdRegMovLogistico.ToString(),
                    ["dtContabilizacion"] = FormatDateTime(movement.DtContabilizacion),
                    ["idMovimiento"] = movement.IdRegMovLogistico.ToString(),
                    ["dtMovimientoIni"] = FormatDateTime(movement.DtMovimientoIni),
                    ["dtMovimientoFin"] = FormatDateTime(movement.DtMovimientoFin),
                    ["tpMovimiento"] = movement.TpMovimiento,
                    ["clsMovimiento"] = movement.NbMovimientoCls,
                    ["TransactionCodeSAP"] = movement.NbGM,
                    ["StockTypeSAP"] = movement.TpInventario,
                    ["NumPedido"] = movement.NumPedido ?? string.Empty,
                    ["PosPedido"] = movement.PosPedido ?? string.Empty,
                    ["idRecOrigen"] = movement.NmRecOrigen,
                    ["idProdOrigen"] = movement.NmProdOrigen,
                    ["idRecDestino"] = movement.NmRecDestino,
                    ["idProdDestino"] = movement.NmProdDestino,
                    ["idSRCCELO"] = movement.NbCenLogOrigen,
                    ["idSRCALMACEN"] = movement.NbAlmLogOrigen,
                    ["idSRCMaterial"] = movement.NbProdLogOrigen,
                    ["idDSTCCELO"] = movement.NbCenLogDestino,
                    ["idDSTALMACEN"] = movement.NbAlmLogDestino,
                    ["idDSTMaterial"] = movement.NbProdLogDestino,
                    ["vlContable"] = FormatDecimalWith3Places(movement.VlContable),
                    ["idUMContable"] = movement.IdUM,
                    ["nbCentroCosto"] = movement.nbCentroCosto ?? string.Empty,
                    ["txEstadoEnvio"] = movement.TxProcesamiento ?? string.Empty,
                    ["vlAtrCalidad"] = movement.VlAtrCalidad ?? string.Empty,
                    ["idUMAtrCalidad"] = movement.IdUMAtrCalidad ?? string.Empty,
                    ["vlCantidaadQCI"] = movement.VlQCI ?? string.Empty,
                    ["idUMCantidadQCI"] = movement.IdUMQCI ?? string.Empty,
                    ["txCantidadQCI"] = movement.UpQCI ?? string.Empty,
                    ["IdPropiedad"] = movement.IdPropiedad ?? string.Empty,
                    ["jsMovimiento"] = GenerateMovementLogisticJson(movement),
					["jsonMovimientos"] = GenerateMovementLogisticJson(movement),
                    ["idUsrAuditoria"] = "AdminBCN",
                    ["dtUsrAuditoria"] = FormatDateTime(DateTime.Now)
                };

                result.Data.Add(payload);
            }

            return result;
        }

        /// <summary>
        /// Parse QCI string to object, handling JSON strings properly
        /// </summary>
        private object ParseQciToObject(string qciString)
        {
            if (string.IsNullOrEmpty(qciString))
                return null;

            try
            {
                // Remover comillas al inicio y final si existen
                var cleanedQci = qciString.Trim().Trim('"');
                
                // Si está vacío después de limpiar, retornar null
                if (string.IsNullOrEmpty(cleanedQci))
                    return null;
                
                // Intentar parsear como JSON
                return JsonConvert.DeserializeObject(cleanedQci);
            }
            catch (Exception ex)
            {
                _loggingService.WriteWarning($"ParseQciToObject - Error parsing QCI JSON: {ex.Message}, returning as string: {qciString}");
                // Si falla el parsing, retornar como string limpio
                return qciString.Trim().Trim('"');
            }
        }

        private string GenerateMovementLogisticJson(WsLogisticMovementModel movement)
        {
            var jsonTemplate = new
            {
                DateLoad = "[dtCargue]",
                SourceSystem = "ARESGRB",
                Evento_SAPPO = "CREAR",
                IDMessageARES = "[idMsg]",
                Site = "GRB",
                DestinationSystem = "S4 HANA",
                Movementdetails = new
                {
                    Movement = new
                    {
                        MovementID = "[idMsgMovimiento]",
                        MovementType = "",
                        MovStatus = "",
                        StartTime = "[dtMovIni]",
                        EndTime = "[dtMovFin]",
                        Batchid = "0",
                        OrderMovement = "1",
                        NumReg = "1",
                        OrderNode = "1",
                        Segment = "1",
                        SGTXT = "",
                        UMCHA = "",
                        HSDAT = "",
                        NumberOrder = "[numPedido]",
                        DocPosition = "[posPedido]",
                        MovementTypeSAP = "[nbClsMov]",
                        TransactionCodeSAP = "[nbGMCODE]",
                        StockTypeSAP = "[tpInventario]",
                        Source = new
                        {
                            SourceCenter = "[nbCenLogOrigen]",
                            Stge_Loc = "[nbAlmLogOrigen]",
                            SourceProductSapCode = "[nbProdOrigen]"
                        },
                        Destination = new
                        {
                            DestinationCenter = "[nbCenLogDestino]",
                            Move_Stloc = "[nbAlmLogDestino]",
                            DestinationProductSapCode = "[nbProdDestino]"
                        },
                        NetStandardQuantity = "[CantNS]",
                        MeasurementUnit = "[cantNSUM]",
                        CostCenter = "[nbCentroCosto]", 
                        QCI = ParseQciToObject(movement.TxQCI),
                        Attributes = new { Attribute = new { PropertyQualityID = "", PropertyQuality = "[idAtrCalidad]", NumberValue = "[vlAtrCalidad]", TextValue = "Density Liquid", Uom = "[idUMAtrCalidad]" } },
                        Owners = new
                        {
                            Owner = new
                            {
                                OwnerID = "[nmPropietario]"
                            }
                        },
                        UserMovement = "[idUsuario]"
                    }
                }
            };

            // Convertir a JSON string con formato indentado y reemplazar los placeholders con valores reales
            var jsonString = JsonConvert.SerializeObject(jsonTemplate, Formatting.Indented);

            // Selección de cantidad y UM según NumPedido como en Python
            var cantNs = string.IsNullOrEmpty(movement.NumPedido)
                ? FormatDecimalWith3Places(movement.VlContable)
                : (movement.VlQCI ?? "0.000");
            var cantNsUm = string.IsNullOrEmpty(movement.NumPedido)
                ? (movement.IdUM ?? "")
                : (movement.IdUMQCI ?? "");

            // Reemplazar placeholders con valores del movimiento
            jsonString = jsonString
                .Replace("[dtCargue]", FormatDateTime(DateTime.Now))
                .Replace("[idMsg]", movement.IdRegMovLogistico.ToString())
                .Replace("[idMsgMovimiento]", $"SM-ARES-{movement.IdRegMovLogistico}")
                .Replace("[dtMovIni]", FormatDateTime(movement.DtMovimientoIni))
                .Replace("[dtMovFin]", FormatDateTime(movement.DtMovimientoFin))
                .Replace("[numPedido]", movement.NumPedido ?? "")
                .Replace("[posPedido]", movement.PosPedido ?? "")
                .Replace("[nbClsMov]", movement.NbMovimientoCls ?? "")
                .Replace("[nbGMCODE]", movement.NbGM ?? "")
                .Replace("[tpInventario]", movement.TpInventario ?? "")
                .Replace("[nbCenLogOrigen]", movement.NbCenLogOrigen ?? "")
                .Replace("[nbAlmLogOrigen]", movement.NbAlmLogOrigen ?? "")
                .Replace("[nbProdOrigen]", movement.NbProdLogOrigen ?? "")
                .Replace("[nbCenLogDestino]", movement.NbCenLogDestino ?? "")
                .Replace("[nbAlmLogDestino]", movement.NbAlmLogDestino ?? "")
                .Replace("[nbProdDestino]", movement.NbProdLogDestino ?? "")
                .Replace("[CantNS]", cantNs)
                .Replace("[cantNSUM]", cantNsUm)
                .Replace("[nbCentroCosto]", movement.nbCentroCosto ?? "")
                .Replace("[txQCI]", movement.TxQCI ?? "")
                .Replace("[idAtrCalidad]", movement.IdAtrCalidad ?? "")
                .Replace("[vlAtrCalidad]", movement.VlAtrCalidad ?? "")
                .Replace("[idUMAtrCalidad]", movement.IdUMAtrCalidad ?? "")
                .Replace("[nmPropietario]", movement.IdPropiedad ?? "")
                .Replace("[idUsuario]", "AdminBCN");

            return jsonString;
        }

        /// <summary>
        /// Convert cost data to ARES payload format
        /// </summary>
        private QueryResult ConvertCostToAresPayload(IEnumerable<WsCostModel> costData)
        {
            var result = new QueryResult();
            result.Columns = new List<string> 
            { 
                "idMessage", "tpObjCostos", "txMovimiento", "dtContabilizacion", "idObjCosto",
                "idValEstadistico", "nmProducto", "idUM", "vlContabilizado", "jsMovimiento",
                "idUsrAuditoria", "dtUsrAuditoria"
            };

            if (costData == null)
                return result;

            foreach (var cost in costData)
            {
                if (cost == null)
                    continue;

                var payload = new Dictionary<string, object>
                {
                    ["idMessage"] = cost.IdRegCosto.ToString(),
                    ["tpObjCostos"] = cost.TpObjCosto,
                    ["txMovimiento"] = $"{cost.TpObjCosto}: {cost.IdObjCosto} - {cost.ObjEstadistico}",
                    ["dtContabilizacion"] = FormatDateTime(cost.DtContabilizacion),
                    ["idObjCosto"] = cost.IdObjCosto,
                    ["idValEstadistico"] = cost.ObjEstadistico,
                    ["nmProducto"] = cost.NmProducto,
                    ["idUM"] = cost.IdUM,
                    ["vlContabilizado"] = FormatDecimalWith3Places(cost.VlContable),
                    ["jsMovimiento"] = cost.TxMovimiento,
                    ["idUsrAuditoria"] = "AdminBCN",
                    ["dtUsrAuditoria"] = FormatDateTime(DateTime.Now)
                };

                result.Data.Add(payload);
            }

            return result;
        }

        public ActionResult LargeJsonResult(object data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                MaxDepth = null
            });
            return Content(json, "application/json");
        }

        /// <summary>
        /// Transform data to use display names as keys instead of property names
        /// Ensures all expected columns from the property mapping are present in the exact order, even if data doesn't have them
        /// </summary>
        private List<Dictionary<string, object>> TransformDataToMatchColumns(
            List<Dictionary<string, object>> originalData, 
            Dictionary<string, string> propertyToDisplayMapping)
        {
            // Handle null input to prevent null pointer exceptions
            if (originalData == null)
            {
                return new List<Dictionary<string, object>>();
            }
            
            if (propertyToDisplayMapping == null)
            {
                propertyToDisplayMapping = new Dictionary<string, string>();
            }
            
            var transformedData = new List<Dictionary<string, object>>();
            
            _loggingService.WriteInfo($"TransformDataToMatchColumns - Transformando {originalData.Count} filas con {propertyToDisplayMapping.Count} mapeos");
            _loggingService.WriteInfo($"TransformDataToMatchColumns - Mapeos: {string.Join(", ", propertyToDisplayMapping.Select(kvp => $"{kvp.Key}->{kvp.Value}"))}");
            
            foreach (var row in originalData)
            {
                if (row == null) continue; // Skip null rows
                
                var transformedRow = new Dictionary<string, object>();
                
                // First, add all mapped properties from the original data
                foreach (var kvp in row)
                {
                    if (propertyToDisplayMapping.ContainsKey(kvp.Key))
                    {
                        // Use the display name as the key
                        var displayName = propertyToDisplayMapping[kvp.Key];
                        transformedRow[displayName] = kvp.Value;
                        _loggingService.WriteInfo($"TransformDataToMatchColumns - Mapeando '{kvp.Key}' -> '{displayName}' = {kvp.Value}");
                    }
                    else
                    {
                        // Log unmapped properties for debugging
                        _loggingService.WriteInfo($"TransformDataToMatchColumns - Propiedad no mapeada: '{kvp.Key}' = {kvp.Value}");
                    }
                }
                
                // Then, ensure all expected columns from the mapping are present in the exact order
                // If a column is missing, add it with an empty value
                foreach (var mapping in propertyToDisplayMapping)
                {
                    var displayName = mapping.Value;
                    if (!transformedRow.ContainsKey(displayName))
                    {
                        transformedRow[displayName] = ""; // Set missing fields as blank
                        _loggingService.WriteInfo($"TransformDataToMatchColumns - Agregando columna faltante: '{displayName}' = ''");
                    }
                }
                
                transformedData.Add(transformedRow);
            }
            
            _loggingService.WriteInfo($"TransformDataToMatchColumns - Transformación completada. Filas resultantes: {transformedData.Count}");
            
            return transformedData;
        }

        /// <summary>
        /// Ensures that if an "Item" column exists, it is populated with sequential numbers.
        /// This guarantees that every row in the grid has a proper item number.
        /// </summary>
        private QueryResult EnsureSequentialItemColumn(QueryResult result)
        {
            if (result?.Data != null && result.Data.Any() && result.Columns.Contains("Item"))
            {
                for (int i = 0; i < result.Data.Count; i++)
                {
                    // The key in the data dictionary is the display name "Item"
                    // because this method is called after column and data transformation.
                    result.Data[i]["Item"] = i + 1;
                }
            }
            return result;
        }

        /// <summary>
        /// Export query results to Excel format
        /// 
        /// useInitialDate parameter:
        /// - true: Use fechaIni (initial date) for inventory queries
        /// - false: Use fechaFin (final date) for inventory queries (default)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> ExportToExcel(string option, string fechaIni, string fechaFin, string type = null, bool useInitialDate = false, bool viewDataIntegrar = false, string searchTerm = "", string columnFilters = "", string fechaConsulta = "", string moduleTitle = "")
        {
            try
            {
                _loggingService.WriteInfo($"ExportToExcel - Iniciando exportación - Type: {type}, Option: {option}, ViewDataIntegrar: {viewDataIntegrar}");
                _loggingService.WriteInfo($"ExportToExcel - Fechas: {fechaIni} - {fechaFin}, UseInitialDate: {useInitialDate}");
                
                // Get the data as you do in DynamicQuery
                DateTime? consultaIni, consultaFin;
                
                // Apply date normalization based on type and context (same as DynamicQuery)
                if (viewDataIntegrar && type == "integrar")
                {
                    // For integrar visualization, use specific rules that consider viewDataIntegrar
                    (consultaIni, consultaFin) = NormalizeDateRangeForIntegrarOption(fechaIni, fechaFin, option, useInitialDate, viewDataIntegrar);
                }
                else if (type == "consolidar")
                {
                    // Use specific consolidar date rules that replicate Python behavior exactly
                    (consultaIni, consultaFin) = NormalizeDateRangeForConsolidarOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else if (type == "logistica" && (option == "03" || option == "04" || option == "05"))
                {
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else if (type == "ares" && (option == "04" || option == "05" || option == "06" || option == "07"))
                {
                    // Python: opciones 04 y 05 usan fechaIni, opciones 06 y 07 usan fechaFin
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }
                else
                {
                    // Default normalization for other cases
                    (consultaIni, consultaFin) = NormalizeDateRangeForOption(fechaIni, fechaFin, option, useInitialDate);
                }
                
                QueryResult result;
                
                // Handle viewDataIntegrar the same way as DynamicQuery
                if (viewDataIntegrar && type == "integrar")
                {
                    // Use the already normalized dates from above - ensure they are in standard format
                    var fechaIniFormatted = consultaIni?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                    var fechaFinFormatted = consultaFin?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                    result = await IntegrarView(option, fechaIniFormatted, fechaFinFormatted, useInitialDate, viewDataIntegrar);
                    
                    // Apply the same transformation that ExecuteQueryByTypeAndOption does
                    var propertyToDisplayMapping = GetPropertyToDisplayMapping(type, option);
                    
                    // Ensure result has valid properties to avoid null pointer exceptions
                    if (result.Columns == null)
                    {
                        result.Columns = new List<string>();
                    }

                    if (result.Data == null)
                    {
                        result.Data = new List<Dictionary<string, object>>();
                    }

                    result.Columns = GetCustomColumnNames(type, option, result.Columns);
                    result.Data = TransformDataToMatchColumns(result.Data, propertyToDisplayMapping);
                }
                else
                {
                    result = await ExecuteQueryByTypeAndOption(type, option, consultaIni, consultaFin, useInitialDate, viewDataIntegrar);
                }

                // Debug original result structure
                DebugDataStructure(result, "Después de ExecuteQueryByTypeAndOption");

                // Log original result for debugging
                _loggingService.WriteInfo($"ExportToExcel - Resultado original - Success: {result.Success}, Data Count: {result.Data?.Count ?? 0}, Columns Count: {result.Columns?.Count ?? 0}");
                _loggingService.WriteInfo($"ExportToExcel - Columnas originales: {string.Join(", ", result.Columns ?? new List<string>())}");
                
                if (result?.Data == null || result.Data.Count == 0)
                {
                    _loggingService.WriteWarning("ExportToExcel - No hay datos para exportar");
                    return Json(new { success = false, message = "No hay datos para exportar" });
                }

                // Ensure the "Item" column is always correctly numbered
                result = EnsureSequentialItemColumn(result);
                DebugDataStructure(result, "Después de EnsureSequentialItemColumn");
                
                // Apply filters if provided
                if (!string.IsNullOrEmpty(searchTerm) || !string.IsNullOrEmpty(columnFilters))
                {
                    _loggingService.WriteInfo($"ExportToExcel - Aplicando filtros - SearchTerm: '{searchTerm}', ColumnFilters: {columnFilters}");
                    result = ApplyFiltersToData(result, searchTerm, columnFilters);
                    _loggingService.WriteInfo($"ExportToExcel - Después de filtros - Data Count: {result.Data?.Count ?? 0}");
                    DebugDataStructure(result, "Después de ApplyFiltersToData");
                }
                
                // Clean empty columns but preserve important ones
                result = RemoveEmptyColumnsPreservingImportant(result);
                DebugDataStructure(result, "Después de RemoveEmptyColumnsPreservingImportant");
                
                // Log the final result for debugging
                _loggingService.WriteInfo($"ExportToExcel - Resultado final - Success: {result.Success}, Data Count: {result.Data?.Count ?? 0}, Columns Count: {result.Columns?.Count ?? 0}");
                _loggingService.WriteInfo($"ExportToExcel - Result Message: {result.Message}");
                _loggingService.WriteInfo($"ExportToExcel - Columnas finales: {string.Join(", ", result.Columns ?? new List<string>())}");

                // Validate data before export
                if (!ValidateDataForExport(result))
                {
                    _loggingService.WriteError("ExportToExcel - Los datos no son válidos para la exportación");
                    return Json(new { success = false, message = "Los datos no son válidos para la exportación" });
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Datos");

                    // --- ENCABEZADO CON LOGOS Y DATOS ---
                    var logoEcopetrol = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "ecopetrol.png");
                    var logoAres = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "ares.png");
                    int headerRows = 6;
                    int colCount = Math.Max(result.Columns.Count, 6); // Mínimo 6 columnas para el encabezado

                    // Ajustar ancho de columnas del encabezado
                    for (int i = 1; i <= colCount; i++) worksheet.Column(i).Width = 20;

                    // Insertar logos (asegúrate de que la ruta sea absoluta y válida)
                    if (System.IO.File.Exists(logoEcopetrol))
                    {
                        var imgEcopetrol = worksheet.AddPicture(logoEcopetrol)
                            .MoveTo(worksheet.Cell(1, 1))
                            .WithSize(150, 80); // Ajustado: ancho 150, alto 80
                    }
                    if (System.IO.File.Exists(logoAres))
                    {
                        var imgAres = worksheet.AddPicture(logoAres)
                            .MoveTo(worksheet.Cell(1, colCount))
                            .WithSize(150, 80); // Ajustado: ancho 150, alto 80
                    }

                    // Título y datos
                    string reportTitle = !string.IsNullOrEmpty(moduleTitle) ? moduleTitle : GetModuleTitle(type, option);
                    worksheet.Range(1, 1, 1, colCount).Merge().Value = reportTitle;
                    worksheet.Range(1, 1, 1, colCount).Style.Font.Bold = true;
                    worksheet.Range(1, 1, 1, colCount).Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, colCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(1, 1, 1, colCount).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Determinar si es inventario y agregar el tipo de inventario
                    var requiresDateSelection = (type == "integrar" && (option == "01" || option == "04" || option == "06")) ||
                                             (type == "consolidar" && (option == "01" || option == "06" || option == "07")) ||
                                             (type == "ares" && (option == "04" || option == "06"));
                    
                    int currentRow = 2;
                    
                    // Agregar línea de tipo de inventario si aplica
                    if (requiresDateSelection)
                    {
                        string inventoryType = useInitialDate ? "Inventario inicial:" : "Inventario final:";
                        worksheet.Range(currentRow, 1, currentRow, colCount).Merge().Value = inventoryType;
                        worksheet.Range(currentRow, 1, currentRow, colCount).Style.Font.Bold = true;
                        worksheet.Range(currentRow, 1, currentRow, colCount).Style.Font.FontSize = 14;
                        worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Range(currentRow, 1, currentRow, colCount).Style.Font.FontColor = XLColor.FromHtml("#004237"); // Verde Ecopetrol
                        currentRow++;
                    }

                    // Determinar el texto de fecha según el tipo de consulta
                    string fechaConsultaText;
                    if (!string.IsNullOrEmpty(fechaConsulta))
                    {
                        fechaConsultaText = fechaConsulta;
                    }
                    else
                    {
                        // Lógica de respaldo para determinar el tipo de consulta
                        if (requiresDateSelection)
                        {
                            var selectedDate = useInitialDate ? consultaIni : consultaFin;
                            var timeSuffix = useInitialDate ? " 00:00:00" : " 23:59:59";
                            fechaConsultaText = $"Fecha de Consulta: {selectedDate:yyyy-MM-dd}{timeSuffix}";
                        }
                        else
                        {
                            if (consultaIni?.Date == consultaFin?.Date)
                            {
                                fechaConsultaText = $"Fecha de Consulta: {consultaIni:yyyy-MM-dd} 00:00:00 - {consultaFin:yyyy-MM-dd} 23:59:59";
                            }
                            else
                            {
                                fechaConsultaText = $"Rango de Consulta: {consultaIni:yyyy-MM-dd} 00:00:00 - {consultaFin:yyyy-MM-dd} 23:59:59";
                            }
                        }
                    }
                    
                    worksheet.Range(currentRow, 1, currentRow, colCount).Merge().Value = fechaConsultaText;
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Font.FontSize = 12;
                    currentRow++;

                    // Línea en blanco
                    worksheet.Range(currentRow, 1, currentRow, colCount).Merge();
                    currentRow++;

                    worksheet.Range(currentRow, 1, currentRow, colCount).Merge().Value = "PROPIETARIO: ECP";
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Range(currentRow, 1, currentRow, colCount).Style.Font.FontSize = 12;
                    currentRow++;

                    // Fecha de generación en la izquierda
                    worksheet.Range(currentRow, 1, currentRow, 3).Merge().Value = $"Fecha Generación \n{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    worksheet.Range(currentRow, 1, currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(currentRow, 1, currentRow, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    
                    // Usuario y Máquina en la derecha, separados en líneas diferentes
                    worksheet.Range(currentRow, colCount - 4, currentRow, colCount).Merge().Value = "Usuario: AdminBCN \nIP Máquina: 127.0.0.1";
                    worksheet.Range(currentRow, colCount - 4, currentRow, colCount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(currentRow, colCount - 4, currentRow, colCount).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    currentRow++;

                    // Ajustar headerRows si se agregó la línea de tipo de inventario
                    if (requiresDateSelection)
                    {
                        headerRows = 7; // Una fila más por el tipo de inventario
                    }

                    // Aplicar fondo y borde exterior al área del header completo
                    worksheet.Range(1, 1, headerRows, colCount).Style.Fill.BackgroundColor = XLColor.WhiteSmoke;
                    worksheet.Range(1, 1, headerRows, colCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    
                    // Combinar solo las filas individuales del header para evitar bordes internos
                    // pero mantener la información visible en cada fila
                    for (int row = 1; row <= headerRows; row++)
                    {
                        worksheet.Range(row, 1, row, colCount).Merge();
                    }

                    // Encabezados de columna (empezando después del encabezado)
                    for (int i = 0; i < result.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(headerRows + 1, i + 1);
                        cell.Value = result.Columns[i];
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#009739"); // Verde Ecopetrol
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Font.Bold = true;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    // Datos con mejor manejo de valores nulos y validación
                    _loggingService.WriteInfo($"ExportToExcel - Iniciando escritura de {result.Data.Count} filas de datos");
                    for (int row = 0; row < result.Data.Count; row++)
                    {
                        var rowData = result.Data[row];
                        for (int col = 0; col < result.Columns.Count; col++)
                        {
                            var colName = result.Columns[col];
                            var cellValue = rowData[colName];
                            
                            var cell = worksheet.Cell(headerRows + 2 + row, col + 1);
                            _loggingService.WriteInfo($"ExportToExcel  - COLUMNA NAME {colName}");
                            var notFormatColumns = new HashSet<string> { "Item", "API", "Api"};
                            
                            // Set the value based on its type
                            if ((cellValue is decimal || cellValue is double || cellValue is float || cellValue is int || cellValue is long) && !notFormatColumns.Contains(colName))
                            {
                                // Format number to match interface display (period as thousands separator, comma as decimal separator)
                                // var formattedValue = FormatNumberForInterface(cellValue);
                                cell.Value = cellValue;
                                cell.Style.NumberFormat.NumberFormatId = 4;
                                // cell.Style.NumberFormat.Format = "#,##0.00";
                            }
                            else
                            {
                                cell.Value = cellValue?.ToString() ?? "";
                            }
                        }
                        
                        // Log progress every 100 rows
                        if ((row + 1) % 100 == 0)
                        {
                            _loggingService.WriteInfo($"ExportToExcel - Procesadas {row + 1} filas de {result.Data.Count}");
                        }
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        try
                        {
                            workbook.SaveAs(stream);
                            stream.Position = 0;
                            
                            // Validate that the stream has content
                            if (stream.Length == 0)
                            {
                                _loggingService.WriteError("ExportToExcel - El archivo Excel generado está vacío");
                                return Json(new { success = false, message = "Error al generar el archivo Excel: archivo vacío" });
                            }
                            
                            var fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                            
                            _loggingService.WriteInfo($"ExportToExcel - Archivo generado exitosamente: {fileName}, Tamaño: {stream.Length} bytes");
                            
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.WriteError($"ExportToExcel - Error al guardar el archivo Excel: {ex.Message}");
                            return Json(new { success = false, message = $"Error al generar el archivo Excel: {ex.Message}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"ExportToExcel - Error durante la exportación: {ex.Message}");
                _loggingService.WriteError($"ExportToExcel - Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error durante la exportación: {ex.Message}" });
            }
        }

        /// <summary>
        /// Format number to match interface display (period as thousands separator, comma as decimal separator)
        /// </summary>
        private string FormatNumberForInterface(object value)
        {
            if (value == null) return "";
            
            if (value is decimal decimalValue)
            {
                return FormatDecimalForInterface(decimalValue);
            }
            else if (value is double doubleValue)
            {
                return FormatDecimalForInterface((decimal)doubleValue);
            }
            else if (value is float floatValue)
            {
                return FormatDecimalForInterface((decimal)floatValue);
            }
            else if (value is int intValue)
            {
                return FormatDecimalForInterface((decimal)intValue);
            }
            else if (value is long longValue)
            {
                return FormatDecimalForInterface((decimal)longValue);
            }
            
            return value.ToString();
        }

        /// <summary>
        /// Format decimal value to match interface display
        /// </summary>
        private string FormatDecimalForInterface(decimal value)
        {
            _loggingService.WriteInfo($"FormatDecimalForInterface - Value: {value}");

            var formatted = value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            _loggingService.WriteInfo($"FormatDecimalForInterface - Formatted: {formatted}");
            // Split by decimal point
            var parts = formatted.Split('.');
            var integerPart = parts[0];
            var decimalPart = parts[1];
            _loggingService.WriteInfo($"FormatDecimalForInterface - Integer Part: {integerPart}, Decimal Part: {decimalPart}");
            
            // Add thousands separators (period) to integer part
            if (integerPart.Length > 3)
            {
                var result = "";
                var count = 0;
                for (int i = integerPart.Length - 1; i >= 0; i--)
                {
                    if (count > 0 && count % 3 == 0)
                    {
                        result = "." + result;
                    }
                    result = integerPart[i] + result;
                    count++;
                }
                integerPart = result;
            }
            
            return integerPart + "," + decimalPart;
        }

        /// <summary>
        /// Debug method to log detailed information about the data structure
        /// </summary>
        private void DebugDataStructure(QueryResult result, string context)
        {
            try
            {
                _loggingService.WriteInfo($"DebugDataStructure - {context} - Iniciando análisis de estructura de datos");
                
                if (result?.Data == null)
                {
                    _loggingService.WriteWarning($"DebugDataStructure - {context} - Data es null");
                    return;
                }

                if (result?.Columns == null)
                {
                    _loggingService.WriteWarning($"DebugDataStructure - {context} - Columns es null");
                    return;
                }

                _loggingService.WriteInfo($"DebugDataStructure - {context} - Total filas: {result.Data.Count}, Total columnas: {result.Columns.Count}");
                _loggingService.WriteInfo($"DebugDataStructure - {context} - Columnas: {string.Join(", ", result.Columns)}");

                // Log sample data for first few rows
                var sampleRows = Math.Min(3, result.Data.Count);
                for (int i = 0; i < sampleRows; i++)
                {
                    var row = result.Data[i];
                    if (row != null)
                    {
                        _loggingService.WriteInfo($"DebugDataStructure - {context} - Fila {i + 1}: {row.Count} campos");
                        foreach (var kvp in row.Take(5)) // Log first 5 fields
                        {
                            _loggingService.WriteInfo($"DebugDataStructure - {context} - Fila {i + 1} - {kvp.Key}: {kvp.Value}");
                        }
                        if (row.Count > 5)
                        {
                            _loggingService.WriteInfo($"DebugDataStructure - {context} - Fila {i + 1} - ... y {row.Count - 5} campos más");
                        }
                    }
                    else
                    {
                        _loggingService.WriteWarning($"DebugDataStructure - {context} - Fila {i + 1} es null");
                    }
                }

                // Check for data consistency
                var expectedColumns = new HashSet<string>(result.Columns);
                var inconsistentRows = 0;
                for (int i = 0; i < result.Data.Count; i++)
                {
                    var row = result.Data[i];
                    if (row != null)
                    {
                        var missingColumns = expectedColumns.Except(row.Keys).ToList();
                        if (missingColumns.Any())
                        {
                            inconsistentRows++;
                            if (inconsistentRows <= 3) // Log only first 3 inconsistencies
                            {
                                _loggingService.WriteWarning($"DebugDataStructure - {context} - Fila {i + 1} faltan columnas: {string.Join(", ", missingColumns)}");
                            }
                        }
                    }
                }

                if (inconsistentRows > 0)
                {
                    _loggingService.WriteWarning($"DebugDataStructure - {context} - Total filas inconsistentes: {inconsistentRows}");
                }

                _loggingService.WriteInfo($"DebugDataStructure - {context} - Análisis de estructura completado");
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"DebugDataStructure - {context} - Error durante el análisis: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Request model for operational integration
    /// </summary>
    public class OperationalIntegrationRequest
    {
        public string OptionId { get; set; }
        public string UserAudit { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool UseInitialDate { get; set; } = false;
    }

    /// <summary>
    /// Request model for consolidated integration
    /// </summary>
    public class ConsolidatedIntegrationRequest
    {
        public string OptionId { get; set; }
        public string UserAudit { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool UseInitialDate { get; set; } = false;
    }

    /// <summary>
    /// Request model for balance integration
    /// </summary>
    public class BalanceIntegrationRequest
    {
        public string OptionId { get; set; }
        public string UserAudit { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool UseInitialDate { get; set; } = false;
    }

    /// <summary>
    /// Request model for ARES communication
    /// </summary>
    public class AresRequest
    {
        public string DataType { get; set; }
        public string UserAudit { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool UseInitialDate { get; set; } = false;
    }

    public class DynamicQueryRequest
    {
        public string Option { get; set; } = string.Empty;
        public string FechaIni { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool UseInitialDate { get; set; } = false;
    }

    public class BcnInventoryDetailDto
    {
        [JsonProperty("Item")]
        public long Item { get; set; }
        [JsonProperty("Producto")]
        public string Producto { get; set; }              // NmProdOrigen
        [JsonProperty("Almacen")]
        public string Almacen { get; set; }               // NmRecOrigen
        [JsonProperty("Foto Inv.")]
        public string FotoInv { get; set; }               // BoFotoInventario
        [JsonProperty("VoBo")]
        public string VoBo { get; set; }                  // BoVoBoAlmacen
        [JsonProperty("API")]
        public decimal API { get; set; }                 // NbAPI60
        [JsonProperty("Volumen Total")]
        public decimal VolumenTotal { get; set; }        // CantVolTotal
        [JsonProperty("Volumen Bombeable")]
        public decimal VolumenBombeable { get; set; }    // CantVolBombeable
        [JsonProperty("Volumen Remanente")]
        public decimal VolumenRemanente { get; set; }    // CantVolRemanente
        [JsonProperty("UM Volumen")]
        public string UMVolumen { get; set; }             // IdUMVolumen
        [JsonProperty("Masa Total")]
        public decimal MasaTotal { get; set; }           // CantMasTotal
        [JsonProperty("Masa Bombeable")]
        public decimal MasaBombeable { get; set; }       // CantMasBombeable
        [JsonProperty("Masa Remanente")]
        public decimal MasaRemanente { get; set; }       // CantMasRemanente
        [JsonProperty("UM Masa")]
        public string UMMasa { get; set; }                // IdUMMasa
        [JsonProperty("ID Muestra")]
        public string IdMuestra { get; set; }               // IdRecOrigen
        // public int? IdMuestraOrigen { get; set; }         // IdProdOrigen
        [JsonProperty("Estado")]
        public string Estado { get; set; }                // NmEstado
    }

    public class BcnMovementDto
    {
        [JsonProperty("Item")]
        public long Item { get; set; }
        [JsonProperty("Tag")]
        public string Tag { get; set; }                        // nbMovimientoTag
        [JsonProperty("Tipo Mov.")]
        public string TipoMov { get; set; }                    // tpMovimientoCls
        [JsonProperty("Fecha Inicio")]
        public string FechaInicio { get; set; }                // dtMovimientoIni
        [JsonProperty("Fecha Fin")]
        public string FechaFin { get; set; }                   // dtMovimientoFin
        [JsonProperty("Recurso Origen")]
        public string RecOrigen { get; set; }                  // nmRecOrigen
        [JsonProperty("Producto Origen")]
        public string ProdOrigen { get; set; }                 // nmProdOrigen
        [JsonProperty("Recurso Destino")]
        public string RecDestino { get; set; }                 // nmRecDestino
        [JsonProperty("Producto Destino")]
        public string ProdDestino { get; set; }                // nmProdDestino
        [JsonProperty("Fuente Volumen")]
        public decimal FuenteVolumen { get; set; }                // vlCantVolFuente
        [JsonProperty("Reconciliado Volumen")]
        public decimal ReconciliadoVolumen { get; set; }          // vlCantVolReconciliado
        [JsonProperty("Conciliado Volumen")]
        public decimal ConciliadoVolumen { get; set; }            // vlCantVolConciliado
        [JsonProperty("UM Volumen")]
        public string UMVolumen { get; set; }                      // idUMCantVol
        [JsonProperty("Fuente Masa")]
        public decimal FuenteMasa { get; set; }                // vlCantMasFuente
        [JsonProperty("Reconciliado Masa")]
        public decimal ReconciliadoMasa { get; set; }          // vlCantMasReconciliado
        [JsonProperty("Conciliado Masa")]
        public decimal ConciliadoMasa { get; set; }            // vlCantMasConciliado
        [JsonProperty("UM Masa")]
        public string UMMasa { get; set; }                      // idUMCantMas
        [JsonProperty("API")]
        public string API { get; set; }                        // nbAPI60
        [JsonProperty("ID Muestra")]
        public string IDMuestra { get; set; }                  // IDMuestra
        [JsonProperty("Num. Pedido")]
        public string NumPedido { get; set; }                  // numPedido
        [JsonProperty("Pos. Pedido")]
        public string PosPedido { get; set; }                  // posPedido
        [JsonProperty("UM Pedido")]
        public string UMPedido { get; set; }                 // idUMPedido
        [JsonProperty("Estado")]
        public string Estado { get; set; }                     // nmEstado
    }

    public class BcnCostComparisonDto
    {
        public long Item { get; set; }
        public int IdRegCosto { get; set; }
        public string DtContabilizacion { get; set; }
        public string TxMovimiento { get; set; }
        public string TpObjCosto { get; set; }
        public string IdObjCosto { get; set; }
        public string IdValEstadistico { get; set; }
        public string NmProducto { get; set; }
        public decimal VlContabilizado { get; set; }
        public string IdUM { get; set; }
    }

    public class BcnBalanceOperativoDto
    {
        [JsonProperty("Item")]
        public long Item { get; set; }
        [JsonProperty("ID")]
        public int IdRecurso { get; set; }
        [JsonProperty("Código")]
        public string NbRecurso { get; set; }
        [JsonProperty("Recurso")]
        public string NmRecurso { get; set; }
        [JsonProperty("UM")]
        public string UMBalance { get; set; }
        [JsonProperty("Producto Inicial")]
        public string NmProductoIni { get; set; }
        [JsonProperty("Producto Final")]
        public string NmProductoFin { get; set; }
        [JsonProperty("Inv. Inicial Volumen")]
        public decimal InvIniVol { get; set; }
        [JsonProperty("Entradas Volumen")]
        public decimal VlEntVol { get; set; }
        [JsonProperty("Salidas Volumen")]
        public decimal VlSalVol { get; set; }
        [JsonProperty("Inv. Final Volumen")]
        public decimal InvFinVol { get; set; }
        [JsonProperty("Desbalance Volumen")]
        public decimal VlDesbalanceVol { get; set; }
        [JsonProperty("UM Volumen")]
        public string UMVol { get; set; }
        [JsonProperty("Inv. Inicial Masa")]
        public decimal InvIniMas { get; set; }
        [JsonProperty("Entradas Masa")]
        public decimal VlEntMas { get; set; }
        [JsonProperty("Salidas Masa")]
        public decimal VlSalMas { get; set; }
        [JsonProperty("Inv. Final Masa")]
        public decimal InvFinMas { get; set; }
        [JsonProperty("Desbalance Masa")]
        public decimal VlDesbalanceMas { get; set; }
        [JsonProperty("UM Masa")]
        public string UMMas { get; set; }
        [JsonProperty("SG Inv. Final")]
        public decimal SGInvFin { get; set; }
        [JsonProperty("FC")]
        public decimal FC { get; set; }
    }
}
