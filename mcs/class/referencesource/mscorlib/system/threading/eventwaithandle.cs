// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>Microsoft</OWNER>
/*=============================================================================
**
** Class: EventWaitHandle
**
**
** Purpose: Base class for representing Events
**
**
=============================================================================*/


#if !MOBILE
#if !FEATURE_MACL
namespace System.Security.AccessControl
{
    public class EventWaitHandleSecurity
    {
    }
    public enum EventWaitHandleRights
    {
    }
}
#endif
#endif

namespace System.Threading
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.IO;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.AccessControl;
    using System.Diagnostics.Contracts;

    [HostProtection(Synchronization=true, ExternalThreading=true)]
    [ComVisibleAttribute(true)]
    public class EventWaitHandle : WaitHandle
    {
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public EventWaitHandle(bool initialState, EventResetMode mode) : this(initialState,mode,null) { }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public EventWaitHandle(bool initialState, EventResetMode mode, string name)
        {
            if(null != name && System.IO.Path.MAX_PATH < name.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong",name));
            }
            Contract.EndContractBlock();
            
            SafeWaitHandle _handle = null;
#if MONO
            int errorCode;
#endif
            switch(mode)
            {
                case EventResetMode.ManualReset:
#if MONO
                    _handle = new SafeWaitHandle (NativeEventCalls.CreateEvent_internal (true, initialState, name, out errorCode), true);
#else
                    _handle = Win32Native.CreateEvent(null, true, initialState, name);
#endif
                    break;
                case EventResetMode.AutoReset:
#if MONO
                    _handle = new SafeWaitHandle (NativeEventCalls.CreateEvent_internal (false, initialState, name, out errorCode), true);
#else
                    _handle = Win32Native.CreateEvent(null, false, initialState, name);
#endif
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag",name));
            };
                
            if (_handle.IsInvalid)
            {
#if !MONO
                int errorCode = Marshal.GetLastWin32Error();
#endif
            
                _handle.SetHandleAsInvalid();
                if(null != name && 0 != name.Length && Win32Native.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle",name));

                __Error.WinIOError(errorCode, name);
            }
            SetHandleInternal(_handle);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew)
            : this(initialState, mode, name, out createdNew, null)
        {
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public unsafe EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity)
        {
            if(null != name && System.IO.Path.MAX_PATH < name.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong",name));
            }
            Contract.EndContractBlock();
#if !MONO
            Win32Native.SECURITY_ATTRIBUTES secAttrs = null;

#if FEATURE_MACL
            // For ACL's, get the security descriptor from the EventWaitHandleSecurity.
            if (eventSecurity != null) {
                secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);

                byte[] sd = eventSecurity.GetSecurityDescriptorBinaryForm();
                byte* pSecDescriptor = stackalloc byte[sd.Length];
                Buffer.Memcpy(pSecDescriptor, 0, sd, 0, sd.Length);
                secAttrs.pSecurityDescriptor = pSecDescriptor;
            }
#endif
#endif

            SafeWaitHandle _handle = null;
            Boolean isManualReset;
            switch(mode)
            {
                case EventResetMode.ManualReset:
                    isManualReset = true;
                    break;
                case EventResetMode.AutoReset:
                    isManualReset = false;
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag",name));
            };

#if MONO
            int errorCode;
            _handle = new SafeWaitHandle (NativeEventCalls.CreateEvent_internal (isManualReset, initialState, name, out errorCode), true);
#else
            _handle = Win32Native.CreateEvent(secAttrs, isManualReset, initialState, name);
            int errorCode = Marshal.GetLastWin32Error();
#endif

            if (_handle.IsInvalid)
            {

                _handle.SetHandleAsInvalid();
                if(null != name && 0 != name.Length && Win32Native.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle",name));

                __Error.WinIOError(errorCode, name);
            }
            createdNew = errorCode != Win32Native.ERROR_ALREADY_EXISTS;
            SetHandleInternal(_handle);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private EventWaitHandle(SafeWaitHandle handle)
        {
            SetHandleInternal(handle);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static EventWaitHandle OpenExisting(string name)
        {
#if !FEATURE_MACL
            return OpenExisting(name, (EventWaitHandleRights)0);
#else
            return OpenExisting(name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize);
#endif  // !FEATURE_PAL
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static EventWaitHandle OpenExisting(string name, EventWaitHandleRights rights)
        {
            EventWaitHandle result;
            switch (OpenExistingWorker(name, rights, out result))
            {
                case OpenExistingResult.NameNotFound:
                    throw new WaitHandleCannotBeOpenedException();

                case OpenExistingResult.NameInvalid:
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name));

                case OpenExistingResult.PathNotFound:
                    __Error.WinIOError(Win32Native.ERROR_PATH_NOT_FOUND, "");
                    return result; //never executes

                default:
                    return result;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static bool TryOpenExisting(string name, out EventWaitHandle result)
        {
#if !FEATURE_MACL
            return OpenExistingWorker(name, (EventWaitHandleRights)0, out result) == OpenExistingResult.Success;
#else
            return OpenExistingWorker(name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, out result) == OpenExistingResult.Success;
#endif  // !FEATURE_PAL
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static bool TryOpenExisting(string name, EventWaitHandleRights rights, out EventWaitHandle result)
        {
            return OpenExistingWorker(name, rights, out result) == OpenExistingResult.Success;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static OpenExistingResult OpenExistingWorker(string name, EventWaitHandleRights rights, out EventWaitHandle result)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_WithParamName"));
            }

            if(name.Length  == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }

            if(null != name && System.IO.Path.MAX_PATH < name.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong",name));
            }
            
            Contract.EndContractBlock();

            result = null;

#if MOBILE
            throw new NotSupportedException ();
#else

#if MONO
            int errorCode;
            var myHandle = new SafeWaitHandle (NativeEventCalls.OpenEvent_internal (name, rights, out errorCode), true);
#else
#if FEATURE_MACL
            SafeWaitHandle myHandle = Win32Native.OpenEvent((int) rights, false, name);
#else
            SafeWaitHandle myHandle = Win32Native.OpenEvent(Win32Native.EVENT_MODIFY_STATE | Win32Native.SYNCHRONIZE, false, name);
#endif
#endif
            
            if (myHandle.IsInvalid)
            {
#if !MONO
                int errorCode = Marshal.GetLastWin32Error();
#endif

                if(Win32Native.ERROR_FILE_NOT_FOUND == errorCode || Win32Native.ERROR_INVALID_NAME == errorCode)
                    return OpenExistingResult.NameNotFound;
                if (Win32Native.ERROR_PATH_NOT_FOUND == errorCode)
                    return OpenExistingResult.PathNotFound;
                if(null != name && 0 != name.Length && Win32Native.ERROR_INVALID_HANDLE == errorCode)
                    return OpenExistingResult.NameInvalid;
                //this is for passed through Win32Native Errors
                __Error.WinIOError(errorCode,"");
            }
            result = new EventWaitHandle(myHandle);
            return OpenExistingResult.Success;
#endif
        }
        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool Reset()
        {
#if MONO
            var res = NativeEventCalls.ResetEvent(safeWaitHandle);
#else
            bool res = Win32Native.ResetEvent(safeWaitHandle);
#endif
            if (!res)
#if MONO
                throw new IOException ();
#else
                __Error.WinIOError();
#endif
            return res;
        }
        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool Set()
        {
#if MONO
            var res = NativeEventCalls.SetEvent(safeWaitHandle);
#else
            bool res = Win32Native.SetEvent(safeWaitHandle);
#endif

            if (!res)
#if MONO
                throw new IOException ();
#else
                __Error.WinIOError();
#endif

            return res;
        }

#if FEATURE_MACL || MOBILE
        [System.Security.SecuritySafeCritical]  // auto-generated
        public EventWaitHandleSecurity GetAccessControl()
        {
            return new EventWaitHandleSecurity(safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        }
#endif

#if FEATURE_MACL
        [System.Security.SecuritySafeCritical]  // auto-generated
        public void SetAccessControl(EventWaitHandleSecurity eventSecurity)
        {
            if (eventSecurity == null)
                throw new ArgumentNullException("eventSecurity");
            Contract.EndContractBlock();

            eventSecurity.Persist(safeWaitHandle);
        }
#endif
    }
}

