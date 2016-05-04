//------------------------------------------------------------------------------
// <copyright file="ConfigurationValueFlags.cs" company="Microsoft">
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

    [Flags]
    internal enum ConfigurationValueFlags {
        Default = 0,
        Inherited = 1,
        Modified = 2,
        Locked = 4,
        XMLParentInherited = 8,
    }
}
