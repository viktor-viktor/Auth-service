using System;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using AuthService.DAL;
using AuthService.Models;

namespace AuthService.Binding.Rest
{
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

        [HttpPost]
        public Token Post()
        {
            AuthenticationService service = TryCreateAuthService();
            Token retVal = null;
            if (service != null)
            {
                retVal = service.RegisterUser();
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
        public AuthorizationController(ErrorHandler errorHandler)
        {
            m_errorHandler = errorHandler;
        }

        [HttpGet("{role}")]
        //TODO: change return value on something valid (user should receive json data)
        // all validation is already done at constructor
        public void Get(string role)
        {
            AuthorizationService service = null;
            if (TryGetService(out service))
            {
                if (role == Role.Admin.Value)
                {
                    service.IsAdminRole();
                }
                else if (role == Role.Dev.Value)
                {
                    service.IsDevRole();
                }
                else if (role == Role.User.Value)
                {
                    service.IsUserRole();
                }
                else
                {
                    m_errorHandler.SetErrorData(400, "Wrong role specified at url");
                }
            }
        }

        private bool TryGetService(out AuthorizationService service)
        {
            StringValues header;
            service = null;
            if (Request.Headers.TryGetValue("Authorization", out header))
            {
                string authHeader = header.ToString();
                authHeader = authHeader.Substring("Bearer ".Length).Trim();

                service = new AuthorizationService(m_errorHandler);
                service.Init(authHeader);

                return true;
            }

            return false;
        }

        private AuthorizationService m_service;
        private ErrorHandler m_errorHandler;
    }
}
