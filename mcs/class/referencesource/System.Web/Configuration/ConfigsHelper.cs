//------------------------------------------------------------------------------
// <copyright file="ConfigsHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration 
{
    using System.Configuration;
    using System.Configuration.Provider;

    internal class ConfigsHelper {
        internal static void GetRegistryStringAttribute(ref string val, ConfigurationElement config, string propName) {
            if (HandlerBase.CheckAndReadRegistryValue(ref val, false) == false) {
                throw new ConfigurationErrorsException(
                        SR.GetString(SR.Invalid_registry_config),
                        config.ElementInformation.Properties[propName].Source, config.ElementInformation.Properties[propName].LineNumber);
            }
        }
    }
}
