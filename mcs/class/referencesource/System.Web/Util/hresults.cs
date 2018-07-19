//------------------------------------------------------------------------------
// <copyright file="HResults.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    // Note: FACILITY_URT is defined as 0x13 (0x8013xxxx).  Within that
    // range, 0x1yyy is for Runtime errors (used for Security, Metadata, etc).
    // In that subrange, 0x15zz and 0x16zz have been allocated for classlib-type 
    // HResults. Also note that some of our HResults have to map to certain 
    // COM HR's, etc.
    
    internal static class HResults {
        internal const int S_OK                   = 0;
        internal const int S_FALSE                = 1;
        internal const int E_NOTIMPL              = unchecked((int)0x80004001); 
        internal const int E_POINTER              = unchecked((int)0x80004003); 
        internal const int E_FAIL                 = unchecked((int)0x80004005);     
        internal const int E_FILENOTFOUND         = unchecked((int)0x80070002);     
        internal const int E_PATHNOTFOUND         = unchecked((int)0x80070003);     
        internal const int E_ACCESSDENIED         = unchecked((int)0x80070005);
        internal const int E_INVALID_DATA         = unchecked((int)0x8007000D);
        internal const int E_OUTOFMEMORY          = unchecked((int)0x8007000E);
        internal const int E_INVALIDARG           = unchecked((int)0x80070057);
        internal const int E_INSUFFICIENT_BUFFER  = unchecked((int)0x8007007A);
        internal const int E_NOT_SET              = unchecked((int)0x80070490);
        internal const int WSAECONNABORTED        = unchecked((int)0x80072745);
        internal const int WSAECONNRESET          = unchecked((int)0x80072746);
        internal const int ERROR_TOO_MANY_CMDS    = unchecked((int)0x80070038);
        internal const int ERROR_NOT_SUPPORTED    = unchecked((int)0x80070032);
    }
}
