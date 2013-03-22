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
               //applicationRegistrationService.RemoveApplication("yourclientid");
                applicationRegistrationService.RegisterApplication("yourclientid", "yourclientsecret", "http://yourrealm/", "Just a description");
                            }
            catch
            {
                // Pokemon handler: Gotta catch em all!
            }

            return Content("All set!");
        }
    }
}
