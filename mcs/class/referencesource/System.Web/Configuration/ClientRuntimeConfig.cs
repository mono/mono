//------------------------------------------------------------------------------
// <copyright file="ClientRuntimeConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Configuration;
using System.Security;
using System.Security.Permissions;

namespace System.Web.Configuration {

    //
    // If we are not using the HttpConfigurationSystem, delegate to the 
    // client configuration system.
    //
    internal class ClientRuntimeConfig : RuntimeConfig {

        internal ClientRuntimeConfig() : base(null, false) {}

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        protected override object GetSectionObject(string sectionName) {
            return ConfigurationManager.GetSection(sectionName);
        }
    }
}
