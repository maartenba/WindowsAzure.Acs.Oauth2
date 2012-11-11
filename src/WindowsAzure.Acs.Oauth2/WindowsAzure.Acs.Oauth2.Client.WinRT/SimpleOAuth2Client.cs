using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol;

namespace WindowsAzure.Acs.Oauth2.Client.WinRT
{
    /// <summary>
    /// SimpleOAuth2Client.
    /// </summary>
    public class SimpleOAuth2Client
    {
        /// <summary>
        /// Gets or sets the authorize URI.
        /// </summary>
        /// <value>
        /// The authorize URI.
        /// </value>
        public Uri AuthorizeUri { get; set; }

        /// <summary>
        /// Gets or sets the access token URI.
        /// </summary>
        /// <value>
        /// The access token URI.
        /// </value>
        public Uri AccessTokenUri { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        /// <value>
        /// The client id.
        /// </value>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        /// <value>
        /// The redirect URI.
        /// </value>
        public Uri RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the current access token.
        /// </summary>
        /// <value>
        /// The current access token.
        /// </value>
        protected AccessTokenResponse CurrentAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the last access token refresh.
        /// </summary>
        /// <value>
        /// The last access token refresh.
        /// </value>
        protected DateTime LastAccessTokenRefresh { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleOAuth2Client"/> class.
        /// </summary>
        /// <param name="authorizeUri">The authorize URI.</param>
        /// <param name="accessTokenUri">The access token URI.</param>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="redirectUri">The redirect URI.</param>
        public SimpleOAuth2Client(Uri authorizeUri, Uri accessTokenUri, string clientId, string clientSecret, string scope, Uri redirectUri)
        {
            if (authorizeUri == null) throw new ArgumentNullException("authorizeUri");
            if (accessTokenUri == null) throw new ArgumentNullException("accessTokenUri");
            if (clientId == null) throw new ArgumentNullException("clientId");
            if (clientSecret == null) throw new ArgumentNullException("clientSecret");
            if (scope == null) throw new ArgumentNullException("scope");
            if (redirectUri == null) throw new ArgumentNullException("redirectUri");

            AuthorizeUri = authorizeUri;
            AccessTokenUri = accessTokenUri;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scope = scope;
            RedirectUri = redirectUri;
        }

        /// <summary>
        /// Builds the authorization URI.
        /// </summary>
        /// <returns></returns>
        public Uri BuildAuthorizationUri()
        {
            var authorizationUri = string.Format("{0}?client_id={1}&redirect_uri={2}&scope={3}&response_type=code", AuthorizeUri, ClientId, RedirectUri, Scope);
            return new Uri(authorizationUri);
        }

        /// <summary>
        /// Authorizes the specified refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        public async Task AuthorizeAsync(string refreshToken)
        {
            var authorizeRequest = BuildAccessTokenRequest(refreshToken);

            var req = new HttpClient();
            var response = await req.PostAsync(authorizeRequest.BaseUri, new FormUrlEncodedContent(authorizeRequest.Parameters));

            var serializer = new OAuthMessageSerializer();

            var deserializedMessage = await serializer.Read(response);

            var message = deserializedMessage as AccessTokenResponse;
            if (message != null)
            {
                CurrentAccessToken = message;
                LastAccessTokenRefresh = DateTime.UtcNow;
            }

            var endUserAuthorizationFailedResponse = deserializedMessage as EndUserAuthorizationFailedResponse;
            if (endUserAuthorizationFailedResponse != null)
            {
                throw new SecurityException(endUserAuthorizationFailedResponse.ErrorDescription);
            }

            var userAuthorizationFailedResponse = deserializedMessage as ResourceAccessFailureResponse;
            if (userAuthorizationFailedResponse != null)
            {
                throw new SecurityException(userAuthorizationFailedResponse.ErrorDescription);
            }
        }

        /// <summary>
        /// Appends the access token to.
        /// </summary>
        /// <param name="client">The client.</param>
        public async Task AppendAccessTokenToAsync(HttpClient client)
        {
            if (CurrentAccessToken == null)
            {
                throw new ArgumentNullException("CurrentAccessToken is null. A call to Authorize() using an authorization or refresh token should be made first.", "CurrentAccessToken");
            }

            if (DateTime.UtcNow.AddSeconds(-15) < LastAccessTokenRefresh.AddSeconds(CurrentAccessToken.ExpiresIn))
            {
                await AuthorizeAsync(CurrentAccessToken.RefreshToken);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(CurrentAccessToken.AccessToken)));
        }

        private AccessTokenRequestWithAuthorizationCode BuildAccessTokenRequest(string refreshToken)
        {
            return new AccessTokenRequestWithAuthorizationCode(AccessTokenUri)
                       {
                           ClientId = ClientId,
                           ClientSecret = ClientSecret,
                           Scope = Scope,
                           GrantType = "authorization_code",
                           Code = refreshToken,
                           RedirectUri = RedirectUri
                       };
        }
    }
}
