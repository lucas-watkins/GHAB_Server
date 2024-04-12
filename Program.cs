using System.Net;
using System.Text;
// lol there was a 12gb memory leak with this one 

namespace Server{

    // context handler class which stores a task, string, and bool to determine what context the http get is in 
    class contextHandler{
        public Func<string> task {get;}
        public string path {get;}

        public bool isPost{get;}
        public contextHandler(Func<string> task, string path, bool isPost){
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
                context = await httpListener.GetContextAsync();
                await handleIncommingConnections(context);
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
                        if (httpMethod.Equals("GET")){
                            HttpListenerResponse response = context.Response;
                            string message = 
                                "<!DOCTYPE>" +
                                "<html>" +
                                "  <head>" +
                                "    <title>HAB Server</title>" +
                                "  </head>" +
                                "  <body>" +
                                "  <p>" + handler.task.Invoke() + "</p>" + 
                                "  </body>" +
                                "</html>"; 

                            byte[] data = Encoding.UTF8.GetBytes(message);
                            response.ContentType = "text/html"; 
                            response.ContentEncoding = Encoding.UTF8; 
                            response.ContentLength64 = data.Length; 
                            await response.OutputStream.WriteAsync(data, 0, data.Length);
                            response.OutputStream.Close(); 
                        }
                        // handle task
                         
                }
            }
           
        }
    }   
}