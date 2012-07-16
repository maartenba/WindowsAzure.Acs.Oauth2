using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.IdentityModel.Claims;
using WindowsAzure.Acs.Oauth2.Protocol.Swt;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public class OAuth2MessageHandler
        : DelegatingHandler
    {
        private readonly string _realm;
        private string _issuer;
        private string _tokenSigningKey;

        public OAuth2MessageHandler()
            : this(ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyRealm"],
                    string.Format("https://{0}.accesscontrol.windows.net/", ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespace"]),
                    ConfigurationManager.AppSettings["WindowsAzure.OAuth.SwtSigningKey"])
        {
        }

        public OAuth2MessageHandler(string realm, string issuer, string tokenSigningKey)
        {
            if (realm == null) throw new ArgumentNullException("realm");
            if (issuer == null) throw new ArgumentNullException("issuer");
            if (tokenSigningKey == null) throw new ArgumentNullException("tokenSigningKey");

            _realm = realm;
            _issuer = issuer;
            _tokenSigningKey = tokenSigningKey;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContextBase httpContext;

            if (!request.TryGetHttpContext(out httpContext))
            {
                throw new InvalidOperationException("HttpContext must not be null.");
            }


            string accessToken;
            // checks if it is an OAuth Request and gets the access token
            if (TryReadAccessToken(request, out accessToken))
            {
                // Parses the token and validates it.
                ResourceAccessErrorResponse error;
                if (!ReadAndValidateToken(accessToken, out error))
                {
                    throw new HttpResponseException(
                        new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new ObjectContent(typeof(ResourceAccessErrorResponse), error, new JsonMediaTypeFormatter())
                            });
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// This method looks for the access token in the incoming request.
        /// </summary>
        /// <param name="request">The incoming request message.</param>
        /// <param name="accessToken">This out parameter contains the access token if found.</param>
        /// <returns>True if access token is found , otherwise false.</returns>
        protected bool TryReadAccessToken(HttpRequestMessage request, out string accessToken)
        {
            accessToken = null;

            // search for tokens in the Authorization header            
            accessToken = GetTokenFromAuthorizationHeader(request);
            if (!string.IsNullOrEmpty(accessToken))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the access token from the Authorization header of the incoming request.
        /// </summary>
        /// <param name="request">The Http request message.</param>
        /// <returns>The access token.</returns>
        public string GetTokenFromAuthorizationHeader(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.Headers.Contains("Authorization"))
            {
                return null;
            }

            string bearer = "Bearer";
            string authHeader = request.Headers.GetValues("Authorization").FirstOrDefault();
            string token = null;

            // the authorization header looks like
            // Authorization: Bearer <access token>
            if (!string.IsNullOrEmpty(authHeader))
            {
                if (String.CompareOrdinal(authHeader, 0, bearer, 0, bearer.Length) == 0)
                {
                    token = authHeader.Remove(0, bearer.Length + 1);
                }
            }

            return token;
        }

        /// <summary>
        /// This method parses the incoming token and validates it.
        /// </summary>
        /// <param name="accessToken">The incoming access token.</param>
        /// <param name="error">This out paramter is set if any error occurs.</param>
        /// <returns>True on success, False on error.</returns>
        protected bool ReadAndValidateToken(string accessToken, out ResourceAccessErrorResponse error)
        {
            bool tokenValid = false;
            error = null;

            SecurityToken token = null;
            ClaimsIdentityCollection claimsIdentityCollection = null;

            try
            {
                var handler = new SimpleWebTokenHandler(_issuer, _tokenSigningKey);

                // read the token
                token = handler.ReadToken(accessToken);

                // validate the token
                claimsIdentityCollection = handler.ValidateToken(token, _realm);

                // create a claims Principal from the token
                var claimsPrincipal = ClaimsPrincipal.CreateFromIdentities(claimsIdentityCollection);
                if (claimsPrincipal != null)
                {
                    tokenValid = true;

                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = claimsPrincipal;
                    }
                    Thread.CurrentPrincipal = claimsPrincipal;
                }
            }
            catch (InvalidTokenReceivedException ex)
            {
                error = new ResourceAccessErrorResponse(_realm, ex.ErrorCode, ex.ErrorDescription);
            }
            catch (ExpiredTokenReceivedException ex)
            {
                error = new ResourceAccessErrorResponse(_realm, ex.ErrorCode, ex.ErrorDescription);
            }
            catch (Exception)
            {
                error = new ResourceAccessErrorResponse(_realm, "SWT401", "Token validation failed");
            }

            return tokenValid;
        }
    }
}