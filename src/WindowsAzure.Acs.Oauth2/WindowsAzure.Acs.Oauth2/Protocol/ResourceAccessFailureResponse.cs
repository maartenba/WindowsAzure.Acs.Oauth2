using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class ResourceAccessFailureResponse
        : OAuthMessage
    {
        public string Realm
        {
            get
            {
                return base.Parameters["realm"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                base.Parameters["realm"] = value;
            }
        }

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
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
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
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException(string.Format(Resources.ID3709, value.ToString()), "value");
                }
                base.Parameters["error_uri"] = value.AbsoluteUri;
            }
        }

        public ResourceAccessFailureResponse(Uri baseUri)
            : base(baseUri)
        {
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(this.Realm))
            {
                throw new OAuthMessageException(Resources.ID3715);
            }
        }
    }
}