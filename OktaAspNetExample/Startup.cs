using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Configuration;
using System.Security.Claims;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(OktaAspNetExample.Startup))]

namespace OktaAspNetExample
{
    public class Startup
    {
        // These values are stored in Web.config. Make sure you update them!
        private readonly string clientId = ConfigurationManager.AppSettings["okta:ClientId"];
        private readonly string redirectUri = ConfigurationManager.AppSettings["okta:RedirectUri"];
        private readonly string authority = ConfigurationManager.AppSettings["okta:OrgUri"];
        private readonly string clientSecret = ConfigurationManager.AppSettings["okta:ClientSecret"];
        private readonly string postLogoutRedirectUri = ConfigurationManager.AppSettings["okta:PostLogoutRedirectUri"];

        /// <summary>
        /// Configure OWIN to use OpenID Connect to log in with Okta.
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Authority = authority,
                RedirectUri = redirectUri,
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = OpenIdConnectScope.OpenIdProfile,
                PostLogoutRedirectUri = postLogoutRedirectUri,
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name"
                },

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n => 
                    {
                        // Exchange code for access and ID tokens
                        var tokenClient = new TokenClient(authority + "/v1/token", clientId, clientSecret);
                        var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, redirectUri);

                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }

                        var userInfoClient = new UserInfoClient(authority + "/v1/userinfo");
                        var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);
                        var claims = new List<Claim>();
                        claims.AddRange(userInfoResponse.Claims);
                        claims.Add(new Claim("id_token", tokenResponse.IdentityToken));
                        claims.Add(new Claim("access_token", tokenResponse.AccessToken));

                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            claims.Add(new Claim("refresh_token", tokenResponse.RefreshToken));
                        }

                        n.AuthenticationTicket.Identity.AddClaims(claims);

                        return;
                    },

                    RedirectToIdentityProvider = n =>
                    {
                        // If signing out, add the id_token_hint
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var idTokenClaim = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenClaim != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenClaim.Value;
                            }

                        }

                        return Task.CompletedTask;
                    }
                },
            });
        }
    }
}
