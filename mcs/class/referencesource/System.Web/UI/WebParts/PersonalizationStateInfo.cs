//------------------------------------------------------------------------------
// <copyright file="PersonalizationStateInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Web.Util;

    [Serializable]
    public abstract class PersonalizationStateInfo {

        private string _path;
        private DateTime _lastUpdatedDate;
        private int _size;

        // We only want our assembly to inherit this class, so make it internal
        internal PersonalizationStateInfo(string path, DateTime lastUpdatedDate, int size) {
            _path = StringUtil.CheckAndTrimString(path, "path");
            PersonalizationProviderHelper.CheckNegativeInteger(size, "size");
            _lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
            _size = size;
        }

        public string Path {
            get {
                return _path;
            }
        }

        public DateTime LastUpdatedDate {
            get {
                return _lastUpdatedDate.ToLocalTime();
            }
        }

        public int Size {
            get {
                return _size;
            }
        }
    }
}
