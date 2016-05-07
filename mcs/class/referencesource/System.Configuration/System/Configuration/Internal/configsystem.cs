//------------------------------------------------------------------------------
// <copyright file="ConfigSystem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {
    using System;
    using System.Configuration;

    // The runtime config system
    internal class ConfigSystem : IConfigSystem {
        IInternalConfigRoot _configRoot;
        IInternalConfigHost _configHost;

        void IConfigSystem.Init(Type typeConfigHost, params object[] hostInitParams) {
            _configRoot = new InternalConfigRoot();
            _configHost = (IInternalConfigHost) TypeUtil.CreateInstanceWithReflectionPermission(typeConfigHost);

            _configRoot.Init(_configHost, false);
            _configHost.Init(_configRoot, hostInitParams);
        }

        IInternalConfigHost IConfigSystem.Host {
            get {return _configHost;}
        }

        IInternalConfigRoot IConfigSystem.Root {
            get {return _configRoot;}
        }
    }
}
