using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(OktaAspNetExample.Startup))]

namespace OktaAspNetExample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            ConfigureAuth(app);
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
            });

            var clientId = ConfigurationManager.AppSettings["okta:ClientId"].ToString();
            var clientSecret = ConfigurationManager.AppSettings["okta:ClientSecret"].ToString();
            var issuer = ConfigurationManager.AppSettings["okta:Issuer"].ToString();
            var redirectUri = ConfigurationManager.AppSettings["okta:RedirectUri"].ToString();
            var apiAccessManagement =
                bool.Parse(ConfigurationManager.AppSettings["okta:ApiAccessManagement"].ToString());
            var authorizationServerId = ConfigurationManager.AppSettings["okta:AuthorizationServerId"].ToString();
            
            var baseUrlBuilder = new StringBuilder(issuer);
            baseUrlBuilder.Append("/oauth2");

            if (apiAccessManagement)
            {
                baseUrlBuilder.Append($"/{authorizationServerId}");
            }

            var baseUrl = baseUrlBuilder.ToString();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Authority = apiAccessManagement ? baseUrl : issuer,
                RedirectUri = redirectUri,
                ResponseType = "code id_token",
                UseTokenLifetime = false,
                Scope = "openid profile",
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["okta:PostLogoutRedirectUri"].ToString(),
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name"
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = context =>
                    {
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idToken = context.OwinContext.Authentication.User.Claims.FirstOrDefault(c => c.Type == "id_token")?.Value;
                            context.ProtocolMessage.IdTokenHint = idToken;
                        }

                        return Task.FromResult(true);
                    },
                    AuthorizationCodeReceived = async context =>
                    {
                        // Exchange code for access and ID tokens
                        var tokenClient = new TokenClient($"{baseUrlBuilder}/v1/token", clientId, clientSecret);
                        var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(context.ProtocolMessage.Code, redirectUri);

                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }
                        
                        var userInfoClient = new UserInfoClient($"{baseUrlBuilder}/v1/userinfo");
                        var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);

                        var identity = new ClaimsIdentity();
                        identity.AddClaims(userInfoResponse.Claims);

                        identity.AddClaim(new Claim("id_token", tokenResponse.IdentityToken));
                        identity.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                        {
                            identity.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                        }

                        var nameClaim = new Claim(ClaimTypes.Name, userInfoResponse.Claims.FirstOrDefault(c => c.Type == "name")?.Value);
                        identity.AddClaim(nameClaim);


                        context.AuthenticationTicket = new AuthenticationTicket(
                            new ClaimsIdentity(identity.Claims, context.AuthenticationTicket.Identity.AuthenticationType),
                            context.AuthenticationTicket.Properties);
                    }
                }
            });
        }
    }
}
