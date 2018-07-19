//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace System.Security.Claims
{
    /// <summary>
    /// The authentication information that an authority asserted when creating a token for a subject.
    /// </summary>
    public class AuthenticationInformation
    {
        string _address;
        Collection<AuthenticationContext> _authContexts;
        string _dnsName;
        DateTime? _notOnOrAfter;
        string _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationInformation"/> class.
        /// </summary>
        public AuthenticationInformation()
        {
            _authContexts = new Collection<AuthenticationContext>();
        }
        
        /// <summary>
        /// Gets or sets the address of the authority that created the token.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        /// <summary>
        /// Gets the <see cref="AuthorizationContext"/> used by the authenticating authority when issuing tokens.
        /// </summary>
        public Collection<AuthenticationContext> AuthorizationContexts
        {
            get { return _authContexts; }
        }

        /// <summary>
        /// Gets or sets the DNS name of the authority that created the token.
        /// </summary>
        public string DnsName
        {
            get { return _dnsName; }
            set { _dnsName = value; }
        }

        /// <summary>
        /// Gets or sets the time that the session referred to in the session index MUST be considered ended.
        /// </summary>
        public DateTime? NotOnOrAfter
        {
            get { return _notOnOrAfter; }
            set { _notOnOrAfter = value; }
        }

        /// <summary>
        /// Gets or sets the session index that describes the session between the authority and the client.
        /// </summary>
        public string Session
        {
            get { return _session; }
            set { _session = value; }
        }
    }
}
