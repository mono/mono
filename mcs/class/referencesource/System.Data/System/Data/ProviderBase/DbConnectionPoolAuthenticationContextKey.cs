//------------------------------------------------------------------------------
// <copyright file="DbConnectionPoolAuthenticationContextKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">harsudan</owner>
// <owner current="true" primary="false">yuronhe</owner>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace System.Data.ProviderBase
{
    /// <summary>
    /// Represents the key of dbConnectionPoolAuthenticationContext.
    /// All data members should be immutable and so, hashCode is pre-computed.
    /// </summary>
    sealed internal class DbConnectionPoolAuthenticationContextKey
    {
        /// <summary>
        /// Security Token Service Authority.
        /// </summary>
        private readonly string _stsAuthority;

        /// <summary>
        /// Service Principal Name.
        /// </summary>
        private readonly string _servicePrincipalName;

        /// <summary>
        /// Pre-Computed Hash Code.
        /// </summary>
        private readonly int _hashCode;

        internal string StsAuthority {
            get {
                return _stsAuthority;
            }
        }

        internal string ServicePrincipalName {
            get {
                return _servicePrincipalName;
            }
        }

        /// <summary>
        /// Constructor for the type.
        /// </summary>
        /// <param name="stsAuthority">Token Endpoint URL</param>
        /// <param name="servicePrincipalName">SPN representing the SQL service in an active directory.</param>
        internal DbConnectionPoolAuthenticationContextKey(string stsAuthority, string servicePrincipalName) {
            Debug.Assert(!string.IsNullOrWhiteSpace(stsAuthority));
            Debug.Assert(!string.IsNullOrWhiteSpace(servicePrincipalName));

            _stsAuthority = stsAuthority;
            _servicePrincipalName = servicePrincipalName;

            // Pre-compute hash since data members are not going to change.
            _hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Override the default Equals implementation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            DbConnectionPoolAuthenticationContextKey otherKey = obj as DbConnectionPoolAuthenticationContextKey;
            if (otherKey == null) {
                return false;
            }

            return (String.Equals(StsAuthority, otherKey.StsAuthority, StringComparison.InvariantCultureIgnoreCase)
                && String.Equals(ServicePrincipalName, otherKey.ServicePrincipalName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Override the default GetHashCode implementation.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return _hashCode;
        }

        /// <summary>
        /// Compute the hash code for this object.
        /// </summary>
        /// <returns></returns>
        private int ComputeHashCode() {
            int hashCode = 33;

            unchecked
            {
                hashCode = (hashCode * 17) + StsAuthority.GetHashCode();
                hashCode = (hashCode * 17) + ServicePrincipalName.GetHashCode();
            }

            return hashCode;
        }
    }
}
