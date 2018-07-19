//------------------------------------------------------------------------------
// <copyright file="WebLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration.Internal;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration.Internal;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Xml;

    // If a path is null, we need this to tell the difference between machine.config
    // or root web.config.
    enum WebLevel {
        Machine = 1,
        Path = 2
    }
}
