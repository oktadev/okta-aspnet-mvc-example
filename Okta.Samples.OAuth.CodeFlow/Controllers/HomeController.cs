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

using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Web;
using System.Web.Mvc;


namespace Okta.Samples.OAuth.CodeFlow.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Claims()
        {
            ViewBag.Message = "Your claims.";

            return View();
        }


        public ActionResult SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);

            return View();
        }

        [HttpPost]
        public ActionResult OpenIDConnect(FormCollection form)
        {
            if (form["error"] != null)
            { //we fall here when Okta is the Authorization server
                string error = form["error"];
                string desc = form["error_description"];
            }
            else if (form["code"] != null)
            {//we fall here when IdentityServer is the Authorization server
                string authCode = form["code"];
                string state = form["state"];

            }

            return View();
        }

    }
}