namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Security;

    internal static class SecurityUtil {

        private static Action<Action> _callInAppTrustThunk;

        // !! IMPORTANT !!
        // Do not try to optimize this method or perform any extra caching; doing so could lead to MVC not operating
        // correctly until the AppDomain is restarted.
        [SuppressMessage("Microsoft.Security", "CA2107:ReviewDenyAndPermitOnlyUsage",
            Justification = "This is essentially the same logic as Page.ProcessRequest.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "If an exception is thrown, assume we're running in same trust level as the application itself, so we don't need to do anything special.")]
        private static Action<Action> GetCallInAppTrustThunk() {
            // do we need to create the thunk?
            if (_callInAppTrustThunk == null) {
                try {
                    if (!typeof(SecurityUtil).Assembly.IsFullyTrusted /* bin-deployed */
                        || AppDomain.CurrentDomain.IsHomogenous /* .NET 4 CAS model */) {
                        // we're already running in the application's trust level, so nothing to do
                        _callInAppTrustThunk = f => f();
                    }
                    else {
                        // legacy CAS model - need to lower own permission level to be compatible with legacy systems
                        // This is essentially the same logic as Page.ProcessRequest(HttpContext)
                        NamedPermissionSet namedPermissionSet = (NamedPermissionSet)typeof(HttpRuntime).GetProperty("NamedPermissionSet", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                        bool disableProcessRequestInApplicationTrust = (bool)typeof(HttpRuntime).GetProperty("DisableProcessRequestInApplicationTrust", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                        if (namedPermissionSet != null && !disableProcessRequestInApplicationTrust) {
                            _callInAppTrustThunk = f => {
                                // lower permissions
                                namedPermissionSet.PermitOnly();
                                f();
                            };
                        }
                        else {
                            // application's trust level is FullTrust, so nothing to do
                            _callInAppTrustThunk = f => f();
                        }
                    }
                }
                catch {
                    // MVC assembly is already running in application trust, so swallow exceptions
                }
            }

            // if there was an error, just process transparently
            return _callInAppTrustThunk ?? (Action<Action>)(f => f());
        }

        public static TResult ProcessInApplicationTrust<TResult>(Func<TResult> func) {
            TResult result = default(TResult);
            ProcessInApplicationTrust(delegate { result = func(); });
            return result;
        }

        public static void ProcessInApplicationTrust(Action action) {
            Action<Action> executor = GetCallInAppTrustThunk();
            executor(action);
        }

    }
}
