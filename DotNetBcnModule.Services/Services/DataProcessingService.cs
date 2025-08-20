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
                            valueStr = dateValue.ToString("yyyy-MM-ddTHH:mm:ss");
                        }
                        else if (value is decimal decimalValue)
                        {
                            valueStr = Math.Round(decimalValue, 3).ToString(CultureInfo.InvariantCulture);
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
        /// <returns>Processing result</returns>
        public async Task<bool> ProcessXmlItemsAsync(List<string> xmlItems, string userAudit, string source, string tagQuery)
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
                    
                    _loggingService.WriteInfo($"{timestamp} Info: Ejecutando procedimiento almacenado en BCN database: {sql}");
                    await _databaseService.ExecuteAsync(sql);
                    
                    _loggingService.WriteInfo($"{timestamp} Info: Procesado paquete {i + 1} en BCN database");
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error processing XML items on BCN database: {ex.Message}");
                return false;
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
            bool useInitialDate = false)
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
                        var romssInventoryDate = useInitialDate ? dateFrom : dateTo.AddSeconds(1);
                        _loggingService.WriteInfo($"ROMSS Inventory: using romssInventoryDate={romssInventoryDate} (useInitialDate={useInitialDate})");
                        data = await queriesService.GetRomssInventoryAsync(romssInventoryDate);
                        break;

                    case "05": // ROMSS: Movements
                        tagQuery = "MOVOPERROMSS";
                        xmlTag = "Movimientos";
                        data = await queriesService.GetRomssMovementsAsync(dateFrom, dateTo);
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
                    await ProcessXmlItemsAsync(result.XmlItems, userAudit, source, tagQuery);
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
                        var consolidatedInventoryDate = useInitialDate ? dateFrom.AddMinutes(-1) : dateTo.AddSeconds(-59);
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
                        var consolidatedPhotoDate = useInitialDate ? dateFrom.AddMinutes(-1) : dateTo.AddSeconds(-59);
                        _loggingService.WriteInfo($"BCN Consolidated Inventory Photo: using consolidatedPhotoDate={consolidatedPhotoDate} (useInitialDate={useInitialDate})");
                        data.Add(new { dtInventario = consolidatedPhotoDate });
                        break;

                    case "07": // BCN: Balance Rule
                        result.Success = false;
                        result.Message = $"Esta opción BCN: Balance Rule no esta disponible para ejecucion: {infoType}";
                        return result;

                    case "08": // BCN: Balance Difference
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

                // Process XML items through stored procedure
                await ProcessXmlItemsAsync(result.XmlItems, userAudit, source, tagQuery);
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
    }
}