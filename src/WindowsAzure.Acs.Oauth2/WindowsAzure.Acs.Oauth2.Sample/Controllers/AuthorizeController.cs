using System.Web.Mvc;

namespace WindowsAzure.Acs.Oauth2.Sample.Controllers
{
    [Authorize]
    public class AuthorizeController
        : AuthorizationServer
    {
    }
}
