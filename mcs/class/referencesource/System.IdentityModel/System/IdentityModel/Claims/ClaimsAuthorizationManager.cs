//-----------------------------------------------------------------------
// <copyright file="ClaimsAuthorizationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Security.Claims
{
    using System.IdentityModel;
    using System.IdentityModel.Configuration;
    using System.Security.Claims;
    using System.Xml;

    /// <summary>
    /// Defines the base implementation for a claims authorization manager.
    /// </summary>
    public class ClaimsAuthorizationManager : ICustomIdentityConfiguration
    {
        /// <summary>
        /// When implemented in a derived class, this method will authorize the subject specified in the
        /// context to perform the specified action on the specified resource.
        /// </summary>
        /// <param name="context"><see cref="AuthorizationContext"/> that encapsulates the subject, resource, and action.</param>
        /// <returns>true if authorized, false otherwise.</returns>
        public virtual bool CheckAccess(AuthorizationContext context)
        {
            return true;
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
