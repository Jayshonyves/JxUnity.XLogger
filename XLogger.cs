using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class XLogger
{
    private static XLogHandler s_logger = new XLogHandler();
    public static ILogger logger { get => s_logger; }

    public static bool IsOutEditor { get => s_logger.IsOutEditor; }
    public static bool IsOutFile { get => s_logger.IsOutFile; }

    public static string SaveFolder { get => s_logger.SaveFolder; }
    public static string SaveFileFullName { get => s_logger.SaveFileFullName; }
    public static string SaveFileName { get => s_logger.SaveFileName; }

    public static void Init_SetOutFile(string folder, int cacheSize = 5)
    {
        s_logger.Init_SetOutFile(folder, cacheSize);
    }
    public static void Init_SetOutEditor()
    {
        s_logger.Init_SetEditorConsole();
    }
    public static void Init_Done()
    {
        s_logger.Init_Done();
    }
    public static void Close()
    {
        s_logger.Dispose();
    }

    public static void Log(object message) => s_logger.Log(message);
    public static void LogFormat(string format, params object[] args) => s_logger.LogFormat(LogType.Log, format, args);
    public static void LogError(object message) => s_logger.LogError(message);
    public static void LogWarning(object message) => s_logger.LogWarning(message);

    public class XLogHandler : ILogger, ILogHandler, IDisposable
    {
        public bool IsOutEditor { get; set; } = false;
        public bool IsOutFile { get; set; } = false;

        public string SaveFolder { get; private set; }
        public string SaveFileFullName { get; private set; }
        public string SaveFileName { get; private set; }
        public int CacheSize { get; private set; }
        private int cacheCount = 0;

        private Stream stream = null;
        private StreamWriter sw = null;

        public ILogHandler logHandler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Init_SetOutFile(string folder, int cacheSize)
        {
            this.IsOutFile = true;

            this.SaveFolder = folder;
            if (!Directory.Exists(SaveFolder))
                Directory.CreateDirectory(SaveFolder);

            this.SaveFileName = string.Format("{0}.log", DateTime.Now.ToString("yyyyMMddhhmmss"));
            this.SaveFileFullName = Path.Combine(SaveFolder, SaveFileName);
            this.CacheSize = cacheSize;
            this.stream = File.OpenWrite(SaveFileFullName);
            this.sw = new StreamWriter(this.stream);
        }
        public void Init_SetEditorConsole()
        {
            this.IsOutEditor = true;
        }
        public void Init_Done()
        {
            Application.quitting += Application_quitting;
        }

        private void Application_quitting()
        {
            this.Dispose();
        }

        public bool IsLogTypeAllowed(LogType logType)
        {
            if (logType == LogType.Exception)
            {
                return false;
            }
            return true;
        }

        private string GetCurTime() => DateTime.Now.ToString("hh:mm:ss");


        private void Write(string msg)
        {
            if (!this.IsOutFile)
            {
                return;
            }

            this.cacheCount++;
            this.sw.WriteLine(msg);
            if (this.cacheCount >= this.CacheSize)
            {
                this.cacheCount = 0;
                this.sw.Flush();
            }
        }

        private void Internel_Log(LogType logType, object msg)
        {
            string logContent = null;
            switch (logType)
            {
                case LogType.Error:
                    if (this.IsOutEditor)
                        Debug.LogError(msg);
                    logContent = string.Format("[{0}] ERROR: {1}\n{2}",
                        GetCurTime(),
                        msg,
                        (new StackTrace(2)).ToString());
                    break;
                case LogType.Warning:
                    if (this.IsOutEditor)
                        Debug.LogWarning(msg);
                    logContent = string.Format("[{0}] WARNING: {1}", GetCurTime(), msg);
                    break;
                case LogType.Log:
                    if (this.IsOutEditor)
                        Debug.Log(msg);
                    logContent = string.Format("[{0}] INFO: {1}", GetCurTime(), msg);
                    break;
                case LogType.Exception:
                    break;
                default:
                    break;
            }
            Write(logContent);
        }

        public void Log(LogType logType, object message)
        {
            Internel_Log(logType, message);
        }

        public void Log(LogType logType, object message, UnityEngine.Object context)
        {
            Internel_Log(logType, message);
        }

        public void Log(LogType logType, string tag, object message)
        {
            Internel_Log(logType, message);
        }

        public void Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            Internel_Log(logType, message);
        }

        public void Log(object message)
        {
            Internel_Log(LogType.Log, message);
        }

        public void Log(string tag, object message)
        {
            Internel_Log(LogType.Log, message);
        }

        public void Log(string tag, object message, UnityEngine.Object context)
        {
            Internel_Log(LogType.Log, message);
        }

        public void LogError(object message)
        {
            Internel_Log(LogType.Error, message);
        }

        public void LogError(string tag, object message)
        {
            Internel_Log(LogType.Error, message);
        }

        public void LogError(string tag, object message, UnityEngine.Object context)
        {
            Internel_Log(LogType.Error, message);
        }

        public void LogException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            throw new NotImplementedException();
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            Internel_Log(logType, string.Format(format, args));
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            Internel_Log(logType, string.Format(format, args));
        }
        public void LogWarning(object message)
        {
            Internel_Log(LogType.Warning, message);
        }
        public void LogWarning(string tag, object message)
        {
            Internel_Log(LogType.Warning, message);
        }

        public void LogWarning(string tag, object message, UnityEngine.Object context)
        {
            Internel_Log(LogType.Warning, message);
        }

        public void Dispose()
        {
            this.stream?.Close();
        }
    }
}
