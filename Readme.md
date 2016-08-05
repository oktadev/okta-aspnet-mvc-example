# Okta ASP.NET MVC OAuth Authorization Code Flow Sample
This is a a code sample demonstrating the use of OpenID Connect with Okta and the Microsoft.Owin.Security.OpenIdConnect library.

###Setup
1. Open the Okta.Samples.OAuth.CodeFlow.sln solution in Visual Studio 2015 and restore all NuGet packages
2. In the  Microsoft.Owin.Security.OpenIdConnect project, remove the reference to the Microsoft.Owin.Security.OpenIdConnect assemblu, then compile the project
3. In the Okta.Samples.OAuth.CodeFlow project, verify that the reference to the Microsoft.Owin.Security.OpenIdConnect assembly points to the Microsoft.Owin.Security.OpenIdConnect project, not the NuGet package.
4. Compile and run the Okta.Samples.OAuth.CodeFlow project. It should open the sample web application at https://localhost:44327
5. Click on "Sign in with OpenID Connect" and sign in with the following Okta credentials:

Username: bob
Password: pass

6. When you're back to the application, you may click on the "My Claims" link to view the claims retrieved from the /oauth2/v1/userinfo endpoint
