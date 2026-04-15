using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseCommon.Utilities
{
    public class LogHelper
    {
        private const string InfoTag = "I ; ";
        private const string DebugTag = "D ; ";
        private const string WarnTag = "W ; ";
        private const string ErrorTag = "E ; ";
        private const string VerboseTag = "V ; ";

        private static StreamWriter _logWriter;

        private static readonly object Mutex = new object();

        private static DateTime _date;

        private static void InitializeLogger()
        {
            var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var dateTag = DateTime.Now.ToString("yyyyMMdd");
            var logFileName = $"Log_{dateTag}.log";
            var logFilePath = Path.Combine(logFolder, logFileName);
            if (!File.Exists(logFileName))
            {
                _logWriter?.Dispose();
                var logFileStream = new FileStream(logFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                _logWriter = new StreamWriter(logFileStream);
            }
        }

        /// <summary>
        /// Write an info log into the file.
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLineInfo(string msg)
        {
            Write(LogLevel.Info, msg);
        }

        /// <summary>
        /// Write an error log into the file.
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLineError(string msg)
        {
            Write(LogLevel.Error, msg);
        }

        /// <summary>
        /// Write an warning log into the file.
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLineWarn(string msg)
        {
            Write(LogLevel.Warn, msg);
        }

        private static void Write(LogLevel level, string msg)
        {
            lock (Mutex)
            {
                if (DateTime.Today != _date)
                {
                    InitializeLogger();
                    _date = DateTime.Today;
                }
                try
                {
                    var now = DateTime.Now;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(
                        $"[{now:yyyyMMddHHmmss},{now.Millisecond:d3}]-");
                    string message = msg;
                    if (level == LogLevel.Error)
                    {
                        sb.Append(ErrorTag);
                    }
                    else if (level == LogLevel.Warn)
                    {
                        sb.Append(WarnTag);
                    }
                    else if (level == LogLevel.Info)
                    {
                        sb.Append(InfoTag);
                    }
                    else if (level == LogLevel.Verbose)
                    {
                        sb.Append(VerboseTag);
                    }
                    else if (level == LogLevel.Debug)
                    {
                        sb.Append(DebugTag);
                    }
                    sb.Append(message);
                    var logMessage = sb.ToString();
                    _logWriter.WriteLine(logMessage);
                    _logWriter.Flush();
#if DEBUG
                    var oldColor = Console.ForegroundColor;
                    if (level == LogLevel.Info)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    if (level == LogLevel.Error)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine(logMessage);
                    Console.ForegroundColor = oldColor;

#endif
                }
                catch (Exception)
                {
                    // There is exception during writing log, so just ignore
                }
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error,
        Verbose,
        Debug
    }
}
