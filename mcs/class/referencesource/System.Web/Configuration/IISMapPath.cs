//------------------------------------------------------------------------------
// <copyright file="IISMapPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Xml;
    using System.Text;
    using System.Web.Util;
    using System.Web.UI;
    using System.IO;
    using System.Web.Hosting;

    //
    // Abstracts the difference between Metabase and SitesSection IConfigMapPath.
    //
    static internal class IISMapPath {
        static internal IConfigMapPath GetInstance() {
            // IIS 7 bits on <= IIS 6.x: use the metabase
            if (ServerConfig.UseMetabase) {
                return (IConfigMapPath) MetabaseServerConfig.GetInstance();
            }

            if (ServerConfig.IISExpressVersion != null) {
                return (IConfigMapPath) ServerConfig.GetInstance();
            }
            
            ProcessHost host = ProcessHost.DefaultHost;
            IProcessHostSupportFunctions functions = null;
            
            if (null != host) {
                functions = host.SupportFunctions;                        
            }
            
            if (functions == null) {
                functions = HostingEnvironment.SupportFunctions;
            }
            
            return new ProcessHostMapPath(functions);
        }

        // A site name might be an id if it is a number.
        static internal bool IsSiteId(string siteName) {
            if (string.IsNullOrEmpty(siteName))
                return false;

            for (int i = 0; i < siteName.Length; i++) {
                if (!Char.IsDigit(siteName[i])) {
                    return false;
                }
            }

            return true;
        }
    }
}
