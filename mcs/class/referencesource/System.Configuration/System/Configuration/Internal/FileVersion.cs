//------------------------------------------------------------------------------
// <copyright file="FileVersion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {
    using System.Configuration;
    using System.IO;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Threading;
    using System.Security;
    using System.CodeDom.Compiler;
    using Microsoft.Win32;	
#if !FEATURE_PAL
    using System.Security.AccessControl;
#endif

    internal class FileVersion {
        bool        _exists;
        long        _fileSize;
        DateTime    _utcCreationTime;
        DateTime    _utcLastWriteTime;

        internal FileVersion(bool exists, long fileSize, DateTime utcCreationTime, DateTime utcLastWriteTime) {
            _exists = exists;
            _fileSize = fileSize;
            _utcCreationTime = utcCreationTime;
            _utcLastWriteTime = utcLastWriteTime;
        }

        public override bool Equals(Object obj) {
            FileVersion other = obj as FileVersion;
            return
                   other != null
                && _exists == other._exists
                && _fileSize == other._fileSize
                && _utcCreationTime == other._utcCreationTime
                && _utcLastWriteTime == other._utcLastWriteTime;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
