//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace System.IdentityModel.Selectors
{
    //
    // Summary
    //  This structure is used to marshal the optional data associated with a request 
    //  to the native api
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct RpcInfoCardOptions
    {
        public bool UISuppression;        // If UI supression is requested
        public int cchKeyLength;         // Length of the key
        [MarshalAs(UnmanagedType.LPWStr)]
        public string keyType;              // Type of the key
        public int cbKeyValue;           // Size of the key buffer
        public byte[] keyValue;             // Buffer containing the key
    }
}

