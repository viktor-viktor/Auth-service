using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

using CSharp;
using CSharp.DAL;

namespace CSharp.Binding.Rest
{
    class InitApi
    {
        public static void Init()
        {
            AuthServer.App.Urlmanager.RegisterUrlClass(AuthenticationApi.CreateClass);
            AuthServer.App.Urlmanager.RegisterUrlClass(AuthorizationApi.CreateClass);
        }
    }

    class AuthenticationApi : AuthServer.UrlBase
    {
        public static AuthenticationApi CreateClass()
        {
            return new AuthenticationApi("/registration");
        }

        public AuthenticationApi(string url) : base(url)
        {
        }

        public override void Init(HttpListenerContext ctx)
        {
            string authHeader = ctx.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                authHeader = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string namePsw = encoding.GetString(Convert.FromBase64String(authHeader));

                int seperatorIndex = namePsw.IndexOf(':');
                string username = namePsw.Substring(0, seperatorIndex);
                string psw = namePsw.Substring(seperatorIndex + 1);

                m_service = new AuthenticationService(username, psw);
            }
            else
            {
                throw new HttpBadRequestException("Missing header: Authorization");
            }
        }

        public override void Get(HttpListenerContext ctx)
        {
            string retVal = m_service.SignInUser();
            AuthServer.App.Urlmanager.WriteAnswer(ctx, retVal);
        }

        public override void Post(HttpListenerContext ctx)
        {
            string retVal = m_service.RegisterUser();
            AuthServer.App.Urlmanager.WriteAnswer(ctx, retVal);
        }
        public override void Delete(HttpListenerContext ctx)
        {
            string retVal = m_service.UnregisterUser();
            AuthServer.App.Urlmanager.WriteAnswer(ctx, retVal);
        }

        private AuthenticationService m_service;
    }

    class AuthorizationApi : AuthServer.UrlBase
    {
        public static AuthorizationApi CreateClass()
        {
            return new AuthorizationApi("/auth");
        }
        public AuthorizationApi(string url) : base(url)
        {
        }

        public override void Init(HttpListenerContext ctx)
        {
            string authHeader = ctx.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer"))
            {
                authHeader = authHeader.Substring("Bearer ".Length).Trim();
                
                m_service = new AuthorizationService();
                m_service.Init(authHeader);
            }
            else
            {
                throw new HttpBadRequestException("Missing header: Authorization");
            }
        }

        public override void Get(HttpListenerContext ctx)
        {
            // TODO: update token if it's about to expired
            AuthServer.App.Urlmanager.WriteAnswer(ctx, "Token is valid");
        }

        private AuthorizationService m_service;
    }

}
