using NetBcnModule.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace DotNetBcnModule.Services.Contracts
{
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Creates a database connection for the specified target
        /// </summary>
        /// <param name="target">Database target to connect to</param>
        /// <returns>Database connection object</returns>
        SqlConnection CreateDataBase(DatabaseTarget target);

        /// <summary>
        /// Tests the database connection
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Gets connection information
        /// </summary>
        string GetConnectionInfo();

        /// <summary>
        /// Executes a query and returns the results asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the results to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of mapped objects</returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a command and returns the number of affected rows
        /// </summary>
        /// <param name="sql">The SQL command to execute</param>
        /// <param name="parameters">The parameters for the command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of affected rows</returns>
        Task<int> ExecuteAsync(string sql, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns a single result asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the result to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A single mapped object or null</returns>
        Task<T> QuerySingleAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query and returns the first result asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the result to</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">The parameters for the query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The first mapped object or null</returns>
        Task<T> QueryFirstAsync<T>(string sql, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a stored procedure and returns the results asynchronously
        /// </summary>
        /// <typeparam name="T">The type to map the results to</typeparam>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="parameters">The parameters for the stored procedure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of mapped objects</returns>
        Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string procedureName, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a stored procedure and returns the number of affected rows
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="parameters">The parameters for the stored procedure</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of affected rows</returns>
        Task<int> ExecuteStoredProcedureAsync(string procedureName, object parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the underlying SqlConnection instance
        /// </summary>
        object GetSqlConnection();
    }
} 