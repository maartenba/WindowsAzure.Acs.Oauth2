//-----------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
//
//-----------------------------------------------------------------------------

using System;

namespace WindowsAzure.Acs.Oauth2.ResourceServer
{
    /// <summary>
    /// The error response message sent back when an error occurs.
    /// </summary>
    public class ResourceAccessErrorResponse
    {
        string _errorCode;
        string _errorDescription;
        Uri _errorUri;
        string _scope;
        string _realm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAccessErrorResponse"/> class.
        /// </summary>
        /// <param name="realm">The realm of the web resource.</param>
        /// <param name="errorCode">The OAuth error code.</param>
        /// <param name="errorDescription">A description of the error.</param>
        public ResourceAccessErrorResponse(string realm, string errorCode, string errorDescription)
        {
            if (string.IsNullOrEmpty(realm))
            {
                throw new ArgumentNullException("realm");
            }

            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentNullException("errorCode");
            }

            _realm = realm;
            _errorCode = errorCode;
            _errorDescription = errorDescription;
        }

        /// <summary>
        /// Gets or sets the OAuth error code for the error.
        /// </summary>
        /// <value>The OAuth error code.</value>
        public string ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        /// <summary>
        /// Gets or sets the description of the error.
        /// </summary>
        /// <value>The description of the error.</value>
        public string ErrorDescription
        {
            get { return _errorDescription; }
            set { _errorDescription = value; }
        }

        /// <summary>
        /// Gets or sets a Uri pointing to a web resource with more details about the error.
        /// </summary>
        /// <value>A Uri pointing to a web resource with more details about the error.</value>
        public Uri ErrorUri
        {
            get { return _errorUri; }
            set { _errorUri = value; }
        }

        /// <summary>
        /// Gets or sets the realm of the web resource.
        /// </summary>
        /// <value>The realm of the web resource.</value>
        public string Realm
        {
            get { return _realm; }
            set { _realm = value; }
        }

        /// <summary>
        /// Gets or sets the scope over which the token must be valid for the resource to accept it.
        /// </summary>
        /// <value>The scope over which the token must be valid.</value>
        public string Scope
        {            
            get { return _scope; }
            set { _scope = value; }
        }
    }
}