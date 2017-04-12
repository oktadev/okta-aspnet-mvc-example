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

using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Okta.Samples.OAuth.CodeFlow.Controllers
{
    public class CallApiController : Controller
    {
        // GET: CallApi
        public ActionResult Index()
        {
            return View();
        }

        // GET: CallApi/ClientCredentials
        public async Task<ActionResult> GetProtectedResource()
        {
            ClaimsPrincipal identity = Request.GetOwinContext().Authentication.User;

            var accessToken = identity.Claims.Where(c => c.Type == "access_token").Select(c => c.Value).SingleOrDefault();

            var result = await CallApi(accessToken);

            ViewBag.Json = result;
            return View("ShowApiResult");
        }

        private async Task<string> CallApi(string token)
        {
            string apiEndpointUrl = System.Configuration.ConfigurationManager.AppSettings["okta:ApiEndpoint"];
            var client = new HttpClient();
            client.SetBearerToken(token);

            var json = await client.GetStringAsync(apiEndpointUrl);
            return JArray.Parse(json).ToString();
        }
    }
}