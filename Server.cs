namespace Server{
    class mainServer : BaseHttpServer{
        public mainServer(){
            ctxHandlers = [new contextHandler(writeGot, "/", false)];
        }

        public static async Task Main(string[] args) {
            mainServer s = new mainServer();
            await s.Serve();   
        }


        public static string writeGot(byte[]m){
            Console.WriteLine("Get Recieved");
            return "Hello World!";     
        }
    }
}