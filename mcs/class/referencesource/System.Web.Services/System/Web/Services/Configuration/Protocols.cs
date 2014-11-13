//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration
{
    using System.ComponentModel;
    
    [Flags]
    public enum WebServiceProtocols {
        Unknown = 0x0,
        HttpSoap = 0x1,
        HttpGet = 0x2,
        HttpPost = 0x4,
        Documentation = 0x8,
        HttpPostLocalhost = 0x10,
        HttpSoap12 = 0x20,

        // composite flag
        AnyHttpSoap = 0x21,
    }
}
