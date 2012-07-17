using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class EndUserAuthorizationResponse
        : OAuthMessage
    {
        public string Code
        {
            get
            {
                return base.Parameters["code"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["code"] = value;
            }
        }
        public string AccessToken
        {
            get
            {
                return base.Parameters["access_token"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["access_token"] = value;
            }
        }
        public int ExpiresIn
        {
            get
            {
                if (base.Parameters["expires_in"] != null)
                {
                    return int.Parse(base.Parameters["expires_in"], System.Globalization.CultureInfo.InvariantCulture);
                }
                return -1;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(Resources.ID3712, "value");
                }
                base.Parameters["expires_in"] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        public EndUserAuthorizationResponse(Uri baseUri)
            : base(baseUri)
        {
        }
        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Code) && string.IsNullOrEmpty(this.AccessToken))
            {
                throw new OAuthMessageException(Resources.ID3718);
            }
            if (!string.IsNullOrEmpty(base.Parameters["expires_in"]) && this.ExpiresIn <= 0)
            {
                throw new OAuthMessageException(Resources.ID3712);
            }
        }
    }
}