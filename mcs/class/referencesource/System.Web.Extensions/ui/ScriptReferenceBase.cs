//------------------------------------------------------------------------------
// <copyright file="ScriptReferenceBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI.WebControls;
    using Debug = System.Diagnostics.Debug;

    [
    DefaultProperty("Path"),
    ]
    public abstract class ScriptReferenceBase {
        private bool _alwaysLoadBeforeUI;
        private IClientUrlResolver _clientUrlResolver;
        private Control _containingControl;
        private bool _isStaticReference;
        private bool _notifyScriptLoaded = true;
        private string _path;
        private string[] _resourceUICultures;
        private ScriptMode _scriptMode;

        protected ScriptReferenceBase() {}

        internal bool AlwaysLoadBeforeUI {
            get {
                return _alwaysLoadBeforeUI;
            }
            set {
                _alwaysLoadBeforeUI = value;
            }
        }

        // Used by ScriptManager to associate a ScriptReference with its url resolver
        // (i.e. ScriptManager or ScriptManagerProxy)
        internal IClientUrlResolver ClientUrlResolver {
            get {
                return _clientUrlResolver;
            }
            set {
                _clientUrlResolver = value;
            }
        }

        internal Control ContainingControl {
            get {
                return _containingControl;
            }
            set {
                _containingControl = value;
            }
        }

        // isStaticReference is true if the reference came from a ScriptManager or ScriptManagerProxy scripts collection,
        // false if it came from an IScriptControl or ExtenderControl.
        internal bool IsStaticReference {
            get {
                return _isStaticReference;
            }
            set {
                _isStaticReference = value;
            }
        }

        // True if this is a reference to a bundle
        internal bool IsBundleReference {
            get;
            set;
        }

        [
        Category("Behavior"),
        DefaultValue(true),
        NotifyParentProperty(true),
        ResourceDescription("ScriptReference_NotifyScriptLoaded"),
        Obsolete("NotifyScriptLoaded is no longer required in script references.")
        ]
        public bool NotifyScriptLoaded {
            get {
                return _notifyScriptLoaded;
            }
            set {
                _notifyScriptLoaded = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        NotifyParentProperty(true),
        ResourceDescription("ScriptReference_Path"),
        UrlProperty("*.js")
        ]
        public string Path {
            get {
                return (_path == null) ? String.Empty : _path;
            }
            set {
                _path = value;
            }
        }

        [
        ResourceDescription("ScriptReference_ResourceUICultures"),
        DefaultValue(null),
        Category("Behavior"),
        MergableProperty(false),
        NotifyParentProperty(true),
        TypeConverter(typeof(StringArrayConverter)),
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "String[] has existing TypeConverter support, which we don't want to add for List<string> to the assembly just for this property and at this point in the product cycle.")
        ]
        public string[] ResourceUICultures {
            get {
                return _resourceUICultures;
            }
            set {
                _resourceUICultures = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(ScriptMode.Auto),
        NotifyParentProperty(true),
        ResourceDescription("ScriptReference_ScriptMode")
        ]
        public ScriptMode ScriptMode {
            get {
                return _scriptMode;
            }
            set {
                if (value < ScriptMode.Auto || value > ScriptMode.Release) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _scriptMode = value;
            }
        }

        [Obsolete("Use IsAjaxFrameworkScript(ScriptManager)")]
        protected internal abstract bool IsFromSystemWebExtensions();

        protected internal virtual bool IsAjaxFrameworkScript(ScriptManager scriptManager) {
            return false;
        }

        internal virtual bool IsDefiningSys {
            get;
            set;
        }

        // Script path may contain a query string, while script name may not
        // Release: foo.js?key=value
        // Debug:   foo.debug.js?key=value
        internal static string GetDebugPath(string releasePath) {
            // Per RFC 3986, the '?' delimits the query, regardless of the protocol.  This overrides
            // an earlier RFC, which stated that FTP protocol allows '?' as path characters.
            string pathWithoutQuery;
            string query;
            if (releasePath.IndexOf('?') >= 0) {
                int indexOfQuery = releasePath.IndexOf('?');
                pathWithoutQuery = releasePath.Substring(0, indexOfQuery);
                query = releasePath.Substring(indexOfQuery);
            }
            else {
                pathWithoutQuery = releasePath;
                query = null;
            }

            if (!pathWithoutQuery.EndsWith(".js", StringComparison.Ordinal)) {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentUICulture, AtlasWeb.ScriptReference_InvalidReleaseScriptPath, pathWithoutQuery));
            }

            return ReplaceExtension(pathWithoutQuery) + query;
        }

        [SuppressMessage("Microsoft.Design", "CA1055", Justification = "Consistent with other URL properties in ASP.NET.")]
        protected internal abstract string GetUrl(ScriptManager scriptManager, bool zip);

        // Assumes the input ends with ".js".  Replaces the ".js" at the end of the input
        // with ".debug.js".
        protected static string ReplaceExtension(string pathOrName) {
            Debug.Assert(pathOrName.EndsWith(".js", StringComparison.Ordinal));
            return (pathOrName.Substring(0, pathOrName.Length - 2) + "debug.js");
        }
    }
}
