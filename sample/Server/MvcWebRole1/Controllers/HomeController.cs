using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentACS.ManagementService;
using WindowsAzure.Acs.Oauth2;

namespace MvcWebRole1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            var applicationRegistrationService = new ApplicationRegistrationService();
            try
            {
               //applicationRegistrationService.RemoveApplication("messagehandlertestclient");
                applicationRegistrationService.RegisterApplication("messagehandlertestclient", "0A912A80-C3BD-45CE-92B7-CBDE2B39DD60", "http://messagehandler/testclient", "Messagehandler Windows 8 client");
                applicationRegistrationService.RegisterApplication("12345", "12345", "http://messagehandler/12345", "12345");
            }
            catch
            {
                // Pokemon handler: Gotta catch em all!
            }

            return Content("All set!");
        }
    }
}
