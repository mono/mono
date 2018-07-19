//------------------------------------------------------------------------------
// <copyright file="InvalidPropValue.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;

namespace System.Configuration {

    // Class to support caching of property values as string
    internal sealed class InvalidPropValue {
        private string _value;
        private ConfigurationException _error;

        internal InvalidPropValue(string value, ConfigurationException error) {
            _value = value;
            _error = error;
        }

        internal ConfigurationException Error {
            get {
                return _error;
            }
        }
        internal string Value {
            get {
                return _value;
            }
        }
    }
}
