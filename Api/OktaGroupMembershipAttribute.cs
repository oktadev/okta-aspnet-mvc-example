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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading;
using System.Web.Http.Controllers;

namespace Api
{
    public class OktaGroupAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        public GroupPolicy Policy { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool isAuthorized = base.IsAuthorized(actionContext);
            if (isAuthorized)
            {
                if (Thread.CurrentPrincipal != null)
                {
                    string strGroups = System.Web.Configuration.WebConfigurationManager.AppSettings["okta:RequiredGroupMemberships"];
                    if (!string.IsNullOrEmpty(strGroups))
                    {
                        List<string> lstGroupNames = strGroups.Split(',').ToList<string>();
                        ClaimsPrincipal principal = Thread.CurrentPrincipal as ClaimsPrincipal;// HttpContext.Current.User as ClaimsPrincipal;
                        IEnumerable<Claim> groupsClaimEnum = principal.Claims.Where(c => c.Type == "groups");
                        List<Claim> groupsClaim = null;
                        if (groupsClaimEnum != null)
                        {
                            groupsClaim = groupsClaimEnum.ToList();

                        }
                        try
                        {
                            if (groupsClaim != null && groupsClaim.Count > 0)
                            {
                                int iFoundGroups = 0;
                                foreach (string strGoupName in lstGroupNames)
                                {
                                    if (groupsClaim.Find(g => g.Value == strGoupName) != null)
                                    {
                                        ++iFoundGroups;
                                    }
                                    if (iFoundGroups > 0 && Policy == GroupPolicy.Any)
                                        break;
                                }

                                switch (Policy)
                                {
                                    case GroupPolicy.Any:
                                        if (iFoundGroups > 0) isAuthorized = true;
                                        else isAuthorized = false;
                                        break;
                                    case GroupPolicy.All:
                                    default:
                                        if (iFoundGroups == lstGroupNames.Count) isAuthorized = true;
                                        else isAuthorized = false;
                                        break;
                                }
                            }
                            else
                            {
                                isAuthorized = false;
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    else
                    {
                        //we specified no group on the method or class, so we'll assume the user is authorized
                        isAuthorized = true;
                    }

                }
                else
                {
                    isAuthorized = false;
                }
            }

            return isAuthorized;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var tokenHasExpired = false;
            base.HandleUnauthorizedRequest(actionContext);

            var owinContext = actionContext.Request.GetOwinContext();
            if (owinContext != null)
            {
                tokenHasExpired = owinContext.Environment.ContainsKey("oauth.token_expired");
            }
            if (tokenHasExpired)
            {
                actionContext.Response = new AuthenticationFailureMessage("unauthorized", actionContext.Request,
                    new
                    {
                        error = "invalid_token",
                        error_message = "The Token has expired"
                    });
            }
            else
            {
                if (owinContext.Authentication.User != null)
                {
                    actionContext.Response = new AuthenticationFailureMessage("unauthorized", actionContext.Request,
                        new
                        {
                            error = "validation_error",
                            error_message = string.Format("The user could be found in the JWT claims (userid: {0}) but the JWT itself is invalid, most likely because it doesn't contain the proper groups claim value.", owinContext.Authentication.User.Claims.ElementAt(4))
                        });
                }

                else
                {
                    actionContext.Response = new AuthenticationFailureMessage("unauthorized", actionContext.Request,
                        new
                        {
                            error = "invalid_user",
                            error_message = "The user could not be found, so most likely the user claims could not be extracted from the token you sent"
                        });
                }
            }
        }

    }

    public class AuthenticationFailureMessage : HttpResponseMessage
    {
        public AuthenticationFailureMessage(string reasonPhrase, HttpRequestMessage request, object responseMessage)
            : base(HttpStatusCode.Unauthorized)
        {
            MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();

            Content = new ObjectContent<object>(responseMessage, jsonFormatter);
            RequestMessage = request;
            ReasonPhrase = reasonPhrase;
        }
    }


    public enum GroupPolicy
    {
        Any,
        All
    }
}