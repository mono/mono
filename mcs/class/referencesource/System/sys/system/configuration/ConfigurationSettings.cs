//------------------------------------------------------------------------------
// <copyright file="ConfigurationSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    using System.Collections.Specialized;

    public sealed class ConfigurationSettings {
        private ConfigurationSettings() {}

        [Obsolete("This method is obsolete, it has been replaced by System.Configuration!System." +
                  "Configuration.ConfigurationManager.AppSettings")]
        public static NameValueCollection AppSettings {
            get {
                return ConfigurationManager.AppSettings;
            }
        }

        [Obsolete("This method is obsolete, it has been replaced by System.Configuration!System." +
                  "Configuration.ConfigurationManager.GetSection")]
        public static object GetConfig(string sectionName) {
            return ConfigurationManager.GetSection( sectionName );
        }
    }
}
