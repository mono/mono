//------------------------------------------------------------------------------
// <copyright file="IConfigSystem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {

    using System;
    using System.Configuration;

    public interface IConfigSystem {
        void Init(Type typeConfigHost, params object[] hostInitParams);

        IInternalConfigHost Host {get;}
        IInternalConfigRoot Root {get;}
    }
}
