using System.Web.Mvc;
using WindowsAzure.Acs.Oauth2;

namespace $rootnamespace$.Controllers
{
    [Authorize]
    public class AuthorizeController
        : AuthorizationServer
    {
    }
}
