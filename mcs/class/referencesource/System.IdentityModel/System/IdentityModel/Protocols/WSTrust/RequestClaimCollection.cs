//-----------------------------------------------------------------------
// <copyright file="RequestClaimCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// This class is used to represent a collection of the RequestClaims inside RequestSecurityToken.
    /// </summary>
    public class RequestClaimCollection : Collection<RequestClaim>
    {
        string _dialect = WSIdentityConstants.Dialect;

        /// <summary>
        /// Instantiates an empty requested claim collection.
        /// </summary>
        public RequestClaimCollection()
        {
        }

        /// <summary>
        /// Gets or sets the Claims dialect attribute.
        /// </summary>
        public string Dialect
        {
            get 
            { 
                return _dialect; 
            }
            set 
            { 
                _dialect = value; 
            }
        }
    }
}
