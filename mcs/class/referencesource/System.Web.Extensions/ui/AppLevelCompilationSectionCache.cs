//------------------------------------------------------------------------------
// <copyright file="AppLevelCompilationSectionCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Configuration;

    // The compilation section can be defined below the application level, but ScriptManager only considers the
    // application-level debug setting.
    internal sealed class AppLevelCompilationSectionCache : ICompilationSection {
        private static readonly AppLevelCompilationSectionCache _instance = new AppLevelCompilationSectionCache();

        // Value is cached statically, because AppLevelCompilationSectionCache is a Singleton.
        private bool? _debug;

        private AppLevelCompilationSectionCache() {
        }

        public static AppLevelCompilationSectionCache Instance {
            get {
                return _instance;
            }
        }

        public bool Debug {
            get {
                if (_debug == null) {
                    _debug = GetDebugFromConfig();
                }
                return _debug.Value;
            }
        }

        [
        ConfigurationPermission(SecurityAction.Assert, Unrestricted = true),
        SecuritySafeCritical(),
        ]
        private static bool GetDebugFromConfig() {
            CompilationSection section =
                (CompilationSection)WebConfigurationManager.GetWebApplicationSection("system.web/compilation");
            return section.Debug;
        }
    }
}
