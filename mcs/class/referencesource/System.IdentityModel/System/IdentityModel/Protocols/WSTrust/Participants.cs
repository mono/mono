//-----------------------------------------------------------------------
// <copyright file="Participants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the &lt;wst:Participants> elements. This element is an extension to
    /// the &lt;wst:RequestSecurityToken element for passing information about which 
    /// parties are authorized to participate in the use of the token.
    /// </summary>
    public class Participants
    {
        EndpointReference _primary;
        List<EndpointReference> _participant = new List<EndpointReference>();

        /// <summary>
        /// Gets the Primary user of the Issued Token.
        /// </summary>
        public EndpointReference Primary
        {
            get
            {
                return _primary;
            }
            set
            {
                _primary = value;
            }
        }

        /// <summary>
        /// Gets the list of Participants who are allowed to use
        /// the token.
        /// </summary>
        public List<EndpointReference> Participant
        {
            get
            {
                return _participant;
            }
        }
    }
}
