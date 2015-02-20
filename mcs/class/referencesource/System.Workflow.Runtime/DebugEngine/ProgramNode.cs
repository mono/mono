// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    internal sealed class ProgramNode : IWDEProgramNode
    {
        private DebugController controller;

        public ProgramNode(DebugController controller)
        {
            this.controller = controller;
        }

        void IWDEProgramNode.Attach(ref Guid programId, int attachTimeout, int detachPingInterval, out string hostName, out string uri, out int controllerThreadId, out bool isSynchronousAttach)
        {
            this.controller.Attach(programId, attachTimeout, detachPingInterval, out hostName, out uri, out controllerThreadId, out isSynchronousAttach);
        }
    }

    [ComImport(), Guid(Guids.IID_IWDEProgramNode), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IWDEProgramNode
    {
        void Attach(ref Guid programId, int attachTimeout, int detachPingInterval, [Out, MarshalAs(UnmanagedType.BStr)] out string hostName, [Out, MarshalAs(UnmanagedType.BStr)] out string uri, [Out] out int controllerThreadId, [Out, MarshalAs(UnmanagedType.Bool)] out bool isSynchronousAttach);
    }
}
