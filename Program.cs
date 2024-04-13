using System.Net;
using System.Text;
// lol there was a 12gb memory leak with this one 

namespace Server{

    // context handler class which stores a task, string, and bool to determine what context the http get is in 
    class contextHandler{
        public Func<byte[], string> task {get;}
        public string path {get;}

        public bool isPost{get;}
        public contextHandler(Func<byte[], string> task, string path, bool isPost){
            this.task = task; 
            this.path = path; 
            this.isPost = isPost;
        }
    }

    class BaseHttpServer{
        bool isRunning = true; 
        
        // http listener class for server
        private HttpListener httpListener;

        private HttpListenerContext? context;
        
        // url to serve on 
        public string baseURL = "http://*:";

        public int port = 8080; 

        public contextHandler[] ctxHandlers = []; 


        public BaseHttpServer(){
            httpListener = new HttpListener();
            httpListener.Prefixes.Add(baseURL + port.ToString() + "/");
            httpListener.Start();

        }

        public void Stop(){
            httpListener.Stop();
            isRunning = false; 
        }

        public async Task Serve(){
            while (isRunning){
                // allows for handling of multiple connections
                context = await httpListener.GetContextAsync();
                #pragma warning disable 4014
                Task.Run(() => handleIncommingConnections(context));
            }
            Stop(); 
        }

        public async Task handleIncommingConnections(HttpListenerContext context){
            // foreach contexthandler in the list of handlers, if it matches the http method and is the request url,
            // then start the task
            // nullable path for server request, but check for null anyways during the if statement
            string? absPath = context.Request.Url?.AbsolutePath;
            string httpMethod = context.Request.HttpMethod;
            foreach (contextHandler handler in ctxHandlers){
                if (absPath != null &&(handler.isPost ? "POST" : "GET").Equals(httpMethod) && 
                    handler.path.Equals(absPath)){
                        // if http method is get get response and send back message returned from task
                        if (httpMethod.Equals("GET")){
                            HttpListenerResponse response = context.Response;
                            string message = 
                                "<!DOCTYPE>" +
                                "<html>" +
                                "  <head>" +
                                "    <title>HAB Server</title>" +
                                "  </head>" +
                                "  <body>" +
                                "  <p>" + handler.task.Invoke([]) + "</p>" + 
                                "  </body>" +
                                "</html>"; 

                            // write back bytes
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            response.ContentType = "text/html"; 
                            response.ContentEncoding = Encoding.UTF8; 
                            response.ContentLength64 = data.Length; 
                            await response.OutputStream.WriteAsync(data, 0, data.Length);
                            response.OutputStream.Close(); 
                        }

                        // if http method is post then run a task asyncronously with the read bytes as input and send
                        // back "OK" on webpage
                        if (httpMethod.Equals("POST")){ 

                            // read all bytes with memory stream, make sure not to include headers in the request
                            // in client software otherwise this will not work to save images properly
                            Stream inputStream = context.Request.InputStream;
                            MemoryStream ms = new MemoryStream(); 
                            inputStream.CopyTo(ms); 
                            byte[] data = ms.ToArray(); 

                            // handle task
                            Task.Run(() => handler.task.Invoke(data)); 
                            byte[] message =
                            Encoding.UTF8.GetBytes(
                                "<!DOCTYPE>" +
                                "<html>" +
                                "  <head>" +
                                "    <title>HAB Server</title>" +
                                "  </head>" +
                                "  <body>" +
                                "  <p>Submit OK</p>" + 
                                "  </body>" +
                                "</html>");

                            // write bytes
                            HttpListenerResponse response = context.Response;  
                            response.ContentType = "text/html";
                            response.ContentEncoding = Encoding.UTF8; 
                            response.ContentLength64 = message.Length; 
                            await response.OutputStream.WriteAsync(message, 0, message.Length);
                            response.OutputStream.Close();    
                        }
                        
                         
                }
            }
           
        }
    }   
}