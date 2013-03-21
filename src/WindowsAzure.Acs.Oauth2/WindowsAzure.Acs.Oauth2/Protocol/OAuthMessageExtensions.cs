﻿using System;
using System.Collections.Specialized;
using System.Web;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    public static class OAuthMessageExtensions
    {
        /// <summary>
        /// Generates a Uri with the error message in the query string.
        /// </summary>
        /// <param name="message">Then incoming request message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">A description of the error.</param>
        /// <returns>
        /// Returns the redirect Uri.
        /// </returns>
        public static string GetErrorResponseUri(this OAuthMessage message, string errorCode, string errorDescription)
        {
            string state;

            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (errorCode == null)
            {
                throw new ArgumentNullException("errorCode");
            }

            if (errorDescription == null)
            {
                throw new ArgumentNullException("errorDescription");
            }

            string redirectUri = message.Parameters[OAuthConstants.RedirectUri];
            if (redirectUri == null)
            {
                throw new InvalidOperationException(OAuthConstants.RedirectUri + " cannot be null");
            }

            NameValueCollection responseParameters = new NameValueCollection();
            responseParameters.Add(OAuthConstants.Error, errorCode);
            responseParameters.Add(OAuthConstants.ErrorDescription, errorDescription);
            state = message.Parameters[OAuthConstants.State];
            if (state != null)
            {
                responseParameters.Add(OAuthConstants.State, state);
            }

            return redirectUri + CreateQueryString(responseParameters);
        }

        /// <summary>
        /// Generates a Uri to redirect the user's browser to, along with some OAuth paramters in the query string.
        /// </summary>
        /// <param name="message">The incoming request message.</param>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <returns>The redirect Uri.</returns>
        public static string GetCodeResponseUri(this OAuthMessage message, string authorizationCode)
        {
            string state;
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            string redirectUri = message.Parameters[OAuthConstants.RedirectUri];
            if (redirectUri == null && message.Parameters[OAuthConstants.GrantType] != OAuthConstants.AccessGrantType.ClientCredentials)
            {
                throw new InvalidOperationException(OAuthConstants.RedirectUri + " cannot be null");
            }

            NameValueCollection responseParameters = new NameValueCollection();
            responseParameters.Add(OAuthConstants.Code, authorizationCode);
            state = message.Parameters[OAuthConstants.State];
            if (state != null)
            {
                responseParameters.Add(OAuthConstants.State, state);
            }

            return redirectUri + CreateQueryString(responseParameters);
        }

        /// <summary>
        /// Creates a query string from the given name-value pairs.
        /// </summary>
        /// <param name="parameters">The name-value pairs.</param>
        /// <returns>The name-valeu pairs in query string format.</returns>
        public static string CreateQueryString(NameValueCollection parameters)
        {
            string result = string.Empty;
            string key, value;
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += "&";
                }

                key = parameters.GetKey(i);
                value = HttpUtility.UrlEncode(parameters[key]);
                result = result + key + "=" + value;
            }

            if (result != string.Empty)
            {
                result = "?" + result;
            }

            return result;
        }
    }
}
