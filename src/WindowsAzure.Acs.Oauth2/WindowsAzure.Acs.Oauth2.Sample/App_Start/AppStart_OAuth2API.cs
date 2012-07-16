using System;
using System.Configuration;
using System.Web.Http;
using WindowsAzure.Acs.Oauth2.ResourceServer;
using WindowsAzure.Acs.Oauth2.Sample.App_Start;

[assembly: WebActivator.PostApplicationStartMethod(typeof(AppStart_OAuth2API), "Start")]

namespace WindowsAzure.Acs.Oauth2.Sample.App_Start
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
                throw new ArgumentException("To enable OAuth2 support for your web project, add a call to GlobalConfiguration.Configuration.MessageHandlers.Add(new OAuth2MessageHandler(\"\", \"\", \"\"));.");
            }
        }
    }
}