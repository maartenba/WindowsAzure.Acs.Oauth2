using System;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Web;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public class IdentityFoundationAuthenticationStep
        : IAuthenticationStep
    {
        public IClaimsPrincipal Authenticate(SecurityToken token, IClaimsPrincipal claimsPrincipal)
        {
            // push it through the WIF pipeline
            try
            {
                var cam = FederatedAuthentication.ServiceConfiguration.ClaimsAuthenticationManager;
                if (cam != null)
                {
                    claimsPrincipal = cam.Authenticate("", claimsPrincipal);
                }
            }
            catch (NullReferenceException)
            {
                // swallow intentionally
            }
            return claimsPrincipal;
        }
    }
}