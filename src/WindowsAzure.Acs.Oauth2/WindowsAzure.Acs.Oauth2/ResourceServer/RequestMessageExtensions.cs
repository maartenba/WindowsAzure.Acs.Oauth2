using System.Net.Http;
using System.Web;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public static class RequestMessageExtensions
    {
        const string HttpContextKey = "MS_HttpContext";

        public static bool TryGetHttpContext(this HttpRequestMessage requestMessage, out HttpContextBase httpContext)
        {
            httpContext = null;
            object obj;
            if (requestMessage.Properties.TryGetValue(HttpContextKey, out obj))
            {
                httpContext = (HttpContextBase) obj;
            }

            return httpContext != null;
        }
    }
}