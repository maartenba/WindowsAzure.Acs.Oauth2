using System;

namespace WindowsAzure.Acs.Oauth2.Protocol
{
    [Serializable]
    public class OAuthMessageSerializationException
        : Exception
    {
        public OAuthMessageSerializationException()
            : base(Resources.ID3731)
        {
        }
        public OAuthMessageSerializationException(string message)
            : base(message)
        {
        }
        public OAuthMessageSerializationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
        protected OAuthMessageSerializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}