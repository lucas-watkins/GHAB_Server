﻿using System.Net;
using System.Text;
// lol there was a 12gb memory leak with this one 

namespace HAB_WebServer{

    // context handler class which stores a task, string, and bool to determine what context the http get is in 
    class ContextHandler(Func<byte[], string> task, string path, bool isPost)
    {
        public Func<byte[], string> Task {get;} = task;
        public string Path {get;} = path;

        public bool IsPost{get;} = isPost;
    }

    class BaseHttpServer{
        const bool IsRunning = true; 
        
        // http listener class for server
        public static HttpListener? HttpListener;

        private HttpListenerContext? _context;
        
        // url to serve on 
        public static string? BaseUrl;

        public static int? Port;

        protected ContextHandler[] CtxHandlers = [];

        protected Logging? Logger; 
        
        protected async Task Serve(){
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(BaseUrl + ':' + Port.ToString() + "/");
            HttpListener.Start();
            // if logger not null start server
            Logger?.LogServerStart();
            while (IsRunning){
                // allows for handling of multiple connections
                _context = await HttpListener.GetContextAsync();
                #pragma warning disable 4014
                Task.Run(() => HandleIncomingConnections(_context));
            }
        }

        private async Task HandleIncomingConnections(HttpListenerContext context){
            // foreach context handler in the list of handlers, if it matches the http method and is the request url,
            // then start the task
            // nullable path for server request, but check for null anyways during the if statement
            string? absPath = context.Request.Url?.AbsolutePath;
            string httpMethod = context.Request.HttpMethod;
            foreach (ContextHandler handler in CtxHandlers){
                if (absPath != null &&(handler.IsPost ? "POST" : "GET").Equals(httpMethod) && 
                    handler.Path.Equals(absPath)){
                        // if http method is get response and send back message returned from task
                        // log incoming connection if logger is not null
                        Logger?.LogIncomingConnection(context.Request.UserHostAddress, 
                            absPath, httpMethod);
                        
                        if (httpMethod.Equals("GET")){
                            HttpListenerResponse response = context.Response;
                            string message = 
                                "<!DOCTYPE>" +
                                "<html>" +
                                "  <head>" +
                                "    <title>HAB Server</title>" +
                                "  </head>" +
                                "  <body>" +
                                "  <p>" + handler.Task.Invoke([]) + "</p>" + 
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

                        // if http method is post then run a task asynchronously with the read bytes as input and send
                        // back "OK" on webpage
                        if (httpMethod.Equals("POST")){ 

                            // read all bytes with memory stream, make sure not to include headers in the request
                            // in client software otherwise this will not work to save images properly
                            Stream inputStream = context.Request.InputStream;
                            MemoryStream ms = new MemoryStream(); 
                            await inputStream.CopyToAsync(ms); 
                            byte[] data = ms.ToArray(); 
                            
                            // Log connection
                            

                            // handle task
                            Task.Run(() => handler.Task.Invoke(data)); 
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