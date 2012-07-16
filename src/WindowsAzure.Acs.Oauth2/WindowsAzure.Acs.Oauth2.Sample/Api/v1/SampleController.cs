using System.Collections.Generic;
using System.Web.Http;
using Microsoft.IdentityModel.Claims;

namespace WindowsAzure.Acs.Oauth2.Sample.Api.v1
{
    [Authorize]
    public class SampleController
        : ApiController
    {
        public string Get()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            List<string> claims = new List<string>();
            foreach (var c in claimsIdentity.Claims)
            {
                claims.Add(c.ClaimType + " - " + c.Value);
            }

            return string.Format("Hello, world! And hello, {0}.\r\n\r\nYour claims:\r\n{1}", User.Identity.Name, string.Join("\r\n", claims));
        }
    }
}