//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace System.IdentityModel.Selectors
{
    //
    // Summary
    //  This structure encapsulates the information contained within the request for 
    //  a security token to marshal it to the native api
    //
    [StructLayout(LayoutKind.Sequential)]
    struct RpcTokenRequestDetail
    {
        public int uriLength;          // Length of the recipient Uri in bytes
        [MarshalAs(UnmanagedType.LPWStr)]
        public string recipientUri;       // Uri for the recipient
        public int cbRecipientToken;   // Size of the recipient security token buffer    
        public byte[] recipientToken;     // Buffer containing the recipient security token      
        public int cchPolicy;          // Chracter count of the Policy buffer 
        [MarshalAs(UnmanagedType.LPWStr)]
        public string policy;             // Policy 
    }
}

