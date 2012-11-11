using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public interface IAuthenticationStep
    {
        IClaimsPrincipal Authenticate(SecurityToken token, IClaimsPrincipal claimsPrincipal);
    }
}