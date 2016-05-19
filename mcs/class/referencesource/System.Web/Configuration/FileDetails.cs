//------------------------------------------------------------------------------
// <copyright file="FileDetails.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Security.Principal;
    using System.Threading;
    using System.Globalization;

    internal class FileDetails
    {
        bool _exists;
        long _fileSize;
        DateTime _utcCreationTime;
        DateTime _utcLastWriteTime;

        internal FileDetails(bool exists, long fileSize, DateTime utcCreationTime, DateTime utcLastWriteTime) {
            _exists = exists;
            _fileSize = fileSize;
            _utcCreationTime = utcCreationTime;
            _utcLastWriteTime = utcLastWriteTime;
        }

        public override bool Equals(Object obj) {
            FileDetails other = obj as FileDetails;
            return
                   other != null
                && _exists == other._exists
                && _fileSize == other._fileSize
                && _utcCreationTime == other._utcCreationTime
                && _utcLastWriteTime == other._utcLastWriteTime;
        }

        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(_exists.GetHashCode(), _fileSize.GetHashCode(),
                                                     _utcCreationTime.GetHashCode(), _utcLastWriteTime.GetHashCode());
        }
    }
}
