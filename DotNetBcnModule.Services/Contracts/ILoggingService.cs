namespace DotNetBcnModule.Services.Contracts
{
    public interface ILoggingService
    {
        /// <summary>
        /// Writes a message to the log file
        /// </summary>
        /// <param name="message">Message to log</param>
        void WriteLog(string message);

        /// <summary>
        /// Writes an info message to the log
        /// </summary>
        /// <param name="message">Info message</param>
        void WriteInfo(string message);

        /// <summary>
        /// Writes an error message to the log
        /// </summary>
        /// <param name="message">Error message</param>
        void WriteError(string message);

        /// <summary>
        /// Writes a warning message to the log
        /// </summary>
        /// <param name="message">Warning message</param>
        void WriteWarning(string message);

        /// <summary>
        /// Writes SQL query to the log
        /// </summary>
        /// <param name="sql">SQL query to log</param>
        void WriteSql(string sql);
    }
} 