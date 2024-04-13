using System.Data.SqlTypes;
using System.Text;

namespace Server{
    class mainServer : BaseHttpServer{

        public int imageNumber;
        public mainServer(){
            // checks if images directory exists and if not creates it 
            if (!Directory.Exists("images")){
                Directory.CreateDirectory("images");
            }

            // set image number to be right integer
            imageNumber = Directory.GetFiles("images").Length; 

            // add context handlers here, supports GET and POST
            ctxHandlers = [new contextHandler(sendOKStatus, "/", false),
                           new contextHandler(saveImage, "/submit", true)];
            
        }

        public static async Task Main(string[] args) {
            mainServer s = new mainServer();
            await s.Serve();   
        }

        // in a post request the byte array is passed as the recieved data from said request
        public string printBytesToConsole(byte[] bytes){
            Console.WriteLine(Encoding.UTF8.GetString(bytes)); 
            return ""; 
        }


        // returned string values in functions if the context handler is set to get are displayed on the web page
        // byte[] is needed to satisfy parameters of contextHandler.task
        public string sendOKStatus(byte[] bytes){
            return "OK 200";     
        }

        // save image submitted from http request
        public string saveImage(byte[] bytes){
            File.WriteAllBytes("images/image" + imageNumber.ToString() + ".jpg", bytes);
            imageNumber++;  
            return "";
        }
    }
}