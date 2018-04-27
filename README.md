# ASP.NET MVC + Okta

This example shows how to use Okta, OpenID Connect, and ASP.NET MVC 4.x+.

You can follow the **[ quickstart](https://developer.okta.com/quickstart/#/okta-sign-in-page/dotnet/aspnet4)** for this project to see how it was created.

**Prerequisites:** [Visual Studio](https://www.visualstudio.com/downloads/) and Windows.

> [Okta](https://developer.okta.com/) has Authentication and User Management APIs that reduce development time with instant-on, scalable user infrastructure. Okta's intuitive API and expert support make it easy for developers to authenticate, manage and secure users and roles in any application.

* [Getting started](#getting-started)
* [Links](#links)
* [Help](#help)
* [License](#license)

## Getting started

To install this example application, clone this repository with Git:

```bash
git clone https://github.com/oktadeveloper/okta-aspnet-mvc-example.git
```

Or download a zip archive of the repository from GitHub and extract it on your machine.

### Create an application in Okta

You will need to create an application in Okta to perform authentication. 

Log in to your Okta Developer account (or [sign up](https://developer.okta.com/signup/) if you don’t have an account) and navigate to **Applications** > **Add Application**. Click **Web**, click **Next**, and give the app a name you’ll remember.

Change the **Base URI** to:

```
http://localhost:8080/
```

Change the **Login redirect URI** to:

```
http://localhost:8080/authorization-code/callback
```

Click **Done**. On the General Settings screen, click **Edit**.

Check **Implicit (Hybrid)** and **Allow ID Token**. Add a **Logout redirect URI**:

```
http://localhost:8080/Account/PostLogout
```

**Note**: You can run the project in Visual Studio to see the port it is assigned on your machine. It may be different than 8080. In that case, you'll need to update the URIs in Okta.

Scroll to the bottom of the Okta application page to find the client ID and client secret. You'll need those values in the next step.

### Project configuration

Open the `Web.config` file and update these values:

* `okta:ClientId` - The client ID of the Okta application
* `okta:ClientSecret` - The client secret of the Okta application
* `okta:OrgUri` - Replace `{yourOktaDomain}` with your Okta domain, found at the top-right of the Dashboard page.

**Note:** The value of `{yourOktaDomain}` should be something like `dev-123456.oktapreview.com`. Make sure you don't include `-admin` in the value!

### Start the application

Use Visual Studio to run the project. It should start up on `http://localhost:8080`. (If it doesn't, update the URLs in Okta and in `Web.config`)

## Links

* [ASP.NET + Okta authentication quickstart](https://developer.okta.com/quickstart/#/okta-sign-in-page/dotnet/aspnet4)
* Use the [Okta .NET SDK](https://github.com/okta/okta-sdk-dotnet) if you need to call [Okta APIs](https://developer.okta.com/docs/api/resources/users) for management tasks

## Help

Please post any questions on the [Okta Developer Forums](https://devforum.okta.com/). You can also email developers@okta.com if you would like to create a support ticket.

## License

Apache 2.0, see [LICENSE](LICENSE).
