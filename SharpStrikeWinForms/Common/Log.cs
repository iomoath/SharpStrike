
namespace SharpStrike
{
    public static class Log
    {
        private static readonly object FileLock = new object();
        private static readonly string DatetimeFormat = @"yyyy-MM-dd HH:mm:ss";


        /// <summary>
        /// Log a DEBUG message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="text">Message</param>
        public static void Error(System.Exception e, string text)
        {
            var msg = $"{e.GetType().Name} {text}\n{e.StackTrace}";
            WriteFormattedLog(LogLevel.ERROR, msg);
        }

        /// <summary>
        /// Log a FATAL ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an INFO message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Information(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        /// <summary>
        /// Log a TRACE message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a WARNING message
        /// </summary>
        /// <param name="text">Message</param>
        public static void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private static void WriteLine(string text, bool append = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }


            lock (FileLock)
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(Env.LogFilePath, append, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(text);
                }
            }
        }

        private static void WriteFormattedLog(LogLevel level, string text)
        {
            if (!SharedGlobals.LoggingEnabled)
                return;

            string pretext;
            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [TRACE]  ";
                    break;
                case LogLevel.INFO:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [INFO]   ";
                    break;
                case LogLevel.DEBUG:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [DEBUG]  ";
                    break;
                case LogLevel.WARNING:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [WARNING]  ";
                    break;
                case LogLevel.ERROR:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [ERROR]  ";
                    break;
                case LogLevel.FATAL:
                    pretext = System.DateTime.Now.ToString(DatetimeFormat) + " [FATAL]  ";
                    break;
                default:
                    pretext = "";
                    break;
            }

            WriteLine(pretext + text, true);
        }

        [System.Flags]
        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
    }
}