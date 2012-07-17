using System;
using System.Collections.Specialized;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public abstract class OAuthMessage
    {
        private NameValueCollection parameters = new NameValueCollection();
        private Uri _baseUri;
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
        }
        public string Scope
        {
            get
            {
                return this.Parameters["scope"];
            }
            set
            {
                if (System.StringComparer.OrdinalIgnoreCase.Equals(value, string.Empty))
                {
                    throw new ArgumentException(Resources.ID3719, "value");
                }
                this.Parameters["scope"] = value;
            }
        }
        public string State
        {
            get
            {
                return this.Parameters["state"];
            }
            set
            {
                if (System.StringComparer.OrdinalIgnoreCase.Equals(value, string.Empty))
                {
                    throw new ArgumentException(Resources.ID3720, "value");
                }
                this.Parameters["state"] = value;
            }
        }
        public NameValueCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }
        protected OAuthMessage(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }
            this._baseUri = baseUri;
        }
        public abstract void Validate();
    }
}
