/* 
 * Security review: We're calling into potentially untrusted code, as we don't check the identity of the target. But since we're neither passing sensitive information nor treating the return values as trusted, this is fine.    
 */

namespace System.Web.UI {
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Web.Compilation;


    internal sealed class BundleReflectionHelper {
        // Helper class for ScriptManager to call into Bundling
        // Expectation is that this Bundling will expose an object at System.Web.Optimization.BundleResolver.Current
        // and this type will have the following public methods:
        //    bool IsBundleVirtualPath(string virtualPath);
        private delegate bool IsBundleVirtualPathDelegate(string virtualPath);
        private IsBundleVirtualPathDelegate IsBundleVirtualPathMethod { get; set; }

        //    IEnumerable<string> GetBundleContents(string virtualPath);
        private delegate IEnumerable<string> GetBundleContentsDelegate(string virtualPath);
        private GetBundleContentsDelegate GetBundleContentsMethod { get; set; }

        //    string GetBundleUrl(string virtualPath);
        private delegate string GetBundleUrlDelegate(string virtualPath);
        private GetBundleUrlDelegate GetBundleUrlMethod { get; set; }

        //    static object System.Web.Optimization.BundleResolver.Current
        private delegate object BundleResolverCurrentDelegate();
        private static BundleResolverCurrentDelegate BundleResolverCurrentMethod { get; set; }


        // Normal runtime code path, try to get the resolver from System.Web.Optimization.BundleResolver.Current and bind to its methods
        public BundleReflectionHelper() {
            BundleResolver = CallBundleResolverCurrent();
        }

        // Unit tests can pass in their own bundleResolver
        public BundleReflectionHelper(object bundleResolver) {
            BundleResolver = bundleResolver;
        }

        // ScriptManager will assume bundling is not enabled if this property is null.
        // Expectation is that this object type will have the following methods.  
        //    bool IsBundleVirtualPath(string virtualPath);
        //    IEnumerable<string> GetBundleContents(string virtualPath);
        //    string GetBundleUrl(string virtualPath);
        // Unit tests will set this directly, otherwise 
        private object _resolver;
        internal object BundleResolver {
            get {
                return _resolver;
            }
            set {
                if (value != null) {
                    Type resolverType = value.GetType();
                    Type[] args = new Type[] { typeof(string) };
                    IsBundleVirtualPathMethod = MakeDelegate<IsBundleVirtualPathDelegate>(value, resolverType.GetMethod("IsBundleVirtualPath", args));
                    GetBundleContentsMethod = MakeDelegate<GetBundleContentsDelegate>(value, resolverType.GetMethod("GetBundleContents", args));
                    GetBundleUrlMethod = MakeDelegate<GetBundleUrlDelegate>(value, resolverType.GetMethod("GetBundleUrl", args));
                    // Only allow the set if all 3 methods are found
                    if (IsBundleVirtualPathMethod != null && GetBundleContentsMethod != null && GetBundleUrlMethod != null) {
                        _resolver = value;
                    }
                }
                else {
                    _resolver = null;
                }
            }
        }

        public bool IsBundleVirtualPath(string virtualPath) {
            if (BundleResolver != null) {
                try {
                    return IsBundleVirtualPathMethod(virtualPath);
                }
                catch {
                    // We never ever want to ---- up in an exception from this
                }
            }

            return false;
        }

        public IEnumerable<string> GetBundleContents(string virtualPath) {
            if (BundleResolver != null) {
                try {
                    return GetBundleContentsMethod(virtualPath);
                }
                catch {
                    // We never ever want to ---- up in an exception from this
                }
            }

            return null;
        }

        public string GetBundleUrl(string virtualPath) {
            if (BundleResolver != null) {
                try {
                    return GetBundleUrlMethod(virtualPath);
                }
                catch {
                    // We never ever want to ---- up in an exception from this
                }
            }

            return virtualPath;
        }

        // Attempts to call a static property System.Web.Optimziation.BundleResolver.Current
        // Only looks for the property once, but will call into the property every time
        private static bool s_lookedForCurrentProperty;
        internal static object CallBundleResolverCurrent() {
            if (!Volatile.Read(ref s_lookedForCurrentProperty)) {
                try {
                    Type bundleResolverType = BuildManager.GetType("System.Web.Optimization.BundleResolver", throwOnError: false);
                    if (bundleResolverType != null) {
                        PropertyInfo bundleResolverCurrentProperty = bundleResolverType.GetProperty("Current", BindingFlags.Static | BindingFlags.Public);
                        if (bundleResolverCurrentProperty != null) {
                            BundleResolverCurrentMethod = MakeDelegate<BundleResolverCurrentDelegate>(null, bundleResolverCurrentProperty.GetGetMethod());
                        }
                    }
                }
                catch {
                    // We never want to throw an exception if this fails, we just want to treat this as bundling is off
                }
                Volatile.Write(ref s_lookedForCurrentProperty, true);
            }

            if (BundleResolverCurrentMethod == null) {
                return null;
            }

            return BundleResolverCurrentMethod();
        }

        private static T MakeDelegate<T>(object target, MethodInfo method) where T : class {
            return Delegate.CreateDelegate(typeof(T), target, method, false) as T;
        }

    }

}
