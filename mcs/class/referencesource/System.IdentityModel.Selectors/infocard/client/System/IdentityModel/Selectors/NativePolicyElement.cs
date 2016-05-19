//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Runtime.InteropServices;

    //
    // Summary:
    //  Maps directly to the POLICY_ELEMENT struct in native code.
    //
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePolicyElement
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public String targetEndpointAddress;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String issuerEndpointAddress;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String issuedTokenParameters;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String policyNoticeLink;
        [MarshalAs(UnmanagedType.U4)]
        public int policyNoticeVersion;
        [MarshalAs(UnmanagedType.Bool)]
        public bool isManagedCardProvider;
    }
}
