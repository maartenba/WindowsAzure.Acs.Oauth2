using System.Configuration;
using System.Web;
using System.Web.Mvc;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// AuthorizationServer class.
    /// </summary>
    [EnsureOAuthMessageIntercepted]
    public abstract class AuthorizationServerBase : Controller
    {
        protected const string OauthMessageKey = "__AuthorizationServer_OAuthMessage";

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
        /// Initializes a new instance of the <see cref="AuthorizationServerBase"/> class.
        /// The parameters are read from the application configuration's appSettings keys 'WindowsAzure.OAuth.ServiceNamespace', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserName', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserKey' and 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        protected AuthorizationServerBase()
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], new ApplicationRegistrationService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServerBase"/> class.
        /// The relying party name is read from the application configuration's appSettings key 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        /// <param name="applicationRegistrationService">The application registration service.</param>
        protected AuthorizationServerBase(IApplicationRegistrationService applicationRegistrationService)
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"], applicationRegistrationService)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServerBase"/> class.
        /// </summary>
        /// <param name="relyingPartyName">The relying party name.</param>
        protected AuthorizationServerBase(string relyingPartyName, IApplicationRegistrationService applicationRegistrationService)
        {
            RelyingPartyName = relyingPartyName;
            ApplicationRegistrationService = applicationRegistrationService;
        }

        /// <summary>
        /// Stores the incoming request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Returns the OAuth message created from the incoming request.</returns>
        public OAuthMessage StoreIncomingRequest(HttpContextBase httpContext)
        {
            var message = ParseIncomingRequest(HttpContext);
            TempData[OauthMessageKey] = message;
            return message;
        }

        /// <summary>
        /// This method parses the incoming request and creates an OAuth message from it.
        /// </summary>
        /// <param name="httpContext"> The current HttpContext.</param>
        /// <returns>Returns the OAuth message created from the incoming request.</returns>
        public virtual OAuthMessage ParseIncomingRequest(HttpContextBase httpContext)
        {
            if (TempData[OauthMessageKey] != null)
            {
                TempData.Keep(OauthMessageKey);
                return TempData[OauthMessageKey] as OAuthMessage;
            }

            var serializer = new OAuthMessageSerializer();
            var message = serializer.Read(httpContext);
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

            var applicationRegistration = ApplicationRegistrationService.GetApplication(message.Parameters["client_id"]);

            model.ApplicationName = applicationRegistration.ApplicationName;
            model.ApplicationUrl = applicationRegistration.ApplicationUrl;

            return model;
        }

        /// <summary>
        /// Gets the delegated identity. Override this if you require specifying the IdentityProvider for the delegated identity.
        /// </summary>
        /// <returns>An <see cref="AuthorizationServerIdentity"/>.</returns>
        protected abstract AuthorizationServerIdentity GetDelegatedIdentity();
       

    }
}