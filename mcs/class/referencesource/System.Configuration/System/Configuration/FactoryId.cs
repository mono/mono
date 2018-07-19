//------------------------------------------------------------------------------
// <copyright file="FactoryId.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    // Identifies a factory
    [System.Diagnostics.DebuggerDisplay("FactoryId {ConfigKey}")]
    internal class FactoryId {
        private string  _configKey;
        private string  _group;
        private string  _name;

        internal FactoryId(string configKey, string group, string name) {
            _configKey = configKey;
            _group = group;
            _name = name;
        }

        internal string ConfigKey {
            get {return _configKey;}
        }

        internal string Group {
            get {return _group;}
        }

        internal string Name {
            get {return _name;}
        }
    }
}
