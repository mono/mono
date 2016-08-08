//------------------------------------------------------------------------------
// <copyright file="FormsIdentity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * FormsIdentity
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Claims;
    using System.Security.Permissions;
    using System.Security.Principal;

    /// <devdoc>
    ///    This class is an IIdentity derived class
    ///    used by FormsAuthenticationModule. It provides a way for an application to
    ///    access the cookie authentication ticket.
    /// </devdoc>
    [Serializable]
    [ComVisible(false)]
    public class FormsIdentity : ClaimsIdentity {
      
        /// <devdoc>
        ///    The name of the identity (in this case, the
        ///    passport user name).
        /// </devdoc>
        public  override String                       Name { get { return _Ticket.Name;}}

        /// <devdoc>
        ///    The type of the identity (in this case,
        ///    "Forms").
        /// </devdoc>
        public  override String                       AuthenticationType { get { return "Forms";}}

        /// <devdoc>
        ///    Indicates whether or not authentication took
        ///    place.
        /// </devdoc>
        public  override bool                         IsAuthenticated { get { return true;}}
        
        private FormsAuthenticationTicket _Ticket;

        /// <devdoc>
        ///    Returns the FormsAuthenticationTicket
        ///    associated with the current request.
        /// </devdoc>
        public  FormsAuthenticationTicket   Ticket { get { return _Ticket;}}

        public override IEnumerable<Claim> Claims
        {
            get
            {
                return base.Claims;
            }
        }

        /// <devdoc>
        ///    Constructor.
        /// </devdoc>
        public FormsIdentity (FormsAuthenticationTicket ticket) {
            if (ticket == null)
                throw new ArgumentNullException("ticket");
            
            _Ticket = ticket;

            AddNameClaim();
        }

        /// <devdoc>
        ///    Constructor.
        /// </devdoc>
        protected FormsIdentity(FormsIdentity identity)
            : base((IIdentity)identity)
        {
            _Ticket = identity._Ticket;
        }

        /// <devdoc>
        /// Returns a new instance of <see cref="FormsIdentity"/> with values copied from this object.
        /// </devdoc>
        public override ClaimsIdentity Clone()
        {
            return new FormsIdentity(this);
        }

        [OnDeserialized()]
        private void OnDeserializedMethod(StreamingContext context)
        {
            // FormIdentities that have been deserialized from a .net 4.0 runtime, will not have any claims. 
            // In this case add a name claim, otherwise assume it was deserialized.

            bool claimFound = false;
            foreach (Claim c in base.Claims)
            {
                claimFound = true;
                break;
            }

            if (!claimFound)
            {
                AddNameClaim();
            }
        }

        [SecuritySafeCritical]
        private void AddNameClaim()
        {
            if (_Ticket != null && _Ticket.Name != null)
            {
                base.AddClaim(new Claim(base.NameClaimType, _Ticket.Name, ClaimValueTypes.String, "Forms", "Forms", this));
            }
        }
    }
}
