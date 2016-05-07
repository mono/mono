//------------------------------------------------------------------------------
// <copyright file="STATDATA.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.InteropServices.ComTypes {

    using System;

    /// <devdoc>
    /// </devdoc>
    public struct STATDATA {
        public FORMATETC       formatetc;
        public ADVF            advf;
        public IAdviseSink     advSink;
        public int             connection;
    }
}


