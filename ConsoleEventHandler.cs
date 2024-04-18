namespace HAB_WebServer
{

    static class ConsoleEventHandler
    {
        // add event handler to console on key press
        public static void AddConsoleEventHandler()
        {
            Console.CancelKeyPress += Handle;
        }

        // Stop server and close http listener
        private static void Handle(object? sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nStopping Server...");
            BaseHttpServer.HttpListener?.Stop();
            Environment.Exit(Environment.ExitCode);
        }
    }
}