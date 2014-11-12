// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// GacInstalled is an IIdentity representing whether or not an assembly is installed in the Gac
//

namespace System.Security.Policy {
    using System.Runtime.Remoting;
    using System;
    using System.Security;
    using System.Security.Util;
    using System.IO;
    using System.Collections;
    using GacIdentityPermission = System.Security.Permissions.GacIdentityPermission;
    using System.Runtime.CompilerServices;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class GacInstalled : EvidenceBase, IIdentityPermissionFactory
    {
        public GacInstalled()
        {
        }

        public IPermission CreateIdentityPermission( Evidence evidence )
        {
            return new GacIdentityPermission();
        }

        public override bool Equals(Object o)
        {
            return o is GacInstalled;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override EvidenceBase Clone()
        {
            return new GacInstalled();
        }

        public Object Copy()
        {
            return Clone();
        }

        internal SecurityElement ToXml()
        {
            SecurityElement elem = new SecurityElement( this.GetType().FullName );
            elem.AddAttribute( "version", "1" );
            return elem;
        }

        public override String ToString()
        {
            return ToXml().ToString();
        }
    }
}
