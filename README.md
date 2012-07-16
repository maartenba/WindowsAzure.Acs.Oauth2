WindowsAzure.Acs.Oauth2
=======================

Oauth2 delegation through Windows Azure Access Control Service for ASP.NET Web API.

## Concept
*todo*

## Installation
### Installation in an ASP.NET Web API project
Installing *WindowsAzure.Acs.Oauth2* can be done using [NuGet](http://www.nuget.org).

As *WindowsAzure.Acs.Oauth2* is currently in alpha status, you will have to register this package in your ASP.NET MVC Web API project using the package manager console, issuing the following command:

	Install-Package WindowsAzure.Acs.Oauth2 -IncludePrerelease

This command will bring some dependencies to your project and installs the following source files:
- App_Start/AppStart_OAuth2API.cs - Makes sure that OAuth2-signed SWT tokenas are transformed into a *ClaimsIdentity* for use in your API.

- Controllers/AuthorizeController.cs - A standard authorization server implementation which is configured by the *Web.config* settings. You can override certain methods here, for example if you want to show additional application information on the consent page.

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

## Usage
### Registering a client application
*todo*
