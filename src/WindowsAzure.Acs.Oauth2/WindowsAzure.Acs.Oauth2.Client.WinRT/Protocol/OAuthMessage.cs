using System;
using System.Collections.Generic;

namespace WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol
{
    public abstract class OAuthMessage
    {
        private Dictionary<string,string> parameters = new Dictionary<string,string>();
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
        public Dictionary<string,string> Parameters
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
