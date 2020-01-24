﻿using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace AuthService.Middleware
{ 
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) :
            base(options, logger, encoder, clock)
        {

        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthenticationTicket ticket = new AuthenticationTicket();
            return AuthenticateResult.Success("Aasd");
        }
    }
}
}