namespace System.Web.UI {
    using System;
    using System.Reflection;

    public class ScriptResourceDefinition : IScriptResourceDefinition {
        private string _path;
        private string _debugPath;
        private string _resourceName;
        private Assembly _resourceAssembly;
        private string _cdnPath;
        private string _cdnDebugPath;
        private string _cdnPathSecureConnection;
        private string _cdnDebugPathSecureConnection;
        private bool _cdnSupportsSecureConnection;
        private string _loadSuccessExpression;

        public string CdnDebugPath {
            get {
                return _cdnDebugPath ?? String.Empty;
            }
            set {
                _cdnDebugPath = value;
            }
        }

        public string CdnPath {
            get {
                return _cdnPath ?? String.Empty;
            }
            set {
                _cdnPath = value;
            }
        }

        internal string CdnDebugPathSecureConnection {
            get {
                if (_cdnDebugPathSecureConnection == null) {
                    _cdnDebugPathSecureConnection = GetSecureCdnPath(CdnDebugPath);
                }
                return _cdnDebugPathSecureConnection;
            }
        }

        internal string CdnPathSecureConnection {
            get {
                if (_cdnPathSecureConnection == null) {
                    _cdnPathSecureConnection = GetSecureCdnPath(CdnPath);
                }
                return _cdnPathSecureConnection;
            }
        }

        public bool CdnSupportsSecureConnection {
            get {
                return _cdnSupportsSecureConnection;
            }
            set {
                _cdnSupportsSecureConnection = value;
            }
        }

        public string LoadSuccessExpression {
            get {
                return _loadSuccessExpression ?? String.Empty;
            }
            set {
                _loadSuccessExpression = value;
            }
        }

        public string DebugPath {
            get {
                return _debugPath ?? String.Empty;
            }
            set {
                _debugPath = value;
            }
        }

        public string Path {
            get {
                return _path ?? String.Empty;
            }
            set {
                _path = value;
            }
        }

        public Assembly ResourceAssembly {
            get {
                return _resourceAssembly;
            }
            set {
                _resourceAssembly = value;
            }
        }

        public string ResourceName {
            get {
                return _resourceName ?? String.Empty;
            }
            set {
                _resourceName = value;
            }
        }

        private string GetSecureCdnPath(string unsecurePath) {
            string cdnPath = String.Empty;
            if (!String.IsNullOrEmpty(unsecurePath)) {
                if (_cdnSupportsSecureConnection) {
                    // convert 'http' to 'https'
                    if (unsecurePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) {
                        cdnPath = "https" + unsecurePath.Substring(4);
                    }
                    else {
                        // cdnPath is not 'http' so we cannot determine the secure path
                        cdnPath = String.Empty;
                    }
                }
                else {
                    cdnPath = String.Empty;
                }
            }
            return cdnPath;
        }

        string IScriptResourceDefinition.CdnPathSecureConnection {
            get {
                return CdnPathSecureConnection;
            }
        }

        string IScriptResourceDefinition.CdnDebugPathSecureConnection {
            get {
                return CdnDebugPathSecureConnection;
            }
        }
    }
}
