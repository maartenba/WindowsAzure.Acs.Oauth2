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
*todo*

## Usage
### Registering a client application
*todo*
