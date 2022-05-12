using UnityEngine;

namespace SolFrame.Utils
{
    public static class LoggingExtensions
    {
        public static void LogObj(this Object obj, object message, LogType logType = LogType.Log)
        {
            if (!Debug.unityLogger.logEnabled) return;
            var netPrefix = string.Empty;

            netPrefix += $"<i><b>{obj.GetType().Name}</b></i>";
            if (obj is MonoBehaviour mono)
            {
                netPrefix += $"<i><b> on {mono.gameObject.name}</b></i>";
            }
            var colorPrefix = logType is LogType.Log ? "#4ac76b" : logType is LogType.Warning ? "#ffe366" : logType is LogType.Error ? "#d45d5d" : "#ffffff";
            netPrefix += $" => <color={colorPrefix}>{message}</color>";
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(netPrefix);
                    break;

                case LogType.Warning:
                    Debug.LogWarning(netPrefix);
                    break;

                case LogType.Error:
                    Debug.LogError(netPrefix);
                    break;
            }
        }

        public static void LogObj(this Object obj, object message, ref bool enabled, LogType logType = LogType.Log)
        {
            if (!enabled) return;
            LogObj(obj, message, logType);
        }
    }
}