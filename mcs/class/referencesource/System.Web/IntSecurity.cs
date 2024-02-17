namespace System.Web {
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.Security.Permissions;

    internal static class InternalSecurityPermissions {

        private static IStackWalk   _unrestricted;
        private static IStackWalk   _unmanagedCode;
        private static IStackWalk   _controlPrincipal;
        private static IStackWalk   _reflection;
        private static IStackWalk   _appPathDiscovery;
        private static IStackWalk   _controlThread;
        private static IStackWalk   _levelLow;
        private static IStackWalk   _levelMedium;
        private static IStackWalk   _levelHigh;

        //
        // Static permissions as properties, created on demand
        //

        internal static IStackWalk Unrestricted {
            get {
                if (_unrestricted == null)
                    _unrestricted = new PermissionSet(PermissionState.Unrestricted);

                System.Web.Util.Debug.Trace("Permissions", "Unrestricted Set");
                return _unrestricted;
            }
        }

        internal static IStackWalk UnmanagedCode {
            get {                
                System.Web.Util.Debug.Trace("Permissions", "UnmanagedCode");
                return Unrestricted;
            }
        }

        internal static IStackWalk ControlPrincipal {
            get {                
                System.Web.Util.Debug.Trace("Permissions", "ControlPrincipal");
                return Unrestricted;
            }
        }

        internal static IStackWalk Reflection {
            get {                
                System.Web.Util.Debug.Trace("Permissions", "Reflection");
                return Unrestricted;
            }
        }

        internal static IStackWalk AppPathDiscovery {
            get {                
                if (_appPathDiscovery == null)
                    _appPathDiscovery = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, HttpRuntime.AppDomainAppPathInternal);
 
                Debug.Trace("Permissions", "AppPathDiscovery");
                return _appPathDiscovery;
            }
        }

        internal static IStackWalk ControlThread {
            get {                
                System.Web.Util.Debug.Trace("Permissions", "ControlThread");
                return Unrestricted;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelLow {
            get {
                System.Web.Util.Debug.Trace("Permissions", "AspNetHostingPermissionLevelLow");
                return Unrestricted;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelMedium {
            get {
                System.Web.Util.Debug.Trace("Permissions", "AspNetHostingPermissionLevelMedium");
                return Unrestricted;
            }
        }

        internal static IStackWalk AspNetHostingPermissionLevelHigh {
            get {            
                System.Web.Util.Debug.Trace("Permissions", "AspNetHostingPermissionLevelHigh");
                return Unrestricted;
            }
        }


        // Parameterized permissions

        internal static IStackWalk FileReadAccess(String filename) {
            Debug.Trace("Permissions", "FileReadAccess(" + filename + ")");
            return new FileIOPermission(FileIOPermissionAccess.Read, filename);
        }
 
        internal static IStackWalk FileWriteAccess(String filename) {
            Debug.Trace("Permissions", "FileWriteAccess(" + filename + ")");
            return new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, filename);
        }
 
        internal static IStackWalk PathDiscovery(String path) {
            Debug.Trace("Permissions", "PathDiscovery(" + path + ")");
            return new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
        }

    }
}
