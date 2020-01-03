
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Web;

using MongoDB.Bson;
using MongoDB.Driver;

using StackExchange.Redis;

using CSharp.DAL;
using CSharp;
using CSharp.Binding.Rest;
using Microsoft.IdentityModel.Logging;


namespace AuthServer
{
    class UrlManager
    {
        public void Init()
        {
            m_urls = new List<Func<UrlBase>>();
            IdentityModelEventSource.ShowPII = true;
        }

        public void WriteAnswer(HttpListenerContext ctx, string data, int statusCode = 200)
        {
            byte[] result = Encoding.UTF8.GetBytes(data);
            ctx.Response.ContentType = "text/html";
            ctx.Response.ContentEncoding = Encoding.UTF8;
            ctx.Response.ContentLength64 = result.LongLength;

            ctx.Response.StatusCode = statusCode;
            ctx.Response.OutputStream.WriteAsync(result, 0, result.Length);
            ctx.Response.Close();
        }

        //TODO: change method to take method that will create class instance with given context
        public void RegisterUrlClass(Func<UrlBase> method)
        {
            m_urls.Add(method);
        }

        public void ProcessRequest(HttpListenerContext ctx)
        {
            System.Threading.Thread.Sleep(50);
            try
            {
                ProcessRequestInternal(ctx);
            }
            catch (Exception e)
            {
                HandleException(ctx, e);
            }

        }
        private void ProcessRequestInternal(HttpListenerContext ctx)
        {
            bool urlFound = false;
            HttpListenerRequest req = ctx.Request;
            foreach (Func<UrlBase> func in m_urls)
            {
                UrlBase api = func();
                if (api.GetUrl() == req.Url.AbsolutePath)
                {
                    api.Init(ctx);
                    string method = req.HttpMethod;
                    if (method == "GET")
                        api.Get(ctx);
                    else if (method == "POST")
                        api.Post(ctx);
                    else if (method == "PUT")
                        api.Put(ctx);
                    else if (method == "DELETE")
                        api.Delete(ctx);
                    else
                        throw new HttpMethodNotAllowedException("Given method isn't allowed !");

                    urlFound = true;
                    break;
                }
            }

            if (!urlFound)
            {
                throw new HttpNotFoundException("Handler for such URL isn't found !");
            }
        }
        
        private void HandleException(HttpListenerContext ctx, Exception e)
        {
            if (e is HttpException)
            {
                HttpException exc = (HttpException)(e);
                WriteAnswer(ctx, exc.Message, exc.GetHttpCode());
            }
            else
            {
                WriteAnswer(ctx, "Unknow exception occured !", 500);
            }
        }

        private List<Func<UrlBase>> m_urls;
    }

    class UrlBase
    {
        public UrlBase(string url) { m_url = url; }

        public string GetUrl() { return m_url; }
        public virtual void Init(HttpListenerContext ctx) { throw new HttpInternalErrorException("UrlBase.Init() method should be overrided!"); }

        public virtual void Get(HttpListenerContext ctx) { throw new HttpMethodNotAllowedException("Given method isn't allowed !"); }
        public virtual void Post(HttpListenerContext ctx) { throw new HttpMethodNotAllowedException("Given method isn't allowed !"); }
        public virtual void Put(HttpListenerContext ctx) { throw new HttpMethodNotAllowedException("Given method isn't allowed !"); }
        public virtual void Delete(HttpListenerContext ctx) { throw new HttpMethodNotAllowedException("Given method isn't allowed !"); }


        private string m_url;
    }
    class HttpServer
    {
        public  HttpListener listener;
        public  string url = "http://127.0.0.1:8000/";

        public  async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();

                //Task.Run(() => App.Urlmanager.ProcessRequest(ctx));
                App.Urlmanager.ProcessRequest(ctx);
            }
        }

        public  void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            listener.Close();
        }
    }

    class App
    {
        public static HttpServer Server;
        public static UrlManager Urlmanager;
        public static MongoDAL MongoDal;

        public static void Main(string[] args)
        {
            MongoDal = new MongoDAL();
            MongoDal.Init();

            Urlmanager = new UrlManager();
            Urlmanager.Init();
            InitApi.Init();

            Server = new HttpServer();
            Server.Start();
        }
    }
}
