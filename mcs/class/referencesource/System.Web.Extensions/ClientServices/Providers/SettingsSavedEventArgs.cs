//------------------------------------------------------------------------------
// <copyright file="SettingsSavedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ClientServices.Providers {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class SettingsSavedEventArgs : EventArgs {
        private ReadOnlyCollection<string> _failedSettingsList;

        public SettingsSavedEventArgs(IEnumerable<string> failedSettingsList) {
            List<string> list =
                (failedSettingsList == null) ? new List<string>() : new List<string>(failedSettingsList);
            _failedSettingsList = new ReadOnlyCollection<string>(list);
        }

        public ReadOnlyCollection<string> FailedSettingsList {
            get {
                return _failedSettingsList;
            }
        }
    }
}
