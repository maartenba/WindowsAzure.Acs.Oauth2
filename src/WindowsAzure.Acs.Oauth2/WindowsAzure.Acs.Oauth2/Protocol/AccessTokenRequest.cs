using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class AccessTokenRequest
        : OAuthMessage
    {
        public string GrantType
        {
            get
            {
                return base.Parameters["grant_type"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                if (!AccessTokenRequest.IsValidGrantType(value))
                {
                    throw new ArgumentException(string.Format(Resources.ID3707, value), "value");
                }
                base.Parameters["grant_type"] = value;
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
        public string ClientSecret
        {
            get
            {
                return base.Parameters["client_secret"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["client_secret"] = value;
            }
        }
        public AccessTokenRequest(Uri baseUri)
            : base(baseUri)
        {
        }
        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.GrantType) || (string.IsNullOrEmpty(this.ClientId) && string.IsNullOrEmpty(this.ClientSecret)))
            {
                throw new OAuthMessageException(Resources.ID3708);
            }
            if (!AccessTokenRequest.IsValidGrantType(this.GrantType))
            {
                throw new OAuthMessageException(string.Format(Resources.ID3707, this.GrantType));
            }
        }
        private static bool IsValidGrantType(string grantType)
        {
            return grantType == "authorization_code" || grantType == "password" || grantType == "client_credentials" || grantType == "refresh_token" || AccessTokenRequest.IsValidAbsoluteUri(grantType);
        }
        private static bool IsValidAbsoluteUri(string uri)
        {
            Uri absoluteUri;
            return Uri.TryCreate(uri, UriKind.Absolute, out absoluteUri);
        }
    }
}