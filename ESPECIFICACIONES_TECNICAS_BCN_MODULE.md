# ESPECIFICACIONES TÉCNICAS BCN MODULE
## Documento de Soporte Técnico para Equipos de Desarrollo y Mantenimiento

**Versión:** 1.0  
**Fecha:** Enero 2024  
**Plataforma:** ASP.NET MVC  
**Empresa:** Ecopetrol  
**Audiencia:** Equipos de Desarrollo, DevOps, Soporte Técnico  

---

## 📋 ÍNDICE TÉCNICO

1. [ARQUITECTURA DEL SISTEMA](#arquitectura-del-sistema)
2. [ESTRUCTURA DE DATOS](#estructura-de-datos)
3. [CONFIGURACIONES Y PARÁMETROS](#configuraciones-y-parámetros)
4. [LOGS Y MONITOREO](#logs-y-monitoreo)
5. [MANEJO DE ERRORES](#manejo-de-errores)
6. [PERFORMANCE Y OPTIMIZACIÓN](#performance-y-optimización)
7. [SEGURIDAD](#seguridad)
8. [INTEGRACIÓN CON SISTEMAS EXTERNOS](#integración-con-sistemas-externos)
9. [BACKUP Y RECUPERACIÓN](#backup-y-recuperación)
10. [TROUBLESHOOTING AVANZADO](#troubleshooting-avanzado)
11. [VERSIONAMIENTO Y PUBLICACIÓN](#versionamiento-y-publicación)
12. [COMPATIBILIDAD Y REQUISITOS](#compatibilidad-y-requisitos)
13. [SEGURIDAD DE GUARDADO](#seguridad-de-guardado)
14. [PROCESO DE COMPILACIÓN INDRA](#proceso-de-compilación-indra)
15. [NOVEDADES DE CONSOLIDACIÓN](#novedades-de-consolidación)
16. [CONFIGURACIÓN DE ANTIVIRUS](#configuración-de-antivirus)

---

## 🏗️ ARQUITECTURA DEL SISTEMA

### Stack Tecnológico

| **Capa** | **Tecnología** | **Versión** | **Propósito** |
|-----------|----------------|-------------|---------------|
| **Frontend** | ASP.NET MVC | 4.8+ | Interfaz de usuario y controladores |
| **Backend** | .NET Framework | 4.8+ | Lógica de negocio y servicios |
| **Base de Datos** | SQL Server | 2016+ | Almacenamiento de datos |
| **ORM** | Entity Framework | 6.4+ | Mapeo objeto-relacional |
| **Web Services** | WCF/ASMX | .NET 4.8 | Comunicación con sistemas externos |
| **Logging** | NLog | 4.7+ | Sistema de logging estructurado |
| **Caching** | Memory Cache | Built-in | Caché en memoria |
| **Authentication** | Windows Auth | Built-in | Autenticación corporativa |

### Estructura de Proyectos

```
BCN Module/
├── CapaPresentacion/           # ASP.NET MVC Web App
│   ├── Controllers/            # Controladores MVC
│   ├── Views/                  # Vistas Razor
│   ├── Models/                 # ViewModels
│   └── Scripts/                # JavaScript y CSS
├── DotNetBcnModule.Services/   # Capa de Servicios
│   ├── Services/               # Implementación de servicios
│   ├── Contracts/              # Interfaces de servicios
│   ├── Models/                 # Modelos de dominio
│   └── Queries/                # Consultas SQL
```

### Patrones de Diseño Implementados

- **MVC Pattern**: Separación de responsabilidades en la capa de presentación
- **Repository Pattern**: Abstracción del acceso a datos
- **Service Layer Pattern**: Lógica de negocio encapsulada
- **Factory Pattern**: Creación de conexiones a bases de datos
- **Observer Pattern**: Notificaciones de cambios de estado

---

## 🗄️ ESTRUCTURA DE DATOS

### Modelos de Datos

#### Modelo de Inventario
```csharp
public class InventarioModel
{
    public int Item { get; set; }
    public string Producto { get; set; }
    public string Almacen { get; set; }
    public bool FotoInventario { get; set; }
    public bool VoBo { get; set; }
    public decimal API { get; set; }
    public decimal VolumenTotal { get; set; }
    public decimal VolumenBombeable { get; set; }
    public decimal VolumenRemanente { get; set; }
    public string UMVolumen { get; set; }
    public decimal MasaTotal { get; set; }
    public decimal MasaBombeable { get; set; }
    public decimal MasaRemanente { get; set; }
    public string UMMasa { get; set; }
    public string IDMuestra { get; set; }
    public string Estado { get; set; }
}
```

#### Modelo de Movimiento Logístico
```csharp
public class MovimientoLogisticoModel
{
    public int Item { get; set; }
    public string IDMessage { get; set; }
    public string ClaseMovimiento { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string RecursoOrigen { get; set; }
    public string ProductoOrigen { get; set; }
    public string RecursoDestino { get; set; }
    public string ProductoDestino { get; set; }
    public DateTime FechaContabilizacion { get; set; }
    public decimal ValorContable { get; set; }
    public string UM { get; set; }
    public string NumeroPedido { get; set; }
    public string PosicionPedido { get; set; }
    public string UMPedido { get; set; }
    public string CeCo { get; set; }
    public string Estado { get; set; }
}
```

---

## ⚙️ CONFIGURACIONES Y PARÁMETROS

### Archivo de Configuración Principal

**Ubicación**: `Web.config` en la aplicación principal

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="ARESConnection" 
         connectionString="Server=ARES-SERVER;Database=ARES_DB;User Id=BCN_USER;Password=ENCODED_PASSWORD;" 
         providerName="System.Data.SqlClient" />
    <add name="ROMSSConnection" 
         connectionString="Server=ROMSS-SERVER;Database=ROMSS_DB;User Id=BCN_USER;Password=ENCODED_PASSWORD;" 
         providerName="System.Data.SqlClient" />
    <add name="BCNConnection" 
         connectionString="Server=BCN-SERVER;Database=BCN_DB;User Id=BCN_USER;Password=ENCODED_PASSWORD;" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <appSettings>
    <add key="ARESWebServiceURL" value="https://ares.ecopetrol.com/api/v1" />
    <add key="SAPECCEndpoint" value="https://sap.ecopetrol.com/ecp" />
    <add key="LogLevel" value="INFO" />
    <add key="MaxRetryAttempts" value="3" />
    <add key="ConnectionTimeout" value="30" />
    <add key="CommandTimeout" value="120" />
    <add key="BatchSize" value="1000" />
    <add key="EnableCaching" value="true" />
    <add key="CacheExpirationMinutes" value="15" />
  </appSettings>
  
  <system.web>
    <authentication mode="Windows" />
    <authorization>
      <deny users="?" />
    </authorization>
    <compilation debug="false" targetFramework="4.8" />
    <httpRuntime targetFramework="4.8" maxRequestLength="102400" executionTimeout="3600" />
  </system.web>
</configuration>
```

### Parámetros de Conexión

#### Cadena de Conexión Estándar
```csharp
// Formato estándar para SQL Server
"Server={SERVER_NAME};Database={DATABASE_NAME};User Id={USER_ID};Password={PASSWORD};"

// Parámetros adicionales recomendados
"Server={SERVER_NAME};Database={DATABASE_NAME};User Id={USER_ID};Password={PASSWORD};" +
"Connection Timeout=30;Command Timeout=120;Max Pool Size=100;Min Pool Size=5;" +
"Application Name=BCN_Module;Workstation ID={MACHINE_NAME};"
```

#### Configuración de Connection Pool
```csharp
// Configuración recomendada para alta concurrencia
"Max Pool Size=100;Min Pool Size=5;Pooling=true;Connection Lifetime=300;"
```

### Parámetros de Rendimiento

| **Parámetro** | **Valor Recomendado** | **Descripción** |
|----------------|------------------------|-----------------|
| **MaxRetryAttempts** | 3 | Número máximo de reintentos en caso de fallo |
| **ConnectionTimeout** | 30 segundos | Tiempo de espera para establecer conexión |
| **CommandTimeout** | 120 segundos | Tiempo de espera para ejecutar comando |
| **BatchSize** | 1000 | Tamaño del lote para operaciones masivas |
| **CacheExpirationMinutes** | 15 minutos | Tiempo de expiración del caché |
| **MaxPoolSize** | 100 | Número máximo de conexiones en el pool |

---

## 📝 LOGS Y MONITOREO

### Configuración de NLog

**Archivo**: `nlog.config`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <!-- Archivo de logs principal -->
    <target name="logfile" xsi:type="File"
            fileName="${basedir}/logs/bcn_module_${shortdate}.log"
            layout="${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=tostring}" />
    
    <!-- Archivo de logs de errores -->
    <target name="errorfile" xsi:type="File"
            fileName="${basedir}/logs/errors_${shortdate}.log"
            layout="${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=tostring}" />
    
    <!-- Base de datos para auditoría -->
    <target name="database" xsi:type="Database"
            connectionString="name=BCNConnection"
            commandText="INSERT INTO LogOperaciones (Usuario, Operacion, FechaOperacion, Parametros, Resultado, TiempoEjecucion, IPAddress) VALUES (@Usuario, @Operacion, @FechaOperacion, @Parametros, @Resultado, @TiempoEjecucion, @IPAddress)">
      <parameter name="@Usuario" layout="${aspnet-user-identity}" />
      <parameter name="@Operacion" layout="${event-properties:item=Operacion}" />
      <parameter name="@FechaOperacion" layout="${date}" />
      <parameter name="@Parametros" layout="${event-properties:item=Parametros}" />
      <parameter name="@Resultado" layout="${event-properties:item=Resultado}" />
      <parameter name="@TiempoEjecucion" layout="${event-properties:item=TiempoEjecucion}" />
      <parameter name="@IPAddress" layout="${aspnet-request-ip}" />
    </target>
  </targets>

  <rules>
    <!-- Logs de información general -->
    <logger name="*" minlevel="Info" writeTo="logfile" />
    
    <!-- Logs de errores -->
    <logger name="*" minlevel="Error" writeTo="errorfile,database" />
    
    <!-- Logs específicos del sistema -->
    <logger name="BCNModule.*" minlevel="Debug" writeTo="logfile" />
  </rules>
</nlog>
```

### Estructura de Logs

#### Nivel INFO
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "INFO",
  "logger": "BCNModule.IntegrationController",
  "operation": "DynamicQuery",
  "parameters": {
    "option": "01",
    "type": "integrar",
    "fechaIni": "2024-01-15 00:00:00",
    "fechaFin": "2024-01-15 23:59:59"
  },
  "result": {
    "success": true,
    "recordCount": 1250,
    "executionTime": "2.5s"
  },
  "user": "AdminBCN",
  "ipAddress": "192.168.1.100"
}
```

#### Nivel ERROR
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "ERROR",
  "logger": "BCNModule.DataService",
  "operation": "ExecuteQuery",
  "error": {
    "type": "SqlException",
    "message": "Login failed for user 'BCN_USER'",
    "code": 18456,
    "stackTrace": "..."
  },
  "context": {
    "connectionString": "Server=ARES-SERVER;Database=ARES_DB;...",
    "query": "SELECT * FROM Inventarios WHERE...",
    "parameters": {...}
  },
  "user": "AdminBCN",
  "ipAddress": "192.168.1.100"
}
```

### Métricas de Monitoreo

#### Métricas de Sistema
```csharp
public class SystemMetrics
{
    public int ActiveConnections { get; set; }
    public double MemoryUsagePercentage { get; set; }
    public double CpuUsagePercentage { get; set; }
    public int RequestsPerSecond { get; set; }
    public double AverageResponseTime { get; set; }
    public int ErrorRate { get; set; }
    public int CacheHitRate { get; set; }
}
```

#### Métricas de Negocio
```csharp
public class BusinessMetrics
{
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public double ProcessingTime { get; set; }
    public int IntegrationSuccessRate { get; set; }
    public int ConsolidationSuccessRate { get; set; }
    public int ARESSendSuccessRate { get; set; }
}
```

---

## 🚨 MANEJO DE ERRORES

### Estrategia de Manejo de Excepciones

#### Niveles de Error

| **Nivel** | **Tipo de Error** | **Acción** | **Notificación** |
|------------|-------------------|------------|-------------------|
| **CRÍTICO** | Error de conexión a BD principal | Fallback a BD secundaria | Email + SMS inmediato |
| **ALTO** | Error en WebService ARES | Reintento automático | Email en 5 minutos |
| **MEDIO** | Error de validación de datos | Log y continuar | Email en 15 minutos |
| **BAJO** | Warning de rendimiento | Log y monitoreo | Email diario |

#### Implementación de Retry Pattern

```csharp
public class RetryPolicy
{
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _delayBetweenRetries;
    
    public RetryPolicy(int maxRetryAttempts = 3, int delaySeconds = 5)
    {
        _maxRetryAttempts = maxRetryAttempts;
        _delayBetweenRetries = TimeSpan.FromSeconds(delaySeconds);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        var lastException = new Exception();
        
        for (int attempt = 1; attempt <= _maxRetryAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt == _maxRetryAttempts)
                    throw new RetryException($"Operation failed after {_maxRetryAttempts} attempts", lastException);
                
                if (ShouldRetry(ex))
                {
                    await Task.Delay(_delayBetweenRetries);
                    continue;
                }
                
                throw;
            }
        }
        
        throw lastException;
    }
    
    private bool ShouldRetry(Exception ex)
    {
        // Reintentar en errores transitorios
        return ex is SqlException sqlEx && 
               (sqlEx.Number == 1205 || // Deadlock
                sqlEx.Number == 1222 || // Lock timeout
                sqlEx.Number == 8645);  // Connection timeout
    }
}
```

#### Circuit Breaker Pattern

```csharp
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;
    private CircuitBreakerState _state;
    private int _failureCount;
    private DateTime _lastFailureTime;
    
    public CircuitBreaker(int failureThreshold = 5, int resetTimeoutSeconds = 60)
    {
        _failureThreshold = failureThreshold;
        _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds);
        _state = CircuitBreakerState.Closed;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                _state = CircuitBreakerState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
        }
        
        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure();
            throw;
        }
    }
    
    private void OnSuccess()
    {
        _failureCount = 0;
        _state = CircuitBreakerState.Closed;
    }
    
    private void OnFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
        
        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitBreakerState.Open;
        }
    }
}
```

---

## ⚡ PERFORMANCE Y OPTIMIZACIÓN

### Estrategias de Caching

#### Caching en Memoria

```csharp
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;
    
    public MemoryCacheService(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _defaultExpiration = TimeSpan.FromMinutes(
            int.Parse(configuration["CacheExpirationMinutes"] ?? "15"));
    }
    
    public T Get<T>(string key)
    {
        return _cache.Get<T>(key);
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration,
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };
        
        _cache.Set(key, value, options);
    }
    
    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
```

#### Caching de Consultas

```csharp
public class CachedQueryService : IQueryService
{
    private readonly IQueryService _queryService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedQueryService> _logger;
    
    public async Task<QueryResult> GetDataAsync(DateTime fromDate, DateTime toDate)
    {
        var cacheKey = $"query_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
        
        var cachedResult = _cacheService.Get<QueryResult>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResult;
        }
        
        var result = await _queryService.GetDataAsync(fromDate, toDate);
        
        _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(15));
        _logger.LogInformation("Cache miss for key: {CacheKey}, stored result", cacheKey);
        
        return result;
    }
}
```

### Optimización de Consultas SQL

#### Consultas Optimizadas

```sql
-- Consulta optimizada para inventarios
SELECT 
    i.ID,
    i.Producto,
    i.Almacen,
    i.VolumenTotal,
    i.MasaTotal,
    i.API,
    i.FechaContabilizacion
FROM Inventarios i WITH (NOLOCK)
WHERE i.FechaContabilizacion BETWEEN @FechaInicio AND @FechaFin
    AND i.Estado = 'Activo'
ORDER BY i.Almacen, i.Producto;

-- Índices recomendados
CREATE NONCLUSTERED INDEX IX_Inventarios_FechaEstado 
ON Inventarios (FechaContabilizacion, Estado) 
INCLUDE (Producto, Almacen, VolumenTotal, MasaTotal, API);

CREATE NONCLUSTERED INDEX IX_Inventarios_AlmacenProducto 
ON Inventarios (Almacen, Producto) 
INCLUDE (VolumenTotal, MasaTotal, API, FechaContabilizacion, Estado);
```

#### Consultas con Paginación

```sql
-- Paginación eficiente con OFFSET-FETCH
SELECT 
    i.ID,
    i.Producto,
    i.Almacen,
    i.VolumenTotal,
    i.MasaTotal
FROM Inventarios i WITH (NOLOCK)
WHERE i.FechaContabilizacion BETWEEN @FechaInicio AND @FechaFin
ORDER BY i.Almacen, i.Producto
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;

-- Conteo total para paginación
SELECT COUNT(*) 
FROM Inventarios i WITH (NOLOCK)
WHERE i.FechaContabilizacion BETWEEN @FechaInicio AND @FechaFin;
```

### Configuración de Connection Pool

```csharp
public class DatabaseConnectionFactory
{
    private readonly string _connectionString;
    private readonly int _maxPoolSize;
    private readonly int _minPoolSize;
    
    public DatabaseConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BCNConnection");
        _maxPoolSize = int.Parse(configuration["MaxPoolSize"] ?? "100");
        _minPoolSize = int.Parse(configuration["MinPoolSize"] ?? "5");
    }
    
    public SqlConnection CreateConnection()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            MaxPoolSize = _maxPoolSize,
            MinPoolSize = _minPoolSize,
            Pooling = true,
            ConnectionLifetime = 300 // 5 minutos
        };
        
        return new SqlConnection(builder.ToString());
    }
}
```

---

## 🔒 SEGURIDAD

### Autenticación y Autorización

#### Configuración de Windows Authentication

```xml
<!-- Web.config -->
<system.web>
  <authentication mode="Windows" />
  <authorization>
    <deny users="?" />
  </authorization>
</system.web>

<system.webServer>
  <security>
    <authentication>
      <anonymousAuthentication enabled="false" />
      <windowsAuthentication enabled="true" />
    </authentication>
  </security>
</system.webServer>
```

#### Verificación de Roles

```csharp
[Authorize(Roles = "BCN_Users,BCN_Admins")]
public class IntegrationController : Controller
{
    [Authorize(Roles = "BCN_Admins")]
    public async Task<ActionResult> ExecuteOperation(string option, DateTime fromDate, DateTime toDate)
    {
        // Solo usuarios con rol de administrador pueden ejecutar operaciones
        var user = User.Identity.Name;
        var roles = ((WindowsIdentity)User.Identity).Groups
            .Select(g => g.Translate(typeof(NTAccount)).Value);
        
        if (!roles.Any(r => r.Contains("BCN_Admins")))
        {
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Insufficient permissions");
        }
        
        // Lógica de ejecución
    }
}
```

### Encriptación de Datos Sensibles

#### Codificación Base64 para Cadenas de Conexión

```csharp
public class SecurityService : ISecurityService
{
    public string EncodeBase64(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;
        
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
    
    public string DecodeBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return string.Empty;
        
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            // Si no es Base64 válido, retornar el valor original
            return base64;
        }
    }
    
    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
```

#### Validación de Entrada

```csharp
public class InputValidationService : IInputValidationService
{
    public ValidationResult ValidateDateRange(DateTime fromDate, DateTime toDate)
    {
        var result = new ValidationResult();
        
        if (fromDate > DateTime.Now)
        {
            result.AddError("Fecha de inicio no puede ser futura");
        }
        
        if (toDate > DateTime.Now)
        {
            result.AddError("Fecha de fin no puede ser futura");
        }
        
        if (fromDate > toDate)
        {
            result.AddError("Fecha de inicio debe ser anterior a fecha de fin");
        }
        
        return result;
    }
    
    public ValidationResult ValidateOption(string option)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(option))
        {
            result.AddError("Opción no puede estar vacía");
            return result;
        }
        
        var validOptions = new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };
        if (!validOptions.Contains(option))
        {
            result.AddError($"Opción '{option}' no es válida");
        }
        
        return result;
    }
}
```

---

## 🔗 INTEGRACIÓN CON SISTEMAS EXTERNOS

### WebService ARES

#### Configuración del Cliente

```csharp
public class ARESWebServiceClient : IARESWebServiceClient
{
    private readonly string _serviceUrl;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ARESWebServiceClient> _logger;
    
    public ARESWebServiceClient(IConfiguration configuration, ILogger<ARESWebServiceClient> logger)
    {
        _serviceUrl = configuration["ARESWebServiceURL"];
        _apiKey = configuration["ARESApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        _logger = logger;
    }
    
    public async Task<ARESResponse> SendInventoryDataAsync(List<InventoryData> data)
    {
        try
        {
            var request = new ARESInventoryRequest
            {
                Timestamp = DateTime.UtcNow,
                Data = data.Select(d => new ARESInventoryItem
                {
                    ProductId = d.ProductId,
                    StorageId = d.StorageId,
                    Quantity = d.Quantity,
                    Unit = d.Unit
                }).ToList()
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_serviceUrl}/inventory", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ARESResponse>(responseContent);
            }
            
            throw new ARESException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending data to ARES");
            throw;
        }
    }
}
```

#### Modelos de Datos ARES

```csharp
public class ARESInventoryRequest
{
    public DateTime Timestamp { get; set; }
    public List<ARESInventoryItem> Data { get; set; }
}

public class ARESInventoryItem
{
    public string ProductId { get; set; }
    public string StorageId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}

public class ARESResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

### Integración con SAP ECC ECP

#### Cliente SAP

```csharp
public class SAPECEClient : ISAPECEClient
{
    private readonly string _endpoint;
    private readonly string _username;
    private readonly string _password;
    private readonly ILogger<SAPECEClient> _logger;
    
    public async Task<SAPProcessingStatus> GetProcessingStatusAsync(DateTime date)
    {
        try
        {
            // Implementación específica para SAP ECC ECP
            // Usar librería SAP .NET Connector o REST API según configuración
            
            var status = new SAPProcessingStatus
            {
                Date = date,
                Status = "Completed",
                ProcessedRecords = 1250,
                FailedRecords = 0,
                ProcessingTime = TimeSpan.FromMinutes(15)
            };
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SAP processing status for date: {Date}", date);
            throw;
        }
    }
}
```

---

## 💾 BACKUP Y RECUPERACIÓN

### Estrategia de Backup

#### Backup de Base de Datos

```sql
-- Script de backup automático
USE master;
GO

-- Backup completo diario
BACKUP DATABASE [BCN_DB] 
TO DISK = 'C:\Backups\BCN_DB_Full_' + CONVERT(VARCHAR(8), GETDATE(), 112) + '.bak'
WITH COMPRESSION, CHECKSUM, STATS = 10;

-- Backup de transacciones cada 15 minutos
BACKUP LOG [BCN_DB] 
TO DISK = 'C:\Backups\BCN_DB_Log_' + CONVERT(VARCHAR(8), GETDATE(), 112) + '_' + 
          RIGHT('0' + CAST(DATEPART(HOUR, GETDATE()) AS VARCHAR(2)), 2) + 
          RIGHT('0' + CAST(DATEPART(MINUTE, GETDATE()) AS VARCHAR(2)), 2) + '.trn'
WITH COMPRESSION, CHECKSUM, STATS = 10;
```

#### Backup de Configuración

```csharp
public class ConfigurationBackupService : IConfigurationBackupService
{
    private readonly string _backupPath;
    private readonly ILogger<ConfigurationBackupService> _logger;
    
    public async Task BackupConfigurationAsync()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web.config");
            var backupFileName = $"WebConfig_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupPath = Path.Combine(_backupPath, backupFileName);
            
            File.Copy(configPath, backupPath, true);
            
            _logger.LogInformation("Configuration backed up to: {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up configuration");
            throw;
        }
    }
    
    public async Task RestoreConfigurationAsync(string backupFileName)
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web.config");
            var backupPath = Path.Combine(_backupPath, backupFileName);
            
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupFileName}");
            }
            
            // Crear backup del archivo actual antes de restaurar
            var currentBackup = $"WebConfig_Current_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var currentBackupPath = Path.Combine(_backupPath, currentBackup);
            File.Copy(configPath, currentBackupPath, true);
            
            // Restaurar configuración
            File.Copy(backupPath, configPath, true);
            
            _logger.LogInformation("Configuration restored from: {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring configuration from: {BackupFileName}", backupFileName);
            throw;
        }
    }
}
```

### Recuperación de Desastres

#### Procedimiento de Recuperación

```csharp
public class DisasterRecoveryService : IDisasterRecoveryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DisasterRecoveryService> _logger;
    
    public async Task<RecoveryResult> PerformRecoveryAsync(RecoveryType recoveryType)
    {
        var result = new RecoveryResult
        {
            StartTime = DateTime.UtcNow,
            RecoveryType = recoveryType
        };
        
        try
        {
            switch (recoveryType)
            {
                case RecoveryType.Database:
                    await RecoverDatabaseAsync();
                    break;
                case RecoveryType.Configuration:
                    await RecoverConfigurationAsync();
                    break;
                case RecoveryType.Full:
                    await RecoverFullSystemAsync();
                    break;
            }
            
            result.Success = true;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
            
            _logger.LogInformation("Recovery completed successfully. Type: {RecoveryType}, Duration: {Duration}", 
                recoveryType, result.Duration);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
            
            _logger.LogError(ex, "Recovery failed. Type: {RecoveryType}", recoveryType);
        }
        
        return result;
    }
    
    private async Task RecoverDatabaseAsync()
    {
        // Restaurar base de datos desde último backup
        var backupPath = GetLatestBackupPath();
        await RestoreDatabaseFromBackupAsync(backupPath);
    }
    
    private async Task RecoverConfigurationAsync()
    {
        // Restaurar configuración desde backup
        var configBackup = GetLatestConfigurationBackup();
        await RestoreConfigurationFromBackupAsync(configBackup);
    }
    
    private async Task RecoverFullSystemAsync()
    {
        // Recuperación completa del sistema
        await RecoverDatabaseAsync();
        await RecoverConfigurationAsync();
        await RestartApplicationServicesAsync();
    }
}
```

---

## 🔧 TROUBLESHOOTING AVANZADO

### Diagnóstico de Problemas de Conexión

#### Verificación de Conectividad

```csharp
public class ConnectivityDiagnosticService : IConnectivityDiagnosticService
{
    public async Task<ConnectivityReport> DiagnoseConnectivityAsync()
    {
        var report = new ConnectivityReport
        {
            Timestamp = DateTime.UtcNow,
            Checks = new List<ConnectivityCheck>()
        };
        
        // Verificar conectividad de red
        await CheckNetworkConnectivityAsync(report);
        
        // Verificar conectividad a bases de datos
        await CheckDatabaseConnectivityAsync(report);
        
        // Verificar conectividad a WebServices
        await CheckWebServiceConnectivityAsync(report);
        
        // Verificar DNS y resolución de nombres
        await CheckDNSResolutionAsync(report);
        
        return report;
    }
    
    private async Task CheckDatabaseConnectivityAsync(ConnectivityReport report)
    {
        var databases = new[] { "ARES", "ROMSS", "BCN" };
        
        foreach (var db in databases)
        {
            var check = new ConnectivityCheck
            {
                Component = $"Database_{db}",
                Type = ConnectivityType.Database
            };
            
            try
            {
                using (var connection = CreateConnection(db))
                {
                    await connection.OpenAsync();
                    
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT 1";
                    command.CommandTimeout = 10;
                    
                    var result = await command.ExecuteScalarAsync();
                    
                    check.Status = ConnectivityStatus.Success;
                    check.ResponseTime = TimeSpan.FromMilliseconds(100); // Ejemplo
                    check.Details = "Connection successful";
                }
            }
            catch (Exception ex)
            {
                check.Status = ConnectivityStatus.Failed;
                check.ErrorMessage = ex.Message;
                check.Details = $"Connection failed: {ex.GetType().Name}";
            }
            
            report.Checks.Add(check);
        }
    }
}
```

#### Análisis de Logs

```csharp
public class LogAnalysisService : ILogAnalysisService
{
    public async Task<LogAnalysisReport> AnalyzeLogsAsync(DateTime fromDate, DateTime toDate)
    {
        var report = new LogAnalysisReport
        {
            FromDate = fromDate,
            ToDate = toDate,
            Analysis = new List<LogPattern>()
        };
        
        // Analizar patrones de error
        await AnalyzeErrorPatternsAsync(report);
        
        // Analizar patrones de rendimiento
        await AnalyzePerformancePatternsAsync(report);
        
        // Analizar patrones de uso
        await AnalyzeUsagePatternsAsync(report);
        
        return report;
    }
    
    private async Task AnalyzeErrorPatternsAsync(LogAnalysisReport report)
    {
        var errorPatterns = new Dictionary<string, int>();
        
        // Leer logs y agrupar errores por tipo
        var logFiles = GetLogFiles(fromDate, toDate);
        
        foreach (var logFile in logFiles)
        {
            var lines = await File.ReadAllLinesAsync(logFile);
            
            foreach (var line in lines)
            {
                if (line.Contains("ERROR"))
                {
                    var errorType = ExtractErrorType(line);
                    if (errorPatterns.ContainsKey(errorType))
                        errorPatterns[errorType]++;
                    else
                        errorPatterns[errorType] = 1;
                }
            }
        }
        
        // Crear patrones de error
        foreach (var pattern in errorPatterns.OrderByDescending(p => p.Value))
        {
            report.Analysis.Add(new LogPattern
            {
                Type = "Error",
                Pattern = pattern.Key,
                Frequency = pattern.Value,
                Severity = GetErrorSeverity(pattern.Key)
            });
        }
    }
}
```

### Monitoreo de Recursos del Sistema

#### Monitoreo de Memoria y CPU

```csharp
public class SystemResourceMonitor : ISystemResourceMonitor
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly ILogger<SystemResourceMonitor> _logger;
    
    public SystemResourceMonitor(ILogger<SystemResourceMonitor> logger)
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        _logger = logger;
    }
    
    public async Task<SystemResourceStatus> GetCurrentStatusAsync()
    {
        var status = new SystemResourceStatus
        {
            Timestamp = DateTime.UtcNow,
            CpuUsage = _cpuCounter.NextValue(),
            AvailableMemory = _memoryCounter.NextValue(),
            TotalMemory = GetTotalMemory(),
            ProcessMemory = GetProcessMemory()
        };
        
        status.MemoryUsagePercentage = ((status.TotalMemory - status.AvailableMemory) / status.TotalMemory) * 100;
        
        // Verificar umbrales de alerta
        if (status.CpuUsage > 80 || status.MemoryUsagePercentage > 80)
        {
            _logger.LogWarning("High resource usage detected. CPU: {CpuUsage}%, Memory: {MemoryUsage}%", 
                status.CpuUsage, status.MemoryUsagePercentage);
        }
        
        return status;
    }
    
    private long GetTotalMemory()
    {
        var computerInfo = new ComputerInfo();
        return computerInfo.TotalPhysicalMemory / (1024 * 1024); // Convertir a MB
    }
    
    private long GetProcessMemory()
    {
        var process = Process.GetCurrentProcess();
        return process.WorkingSet64 / (1024 * 1024); // Convertir a MB
    }
}
```

---

## 📦 VERSIONAMIENTO Y PUBLICACIÓN

### Estrategia de Versionamiento

#### Esquema de Versionado Semántico

**Formato**: `MAJOR.MINOR.PATCH-BUILD`

| **Componente** | **Descripción** | **Ejemplo** |
|----------------|-----------------|-------------|
| **MAJOR** | Cambios incompatibles con versiones anteriores | 2.0.0 |
| **MINOR** | Nuevas funcionalidades compatibles | 1.5.0 |
| **PATCH** | Correcciones de bugs compatibles | 1.4.3 |
| **BUILD** | Número de build automático | 1.4.3.20240115 |

#### Archivos de Versionamiento

```xml
<!-- AssemblyInfo.cs -->
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-Beta")]

<!-- packages.config -->
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="EntityFramework" version="6.4.4" targetFramework="net48" />
  <package id="NLog" version="4.7.15" targetFramework="net48" />
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
</packages>

<!-- .csproj -->
<PropertyGroup>
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <PackageVersion>1.0.0</PackageVersion>
</PropertyGroup>
```

#### Control de Versiones con Git

```bash
# Estructura de ramas recomendada
main (producción)
├── develop (desarrollo)
├── feature/BCN-001-nueva-funcionalidad
├── hotfix/BCN-002-correccion-critica
└── release/v1.1.0

# Comandos para versionado
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# Crear rama de release
git checkout -b release/v1.1.0 develop
git checkout main
git merge release/v1.1.0
git tag -a v1.1.0 -m "Release version 1.1.0"
git branch -d release/v1.1.0
```

### Archivos Críticos para Publicación

#### Archivos Obligatorios

| **Archivo** | **Propósito** | **Ubicación** | **Crítico** |
|-------------|---------------|---------------|-------------|
| **Web.config** | Configuración principal | `/` | 🔴 SÍ |
| **Global.asax** | Punto de entrada | `/` | 🔴 SÍ |
| **AssemblyInfo.cs** | Metadatos del ensamblado | `/Properties/` | 🔴 SÍ |
| **packages.config** | Dependencias NuGet | `/` | 🔴 SÍ |
| **.csproj** | Proyecto principal | `/` | 🔴 SÍ |
| **.sln** | Solución completa | `/` | 🔴 SÍ |
| **nlog.config** | Configuración de logging | `/` | 🟡 SÍ |
| **Web.Debug.config** | Configuración debug | `/` | 🟡 NO |
| **Web.Release.config** | Configuración release | `/` | 🟡 SÍ |

#### Estructura de Archivos para Publicación

```
BCN_Module_Publish/
├── bin/                          # Ensamblados compilados
│   ├── DotNetBcnModule.Services.dll
│   ├── DotNetBcnModule.Presentation.dll
│   └── *.dll (dependencias)
├── Content/                      # Archivos CSS y recursos
├── Scripts/                      # Archivos JavaScript
├── Views/                        # Vistas Razor
├── Web.config                    # Configuración principal
├── Global.asax                   # Punto de entrada
├── nlog.config                   # Configuración de logging
└── packages.config               # Dependencias NuGet
```

### Proceso de Compilación

#### Configuración de Build

```xml
<!-- .csproj - Configuración de Build -->
<PropertyGroup>
  <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
  <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
  <ProjectGuid>{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}</ProjectGuid>
  <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
  <OutputType>Library</OutputType>
  <AppDesignerFolder>Properties</AppDesignerFolder>
  <RootNamespace>DotNetBcnModule.Presentation</RootNamespace>
  <AssemblyName>DotNetBcnModule.Presentation</AssemblyName>
  <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  <UseIISExpress>true</UseIISExpress>
  <Use64BitIISExpress />
  <IISExpressSSLPort />
  <IISExpressAnonymousAuthentication />
  <IISExpressWindowsAuthentication />
  <IISExpressUseClassicPipelineMode />
  <UseGlobalApplicationHostFile />
</PropertyGroup>

<!-- Configuraciones de Build -->
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
  <DebugSymbols>true</DebugSymbols>
  <DebugType>full</DebugType>
  <Optimize>false</Optimize>
  <OutputPath>bin\</OutputPath>
  <DefineConstants>DEBUG;TRACE</DefineConstants>
  <ErrorReport>prompt</ErrorReport>
  <WarningLevel>4</WarningLevel>
</PropertyGroup>

<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
  <DebugType>pdbonly</DebugType>
  <Optimize>true</Optimize>
  <OutputPath>bin\</OutputPath>
  <DefineConstants>TRACE</DefineConstants>
  <ErrorReport>prompt</ErrorReport>
  <WarningLevel>4</WarningLevel>
</PropertyGroup>
```

#### Script de Build Automatizado

```powershell
# build-and-publish.ps1
param(
    [string]$Configuration = "Release",
    [string]$Platform = "AnyCPU",
    [string]$OutputPath = ".\Publish",
    [string]$Version = "1.0.0"
)

Write-Host "=== BCN Module Build Script ===" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow

# Limpiar directorio de salida
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null

# Restaurar paquetes NuGet
Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Cyan
nuget restore "DotNetBcnModule.Presentation.sln"

# Compilar solución
Write-Host "Compilando solución..." -ForegroundColor Cyan
msbuild "DotNetBcnModule.Presentation.sln" /p:Configuration=$Configuration /p:Platform=$Platform /p:Version=$Version /verbosity:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en la compilación!" -ForegroundColor Red
    exit 1
}

# Publicar aplicación
Write-Host "Publicando aplicación..." -ForegroundColor Cyan
msbuild "DotNetBcnModule.Presentation.sln" /p:Configuration=$Configuration /p:Platform=$Platform /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:PublishUrl=$OutputPath

# Copiar archivos adicionales
Write-Host "Copiando archivos adicionales..." -ForegroundColor Cyan
Copy-Item "nlog.config" -Destination $OutputPath -Force
Copy-Item "packages.config" -Destination $OutputPath -Force

# Verificar archivos críticos
Write-Host "Verificando archivos críticos..." -ForegroundColor Cyan
$criticalFiles = @("Web.config", "Global.asax", "bin\DotNetBcnModule.Presentation.dll")
foreach ($file in $criticalFiles) {
    if (Test-Path "$OutputPath\$file") {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - FALTANTE!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Build completado exitosamente!" -ForegroundColor Green
Write-Host "Output: $OutputPath" -ForegroundColor Cyan
```

### Problemas Comunes en Publicación

#### 1. Errores de Compilación

| **Error** | **Causa** | **Solución** |
|------------|-----------|---------------|
| **CS0234** | Namespace no encontrado | Verificar referencias del proyecto |
| **CS0103** | Variable no definida | Verificar scope y declaraciones |
| **CS1503** | Tipos incompatibles | Verificar conversiones de tipos |
| **CS0246** | Tipo no encontrado | Verificar using statements |

**Solución Detallada:**
```bash
# Limpiar y reconstruir
dotnet clean
dotnet restore
dotnet build --configuration Release

# Verificar referencias
dotnet list package
dotnet add package [nombre-paquete]
```

#### 2. Errores de Dependencias

| **Error** | **Causa** | **Solución** |
|------------|-----------|---------------|
| **Could not load file or assembly** | DLL faltante | Verificar packages.config |
| **Version conflict** | Versiones incompatibles | Actualizar paquetes NuGet |
| **Missing reference** | Referencia no agregada | Agregar referencia al proyecto |

**Solución Detallada:**
```xml
<!-- packages.config - Verificar versiones -->
<packages>
  <package id="EntityFramework" version="6.4.4" targetFramework="net48" />
  <package id="NLog" version="4.7.15" targetFramework="net48" />
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
</packages>

<!-- Comandos de solución -->
nuget restore
nuget update -self
nuget update DotNetBcnModule.Presentation.sln
```

#### 3. Errores de Configuración

| **Error** | **Causa** | **Solución** |
|------------|-----------|---------------|
| **Connection string not found** | Web.config corrupto | Restaurar desde backup |
| **Authentication failed** | Configuración IIS | Verificar permisos |
| **Module not found** | Handler no registrado | Verificar web.config |

**Solución Detallada:**
```xml
<!-- Web.config - Verificar secciones críticas -->
<configuration>
  <connectionStrings>
    <add name="ARESConnection" 
         connectionString="Server=ARES-SERVER;Database=ARES_DB;User Id=BCN_USER;Password=ENCODED_PASSWORD;" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <system.web>
    <authentication mode="Windows" />
    <authorization>
      <deny users="?" />
    </authorization>
    <compilation debug="false" targetFramework="4.8" />
  </system.web>
  
  <system.webServer>
    <handlers>
      <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*." verb="*" type="System.Web.Handlers.TransferRequestHandler" preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
  </system.webServer>
</configuration>
```

#### 4. Errores de Permisos

| **Error** | **Causa** | **Solución** |
|------------|-----------|---------------|
| **Access denied** | Permisos insuficientes | Configurar IIS_IUSRS |
| **Cannot write to directory** | Permisos de escritura | Verificar permisos de carpeta |
| **Service unavailable** | Pool de aplicaciones | Reiniciar App Pool |

**Solución Detallada:**
```powershell
# Script de configuración de permisos
# Ejecutar como Administrador

$sitePath = "C:\inetpub\wwwroot\BCN_Module"
$appPoolName = "BCN_Module_Pool"

# Configurar permisos de carpeta
$acl = Get-Acl $sitePath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $sitePath $acl

# Configurar App Pool
Import-Module WebAdministration
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"

# Reiniciar App Pool
Restart-WebAppPool $appPoolName
```

### Checklist de Publicación

#### Antes de la Publicación

- [ ] **Código Fuente**
  - [ ] Todos los cambios committeados en Git
  - [ ] Tag de versión creado
  - [ ] Rama de release creada y probada
  - [ ] Code review completado

- [ ] **Dependencias**
  - [ ] packages.config actualizado
  - [ ] Todas las referencias del proyecto verificadas
  - [ ] Versiones de paquetes compatibles
  - [ ] NuGet restore ejecutado

- [ ] **Configuración**
  - [ ] Web.config validado
  - [ ] Connection strings verificados
  - [ ] AppSettings configurados
  - [ ] nlog.config presente

#### Durante la Publicación

- [ ] **Build**
  - [ ] Compilación exitosa en Release mode
  - [ ] Sin warnings críticos
  - [ ] Todos los archivos generados
  - [ ] Dependencias incluidas

- [ ] **Deployment**
  - [ ] Archivos copiados al servidor
  - [ ] Permisos configurados
  - [ ] App Pool configurado
  - [ ] IIS configurado

#### Después de la Publicación

- [ ] **Verificación**
  - [ ] Aplicación accesible
  - [ ] Logs funcionando
  - [ ] Base de datos conectada
  - [ ] Funcionalidades críticas probadas

- [ ] **Monitoreo**
  - [ ] Logs de aplicación revisados
  - [ ] Event Viewer verificado
  - [ ] Performance monitor activado
  - [ ] Alertas configuradas

### Rollback y Recuperación

#### Procedimiento de Rollback

```powershell
# Script de rollback automático
param(
    [string]$BackupPath = "C:\Backups\BCN_Module",
    [string]$SitePath = "C:\inetpub\wwwroot\BCN_Module",
    [string]$Version = "1.0.0"
)

Write-Host "=== Rollback BCN Module ===" -ForegroundColor Red

# Crear backup del estado actual
$currentBackup = "$BackupPath\Current_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Creando backup del estado actual: $currentBackup" -ForegroundColor Yellow
Copy-Item $SitePath $currentBackup -Recurse -Force

# Restaurar versión anterior
$rollbackPath = "$BackupPath\Version_$Version"
if (Test-Path $rollbackPath) {
    Write-Host "Restaurando versión: $Version" -ForegroundColor Yellow
    Remove-Item $SitePath -Recurse -Force
    Copy-Item $rollbackPath $SitePath -Recurse -Force
    
    # Reiniciar App Pool
    Import-Module WebAdministration
    Restart-WebAppPool "BCN_Module_Pool"
    
    Write-Host "Rollback completado exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error: No se encontró la versión $Version" -ForegroundColor Red
    exit 1
}
```

#### Estrategia de Backup para Rollback

```csharp
public class DeploymentBackupService : IDeploymentBackupService
{
    private readonly string _backupPath;
    private readonly string _sitePath;
    private readonly ILogger<DeploymentBackupService> _logger;
    
    public async Task<string> CreateDeploymentBackupAsync(string version)
    {
        try
        {
            var backupFolder = Path.Combine(_backupPath, $"Version_{version}_{DateTime.Now:yyyyMMdd_HHmmss}");
            
            if (!Directory.Exists(_backupPath))
                Directory.CreateDirectory(_backupPath);
            
            // Crear backup completo del sitio
            CopyDirectory(_sitePath, backupFolder);
            
            // Crear backup de base de datos
            await CreateDatabaseBackupAsync(version);
            
            _logger.LogInformation("Deployment backup created: {BackupPath}", backupFolder);
            return backupFolder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deployment backup for version: {Version}", version);
            throw;
        }
    }
    
    private void CopyDirectory(string source, string destination)
    {
        var dir = new DirectoryInfo(source);
        Directory.CreateDirectory(destination);
        
        foreach (var file in dir.GetFiles())
        {
            file.CopyTo(Path.Combine(destination, file.Name));
        }
        
        foreach (var subDir in dir.GetDirectories())
        {
            CopyDirectory(subDir.FullName, Path.Combine(destination, subDir.Name));
        }
    }
}
```

---

## 🔄 COMPATIBILIDAD Y REQUISITOS

### Requisitos del Sistema

#### Requisitos Mínimos

| **Componente** | **Requisito Mínimo** | **Recomendado** | **Notas** |
|----------------|----------------------|-----------------|-----------|
| **Sistema Operativo** | Windows Server 2016 | Windows Server 2019/2022 | Solo 64-bit |
| **Procesador** | 2 cores x64 | 4+ cores x64 | Intel/AMD compatible |
| **Memoria RAM** | 8 GB | 16+ GB | DDR4 recomendado |
| **Espacio en Disco** | 50 GB | 100+ GB | SSD recomendado |
| **.NET Framework** | 4.8 | 4.8 | No compatible con .NET Core |
| **SQL Server** | 2016 | 2019/2022 | Standard o Enterprise |
| **IIS** | 10.0 | 10.0 | Windows Server |

#### Compatibilidad de Navegadores

| **Navegador** | **Versión Mínima** | **Estado** | **Notas** |
|----------------|---------------------|------------|-----------|
| **Internet Explorer** | 11.0 | ✅ Compatible | Solo Windows |
| **Microsoft Edge** | 79+ | ✅ Compatible | Chromium-based |
| **Google Chrome** | 80+ | ✅ Compatible | Recomendado |
| **Mozilla Firefox** | 75+ | ✅ Compatible | Versión ESR |
| **Safari** | 13+ | ⚠️ Limitado | Solo macOS |

#### Compatibilidad de Base de Datos

| **Sistema** | **Versión** | **Estado** | **Configuración** |
|--------------|--------------|------------|-------------------|
| **SQL Server** | 2016+ | ✅ Principal | Always On, Mirroring |
| **Oracle** | 12c+ | ⚠️ Limitado | Solo consultas básicas |
| **PostgreSQL** | 10+ | ❌ No compatible | Requiere migración |
| **MySQL** | 8.0+ | ❌ No compatible | Requiere migración |

### Matriz de Compatibilidad

#### Versiones de .NET Framework

| **Versión** | **Estado** | **Funcionalidades** | **Limitaciones** |
|--------------|------------|---------------------|------------------|
| **4.5** | ❌ No compatible | Ninguna | Framework muy antiguo |
| **4.6** | ⚠️ Parcial | Básicas | Sin async/await completo |
| **4.7** | ⚠️ Parcial | Intermedias | Algunas features faltantes |
| **4.8** | ✅ Total | Completas | Versión recomendada |

#### Compatibilidad de Servidores

| **Servidor** | **Windows Server** | **IIS** | **Estado** |
|---------------|-------------------|---------|------------|
| **Desarrollo** | 2019 | 10.0 | ✅ Compatible |
| **QA/Testing** | 2019 | 10.0 | ✅ Compatible |
| **Producción** | 2022 | 10.0 | ✅ Compatible |

---

## 🔐 SEGURIDAD DE GUARDADO

### Estrategia de Encriptación

#### Encriptación de Datos Sensibles

```csharp
public class DataEncryptionService : IDataEncryptionService
{
    private readonly string _encryptionKey;
    private readonly string _encryptionIV;
    private readonly ILogger<DataEncryptionService> _logger;
    
    public DataEncryptionService(IConfiguration configuration, ILogger<DataEncryptionService> logger)
    {
        _encryptionKey = configuration["EncryptionKey"];
        _encryptionIV = configuration["EncryptionIV"];
        _logger = logger;
    }
    
    public string EncryptData(string plainText)
    {
        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_encryptionIV);
                
                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new EncryptionException("Failed to encrypt data", ex);
        }
    }
    
    public string DecryptData(string cipherText)
    {
        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_encryptionIV);
                
                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new DecryptionException("Failed to decrypt data", ex);
        }
    }
}
```

#### Configuración de Seguridad

```xml
<!-- Web.config - Configuración de Seguridad -->
<configuration>
  <appSettings>
    <!-- Claves de encriptación (Base64) -->
    <add key="EncryptionKey" value="YourBase64EncodedKeyHere" />
    <add key="EncryptionIV" value="YourBase64EncodedIVHere" />
    
    <!-- Configuración de seguridad -->
    <add key="EnableDataEncryption" value="true" />
    <add key="EncryptionAlgorithm" value="AES256" />
    <add key="KeyRotationDays" value="90" />
    <add key="SecureConnectionRequired" value="true" />
  </appSettings>
  
  <system.web>
    <!-- Configuración de seguridad web -->
    <httpCookies requireSSL="true" httpOnlyCookies="true" />
    <sessionState timeout="20" />
    <authentication mode="Windows" />
    <authorization>
      <deny users="?" />
    </authorization>
  </system.web>
  
  <system.webServer>
    <!-- Headers de seguridad -->
    <httpProtocol>
      <customHeaders>
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Strict-Transport-Security" value="max-age=31536000; includeSubDomains" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### Seguridad de Archivos

#### Protección de Archivos Sensibles

```csharp
public class FileSecurityService : IFileSecurityService
{
    private readonly string _secureDirectory;
    private readonly ILogger<FileSecurityService> _logger;
    
    public async Task<string> SaveSecureFileAsync(byte[] fileContent, string fileName, string userId)
    {
        try
        {
            // Crear directorio seguro si no existe
            var userDirectory = Path.Combine(_secureDirectory, userId);
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }
            
            // Generar nombre único para el archivo
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(userDirectory, uniqueFileName);
            
            // Encriptar contenido antes de guardar
            var encryptedContent = await EncryptFileContentAsync(fileContent);
            
            // Guardar archivo encriptado
            await File.WriteAllBytesAsync(filePath, encryptedContent);
            
            // Registrar acceso en auditoría
            await LogFileAccessAsync(userId, fileName, "SAVE", filePath);
            
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving secure file: {FileName}", fileName);
            throw;
        }
    }
    
    public async Task<byte[]> LoadSecureFileAsync(string secureFileName, string userId)
    {
        try
        {
            var filePath = Path.Combine(_secureDirectory, userId, secureFileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Secure file not found: {secureFileName}");
            }
            
            // Verificar permisos de acceso
            if (!await HasFileAccessAsync(userId, secureFileName))
            {
                throw new UnauthorizedAccessException($"Access denied to file: {secureFileName}");
            }
            
            // Leer archivo encriptado
            var encryptedContent = await File.ReadAllBytesAsync(filePath);
            
            // Desencriptar contenido
            var decryptedContent = await DecryptFileContentAsync(encryptedContent);
            
            // Registrar acceso en auditoría
            await LogFileAccessAsync(userId, secureFileName, "LOAD", filePath);
            
            return decryptedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading secure file: {FileName}", secureFileName);
            throw;
        }
    }
}
```

---

## 🏗️ PROCESO DE COMPILACIÓN INDRA

### Responsabilidades de INDRA

#### Checklist de Compilación INDRA

| **Fase** | **Responsabilidad** | **Verificación** | **Estado** |
|-----------|---------------------|------------------|------------|
| **Preparación** | Verificar código fuente | Git status limpio | ⏳ Pendiente |
| **Dependencias** | Restaurar paquetes NuGet | packages.config actualizado | ⏳ Pendiente |
| **Compilación** | Build en modo Release | Sin errores de compilación | ⏳ Pendiente |
| **Empaquetado** | Crear paquete de deployment | Archivos críticos incluidos | ⏳ Pendiente |
| **Validación** | Verificar integridad | Checksums verificados | ⏳ Pendiente |

#### Script de Compilación INDRA

```powershell
# indra-build-script.ps1
# Script específico para el proceso de compilación de INDRA

param(
    [string]$BuildNumber = "INDRA-$(Get-Date -Format 'yyyyMMdd-HHmmss')",
    [string]$Environment = "PROD",
    [switch]$SkipTests = $false,
    [switch]$CreatePackage = $true
)

Write-Host "=== INDRA BUILD PROCESS - BCN Module ===" -ForegroundColor Green
Write-Host "Build Number: $BuildNumber" -ForegroundColor Yellow
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan

# 1. VERIFICACIÓN PREVIA
Write-Host "`n1. VERIFICACIÓN PREVIA" -ForegroundColor Magenta
Write-Host "Verificando estado del repositorio..." -ForegroundColor Cyan

# Verificar que no hay cambios pendientes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "❌ ERROR: Hay cambios pendientes en el repositorio" -ForegroundColor Red
    Write-Host "Cambios pendientes:" -ForegroundColor Red
    Write-Host $gitStatus -ForegroundColor Red
    exit 1
}

# Verificar que estamos en la rama correcta
$currentBranch = git branch --show-current
if ($currentBranch -ne "main" -and $currentBranch -ne "master") {
    Write-Host "⚠️ ADVERTENCIA: No estás en la rama principal (actual: $currentBranch)" -ForegroundColor Yellow
    $continue = Read-Host "¿Continuar? (s/N)"
    if ($continue -ne "s" -and $continue -ne "S") {
        exit 0
    }
}

# 2. LIMPIEZA Y PREPARACIÓN
Write-Host "`n2. LIMPIEZA Y PREPARACIÓN" -ForegroundColor Magenta

# Limpiar directorios de build
$buildDirs = @("bin", "obj", "Publish", "Packages")
foreach ($dir in $buildDirs) {
    if (Test-Path $dir) {
        Write-Host "Limpiando $dir..." -ForegroundColor Cyan
        Remove-Item $dir -Recurse -Force
    }
}

# 3. RESTAURACIÓN DE DEPENDENCIAS
Write-Host "`n3. RESTAURACIÓN DE DEPENDENCIAS" -ForegroundColor Magenta

Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Cyan
nuget restore "DotNetBcnModule.Presentation.sln"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERROR: Fallo en la restauración de paquetes NuGet" -ForegroundColor Red
    exit 1
}

# 4. COMPILACIÓN
Write-Host "`n4. COMPILACIÓN" -ForegroundColor Magenta

Write-Host "Compilando solución en modo Release..." -ForegroundColor Cyan
msbuild "DotNetBcnModule.Presentation.sln" /p:Configuration=Release /p:Platform=AnyCPU /p:BuildNumber=$BuildNumber /verbosity:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ERROR: Fallo en la compilación" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Compilación exitosa" -ForegroundColor Green


# 5. EMPAQUETADO
if ($CreatePackage) {
    Write-Host "`n6. EMPAQUETADO" -ForegroundColor Magenta
    
    $packageDir = "Packages\BCN_Module_$BuildNumber"
    New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
    
    Write-Host "Creando paquete de deployment..." -ForegroundColor Cyan
    
    # Copiar archivos compilados
    Copy-Item "bin\Release\*" -Destination "$packageDir\bin\" -Recurse -Force
    
    # Copiar archivos de configuración
    Copy-Item "Web.config" -Destination $packageDir -Force
    Copy-Item "Global.asax" -Destination $packageDir -Force
    Copy-Item "nlog.config" -Destination $packageDir -Force
    
    # Copiar directorios de contenido
    Copy-Item "Content" -Destination $packageDir -Recurse -Force
    Copy-Item "Scripts" -Destination $packageDir -Recurse -Force
    Copy-Item "Views" -Destination $packageDir -Recurse -Force
    
    # Crear archivo de metadatos
    $metadata = @{
        BuildNumber = $BuildNumber
        BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Environment = $Environment
        SourceBranch = $currentBranch
        CommitHash = git rev-parse HEAD
        BuildMachine = $env:COMPUTERNAME
        BuildUser = $env:USERNAME
    }
    
    $metadata | ConvertTo-Json | Out-File "$packageDir\build-metadata.json" -Encoding UTF8
    
    # Crear archivo ZIP del paquete
    $zipPath = "Packages\BCN_Module_$BuildNumber.zip"
    Compress-Archive -Path $packageDir -DestinationPath $zipPath -Force
    
    Write-Host "✅ Paquete creado: $zipPath" -ForegroundColor Green
}

# 7. VALIDACIÓN FINAL
Write-Host "`n7. VALIDACIÓN FINAL" -ForegroundColor Magenta

Write-Host "Verificando archivos críticos..." -ForegroundColor Cyan
$criticalFiles = @("bin\DotNetBcnModule.Presentation.dll", "Web.config", "Global.asax")
$allFilesPresent = $true

foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - FALTANTE!" -ForegroundColor Red
        $allFilesPresent = $false
    }
}

if (-not $allFilesPresent) {
    Write-Host "❌ ERROR: Faltan archivos críticos" -ForegroundColor Red
    exit 1
}

# 8. REPORTE FINAL
Write-Host "`n=== BUILD COMPLETADO EXITOSAMENTE ===" -ForegroundColor Green
Write-Host "Build Number: $BuildNumber" -ForegroundColor Yellow
Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Cyan
Write-Host "Estado: ✅ EXITOSO" -ForegroundColor Green

if ($CreatePackage) {
    Write-Host "Paquete disponible en: Packages\" -ForegroundColor Cyan
}

Write-Host "`nProceso de compilación INDRA completado." -ForegroundColor Green
```

#### Configuración de Build INDRA

```xml
<!-- .csproj - Configuración específica para INDRA -->
<PropertyGroup>
  <!-- Configuración de Build INDRA -->
  <BuildNumber Condition=" '$(BuildNumber)' == '' ">INDRA-$(Date:yyyyMMdd-HHmmss)</BuildNumber>
  <BuildEnvironment Condition=" '$(BuildEnvironment)' == '' ">PROD</BuildEnvironment>
  
  <!-- Configuración de Assembly Info -->
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <InformationalVersion>1.0.0-INDRA-$(BuildNumber)</InformationalVersion>
  
  <!-- Configuración de Build -->
  <Optimize>true</Optimize>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  
  <!-- Configuración de Output -->
  <OutputPath>bin\Release\</OutputPath>
  <OutputType>Library</OutputType>
</PropertyGroup>

<!-- Configuración específica para Release -->
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
  <DefineConstants>TRACE;INDRA_BUILD;PRODUCTION</DefineConstants>
  <ErrorReport>prompt</ErrorReport>
  <WarningLevel>4</WarningLevel>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

---

## 🆕 NOVEDADES DE CONSOLIDACIÓN

### Funcionalidades Nuevas en Consolidación

#### Nuevas Opciones de Consolidación

| **Opción** | **Nueva Funcionalidad** | **Descripción** | **Estado** |
|------------|-------------------------|-----------------|------------|
| **BCN: Balance ALMACEN** | ✅ NUEVA | Balance consolidado por almacén | 🟢 Activa |
| **BCN: Balance POOL** | ✅ NUEVA | Balance consolidado por pool | 🟢 Activa |
| **BCN: Balance UNIDAD DE PROCESO** | ✅ NUEVA | Balance por unidad de proceso | 🟢 Activa |
| **BCN: Aplicar Regla de Balance** | ✅ NUEVA | Aplicación automática de reglas | 🟡 Beta |
| **BCN: Diferencia Balance** | ✅ NUEVA | Cálculo de diferencias | 🟡 Beta |

#### Mejoras en Procesamiento

```csharp
public class EnhancedConsolidationService : IConsolidationService
{
    // NUEVA: Procesamiento por lotes mejorado
    public async Task<ConsolidationResult> ProcessBatchAsync(List<ConsolidationRequest> requests)
    {
        var result = new ConsolidationResult();
        
        // NUEVA: Procesamiento paralelo
        var tasks = requests.Select(async request => 
        {
            try
            {
                return await ProcessSingleRequestAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request: {RequestId}", request.Id);
                return new ProcessedRequest { Success = false, Error = ex.Message };
            }
        });
        
        var results = await Task.WhenAll(tasks);
        
        // NUEVA: Agregación inteligente de resultados
        result.SuccessCount = results.Count(r => r.Success);
        result.FailureCount = results.Count(r => !r.Success);
        result.ProcessedRequests = results.ToList();
        
        return result;
    }
    
    // NUEVA: Reglas de balance automáticas
    public async Task<BalanceRuleResult> ApplyBalanceRulesAsync(BalanceData data)
    {
        var rules = await LoadBalanceRulesAsync();
        var result = new BalanceRuleResult();
        
        foreach (var rule in rules)
        {
            if (rule.IsEnabled && rule.Matches(data))
            {
                var ruleResult = await ExecuteRuleAsync(rule, data);
                result.AppliedRules.Add(ruleResult);
            }
        }
        
        return result;
    }
    
    // NUEVA: Cálculo de diferencias inteligente
    public async Task<DifferenceCalculation> CalculateDifferencesAsync(BalanceData current, BalanceData previous)
    {
        var differences = new DifferenceCalculation
        {
            Timestamp = DateTime.UtcNow,
            Differences = new List<DataDifference>()
        };
        
        // NUEVA: Algoritmo de comparación mejorado
        var comparison = await CompareDataSetsAsync(current, previous);
        
        foreach (var diff in comparison.Differences)
        {
            if (Math.Abs(diff.PercentageChange) > 5.0m) // NUEVA: Umbral configurable
            {
                differences.SignificantDifferences.Add(diff);
            }
            
            differences.Differences.Add(diff);
        }
        
        return differences;
    }
}


## 🛡️ CONFIGURACIÓN DE ANTIVIRUS

### Configuración de Excepciones de Antivirus

#### Carpetas y Archivos a Excluir

| **Tipo** | **Ruta** | **Razón** | **Prioridad** |
|-----------|----------|-----------|---------------|
| **Carpeta de Logs** | `C:\inetpub\wwwroot\BCN_Module\logs\` | Evitar falsos positivos | 🔴 ALTA |
| **Carpeta de Temp** | `C:\inetpub\wwwroot\BCN_Module\temp\` | Archivos temporales | 🔴 ALTA |
| **Carpeta de Cache** | `C:\inetpub\wwwroot\BCN_Module\cache\` | Cache del sistema | 🟡 MEDIA |
| **Carpeta de Uploads** | `C:\inetpub\wwwroot\BCN_Module\uploads\` | Archivos subidos | 🟡 MEDIA |
| **Archivos de Config** | `*.config` | Configuración del sistema | 🔴 ALTA |
| **Archivos de Log** | `*.log` | Logs del sistema | 🔴 ALTA |

#### Configuración para Windows Defender

```powershell
# Script de configuración de Windows Defender para BCN Module
# Ejecutar como Administrador

$modulePath = "C:\inetpub\wwwroot\BCN_Module"
$exclusions = @(
    "$modulePath\logs\*",
    "$modulePath\temp\*",
    "$modulePath\cache\*",
    "$modulePath\uploads\*",
    "$modulePath\*.config",
    "$modulePath\*.log"
)

Write-Host "=== Configuración de Windows Defender para BCN Module ===" -ForegroundColor Green

# Agregar exclusiones de carpeta
foreach ($exclusion in $exclusions) {
    try {
        Add-MpPreference -ExclusionPath $exclusion
        Write-Host "✓ Exclusión agregada: $exclusion" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Error agregando exclusión: $exclusion" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Verificar exclusiones
Write-Host "`nExclusiones configuradas:" -ForegroundColor Cyan
Get-MpPreference | Select-Object -ExpandProperty ExclusionPath

Write-Host "`nConfiguración completada." -ForegroundColor Green
```

#### Configuración para Antivirus Corporativos

```xml
<!-- Configuración para Symantec Endpoint Protection -->
<Configuration>
  <Antivirus>
    <Exclusions>
      <Folder>
        <Path>C:\inetpub\wwwroot\BCN_Module\logs</Path>
        <Reason>System logs - false positive prevention</Reason>
      </Folder>
      <Folder>
        <Path>C:\inetpub\wwwroot\BCN_Module\temp</Path>
        <Reason>Temporary files - system operation</Reason>
      </Folder>
      <Folder>
        <Path>C:\inetpub\wwwroot\BCN_Module\cache</Path>
        <Reason>System cache - performance optimization</Reason>
      </Folder>
      <FileType>
        <Extension>config</Extension>
        <Reason>Configuration files - system operation</Reason>
      </FileType>
      <FileType>
        <Extension>log</Extension>
        <Reason>Log files - system monitoring</Reason>
      </FileType>
    </Exclusions>
    
    <RealTimeProtection>
      <ScanOnAccess>true</ScanOnAccess>
      <ScanOnWrite>true</ScanOnWrite>
      <ScanOnRead>false</ScanOnRead>
    </RealTimeProtection>
    
    <ScheduledScans>
      <DailyScan>
        <Time>02:00</Time>
        <ExcludeBCNModule>true</ExcludeBCNModule>
      </DailyScan>
    </ScheduledScans>
  </Antivirus>
</Configuration>
```

#### Configuración de Monitoreo

```csharp
public class AntivirusMonitoringService : IAntivirusMonitoringService
{
    private readonly ILogger<AntivirusMonitoringService> _logger;
    private readonly string _modulePath;
    
    public async Task<AntivirusStatus> CheckAntivirusStatusAsync()
    {
        var status = new AntivirusStatus
        {
            Timestamp = DateTime.UtcNow,
            Exclusions = new List<AntivirusExclusion>(),
            Issues = new List<AntivirusIssue>()
        };
        
        try
        {
            // Verificar exclusiones configuradas
            await CheckExclusionsAsync(status);
            
            // Verificar archivos bloqueados
            await CheckBlockedFilesAsync(status);
            
            // Verificar rendimiento del sistema
            await CheckPerformanceImpactAsync(status);
            
            _logger.LogInformation("Antivirus status check completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking antivirus status");
            status.HasErrors = true;
            status.ErrorMessage = ex.Message;
        }
        
        return status;
    }
    
    private async Task CheckExclusionsAsync(AntivirusStatus status)
    {
        var requiredExclusions = new[]
        {
            Path.Combine(_modulePath, "logs"),
            Path.Combine(_modulePath, "temp"),
            Path.Combine(_modulePath, "cache"),
            Path.Combine(_modulePath, "uploads")
        };
        
        foreach (var exclusion in requiredExclusions)
        {
            var exclusionStatus = await VerifyExclusionAsync(exclusion);
            status.Exclusions.Add(exclusionStatus);
            
            if (!exclusionStatus.IsExcluded)
            {
                status.Issues.Add(new AntivirusIssue
                {
                    Type = "MissingExclusion",
                    Path = exclusion,
                    Severity = "High",
                    Description = "Required antivirus exclusion not configured"
                });
            }
        }
    }
    
    private async Task<AntivirusExclusion> VerifyExclusionAsync(string path)
    {
        // Implementar verificación específica del antivirus
        // Esto dependerá del antivirus corporativo utilizado
        
        return new AntivirusExclusion
        {
            Path = path,
            IsExcluded = true, // Placeholder
            LastVerified = DateTime.UtcNow
        };
    }
}
```

#### Procedimiento de Resolución de Problemas

| **Problema** | **Síntoma** | **Solución** | **Prioridad** |
|--------------|--------------|---------------|---------------|
| **Archivos bloqueados** | Error "Access denied" | Verificar exclusiones de antivirus | 🔴 ALTA |
| **Rendimiento lento** | Operaciones muy lentas | Excluir carpetas de cache/temp | 🟡 MEDIA |
| **Falsos positivos** | Archivos marcados como malware | Revisar exclusiones de logs | 🔴 ALTA |
| **Escaneo en tiempo real** | Interrupciones frecuentes | Configurar exclusiones específicas | 🟡 MEDIA |

