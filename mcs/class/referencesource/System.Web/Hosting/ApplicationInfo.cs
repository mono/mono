//------------------------------------------------------------------------------
// <copyright file="ApplicationInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ApplicationInfo {
        private string _id;
        private VirtualPath _virtualPath;
        private string _physicalPath;

        internal ApplicationInfo(string id, VirtualPath virtualPath, string physicalPath) {
            _id = id;
            _virtualPath = virtualPath;
            _physicalPath = physicalPath;
        }


        public String ID {
            get { return _id; }
        }


        public String VirtualPath {
            get { return _virtualPath.VirtualPathString; }
        }


        public String PhysicalPath {
            get { return _physicalPath; }
        }
    }
}
