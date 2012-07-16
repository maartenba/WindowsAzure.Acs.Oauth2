using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Claims;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// AuthorizationServer class.
    /// </summary>
    [Authorize]
    public class AuthorizationServer
        : Controller
    {
        private const string OauthMessageKey = "__AuthorizationServer_OAuthMessage";

        /// <summary>
        /// Gets or sets the relying party realm.
        /// </summary>
        /// <value>
        /// The relying party realm.
        /// </value>
        protected string RelyingPartyName { get; set; }

        /// <summary>
        /// Gets or sets the application registration service.
        /// </summary>
        /// <value>
        /// The application registration service.
        /// </value>
        protected IApplicationRegistrationService ApplicationRegistrationService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServer"/> class.
        /// The parameters are read from the application configuration's appSettings keys 'WindowsAzure.OAuth.ServiceNamespace', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserName', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserKey' and 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        public AuthorizationServer()
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], new ApplicationRegistrationService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServer"/> class.
        /// The relying party name is read from the application configuration's appSettings key 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        /// <param name="applicationRegistrationService">The application registration service.</param>
        public AuthorizationServer(IApplicationRegistrationService applicationRegistrationService)
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], applicationRegistrationService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServer"/> class.
        /// </summary>
        /// <param name="relyingPartyName">The relying party name.</param>
        public AuthorizationServer(string relyingPartyName, IApplicationRegistrationService applicationRegistrationService)
        {
            RelyingPartyName = relyingPartyName;
            ApplicationRegistrationService = applicationRegistrationService;
        }

        /// <summary>
        /// This method parses the incoming request and creates an OAuth message from it.
        /// </summary>
        /// <param name="httpContext"> The current HttpContext.</param>
        /// <returns>Returns the OAuth message created from the incoming request.</returns>
        protected virtual OAuthMessage ParseIncomingRequest(HttpContextBase httpContext)
        {
            var serializer = new OAuthMessageSerializer();
            var message = serializer.Read(httpContext);
            return message;
        }

        /// <summary>
        /// Stores the incoming request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Returns the OAuth message created from the incoming request.</returns>
        protected OAuthMessage StoreIncomingRequest(HttpContextBase httpContext)
        {
            var message = ParseIncomingRequest(HttpContext);
            TempData[OauthMessageKey] = message;
            return message;
        }

        /// <summary>
        /// Builds the model. Override this to add information about the application requesting user consent, such as publisher information or a logo URL.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the model.</returns>
        protected virtual AuthorizationServerViewModel BuildModel(OAuthMessage message)
        {
            var model = new AuthorizationServerViewModel();
            model.ApplicationName = ApplicationRegistrationService.GetApplicationName(message.Parameters["client_id"]);
            model.ApplicationUrl = message.Parameters["redirect_uri"] != null ? new Uri(message.Parameters["redirect_uri"]) : null;
            return model;
        }

        /// <summary>
        /// Gets the delegated identity. Override this if you require specifying the IdentityProvider for the delegated identity.
        /// </summary>
        /// <returns>An <see cref="AuthorizationServerIdentity"/>.</returns>
        protected virtual AuthorizationServerIdentity GetDelegatedIdentity()
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    return new AuthorizationServerIdentity()
                               {
                                   NameIdentifier = claimsIdentity.Claims.First(c => c.ClaimType == ClaimTypes.NameIdentifier).Value,
                                   IdentityProvider = ""
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
