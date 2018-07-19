//------------------------------------------------------------------------------
// <copyright file="ConnectivityStatus.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices
{
    using System;
    using System.IO;
    using System.Security.Principal;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;

    public static class ConnectivityStatus
    {
        public static bool IsOffline  {
            get {
                if (!_IsOfflineFetched)
                    FetchIsOffline();
                return _IsOffline;
            }
            set {
                if (IsOffline != value) {
                    _IsOffline = value;
                    StoreIsOffline();
                }
            }
        }

        private static bool _IsOffline;
        private static bool _IsOfflineFetched;

        //[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void FetchIsOffline()
        {
            string path = Path.Combine(System.Windows.Forms.Application.UserAppDataPath, "AppIsOffline");
            _IsOffline = File.Exists(path);
            _IsOfflineFetched = true;
        }


        //[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void StoreIsOffline()
        {
            string path = Path.Combine(System.Windows.Forms.Application.UserAppDataPath, "AppIsOffline");
            if (!_IsOffline) {
                File.Delete(path);
            } else {
                using (FileStream fs = File.Create(path)) {
                    fs.Write(new byte[0], 0, 0);
                }
            }
        }
    }
}
