# Okta ASP.NET MVC OAuth Authorization Code Flow Sample
This is a a code sample demonstrating the use of OpenID Connect with Okta and the Microsoft.Owin.Security.OpenIdConnect (for the client application) and the Microsoft.Owin.Security.OAuth (for the resource server Api) OWIN libraries.

### Setup
1. Open the __*Okta.Samples.OAuth.CodeFlow.sln*__ solution in Visual Studio 2015 and restore all NuGet packages
2. In the  __*Okta.Samples.OAuth.CodeFlow*__ project, edit the Web.config file and make the modifications to the following parameters:
 + __okta:OAuthClientId__: the Client ID value from the _General => Client Credentials_ section of your OAuth client application in Okta.
 + __okta:OauthClientSecret__: the Client Secret value from the _General => Client Credentials_ section of your OAuth client application in Okta.
 + __okta:OAuthAuthority__: This should be the Url of your Okta organization (such as _https://example.oktapreview.com"_)
 + __okta:OAuthRedirectUri__: This should be the redirect url Okta will post the authorization response to. You don't have to change that value if you are running this sample in Visual Studio with its default parameters.
__Important note__: You must register this url as a _Redirect URI_ in the _General => General Settings_ section of your Okta application.
 + __okta:OAuthScopes__: This is the list of OAuth scopes you are requesting for your user. If you have access to Okta's _API Access Management_ feature, you can include any custom scope you may have created in the _Authorization Server => OAuth 2.0 Access Token_ section of your Okta application.
__Note__: For the _offline_access_ scope to work, you must check the _Refresh Token_ checkbox in the _General => General Settings +> Allowed grant types_ section of your Okta application.
 + __okta:OAuthResponseType__: This is the response type you are requesting. You can choose either the Authorization Code flow (_code_ value) or the Hybrid flow value (_code id_token_). Note that the Microsoft.Owin.Security.OpenIdConnect OWIN library only officially supports the Hybrid flow.
 + __okta:ApiEndpoint__: This is the url of the Resource Server Api endpoint you will when clicking on the _Call API_ link in your application. You shouldn't have to modify this value if you are using the default values of the Api project in Visual Studio.
3. In the __*Api*__ project, open the Web.config file and make the following modifications:
 + __okta:TenantUrl__: the Url of your Okta organization (such as _https://example.oktapreview.com"_)
 + __okta:ClientId__: the Client ID value from the _General => Client Credentials_ section of your OAuth client application in Okta.
3. Compile and run the Okta.Samples.OAuth.CodeFlow project. It should open the sample web application at https://localhost:44327
5. Click on "Sign in with OpenID Connect" and sign in with your test user Okta credentials.
6. When you're back to the application, you may click on the "My Claims" link to view the claims retrieved from the `/oauth2/v1/userinfo` endpoint 
7. You may also use "Call API" to call your backend API.
