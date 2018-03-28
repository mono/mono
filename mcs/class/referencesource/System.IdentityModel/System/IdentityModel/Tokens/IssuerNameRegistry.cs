//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Xml;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Interface that defines the name service that returns that issuer name
    /// of a given token as string. 
    /// </summary>
    public abstract class IssuerNameRegistry : ICustomIdentityConfiguration
    {
        /// <summary>
        /// When implemented in the derived class, the method returns the issuer name of the given 
        /// SecurityToken's issuer.  Implementations must return a non-null and non-empty string to
        /// identify a recognized issuer, or a null string to identify an unrecognized issuer.
        /// </summary>
        /// <param name="securityToken">The SecurityToken whose name is requested.</param>
        /// <returns>Issuer name as a string.</returns>
        public abstract string GetIssuerName( SecurityToken securityToken );

        /// <summary>
        /// When implemented in the derived class the method returns the issuer name 
        /// of the given SecurityToken's issuer. The requested issuer name may be considered
        /// in determining the issuer's name.
        /// </summary>
        /// <param name="securityToken">The SecurityToken whose name is requested.</param>
        /// <param name="requestedIssuerName">Input to determine the issuer name</param>
        /// <remarks>The default implementation ignores the requestedIsserName parameter and simply calls the 
        /// GetIssuerName( SecurityToken securityToken ) method</remarks>
        /// <returns>Issuer name as a string.</returns>
        public virtual string GetIssuerName( SecurityToken securityToken, string requestedIssuerName )
        {
            return GetIssuerName( securityToken );
        }

        /// <summary>
        /// This function returns the default issuer name to be used for Windows claims.
        /// </summary>
        /// <returns>Issuer name as a string.</returns>
        public virtual string GetWindowsIssuerName()
        {
            return ClaimsIdentity.DefaultIssuer;
        }

        /// <summary>
        /// Load custom configuration from Xml
        /// </summary>
        /// <param name="nodelist">Custom configuration elements</param>
        public virtual void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.ID0023, this.GetType().AssemblyQualifiedName)));
        }
    }
}
