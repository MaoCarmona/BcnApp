using DotNetBcnModule.Services.Contracts;
using System;
using System.IO;

namespace NetBcnModule.Services.Services
{
    /// <summary>
    /// Service to handle logging functionality
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly string _logPath;
        private readonly string _logFileName;

        public LoggingService(string logPath = null, string logFileName = "appBCN.log")
        {
            if (string.IsNullOrEmpty(logPath))
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "logs");
            }
            _logPath = logPath;
            _logFileName = logFileName;

            // Ensure log directory exists
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        /// <summary>
        /// Writes a message to the log file
        /// </summary>
        /// <param name="message">Message to log</param>
        public void WriteLog(string message)
        {
            try
            {
                var logFilePath = Path.Combine(_logPath, _logFileName);
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";

                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // If logging fails, we don't want to throw an exception
                // but we might want to write to console or another fallback
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes an info message to the log
        /// </summary>
        /// <param name="message">Info message</param>
        public void WriteInfo(string message)
        {
            WriteLog($"INFO: {message}");
        }

        /// <summary>
        /// Writes an error message to the log
        /// </summary>
        /// <param name="message">Error message</param>
        public void WriteError(string message)
        {
            WriteLog($"ERROR: {message}");
        }

        /// <summary>
        /// Writes a warning message to the log
        /// </summary>
        /// <param name="message">Warning message</param>
        public void WriteWarning(string message)
        {
            WriteLog($"WARNING: {message}");
        }

        /// <summary>
        /// Writes SQL query to the log
        /// </summary>
        /// <param name="sql">SQL query to log</param>
        public void WriteSql(string sql)
        {
            WriteLog($"SQL: {sql}");
        }
    }
}