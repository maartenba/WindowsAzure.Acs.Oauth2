using System;
using System.Configuration;
using System.Web.Http;
using WindowsAzure.Acs.Oauth2.ResourceServer;
using $rootnamespace$.App_Start;

[assembly: WebActivator.PostApplicationStartMethod(typeof(AppStart_OAuth2API), "Start")]

namespace $rootnamespace$.App_Start
{
    // ReSharper disable InconsistentNaming
    public static class AppStart_OAuth2API
    // ReSharper restore InconsistentNaming
    {
        public static void Start()
        {
            if (ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyRealm"] != null && ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespace"] != null && ConfigurationManager.AppSettings["WindowsAzure.OAuth.SwtSigningKey"] != null)
            {
				try
				{
					Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(AcsAuthenticationModule));
				}
				catch (InvalidOperationException)
				{
					throw new ArgumentException("The AcsAuthenticationModule could not be registered for your application. Remove the AppStart_OAuth2API.cs file from your project and add the following entry under the system.webServer.httpModules section in Web.config: <add name=\"AcsAuthenticationModule\" type=\"WindowsAzure.Acs.Oauth2.ResourceServer.AcsAuthenticationModule, WindowsAzure.Acs.Oauth2\" />");
				}
            }
            else
            {
                throw new ArgumentException("To enable OAuth2 support for your web project, configure WindowsAzure.OAuth.RelyingPartyRealm, WindowsAzure.OAuth.ServiceNamespace and WindowsAzure.OAuth.SwtSigningKey in your applications's appSettings.");
            }
        }
    }
}