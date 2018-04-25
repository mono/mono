//------------------------------------------------------------------------------
// <copyright file="ConfigurationLocation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;

    public class ConfigurationLocation {
        Configuration   _config;
        string          _locationSubPath;

        internal ConfigurationLocation(Configuration config, string locationSubPath) {
            _config = config;
            _locationSubPath = locationSubPath;
        }

        public string Path {
            get {return _locationSubPath;}
        }

        public Configuration OpenConfiguration() {
            return _config.OpenLocationConfiguration(_locationSubPath);
        }
    }
}
