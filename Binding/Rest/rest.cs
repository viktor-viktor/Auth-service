using System;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;

using AuthService.DAL;
using AuthService.Models;
using AuthService.Middleware;

namespace AuthService.Binding.Rest
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationController(MongoDAL mongo, ErrorHandler errorHandler)
        {
            m_mongo = mongo;
            m_errorHandler = errorHandler;
        }

        [HttpGet]
        //TODO: change returning valie on something valid, client should recevie json
        public Token Get()
        {
            Token retVal = null;
            AuthenticationService service = TryCreateAuthService();
            if (service != null)
            {
                retVal = service.SignInUser();
            }

            return retVal;
        }

        // TODO: change JsonElement for custom json data class with possibility to make request withoud body
        // by specifying data = null
        [HttpPost]
        public Token Post([FromBody] JsonElement data)
        {
            AuthenticationService service = TryCreateAuthService();
            Token retVal = null;
            if (service != null)
            {
                retVal = service.RegisterUser(data);
            }

            return retVal;
        }

        [HttpDelete]
        public void Delete()
        {
            AuthenticationService service = TryCreateAuthService();
            if (service != null)
            {
                string retVal = service.UnregisterUser();
            }
        }

        private AuthenticationService TryCreateAuthService()
        {
            AuthenticationService service = null;

            StringValues header;
            if (Request.Headers.TryGetValue("Authorization", out header))
            {
                string authHeader = header.ToString();
                authHeader = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string namePsw = encoding.GetString(Convert.FromBase64String(authHeader));

                int seperatorIndex = namePsw.IndexOf(':');
                string username = namePsw.Substring(0, seperatorIndex);
                string psw = namePsw.Substring(seperatorIndex + 1);

                service = new AuthenticationService(username, psw, m_mongo, m_errorHandler);

            }
            else
            {
                m_errorHandler.SetErrorData(new HttpResult(400, "Missing header : 'Authorization'"));
            }

            return service;
        }

        private MongoDAL m_mongo;
        private ErrorHandler m_errorHandler;
    }

    [Route("api/authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        [Authorize(Policy = Policies.AdminOnly)]
        [HttpGet("admin")]
        public void GetAdmin()
        {
        }

        [Authorize(Policy = Policies.AdminAndDev)]
        [HttpGet("dev")]
        public void GetDev()
        {
        }

        [Authorize(Policy = Policies.All)]
        [HttpGet("public")]
        public void GetPublic()
        {
        }
    }
}
