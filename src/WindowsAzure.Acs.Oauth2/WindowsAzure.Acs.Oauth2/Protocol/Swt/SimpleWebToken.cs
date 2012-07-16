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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Claims;

namespace WindowsAzure.Acs.Oauth2.Protocol.Swt
{
    /// <summary>
    /// This class represents the token format for the SimpleWebToken.
    /// </summary>
    public class SimpleWebToken 
        : SecurityToken
    {
        static DateTime swtBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0); // per SWT psec

        string _id;        
        Uri _audienceUri;
        List<Claim> _claims;
        string _issuer;
        DateTime _expiresOn;
        string _signature;
        string _unsignedString; 
        DateTime _validFrom;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWebToken"/> class.
        /// This is internal contructor is only called from the <see cref="SimpleWebTokenHandler"/> when reading a token received from the wire.
        /// </summary>
        /// <param name="audienceUri">The Audience Uri of the token.</param>
        /// <param name="issuer">The issuer of the token.</param>
        /// <param name="expiresOn">The expiry time of the token.</param>
        /// <param name="claims">The claims in the token.</param>
        /// <param name="signature">The signature of the token.</param>
        /// <param name="unsignedString">The serialized token without its signature.</param>
        internal SimpleWebToken( Uri audienceUri, string issuer, DateTime expiresOn, List<Claim> claims, string signature, string unsignedString )
            : this()
        {
            _audienceUri = audienceUri;
            _issuer = issuer;
            _expiresOn = expiresOn;
            _signature = signature;
            _unsignedString = unsignedString;
            _claims = claims;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWebToken"/> class.
        /// </summary>
        public SimpleWebToken()
        {
            _validFrom = swtBaseTime;
            _id = null;
        }

        /// <summary>
        /// Gets the Id of the token.
        /// </summary>
        /// <value>The Id of the token.</value>
        public override string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the keys associated with this token.
        /// </summary>
        /// <value>The keys associated with this token.</value>
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return new ReadOnlyCollection<SecurityKey>( new List<SecurityKey>() ); }
        }
        
        /// <summary>
        /// Gets the time from when the token is valid.
        /// </summary>
        /// <value>The time from when the token is valid.</value>
        public override DateTime ValidFrom
        {           
            get { return _validFrom; }            
        }

        /// <summary>
        /// Gets the time when the token expires.
        /// </summary>
        /// <value>The time upto which the token is valid.</value>
        public override DateTime ValidTo
        {           
            get { return _expiresOn; }           
        }

        /// <summary>
        /// Gets the AudienceUri for the token.
        /// </summary>
        /// <value>The audience Uri of the token.</value>
        public Uri AudienceUri
        {
            get { return _audienceUri; }            
        }

        /// <summary>
        /// Gets the Issuer for the token.
        /// </summary>
        /// <value>The issuer for the token.</value>
        public string Issuer
        {
            get { return _issuer; }
        }

        /// <summary>
        /// Gets the Claims in the token.
        /// </summary>
        /// <value>The Claims in the token.</value>
        public List<Claim> Claims
        {
            get { return _claims; }            
        }

        /// <summary>
        /// Verifies the signature of the incoming token.
        /// </summary>
        /// <param name="key">The key used for signing.</param>
        /// <returns>true if the signatures match, false otherwise.</returns>
        public bool SignVerify(byte[] key)
        {
            if ( key == null )
            {
                throw new ArgumentNullException( "key" );
            }

            if ( _signature == null || _unsignedString == null )
            {
                throw new InvalidOperationException( "Token has never been signed" );
            }

            string verifySignature;

            using ( HMACSHA256 signatureAlgorithm = new HMACSHA256( key ) )
            {
                verifySignature = Convert.ToBase64String( signatureAlgorithm.ComputeHash( Encoding.ASCII.GetBytes( _unsignedString ) ) );
            }

            if ( string.CompareOrdinal( verifySignature, _signature ) == 0 )
            {
                return true;
            }

            return false;
        }
    }    
}