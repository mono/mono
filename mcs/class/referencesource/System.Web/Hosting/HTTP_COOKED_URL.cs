//------------------------------------------------------------------------------
// <copyright file="HTTP_COOKED_URL.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Runtime.InteropServices;

    // http://msdn.microsoft.com/en-us/library/windows/desktop/aa364519.aspx
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct HTTP_COOKED_URL {
        // WARNING: Lengths are specified in bytes, not wide chars!
        // Length does not include a null terminator.
        internal readonly ushort FullUrlLength;
        internal readonly ushort HostLength;
        internal readonly ushort AbsPathLength;
        internal readonly ushort QueryStringLength;

        // WARNING: pFullUrl is the only string guaranteed by HTTP.SYS
        // to be null-terminated (though see comment above re: length).
        // Other fields point within pFullUrl so may not be null-terinated.
        internal readonly char* pFullUrl;
        internal readonly char* pHost;
        internal readonly char* pAbsPath;
        internal readonly char* pQueryString;
    }
}
