using System.Web;
using System.Web.Mvc;

namespace WindowsAzure.Acs.Oauth2.Sample
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}