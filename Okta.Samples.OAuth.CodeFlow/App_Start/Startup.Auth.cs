/*!
 * Copyright (c) 2016, Okta, Inc. and/or its affiliates. All rights reserved.
 * The Okta software accompanied by this notice is provided pursuant to the Apache License, Version 2.0 (the "License.")
 *
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *
 * See the License for the specific language governing permissions and limitations under the License.
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;
using IdentityModel.Client;
using System;
using System.Security.Claims;
using System.Linq;

namespace Okta.Samples.OAuth.CodeFlow
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            string oktaOAuthClientId = ConfigurationManager.AppSettings["okta:OauthClientId"] as string;
            string oidcClientSecret = ConfigurationManager.AppSettings["okta:OAuthClientSecret"];
            string oidcAuthority = ConfigurationManager.AppSettings["okta:OAuthAuthority"] as string;
            string oidcRedirectUri = ConfigurationManager.AppSettings["okta:OAuthRedirectUri"] as string;
            string oidcResponseType = ConfigurationManager.AppSettings["okta:OAuthResponseType"] as string;
            string oauthScopes = ConfigurationManager.AppSettings["okta:OAuthScopes"];

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = oktaOAuthClientId,
                Authority = oidcAuthority,
                RedirectUri = oidcRedirectUri,
                ResponseType = oidcResponseType,
                Scope = oauthScopes,

                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = true,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                    {
                        // use the code to get the access and refresh token
                        var tokenClient = new TokenClient(
                            oidcAuthority + Constants.TokenEndpoint,
                            oktaOAuthClientId,
                            oidcClientSecret, AuthenticationStyle.BasicAuthentication);

                        var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }

                        // use the access token to retrieve claims from userinfo
                        var userInfoClient = new UserInfoClient(new Uri(oidcAuthority + Constants.UserInfoEndpoint), tokenResponse.AccessToken);

                        var userInfoResponse = await userInfoClient.GetAsync();

                        //// create new identity
                        var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
                        //adding the claims we get from the userinfo endpoint
                        var idClaims = userInfoResponse.GetClaimsIdentity();
                        id.AddClaims(idClaims.Claims);

                        //also adding the ID, Access and Refresh tokens to the user claims 
                        id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
                        if(tokenResponse.RefreshToken != null)
                            id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));

                        id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
                        if (tokenResponse.RefreshToken != null)
                        {
                            id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
                        }

                        // Make sure the Name claim is populated (for Identity.Name)
                        var nameClaim = new Claim(ClaimTypes.Name, idClaims.Claims.FirstOrDefault(c => c.Type == "name")?.Value);
                        id.AddClaim(nameClaim);

                        n.AuthenticationTicket = new AuthenticationTicket(new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType),
                            n.AuthenticationTicket.Properties);
                    },
                }
            });
        }
    }
}
