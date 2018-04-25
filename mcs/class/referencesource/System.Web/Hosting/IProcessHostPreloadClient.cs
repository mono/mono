//------------------------------------------------------------------------------
// <copyright file="IProcessHostPreloadClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Defines the interface that must be defined by the WCF class that pre-loads the
 * application for non-HTTP activation
 *
 * Copyright (c) 2008 Microsoft Corporation
 */

namespace System.Web.Hosting {

    public interface IProcessHostPreloadClient {
        void Preload(string[] parameters);
    }
}
