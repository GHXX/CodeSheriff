using DSharpPlus;
using System;

namespace CodeSheriff
{
    public class Log
    {
        public void WriteLogMessage(string message, LogLevel level)
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

        private void WriteColor(string message, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
