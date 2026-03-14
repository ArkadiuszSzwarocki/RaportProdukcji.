using System;
using System.Collections.Generic;
using System.Linq;

namespace RaportProdukcji.Services
{
    public interface ILoggerService
    {
        void LogMessage(string type, string message, Dictionary<string, object> details = null);
        void LogError(string message, Exception ex = null);
        void LogInfo(string message, Dictionary<string, object> details = null);
        void LogWarning(string message, Dictionary<string, object> details = null);
        List<LogEntry> GetLogs(string type = null);
        void ClearLogs();
        Dictionary<string, int> GetStats();
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
        public Dictionary<string, object>? Details { get; set; }
    }

    public class LoggerService : ILoggerService
    {
        private readonly List<LogEntry> _logs = new();
        private readonly int _maxLogs = 1000;
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }

        public void LogMessage(string type, string message, Dictionary<string, object>? details = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Type = type,
                Message = message,
                Details = details ?? new()
            };

            _logs.Add(entry);

            if (_logs.Count > _maxLogs)
            {
                _logs.RemoveAt(0);
            }

            // Log to application logger too
            _logger.LogInformation("[{Type}] {Message}", type, message);
        }

        public void LogError(string message, Exception ex = null)
        {
            var details = new Dictionary<string, object>();
            if (ex != null)
            {
                details["exception"] = ex.GetType().Name;
                details["stackTrace"] = ex.StackTrace;
                details["innerException"] = ex.InnerException?.Message;
            }

            LogMessage("error", message, details);
            _logger.LogError(ex, "[ERROR] {Message}", message);
        }

        public void LogInfo(string message, Dictionary<string, object>? details = null)
        {
            LogMessage("info", message, details);
        }

        public void LogWarning(string message, Dictionary<string, object>? details = null)
        {
            LogMessage("warning", message, details);
            _logger.LogWarning("[WARNING] {Message}", message);
        }

        public List<LogEntry> GetLogs(string? type = null)
        {
            if (string.IsNullOrEmpty(type))
            {
                return _logs.ToList();
            }

            return _logs.Where(l => l.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public void ClearLogs()
        {
            _logs.Clear();
            _logger.LogInformation("Logs cleared");
        }

        public Dictionary<string, int> GetStats()
        {
            return _logs
                .GroupBy(l => l.Type)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
