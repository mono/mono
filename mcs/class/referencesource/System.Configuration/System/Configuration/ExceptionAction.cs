//------------------------------------------------------------------------------
// <copyright file="ExceptionAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Net;

    // ExceptionAction
    //
    // Value to change how we handle the Exception
    //
    internal enum ExceptionAction {
        NonSpecific,    // Not specific to a particular section, nor a global schema error
        Local,          // Error specific to a particular section
        Global,         // Error in the global (file) schema 
    }
}
