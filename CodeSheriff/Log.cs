using DSharpPlus;
using System;

namespace CodeSheriff
{
    public static class Log
    {
        public static void WriteLogMessage(string message, LogLevel level)
        {
            string dateString = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz");
#if DEBUG
            if (level == LogLevel.Debug)
            {
                Console.WriteLine($"[{dateString}] [Log] [Debug] {message}");
                return;
            }
#endif

            switch (level)
            {
                case LogLevel.Info:
                    WriteColor($"[{dateString}] [Log] [Info] {message}", ConsoleColor.White);
                    break;
                case LogLevel.Warning:
                    WriteColor($"[{dateString}] [Log] [Warning] {message}", ConsoleColor.Yellow);
                    break;
                case LogLevel.Error:
                    WriteColor($"[{dateString}] [Log] [Error] {message}", ConsoleColor.Red);
                    break;
                case LogLevel.Critical:
                    WriteColor($"[{dateString}] [Log] [Critical] {message}", ConsoleColor.Red);
                    break;
                default:
                    break;
            }
        }

        private static void WriteColor(string message, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }

    public enum LogOutputLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4,
        Good = 5
    }
}
