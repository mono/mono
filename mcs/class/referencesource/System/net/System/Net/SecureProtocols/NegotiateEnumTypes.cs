/*++
Copyright (c) Microsoft Corporation

Module Name:

    NegoEnumProperties.cs

--*/

namespace System.Net.Security {


    
    //
    // WebRequest - specific authentication flags
    //
    public enum AuthenticationLevel {
        None = 0,
        MutualAuthRequested = 1,    // default setting
        MutualAuthRequired  = 2
    }

    // This will request security properties of a NegotiateStream
    public enum ProtectionLevel
    {
        // Used only with Negotiate on Win9x platform
        None = 0,

        // Data integrity only
        Sign = 1,

        // Both data confidentiality and integrity
        EncryptAndSign = 2
    }

}

