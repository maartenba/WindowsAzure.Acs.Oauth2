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
                GlobalConfiguration.Configuration.MessageHandlers.Add(new OAuth2MessageHandler());
            }
            else
            {
                throw new ArgumentException("To enable OAuth2 support for your web project, configure WindowsAzure.OAuth.RelyingPartyRealm, WindowsAzure.OAuth.ServiceNamespace and WindowsAzure.OAuth.SwtSigningKey in your applications's appSettings.");
            }
        }
    }
}