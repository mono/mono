// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  PermissionRequestEvidence.cs
// 
// <OWNER>[....]</OWNER>
//
//  Encapsulation of permission request as an evidence type.
//

namespace System.Security.Policy {
    using System.Runtime.Remoting;
    using System;
    using System.IO;
    using System.Security.Util;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [Obsolete("Assembly level declarative security is obsolete and is no longer enforced by the CLR by default. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
    public sealed class PermissionRequestEvidence : EvidenceBase
    {
        private PermissionSet m_request;
        private PermissionSet m_optional;
        private PermissionSet m_denied;

        // These fields are not used, they are here for serialization compatibility with Whidbey
#pragma warning disable 169
        private String m_strRequest;
        private String m_strOptional;
        private String m_strDenied;
#pragma warning restore 169
    
        public PermissionRequestEvidence(PermissionSet request, PermissionSet optional, PermissionSet denied)
        {
            if (request == null)
                m_request = null;
            else
                m_request = request.Copy();
                
            if (optional == null)
                m_optional = null;
            else
                m_optional = optional.Copy();
                
            if (denied == null)
                m_denied = null;
            else
                m_denied = denied.Copy();
        }
    
        public PermissionSet RequestedPermissions
        {
            get
            {
                return m_request;
            }
        }

        public PermissionSet OptionalPermissions
        {
            get
            {
                return m_optional;
            }
        }

        public PermissionSet DeniedPermissions
        {
            get
            {
                return m_denied;
            }
        }

        public override EvidenceBase Clone()
        {
            return Copy();
        }

        public PermissionRequestEvidence Copy()
        {
            return new PermissionRequestEvidence(m_request, m_optional, m_denied);
        }

        internal SecurityElement ToXml() {
            SecurityElement root = new SecurityElement( "System.Security.Policy.PermissionRequestEvidence" );
            // If you hit this assert then most likely you are trying to change the name of this class. 
            // This is ok as long as you change the hard coded string above and change the assert below.
            Contract.Assert( this.GetType().FullName.Equals( "System.Security.Policy.PermissionRequestEvidence" ), "Class name changed!" );

            root.AddAttribute( "version", "1" );
            
            SecurityElement elem;
            
            if (m_request != null)
            {
                elem = new SecurityElement( "Request" );
                elem.AddChild( m_request.ToXml() );
                root.AddChild( elem );
            }
                
            if (m_optional != null)
            {
                elem = new SecurityElement( "Optional" );
                elem.AddChild( m_optional.ToXml() );
                root.AddChild( elem );
            }
                
            if (m_denied != null)
            {
                elem = new SecurityElement( "Denied" );
                elem.AddChild( m_denied.ToXml() );
                root.AddChild( elem );
            }
            
            return root;
        }

        public override String ToString()
        {
            return ToXml().ToString();
        }
    }
}
