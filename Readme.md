# ASP.NET MVC + Okta Authorization Code Flow Sample
This is a a code sample demonstrating the use of OpenID Connect with Okta and the Microsoft.Owin.Security.OpenIdConnect (for the client application) and the Microsoft.Owin.Security.OAuth (for the resource server API) OWIN libraries.

To get this sample running, follow these instructions:

### Okta setup

0. If you haven't already, sign up for an [Okta developer account](https://www.okta.com/developer/signup). Once you sign in, click on the Admin button in the top-right corner of the dashboard.
0. Create a new application (Applications - Add Application - Create New App - Web/OpenID Connect). Type a name for the application and click Next.
0. Click Add URI and copy the Redirect URI from the `okta:OAuthRedirectUri` property in the CodeFlow project's Web.config file (the default is `https://localhost:44327/Callback`).
0. Click Finish to create the Okta application.
0. Switch to the People tab and assign at least one user to the application (you'll log in with this user later).
0. Switch to the General tab. Click Edit next to General Settings, and enable the Implicit (Hybrid) checkbox. This will allow you to get an ID Token in addition to an Access Token.


### Application setup
1. Open the solution in Visual Studio 2015 (or later).
2. In the Web.config file for the CodeFlow project, edit these values:

| Property | Change to: |
| -------- | ----------- |
| **okta:OAuthClientId** | The client ID from the Client Credentials section of your Okta application |
| **okta:OauthClientSecret** | The client secret from the Client Credentials section of your Okta application |
| **okta:OAuthAuthority** | The URL of your Okta organization (such as `https://dev-12345.oktapreview.com`) |

> :bulb: The org URL should have the format `dev-*.oktapreview.com`. Make sure you don't copy the `dev-*-admin` URL!

3. Next, open the Web.config file for the Api project and edit these values:

| Property | Change to: |
| -------- | ----------- |
| **okta:ClientId** | The client ID from the Client Credentials section of your Okta application |
| **okta:TenantUrl** | The URL of your Okta organization (such as `https://dev-12345.oktapreview.com`) |


4. Right-click on the CodeFlow project and select **Set as startup project**.
0. Choose Rebuild All to build the solution and restore any missing packages.
0. Run the solution. It should open the sample web application at https://localhost:44327. The Api project (a sample resource server) will also start up at https://localhost:44316.
0. Click on **Sign in with OpenID Connect** and sign in with your test user Okta credentials.
0. When you're back to the application, click on the **My Claims** link to view the claims retrieved from the `/oauth2/v1/userinfo` endpoint.
0. Click the **Call API** link to call your resource server using the user's access token.
