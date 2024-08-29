using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Globalization;
using System.Resources;
using System.Security.Permissions;
using System.Reflection;
using System.Web.Caching;
using System.Threading;

 namespace System.Web.Globalization {
    public sealed class ResourceFileStringLocalizerProvider : IStringLocalizerProvider {
        private readonly ConcurrentDictionary<string, object> _missingManifestCache = new ConcurrentDictionary<string, object>();
        private ResourceManager _resourceManager;
        private bool _loadedResourceAssembly = false;
        public const string ResourceFileName = "DataAnnotation.Localization";

         public string GetLocalizedString(CultureInfo culture, string name, params object[] arguments) {  
            if (culture == null) {
                throw new ArgumentNullException("culture");
            }
            if (string.IsNullOrEmpty(name)) {
                return name;
            }

             // If we can't get localized string from the resource manager,
            // we will return null. Any place where GetLocalizedString is called,
            // we should have fallback logic(use ErrorMessage from DataAnnotation attributes)
            // to handle this case.
            var format = GetStringSafely(name, culture);
            if (format != null) {
                return string.Format(format, arguments);
            }
            else {
                return format;
            }
        }

         private string GetStringSafely(string name, CultureInfo culture) {
            if (culture == null) {
                throw new ArgumentNullException("culture");
            }

             EnsureResourceManager();
            string localizedString = null;

             if (_resourceManager == null) {
                return localizedString;
            }           

             var cacheKey = string.Format("n={0}&c={1}", name, culture.Name);

             if (_missingManifestCache.ContainsKey(cacheKey)) {
                return localizedString;
            }

             try {
                localizedString = (string)_resourceManager.GetObject(name, culture);
            }
            catch (Exception) {
                _missingManifestCache.TryAdd(cacheKey, null);
            }

             return localizedString;
        }

         private ResourceManager EnsureResourceManager() {
            if (_loadedResourceAssembly) {
                return _resourceManager;
            }
            else {
                var resourceAssembly = GetLocalResourceAssembly();

                 if (resourceAssembly != null) {
                    _resourceManager = new ResourceManager(ResourceFileName, resourceAssembly);
                    _resourceManager.IgnoreCase = true;
                }
                _loadedResourceAssembly = true;
                return _resourceManager;
            }
        }

         [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private Assembly GetLocalResourceAssembly() {

             var appRootVpath = VirtualPath.Create(HttpRuntime.AppDomainAppVirtualPath);
            var cacheKey = BuildManager.GetLocalResourcesAssemblyName(appRootVpath);

             BuildResult result = BuildManager.GetBuildResultFromCache(cacheKey);

             if (result != null) {
                return ((BuildResultCompiledAssembly)result).ResultAssembly;
            }

             return null;
        }
    }
}