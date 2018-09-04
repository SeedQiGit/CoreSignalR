using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;


namespace FeiCheSignalR.Infrastructure.Helper
{
    public class LogHelper
    {
        private static ILogger _logger;

        static LogHelper()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            //这里会输出到Nlog
            loggerFactory.AddNLog();
            _logger = loggerFactory.CreateLogger(nameof(LogHelper));
        }

        public static void Info(string msg)
        {
            _logger.LogInformation(msg);
        }

        public static void Error(string msg)
        {
            _logger.LogError(msg);
        }

        public static void Warning(string msg)
        {
            _logger.LogWarning(msg);
        }
        public static void Trace(string msg)
        {
            _logger.LogTrace(msg);
        }

        public static void Error(Exception ex)
        {
            Error("发生异常:" + ex.Source + Environment.NewLine + ex.StackTrace + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
        }
    }
}
