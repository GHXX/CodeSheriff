using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;

namespace CodeSheriff
{
    class DebugLogger
    {
        public void WriteDebugMessage(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public void WriteDebugLog(string message)
        {
            Log.WriteLogMessage(message, LogLevel.Debug);
        }
    }
}
