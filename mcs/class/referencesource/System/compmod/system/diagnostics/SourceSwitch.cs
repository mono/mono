//------------------------------------------------------------------------------
// <copyright file="SourceSwitch.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics {
    public class SourceSwitch : Switch {
        public SourceSwitch(string name) : base(name, String.Empty) {}

        public SourceSwitch(string displayName, string defaultSwitchValue) 
            : base(displayName, String.Empty, defaultSwitchValue) { }

        public SourceLevels Level {
            get {
                return (SourceLevels) SwitchSetting;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set {
                SwitchSetting = (int) value;
            }
        }

        public bool ShouldTrace(TraceEventType eventType) {
            return (SwitchSetting & (int) eventType) != 0;
        }

        protected override void OnValueChanged() {
            SwitchSetting = (int) Enum.Parse(typeof(SourceLevels), Value, true);
        }
    }
}
