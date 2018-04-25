//------------------------------------------------------------------------------
// <copyright file="WebConfigurationHostFileChange.cs" company="Microsoft">
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

    //
    // Receives file change notifications from the FileChangesMonitor, 
    // and forwards them to a callback on a configuration host.
    //
    sealed class WebConfigurationHostFileChange {
        StreamChangeCallback    _callback;

        internal WebConfigurationHostFileChange(StreamChangeCallback callback) {
            _callback = callback;
        }

        internal void OnFileChanged(object sender, FileChangeEvent e) {
            _callback(e.FileName);
        }

        internal StreamChangeCallback Callback {
            get {return _callback;}
        }
    }
}
