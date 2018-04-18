// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>Microsoft</OWNER>

using System;
using System.Security;

namespace System.Runtime.InteropServices.WindowsRuntime
{
    // WinRTClassActivator is used to host managed winmds in desktop AppDomains, and is accessed via the native GetClassActivatorForApplication API
    internal sealed class WinRTClassActivator : MarshalByRefObject, IWinRTClassActivator
    {
        [SecurityCritical]
        public object ActivateInstance(string activatableClassId)
        {
            ManagedActivationFactory activationFactory = WindowsRuntimeMarshal.GetManagedActivationFactory(LoadWinRTType(activatableClassId));
            return activationFactory.ActivateInstance();
        }

        [SecurityCritical]
        public IntPtr GetActivationFactory(string activatableClassId, ref Guid iid)
        {
            IntPtr activationFactory = IntPtr.Zero;
            try
            {
                activationFactory = WindowsRuntimeMarshal.GetActivationFactoryForType(LoadWinRTType(activatableClassId));
                
                IntPtr factoryInterface = IntPtr.Zero;
                int hr = Marshal.QueryInterface(activationFactory, ref iid, out factoryInterface);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return factoryInterface;
            }
            finally
            {
                if (activationFactory != IntPtr.Zero)
                {
                    Marshal.Release(activationFactory);
                }
            }
        }

        private Type LoadWinRTType(string acid)
        {
            Type winrtType = Type.GetType(acid + ", Windows, ContentType=WindowsRuntime");
            if (winrtType == null)
            {
                // If the requested type could not be found, the result of the IWinRTClassActivator API is defined to be REGDB_E_CLASSNOTREG
                throw new COMException(__HResults.REGDB_E_CLASSNOTREG);
            }

            return winrtType;
        }

        [SecurityCritical]
        internal IntPtr GetIWinRTClassActivator()
        {
            return Marshal.GetComInterfaceForObject(this, typeof(IWinRTClassActivator));
        }
    }
}
