using System;
using System.Configuration;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using FluentACS.ManagementService;
using WindowsAzure.Acs.Oauth2.Protocol;

namespace WindowsAzure.Acs.Oauth2
{
    public class ApplicationRegistrationService
        : IApplicationRegistrationService
    {
        public string ServiceNamespace { get; protected set; }
        public string ServiceNamespaceManagementUserName { get; protected set; }
        public string ServiceNamespaceManagementUserKey { get; protected set; }
        public string RelyingPartyName { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRegistrationService"/> class.
        /// The parameters are read from the application configuration's appSettings keys 'WindowsAzure.OAuth.ServiceNamespace', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserName', 'WindowsAzure.OAuth.ServiceNamespaceManagementUserKey' and 'WindowsAzure.OAuth.RelyingPartyName'.
        /// </summary>
        public ApplicationRegistrationService()
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespace"], ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespaceManagementUserName"], ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespaceManagementUserKey"], ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyName"])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRegistrationService"/> class.
        /// </summary>
        /// <param name="serviceNamespace">The service namespace.</param>
        /// <param name="serviceNamespaceManagementUserName">Name of the service namespace management user.</param>
        /// <param name="serviceNamespaceManagementUserKey">The service namespace management user key.</param>
        /// <param name="relyingPartyName">The relying party name.</param>
        public ApplicationRegistrationService(string serviceNamespace, string serviceNamespaceManagementUserName, string serviceNamespaceManagementUserKey, string relyingPartyName)
        {
            ServiceNamespace = serviceNamespace;
            ServiceNamespaceManagementUserName = serviceNamespaceManagementUserName;
            ServiceNamespaceManagementUserKey = serviceNamespaceManagementUserKey;
            RelyingPartyName = relyingPartyName;
        }


        /// <summary>
        /// Registers an application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        /// <param name="name">The name.</param>
        public void RegisterApplication(string clientId, string clientSecret, string redirectUri, string name)
        {
            var client = CreateManagementServiceClient();

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity != null)
            {
                throw new InvalidOperationException(string.Format(
                    "An application with client_id '{0}' already exists.", clientId));
            }

            // Create a service identity
            serviceIdentity = new ServiceIdentity
                                  {
                                      Name = clientId,
                                      Description = name,
                                      RedirectAddress = redirectUri
                                  };
            var serviceIdentityKey = new ServiceIdentityKey
                                         {
                                             DisplayName = string.Format("Credentials for {0}", clientId),
                                             Value = Encoding.UTF8.GetBytes(clientSecret),
                                             Type = IdentityKeyTypes.Password.ToString(),
                                             Usage = IdentityKeyUsages.Password.ToString(),
                                             StartDate = DateTime.UtcNow,
                                             EndDate = DateTime.UtcNow.AddYears(100) // Validity 100 years. After that?
                                         };

            // Process modifications to the namespace
            client.AddToServiceIdentities(serviceIdentity);
            client.AddRelatedObject(serviceIdentity, "ServiceIdentityKeys", serviceIdentityKey);
            client.SaveChanges(SaveChangesOptions.Batch);
        }

        /// <summary>
        /// Creates the management service client.
        /// </summary>
        /// <returns></returns>
        protected ManagementService CreateManagementServiceClient()
        {
            var serviceManagementWrapper = new ServiceManagementWrapper(ServiceNamespace, ServiceNamespaceManagementUserName, ServiceNamespaceManagementUserKey);
            var client = serviceManagementWrapper.CreateManagementServiceClient();
            client.IgnoreResourceNotFoundException = true;
            return client;
        }

        /// <summary>
        /// Updates an application client secret.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        public void UpdateApplicationClientSecret(string clientId, string clientSecret)
        {
            var client = CreateManagementServiceClient();

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException(string.Format(
                    "An application with client_id '{0}' could not be found.", clientId));
            }

            var serviceIdentityKeys = client.ServiceIdentityKeys.Where(k => k.ServiceIdentityId == serviceIdentity.Id);
            var serviceIdentityKey = serviceIdentityKeys.FirstOrDefault(k => k.Type == IdentityKeyTypes.Password.ToString());
            if (serviceIdentityKey == null)
            {
                serviceIdentityKey = new ServiceIdentityKey
                {
                    DisplayName = string.Format("Credentials for {0}", clientId),
                    Value = Encoding.UTF8.GetBytes(clientSecret),
                    Type = IdentityKeyTypes.Password.ToString(),
                    Usage = IdentityKeyUsages.Password.ToString(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(100) // Validity 100 years. After that?
                };

                client.AddToServiceIdentities(serviceIdentity);
                client.AddRelatedObject(serviceIdentity, "ServiceIdentityKeys", serviceIdentityKey);
            }
            else
            {
                serviceIdentityKey.Value = Encoding.UTF8.GetBytes(clientSecret);
                serviceIdentityKey.EndDate = DateTime.UtcNow.AddYears(100); // Validity 100 years. After that?
            }

            // Process modifications to the namespace
            client.SaveChanges(SaveChangesOptions.Batch);
        }

        /// <summary>
        /// Updates an application redirect Uri.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        public void UpdateApplicationRedirectUri(string clientId, string redirectUri)
        {
            var client = CreateManagementServiceClient();

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException(string.Format(
                    "An application with client_id '{0}' could not be found.", clientId));
            }

            serviceIdentity.RedirectAddress = redirectUri;

            // Process modifications to the namespace
            client.SaveChanges(SaveChangesOptions.Batch);
        }

        /// <summary>
        /// Removes an application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        public void RemoveApplication(string clientId)
        {
            var client = CreateManagementServiceClient();

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException(string.Format(
                    "An application with client_id '{0}' could not be found.", clientId));
            }

            // Delete service identity
            client.DeleteObject(serviceIdentity);
            client.SaveChanges(SaveChangesOptions.Batch);
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns></returns>
        public string GetApplicationName(string clientId)
        {
            var client = CreateManagementServiceClient();

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new InvalidOperationException(string.Format(
                    "An application with client_id '{0}' could not be found.", clientId));
            }

            return serviceIdentity.Description;
        }

        /// <summary>
        /// This method validates the incoming request.
        /// </summary>
        /// <param name="message">The incoming reequest  message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">Description of the error.</param>
        /// <returns>
        /// True if request is valid, false otherwise.
        /// </returns>
        public bool ValidateIncomingRequest(OAuthMessage message, out string errorCode, out string errorDescription)
        {
            try
            {
                message.Validate();
            }
            catch (OAuthMessageException messageException)
            {
                errorCode = OAuthConstants.ErrorCode.InvalidRequest;
                errorDescription = messageException.Message;
                return false;
            }

            // Verify the client_id and redirect_uri
            return ValidateServiceIdentity(message, out errorCode, out errorDescription);
        }

        /// <summary>
        /// Checks if the client_id and the redirect_uri are valid.
        /// </summary>
        /// <param name="message">The message which contains the client_id and redirect_uri.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">Description of the error.</param>
        /// <returns>
        /// True if the client_id and the redirect_uri are valid, false otherwise
        /// </returns>
        public bool ValidateServiceIdentity(OAuthMessage message, out string errorCode, out string errorDescription)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            string clientId = message.Parameters[OAuthConstants.ClientId];
            string redirectUri = message.Parameters[OAuthConstants.RedirectUri];

            if (clientId == null || redirectUri == null)
            {
                errorCode = OAuthConstants.ErrorCode.InvalidRequest;
                errorDescription = Resources.ID3716;
                return false;
            }

            // Verify the client_id and redirect_uri with Windows Azure ACS
            try
            {
                var client = CreateManagementServiceClient();

                var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
                if (serviceIdentity != null)
                {
                    if (serviceIdentity.RedirectAddress == redirectUri)
                    {
                        errorCode = "";
                        errorDescription = "";
                        return true;
                    }
                    else
                    {
                        errorCode = OAuthConstants.ErrorCode.InvalidClient;
                        errorDescription = Resources.ID3750;
                    }
                }
                else
                {
                    errorCode = OAuthConstants.ErrorCode.InvalidClient;
                    errorDescription = Resources.ID3751;
                }
            }
            catch (Exception)
            {
                errorCode = OAuthConstants.ErrorCode.InvalidClient;
                errorDescription = Resources.ID3751;
            }

            return false;
        }

        /// <summary>
        /// Verify if a delegation exists.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="delegatedIdentity">The delegated identity.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>True if a delegation exists, false otherwise.</returns>
        public bool DelegationExists(string clientId, AuthorizationServerIdentity delegatedIdentity, string scope)
        {
            var client = CreateManagementServiceClient();

            var relyingParty = client.RelyingParties.Where(rp => rp.Name == RelyingPartyName).ToList().FirstOrDefault();
            var relyingPartyId = relyingParty.Id;

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new OAuthMessageException(Resources.ID3751);
            }

            var nameIdentifier = delegatedIdentity.NameIdentifier;
            var identityProvider = delegatedIdentity.IdentityProvider;

            var serviceIdentityId = serviceIdentity.Id;
            var delegation = client.Delegations.Where(d => d.ServiceIdentityId == serviceIdentityId && d.RelyingPartyId == relyingPartyId && d.IdentityProvider == identityProvider && d.NameIdentifier == nameIdentifier).ToList().FirstOrDefault();
            return delegation != null;
        }

        /// <summary>
        /// Gets the authorization code.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="delegatedIdentity">The delegated identity.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>
        /// The authorization code.
        /// </returns>
        public string GetAuthorizationCode(string clientId, AuthorizationServerIdentity delegatedIdentity, string scope)
        {
            var client = CreateManagementServiceClient();

            var relyingParty = client.RelyingParties.Where(rp => rp.Name == RelyingPartyName).ToList().FirstOrDefault();
            var relyingPartyId = relyingParty.Id;

            var serviceIdentity = client.ServiceIdentities.Where(si => si.Name == clientId).ToList().FirstOrDefault();
            if (serviceIdentity == null)
            {
                throw new OAuthMessageException(Resources.ID3751);
            }

            var nameIdentifier = delegatedIdentity.NameIdentifier;
            var identityProvider = delegatedIdentity.IdentityProvider;

            var serviceIdentityId = serviceIdentity.Id;
            var delegation = client.Delegations.Where(d => d.ServiceIdentityId == serviceIdentityId && d.RelyingPartyId == relyingPartyId && d.IdentityProvider == identityProvider && d.NameIdentifier == nameIdentifier).ToList().FirstOrDefault();
            if (delegation == null)
            {
                delegation = new Delegation()
                {
                    NameIdentifier = delegatedIdentity.NameIdentifier,
                    IdentityProvider = delegatedIdentity.IdentityProvider,
                    RelyingPartyId = relyingPartyId,
                    ServiceIdentityId = serviceIdentity.Id,
                    Permissions = scope
                };

                client.AddToDelegations(delegation);
                client.SaveChanges();
            }

            return delegation.AuthorizationCode;
        }
    }
}