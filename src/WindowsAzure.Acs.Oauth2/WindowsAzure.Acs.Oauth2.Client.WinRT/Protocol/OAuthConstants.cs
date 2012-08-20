namespace WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol
{
    public static class OAuthConstants
    {
        public static class AuthorizationType
        {
            public const string Code = "code";
            public const string Token = "token";
        }
        public static class ErrorCode
        {
            public const string InsufficientScope = "insufficient_scope";
            public const string InvalidClient = "invalid_client";
            public const string InvalidRequest = "invalid_request";
            public const string InvalidGrant = "invalid_grant";
            public const string InvalidScope = "invalid_scope";
            public const string InvalidToken = "invalid_token";
            public const string UnsupportedGrantType = "unsupported_grant_type";
            public const string UnauthorizedClient = "unauthorized_client";
            public const string AccessDenied = "access_denied";
            public const string UnsupportedResponseType = "unsupported_response_type";
        }
        public static class AccessGrantType
        {
            public const string AuthorizationCode = "authorization_code";
            public const string Password = "password";
            public const string ClientCredentials = "client_credentials";
            public const string RefreshToken = "refresh_token";
        }
        public const string AccessToken = "access_token";
        public const string AuthenticationType = "Bearer";
        public const string TokenType = "token_type";
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string Code = "code";
        public const string Error = "error";
        public const string ErrorDescription = "error_description";
        public const string ErrorUri = "error_uri";
        public const string ExpiresIn = "expires_in";
        public const string GrantType = "grant_type";
        public const string Realm = "realm";
        public const string RedirectUri = "redirect_uri";
        public const string RefreshToken = "refresh_token";
        public const string ResponseType = "response_type";
        public const string State = "state";
        public const string Scope = "scope";
    }
}