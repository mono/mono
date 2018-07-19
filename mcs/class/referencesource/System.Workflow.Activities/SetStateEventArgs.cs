// ****************************************************************************
// Copyright (C) Microsoft Corporation.  All rights reserved.
// ****************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Runtime.InteropServices;

namespace System.Workflow.Activities
{
    [Serializable]
    [ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SetStateEventArgs : EventArgs
    {
        string targetStateName;

        public SetStateEventArgs(string targetStateName)
        {
            this.targetStateName = targetStateName;
        }

        public string TargetStateName
        {
            get
            {
                return targetStateName;
            }
        }
    }
}
