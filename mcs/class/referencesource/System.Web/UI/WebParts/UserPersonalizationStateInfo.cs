//------------------------------------------------------------------------------
// <copyright file="UserPersonalizationStateInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Web.Util;

    [Serializable]
    public sealed class UserPersonalizationStateInfo : PersonalizationStateInfo {

        private string _username;
        private DateTime _lastActivityDate;

        public UserPersonalizationStateInfo(string path,
                                            DateTime lastUpdatedDate,
                                            int size,
                                            string username,
                                            DateTime lastActivityDate) :
                                            base(path, lastUpdatedDate, size) {
            _username = StringUtil.CheckAndTrimString(username, "username");
            _lastActivityDate = lastActivityDate.ToUniversalTime();
        }

        public string Username {
            get {
                return _username;
            }
        }

        public DateTime LastActivityDate {
            get {
                return _lastActivityDate.ToLocalTime();
            }
        }
    }
}
