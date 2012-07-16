WindowsAzure.Acs.Oauth2
=======================

Oauth2 delegation through Windows Azure Access Control Service for ASP.NET Web API.

## Concept
The concept is simple: you, as an API creator, don't want to be bothered by access tokens and refresh tokens. You want to delegate that to Windows Azure Access Control Service, while still having the possibility to revoke client application access to a specific user's data or in whole.

WindowsAzure.Acs.Oauth2 solves just that. Here's the flow for any client to your API:

0. Register an application and receive a client_id, client_secret and redirect_uri. This step will take place once per application on your site.

    Registering a new client application can be done using the *ApplicationRegistrationService* class:

	var registrationService = new ApplicationRegistrationService();
	registrationService.RegisterApplication(clientId, clientSecret, redirectUri, name);
	
    This call registers the application identified by *client_id* and *client_secret* with a base URL *redirectUrl* and a *name* as an application which can access your application under a given user's name.

    This can then be used by the application developer to ask your users consent and to perform operations delegated by them.

1. Redirect users to request access to your API - This step will take at your site.

    We install an *AuthorizeController* in your application which will require the user to log in at your site and he/she will have to grant or deny access to your API.
    
2. API redirects users back to the client application.

3. The client application will make use of Windows Azure ACcess Control Service from now on to request access tokens to your API.

4. Your API will retrieve a ClaimsIdentity from WindowsAzure.Acs.Oauth2 whenever a valid OAuth2 - SWT token comes in from the client appliation. You can focus on your API:

	    [Authorize]	    
	    public class SampleController
	        : ApiController
	    {
	        public string Get()
	        {
	            var claimsIdentity = (ClaimsIdentity)User.Identity;
	            List<string> claims = new List<string>();
	            foreach (var c in claimsIdentity.Claims)
	            {
	                claims.Add(c.ClaimType + " - " + c.Value);
	            }	            
	            return string.Format("Hello, world! And hello, {0}.\r\n\r\nYour claims:\r\n{1}",
    	                User.Identity.Name, string.Join("\r\n", claims));
	        }
	    }

## Installation
### Installation in an ASP.NET Web API project
Installing *WindowsAzure.Acs.Oauth2* can be done using [NuGet](http://www.nuget.org).

As *WindowsAzure.Acs.Oauth2* is currently in alpha status, you will have to register this package in your ASP.NET MVC Web API project using the package manager console, issuing the following command:

	Install-Package WindowsAzure.Acs.Oauth2 -IncludePrerelease

This command will bring some dependencies to your project and installs the following source files:
- App_Start/AppStart_OAuth2API.cs - Makes sure that OAuth2-signed SWT tokenas are transformed into a *ClaimsIdentity* for use in your API.

- Controllers/[AuthorizeController]().cs - A standard authorization server implementation which is configured by the *Web.config* settings. You can override certain methods here, for example if you want to show additional application information on the consent page.

- Views/Shared/_AuthorizationServer.cshtml - A default consent page. This can be customized at will.

Next to these files, the following entries are added to your *Web.config* file:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <appSettings>
	    <add key="WindowsAzure.OAuth.SwtSigningKey" value="[your 256-bit symmetric key configured in the ACS]" />
	    <add key="WindowsAzure.OAuth.RelyingPartyName" value="[your relying party name configured in the ACS]" />
	    <add key="WindowsAzure.OAuth.RelyingPartyRealm" value="[your relying party realm configured in the ACS]" />
	    <add key="WindowsAzure.OAuth.ServiceNamespace" value="[your ACS service namespace]" />
	    <add key="WindowsAzure.OAuth.ServiceNamespaceManagementUserName" value="ManagementClient" />
	    <add key="WindowsAzure.OAuth.ServiceNamespaceManagementUserKey" value="[your ACS service management key]" />
	  </appSettings>
	</configuration>
    
These settings should be configured based on the Windows Azure Access Control settings.

### Windows Azure Access Control Settings
Ensure you have a Windows Azure Access Control namespace created through the [Windows Azure Management portal](http://windows.azure.com). Log in to the ACS management dashboard and configure the following:

- Under *Relying Party Applications*, create a new Relying Party Application. A Relying Party Application is an application which trusts the OAuth2 tokens created by ACS. In most cases, the project in which your API resides will be a Relying Party.

    Give it a meaningful name (which you can add in the *WindowsAzure.OAuth.RelyingPartyName* setting of your *Web.config*.
    
    Configure the Realm. This is can be any URL you want, but is usually identical to the root URL of your API. Also add it in the *WindowsAzure.OAuth.RelyingPartyRealm* setting of your *Web.config*.
    
    For Return URL, enter the root URL of your API.
    
    The Token Format to be used is SWT. Define the token lifetime to any value you want. Many API's use 5 minutes (300 sec), more works too.
    
    Create a token signing key. This can be generated and added in the *WindowsAzure.OAuth.SwtSigningKey* setting of your *Web.config*. Token validity is your call. Any date will do, but note that if the tokn expires you will have to update your API's *Web.config* again.
    
    Create a new Rule Group.
    
    Save.
- Under *Rule Groups*, find the Rule Group that was just created. It is important to add one Rule wich passes through any information related to a user to your API.

    Click *Add*, for Input Claim Issuer select *Access Control Service*.
    
    Leave all other fields as-is. This will pass-through any information related to the user to your application.
    
    Save.
    
    Under *Edit Rule Group*, Save again.
    
Your ACS configuration is now completed. However, our *Web.config* configuration isn't. Keep the ACS management dashboard open and find the following values:

- Find your Service Namespace. This is usually the part before .accesscontrol.windows.net in the URL. Enter it in the *WindowsAzure.OAuth.ServiceNamespace* setting of your *Web.config*. 

- Under Management Service, find the management service owner name. Usually, this is *ManagementClient* and it is already configured in the *WindowsAzure.OAuth.ServiceNamespaceManagementUserName* setting of your *Web.config*. 

- Find the management service password. Copy it into the *WindowsAzure.OAuth.ServiceNamespaceManagementUserKey* setting of your *Web.config*. 

## API application flow
You can use the following entries in your API documentation as these are the defaults for WindowsAzure.Acs.Oauth2.

This is a description of the OAuth flow from 3rd party web sites.

### 1. Redirect users to request access to your API

	GET http(s)://url.to.our.api/authorize

#### Parameters
 - **client_id**
 
     *Required* string - The client ID you received to access our API.
 - **redirect_uri**
 
     *Required* string - The redirect URI you registered to access our API. This can **not** be different from the registration with our API.
 - **scope**
 
     *Required* string - The API access scope you wish to receive. This is typically the root URL of the API.
     
### 2. API redirects users back to your site (using the redirect_uri)
If the user accepts your request, we redirect you back to your site with a temporary code in a code parameter, for example:

	GET http(s)://your.application.site/?code=temporarycode

Exchange this temporary code for an access token at our access token URL endpoint:

	POST https://<namespace>.accesscontrol.windows.net/v2/OAuth2-13/

#### Parameters
 - **client_id**
 
     *Required* string - The client ID you received to access our API.
 - **client_secret**
 
     *Required* string - The client secret you received to access our API.
 - **redirect_uri**
 
     *Required* string - The redirect URI you registered to access our API. This can **not** be different from the registration with our API.
 - **scope**
 
     *Required* string - The API access scope requested initially. This is typically the root URL of the API.
 - **grant_type**
 
     *Required* string - Value: *authorization_code*
 - **code**
 
     *Required* string - The temporary code you have received from our authorization server in the previous step.
     
The access token will be returned as JSON, for example:

	{"access_token": ".......", "refresh_token": "......"}
	
This token contains several parameters, such as the token lifetime and a refresh token value. 

Whenever the token lifetime expires, you can request a fresh access token by exchanging the refresh token for an access token at our access token URL endpoint. Note that this will also return a fresh refresh token which should be used in subsequent token refreshes.

### 3. Use the access token received to access our API
The access token allows you to make requests to the API on a behalf of a user. You can access our API by adding the *access_token* value as a base64 string to the authorization header as a token bearer:

	GET http(s)://url.to.our.api/api/v1/user
	Authorization: Bearer <base64-encoded access_token value>
