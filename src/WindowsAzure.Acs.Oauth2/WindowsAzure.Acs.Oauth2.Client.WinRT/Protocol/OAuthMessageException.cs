using System;

namespace WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol
{
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
    }
}