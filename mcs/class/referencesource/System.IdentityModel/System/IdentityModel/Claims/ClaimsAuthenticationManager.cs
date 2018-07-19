//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.Xml;

namespace System.Security.Claims
{
    /// <summary>
    /// A Simple ClaimsAuthenticationManager that echoes back the incoming ClaimsIdentities.
    /// </summary>
    public class ClaimsAuthenticationManager : ICustomIdentityConfiguration
    {
        /// <summary>
        /// The method echoes back the incoming ClaimsIdentities.
        /// </summary>
        /// <param name="resourceName">The address to which the request was sent.</param>
        /// <param name="incomingPrincipal"><see cref="ClaimsPrincipal"/> presented by the client (in the form of a 
        /// SecurityToken) to access the resource.</param>
        /// <returns>The ClaimsPrincipal given to the method.</returns>
        public virtual ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            return incomingPrincipal;
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
