//------------------------------------------------------------------------------
// <copyright file="IServerConfig2.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    interface IServerConfig2 {
        bool IsWithinApp(string virtualPath);
    }
}

