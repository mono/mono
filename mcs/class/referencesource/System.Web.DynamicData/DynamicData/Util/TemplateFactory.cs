using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Resources;

namespace System.Web.DynamicData {
    internal class TemplateFactory {
        // Use Hashtable instead of Dictionary<,> because it is more thread safe
        private Hashtable _fieldTemplateVirtualPathCache = new Hashtable();

        private string _defaultLocation;
        private string _templateFolderVirtualPath;

        internal MetaModel Model { get; set; }

        private bool _needToResolveVirtualPath;
        private bool _trackFolderChanges;
        private bool _registeredForChangeNotifications;

        private VirtualPathProvider _vpp;
        private bool _usingCustomVpp;

        internal TemplateFactory(string defaultLocation)
            : this(defaultLocation, true) {
        }

        internal TemplateFactory(string defaultLocation, bool trackFolderChanges) {
            Debug.Assert(!String.IsNullOrEmpty(defaultLocation));
            _defaultLocation = defaultLocation;
            _trackFolderChanges = trackFolderChanges;
        }

        internal string TemplateFolderVirtualPath {
            get {
                if (_templateFolderVirtualPath == null) {
                    // If not set, set its default location
                    TemplateFolderVirtualPath = _defaultLocation;
                }

                if (_needToResolveVirtualPath) {
                    // Make sure it ends with a slash
                    _templateFolderVirtualPath = VirtualPathUtility.AppendTrailingSlash(_templateFolderVirtualPath);

                    // If it's relative, make it relative to the Model's path
                    // Note can be null under Unit Testing
                    if (Model != null) {
                        _templateFolderVirtualPath = VirtualPathUtility.Combine(Model.DynamicDataFolderVirtualPath, _templateFolderVirtualPath);
                    }

                    _needToResolveVirtualPath = false;
                }

                return _templateFolderVirtualPath;
            }
            set {
                _templateFolderVirtualPath = value;

                // Make sure we register for change notifications, since we just got a new path
                _registeredForChangeNotifications = false;

                // It may be relative and need resolution, but let's not do it until we need it
                _needToResolveVirtualPath = true;
            }
        }

        internal VirtualPathProvider VirtualPathProvider {
            get {
                if (_vpp == null) {
                    _vpp = HostingEnvironment.VirtualPathProvider;
                }
                return _vpp;
            }
            set {
                _vpp = value;
                _usingCustomVpp = value != null;
            }
        }

        internal string GetTemplatePath(long cacheKey, Func<string> templatePathFactoryFunction) {
            // Check if we already have it cached
            string virtualPath = this[cacheKey];

            // null is a valid value, so we also need to check whether the key exists
            if (virtualPath == null && !ContainsKey(cacheKey)) {
                // It's not cached, so compute it and cache it.  Make sure multiple writers are serialized
                virtualPath = templatePathFactoryFunction();
                this[cacheKey] = virtualPath;
            }

            return virtualPath;
        }

        private string this[long cacheKey] {
            get {
                EnsureRegisteredForChangeNotifications();
                return (string)_fieldTemplateVirtualPathCache[cacheKey];
            }
            set {
                EnsureRegisteredForChangeNotifications();
                lock (_fieldTemplateVirtualPathCache) {
                    _fieldTemplateVirtualPathCache[cacheKey] = value;
                }
            }
        }

        private bool ContainsKey(long cacheKey) {
            EnsureRegisteredForChangeNotifications();
            return _fieldTemplateVirtualPathCache.ContainsKey(cacheKey);
        }

        private void EnsureRegisteredForChangeNotifications() {
            if (!_trackFolderChanges) {
                return;
            }

            if (!_registeredForChangeNotifications) {
                lock (this) {
                    if (!_registeredForChangeNotifications) {
                        // Make sure the folder exists
                        if (!VirtualPathProvider.DirectoryExists(TemplateFolderVirtualPath)) {
                            throw new InvalidOperationException(String.Format(
                                CultureInfo.CurrentCulture,
                                DynamicDataResources.FieldTemplateFactory_FolderNotFound,
                                TemplateFolderVirtualPath));
                        }

                        // Register for notifications if anything in that folder changes
                        FileChangeNotifier.Register(TemplateFolderVirtualPath, delegate(string path) {
                            // Something has changed, so clear our cache
                            lock (_fieldTemplateVirtualPathCache) {
                                _fieldTemplateVirtualPathCache.Clear();
                            }
                        });

                        _registeredForChangeNotifications = true;
                    }
                }
            }
        }

        internal bool FileExists(string virtualPath) {
            if (_usingCustomVpp) {
                // for unit testing
                return VirtualPathProvider.FileExists(virtualPath);
            } else {
                // Use GetObjectFactory instead of GetCompiledType because it will not throw, which improves the debugging experience
                return BuildManager.GetObjectFactory(virtualPath, /* throwIfNotFound */ false) != null;
            }
        }
    }
}
