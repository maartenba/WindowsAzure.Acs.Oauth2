using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    public interface IApplicationRegistrationService
    {
        /// <summary>
        /// Registers an application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        /// <param name="name">The name.</param>
        void RegisterApplication(string clientId, string clientSecret, string redirectUri, string name);

        /// <summary>
        /// Updates an application client secret.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        void UpdateApplicationClientSecret(string clientId, string clientSecret);

        /// <summary>
        /// Updates an application redirect Uri.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        void UpdateApplicationRedirectUri(string clientId, string redirectUri);

        /// <summary>
        /// Removes an application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        void RemoveApplication(string clientId);

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns></returns>
        string GetApplicationName(string clientId);

        /// <summary>
        /// This method validates the incoming request.
        /// </summary>
        /// <param name="message">The incoming reequest  message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">Description of the error.</param>
        /// <returns>
        /// True if request is valid, false otherwise.
        /// </returns>
        bool ValidateIncomingRequest(OAuthMessage message, out string errorCode, out string errorDescription);

        /// <summary>
        /// Checks if the client_id and the redirect_uri are valid.
        /// </summary>
        /// <param name="message">The message which contains the client_id and redirect_uri.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">Description of the error.</param>
        /// <returns>
        /// True if the client_id and the redirect_uri are valid, false otherwise
        /// </returns>
        bool ValidateServiceIdentity(OAuthMessage message, out string errorCode, out string errorDescription);

        /// <summary>
        /// Verify if a delegation exists.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="delegatedIdentity">The delegated identity.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>True if a delegation exists, false otherwise.</returns>
        bool DelegationExists(string clientId, AuthorizationServerIdentity delegatedIdentity, string scope);

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="delegatedIdentity">The delegated identity.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>The authorization code.</returns>
        string GetAuthorizationCode(string clientId, AuthorizationServerIdentity delegatedIdentity, string scope);
    }
}