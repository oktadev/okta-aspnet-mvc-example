using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Configuration;
using System.Security.Claims;

[assembly: OwinStartup(typeof(OktaAspNetExample.Startup))]

namespace OktaAspNetExample
{
    public class Startup
    {
        // These values are stored in Web.config. Make sure you update them!
        string clientId = ConfigurationManager.AppSettings["okta:ClientId"];
        string redirectUri = ConfigurationManager.AppSettings["okta:RedirectUri"];
        string authority = ConfigurationManager.AppSettings["okta:Issuer"];
        string clientSecret = ConfigurationManager.AppSettings["okta:ClientSecret"];
        string postLogoutRedirectUri = ConfigurationManager.AppSettings["okta:PostLogoutRedirectUri"];

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

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                    {
                        var idToken = n.ProtocolMessage.IdToken;

                        if (idToken != null)
                        {
                            n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", idToken));
                        }

                        return Task.CompletedTask;
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
