//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel
{
    // #define ISC_REQ_DELEGATE                0x00000001
    // #define ISC_REQ_MUTUAL_AUTH             0x00000002
    // #define ISC_REQ_REPLAY_DETECT           0x00000004
    // #define ISC_REQ_SEQUENCE_DETECT         0x00000008
    // #define ISC_REQ_CONFIDENTIALITY         0x00000010
    // #define ISC_REQ_USE_SESSION_KEY         0x00000020
    // #define ISC_REQ_PROMPT_FOR_CREDS        0x00000040
    // #define ISC_REQ_USE_SUPPLIED_CREDS      0x00000080
    // #define ISC_REQ_ALLOCATE_MEMORY         0x00000100
    // #define ISC_REQ_USE_DCE_STYLE           0x00000200
    // #define ISC_REQ_DATAGRAM                0x00000400
    // #define ISC_REQ_CONNECTION              0x00000800
    // #define ISC_REQ_CALL_LEVEL              0x00001000
    // #define ISC_REQ_FRAGMENT_SUPPLIED       0x00002000
    // #define ISC_REQ_EXTENDED_ERROR          0x00004000
    // #define ISC_REQ_STREAM                  0x00008000
    // #define ISC_REQ_INTEGRITY               0x00010000
    // #define ISC_REQ_IDENTIFY                0x00020000
    // #define ISC_REQ_NULL_SESSION            0x00040000
    // #define ISC_REQ_MANUAL_CRED_VALIDATION  0x00080000
    // #define ISC_REQ_RESERVED1               0x00100000
    // #define ISC_REQ_FRAGMENT_TO_FIT         0x00200000
    // #define ISC_REQ_HTTP                    0x10000000

    // #define ASC_REQ_DELEGATE                0x00000001
    // #define ASC_REQ_MUTUAL_AUTH             0x00000002
    // #define ASC_REQ_REPLAY_DETECT           0x00000004
    // #define ASC_REQ_SEQUENCE_DETECT         0x00000008
    // #define ASC_REQ_CONFIDENTIALITY         0x00000010
    // #define ASC_REQ_USE_SESSION_KEY         0x00000020
    // #define ASC_REQ_ALLOCATE_MEMORY         0x00000100
    // #define ASC_REQ_USE_DCE_STYLE           0x00000200
    // #define ASC_REQ_DATAGRAM                0x00000400
    // #define ASC_REQ_CONNECTION              0x00000800
    // #define ASC_REQ_CALL_LEVEL              0x00001000
    // #define ASC_REQ_EXTENDED_ERROR          0x00008000
    // #define ASC_REQ_STREAM                  0x00010000
    // #define ASC_REQ_INTEGRITY               0x00020000
    // #define ASC_REQ_LICENSING               0x00040000
    // #define ASC_REQ_IDENTIFY                0x00080000
    // #define ASC_REQ_ALLOW_NULL_SESSION      0x00100000
    // #define ASC_REQ_ALLOW_NON_USER_LOGONS   0x00200000
    // #define ASC_REQ_ALLOW_CONTEXT_REPLAY    0x00400000
    // #define ASC_REQ_FRAGMENT_TO_FIT         0x00800000
    // #define ASC_REQ_FRAGMENT_SUPPLIED       0x00002000
    // #define ASC_REQ_NO_TOKEN                0x01000000
    // #define ASC_REQ_HTTP                    0x10000000

    [Flags]
    internal enum SspiContextFlags 
    {
        Zero            = 0,
        Delegate        = 0x00000001,
        MutualAuth      = 0x00000002,
        ReplayDetect    = 0x00000004,
        SequenceDetect  = 0x00000008,
        Confidentiality = 0x00000010,
        UseSessionKey   = 0x00000020,
        AllocateMemory  = 0x00000100,
        InitStream      = 0x00008000,
        AcceptStream    = 0x00010000,
        // Client applications requiring extended error messages specify the
        // ISC_REQ_EXTENDED_ERROR flag when calling the InitializeSecurityContext
        // Server applications requiring extended error messages set
        // the ASC_REQ_EXTENDED_ERROR flag when calling AcceptSecurityContext.
        InitExtendedError    = 0x00004000,
        AcceptExtendedError  = 0x00008000,
        InitIdentify                = 0x00020000, // ISC_REQ_IDENTIFY
        AcceptIdentify              = 0x00080000, // ASC_REQ_IDENTIFY
        InitManualCredValidation    = 0x00080000, // ISC_REQ_MANUAL_CRED_VALIDATION
        InitAnonymous               = 0x00040000, // ISC_REQ_NULL_SESSION
        AcceptAnonymous             = 0x00100000,    // ASC_REQ_ALLOW_NULL_SESSION
        ChannelBindingProxyBindings               = 0x04000000,   // ASC_REQ_PROXY_BINDINGS
        ChannelBindingAllowMissingBindings        = 0x10000000    // ASC_REQ_ALLOW_MISSING_BINDINGS
    }
}
