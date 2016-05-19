//------------------------------------------------------------------------------
// <copyright file="Update.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    //
    // Represents an update to a configuration section, either in its
    // declaration or its definition.
    //
    abstract internal class Update {
        private bool    _moved;
        private bool    _retrieved;
        private string  _configKey;
        private string  _updatedXml;

        internal Update(string configKey, bool moved, string updatedXml) {
            _configKey = configKey;
            _moved = moved;
            _updatedXml = updatedXml;
        }

        internal string ConfigKey {
            get {return _configKey;}
        }

        internal bool Moved {
            get {return _moved;}
        }

        internal string UpdatedXml {
            get {return _updatedXml;}
        }

        internal bool Retrieved {
            get {return _retrieved;}
            set {_retrieved = value;}
        }
    }
}
