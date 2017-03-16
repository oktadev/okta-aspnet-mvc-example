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

using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Web.Configuration;
using System.Web.Http;

[assembly: OwinStartup(typeof(Api.Startup))]

namespace Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

          //  web api configuration
           var config = new HttpConfiguration();
            //config.MapHttpAttributeRoutes();
            config.EnableSystemDiagnosticsTracing();

            //app.UseWebApi(config);

            //WebApiConfig.Register(config);

            var clientID = WebConfigurationManager.AppSettings["okta:ClientId"];
            var tenantUrl = WebConfigurationManager.AppSettings["okta:TenantUrl"];

            var tvps = new TokenValidationParameters
            {
                ValidAudience = tenantUrl,
                ValidateAudience = true,
                ValidIssuer = tenantUrl,
                ValidateIssuer = true,
            };

            var additionalTokenValidationParamters = new Dictionary<string, string>()
            {
                // Validate Client ID claim
                ["cid"] = clientID
            };

            var securityTokenProvider = new OpenIdConnectCachingSecurityTokenProvider(tenantUrl + "/.well-known/openid-configuration");
            var jwtFormat = new CustomValidatingJwtFormat(tvps, additionalTokenValidationParamters, securityTokenProvider);

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = jwtFormat
            });
        }
    }
}