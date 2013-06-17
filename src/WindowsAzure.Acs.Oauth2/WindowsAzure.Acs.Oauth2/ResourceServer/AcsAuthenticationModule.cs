using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.IdentityModel.Claims;
using WindowsAzure.Acs.Oauth2.Protocol.Swt;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    public class AcsAuthenticationModule : IHttpModule
    {
        private string _realm;
        private string _issuer;
        private string _tokenSigningKey;
        private List<IAuthenticationStep> authenticationPipeline = new List<IAuthenticationStep>();

        public void Init(HttpApplication context)
        {
            var realm = ConfigurationManager.AppSettings["WindowsAzure.OAuth.RelyingPartyRealm"];
            var issuer = string.Format("https://{0}.accesscontrol.windows.net/", ConfigurationManager.AppSettings["WindowsAzure.OAuth.ServiceNamespace"]);
            var tokenSigningKey = ConfigurationManager.AppSettings["WindowsAzure.OAuth.SwtSigningKey"];

            if (realm == null) throw new ArgumentNullException("realm");
            if (issuer == null) throw new ArgumentNullException("issuer");
            if (tokenSigningKey == null) throw new ArgumentNullException("tokenSigningKey");

            _realm = realm;
            _issuer = issuer;
            _tokenSigningKey = tokenSigningKey;

            context.AuthenticateRequest += AuthenticateRequest;
        }

        void AuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;

            string accessToken;
            if (TryReadAccessToken(request, out accessToken))
            {
                // Parses the token and validates it.
                ResourceAccessErrorResponse error;
                if (!ReadAndValidateToken(accessToken, out error))
                {
                    // as we're this early in the pipeline
                    HttpContext.Current.Items["AuthorizationError"] = error;
                }
            }
        }



        public void AddAuthenticationStep(IAuthenticationStep step)
        {
            authenticationPipeline.Add(step);
        }

        public void SetAuthenticationPipeline(IEnumerable<IAuthenticationStep> pipeline)
        {
            authenticationPipeline = pipeline.ToList();
        }

        public void Dispose()
        {
        }


        /// <summary>
        /// This method looks for the access token in the incoming request.
        /// </summary>
        /// <param name="request">The incoming request message.</param>
        /// <param name="accessToken">This out parameter contains the access token if found.</param>
        /// <returns>True if access token is found , otherwise false.</returns>
        protected bool TryReadAccessToken(HttpRequest request, out string accessToken)
        {
            accessToken = null;

            // search for tokens in the Authorization header            
            accessToken = GetTokenFromAuthorizationHeader(request);
            if (accessToken == null) accessToken = GetTokenFromQueryString(request);
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
        public string GetTokenFromAuthorizationHeader(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.Headers.AllKeys.Contains("Authorization"))
            {
                return null;
            }

            string bearer = "Bearer";
            string authHeader = request.Headers["Authorization"];
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
        /// Gets the access token from the querystring of the incoming request.
        /// </summary>
        /// <param name="request">The Http request message.</param>
        /// <returns>The access token.</returns>
        public string GetTokenFromQueryString(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!request.QueryString.AllKeys.Contains("Authorization"))
            {
                return null;
            }

            string bearer = "Bearer";
            string authHeader = request.QueryString["Authorization"];
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

                    // push it through the pipeline
                    foreach (var step in authenticationPipeline)
                    {
                        claimsPrincipal = step.Authenticate(token, claimsPrincipal);
                    }

                    // assign to threads
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