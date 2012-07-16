namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// AuthorizationServerIdentity class.
    /// </summary>
    public class AuthorizationServerIdentity
    {
        /// <summary>
        /// Gets or sets the name identifier.
        /// </summary>
        /// <value>
        /// The name identifier.
        /// </value>
        public string NameIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the identity provider.
        /// </summary>
        /// <value>
        /// The identity provider.
        /// </value>
        public string IdentityProvider { get; set; }
    }
}