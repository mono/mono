//------------------------------------------------------------------------------
// <copyright file="ScriptResourceInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Handlers;
    using System.Web.Resources;
    using System.Web.Util;

    internal class ScriptResourceInfo {
        private string _contentType;
        private bool _performSubstitution;
        private string _scriptName;
        private string _scriptResourceName;
        private string _typeName;
        private bool _isDebug;
        private string _cdnPath;
        private string _cdnPathSecureConnection;
        private readonly string _loadSuccessExpression;

        private static readonly IDictionary _scriptCache = Hashtable.Synchronized(new Hashtable());
        private static readonly IDictionary _duplicateScriptAttributesChecked = Hashtable.Synchronized(new Hashtable());

        public static readonly ScriptResourceInfo Empty = new ScriptResourceInfo();

        private ScriptResourceInfo() {
        }

        public ScriptResourceInfo(WebResourceAttribute wra, ScriptResourceAttribute sra, Assembly assembly)
            : this() {
            _scriptName = wra.WebResource;
            _cdnPath = wra.CdnPath;
            _contentType = wra.ContentType;
            _performSubstitution = wra.PerformSubstitution;
            _loadSuccessExpression = wra.LoadSuccessExpression;
            _isDebug = !String.IsNullOrEmpty(_scriptName) && _scriptName.EndsWith(".debug.js", StringComparison.OrdinalIgnoreCase);
            if (sra != null) {
                _scriptResourceName = sra.StringResourceName;
                _typeName = sra.StringResourceClientTypeName;
            }
            if (!String.IsNullOrEmpty(_cdnPath)) {
                _cdnPath = AssemblyResourceLoader.FormatCdnUrl(assembly, _cdnPath);
                _cdnPathSecureConnection = AssemblyResourceLoader.FormatCdnUrl(assembly, wra.CdnPathSecureConnection);
            }
        }

        public string CdnPath {
            get {
                return _cdnPath;
            }
        }

        public string CdnPathSecureConnection {
            get {
                return _cdnPathSecureConnection;
            }
        }

        public string LoadSuccessExpression {
            get {
                return _loadSuccessExpression;
            }
        }

        public string ContentType {
            get { return _contentType; }
        }

        public bool IsDebug {
            get { return _isDebug; }
        }

        public bool PerformSubstitution {
            get { return _performSubstitution; }
        }

        public string ScriptName {
            get { return _scriptName; }
        }

        public string ScriptResourceName {
            get { return _scriptResourceName; }
        }

        public string TypeName {
            get { return _typeName; }
        }

        public static ScriptResourceInfo GetInstance(Assembly assembly, string resourceName) {
            // The first time this API is called, check for attributes that point to the same script
            if (!_duplicateScriptAttributesChecked.Contains(assembly)) {
                Dictionary<string, bool> scripts = new Dictionary<string, bool>();
                foreach (ScriptResourceAttribute attr
                    in assembly.GetCustomAttributes(typeof(ScriptResourceAttribute), false)) {

                    string scriptName = attr.ScriptName;
                    if (scripts.ContainsKey(scriptName)) {
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.CurrentCulture,
                                  AtlasWeb.ScriptResourceHandler_DuplicateScriptResources,
                                  scriptName, assembly.GetName()));
                    }
                    scripts.Add(scriptName, true);
                }

                _duplicateScriptAttributesChecked[assembly] = true;
            }
            Tuple<Assembly, string> cacheKey = new Tuple<Assembly, string>(assembly, resourceName);
            ScriptResourceInfo resourceInfo = (ScriptResourceInfo)_scriptCache[cacheKey];
            if (resourceInfo == null) {
                WebResourceAttribute webResourceAttribute = null;
                ScriptResourceAttribute scriptResourceAttribute = null;
                // look for a WebResourceAttribute with that name
                object[] attrs = assembly.GetCustomAttributes(typeof(WebResourceAttribute), false);
                foreach (WebResourceAttribute wra in attrs) {
                    if (String.Equals(wra.WebResource, resourceName, StringComparison.Ordinal)) {
                        webResourceAttribute = wra;
                        break;
                    }
                }
                if (webResourceAttribute != null) {
                    // look for a script resource attribute with that name
                    attrs = assembly.GetCustomAttributes(typeof(ScriptResourceAttribute), false);
                    foreach (ScriptResourceAttribute sra in attrs) {
                        if (String.Equals(sra.ScriptName, resourceName, StringComparison.Ordinal)) {
                            scriptResourceAttribute = sra;
                            break;
                        }
                    }
                    resourceInfo = new ScriptResourceInfo(webResourceAttribute, scriptResourceAttribute, assembly);
                }
                else {
                    resourceInfo = ScriptResourceInfo.Empty;
                }
                // Cache the results so we don't have to do this again
                _scriptCache[cacheKey] = resourceInfo;
            }
            return resourceInfo;
        }
    }
}
