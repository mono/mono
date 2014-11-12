// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

using System;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Text;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Principal
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public sealed class IdentityNotMappedException : SystemException
    {

        private IdentityReferenceCollection unmappedIdentities;
    
        public IdentityNotMappedException()
            : base( Environment.GetResourceString( "IdentityReference_IdentityNotMapped" ))
        {
        }

        public IdentityNotMappedException( string message )
            : base( message )
        {
        }
    
        public IdentityNotMappedException( String message, Exception inner )
            : base( message, inner )
        {
        }

        internal IdentityNotMappedException(string message, IdentityReferenceCollection unmappedIdentities)
            : this( message ) 
        {
            this.unmappedIdentities = unmappedIdentities;
        }

        internal IdentityNotMappedException( SerializationInfo info, StreamingContext context )
            : base ( info, context ) {}

        [System.Security.SecurityCritical]  // auto-generated_required
        public override void GetObjectData( SerializationInfo serializationInfo, StreamingContext streamingContext ) 
        {
            base.GetObjectData(serializationInfo, streamingContext);
        }

        public IdentityReferenceCollection UnmappedIdentities {
            get {
                if (unmappedIdentities == null) {
                    unmappedIdentities = new IdentityReferenceCollection();
                }
                return unmappedIdentities;
            }
        }
    }
    
}
