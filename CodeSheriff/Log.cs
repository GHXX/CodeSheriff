using System;
using System.Collections.Generic;
using System.Text;

namespace CodeSheriff
{
    public class Log
    {
        public void WriteLogMessage(string message, LogOutputLevel level)
        {
#if DEBUG
            if (level == LogOutputLevel.Debug)
            {
                Console.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz")}] [Log] [Debug] {message}");
                return;
            }
#endif
            string dateString = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz");

            switch (level)
            {
                case LogOutputLevel.Info:
                    WriteColor($"[{dateString}] [Log] [Info] {message}", ConsoleColor.White);
                    break;
                case LogOutputLevel.Warning:
                    WriteColor($"[{dateString}] [Log] [Warning] {message}", ConsoleColor.Yellow);
                    break;
                case LogOutputLevel.Error:
                    WriteColor($"[{dateString}] [Log] [Error] {message}", ConsoleColor.Red);
                    break;
                case LogOutputLevel.Critical:
                    WriteColor($"[{dateString}] [Log] [Critical] {message}", ConsoleColor.Red);
                    break;
                case LogOutputLevel.Good:
                    WriteColor($"[{dateString}] [Log] [Good] {message}", ConsoleColor.Green);
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
