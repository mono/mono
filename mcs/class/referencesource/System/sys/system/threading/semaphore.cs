#if MONO
#undef FEATURE_PAL
#endif
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Threading
{
    using System.IO;
    using System.IO.Ports;  // For InternalResources class
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;    
    // CoreSys build problem - we're using mscorlib's implementation assembly instead of one from asmmeta.  There's a conflicting NativeMethods type.
    using Marshal = System.Runtime.InteropServices.Marshal;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
#if !FEATURE_PAL && !FEATURE_NETCORE
    using System.Security.AccessControl;
#endif
    using System.Runtime.Versioning;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;


    [HostProtection(Synchronization=true, ExternalThreading=true)]
    [ComVisibleAttribute(false)]
    public sealed class Semaphore: WaitHandle
    {
        private const int MAX_PATH = 260;

        // creates a nameless semaphore object
        // Win32 only takes maximum count of Int32.MaxValue
        [SecuritySafeCritical]
#if !FEATURE_NETCORE
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
#endif // !FEATURE_NETCORE
        public Semaphore(int initialCount, int maximumCount) : this(initialCount,maximumCount,null){}

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public Semaphore(int initialCount, int maximumCount, string name)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }

            if (maximumCount < 1)
            {
                throw new ArgumentOutOfRangeException("maximumCount", SR.GetString(SR.ArgumentOutOfRange_NeedPosNum));
            }

            if (initialCount > maximumCount)
            {
                throw new ArgumentException(SR.GetString(SR.Argument_SemaphoreInitialMaximum));
            }

            if(null != name && MAX_PATH < name.Length)
            {
                throw new ArgumentException(SR.GetString(SR.Argument_WaitHandleNameTooLong));
            }

#if MONO
            int errorCode;
            var myHandle = new SafeWaitHandle (CreateSemaphore_internal (initialCount, maximumCount, name, out errorCode), true);
#else
            SafeWaitHandle   myHandle = SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
#endif
            
            if (myHandle.IsInvalid)
            {
#if !MONO
                int errorCode = Marshal.GetLastWin32Error(); 
#endif

                if(null != name && 0 != name.Length && NativeMethods.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(SR.GetString(SR.WaitHandleCannotBeOpenedException_InvalidHandle,name));
               
#if MONO
                InternalResources.WinIOError(errorCode, "");
#else
                InternalResources.WinIOError();
#endif
            }
            this.SafeWaitHandle = myHandle;
        }

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public Semaphore(int initialCount, int maximumCount, string name, out bool createdNew)
#if !FEATURE_PAL && !FEATURE_NETCORE
            : this(initialCount, maximumCount, name, out createdNew, null)
        {
        }
            
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public unsafe Semaphore(int initialCount, int maximumCount, string name, out bool createdNew, SemaphoreSecurity semaphoreSecurity)
#endif
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }

            if (maximumCount < 1)
            {
                throw new ArgumentOutOfRangeException("maximumCount", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }

            if (initialCount > maximumCount)
            {
                throw new ArgumentException(SR.GetString(SR.Argument_SemaphoreInitialMaximum));
            }
            
            if(null != name && MAX_PATH < name.Length)
            {
                throw new ArgumentException(SR.GetString(SR.Argument_WaitHandleNameTooLong));
            }
            SafeWaitHandle   myHandle;

#if MONO
            int errorCode;
            myHandle = new SafeWaitHandle (CreateSemaphore_internal (initialCount, maximumCount, name, out errorCode), true);
#else
#if !FEATURE_PAL && !FEATURE_NETCORE
            // For ACL's, get the security descriptor from the SemaphoreSecurity.
            if (semaphoreSecurity != null) {
                NativeMethods.SECURITY_ATTRIBUTES secAttrs = null;
                secAttrs = new NativeMethods.SECURITY_ATTRIBUTES();
                secAttrs.nLength = (int)Marshal.SizeOf(secAttrs);
                byte[] sd = semaphoreSecurity.GetSecurityDescriptorBinaryForm();
                fixed(byte* pSecDescriptor = sd) {                
                    secAttrs.lpSecurityDescriptor = new SafeLocalMemHandle((IntPtr) pSecDescriptor, false);
                    myHandle = SafeNativeMethods.CreateSemaphore(secAttrs, initialCount, maximumCount, name);
                }
            }
            else {
#endif
                myHandle = SafeNativeMethods.CreateSemaphore(null, initialCount, maximumCount, name);
#if !FEATURE_PAL && !FEATURE_NETCORE
            }
#endif

            int errorCode = Marshal.GetLastWin32Error();
#endif
            if (myHandle.IsInvalid)
            {
                if(null != name && 0 != name.Length && NativeMethods.ERROR_INVALID_HANDLE == errorCode)
                    throw new WaitHandleCannotBeOpenedException(SR.GetString(SR.WaitHandleCannotBeOpenedException_InvalidHandle,name));

#if MONO
                InternalResources.WinIOError(errorCode, "");
#else
                InternalResources.WinIOError();
#endif
            }
            createdNew = errorCode != NativeMethods.ERROR_ALREADY_EXISTS;
            this.SafeWaitHandle = myHandle;
        }

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        private Semaphore(SafeWaitHandle handle)
        {
            this.SafeWaitHandle = handle;
        }

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public static Semaphore OpenExisting(string name)
        {
#if !FEATURE_PAL && !FEATURE_NETCORE
            return OpenExisting(name, SemaphoreRights.Modify | SemaphoreRights.Synchronize);
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static Semaphore OpenExisting(string name, SemaphoreRights rights)
        {
            Semaphore result;
            switch (OpenExistingWorker(name, rights, out result))
#else //FEATURE_PAL || FEATURE_NETCORE
            Semaphore result;
            switch (OpenExistingWorker(name, out result))
#endif //FEATURE_PAL || FEATURE_NETCORE
            {
                case OpenExistingResult.NameNotFound:
                    throw new WaitHandleCannotBeOpenedException();
                case OpenExistingResult.NameInvalid:
                    throw new WaitHandleCannotBeOpenedException(SR.GetString(SR.WaitHandleCannotBeOpenedException_InvalidHandle, name));
                case OpenExistingResult.PathNotFound:
                    InternalResources.WinIOError(NativeMethods.ERROR_PATH_NOT_FOUND, string.Empty);
                    return result; //never executes
                default:
                    return result;
            }
        }

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public static bool TryOpenExisting(string name, out Semaphore result)
        {
#if !FEATURE_PAL && !FEATURE_NETCORE
            return OpenExistingWorker(name, SemaphoreRights.Modify | SemaphoreRights.Synchronize, out result) == OpenExistingResult.Success;
#else
            return OpenExistingWorker(name, out result) == OpenExistingResult.Success;
#endif
        }

#if !FEATURE_PAL && !FEATURE_NETCORE
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static bool TryOpenExisting(string name, SemaphoreRights rights, out Semaphore result)
        {
            return OpenExistingWorker(name, rights, out result) == OpenExistingResult.Success;
        }
#endif

        // This exists in WaitHandle, but is oddly ifdefed for some reason...
        private enum OpenExistingResult
        {
            Success,
            NameNotFound,
            PathNotFound,
            NameInvalid
        }

#if FEATURE_NETCORE
        [SecurityCritical]
#else
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        private static OpenExistingResult OpenExistingWorker(
            string name, 
#if !FEATURE_PAL && !FEATURE_NETCORE
            SemaphoreRights rights, 
#endif
            out Semaphore result)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if(name.Length  == 0)
            {
                throw new ArgumentException(SR.GetString(SR.InvalidNullEmptyArgument, "name"), "name");
            }
            if(null != name && MAX_PATH < name.Length)
            {
                throw new ArgumentException(SR.GetString(SR.Argument_WaitHandleNameTooLong));
            }

            result = null;
#if MOBILE
            throw new NotSupportedException ();
#else

#if MONO
            int errorCode;
            var myHandle = new SafeWaitHandle (OpenSemaphore_internal (name, rights, out errorCode), true);
#else
            //Pass false to OpenSemaphore to prevent inheritedHandles
#if FEATURE_PAL || FEATURE_NETCORE
            const int SYNCHRONIZE            = 0x00100000;
            const int SEMAPHORE_MODIFY_STATE = 0x00000002;

            SafeWaitHandle myHandle = SafeNativeMethods.OpenSemaphore(SEMAPHORE_MODIFY_STATE | SYNCHRONIZE, false, name);
#else
            SafeWaitHandle myHandle = SafeNativeMethods.OpenSemaphore((int) rights, false, name);
#endif
#endif
            
            if (myHandle.IsInvalid)
            {
#if !MONO
                int errorCode = Marshal.GetLastWin32Error();
#endif

                if (NativeMethods.ERROR_FILE_NOT_FOUND == errorCode || NativeMethods.ERROR_INVALID_NAME == errorCode)
                    return OpenExistingResult.NameNotFound;
                if (NativeMethods.ERROR_PATH_NOT_FOUND == errorCode)
                    return OpenExistingResult.PathNotFound;
                if (null != name && 0 != name.Length && NativeMethods.ERROR_INVALID_HANDLE == errorCode)
                    return OpenExistingResult.NameInvalid;
                //this is for passed through NativeMethods Errors
#if MONO
                InternalResources.WinIOError(errorCode, "");
#else
                InternalResources.WinIOError();
#endif
            }
            result = new Semaphore(myHandle);
            return OpenExistingResult.Success;
#endif
        }


        // increase the count on a semaphore, returns previous count
#if !FEATURE_NETCORE
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [PrePrepareMethod]
#endif
        public int Release()
        {  
            return Release(1);
        }

        // increase the count on a semaphore, returns previous count
#if FEATURE_NETCORE
        [SecuritySafeCritical]
#else
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        public int Release(int releaseCount)
        {
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException("releaseCount", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }
            int previousCount;

            //If ReleaseSempahore returns false when the specified value would cause
            //   the semaphore's count to exceed the maximum count set when Semaphore was created
            //Non-Zero return 

#if MONO
            if (!ReleaseSemaphore_internal(Handle, releaseCount, out previousCount))
#else
            if (!SafeNativeMethods.ReleaseSemaphore(SafeWaitHandle, releaseCount, out previousCount))
#endif
            {
                throw new SemaphoreFullException();
            }

            return previousCount;
        }

#if !FEATURE_PAL && !FEATURE_NETCORE
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public SemaphoreSecurity GetAccessControl() {
            return new SemaphoreSecurity(SafeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void SetAccessControl(SemaphoreSecurity semaphoreSecurity) {
            if (semaphoreSecurity == null)
                throw new ArgumentNullException("semaphoreSecurity");

            semaphoreSecurity.Persist(SafeWaitHandle);
        }
#endif

#if MONO
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern IntPtr CreateSemaphore_internal (
            int initialCount, int maximumCount, string name, out int errorCode);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool ReleaseSemaphore_internal (
            IntPtr handle, int releaseCount, out int previousCount);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        private static extern IntPtr OpenSemaphore_internal (string name, SemaphoreRights rights, out int errorCode);
#endif
    }
}

