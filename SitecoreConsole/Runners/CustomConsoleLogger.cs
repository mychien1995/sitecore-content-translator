using System;

namespace SitecoreConsole.Runners
{
    public class CustomConsoleLogger
    {
        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}
