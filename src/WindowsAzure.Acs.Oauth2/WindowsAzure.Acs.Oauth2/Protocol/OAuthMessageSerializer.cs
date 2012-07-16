using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    public class OAuthMessageSerializer
    {
        public virtual OAuthMessage Read(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return this.Read(context.Request.HttpMethod, context.Request.ContentType, context.Request.Url, context.Request.InputStream);
        }

        public virtual OAuthMessage Read(HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return this.Read(context.Request.HttpMethod, context.Request.ContentType, context.Request.Url, context.Request.InputStream);
        }

        public virtual OAuthMessage Read(HttpWebResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            return this.Read(response.Method, response.ContentType, response.ResponseUri, response.GetResponseStream());
        }

        public virtual OAuthMessage Read(string httpMethod, string httpContentType, Uri requestUri, System.IO.Stream incomingStream)
        {
            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentOutOfRangeException("httpMethod");
            }
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }
            if (incomingStream == null)
            {
                throw new ArgumentNullException("incomingStream");
            }

            NameValueCollection oAuthParameters = new NameValueCollection();
            if (httpMethod == "POST")
            {
                if (httpContentType.Contains("application/x-www-form-urlencoded"))
                {
                    oAuthParameters = this.ReadFormEncodedParameters(incomingStream);
                }
                else
                {
                    if (!httpContentType.Contains("application/json"))
                    {
                        throw new OAuthMessageSerializationException(string.Format(Resources.ID3721, httpMethod, httpContentType));
                    }
                    oAuthParameters = this.ReadJsonEncodedParameters(incomingStream);
                }
            }
            else
            {
                if (!(httpMethod == "GET"))
                {
                    throw new OAuthMessageSerializationException(string.Format(Resources.ID3722, httpMethod));
                }
                oAuthParameters = HttpUtility.ParseQueryString(requestUri.Query);
            }
            return this.CreateTypedOAuthMessageFromParameters(OAuthMessageSerializer.GetBaseUrl(requestUri), oAuthParameters);
        }

        public virtual NameValueCollection ReadFormEncodedParameters(System.IO.Stream incomingStream)
        {
            if (incomingStream == null)
            {
                throw new ArgumentNullException("incomingStream");
            }
            System.IO.StreamReader reader = new System.IO.StreamReader(incomingStream);
            return HttpUtility.ParseQueryString(reader.ReadToEnd());
        }

        public virtual NameValueCollection ReadJsonEncodedParameters(System.IO.Stream incomingStream)
        {
            if (incomingStream == null)
            {
                throw new ArgumentNullException("incomingStream");
            }

            NameValueCollection parameters = new NameValueCollection();
            var jsonReader = new JsonTextReader(new StreamReader(incomingStream));

            while (jsonReader.Read())
            {
                // Not interested in nested objects/arrays! 
                if (jsonReader.Depth > 1)
                {
                    jsonReader.Skip();
                }
                else if (jsonReader.TokenType == JsonToken.PropertyName)
                {
                    string key = jsonReader.Value.ToString();
                    if (jsonReader.Read())
                    {
                        switch (jsonReader.TokenType)
                        {
                            case JsonToken.Boolean:
                            case JsonToken.Date:
                            case JsonToken.Float:
                            case JsonToken.Integer:
                            case JsonToken.Null:
                            case JsonToken.String:
                                parameters[key] = jsonReader.Value.ToString();
                                break;
                        }
                    }
                }
            }

            return parameters;
        }

        public virtual ResourceAccessFailureResponse ReadAuthenticationHeader(string authenticateHeader, Uri resourceUri)
        {
            if (string.IsNullOrEmpty(authenticateHeader))
            {
                throw new ArgumentNullException("authenticateHeader");
            }
            if (resourceUri == null)
            {
                throw new ArgumentNullException("resourceUri");
            }

            ResourceAccessFailureResponse response = null;
            string expectedAuthType = "Bearer";
            string authType = authenticateHeader.Split(new char[] { ' ' }, 2)[0];

            if (string.IsNullOrEmpty(authType))
            {
                throw new OAuthMessageSerializationException(string.Format(Resources.ID3741, authType));
            }

            NameValueCollection keyValuePairs = new NameValueCollection();
            if (authType.Contains(expectedAuthType))
            {
                response = new ResourceAccessFailureResponse(resourceUri);
                authenticateHeader = authenticateHeader.Remove(0, authType.Length);
                authenticateHeader = authenticateHeader.TrimStart(new char[] { ' ' });
                if (!string.IsNullOrEmpty(authenticateHeader))
                {
                    string[] parameters = authenticateHeader.Split(new string[] { "\", " }, System.StringSplitOptions.None);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        string entry = parameters[i];
                        string splitAtEqualSign = "=\"";
                        string[] pairs = entry.Split(new string[] { splitAtEqualSign }, 2, System.StringSplitOptions.None);
                        if (pairs.Length != 2)
                        {
                            throw new OAuthMessageSerializationException(string.Format(Resources.ID3741, authType));
                        }
                        if (i == parameters.Length - 1 && pairs[1][pairs[1].Length - 1] == '"')
                        {
                            pairs[1] = pairs[1].Remove(pairs[1].Length - 1, 1);
                        }
                        keyValuePairs.Add(pairs[0], pairs[1]);
                    }
                    response.Parameters.Add(keyValuePairs);
                    response.Validate();
                }
            }

            return response;
        }

        protected virtual OAuthMessage CreateTypedOAuthMessageFromParameters(Uri baseUri, NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            OAuthMessage request = null;
            if (parameters["response_type"] == "code" || parameters["response_type"] == "token")
            {
                request = new EndUserAuthorizationRequest(baseUri);
            }
            if (!string.IsNullOrEmpty(parameters["code"]) || (!string.IsNullOrEmpty(parameters["access_token"]) && string.IsNullOrEmpty(parameters["refresh_token"])))
            {
                request = new EndUserAuthorizationResponse(baseUri);
            }
            if (!string.IsNullOrEmpty(parameters["error"]))
            {
                request = new EndUserAuthorizationFailedResponse(baseUri);
            }
            if (!string.IsNullOrEmpty(parameters["grant_type"]) && parameters["grant_type"] == "authorization_code")
            {
                request = new AccessTokenRequestWithAuthorizationCode(baseUri);
            }
            if (!string.IsNullOrEmpty(parameters["access_token"]))
            {
                request = new AccessTokenResponse(baseUri);
            }
            if (request == null)
            {
                throw new OAuthMessageSerializationException(Resources.ID3723);
            }

            request.Parameters.Add(parameters);
            request.Validate();
            return request;
        }

        public virtual void Write(OAuthMessage message, HttpContext context)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Response.ContentType = this.GetHttpContentType(message);
            context.Response.Clear();
            this.Write(message, context.Response.OutputStream);
            context.Response.Flush();
        }

        public virtual void Write(OAuthMessage message, HttpContextBase context)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Response.ContentType = this.GetHttpContentType(message);
            context.Response.Clear();
            this.Write(message, context.Response.OutputStream);
            context.Response.Flush();
        }

        public virtual void Write(OAuthMessage message, HttpWebRequest request)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Method = this.GetHttpMethod(message);
            request.ContentType = this.GetHttpContentType(message);
            this.Write(message, request.GetRequestStream());
        }

        public virtual void Write(OAuthMessage message, System.IO.Stream requestStream)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (requestStream == null)
            {
                throw new ArgumentNullException("requestStream");
            }

            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(requestStream);
            AccessTokenRequest atRequestMsg = message as AccessTokenRequest;
            if (atRequestMsg != null)
            {
                streamWriter.Write(this.GetFormEncodedQueryFormat(message));
                streamWriter.Flush();
                return;
            }

            AccessTokenResponse atResponseMsg = message as AccessTokenResponse;
            if (atResponseMsg != null)
            {
                streamWriter.Write(this.GetJsonEncodedFormat(message));
                streamWriter.Flush();
                return;
            }

            throw new OAuthMessageException(string.Format(Resources.ID3724, message.GetType()));
        }

        public virtual string GetQueryStringFormat(OAuthMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Append(message.BaseUri.AbsoluteUri);
            strBuilder.Append("?");
            strBuilder.Append(this.GetFormEncodedQueryFormat(message));
            return strBuilder.ToString();
        }

        public virtual string GetFormEncodedQueryFormat(OAuthMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            bool skipDelimiter = true;
            foreach (string key in message.Parameters.Keys)
            {
                if (message.Parameters[key] != null)
                {
                    if (!skipDelimiter)
                    {
                        strBuilder.Append("&");
                    }
                    strBuilder.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}={1}", new object[]
                                                                                                                      {
                                                                                                                          HttpUtility.UrlEncode(key), 
                                                                                                                          HttpUtility.UrlEncode(message.Parameters[key])
                                                                                                                      }));
                    skipDelimiter = false;
                }
            }
            return strBuilder.ToString();
        }

        public virtual string GetJsonEncodedFormat(OAuthMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var serializedMessage = JsonConvert.SerializeObject(message.Parameters);

            // TODO: replace token of array to object...
            return serializedMessage;
        }

        public virtual string GetHttpMethod(OAuthMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            AccessTokenRequest atRequestMessage = message as AccessTokenRequest;
            if (atRequestMessage != null)
            {
                return "POST";
            }

            AccessTokenResponse atResponseMessage = message as AccessTokenResponse;
            if (atResponseMessage != null)
            {
                return "POST";
            }

            return "GET";
        }

        public virtual string GetHttpContentType(OAuthMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            AccessTokenRequest atRequestMessage = message as AccessTokenRequest;
            if (atRequestMessage != null)
            {
                return "application/x-www-form-urlencoded";
            }

            AccessTokenResponse atResponseMessage = message as AccessTokenResponse;
            if (atResponseMessage != null)
            {
                return "application/json";
            }
            return "text/plain; charset=us-ascii";
        }

        private static Uri GetBaseUrl(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            string tempUri = uri.AbsoluteUri;
            int index = tempUri.IndexOf("?", 0, System.StringComparison.Ordinal);
            if (index > -1)
            {
                tempUri = tempUri.Substring(0, index);
            }
            return new Uri(tempUri);
        }
    }
}