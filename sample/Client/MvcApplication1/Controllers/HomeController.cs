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
            var clientId = "yourclientid";
            var clientSecret = "yoursecret";
            var redirectUri = "http://yourredirecturi/";
            var scope = "http://yourscope/";
            var authServer = "http://localhost/authorize";
            var apiRoot = "http://localhost/yourapi/";
            var accessTokenServer = "https://youracsnamespace-prod.accesscontrol.windows.net/v2/OAuth2-13/";
            
            var client = new SimpleOAuth2Client(new Uri(authServer), new Uri(accessTokenServer), clientId, clientSecret, scope, new Uri(redirectUri), ClientMode.TwoLegged);
            client.Authorize();

            var req = WebRequest.CreateHttp(apiRoot);
            client.AppendAccessTokenTo(req);
            
            var response = req.GetResponse();

            return View();
        }

    }
}
