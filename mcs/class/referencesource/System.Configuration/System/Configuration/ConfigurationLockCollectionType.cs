//------------------------------------------------------------------------------
// <copyright file="ConfigurationLockCollectionType.cs" company="Microsoft">
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

    internal enum ConfigurationLockCollectionType {
        LockedAttributes = 1,
        LockedExceptionList = 2,
        LockedElements = 3,
        LockedElementsExceptionList = 4
    }
}
