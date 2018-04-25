//------------------------------------------------------------------------------
// <copyright file="BrowserCapsElementType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Globalization;

    internal enum BrowserCapsElementType {
        Capabilities,
        Capture,
        ControlAdapters,
        Identification,
        SampleHeaders
    }
}
