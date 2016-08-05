using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace OktaOpenIDConnect
{
    public class OktaCompatibleOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly ILogger _logger2;

        public OktaCompatibleOpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options): 
            base(next, app, options)
            
        {
            _logger2 = app.CreateLogger<OpenIdConnectAuthenticationMiddleware>();
            
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new OpenIdConnectAuthenticationHandler(_logger2);
        }
    }
}