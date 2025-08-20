using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Threading;
using NetBcnModule.Services.Models;
using NetBcnModule.Services.Services;
using DotNetBcnModule.Services.Contracts;

namespace NetBcnModule.Services
{
    /// <summary>
    /// Service class that provides database operations using SqlConnection
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly SqlConnection _sqlConnection;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the DatabaseService class
        /// </summary>
        public DatabaseService(ILoggingService loggingService)
        {
            _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _sqlConnection = new SqlConnection();
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseService class with specific database target
        /// </summary>
        /// <param name="target">Database target to connect to</param>
        /// <param name="loggingService">Logging service</param>
        public DatabaseService(DatabaseTarget target, ILoggingService loggingService)
        {
            _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _sqlConnection = CreateDataBase(target);
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseService class with SqlConnection
        /// </summary>
        /// <param name="sqlConnection">The SqlConnection instance</param>
        /// <param name="loggingService">Logging service</param>
        public DatabaseService(SqlConnection sqlConnection, ILoggingService loggingService)
        {
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Creates a database connection for the specified target
        /// </summary>
        /// <param name="target">Database target to connect to</param>
        /// <returns>Database connection object</returns>
        public SqlConnection CreateDataBase(DatabaseTarget target)
        {
            string name = target.ToString();
            var setting = ConfigurationManager.ConnectionStrings[name];

            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new InvalidOperationException($"Connection string '{name}' not found or invalid.");

            string connectionString = setting.ConnectionString;

            if (IsValidSqlConnectionString(connectionString))
            {
                return new SqlConnection(connectionString);
            }

            var decodedConn = DecodeIfBase64(connectionString);
            
            // Validate the decoded string
            if (IsValidSqlConnectionString(decodedConn))
            {
                return new SqlConnection(decodedConn);
            }

            _logger.WriteError($"Invalid connection string format for '{name}'. Neither plain text nor base64 encoded.");

            throw new InvalidOperationException($"Invalid connection string format for '{name}'. Neither plain text nor base64 encoded.");
        }

        /// <summary>
        /// Validates if a string is a valid SQL connection string
        /// </summary>
        /// <param name="connectionString">Connection string to validate</param>
        /// <returns>True if valid SQL connection string, false otherwise</returns>
        private static bool IsValidSqlConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            var lowerConn = connectionString.ToLowerInvariant();
            
            bool hasServer = lowerConn.Contains("server=") || lowerConn.Contains("data source=");
            bool hasDatabase = lowerConn.Contains("database=") || lowerConn.Contains("initial catalog=");
            bool hasUserId = lowerConn.Contains("user id=") || lowerConn.Contains("uid=");
            bool hasPassword = lowerConn.Contains("password=") || lowerConn.Contains("pwd=");
            bool hasIntegratedSecurity = lowerConn.Contains("integrated security=") || lowerConn.Contains("trusted_connection=");

            return hasServer && (hasDatabase || hasIntegratedSecurity) && (hasUserId || hasPassword);
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            return await _sqlConnection.TestConnectionAsync();
        }

        /// <summary>
        /// Gets connection information
        /// </summary>
        public string GetConnectionInfo()
        {
            return _sqlConnection.GetConnectionInfo();
        }

        /// <summary>
        /// Executes a query and returns the results asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the results to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of mapped objects</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.QueryAsync<T>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a command and returns the number of affected rows
        /// </summary>
        /// <param name="sql">The SQL command to execute</param>
        /// <param name="parameters">The parameters for the command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of affected rows</returns>
        public async Task<int> ExecuteAsync(string sql, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.ExecuteAsync(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns a single result asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the result to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A single mapped object or null</returns>
        public async Task<T> QuerySingleAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.QuerySingleAsync<T>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a query and returns the first result asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the result to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The first mapped object or null</returns>
        public async Task<T> QueryFirstAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.QueryFirstAsync<T>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a stored procedure and returns the results asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the results to</typeparam>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="parameters">The parameters for the stored procedure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of mapped objects</returns>
        public async Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string procedureName, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.QueryStoredProcedureAsync<T>(procedureName, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a stored procedure and returns the number of affected rows
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="parameters">The parameters for the stored procedure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of affected rows</returns>
        public async Task<int> ExecuteStoredProcedureAsync(string procedureName, object parameters = null, CancellationToken cancellationToken = default)
        {
            return await _sqlConnection.ExecuteStoredProcedureAsync(procedureName, parameters, cancellationToken);
        }

        /// <summary>
        /// Gets the underlying SqlConnection instance
        /// </summary>
        public object GetSqlConnection()
        {
            return _sqlConnection;
        }

        /// <summary>
        /// Decodes base64 encoded connection strings
        /// </summary>
        /// <param name="encoded">Encoded connection string</param>
        /// <returns>Decoded connection string</returns>
        public static string DecodeIfBase64(string encoded)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encoded);
                string decoded = System.Text.Encoding.UTF8.GetString(data);
                if (decoded.Contains(";"))
                    return decoded;
            }
            catch
            {
                // Log error if logger is available
                Console.WriteLine($"Failed to decode connection string: {encoded}. It may not be a valid Base64 string.");
            }
            return encoded;
        }

        /// <summary>
        /// Disposes the DatabaseService and its underlying SqlConnection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the DatabaseService and its underlying SqlConnection
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sqlConnection?.Dispose();
            }
        }
    }

    public enum DatabaseTarget
    {
        DBBCN,
        DBARES,
        DBAROMSS
    }
}