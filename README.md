# ASP.NET Core Web App with Custom B2C Policy

This web app is a very simple ASP.NET Core application that demonstrates how to add an Azure B2C Custom Policy to an existing application that already uses the Microsoft.Identity.Web library to handle built-in B2C User Flows.

The differences that make the Extra Custom Policy work are found in the following files:
 - appsettings.json
 - Startup.cs
 - HomeController.cs
 - \_loginPartial.cshtml


###### appsettings.json

This file is found in the root of the project directory. The important part in this file is the second Azure B2C configuration object. In this example the following code was added to line 12:

```
"AzureADB2CEditEmail": {
    "Instance": "https://markstestorganization1.b2clogin.com",
    "ClientId": "09717d12-ca7f-4388-8393-dafe42c0c3a5",
    "CallbackPath": "/signin-oidc-editemail",
    "SignedOutCallbackPath": "/signout/B2C_1_signupsignin1",
    "Domain": "markstestorganization1.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1_signupsignin1"
},
```


###### Startup.cs

This file is found in the root of the project directory. The important changes here are in the ConfigureServices function. We will be adding and configuring an Authentication Scheme for the B2C Configuration we added to the appsettings.json file. Notice in the below added code that in the AddMicrosoftIdentityWebApp function, we refer to the configuration in the appsettings.json file, then we pass a String parameter to define the name for the new Authentication Scheme, and we pass in a String parameter to define the name for this Authentication Scheme's cookie policy. These last two are arbitrary, and you can call them whatever you want, but I recommend something descriptive for better code readability.

Within the services.Configure function, there are two very crucial details: you must reference the name of the Authentication Scheme you defined in the previous line of code, and you must configure the MetadataAddress property to point towards your custom policy. You can get the metadata address from the custom policy portal within the Azure Portal.

Here is the code added to line 52:

```
// Create another authentication scheme to handle extra custom policy
services.AddAuthentication()
    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2CEditEmail"), "B2CEditEmail", "cookiesB2C");

services.Configure<OpenIdConnectOptions>("B2CEditEmail", options =>
    {
        options.MetadataAddress = "https://markstestorganization1.b2clogin.com/markstestorganization1.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1A_DEMO_CHANGESIGNINNAME";
    });
```


###### HomeController.cs

This file is found in the /Controllers folder within the project directory. The addition here is an action I have called "EditEmail" referring to the custom policy I'm using. You can name the function whatever you would like, and, of course, I recommend something description about the custom policy you are invoking. In your own application, you will need to change the name of the policy to your own custom policy (In my example app: "B2C_1A_DEMO_CHANGESIGNINNAME"), and change the name of the Authentication Context (In my example app: "B2CEditEmail"). The name of the Authentication Context comes from the Startup.cs file. Here is the code, added at line 40:

```
[Authorize]
public IActionResult EditEmail()
{
    var redirectUrl = Url.Content("~/");
    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
    properties.Items["policy"] = "B2C_1A_DEMO_CHANGESIGNINNAME";
    return Challenge(properties, "B2CEditEmail");
}
```


###### \_LoginPartial.cshtml

This file is found in the /views/shared folder within the project directory. In this file, I have added another button alongside the existing Microsoft.Identity.Web buttons. The code added to line 13 is:

```
<li class="navbar-btn">
    <form method="get" asp-area="" asp-controller="Home" asp-action="EditEmail">
        <button type="submit" class="btn btn-primary" style="margin-right:5px">Edit Email</button>
    </form>
</li>
```
