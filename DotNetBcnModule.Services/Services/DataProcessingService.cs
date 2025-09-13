using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotNetBcnModule.Services.Contracts;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Queries;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Service to handle data processing and transformation
    /// </summary>
    public class DataProcessingService : IDataProcessingService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILoggingService _loggingService;

        public DataProcessingService(IDatabaseService databaseService, ILoggingService loggingService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
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
        /// Converts data records to XML format
        /// </summary>
        public List<string> ConvertToXml(IEnumerable<object> dataRecords, string tagName)
        {
            var xmlItems = new List<string>();
            var records = dataRecords.ToList();

            if (!records.Any())
                return xmlItems;

            var count = 0;
            var xmlBuilder = new StringBuilder();
            var isFirst = true;

            // Lista de excepciones a convertir a CamelCase
            var camelCaseExceptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "totalNSV",
                "bombeableNSV",
                "remanenteNSV",
                "totalNSW",
                "pumpableNSW",
                "remanenteNSW",
                "tag",
            };

            foreach (var record in records)
            {
                count++;

                if (isFirst)
                {
                    xmlBuilder.Append($"<{tagName}>");
                    isFirst = false;
                }

                var recordXml = new StringBuilder("<reg>");

                if (tagName == "INVCONSBCN")
                {
                    var dtInventario = ((dynamic)record).dtInventario as DateTime?;
                    recordXml.Append($"<dtInventario>{dtInventario?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtInventario}</dtInventario>");
                }
                else if (tagName == "MOVCONSBCN")
                {
                    var dtMovimientoIni = ((dynamic)record).dtMovimientoIni as DateTime?;
                    var dtMovimientoFin = ((dynamic)record).dtMovimientoFin as DateTime?;
                    recordXml.Append($"<dtMovimientoIni>{dtMovimientoIni?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoIni}</dtMovimientoIni>");
                    recordXml.Append($"<dtMovimientoFin>{dtMovimientoFin?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoFin}</dtMovimientoFin>");
                }
                else if (tagName == "DIFBALANCE")
                {
                    var dtMovimientoIni = ((dynamic)record).dtMovimientoIni as DateTime?;
                    var dtMovimientoFin = ((dynamic)record).dtMovimientoFin as DateTime?;
                    recordXml.Append($"<dtMovimientoIni>{dtMovimientoIni?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoIni}</dtMovimientoIni>");
                    recordXml.Append($"<dtMovimientoFin>{dtMovimientoFin?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoFin}</dtMovimientoFin>");
                }
                else if (tagName == "REGBALANCE")
                {
                    var dtMovimientoIni = ((dynamic)record).dtMovimientoIni as DateTime?;
                    var dtMovimientoFin = ((dynamic)record).dtMovimientoFin as DateTime?;
                    recordXml.Append($"<dtMovimientoIni>{dtMovimientoIni?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoIni}</dtMovimientoIni>");
                    recordXml.Append($"<dtMovimientoFin>{dtMovimientoFin?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ((dynamic)record).dtMovimientoFin}</dtMovimientoFin>");
                }
                else
                {
                    var properties = record.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var name = property.Name;
                        var value = property.GetValue(record);

                        // Determinar el tag, respetando excepciones
                        string tag = camelCaseExceptions.Contains(name)
                            ? char.ToUpperInvariant(name[0]) + name.Substring(1)
                            : char.ToLowerInvariant(name[0]) + name.Substring(1);

                        string valueStr;
                        if (value is DateTime dateValue)
                        {
                            valueStr = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (value is decimal decimalValue)
                        {
                            valueStr = FormatDecimalWith3Places(decimalValue);
                        }
                        else
                        {
                            valueStr = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
                        }

                        recordXml.Append($"<{tag}>{System.Security.SecurityElement.Escape(valueStr)}</{tag}>");
                    }
                }

                recordXml.Append("</reg>");
                xmlBuilder.Append(recordXml);

                if (count == 90)
                {
                    xmlBuilder.Append($"</{tagName}>");
                    xmlItems.Add(xmlBuilder.ToString());
                    count = 0;
                    isFirst = true;
                    xmlBuilder.Clear();
                }
            }

            if (!isFirst)
            {
                xmlBuilder.Append($"</{tagName}>");
                xmlItems.Add(xmlBuilder.ToString());
            }

            return xmlItems;
        }

        /// <summary>
        /// Processes XML items through stored procedure (replicating Python logic)
        /// </summary>
        /// <param name="xmlItems">XML items to process</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <param name="source">Source system (GRB)</param>
        /// <param name="tagQuery">Query tag identifier</param>
        /// <returns>Processing result with error information</returns>
        public async Task<(bool Success, string ErrorMessage)> ProcessXmlItemsAsync(List<string> xmlItems, string userAudit, string source, string tagQuery)
        {
            try
            {
                _loggingService.WriteInfo($"Processing {xmlItems.Count} XML packages for {tagQuery} on BCN database");
                _loggingService.WriteInfo($"Database connection info: {_databaseService.GetConnectionInfo()}");
                _loggingService.WriteInfo($"User audit: {userAudit}");
                _loggingService.WriteInfo($"Source: {source}");
                _loggingService.WriteInfo($"Tag query: {tagQuery}");
                _loggingService.WriteInfo($"XML items: {xmlItems.Count}");
                _loggingService.WriteInfo($"XML items whole: {xmlItems}");

                for (int i = 0; i < xmlItems.Count; i++)
                {
                    var xmlItem = xmlItems[i];
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    _loggingService.WriteInfo($"{timestamp} Info: Fuente: {source} Tag: {tagQuery} Usuario: {userAudit} Paquete XML: {i + 1} de {xmlItems.Count}");
                    _loggingService.WriteInfo(xmlItem);

                    // Execute stored procedure like Python - use direct string interpolation
                    // Escape single quotes in XML to prevent SQL injection
                    var escapedXml = xmlItem.Replace("'", "''");
                    var sql = $"EXEC BCN.spAppIntegrarProcesarInfo '{userAudit}', '{source}', '{tagQuery}', '{escapedXml}';";
                    
                    _loggingService.WriteInfo($"{timestamp} Info: Ejecutando procedimiento almacenado en BCN database");
                    _loggingService.WriteInfo($"=== SQL EJECUTADO ===");
                    _loggingService.WriteInfo($"Stored Procedure: BCN.spAppIntegrarProcesarInfo");
                    _loggingService.WriteInfo($"Usuario: {userAudit}");
                    _loggingService.WriteInfo($"Fuente: {source}");
                    _loggingService.WriteInfo($"Tag Query: {tagQuery}");
                    _loggingService.WriteInfo($"XML Escapado (primeros 200 chars): {(escapedXml.Length > 200 ? escapedXml.Substring(0, 200) + "..." : escapedXml)}");
                    _loggingService.WriteInfo($"SQL Completo: {sql}");
                    _loggingService.WriteInfo($"=== FIN SQL ===");
                    
                    try
                    {
                        await _databaseService.ExecuteAsync(sql);
                        _loggingService.WriteInfo($"{timestamp} Info: Procesado paquete {i + 1} en BCN database");
                    }
                    catch (Exception packageEx)
                    {
                        // Handle specific subquery error for flows
                        if (packageEx.Message.Contains("Subquery returned more than 1 value") && tagQuery == "FLUOPERAORA")
                        {
                            _loggingService.WriteWarning($"Error específico de subconsulta en flujos operativos (paquete {i + 1}): {packageEx.Message}");
                            _loggingService.WriteWarning($"Continuando con el siguiente paquete...");
                            
                            // Log the problematic XML for debugging
                            _loggingService.WriteWarning($"XML problemático (primeros 500 chars): {(xmlItem.Length > 500 ? xmlItem.Substring(0, 500) + "..." : xmlItem)}");
                            
                            // Continue processing other packages instead of failing completely
                            continue;
                        }
                        else
                        {
                            // Re-throw other errors
                            throw;
                        }
                    }
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error en la base de datos: {ex.Message}";
                _loggingService.WriteError($"Error processing XML items on BCN database: {ex.Message}");
                
                // Log more detailed error information
                if (ex.InnerException != null)
                {
                    _loggingService.WriteError($"Inner Exception: {ex.InnerException.Message}");
                    errorMessage += $"\nExcepción interna: {ex.InnerException.Message}";
                }
                
                _loggingService.WriteError($"Stack Trace: {ex.StackTrace}");
                return (false, errorMessage);
            }
        }

        /// <summary>
        /// Processes operational information integration (replicating Python InfOperativo logic)
        /// </summary>
        public async Task<IntegrationResult> ProcessOperationalInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            bool useInitialDate = false,
            bool viewDataIntegrar = false)
        {
            var result = new IntegrationResult();
            var source = "GRB"; // Same as Python

            try
            {
                string tagQuery = "";
                string xmlTag = "";
                IEnumerable<object> data = null;

                switch (infoType)
                {
                    case "01": // AORA: Inventory
                        tagQuery = "INVOPERAORA";
                        xmlTag = "Inventarios";
                        // For inventory queries, use the date selection logic
                        var aoraInventoryDate = useInitialDate ? dateFrom.AddMinutes(-1) : dateTo.AddSeconds(-59);
                        _loggingService.WriteInfo($"AORA Inventory: using aoraInventoryDate={aoraInventoryDate} (useInitialDate={useInitialDate})");
                        data = await queriesService.GetAoraInventoryAsync(aoraInventoryDate);
                        break;

                    case "02": // AORA: Movements
                        tagQuery = "MOVOPERAORA";
                        xmlTag = "Movimientos";
                        data = await queriesService.GetAoraMovementsAsync(dateFrom, dateTo);
                        break;

                    case "03": // AORA: Flows
                        tagQuery = "FLUOPERAORA";
                        xmlTag = "Flujos";
                        data = await queriesService.GetAoraFlowsAsync(dateFrom, dateTo);
                        break;

                    case "04": // ROMSS: Inventory
                        tagQuery = "INVOPERROMSS";
                        xmlTag = "Inventarios";
                        // For inventory queries, use the date selection logic
                        // RN05: ROMSS Inventario Final - aumentar 1 segundo a Fecha Final
                        var romssInventoryDate = useInitialDate ? dateFrom : dateTo.AddSeconds(1);
                        _loggingService.WriteInfo($"ROMSS Inventory: using romssInventoryDate={romssInventoryDate:yyyy-MM-dd HH:mm:ss} (useInitialDate={useInitialDate})");
                        _loggingService.WriteInfo($"ROMSS Inventory: original dateTo={dateTo:yyyy-MM-dd HH:mm:ss}, after AddSeconds(1)={dateTo.AddSeconds(1):yyyy-MM-dd HH:mm:ss}");
                        data = await queriesService.GetRomssInventoryAsync(romssInventoryDate);
                        break;

                    case "05": // ROMSS: Movements
                        tagQuery = "MOVOPERROMSS";
                        xmlTag = "Movimientos";
                        // Use dates as-is for ROMSS movements (no +1 second needed)
                        // Python: vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds = 1)
                        // But user specification requires: dtMovimientoIni >= '2025-08-03 00:00:00' AND dtMovimientoIni <= '2025-08-03 23:59:59'
                        var romssMovementsDateTo = viewDataIntegrar ? dateTo : dateTo.AddSeconds(1);
                        _loggingService.WriteInfo($"ROMSS Movements: using dateFrom={dateFrom:yyyy-MM-dd HH:mm:ss}, dateTo={dateTo:yyyy-MM-dd HH:mm:ss}");

                        data = await queriesService.GetRomssMovementsAsync(dateFrom, romssMovementsDateTo);
                        break;

                    case "06": // BCN: Inventory Photo
                        tagQuery = "INVFOTOBCN";
                        xmlTag = "Inventarios";
                        // For inventory queries, use the date selection logic
                        var bcnPhotoDate = useInitialDate ? dateFrom.AddMinutes(-1) : dateTo.AddSeconds(-59);
                        _loggingService.WriteInfo($"BCN Inventory Photo: using bcnPhotoDate={bcnPhotoDate} (useInitialDate={useInitialDate})");
                        data = await queriesService.GetBcnInventoryPhotoAsync(bcnPhotoDate);
                        break;

                    case "07": // BCN: Movements
                        tagQuery = "MOVBCN";
                        xmlTag = "Movimientos";
                        data = await queriesService.GetBcnMovementsAsync(dateFrom, dateTo);
                        break;

                    case "10": // ARES: HPI Movements
                        tagQuery = "MOVHPIARES";
                        xmlTag = "HPI";
                        data = await queriesService.GetHpiMovementsAsync(dateFrom);
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Esta opción no esta disponible para ejecucion: {infoType}";
                        return result;
                }

                if (data != null && data.Any())
                {
                    result.XmlItems = ConvertToXml(data, xmlTag);
                    result.RecordCount = data.Count();
                    result.Success = true;

                    // Process XML items through stored procedure
                    var processResult = await ProcessXmlItemsAsync(result.XmlItems, userAudit, source, tagQuery);
                    
                    // Check if XML processing failed
                    if (!processResult.Success)
                    {
                        result.Success = false;
                        result.Message = processResult.ErrorMessage;
                        return result;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "No existe información para la acción";
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

        /// <summary>
        /// Processes consolidated information integration (replicating Python InfConsolidado logic)
        /// </summary>
        public async Task<IntegrationResult> ProcessConsolidatedInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo,
            bool useInitialDate = false)
        {
            var result = new IntegrationResult();
            var source = "GRB";

            try
            {
                string tagQuery = "";
                string xmlTag = "";
                List<object> data = new List<object>();

                switch (infoType)
                {
                    case "01": // BCN: Consolidated Inventory
                        tagQuery = "INVCONSBCN";
                        xmlTag = "Inventarios";
                        // For inventory queries, use the date selection logic
                        var consolidatedInventoryDate = useInitialDate ? dateFrom : dateTo;
                        _loggingService.WriteInfo($"BCN Consolidated Inventory: using consolidatedInventoryDate={consolidatedInventoryDate} (useInitialDate={useInitialDate})");
                        data.Add(new { dtInventario = consolidatedInventoryDate });
                        break;

                    case "02": // BCN: Consolidated Movements
                        tagQuery = "MOVCONSBCN";
                        xmlTag = "Movimientos";
                        // Like Python: creates single record with movement date range
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "03": // BCN: Balance ALMACEN
                        tagQuery = "BALCONSALMACEN";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "04": // BCN: Balance POOL
                        tagQuery = "BALCONSPOOL";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "05": // BCN: Balance UNIDAD DE PROCESO
                        tagQuery = "BALCONSUNIDAD";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "06": // BCN: Consolidated Inventory Photo
                        tagQuery = "INVFOTOCONSBCN";
                        xmlTag = "Inventarios";
                        // For inventory queries, use the date selection logic
                        var consolidatedPhotoDate = useInitialDate ? dateFrom : dateTo;
                        _loggingService.WriteInfo($"BCN Consolidated Inventory Photo: using consolidatedPhotoDate={consolidatedPhotoDate} (useInitialDate={useInitialDate})");
                        data.Add(new { dtInventario = consolidatedPhotoDate });
                        break;

                    case "07": // BCN: Corregir Balance Signo Contrario
                        tagQuery = "RNSIGCONTRARIO";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "08": // BCN: Aplicar Regla de Balance
                        tagQuery = "REGBALANCE";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    case "09": // BCN: Diferencia Balance
                        tagQuery = "DIFBALANCE";
                        xmlTag = "Movimientos";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo });
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Esta opción no esta disponible para ejecucion: {infoType}";
                        return result;
                }

                result.XmlItems = ConvertToXml(data, tagQuery);
                result.RecordCount = data.Count;
                result.Success = true;

                // Log the generated XML for debugging (especially for options 07 and 08)
                _loggingService.WriteInfo($"=== XML GENERADO PARA {tagQuery} ===");
                _loggingService.WriteInfo($"Total de paquetes XML: {result.XmlItems.Count}");
                _loggingService.WriteInfo($"Registros procesados: {result.RecordCount}");
                
                for (int i = 0; i < result.XmlItems.Count; i++)
                {
                    _loggingService.WriteInfo($"=== PAQUETE XML {i + 1} ===");
                    _loggingService.WriteInfo($"Tamaño: {result.XmlItems[i].Length} caracteres");
                    _loggingService.WriteInfo($"Contenido completo:");
                    _loggingService.WriteInfo(result.XmlItems[i]);
                    _loggingService.WriteInfo($"=== FIN PAQUETE XML {i + 1} ===");
                }
                _loggingService.WriteInfo($"=== FIN XML GENERADO PARA {tagQuery} ===");

                // Process XML items through stored procedure
                var processResult = await ProcessXmlItemsAsync(result.XmlItems, userAudit, source, tagQuery);
                
                // Check if XML processing failed
                if (!processResult.Success)
                {
                    result.Success = false;
                    result.Message = processResult.ErrorMessage;
                    return result;
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

        /// <summary>
        /// Processes balance information integration (replicating Python InfBalance logic)
        /// </summary>
        public async Task<IntegrationResult> ProcessBalanceInfoAsync(
            string infoType,
            IQueriesService queriesService,
            string userAudit,
            DateTime dateFrom,
            DateTime dateTo)
        {
            var result = new IntegrationResult();
            var source = "GRB";

            try
            {
                string tagQuery = "";
                string xmlTag = "";
                List<object> data = new List<object>();

                switch (infoType)
                {
                    case "01": // Transform consolidated movements to logistic movements
                        tagQuery = "TRANSMOVCONSBALLOG";
                        xmlTag = "MovimientosLogistico";
                        // Like Python: creates single record with movement date range
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo.AddSeconds(1) });
                        break;

                    case "02": // Transform consolidated movements to cost movements
                        tagQuery = "TRANSMOVCONSCOSTO";
                        xmlTag = "MovimientosCosto";
                        data.Add(new { dtMovimientoIni = dateFrom, dtMovimientoFin = dateTo.AddSeconds(1) });
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Esta opción no esta disponible para ejecucion: {infoType}";
                        return result;
                }

                result.XmlItems = ConvertToXml(data, xmlTag);
                result.RecordCount = data.Count;
                result.Success = true;

                // Process XML items through stored procedure
                await ProcessXmlItemsAsync(result.XmlItems, userAudit, source, tagQuery);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error procesando información de balance: {ex.Message}";
                _loggingService.WriteError($"Balance info error: {ex.Message}");
            }

            return result;
        }

        public Task<IntegrationResult> ProcessOperationalInfoAsync(string infoType, IQueriesService queriesService, string userAudit, DateTime dateFrom, DateTime dateTo, bool useInitialDate = false)
        {
            throw new NotImplementedException();
        }
    }
}