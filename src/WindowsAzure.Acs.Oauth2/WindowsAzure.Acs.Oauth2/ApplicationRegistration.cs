using System;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// The ApplicationRegistration.
    /// </summary>
    public class ApplicationRegistration
    {
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the application URL.
        /// </summary>
        /// <value>
        /// The application URL.
        /// </value>
        public Uri ApplicationUrl { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        /// <value>
        /// The client id.
        /// </value>
        public string ClientId { get; set; }
    }
}