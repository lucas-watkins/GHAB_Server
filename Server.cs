using System.Text;

namespace Server{
    class mainServer : BaseHttpServer{
        public mainServer(){
            ctxHandlers = [new contextHandler(writeGot, "/", false),
                           new contextHandler(printBytesToConsole, "/post", true)];
            
        }

        public static async Task Main(string[] args) {
            mainServer s = new mainServer();
            await s.Serve();   
        }

        public static string printBytesToConsole(byte[] bytes){
            Console.WriteLine(Encoding.UTF8.GetString(bytes)); 
            return ""; 
        }


        public static string writeGot(byte[] b){
            Console.WriteLine("Get Recieved");
            return "Hello World!";     
        }
    }
}