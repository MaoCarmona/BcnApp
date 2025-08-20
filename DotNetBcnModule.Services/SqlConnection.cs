using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Threading;

namespace NetBcnModule.Services
{
    /// <summary>
    /// Handles SQL Server database connections and operations
    /// </summary>
    public class SqlConnection : IDisposable
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the SqlConnection class
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        public SqlConnection()
        {
            // Retrieve connection string from configuration
            // This assumes the connection string is named "DBARES" in the configuration file
            var connSetting = ConfigurationManager.ConnectionStrings["DBARES"];
            if (connSetting == null || string.IsNullOrEmpty(connSetting.ConnectionString))
                throw new InvalidOperationException("Missing or invalid 'DefaultConnection' in configuration.");
            
            // Decode base64 if necessary
            _connectionString = DatabaseService.DecodeIfBase64(connSetting.ConnectionString);
        }

        /// <summary>
        /// Initializes a new instance of the SqlConnectionLegacy class with connection string
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        public SqlConnection(string connectionString)
        {
            _connectionString = EnsureTimeoutSettings(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    return false;
                }

                using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets connection string information
        /// </summary>
        public string GetConnectionInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    return "Connection string is null or empty";
                }

                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);
                return $"Server: {builder.DataSource}, Database: {builder.InitialCatalog}, Integrated Security: {builder.IntegratedSecurity}";
            }
            catch (Exception ex)
            {
                return $"Unable to parse connection string: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets the raw connection string (for debugging)
        /// </summary>
        public string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Validates the connection string format
        /// </summary>
        public bool IsConnectionStringValid()
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    return false;
                }

                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);
                return !string.IsNullOrEmpty(builder.DataSource) && !string.IsNullOrEmpty(builder.InitialCatalog);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ensures the connection string includes timeout settings for long-running queries
        /// </summary>
        /// <param name="connectionString">The original connection string</param>
        /// <returns>Connection string with timeout settings</returns>
        private string EnsureTimeoutSettings(string connectionString)
        {
            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                
                // Set connection timeout to 0 (no timeout) - will wait indefinitely
                if (!builder.ContainsKey("ConnectTimeout") || builder.ConnectTimeout != 0)
                {
                    builder.ConnectTimeout = 0;
                }
                
                return builder.ConnectionString;
            }
            catch
            {
                // If we can't parse the connection string, return it as-is
                return connectionString;
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                
                // Set command timeout to 0 (no timeout) - will wait indefinitely
                var command = new System.Data.SqlClient.SqlCommand(sql, connection);
                command.CommandTimeout = 0; // No timeout
                
                return await connection.QueryAsync<T>(sql, parameters, commandTimeout: 0, commandType: CommandType.Text);
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.ExecuteAsync(sql, parameters, commandTimeout: 0, commandType: CommandType.Text);
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, commandTimeout: 0, commandType: CommandType.Text);
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandTimeout: 0, commandType: CommandType.Text);
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure, commandTimeout: 0);
            }
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
            using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure, commandTimeout: 0);
            }
        }

        /// <summary>
        /// Disposes the SqlConnectionLegacy
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the SqlConnectionLegacy
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No managed resources to dispose in this case
                // The connection string is just a string, not a disposable resource
            }
        }
    }
}