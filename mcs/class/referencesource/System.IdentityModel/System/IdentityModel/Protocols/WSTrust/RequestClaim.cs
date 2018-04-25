//-----------------------------------------------------------------------
// <copyright file="RequestClaim.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// This class is used to represent the Request Claims collection inside RequestSecurityToken.
    /// Indicate whether the claim is optional or not. 
    /// </summary>
    public class RequestClaim
    {
        string _claimType;
        bool _isOptional;
        string _value;

        /// <summary>
        /// Instantiates a required RequestClaim instance with ClaimType Uri. 
        /// </summary>
        /// <param name="claimType">ClaimType Uri attribute.</param>
        public RequestClaim(string claimType)
            : this(claimType, false)
        {
        }

        /// <summary>
        /// Instantiates a RequestClaim instance with ClaimType Uri and inidicates whether it is 
        /// optional.
        /// </summary>
        /// <param name="claimType">The ClaimType Uri attribute.</param>
        /// <param name="isOptional">The ClaimType Optional attribute.</param>
        public RequestClaim(string claimType, bool isOptional)
            : this(claimType, isOptional, null)
        {
        }

        /// <summary>
        /// Instantiates a RequestClaim instance with ClaimType Uri, the flag to inidicate whether it is 
        /// optional and the value of the request.
        /// </summary>
        /// <param name="claimType">The ClaimType Uri attribute.</param>
        /// <param name="isOptional">The ClaimType Optional attribute.</param>
        /// <param name="value">The actual value of the claim.</param>
        public RequestClaim(string claimType, bool isOptional, string value)
        {
            if (string.IsNullOrEmpty(claimType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID0006), "claimType"));
            }

            _claimType = claimType;
            _isOptional = isOptional;
            _value = value;
        }

        /// <summary>
        /// Gets ClaimType uri attribute.
        /// </summary>
        public string ClaimType
        {
            get
            {
                return _claimType;
            }
        }

        /// <summary>
        /// Gets ClaimType optional attribute.
        /// </summary>
        public bool IsOptional
        {
            get
            {
                return _isOptional;
            }
            set
            {
                _isOptional = value;
            }
        }

        /// <summary>
        /// Gets ClaimType value element.
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}
