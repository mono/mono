//------------------------------------------------------------------------------
// <copyright file="AuthenticationSchemeSelector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
#if SECURITY_DEP
    public delegate AuthenticationSchemes AuthenticationSchemeSelector(HttpListenerRequest httpRequest);
#endif
}

