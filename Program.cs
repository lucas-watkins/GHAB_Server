﻿using System.Net;
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

                            byte[] data = Encoding.UTF8.GetBytes(message);
                            response.ContentType = "text/html"; 
                            response.ContentEncoding = Encoding.UTF8; 
                            response.ContentLength64 = data.Length; 
                            await response.OutputStream.WriteAsync(data, 0, data.Length);
                            response.OutputStream.Close(); 
                        }

                        if (httpMethod.Equals("POST")){ 
                            Stream inputStream = context.Request.InputStream;
                            StreamReader sr = new StreamReader(inputStream, context.Request.ContentEncoding); 
                            byte[] data = context.Request.ContentEncoding.GetBytes(sr.ReadToEnd());

                            Task.Run(() => handler.task.Invoke(data)); 
                            byte[] message =
                            Encoding.UTF8.GetBytes(
                                "<!DOCTYPE>" +
                                "<html>" +
                                "  <head>" +
                                "    <title>HAB Server</title>" +
                                "  </head>" +
                                "  <body>" +
                                "  <p>OK</p>" + 
                                "  </body>" +
                                "</html>");

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