using System;

namespace Common
{
    public static class Logger
    {
        private enum ELogLevel
        {
            Info,
            Warning,
            Error
        }
        
        public static void LogInfo(string message)
        {
            Log(ELogLevel.Info, message);
        }
        
        public static void LogWarning(string message)
        {
            Log(ELogLevel.Warning, message);
        }
        
        public static void LogError(string message)
        {
            Log(ELogLevel.Error, message);
        }
        
        private static void Log(ELogLevel level, string message)
        {
            switch (level)
            {
                case ELogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case ELogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case ELogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            //add timestamp
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.WriteLine($"[{timestamp}] {message}");
            Console.ResetColor();
        }
    }
}