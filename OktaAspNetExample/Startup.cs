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
        string clientId = ConfigurationManager.AppSettings["okta:ClientId"];
        string redirectUri = ConfigurationManager.AppSettings["okta:RedirectUri"];
        string authority = ConfigurationManager.AppSettings["okta:Issuer"];
        string clientSecret = ConfigurationManager.AppSettings["okta:ClientSecret"];
        string postLogoutRedirectUri = ConfigurationManager.AppSettings["okta:PostLogoutRedirectUri"];

        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
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
                            var tokenId = n.ProtocolMessage.IdToken;

                            if (tokenId != null)
                            {
                                n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", tokenId));
                            }

                            return Task.FromResult(0);
                        },

                        RedirectToIdentityProvider = n =>
                        {
                            // if signing out, add the id_token_hint
                            if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Logout)
                            {
                                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                                if (idTokenHint != null)
                                {
                                    n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                                }

                            }

                            return Task.FromResult(0);
                        }
                    },
                }
            );
        }
    }
}
