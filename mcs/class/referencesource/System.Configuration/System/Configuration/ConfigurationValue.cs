//------------------------------------------------------------------------------
// <copyright file="ConfigurationValue.cs" company="Microsoft">
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

    internal class ConfigurationValue {

        internal ConfigurationValueFlags ValueFlags;
        internal object Value;
        internal PropertySourceInfo SourceInfo;

        internal ConfigurationValue(object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo) {
            Value = value;
            ValueFlags = valueFlags;
            SourceInfo = sourceInfo;
        }
    }
}
