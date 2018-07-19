//------------------------------------------------------------------------------
// <copyright file="InternalConfigConfigurationFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using ClassConfiguration = System.Configuration.Configuration;
using System.Collections;
using System.Configuration;
using System.Configuration.Internal;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Configuration.Internal {

    //
    // Class used to create and initialize an instance of the Configuration class
    // from assemblies other than System.
    //
    internal sealed class InternalConfigConfigurationFactory : IInternalConfigConfigurationFactory {

        private InternalConfigConfigurationFactory() {}

        ClassConfiguration IInternalConfigConfigurationFactory.Create(Type typeConfigHost, params object[] hostInitConfigurationParams) {
            return new ClassConfiguration(null, typeConfigHost, hostInitConfigurationParams);
        }
        
        // Normalize a locationSubpath argument
        string IInternalConfigConfigurationFactory.NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo) {
            return BaseConfigurationRecord.NormalizeLocationSubPath(subPath, errorInfo);
        }
    }
}
