//------------------------------------------------------------------------------
// <copyright file="IPrincipalContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Security.Principal;

    // A container that allows getting and setting the current principal.

    internal interface IPrincipalContainer {

        IPrincipal Principal {
            get;
            set;
        }

    }
}
