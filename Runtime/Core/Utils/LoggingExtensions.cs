using UnityEngine;

namespace SolFrame.Utils
{
    public static class LoggingExtensions
    {
        /// <summary>
        ///   Format and log a specified message. Includes information about the <see cref="Object"/> logging
        ///   <para> <i> Runs on the main thread </i> </para>
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="message"> </param>
        /// <param name="logType"> </param>
        public static void LogObj(this Object obj, object message, LogType logType = LogType.Log)
        {
            MainThread.InUpdate(() =>
            {
                if (!Debug.unityLogger.logEnabled) return;
                var netPrefix = string.Empty;

                netPrefix += $"<i><b>{obj.GetType().Name}</b></i>";
                if (obj is MonoBehaviour mono)
                {
                    netPrefix += $"<i><b> on {mono.gameObject.name}</b></i>";
                }

                netPrefix += $" => <color={logType.GetColorPrefix()}>{message}</color>";

                switch (logType)
                {
                    case LogType.Log:
                        Debug.Log(netPrefix);
                        break;

                    case LogType.Assert:
                        Debug.Log(netPrefix);
                        break;

                    case LogType.Warning:
                        Debug.LogWarning(netPrefix);
                        break;

                    case LogType.Error:
                        Debug.LogError(netPrefix);
                        break;

                    case LogType.Exception:
                        Debug.LogError(netPrefix);
                        break;
                }
            });
        }

        /// <summary>
        ///   <inheritdoc cref="LogObj(Object, object, LogType)"/>
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="message"> </param>
        /// <param name="enabled"> </param>
        /// <param name="logType"> </param>
        public static void LogObj(this Object obj, object message, ref bool enabled, LogType logType = LogType.Log)
        {
            if (!enabled) return;
            LogObj(obj, message, logType);
        }

        private static string GetColorPrefix(this LogType type)
        {
            return type switch
            {
                LogType.Log => "#4ac76b",
                LogType.Warning => "#ffe366",
                LogType.Error => "#d45d5d",
                LogType.Assert => "#4dc6ff",
                LogType.Exception => "#e346b1",
                _ => "#ffffff"
            };
        }
    }
}