//------------------------------------------------------------------------------
// <copyright file="MapPathCacheInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;
    using System.Web.Hosting;
    using System.Web.Caching;
    using Microsoft.Win32;

    internal class MapPathCacheInfo
    {
        internal string     MapPathResult;
        internal bool       Evaluated;
        internal Exception  CachedException;
    }
}

