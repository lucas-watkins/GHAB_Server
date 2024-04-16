using System.Net;

namespace HAB_WebServer
{

    class ConsoleEventHandler
    {
        
        public static void AddConsoleEventHandler()
        {
            Console.CancelKeyPress += Handle;
        }

        // Stop server and close http listener
        private static void Handle(object? sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nStopping Server...");
            BaseHttpServer._httpListener?.Stop();
            Environment.Exit(Environment.ExitCode);
        }
    }
}