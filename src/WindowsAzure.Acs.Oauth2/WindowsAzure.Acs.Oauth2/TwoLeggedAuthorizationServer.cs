using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// Two legged AuthorizationServer class.
    /// </summary>
    public class TwoLeggedAuthorizationServer : AuthorizationServerBase
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="TwoLeggedAuthorizationServer"/> class.
        /// The parameters are read from the application configuration's appSettings keys 'WindowsAzure.OAuth.ServiceNamespace', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserName', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserKey' and 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        public TwoLeggedAuthorizationServer()
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], new ApplicationRegistrationService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoLeggedAuthorizationServer"/> class.
        /// The relying party name is read from the application configuration's appSettings key 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        /// <param name="applicationRegistrationService">The application registration service.</param>
        public TwoLeggedAuthorizationServer(IApplicationRegistrationService applicationRegistrationService)
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], applicationRegistrationService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoLeggedAuthorizationServer"/> class.
        /// </summary>
        /// <param name="relyingPartyName">The relying party name.</param>
        public TwoLeggedAuthorizationServer(string relyingPartyName, IApplicationRegistrationService applicationRegistrationService)
        {
            RelyingPartyName = relyingPartyName;
            ApplicationRegistrationService = applicationRegistrationService;
        }

        /// <summary>
        /// Gets the delegated identity. Override this if you require specifying the IdentityProvider for the delegated identity.
        /// </summary>
        /// <returns>An <see cref="AuthorizationServerIdentity"/>.</returns>
        protected override AuthorizationServerIdentity GetDelegatedIdentity()
        {
            var message = TempData[OauthMessageKey] as OAuthMessage;
            if (message != null &&
                message.Parameters[OAuthConstants.GrantType] == OAuthConstants.AccessGrantType.ClientCredentials)
            {
                return new AuthorizationServerIdentity()
                    {
                        NameIdentifier = message.Parameters[OAuthConstants.ClientId],
                        IdentityProvider = ""
                    };
            }
            
            return null;
        }

        /// <summary>
        /// Index action method.
        /// </summary>
        /// <param name="model">The AuthorizationServerViewModel model.</param>
        /// <returns>A <see cref="RedirectResult"/>.</returns>
        [HttpPost, ActionName("Index")]
        public virtual ActionResult Index_Post(AuthorizationServerViewModel model)
        {
            var message = StoreIncomingRequest(HttpContext);

            if (message != null && message.Parameters[OAuthConstants.GrantType] == OAuthConstants.AccessGrantType.ClientCredentials)
            {
                string code = ApplicationRegistrationService.GetAuthorizationCode(message.Parameters[OAuthConstants.ClientId], GetDelegatedIdentity(), message.Parameters[OAuthConstants.Scope]);
                if (code != null)
                {
                    return Redirect(message.GetCodeResponseUri(code));
                }
                else
                {
                    return Redirect(message.GetErrorResponseUri(OAuthConstants.ErrorCode.AccessDenied, "Error generating Authorization code. Please check if the Service Identity and the Replying Party are correct."));
                }
            }
            else
            {
                return Redirect(message.GetErrorResponseUri(OAuthConstants.ErrorCode.UnsupportedGrantType, "The provided grant type is not supported by this endpoint"));
            }
        }
    }
}