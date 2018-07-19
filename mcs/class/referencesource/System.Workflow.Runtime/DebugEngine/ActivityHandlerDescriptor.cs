// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.Runtime.InteropServices;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;

#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public struct ActivityHandlerDescriptor
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string Name;
        public int Token;
    }
}
