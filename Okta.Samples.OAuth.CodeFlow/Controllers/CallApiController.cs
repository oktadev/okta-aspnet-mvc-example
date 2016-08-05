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


        //private async Task<TokenResponse> GetTokenAsync()
        //{
        //    var client = new TokenClient(
        //        "https://localhost:44323/identity/connect/token",
        //        "mvc_service",
        //        "secret");

        //    return await client.RequestClientCredentialsAsync("sampleApi");
        //}

        private async Task<string> CallApi(string token)
        {
            string apiEndpointUrl = System.Configuration.ConfigurationManager.AppSettings["okta:ApiEndpoint"];
            //test with invalid token
            //token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlU1UjhjSGJHdzQ0NVFicTh6Vk8xUGNDcFhMOHlHNkljb3ZWYTNsYUNveE0ifQ.eyJ2ZXIiOjEsImp0aSI6IkFULjlZRkpfbjdWS3ZLaGNHVG9Lc2F0OVcyX2wtUzAxT0ltZGlCc1pLb0l0dWsiLCJpc3MiOiJodHRwczovL29pZGNkZW1vcy5va3RhcHJldmlldy5jb20vYXMvb3JzNmRyN2t4OE5FbUVPejQwaDciLCJhdWQiOiJ3TWxvMGw5VDNiZW5jTUsyZXhZOSIsInN1YiI6IjAwdTZkcjdqYnZZRTB5ZDlmMGg3IiwiaWF0IjoxNDY5MTYyMzM2LCJleHAiOjE0NjkxNjU5MzYsImNpZCI6IndNbG8wbDlUM2JlbmNNSzJleFk5IiwidWlkIjoiMDB1NmRyN2pidllFMHlkOWYwaDciLCJzY3AiOlsib3BlbmlkIiwiZW1haWwiLCJwcm9maWxlIiwiYWRkcmVzcyIsInBob25lIiwiZ3JvdXBzIiwib2ZmbGluZV9hY2Nlc3MiXSwiY3VzdG9tLXVzZXJuYW1lIjoiYm9iLWFwcHVzZXJAbWFpbGluYXRvci5jb20ifQ.bHcVHHr6lSwldc1gCW7YC3m4Iig4_2iK - QYBIv7TnYTQAnDlbjRhzxlUiA88ph - pNXPvyz80434 - 9IB3 - 1Bq2a19EPyXdBytjOos7dJY2LvUSKa - MprD7W94DsdyczgNHh8SmoiGCeelv8y2apE90ph_O3VjwyGjVvCteY_PIRYPtDsXFOhOTxmndjzmkVJeBxs2Rm8Ve7PDR0Kx11N0NgIxYVsyC7PmW - TjNIhdRZDZx20q7FnXSi6lGVvNrLIEA2DaqhpuHFr5_8vhYVsleDIbpsq3RqrT2whOARAo - NZ3PFko1TNUp3iwoQZHBtRFvTEKhDjWqpL8P3t0haoViQ";
            var client = new HttpClient();
            client.SetBearerToken(token);

            var json = await client.GetStringAsync(apiEndpointUrl);
            return JArray.Parse(json).ToString();
        }
    }
}