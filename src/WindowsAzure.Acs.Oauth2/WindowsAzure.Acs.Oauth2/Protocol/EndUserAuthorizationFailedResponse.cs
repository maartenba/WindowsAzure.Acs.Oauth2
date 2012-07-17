using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class EndUserAuthorizationFailedResponse
        : OAuthMessage
    {
        public string Error
        {
            get
            {
                return base.Parameters["error"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["error"] = value;
            }
        }
        public string ErrorDescription
        {
            get
            {
                return base.Parameters["error_description"];
            }
            set
            {
                if (System.StringComparer.OrdinalIgnoreCase.Equals(value, string.Empty))
                {
                    throw new ArgumentException(Resources.ID3713, "value");
                }
                base.Parameters["error_description"] = value;
            }
        }
        public Uri ErrorUri
        {
            get
            {
                if (base.Parameters["error_uri"] != null)
                {
                    return new Uri(base.Parameters["error_uri"]);
                }
                return null;
            }
            set
            {
                if (value != null && !value.IsAbsoluteUri)
                {
                    throw new ArgumentException(Resources.ID3714, "value");
                }
                base.Parameters["error_uri"] = value.AbsoluteUri;
            }
        }
        public EndUserAuthorizationFailedResponse(Uri baseUri)
            : base(baseUri)
        {
        }
        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Error))
            {
                throw new OAuthMessageException(Resources.ID3715);
            }
        }
    }
}