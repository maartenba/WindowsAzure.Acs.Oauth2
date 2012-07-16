using System.Web;
using System.Web.Optimization;

namespace WindowsAzure.Acs.Oauth2.Sample
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));
        }
    }
}