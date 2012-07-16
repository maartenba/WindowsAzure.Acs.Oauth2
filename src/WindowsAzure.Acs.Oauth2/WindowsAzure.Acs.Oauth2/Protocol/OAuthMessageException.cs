using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class OAuthMessageException
        : Exception
    {
        public OAuthMessageException()
            : base(Resources.ID3730)
        {
        }
        public OAuthMessageException(string message)
            : base(message)
        {
        }
        public OAuthMessageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        protected OAuthMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}