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
    /// This exception is thrown when an expired access token is received.
    /// </summary>
    public class ExpiredTokenReceivedException : Exception
    {
        static string _errorCode = "SWT401";

        string _errorDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredTokenReceivedException"/> class.
        /// </summary>
        /// <param name="errorDescription">A description of the error that occured.</param>
        public ExpiredTokenReceivedException(string errorDescription)
            : base()
        {
            _errorDescription = errorDescription;
        }

        /// <summary>
        /// Gets the OAuth error code corresponding to this exception.
        /// </summary>
        /// <value>The OAuth error code.</value>
        public string ErrorCode
        {
            get { return _errorCode; }
        }

        /// <summary>
        /// Gets the description of the error which caused this exception.
        /// </summary>
        /// <value>A description of the error that occured.</value>
        public string ErrorDescription
        {
            get { return _errorDescription; }
        }
    }
}