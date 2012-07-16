using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Text;

namespace WindowsAzure.Acs.Oauth2.Protocol.Client
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
        public void Authorize(string refreshToken)
        {
            var authorizeRequest = BuildAccessTokenRequest(refreshToken);

            var serializer = new OAuthMessageSerializer();
            var encodedQueryFormat = serializer.GetFormEncodedQueryFormat(authorizeRequest);

            HttpWebRequest httpWebRequest = WebRequest.Create(authorizeRequest.BaseUri) as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(encodedQueryFormat);
            streamWriter.Close();

            try
            {
                var message = serializer.Read(httpWebRequest.GetResponse() as HttpWebResponse) as AccessTokenResponse;
                if (message != null)
                {
                    CurrentAccessToken = message;
                    LastAccessTokenRefresh = DateTime.UtcNow;
                }
            }
            catch (WebException webex)
            {
                var message = serializer.Read(webex.Response as HttpWebResponse);

                var endUserAuthorizationFailedResponse = message as EndUserAuthorizationFailedResponse;
                if (endUserAuthorizationFailedResponse != null)
                {
                    throw new AuthenticationException(endUserAuthorizationFailedResponse.ErrorDescription);
                }

                var userAuthorizationFailedResponse = message as ResourceAccessFailureResponse;
                if (userAuthorizationFailedResponse != null)
                {
                    throw new AuthenticationException(userAuthorizationFailedResponse.ErrorDescription);
                }

                throw;
            }
        }

        /// <summary>
        /// Appends the access token to.
        /// </summary>
        /// <param name="webRequest">The web request.</param>
        public void AppendAccessTokenTo(HttpWebRequest webRequest)
        {
            if (CurrentAccessToken == null)
            {
                throw new ArgumentNullException("CurrentAccessToken is null. A call to Authorize() using an authorization or refresh token should be made first.", "CurrentAccessToken");
            }

            if (DateTime.UtcNow.AddSeconds(-15) < LastAccessTokenRefresh.AddSeconds(CurrentAccessToken.ExpiresIn))
            {
                Authorize(CurrentAccessToken.RefreshToken);
            }

            webRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + Convert.ToBase64String(Encoding.UTF8.GetBytes(CurrentAccessToken.AccessToken)));
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
