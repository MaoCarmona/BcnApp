using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using NetBcnModule.Services.Models;
using DotNetBcnModule.Services.Contracts;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Service to handle communication with ARES web services
    /// </summary>
    public class AresWebService : IAresWebService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;

        public AresWebService(ILoggingService loggingService)
        {
            _httpClient = new HttpClient();
            _loggingService = loggingService;
        }

        /// <summary>
        /// Loads ARES configuration from app settings
        /// </summary>
        /// <returns>ARES configuration</returns>
        private AresConfig LoadAresConfiguration()
        {
            try
            {
                var base64Config = ConfigurationManager.AppSettings["URLARES"];
                if (string.IsNullOrEmpty(base64Config))
                {
                    throw new InvalidOperationException("URLARES configuration not found in app settings");
                }

                // Decode Base64 string to UTF8
                var jsonBytes = Convert.FromBase64String(base64Config);
                var jsonString = Encoding.UTF8.GetString(jsonBytes);

                // Deserialize to AresConfig
                var serializer = new JavaScriptSerializer();
                var config = serializer.Deserialize<AresConfig>(jsonString);
                
                if (config == null)
                {
                    throw new InvalidOperationException("Failed to deserialize ARES configuration");
                }

                _loggingService.WriteInfo($"ARES configuration loaded successfully: URL={config.txURL}");
                return config;
            }
            catch (Exception ex)
            {
                _loggingService.WriteError($"Error loading ARES configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends data to ARES web service
        /// </summary>
        /// <param name="payload">Data payload to send</param>
        /// <param name="endpoint">ARES endpoint URL</param>
        /// <param name="username">Username for authentication</param>
        /// <param name="password">Password for authentication</param>
        /// <param name="transactionOrigin">Transaction origin identifier</param>
        /// <returns>Response result</returns>
        public async Task<AresResponse> SendToAresAsync(object payload, string endpoint, string username, string password, string transactionOrigin)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var response = new AresResponse();

            try
            {
                var serializer = new JavaScriptSerializer();
                var jsonPayload = serializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                

                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                _loggingService.WriteInfo($"Sending to ARES: {transactionOrigin} - Endpoint: {endpoint}");
                _loggingService.WriteInfo($"Payload size: {jsonPayload.Length} characters");

                var httpResponse = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                response.StatusCode = (int)httpResponse.StatusCode;
                response.Content = responseContent;

                _loggingService.WriteLog($"Content responded webservice: {responseContent}");

                if (httpResponse.IsSuccessStatusCode)
                {
                    if(responseContent.Contains("OK"))
                    {
                        response.Success = true;
                        response.Message = $"{timestamp} INFO: Envio exitoso del Registro {transactionOrigin} (estado: {response.StatusCode})!!!\n";
                        _loggingService.WriteInfo($"ARES response successful: Status={response.StatusCode}, Content length={responseContent.Length}");
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = $"{timestamp} INFO: Existe un error: {transactionOrigin} (Estado {response.StatusCode})\n";
                        _loggingService.WriteError($"ARES response error: Status={response.StatusCode}, Content length={responseContent.Length}");
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = $"{timestamp} INFO: Existe un error: {transactionOrigin} (Estado {response.StatusCode})\n";
                    _loggingService.WriteError($"ARES response error: Status={response.StatusCode}, Content length={responseContent.Length}");
                }
            }
            catch (HttpRequestException ex)
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = $"{timestamp} ERROR: Estado: 400 - No se pudo conectar con el servidor. Asegurese de que el servidor este en funcionamiento.\n";
                _loggingService.WriteError($"ARES connection error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                // response.Message = $"{timestamp} ERROR: Estado: 500 - Se agotó el tiempo de espera de la solicitud. Verifique su conexión a Internet o vuelva a intentarlo más tarde.\n";
                response.Message = ex.Message;
                _loggingService.WriteError($"ARES timeout error: {ex.Message}");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 0;
                response.Message = $"{timestamp} ERROR: Se produjo el siguiente error: {ex.Message}\n";
                _loggingService.WriteError($"ARES general error: {ex.Message}");
            }

            return response;
        }

        /// <summary>
        /// Sends costs data to ARES
        /// </summary>
        /// <param name="costsData">Costs data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        public async Task<List<AresResponse>> SendCostsToAresAsync(List<AresCostPayload> costsData, string userAudit)
        {
            var responses = new List<AresResponse>();
            var config = LoadAresConfiguration();
            var endpoint = config.txURL + config.txMetodoCosto;

            foreach (var cost in costsData)
            {
                var response = await SendToAresAsync(cost, endpoint, config.idUsr, config.pwUsr, "Movimiento de Costo");
                responses.Add(response);
            }

            return responses;
        }

        /// <summary>
        /// Sends inventory data to ARES
        /// </summary>
        /// <param name="inventoryData">Inventory data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        public async Task<List<AresResponse>> SendInventoryToAresAsync(List<AresInventoryPayload> inventoryData, string userAudit)
        {
            var responses = new List<AresResponse>();
            var config = LoadAresConfiguration();
            var endpoint = config.txURL + config.txMetodoInventario;

            foreach (var inventory in inventoryData)
            {
                var response = await SendToAresAsync(inventory, endpoint, config.idUsr, config.pwUsr, "Inventario Logistico");
                responses.Add(response);
            }

            return responses;
        }

        /// <summary>
        /// Sends logistic movement data to ARES
        /// </summary>
        /// <param name="movementData">Movement data to send</param>
        /// <param name="userAudit">User performing the audit</param>
        /// <returns>List of responses</returns>
        public async Task<List<AresResponse>> SendLogisticMovementsToAresAsync(List<AresMovementPayload> movementData, string userAudit)
        {
            var responses = new List<AresResponse>();
            var config = LoadAresConfiguration();
            var endpoint = config.txURL + config.txMetodoMovimiento;

            foreach (var movement in movementData)
            {
                var response = await SendToAresAsync(movement, endpoint, config.idUsr, config.pwUsr, "Movimiento Logistico");
                responses.Add(response);
            }

            return responses;
        }
    }

    /// <summary>
    /// ARES web service response
    /// </summary>
    public class AresResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Content { get; set; }
    }
}