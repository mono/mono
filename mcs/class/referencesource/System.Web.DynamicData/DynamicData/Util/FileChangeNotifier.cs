using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.Resources;
using System.Globalization;
using System.Web.Caching;
using System.Web.Hosting;

namespace System.Web.DynamicData {
    delegate void FileChangedCallback(string path);

    class FileChangeNotifier {
        private static VirtualPathProvider _vpp;

        internal static VirtualPathProvider VirtualPathProvider {
            private get {
                if (_vpp == null) {
                    _vpp = HostingEnvironment.VirtualPathProvider;
                }
                return _vpp;
            }
            // For unit test purpose
            set {
                _vpp = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId = "System.Web.DynamicData.FileChangeNotifier",
            Justification="The object deals with file change notifications and we don't need to hold on to it")]
        internal static void Register(string virtualPath, FileChangedCallback onFileChanged) {
            new FileChangeNotifier(virtualPath, onFileChanged);
        }

        private FileChangedCallback _onFileChanged;

        private FileChangeNotifier(string virtualPath, FileChangedCallback onFileChanged) {
            _onFileChanged = onFileChanged;
            RegisterForNextNotification(virtualPath);
        }

        private void RegisterForNextNotification(string virtualPath) {
            // Get a CacheDependency from the BuildProvider, so that we know anytime something changes
            var virtualPathDependencies = new List<string>();
            virtualPathDependencies.Add(virtualPath);
            CacheDependency cacheDependency = VirtualPathProvider.GetCacheDependency(
                virtualPath, virtualPathDependencies, DateTime.UtcNow);

            // Rely on the ASP.NET cache for file change notifications, since FileSystemWatcher
            // doesn't work in medium trust
            HttpRuntime.Cache.Insert(virtualPath /*key*/, virtualPath /*value*/, cacheDependency,
                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnCacheItemRemoved));
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason) {

            // We only care about dependency changes
            if (reason != CacheItemRemovedReason.DependencyChanged)
                return;

            _onFileChanged(key);

            // We need to register again to get the next notification
            RegisterForNextNotification(key);
        }
    }
}

