//-----------------------------------------------------------------------
// <copyright file="Renewing.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// This defines the Renewing element inside the RequestSecurityToken message. 
    /// </summary>
    /// <remarks>
    /// The presence of Renewing element indicates the token issuer that the requested token
    /// can be renewed if allow attribute is true, and the token can be renewed after
    /// it expires if ok is true.
    /// </remarks>
    public class Renewing
    {
        bool _allowRenewal = true;
        bool _okForRenewalAfterExpiration; // false by default

        /// <summary>
        /// Initializes a renewing object with AllowRenewal attribute equals to true, and 
        /// OkForRenewalAfterExpiration attribute equals false.
        /// </summary>
        public Renewing()
        {
        }

        /// <summary>
        /// Initializes a renewing object with specified allow and OK attributes.
        /// </summary>
        public Renewing( bool allowRenewal, bool okForRenewalAfterExpiration )
        {
            _allowRenewal = allowRenewal;
            _okForRenewalAfterExpiration = okForRenewalAfterExpiration;
        }

        /// <summary>
        /// Returns true if it is allowed to renew this token.
        /// </summary>
        /// <remarks>
        /// This optional boolean attribute is used to request a renewable token. Default value is true. 
        /// </remarks>
        /// <devdocs>
        /// Please refer to section 7 in the WS-Trust spec for more details.
        /// </devdocs>
        public bool AllowRenewal
        {
            get
            {
                return _allowRenewal;
            }
            set
            {
                _allowRenewal = value;
            }
        }

        /// <summary>
        /// Returns true if the requested token can be renewed after it expires.
        /// </summary>
        /// <remarks>
        /// This optional boolean attriubte is used to indicate that a renewable token is acceptable if
        /// the requested duration exceeds the limit of the issuance service. That is, if true, then the 
        /// token can be renewed after their expiration. Default value is false for security reason. 
        /// </remarks>
        /// <devdocs>
        /// Please refer to section 7 in the WS-Trust spec for more details.
        /// </devdocs>
        public bool OkForRenewalAfterExpiration
        {
            get
            {
                return _okForRenewalAfterExpiration;
            }
            set
            {
                _okForRenewalAfterExpiration = value;
            }
        }
    }
}
