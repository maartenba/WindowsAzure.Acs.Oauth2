using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// AuthorizationServer class.
    /// </summary>
    [EnsureOAuthMessageIntercepted, Authorize]
    public class ThreeLeggedAuthorizationServer: AuthorizationServerBase
    {
     
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreeLeggedAuthorizationServer"/> class.
        /// The parameters are read from the application configuration's appSettings keys 'WindowsAzure.OAuth.ServiceNamespace', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserName', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserKey' and 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        public ThreeLeggedAuthorizationServer(): this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], new ApplicationRegistrationService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreeLeggedAuthorizationServer"/> class.
        /// The relying party name is read from the application configuration's appSettings key 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        /// <param name="applicationRegistrationService">The application registration service.</param>
        public ThreeLeggedAuthorizationServer(IApplicationRegistrationService applicationRegistrationService)
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], applicationRegistrationService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreeLeggedAuthorizationServer"/> class.
        /// </summary>
        /// <param name="relyingPartyName">The relying party name.</param>
        public ThreeLeggedAuthorizationServer(string relyingPartyName, IApplicationRegistrationService applicationRegistrationService)
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
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var identityProvider = "";
                    var identityProviderClaim = claimsIdentity.Claims.FirstOrDefault(c => c.ClaimType == "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");
                    if (identityProviderClaim != null && identityProviderClaim.Value != null)
                    {
                        identityProvider = identityProviderClaim.Value;
                    }
                    return new AuthorizationServerIdentity()
                               {
                                   NameIdentifier = claimsIdentity.Claims.First(c => c.ClaimType == ClaimTypes.NameIdentifier).Value,
                                   IdentityProvider = identityProvider
                               };
                }

                return new AuthorizationServerIdentity()
                           {
                               NameIdentifier = User.Identity.Name,
                               IdentityProvider = ""
                           };
            }
            return null;
        }

        /// <summary>
        /// Index action method. Override if needed but make sure you call <see cref="StoreIncomingRequest"/> in your code.
        /// </summary>
        /// <returns>A <see cref="ViewResult"/>.</returns>
        public virtual ActionResult Index()
        {
            var message = StoreIncomingRequest(HttpContext);

            string errorCode = "";
            string errorDescription = "";
            if (!ApplicationRegistrationService.ValidateIncomingRequest(message, out errorCode, out errorDescription))
            {
                return Redirect(message.GetErrorResponseUri(errorCode, errorDescription));
            }
            if (ApplicationRegistrationService.DelegationExists(message.Parameters[OAuthConstants.ClientId], GetDelegatedIdentity(), message.Parameters[OAuthConstants.Scope]))
            {
                return Index_Post(new AuthorizationServerViewModel { Authorize = true });
            }

            return View("_AuthorizationServer", BuildModel(message));
        }


        /// <summary>
        /// Index action method.
        /// </summary>
        /// <param name="model">The AuthorizationServerViewModel model.</param>
        /// <returns>A <see cref="RedirectResult"/>.</returns>
        [HttpPost, ActionName("Index")]
        public virtual ActionResult Index_Post(AuthorizationServerViewModel model)
        {
            var message = TempData[OauthMessageKey] as OAuthMessage;

            if (model.Authorize)
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
                return Redirect(message.GetErrorResponseUri(OAuthConstants.ErrorCode.AccessDenied, "The end user has denied consent to access the requested resource"));
            }
        }
    }
}
