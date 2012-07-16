using System.Web.Mvc;

namespace WindowsAzure.Acs.Oauth2
{
    internal class EnsureOAuthMessageInterceptedAttribute
        : AuthorizeAttribute
    {
        public EnsureOAuthMessageInterceptedAttribute()
        {
            Order = -1;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var authorizationServer = filterContext.Controller as AuthorizationServer;
            if (authorizationServer != null)
            {
                authorizationServer.StoreIncomingRequest(filterContext.HttpContext);
            }
        }
    }
}