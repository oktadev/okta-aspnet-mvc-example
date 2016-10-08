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

using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Okta.Samples.OAuth.CodeFlow.Controllers
{
    public class CallbackController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Index(FormCollection form)
        {
            ViewBag.Code = form["code"] ?? "none";


            var state = form["state"];
            var tempState = await GetTempStateAsync();

            if (tempState != null && state.Equals(tempState.Item1, StringComparison.Ordinal))
            {
                ViewBag.State = state + " (valid)";
            }
            else
            {
                ViewBag.State = state + " (invalid)";
            }

            ViewBag.Error = Request.QueryString["error"] ?? "none";

            var response = await GetTokenFromAuthServer();

            return View("Token", response);

            //            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetToken()
        {

            var response = await GetTokenFromAuthServer();

            return View("Token", response);
        }

        [Authorize]
        public ActionResult Tokens()
        {
            ViewBag.Message = "View Tokens";

            if (HttpContext.GetOwinContext() != null && HttpContext.GetOwinContext().Authentication.User != null)
            {
                ClaimsPrincipal currentUser = HttpContext.GetOwinContext().Authentication.User;

                var idToken = HttpContext.GetOwinContext().Authentication.User.FindFirst("id_token");
                if (idToken != null)
                    ViewBag.IdentityTokenParsed = ParseJwt(idToken.Value);

                var accessToken = HttpContext.GetOwinContext().Authentication.User.FindFirst("access_token");
                if (accessToken != null)
                    ViewBag.AccessTokenParsed = ParseJwt(accessToken.Value);

                var refreshToken = HttpContext.GetOwinContext().Authentication.User.FindFirst("refresh_token");
                if (refreshToken != null)
                    ViewBag.RefreshToken = refreshToken.Value;
            }

            return View();
        }

        private async Task<TokenResponse> GetTokenFromAuthServer()
        {
            string oidcClientId = ConfigurationManager.AppSettings["okta:OAuthClientId"];
            string oidcClientSecret = ConfigurationManager.AppSettings["okta:OAuthClientSecret"];
            string oktaTenantUrl = ConfigurationManager.AppSettings["okta:OAuthAuthority"];
            string oidcRedirectUrl = ConfigurationManager.AppSettings["okta:OAuthRedirectUri"];

            var client = new TokenClient(
                oktaTenantUrl + Constants.TokenEndpoint,
                oidcClientId,
                oidcClientSecret,
                AuthenticationStyle.PostValues);

            var code = Request.Form["code"];
            var tempState = await GetTempStateAsync();
            Request.GetOwinContext().Authentication.SignOut("TempState");

            var response = await client.RequestAuthorizationCodeAsync(
                code,
                oidcRedirectUrl);

            await ValidateResponseAndSignInAsync(response);

            if (!string.IsNullOrEmpty(response.IdentityToken))
            {
                ViewBag.IdentityTokenParsed = ParseJwt(response.IdentityToken);
            }
            if (!string.IsNullOrEmpty(response.AccessToken))
            {
                ViewBag.AccessTokenParsed = ParseJwt(response.AccessToken);
            }

            return response;
        }

        private async Task ValidateResponseAndSignInAsync(TokenResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.IdentityToken))
            {
                var claims = new List<Claim>();

                if (!string.IsNullOrWhiteSpace(response.AccessToken))
                {
                    claims.AddRange(await GetUserInfoClaimsAsync(response.AccessToken));

                    claims.Add(new Claim("id_token", response.IdentityToken));
                    claims.Add(new Claim("access_token", response.AccessToken));
                    claims.Add(new Claim("expires_at", (DateTime.UtcNow.ToEpochTime() + response.ExpiresIn).ToDateTimeFromEpoch().ToString()));
                }

                if (!string.IsNullOrWhiteSpace(response.RefreshToken))
                {
                    claims.Add(new Claim("refresh_token", response.RefreshToken));
                }

                var id = new ClaimsIdentity(claims, "Cookies");
                Request.GetOwinContext().Authentication.SignIn(id);
            }
        }

        private async Task<IEnumerable<Claim>> GetUserInfoClaimsAsync(string accessToken)
        {
            string oktaTenantUrl = ConfigurationManager.AppSettings["okta:OAuthAuthority"];

            var userInfoClient = new UserInfoClient(new Uri(oktaTenantUrl + Constants.UserInfoEndpoint), accessToken);

            var userInfo = await userInfoClient.GetAsync();

            var claims = new List<Claim>();
            userInfo.Claims.ToList().ForEach(ui => claims.Add(new Claim(ui.Item1, ui.Item2)));

            return claims;
        }

        private string ParseJwt(string token)
        {
            if (!token.Contains("."))
            {
                return token;
            }

            var parts = token.Split('.');
            var part = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));

            var jwt = JObject.Parse(part);
            return jwt.ToString();
        }

        private async Task<Tuple<string, string>> GetTempStateAsync()
        {
            var data = await Request.GetOwinContext().Authentication.AuthenticateAsync("TempState");

            if (data != null && data.Identity != null && data.Identity.FindFirst("state") != null)
            {
                var state = data.Identity.FindFirst("state").Value;
                var nonce = data.Identity.FindFirst("nonce").Value;

                return Tuple.Create(state, nonce);

            }
            return null;
        }
    }
}