using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WindowsAzure.Acs.Oauth2.Client;

namespace MvcApplication1.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var clientId = "messagehandlertestclient";
            var clientSecret = "0A912A80-C3BD-45CE-92B7-CBDE2B39DD60";
            var redirectUri = "http://messagehandler/testclient";
            var scope = "http://api.messagehandler.net/";
            var authServer = "http://localhost:26073/authorize";
            var apiRoot = "http://localhost:26073/values/";
            var accessTokenServer = "https://messagehandler-acs-eu-west-prod.accesscontrol.windows.net/v2/OAuth2-13/";
            
            var client = new SimpleOAuth2Client(new Uri(authServer), new Uri(accessTokenServer), clientId, clientSecret, scope, new Uri(redirectUri), ClientMode.TwoLegged);
            client.Authorize();

            var req = WebRequest.CreateHttp(apiRoot);
            client.AppendAccessTokenTo(req);
            
            var response = req.GetResponse();

            return View();
        }

    }
}
