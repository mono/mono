//------------------------------------------------------------------------------
// <copyright file="AuthenticationSchemeSelector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    public delegate AuthenticationSchemes AuthenticationSchemeSelector(HttpListenerRequest httpRequest);

}

