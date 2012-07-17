using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class EndUserAuthorizationRequest
        : OAuthMessage
    {
        public string ResponseType
        {
            get
            {
                return base.Parameters["response_type"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                if (!EndUserAuthorizationRequest.IsValidResponseType(value))
                {
                    throw new ArgumentException(string.Format(Resources.ID3717, value), "value");
                }
                base.Parameters["response_type"] = value;
            }
        }
        public string ClientId
        {
            get
            {
                return base.Parameters["client_id"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["client_id"] = value;
            }
        }
        public Uri RedirectUri
        {
            get
            {
                if (base.Parameters["redirect_uri"] != null)
                {
                    return new Uri(base.Parameters["redirect_uri"]);
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException(string.Format(Resources.ID3709, value.AbsoluteUri), "value");
                }
                base.Parameters["redirect_uri"] = value.AbsoluteUri;
            }
        }
        public EndUserAuthorizationRequest(Uri baseUri)
            : base(baseUri)
        {
        }
        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.ResponseType) || string.IsNullOrEmpty(this.ClientId) || this.RedirectUri == null)
            {
                throw new OAuthMessageException(Resources.ID3716);
            }
            if (!EndUserAuthorizationRequest.IsValidResponseType(this.ResponseType))
            {
                throw new OAuthMessageException(
                    string.Format(Resources.ID3717, this.ResponseType));
            }
        }
        private static bool IsValidResponseType(string responseType)
        {
            return responseType == "code" || responseType == "token";
        }
    }
}