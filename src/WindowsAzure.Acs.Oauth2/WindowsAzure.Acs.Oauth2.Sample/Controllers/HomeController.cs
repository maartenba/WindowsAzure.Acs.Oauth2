using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WindowsAzure.Acs.Oauth2.Client;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2.Sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string code, string error)
        {
            string authorizeUri = "http://localhost:31875/authorize";
            string clientId = "testclient3";
            string clientSecret = "testsecret";
            string redirectUri = "http://localhost:31875/";
            string scope = "http://localhost:31875/";

            // Register the app (this should be done elsewhere!)
            try
            {
                var x = new ApplicationRegistrationService();
                x.RegisterApplication(clientId, clientSecret, redirectUri, clientId);
            }
            catch
            {
            }

            var client = new SimpleOAuth2Client(
                new Uri(authorizeUri),
                new Uri("https://brewbuddy-prod.accesscontrol.windows.net/v2/OAuth2-13/"),
                clientId,
                clientSecret,
                scope,
                new Uri(redirectUri));

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(error))
            {
                return Redirect(client.BuildAuthorizationUri().ToString());
            }

            client.Authorize(code);

            HttpWebRequest webRequest = HttpWebRequest.Create(new Uri("http://localhost:31875/api/v1/Sample")) as HttpWebRequest;
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.ContentLength = 0;
            client.AppendAccessTokenTo(webRequest);

            var responseText = "";
            try
            {
                var response = webRequest.GetResponse();
                responseText = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException wex)
            {
                responseText = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
            }

            return Content(responseText);
        }
    }
}
