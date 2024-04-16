using System.Text;

namespace HAB_WebServer{
    class MainServer : BaseHttpServer{

        private int _imageNumber;
        
        private MainServer(){
            // add console event handler 
            ConsoleEventHandler.AddConsoleEventHandler(); 
            
            // checks if images directory exists and if not creates it 
            if (!Directory.Exists("images")){
                Directory.CreateDirectory("images");
            }

            // set image number to be right integer
            _imageNumber = Directory.GetFiles("images").Length; 

            // add context handlers here, supports GET and POST
            CtxHandlers = [new ContextHandler(SendOkStatus, "/", false),
                           new ContextHandler(SaveImage, "/submit", true)];
            Port = 8080;
            BaseUrl = "http://*";

        }

        public static async Task Main(string[] args)
        {
                MainServer s = new MainServer();
                await s.Serve();
        }

        // in a post request the byte array is passed as the received data from said request
        public string PrintBytesToConsole(byte[] bytes){
            Console.WriteLine(Encoding.UTF8.GetString(bytes)); 
            return ""; 
        }


        // returned string values in functions if the context handler is set to get are displayed on the web page
        // byte[] is needed to satisfy parameters of contextHandler.task
        private string SendOkStatus(byte[] bytes){
            return "OK 200";     
        }

        // save image submitted from http request
        private string SaveImage(byte[] bytes){
            File.WriteAllBytes("images/image" + _imageNumber.ToString() + ".jpg", bytes);
            _imageNumber++;  
            return "";
        }
    }
}