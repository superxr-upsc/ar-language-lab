using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace CodeBase.Common.LoggerService
{
    public static class GameLogger
    {
        private static readonly StringBuilder _stringBuilder;

        static GameLogger() => 
            _stringBuilder = new StringBuilder();
        
        [Conditional("DEBUG_LOGS_ENABLED")]
        public static void Log(string message, Dictionary<string, object> parameters = null) => 
            Debug.Log($"[INFO] {ConstructMessage(message, parameters)}");

        [Conditional("DEBUG_LOGS_ENABLED")]
        public static void LogWarning(string message, Dictionary<string, object> parameters = null) => 
            Debug.LogWarning($"[WARN] {ConstructMessage(message, parameters)}");

        [Conditional("DEBUG_LOGS_ENABLED")]
        public static void LogError(string message, Dictionary<string, object> parameters = null) => 
            Debug.LogError($"[ERROR] {ConstructMessage(message, parameters)}");

        private static string ConstructMessage(string message, Dictionary<string, object> parameters)
        {
            _stringBuilder.Clear();
            _stringBuilder.AppendLine(message);

            if (parameters == null)
                return _stringBuilder.ToString();

            foreach (var parameter in parameters) 
                _stringBuilder.AppendLine($"[{parameter.Key}] - {parameter.Value}");

            return _stringBuilder.ToString();
        }
    }
}