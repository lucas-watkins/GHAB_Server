namespace HAB_WebServer
{
    public enum LoggingLevels
    {
        None,
        Normal,
        Verbose 
    }
    public class Logging
    {

        private static string _fileName = "";
        private readonly LoggingLevels _loggingLevels; 
        private static string FindLogFileName()
        {
            return "log" + Directory.GetFiles("logs").Length + ".txt"; 
        }

        public Logging(LoggingLevels loggingLevels)
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs"); 
            }
            
            // create log file if none and establish file name and logging level
            _fileName = FindLogFileName();
            _loggingLevels = loggingLevels;
            
        }
        private static void Log(string message)
        {
            // Write string to file path asynchronously and print to console, letting the method continue  
            Task.Run(() =>
            {
                var timeStamp = TimeZoneInfo.ConvertTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 
                    DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), TimeZoneInfo.Local);
                
                Console.WriteLine(timeStamp + " -- " + message); 
                File.AppendAllText("logs/" + _fileName, timeStamp + " -- " + message);
            });   
        }
        
        public void LogServerStart()
        {
            if (_loggingLevels >= LoggingLevels.Normal)
            {
                Log("Server Started On: " + BaseHttpServer.BaseUrl + ':' + BaseHttpServer.Port);
            }
        }

        public void LogIncomingConnection(string ip, string requestedPage, string method)
        {
            if (_loggingLevels >= LoggingLevels.Normal)
            {
                Log("Connection occurred from " + ip + " Requested: " + requestedPage + " Method: " + method);
            }
        }
    }
}