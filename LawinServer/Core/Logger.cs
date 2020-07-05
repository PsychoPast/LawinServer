using System;

namespace LawinServer.Core
{
    internal static class Logger
    {
        public static void LogError(string content)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(content);
            Console.ForegroundColor = oldColor;
        }
    }
}