using System;
using System.Collections.Generic;

namespace WindowsAzure.Acs.Oauth2
{
    /// <summary>
    /// The AuthorizationServerViewModel.
    /// </summary>
    public class AuthorizationServerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServerViewModel"/> class.
        /// </summary>
        public AuthorizationServerViewModel()
        {
            Parameters = new Dictionary<string, string>();
        }

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
        /// Gets or sets the application publisher.
        /// </summary>
        /// <value>
        /// The application publisher.
        /// </value>
        public string ApplicationPublisher { get; set; }

        /// <summary>
        /// Gets or sets the application publisher URL.
        /// </summary>
        /// <value>
        /// The application publisher URL.
        /// </value>
        public Uri ApplicationPublisherUrl { get; set; }

        /// <summary>
        /// Gets or sets the application publisher logo URL.
        /// </summary>
        /// <value>
        /// The application publisher logo URL.
        /// </value>
        public Uri ApplicationPublisherLogoUrl { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AuthorizationServerViewModel"/> is authorized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if authorized; otherwise, <c>false</c>.
        /// </value>
        public bool Authorize { get; set; }
    }
}