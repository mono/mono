/*++
Copyright (c) Microsoft Corporation

Module Name:

    _SafeNetHandles.cs

Abstract:
        The file contains _all_ SafeHandles implementations for System.Net namespace.
        These handle wrappers do guarantee that OS resources get cleaned up when the app domain dies.

        All PInvoke declarations that do freeing  the  OS resources  _must_ be in this file
        All PInvoke declarations that do allocation the OS resources _must_ be in this file


Details:

        The protection from leaking OF the OS resources is based on two technologies
        1) SafeHandle class
        2) Non interuptible regions using Constrained Execution Region (CER) technology

        For simple cases SafeHandle class does all the job. The Prerequisites are:
        - A resource is able to be represented by IntPtr type (32 bits on 32 bits platforms).
        - There is a PInvoke availble that does the creation of the resource.
          That PInvoke either returns the handle value or it writes the handle into out/ref parameter.
        - The above PInvoke as part of the call does NOT free any OS resource.

        For those "simple" cases we desinged SafeHandle-derived classes that provide
        static methods to allocate a handle object.
        Each such derived class provides a handle release method that is run as non-interrupted.

        For more complicated cases we employ the support for non-interruptible methods (CERs).
        Each CER is a tree of code rooted at a catch or finally clause for a specially marked exception
        handler (preceded by the RuntimeHelpers.PrepareConstrainedRegions() marker) or the Dispose or
        ReleaseHandle method of a SafeHandle derived class. The graph is automatically computed by the
        runtime (typically at the jit time of the root method), but cannot follow virtual or interface
        calls (these must be explicitly prepared via RuntimeHelpers.PrepareMethod once the definite target
        method is known). Also, methods in the graph that must be included in the CER must be marked with
        a reliability contract stating guarantees about the consistency of the system if an error occurs
        while they are executing. Look for ReliabilityContract for examples (a full explanation of the
        semantics of this contract is beyond the scope of this comment).

        An example of the top-level of a CER:

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                // Normal code
            }
            finally
            {
                // Guaranteed to get here even in low memory scenarios. Thread abort will not interrupt
                // this clause and we won't fail because of a jit allocation of any method called (modulo
                // restrictions on interface/virtual calls listed above and further restrictions listed
                // below).
            }

        Another common pattern is an empty-try (where you really just want a region of code the runtime
        won't interrupt you in):

            RuntimeHelpers.PrepareConstrainedRegions();
            try {} finally
            {
                // Non-interruptible code here
            }

        This ugly syntax will be supplanted with compiler support at some point.

        While within a CER region certain restrictions apply in order to avoid having the runtime inject
        a potential fault point into your code (and of course you're are responsible for ensuring your
        code doesn't inject any explicit fault points of its own unless you know how to tolerate them).

        A quick and dirty guide to the possible causes of fault points in CER regions:
        - Explicit allocations (though allocating a value type only implies allocation on the stack,
          which may not present an issue).
        - Boxing a value type (C# does this implicitly for you in many cases, so be careful).
        - Use of Monitor.Enter or the lock keyword.
        - Accessing a multi-dimensional array.
        - Calling any method outside your control that doesn't make a guarantee (e.g. via a
          ReliabilityAttribute) that it doesn't introduce failure points.
        - Making P/Invoke calls with non-blittable parameters types. Blittable types are:
            - SafeHandle when used as an [in] parameter
            - NON BOXED base types that fit onto a machine word
            - ref struct with blittable fields
            - class type with blittable fields
            - pinned Unicode strings using "fixed" statement
            - pointers of any kind
            - IntPtr type
        - P/Invokes should not have any CharSet attribute on it's declaration.
          Obvioulsy string types should not appear in the parameters.
        - String type MUST not appear in a field of a marshaled ref struct or class in a P?Invoke

Author:

    Alexei Vopilov    04-Sept-2002

Revision History:

--*/

namespace System.Net {
    using System.Net.Cache;
    using System.Net.Sockets;
    using System.Net.NetworkInformation;
    using System.Net.WebSockets;
    using System.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ComponentModel;
    using System.Text;
    using System.Globalization;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;

#if DEBUG
    //
    // This is a helper class for debugging GC-ed handles that we define.
    // As a general rule normal code path should always destroy handles explicitly
    //
    internal abstract class DebugSafeHandle: SafeHandleZeroOrMinusOneIsInvalid {
        string m_Trace;

        protected DebugSafeHandle(bool ownsHandle): base(ownsHandle) {
            Trace();
        }

        protected DebugSafeHandle(IntPtr invalidValue, bool ownsHandle): base(ownsHandle) {
            SetHandle(invalidValue);
            Trace();
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void Trace() {
            m_Trace = "WARNING! GC-ed  >>" + this.GetType().FullName + "<< (should be excplicitly closed) \r\n";
#if TRAVE
            (new FileIOPermission(PermissionState.Unrestricted)).Assert();
            string stacktrace = Environment.StackTrace;
            m_Trace += stacktrace;
            FileIOPermission.RevertAssert();
#endif //TRAVE
        }

        [ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.None)]
        ~DebugSafeHandle() {
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            GlobalLog.Print(m_Trace);
        }
    }

    //
    // This is a helper class for debugging GC-ed handles that we define.
    // As a general rule normal code path should always destroy handles explicitly
    //
    internal abstract class DebugCriticalHandleMinusOneIsInvalid : CriticalHandleMinusOneIsInvalid {
        string m_Trace;

        protected DebugCriticalHandleMinusOneIsInvalid(): base() {
            Trace();
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void Trace() {
            m_Trace = "WARNING! GC-ed  >>" + this.GetType().FullName + "<< (should be excplicitly closed) \r\n";
            GlobalLog.Print("Creating SafeHandle, type = " + this.GetType().FullName);
#if TRAVE
            (new FileIOPermission(PermissionState.Unrestricted)).Assert();
            string stacktrace = Environment.StackTrace;
            m_Trace += stacktrace;
            FileIOPermission.RevertAssert();
#endif //TRAVE
        }

        ~DebugCriticalHandleMinusOneIsInvalid() {
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            GlobalLog.Print(m_Trace);
        }
    }

    //
    // This is a helper class for debugging GC-ed handles that we define.
    // As a general rule normal code path should always destroy handles explicitly
    //
    internal abstract class DebugSafeHandleMinusOneIsInvalid : SafeHandleMinusOneIsInvalid {
        string m_Trace;

        protected DebugSafeHandleMinusOneIsInvalid(bool ownsHandle): base(ownsHandle) {
            Trace();
        }

        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void Trace() {
            m_Trace = "WARNING! GC-ed  >>" + this.GetType().FullName + "<< (should be excplicitly closed) \r\n";
            GlobalLog.Print("Creating SafeHandle, type = " + this.GetType().FullName);
#if TRAVE
            (new FileIOPermission(PermissionState.Unrestricted)).Assert();
            string stacktrace = Environment.StackTrace;
            m_Trace += stacktrace;
            FileIOPermission.RevertAssert();
#endif //TRAVE
        }

        ~DebugSafeHandleMinusOneIsInvalid() {
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            GlobalLog.Print(m_Trace);
        }
    }

    //
    // This is a helper class for debugging GC-ed handles that we define.
    // As a general rule normal code path should always destroy handles explicitly
    //
    internal abstract class DebugCriticalHandleZeroOrMinusOneIsInvalid : CriticalHandleZeroOrMinusOneIsInvalid {
        string m_Trace;

        protected DebugCriticalHandleZeroOrMinusOneIsInvalid(): base() {
            Trace();
        }

        [SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Justification="DEBUG use only: Require access to Environment.StackTrace regardless of app permissions")]
        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        private void Trace() {
            m_Trace = "WARNING! GC-ed  >>" + this.GetType().FullName + "<< (should be excplicitly closed) \r\n";
            GlobalLog.Print("Creating SafeHandle, type = " + this.GetType().FullName);
#if TRAVE
            (new FileIOPermission(PermissionState.Unrestricted)).Assert();
            string stacktrace = Environment.StackTrace;
            m_Trace += stacktrace;
            FileIOPermission.RevertAssert();
#endif //TRAVE
        }

        ~DebugCriticalHandleZeroOrMinusOneIsInvalid() {
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            GlobalLog.Print(m_Trace);
        }
    }
#endif // DEBUG


#if !FEATURE_PAL

    ///////////////////////////////////////////////////////////////
    //
    // This is safe handle implementaion that depends on
    // ws2_32.dll freeaddrinfo. It's only used by Dns class
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeFreeAddrInfo : DebugSafeHandle {
#else
    internal sealed class SafeFreeAddrInfo : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private const string WS2_32 = "ws2_32.dll";

        private SafeFreeAddrInfo(): base(true) {}

        internal static int GetAddrInfo(string nodename, string servicename, ref AddressInfo hints, out SafeFreeAddrInfo outAddrInfo) {
            return UnsafeNclNativeMethods.SafeNetHandlesXPOrLater.GetAddrInfoW(nodename, servicename, ref hints, out outAddrInfo);
        }

        override protected bool ReleaseHandle()
        {
            UnsafeNclNativeMethods.SafeNetHandlesXPOrLater.freeaddrinfo(handle);
            return true;
        }
    }

#endif // !FEATURE_PAL

    ///////////////////////////////////////////////////////////////
    //
    // This is safe handle factory for any object that depends on
    // KERNEL32 CloseHandle as the handle disposal method.
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeCloseHandle : DebugCriticalHandleZeroOrMinusOneIsInvalid {
#else
    internal sealed class SafeCloseHandle : CriticalHandleZeroOrMinusOneIsInvalid {
#endif
        private int _disposed;

        private SafeCloseHandle() : base() {
        }

        internal IntPtr DangerousGetHandle() {
            return handle;
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                if (Interlocked.Increment(ref _disposed) == 1) {
                    return UnsafeNclNativeMethods.SafeNetHandles.CloseHandle(handle);
                }
            }
            return true;
        }

#if !FEATURE_PAL
        // This method will bypass refCount check done by VM
        // Means it will force handle release if has a valid value
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Abort() {
            ReleaseHandle();
            SetHandleAsInvalid();
        }

#endif // !FEATURE_PAL

    }
    //
    // This class is a wrapper for Http.sys V2 request queue handle.
    //
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Suppress due to tools issues with unit test infrastructure")]
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class HttpRequestQueueV2Handle : DebugCriticalHandleZeroOrMinusOneIsInvalid {
#else
    internal sealed class HttpRequestQueueV2Handle : CriticalHandleZeroOrMinusOneIsInvalid {
#endif

        private int disposed;

        private HttpRequestQueueV2Handle() : base() {
        }

        internal IntPtr DangerousGetHandle() {
            return handle;
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                if (Interlocked.Increment(ref disposed) == 1) {
                    return (UnsafeNclNativeMethods.SafeNetHandles.HttpCloseRequestQueue(handle) ==
                        UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS);
                }
            }
            return true;
        }
    }

    //
    // This class is a wrapper for Http.sys V2 server session. CreateServerSession returns an ID and not a real handle
    // but we use CriticalHandle because it provides us the guarantee that CloseServerSession will always get called.
    //
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class HttpServerSessionHandle : DebugCriticalHandleZeroOrMinusOneIsInvalid {
#else
    internal sealed class HttpServerSessionHandle : CriticalHandleZeroOrMinusOneIsInvalid {
#endif

        private int disposed;
        private ulong serverSessionId;

        internal HttpServerSessionHandle(ulong id) : base() {
            serverSessionId = id;
            
            //
            // This class uses no real handle so we need to set a dummy handle. Otherwise, IsInvalid always remains             
            // true.
            //
            SetHandle(new IntPtr(1));
        }

        internal ulong DangerousGetServerSessionId() {
            return serverSessionId;
        }

        protected override bool ReleaseHandle() {
            if (!IsInvalid) {
                if (Interlocked.Increment(ref disposed) == 1) {

                    //
                    // Closing server session also closes all open url groups under that server session.
                    //
                    return (UnsafeNclNativeMethods.HttpApi.HttpCloseServerSession(serverSessionId) ==
                        UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS);
                }
            }
            return true;
        }
    }

    //
    // SafeHandle to wrap handles created by IcmpCreateFile or Icmp6CreateFile
    // from either icmp.dll or iphlpapi.dll. These handles must be closed by
    // IcmpCloseHandle.
    //
    // Code creating handles will use ComNetOS.IsPostWin2K to determine
    // which DLL being used. This code uses same construct to determine
    // which DLL being used but stashes the OS query results away at ctor
    // time so it is always available at critical finalizer time.
    //
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCloseIcmpHandle : SafeHandleZeroOrMinusOneIsInvalid {

        private SafeCloseIcmpHandle() : base(true) {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        override protected bool ReleaseHandle() {
            return UnsafeNetInfoNativeMethods.IcmpCloseHandle(handle);
        }
    }

    //
    // Used when working with WinHTTP APIs, like WinHttpOpen(). Holds the HINTERNET handle.
    //
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeInternetHandle : DebugSafeHandle {
#else
    internal sealed class SafeInternetHandle : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        public SafeInternetHandle() : base(true) {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected override bool ReleaseHandle() {
            return UnsafeNclNativeMethods.WinHttp.WinHttpCloseHandle(handle);
        }
    }

    //
    // Used when working with SSPI APIs, like SafeSspiAuthDataHandle(). Holds the pointer to the auth. data blob.
    //
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeSspiAuthDataHandle : DebugSafeHandle {
#else
    internal sealed class SafeSspiAuthDataHandle : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        public SafeSspiAuthDataHandle() : base(true) {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle() {
            return UnsafeNclNativeMethods.SspiHelper.SspiFreeAuthIdentity(handle) == SecurityStatus.OK;
        }
    }
    
#if !FEATURE_PAL

    ///////////////////////////////////////////////////////////////
    //
    //  A set of Safe Handles that depend on native FreeContextBuffer finalizer
    //
    ///////////////////////////////////////////////////////////////
//-------------------------------------------------------
    internal enum SecurDll {
        SECURITY    = 0,
        SECUR32     = 1, // Windows 9x only
        SCHANNEL    = 2, // Windows 9x only
    }
//=======================================================
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal abstract class SafeFreeContextBuffer : DebugSafeHandle {
#else
    internal abstract class SafeFreeContextBuffer : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        protected SafeFreeContextBuffer(): base(true) {}

        // This must be ONLY called from this file and in the context of a CER
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void Set(IntPtr value) {
            this.handle = value;
        }

        //
        //
        internal static int EnumeratePackages(SecurDll Dll, out int pkgnum, out SafeFreeContextBuffer pkgArray) {

            int res = -1;
            switch (Dll) {
            case SecurDll.SECURITY:
                SafeFreeContextBuffer_SECURITY pkgArray_SECURITY = null;
                res = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.EnumerateSecurityPackagesW(out pkgnum, out pkgArray_SECURITY);
                pkgArray = pkgArray_SECURITY;
                break;

            default: throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
            }
            if (res != 0 && pkgArray != null) {
                pkgArray.SetHandleAsInvalid();
            }
            return res;
        }

        //
        //
        internal static SafeFreeContextBuffer CreateEmptyHandle(SecurDll dll) {
            switch (dll) {
            case SecurDll.SECURITY: return new SafeFreeContextBuffer_SECURITY();
            default: throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "dll");
            }
        }

        //
        // After PINvoke call the method will fix the refHandle.handle with the returned value.
        // The caller is responsible for creating a correct SafeHandle template or null can be passed if no handle is returned.
        //
        // This method switches between three non-interruptible helper methods.  (This method can't be both non-interruptible and
        // reference imports from all three DLLs - doing so would cause all three DLLs to try to be bound to.)
        //
        public unsafe static int QueryContextAttributes(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return QueryContextAttributes_SECURITY(phContext, contextAttribute, buffer, refHandle);

                default:
                    return -1;
            }
        }

        private unsafe static int QueryContextAttributes_SECURITY(
            SafeDeleteContext phContext,
            ContextAttribute contextAttribute,
            byte* buffer,
            SafeHandle refHandle)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            // We don't want to be interrupted by thread abort exceptions or unexpected out-of-memory errors failing to jit
            // one of the following methods. So run within a CER non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                phContext.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    phContext.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.QueryContextAttributesW(ref phContext._handle, contextAttribute, buffer);
                    phContext.DangerousRelease();
                }

                if (status == 0 && refHandle != null) {
                    if (refHandle is SafeFreeContextBuffer) {
                        ((SafeFreeContextBuffer)refHandle).Set(*(IntPtr*)buffer);
                    }
                    else {
                        ((SafeFreeCertContext)refHandle).Set(*(IntPtr*)buffer);
                    }
                }

                if (status != 0 && refHandle != null) {
                    refHandle.SetHandleAsInvalid();
                }
            }

            return status;
        }

        public static int SetContextAttributes(SecurDll dll, SafeDeleteContext phContext, 
            ContextAttribute contextAttribute, byte[] buffer)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return SetContextAttributes_SECURITY(phContext, contextAttribute, buffer);

                default:
                    return -1;
            }
        }

        private static int SetContextAttributes_SECURITY(
            SafeDeleteContext phContext,
            ContextAttribute contextAttribute,
            byte[] buffer)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            // We don't want to be interrupted by thread abort exceptions or unexpected out-of-memory errors failing 
            // to jit one of the following methods. So run within a CER non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                phContext.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    phContext.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.SetContextAttributesW(
                        ref phContext._handle, contextAttribute, buffer, buffer.Length);
                    phContext.DangerousRelease();
                }
            }

            return status;
        }
    }

    //=======================================================
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBuffer_SECURITY : SafeFreeContextBuffer {

        internal SafeFreeContextBuffer_SECURITY(): base() {}

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeContextBuffer(handle) == 0;
        }

    }
//=======================================================

#endif // !FEATURE_PAL

    ///////////////////////////////////////////////////////////////
    //
    // This is implementaion of Safe AllocHGlobal which is turned out
    // to be LocalAlloc down in CLR
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeLocalFree : DebugSafeHandle {
#else
    internal sealed class SafeLocalFree : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private const int LMEM_FIXED = 0;
        private const int NULL = 0;

        // This returned handle cannot be modified by the application.
        public static SafeLocalFree Zero = new SafeLocalFree(false);

        private SafeLocalFree() : base(true) {}

        private SafeLocalFree(bool ownsHandle) : base(ownsHandle) {}

        public static SafeLocalFree LocalAlloc(int cb) {
            SafeLocalFree result = UnsafeNclNativeMethods.SafeNetHandles.LocalAlloc(LMEM_FIXED, (UIntPtr) cb);
            if (result.IsInvalid) {
                result.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            return result;
        }

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles.LocalFree(handle) == IntPtr.Zero;
        }
    }

    ///////////////////////////////////////////////////////////////
    //
    // A few Win32 APIs return pointers to blobs that need GlobalFree().
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeGlobalFree : DebugSafeHandle {
#else
    internal sealed class SafeGlobalFree : SafeHandleZeroOrMinusOneIsInvalid
    {
#endif
        private SafeGlobalFree() : base(true) { }
        private SafeGlobalFree(bool ownsHandle) : base(ownsHandle) { }

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles.GlobalFree(handle) == IntPtr.Zero;
        }
    }

    [ComVisible(false)]
#if DEBUG
    internal sealed class SafeOverlappedFree : DebugSafeHandle {
#else
    internal sealed class SafeOverlappedFree : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private const int LPTR = 0x0040;

        internal static readonly SafeOverlappedFree Zero = new SafeOverlappedFree(false);

        private SafeCloseSocket             _socketHandle;

        private SafeOverlappedFree() : base(true) {}
        private SafeOverlappedFree(bool ownsHandle) : base(ownsHandle) {}

        public static SafeOverlappedFree Alloc() {
            SafeOverlappedFree result = UnsafeNclNativeMethods.SafeNetHandlesSafeOverlappedFree.LocalAlloc(LPTR, (UIntPtr) Win32.OverlappedSize);
            if (result.IsInvalid) {
                result.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            return result;
        }

        public static SafeOverlappedFree Alloc(SafeCloseSocket socketHandle) {
            SafeOverlappedFree result = Alloc();
            result._socketHandle = socketHandle;
            return result;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Close (bool resetOwner)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {} finally {
                if (resetOwner) {
                    _socketHandle = null;
                }
                Close();
            }
        }

        unsafe override protected bool ReleaseHandle()
        {
            SafeCloseSocket socketHandle = _socketHandle;
            if (socketHandle != null && !socketHandle.IsInvalid)
            {
                // We are being finalized while the I/O operation associated
                // with the current overlapped is still pending (e.g. on app
                // domain shutdown). The socket has to be closed first to
                // avoid reuse after delete of the native overlapped structure.
                socketHandle.Dispose();
            }
            // Release the native overlapped structure
            return UnsafeNclNativeMethods.SafeNetHandles.LocalFree(handle) == IntPtr.Zero;
        }
    }

#if !FEATURE_PAL
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeLoadLibrary : DebugSafeHandle {
#else
    internal sealed class SafeLoadLibrary : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private const string KERNEL32 = "kernel32.dll";

        public static readonly SafeLoadLibrary Zero = new SafeLoadLibrary(false);

        private SafeLoadLibrary() : base(true) {
        }

        private SafeLoadLibrary(bool ownsHandle) : base(ownsHandle) {
        }

        public unsafe static SafeLoadLibrary LoadLibraryEx(string library) {

            SafeLoadLibrary result = UnsafeNclNativeMethods.SafeNetHandles.LoadLibraryExW(library, null, 0);
            if (result.IsInvalid) {
                result.SetHandleAsInvalid();
            }
            return result;
        }

        public unsafe bool HasFunction(string functionName)
        {
            IntPtr ret = UnsafeNclNativeMethods.GetProcAddress(this, functionName);
            return (ret != IntPtr.Zero);
        }

        protected override bool ReleaseHandle() {
            return UnsafeNclNativeMethods.SafeNetHandles.FreeLibrary(handle);
        }

    }

    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles that require CertFreeCertificateChain
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCertChain : SafeHandleZeroOrMinusOneIsInvalid {

        // This ctor will create a handle that we >>don't<< own
        internal SafeFreeCertChain(IntPtr handle) : base(false)
        {
            SetHandle(handle);
        }

        internal SafeFreeCertChain(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        public override string ToString()
        {
            return "0x" + DangerousGetHandle().ToString("x");
        }

        protected override bool ReleaseHandle()
        {
            UnsafeNclNativeMethods.SafeNetHandles.CertFreeCertificateChain(handle);
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles that require CertFreeCertificateChainList
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCertChainList : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFreeCertChainList() : base(true) { }

        public override string ToString()
        {
            return "0x" + DangerousGetHandle().ToString("x");
        }

        protected override bool ReleaseHandle()
        {
            // 












            UnsafeNclNativeMethods.SafeNetHandles.CertFreeCertificateChainList(handle);
            return true;
        }
    }


    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles to encapsulate PCCERT_SELECT_CRITERIA
    // structure and are used in calling CertSelectCertificateChains
    //
    ///////////////////////////////////////////////////////////////
    internal sealed class SafeCertSelectCritera : SafeHandleZeroOrMinusOneIsInvalid
    {
        // from wincrypt.h
        private const string szOID_PKIX_KP_CLIENT_AUTH = "1.3.6.1.5.5.7.3.2";
        private const int CERT_SELECT_BY_ENHKEY_USAGE = 1;
        private const int CERT_SELECT_BY_KEY_USAGE = 2;
        private const byte CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x80;

        // Initial criteria to filter list of certificates.  This is is the algorithm used by WinInet.
        //
        // 1. No Key Usage extension is present or is present and contains the Digital Signature bit.
        // 2. No Extended Key Usage extension is present or is present and contains the Client Auth OID.      
        private const int criteriaCount = 2;
        private List<IntPtr> unmanagedMemoryList;

        internal int Count
        {
            get { return criteriaCount; }
        }

        private IntPtr AllocBuffer(int size)
        {
            IntPtr ptr = Marshal.AllocHGlobal(size);
            unmanagedMemoryList.Add(ptr);
            return ptr;
        }

        private IntPtr AllocString(string str)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
            unmanagedMemoryList.Add(ptr);
            return ptr;
        }

        internal SafeCertSelectCritera()
            : base(true)
        {
            IntPtr criteriaArray;
            IntPtr paraArray;
            UnsafeNclNativeMethods.NativePKI.CERT_SELECT_CRITERIA criteria = 
                new UnsafeNclNativeMethods.NativePKI.CERT_SELECT_CRITERIA();

            // Keep track of unmanaged memory blocks that need to be freed
            unmanagedMemoryList = new List<IntPtr>();

            // Array of criteria
            criteriaArray = AllocBuffer(criteriaCount * Marshal.SizeOf(criteria));
            SetHandle(criteriaArray);
            
            // 1. Enhanced Key Usage criteria
            criteria.dwType = CERT_SELECT_BY_ENHKEY_USAGE;
            criteria.cPara = 1;
            
            IntPtr oidString = AllocString(szOID_PKIX_KP_CLIENT_AUTH);
            paraArray = AllocBuffer(Marshal.SizeOf(oidString));
            Marshal.WriteIntPtr(paraArray, oidString);
            criteria.ppPara = paraArray;

            // criteria contains only blittable fields so DestroyStructure doesn't have to be called.
            Marshal.StructureToPtr(criteria, criteriaArray, false);


            // 2. Key Usage criteria
            criteria = new UnsafeNclNativeMethods.NativePKI.CERT_SELECT_CRITERIA();
            criteria.dwType = CERT_SELECT_BY_KEY_USAGE;
            criteria.cPara = 1;

            UnsafeNclNativeMethods.NativePKI.CERT_EXTENSION certExtension = 
                new UnsafeNclNativeMethods.NativePKI.CERT_EXTENSION();
            
            certExtension.pszObjId = IntPtr.Zero;
            certExtension.fCritical = 0; // FALSE
            certExtension.Value.cbData = 1;
            
            IntPtr keyUsageCriteria = AllocBuffer(Marshal.SizeOf(CERT_DIGITAL_SIGNATURE_KEY_USAGE));
            Marshal.WriteByte(keyUsageCriteria, CERT_DIGITAL_SIGNATURE_KEY_USAGE);
            certExtension.Value.pbData = keyUsageCriteria;

            IntPtr pCertExtension = AllocBuffer(Marshal.SizeOf(certExtension));
            // certExtension only blittable fields so DestroyStructure doesn't have to be called.
            Marshal.StructureToPtr(certExtension, pCertExtension, false);
                        
            paraArray = AllocBuffer(Marshal.SizeOf(pCertExtension));
            Marshal.WriteIntPtr(paraArray, pCertExtension);
            criteria.ppPara = paraArray;
            
            // criteria contains only blittable fields so DestroyStructure doesn't have to be called.
            Marshal.StructureToPtr(criteria, criteriaArray + Marshal.SizeOf(criteria), false);
        }

        public override string ToString()
        {
            return "0x" + DangerousGetHandle().ToString("x");
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                foreach (IntPtr ptr in unmanagedMemoryList)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }


    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles required CertFreeCertificateContext
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal sealed class SafeFreeCertContext : DebugSafeHandle {
#else
    internal sealed class SafeFreeCertContext : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private const string CRYPT32 = "crypt32.dll";
        private const string ADVAPI32 = "advapi32.dll";

        internal SafeFreeCertContext() : base(true) {}

        // This must be ONLY called from this file within a CER.
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void Set(IntPtr value) {
            this.handle = value;
        }

        const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x00000040;

        override protected bool ReleaseHandle() {
            UnsafeNclNativeMethods.SafeNetHandles.CertFreeCertificateContext(handle);
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct SSPIHandle {
        private IntPtr HandleHi;
        private IntPtr HandleLo;

        public bool IsZero {
            get {return HandleHi == IntPtr.Zero && HandleLo == IntPtr.Zero;}

        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetToInvalid() {
            HandleHi = IntPtr.Zero;
            HandleLo = IntPtr.Zero;
        }

        public override string ToString() {
            {   return HandleHi.ToString("x") + ":" + HandleLo.ToString("x");}
        }
    }

    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles dependable on FreeCredentialsHandle
    //
    //
    ///////////////////////////////////////////////////////////////
    //------------------------------------------------------------
#if DEBUG
    internal abstract class SafeFreeCredentials : DebugSafeHandle {
#else
    internal abstract class SafeFreeCredentials : SafeHandle {
#endif

        internal SSPIHandle _handle;    //should be always used as by ref in PINvokes parameters

        protected SafeFreeCredentials(): base(IntPtr.Zero, true) {
            _handle = new SSPIHandle();
        }

#if TRAVE
        public override string ToString() {
            return "0x"+_handle.ToString();
        }
#endif

        public override bool IsInvalid
        {
            get {return IsClosed || _handle.IsZero;}
        }

#if DEBUG
        //This method should never be called for this type
        public new IntPtr DangerousGetHandle()
        {
            throw new InvalidOperationException();
        }
#endif

        public unsafe static int AcquireCredentialsHandle( SecurDll dll,
                                                    string package,
                                                    CredentialUse intent,
                                                    ref AuthIdentity authdata,
                                                    out SafeFreeCredentials outCredential
                                                    )
        {

            GlobalLog.Print("SafeFreeCredentials::AcquireCredentialsHandle#1("
                            + dll + ","
                            + package + ", "
                            + intent + ", "
                            + authdata + ")"
                            );

            int errorCode = -1;
            long timeStamp;

            switch (dll) {
            case SecurDll.SECURITY:
                        outCredential = new SafeFreeCredential_SECURITY();

                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {} finally {

                            errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(
                                                                   null,
                                                                   package,
                                                                   (int)intent,
                                                                   null,
                                                                   ref authdata,
                                                                   null,
                                                                   null,
                                                                   ref outCredential._handle,
                                                                   out timeStamp
                                                                   );
                        }
                        break;

            default:  throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
            }
#if TRAVE
            GlobalLog.Print("Unmanaged::AcquireCredentialsHandle() returns 0x"
                            + String.Format("{0:x}", errorCode)
                            + ", handle = " + outCredential.ToString()
                            );
#endif

            if (errorCode != 0) {
                outCredential.SetHandleAsInvalid();
            }
            return errorCode;

        }

        public unsafe static int AcquireDefaultCredential( SecurDll dll,
                                                    string package,
                                                    CredentialUse intent,
                                                    out SafeFreeCredentials outCredential
                                                    )
        {

            GlobalLog.Print("SafeFreeCredentials::AcquireDefaultCredential("
                            + dll + ","
                            + package + ", "
                            + intent + ")"
                            );

            int errorCode = -1;
            long timeStamp;

            switch (dll) {
            case SecurDll.SECURITY:
                        outCredential = new SafeFreeCredential_SECURITY();

                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {} finally {

                            errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(
                                                               null,
                                                               package,
                                                               (int)intent,
                                                               null,
                                                               IntPtr.Zero,
                                                               null,
                                                               null,
                                                               ref outCredential._handle,
                                                               out timeStamp
                                                               );
                        }
                        break;

            default:  throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
            }

#if TRAVE
            GlobalLog.Print("Unmanaged::AcquireCredentialsHandle() returns 0x"
                            + errorCode.ToString("x")
                            + ", handle = " + outCredential.ToString()
                            );
#endif

            if (errorCode != 0) {
                outCredential.SetHandleAsInvalid();
            }
            return errorCode;

        }
        
        // This overload is only called on Win7+ where SspiEncodeStringsAsAuthIdentity() was used to
        // create the authData blob.
        public unsafe static int AcquireCredentialsHandle(
                                                    string package,
                                                    CredentialUse intent,
                                                    ref SafeSspiAuthDataHandle authdata,
                                                    out SafeFreeCredentials outCredential
                                                    )
        {
            int errorCode = -1;
            long timeStamp;

            outCredential = new SafeFreeCredential_SECURITY();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally 
            {
                errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(
                                                       null,
                                                       package,
                                                       (int)intent,
                                                       null,
                                                       authdata,
                                                       null,
                                                       null,
                                                       ref outCredential._handle,
                                                       out timeStamp
                                                       );
            }

            if (errorCode != 0)
            {
                outCredential.SetHandleAsInvalid();
            }
            return errorCode;
        }

        public unsafe static int AcquireCredentialsHandle( SecurDll dll,
                                                    string package,
                                                    CredentialUse intent,
                                                    ref SecureCredential authdata,
                                                    out SafeFreeCredentials outCredential
                                                    )
        {

            GlobalLog.Print("SafeFreeCredentials::AcquireCredentialsHandle#2("
                            + dll + ","
                            + package + ", "
                            + intent + ", "
                            + authdata + ")"
                            );

            int errorCode = -1;
            long timeStamp;


            // If there is a certificate, wrap it into an array.
            // Not threadsafe.
            IntPtr copiedPtr = authdata.certContextArray;
            try
            {
                IntPtr certArrayPtr = new IntPtr(&copiedPtr);
                if (copiedPtr != IntPtr.Zero) {
                    authdata.certContextArray = certArrayPtr;
                }

                switch (dll) {
                case SecurDll.SECURITY:
                            outCredential = new SafeFreeCredential_SECURITY();

                            RuntimeHelpers.PrepareConstrainedRegions();
                            try {} finally {

                                errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(
                                                                   null,
                                                                   package,
                                                                   (int)intent,
                                                                   null,
                                                                   ref authdata,
                                                                   null,
                                                                   null,
                                                                   ref outCredential._handle,
                                                                   out timeStamp
                                                                   );
                            }
                            break;

                default:  throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
                }
            }
            finally
            {
                authdata.certContextArray = copiedPtr;
            }

#if TRAVE
            GlobalLog.Print("Unmanaged::AcquireCredentialsHandle() returns 0x"
                            + errorCode.ToString("x")
                            + ", handle = " + outCredential.ToString()
                            );
#endif

            if (errorCode != 0) {
                outCredential.SetHandleAsInvalid();
            }
            return errorCode;

        }

    }
    //
    // This is a class holding a Credential handle reference, used for static handles cache
    //
#if DEBUG
    internal sealed class SafeCredentialReference: DebugCriticalHandleMinusOneIsInvalid {
#else
    internal sealed class SafeCredentialReference: CriticalHandleMinusOneIsInvalid {
#endif

        //
        // Static cache will return the target handle if found the reference in the table.
        //
        internal SafeFreeCredentials _Target;

        //
        //
        internal static SafeCredentialReference CreateReference(SafeFreeCredentials target)
        {
            SafeCredentialReference result = new SafeCredentialReference(target);
            if (result.IsInvalid)
                return null;

            return result;
        }
        private SafeCredentialReference(SafeFreeCredentials target): base()
        {
            // Bumps up the refcount on Target to signify that target handle is statically cached so
            // its dispose should be postponed
            bool b = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target.DangerousAddRef(ref b);
            }
            catch
            {
                if (b)
                {
                    target.DangerousRelease();
                    b = false;
                }
            }
            finally
            {
                if (b)
                {
                    _Target = target;
                    SetHandle(new IntPtr(0));   // make this handle valid
                }
            }
        }

        override protected bool ReleaseHandle()
        {
            SafeFreeCredentials target = _Target;
            if (target != null)
                target.DangerousRelease();
            _Target = null;
            return true;
        }
    }

//======================================================================
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCredential_SECURITY: SafeFreeCredentials {

        public SafeFreeCredential_SECURITY() : base() {}

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeCredentialsHandle(ref _handle) == 0;
        }

    }
//======================================================================

    ///////////////////////////////////////////////////////////////
    //
    // Implementation of handles that are dependent on DeleteSecurityContext
    //
    //
    ///////////////////////////////////////////////////////////////
#if DEBUG
    internal abstract class SafeDeleteContext : DebugSafeHandle {
#else
    internal abstract class SafeDeleteContext : SafeHandle {
#endif
        private const           string dummyStr = " ";
        private static readonly byte[] dummyBytes = new byte[] {0};

        //
        // ATN: _handle is internal since it is used on PInvokes by other wrapper methods.
        //      However all such wrappers MUST manually and reliably adjust refCounter of SafeDeleteContext handle.
        //
        internal SSPIHandle             _handle;

        protected SafeFreeCredentials _EffectiveCredential;

        protected SafeDeleteContext(): base(IntPtr.Zero, true) {
            _handle = new SSPIHandle();
        }

        public override bool IsInvalid {
            get {
                return IsClosed || _handle.IsZero;
            }
        }

        public override string ToString() {
            return _handle.ToString();
        }

#if DEBUG
        //This method should never be called for this type
        public new IntPtr DangerousGetHandle()
        {
            throw new InvalidOperationException();
        }
#endif

        //-------------------------------------------------------------------
        internal unsafe static int InitializeSecurityContext(
                                                    SecurDll                dll,
                                                    ref SafeFreeCredentials inCredentials,
                                                    ref SafeDeleteContext   refContext,
                                                    string                  targetName,
                                                    ContextFlags            inFlags,
                                                    Endianness              endianness,
                                                    SecurityBuffer          inSecBuffer,
                                                    SecurityBuffer[]        inSecBuffers,
                                                    SecurityBuffer          outSecBuffer,
                                                    ref ContextFlags        outFlags)
        {

#if TRAVE
            GlobalLog.Enter("SafeDeleteContext::InitializeSecurityContext");
            GlobalLog.Print("    DLL              = " + dll);
            GlobalLog.Print("    credential       = " + inCredentials.ToString());
            GlobalLog.Print("    refContext       = " + ValidationHelper.ToString(refContext));
            GlobalLog.Print("    targetName       = " + targetName);
            GlobalLog.Print("    inFlags          = " + inFlags);
//            GlobalLog.Print("    reservedI        = 0x0");
//            GlobalLog.Print("    endianness       = " + endianness);

            if (inSecBuffers==null)
            {
                GlobalLog.Print("    inSecBuffers     = (null)");
            }
            else
            {
                GlobalLog.Print("    inSecBuffers[]   = length:" + inSecBuffers.Length);
//                for (int index=0; index<inSecBuffers.Length; index++) { GlobalLog.Print("    inSecBuffers[" + index + "]   = " + SecurityBuffer.ToString(inSecBuffers[index])); }
            }
//            GlobalLog.Print("    reservedII       = 0x0");
//            GlobalLog.Print("    newContext       = {ref} inContext");
//            GlobalLog.Print("    outSecBuffer     = " + SecurityBuffer.ToString(outSecBuffer));
//            GlobalLog.Print("    outFlags         = {ref} " + outFlags);
//            GlobalLog.Print("    timestamp        = null");
#endif
            GlobalLog.Assert(outSecBuffer != null, "SafeDeleteContext::InitializeSecurityContext()|outSecBuffer != null");
            GlobalLog.Assert(inSecBuffer == null || inSecBuffers == null, "SafeDeleteContext::InitializeSecurityContext()|inSecBuffer == null || inSecBuffers == null");

            if (inCredentials == null)
            {
                throw new ArgumentNullException("inCredentials");
            }

            SecurityBufferDescriptor inSecurityBufferDescriptor = null;
            if (inSecBuffer!=null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers!=null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outSecurityBufferDescriptor = new SecurityBufferDescriptor(1);

            // actually this is returned in outFlags
            bool isSspiAllocated = (inFlags & ContextFlags.AllocateMemory) != 0 ? true : false;

            int errorCode = -1;

            SSPIHandle contextHandle = new SSPIHandle();
            if (refContext != null)
                contextHandle = refContext._handle;

            // these are pinned user byte arrays passed along with SecurityBuffers
            GCHandle[] pinnedInBytes = null;
            GCHandle pinnedOutBytes = new GCHandle();
            // optional output buffer that may need to be freed
            SafeFreeContextBuffer outFreeContextBuffer = null;
            try
            {
                pinnedOutBytes = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
                SecurityBufferStruct[] inUnmanagedBuffer = new SecurityBufferStruct[inSecurityBufferDescriptor==null ? 1: inSecurityBufferDescriptor.Count];
                fixed (void* inUnmanagedBufferPtr = inUnmanagedBuffer)
                {
                    if (inSecurityBufferDescriptor!=null)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        inSecurityBufferDescriptor.UnmanagedPointer = inUnmanagedBufferPtr;
                        pinnedInBytes = new GCHandle[inSecurityBufferDescriptor.Count];
                        SecurityBuffer securityBuffer;
                        for (int index = 0; index < inSecurityBufferDescriptor.Count; ++index)
                        {
                            securityBuffer = inSecBuffer!=null ? inSecBuffer : inSecBuffers[index];
                            if (securityBuffer!=null)
                            {
                                // Copy the SecurityBuffer content into unmanaged place holder
                                inUnmanagedBuffer[index].count = securityBuffer.size;
                                inUnmanagedBuffer[index].type  = securityBuffer.type;

                                // use the unmanaged token if it's not null; otherwise use the managed buffer
                                if (securityBuffer.unmanagedToken != null)
                                {
                                    inUnmanagedBuffer[index].token = securityBuffer.unmanagedToken.DangerousGetHandle();
                                }
                                else if (securityBuffer.token == null || securityBuffer.token.Length == 0)
                                {
                                    inUnmanagedBuffer[index].token = IntPtr.Zero;
                                }
                                else
                                {
                                    pinnedInBytes[index] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
                                    inUnmanagedBuffer[index].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
                                }
    #if TRAVE
                                GlobalLog.Print("SecBuffer: cbBuffer:" + securityBuffer.size +  " BufferType:" + securityBuffer.type);
    #endif
                            }
                        }
                    }

                    SecurityBufferStruct[] outUnmanagedBuffer = new SecurityBufferStruct[1];
                    fixed (void* outUnmanagedBufferPtr = outUnmanagedBuffer)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        outSecurityBufferDescriptor.UnmanagedPointer = outUnmanagedBufferPtr;
                        outUnmanagedBuffer[0].count = outSecBuffer.size;
                        outUnmanagedBuffer[0].type  = outSecBuffer.type;
                        if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
                            outUnmanagedBuffer[0].token = IntPtr.Zero;
                        else
                            outUnmanagedBuffer[0].token = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
    #if TRAVE
    //                    outSecBuffer.DebugDump();
    #endif
                        if (isSspiAllocated)
                            outFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle(dll);

                        switch (dll)
                        {
                        case SecurDll.SECURITY:
                                    if (refContext == null || refContext.IsInvalid)
                                        refContext = new SafeDeleteContext_SECURITY();

                                    if (targetName == null || targetName.Length == 0)
                                        targetName = dummyStr;

                                    fixed (char* namePtr = targetName)
                                    {
                                        errorCode = MustRunInitializeSecurityContext_SECURITY(
                                                        ref inCredentials,
                                                        contextHandle.IsZero? null: &contextHandle,
                                                        (byte*)(((object)targetName == (object) dummyStr)? null: namePtr),
                                                        inFlags,
                                                        endianness,
                                                        inSecurityBufferDescriptor,
                                                        refContext,
                                                        outSecurityBufferDescriptor,
                                                        ref outFlags,
                                                        outFreeContextBuffer
                                                        );
                                    }
                                    break;

                        default:  throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
                        }

                        GlobalLog.Print("SafeDeleteContext:InitializeSecurityContext  Marshalling OUT buffer");
                        // Get unmanaged buffer with index 0 as the only one passed into PInvoke
                        outSecBuffer.size = outUnmanagedBuffer[0].count;
                        outSecBuffer.type = outUnmanagedBuffer[0].type;
                        if (outSecBuffer.size > 0)
                        {
                            outSecBuffer.token = new byte[outSecBuffer.size];
                            Marshal.Copy(outUnmanagedBuffer[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                        }
                        else
                        {
                            outSecBuffer.token = null;
                        }
                    }
                }
            }
            finally {
                if (pinnedInBytes!=null)
                {
                    for (int index=0; index<pinnedInBytes.Length; index++)
                    {
                        if (pinnedInBytes[index].IsAllocated)
                            pinnedInBytes[index].Free();
                    }
                }
                if (pinnedOutBytes.IsAllocated)
                    pinnedOutBytes.Free();

                if (outFreeContextBuffer != null)
                    outFreeContextBuffer.Close();
            }

            GlobalLog.Leave("SafeDeleteContext::InitializeSecurityContext() unmanaged InitializeSecurityContext()", "errorCode:0x" + errorCode.ToString("x8") + " refContext:" + ValidationHelper.ToString(refContext));

            return errorCode;
        }

        //
        // After PINvoke call the method will fix the handleTemplate.handle with the returned value.
        // The caller is responsible for creating a correct SafeFreeContextBuffer_XXX flavour or null can be passed if no handle is returned.
        //
        // Since it has a CER, this method can't have any references to imports from DLLs that may not exist on the system.
        //
        private static unsafe int MustRunInitializeSecurityContext_SECURITY(
                                                  ref SafeFreeCredentials inCredentials,
                                                  void*            inContextPtr,
                                                  byte*            targetName,
                                                  ContextFlags     inFlags,
                                                  Endianness       endianness,
                                                  SecurityBufferDescriptor inputBuffer,
                                                  SafeDeleteContext outContext,
                                                  SecurityBufferDescriptor outputBuffer,
                                                  ref ContextFlags attributes,
                                                  SafeFreeContextBuffer handleTemplate)
        {
            int errorCode = (int) SecurityStatus.InvalidHandle;
            bool b1 = false;
            bool b2 = false;

            // Run the body of this method as a non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                inCredentials.DangerousAddRef(ref b1);
                outContext.DangerousAddRef(ref b2);
            }
            catch(Exception e)
            {
                if (b1)
                {
                    inCredentials.DangerousRelease();
                    b1 = false;
                }
                if (b2)
                {
                    outContext.DangerousRelease();
                    b2 = false;
                }

                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally
            {
                SSPIHandle credentialHandle = inCredentials._handle;
                long timeStamp;

                if (!b1)
                {
                    // caller should retry
                    inCredentials = null;
                }
                else if (b1 && b2)
                {
                    errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.InitializeSecurityContextW(
                                ref credentialHandle,
                                inContextPtr,
                                targetName,
                                inFlags,
                                0,
                                endianness,
                                inputBuffer,
                                0,
                                ref outContext._handle,
                                outputBuffer,
                                ref attributes,
                                out timeStamp);

                    //
                    // When a credential handle is first associated with the context we keep credential
                    // ref count bumped up to ensure ordered finalization.
                    // If the credential handle has been changed we de-ref the old one and associate the
                    //  context with the new cred handle but only if the call was successful.
                    if (outContext._EffectiveCredential != inCredentials && (errorCode & 0x80000000) == 0)
                    {
                        // Disassociate the previous credential handle
                        if (outContext._EffectiveCredential != null)
                            outContext._EffectiveCredential.DangerousRelease();
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }

                    outContext.DangerousRelease();

                    // The idea is that SSPI has allocated a block and filled up outUnmanagedBuffer+8 slot with the pointer.
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(((SecurityBufferStruct*)outputBuffer.UnmanagedPointer)->token); //ATTN: on 64 BIT that is still +8 cause of 2* c++ unsigned long == 8 bytes
                        if (handleTemplate.IsInvalid)
                            handleTemplate.SetHandleAsInvalid();
                    }
                }


                if (inContextPtr == null && (errorCode & 0x80000000) != 0)
                {
                    // an error on the first call, need to set the out handle to invalid value
                    outContext._handle.SetToInvalid();
                }
            }

            return errorCode;
        }

        //-------------------------------------------------------------------
        internal unsafe static int AcceptSecurityContext(
            SecurDll                dll,
            ref SafeFreeCredentials inCredentials,
            ref SafeDeleteContext   refContext,
            ContextFlags            inFlags,
            Endianness              endianness,
            SecurityBuffer          inSecBuffer,
            SecurityBuffer[]        inSecBuffers,
            SecurityBuffer          outSecBuffer,
            ref ContextFlags        outFlags) {

#if TRAVE
            GlobalLog.Enter("SafeDeleteContext::AcceptSecurityContex");
            GlobalLog.Print("    DLL              = " + dll);
            GlobalLog.Print("    credential       = " + inCredentials.ToString());
            GlobalLog.Print("    refContext       = " + ValidationHelper.ToString(refContext));

            GlobalLog.Print("    inFlags          = " + inFlags);
//            GlobalLog.Print("    endianness       = " + endianness);
//            GlobalLog.Print("    inSecBuffer      = " + SecurityBuffer.ToString(inSecBuffer));
//
            if (inSecBuffers==null)
            {
                GlobalLog.Print("    inSecBuffers     = (null)");
            }
            else
            {
                GlobalLog.Print("    inSecBuffers[]   = length:" + inSecBuffers.Length);
//                for (int index=0; index<inSecBuffers.Length; index++) { GlobalLog.Print("    inSecBuffers[" + index + "]   = " + SecurityBuffer.ToString(inSecBuffers[index])); }
            }
//            GlobalLog.Print("    newContext       = {ref} inContext");
//            GlobalLog.Print("    outSecBuffer     = " + SecurityBuffer.ToString(outSecBuffer));
//            GlobalLog.Print("    outFlags         = {ref} " + outFlags);
//            GlobalLog.Print("    timestamp        = null");
#endif
            GlobalLog.Assert(outSecBuffer != null, "SafeDeleteContext::AcceptSecurityContext()|outSecBuffer != null");
            GlobalLog.Assert(inSecBuffer == null || inSecBuffers == null, "SafeDeleteContext::AcceptSecurityContext()|inSecBuffer == null || inSecBuffers == null");

            if (inCredentials == null)
            {
                throw new ArgumentNullException("inCredentials");
            }

            SecurityBufferDescriptor inSecurityBufferDescriptor = null;
            if (inSecBuffer!=null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers!=null)
            {
                inSecurityBufferDescriptor = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outSecurityBufferDescriptor = new SecurityBufferDescriptor(1);

            // actually this is returned in outFlags
            bool isSspiAllocated = (inFlags & ContextFlags.AllocateMemory) != 0 ? true : false;

            int errorCode = -1;

            SSPIHandle contextHandle = new SSPIHandle();
            if (refContext != null)
                contextHandle = refContext._handle;

            // these are pinned user byte arrays passed along with SecurityBuffers
            GCHandle[] pinnedInBytes = null;
            GCHandle pinnedOutBytes = new GCHandle();
            // optional output buffer that may need to be freed
            SafeFreeContextBuffer outFreeContextBuffer = null;
            try
            {
                pinnedOutBytes = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
                SecurityBufferStruct[] inUnmanagedBuffer = new SecurityBufferStruct[inSecurityBufferDescriptor==null ? 1:inSecurityBufferDescriptor.Count];
                fixed (void* inUnmanagedBufferPtr = inUnmanagedBuffer)
                {
                    if (inSecurityBufferDescriptor!=null)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        inSecurityBufferDescriptor.UnmanagedPointer = inUnmanagedBufferPtr;
                        pinnedInBytes = new GCHandle[inSecurityBufferDescriptor.Count];
                        SecurityBuffer securityBuffer;
                        for (int index = 0; index < inSecurityBufferDescriptor.Count; ++index)
                        {
                            securityBuffer = inSecBuffer!=null ? inSecBuffer : inSecBuffers[index];
                            if (securityBuffer!=null)
                            {
                                // Copy the SecurityBuffer content into unmanaged place holder
                                inUnmanagedBuffer[index].count = securityBuffer.size;
                                inUnmanagedBuffer[index].type  = securityBuffer.type;

                                // use the unmanaged token if it's not null; otherwise use the managed buffer
                                if (securityBuffer.unmanagedToken != null)
                                {
                                    inUnmanagedBuffer[index].token = securityBuffer.unmanagedToken.DangerousGetHandle();
                                }
                                else if (securityBuffer.token == null || securityBuffer.token.Length == 0)
                                {
                                    inUnmanagedBuffer[index].token = IntPtr.Zero;
                                }
                                else
                                {
                                    pinnedInBytes[index] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
                                    inUnmanagedBuffer[index].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
                                }
    #if TRAVE
                                GlobalLog.Print("SecBuffer: cbBuffer:" + securityBuffer.size +  " BufferType:" + securityBuffer.type);
    #endif
                            }
                        }
                    }
                    SecurityBufferStruct[] outUnmanagedBuffer = new SecurityBufferStruct[1];
                    fixed (void* outUnmanagedBufferPtr = outUnmanagedBuffer)
                    {
                        // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                        outSecurityBufferDescriptor.UnmanagedPointer = outUnmanagedBufferPtr;
                        // Copy the SecurityBuffer content into unmanaged place holder
                        outUnmanagedBuffer[0].count = outSecBuffer.size;
                        outUnmanagedBuffer[0].type  = outSecBuffer.type;

                        if (outSecBuffer.token == null || outSecBuffer.token.Length == 0)
                            outUnmanagedBuffer[0].token  = IntPtr.Zero;
                        else
                            outUnmanagedBuffer[0].token  = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);

                        if (isSspiAllocated)
                            outFreeContextBuffer = SafeFreeContextBuffer.CreateEmptyHandle(dll);

                        switch (dll)
                        {
                        case SecurDll.SECURITY:
                                    if (refContext == null || refContext.IsInvalid)
                                        refContext = new SafeDeleteContext_SECURITY();

                                    errorCode = MustRunAcceptSecurityContext_SECURITY(
                                                    ref inCredentials,
                                                    contextHandle.IsZero? null: &contextHandle,
                                                    inSecurityBufferDescriptor,
                                                    inFlags,
                                                    endianness,
                                                    refContext,
                                                    outSecurityBufferDescriptor,
                                                    ref outFlags,
                                                    outFreeContextBuffer
                                                    );

                                    break;

                        default:  throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
                        }

                        GlobalLog.Print("SafeDeleteContext:AcceptSecurityContext  Marshalling OUT buffer");
                        // Get unmanaged buffer with index 0 as the only one passed into PInvoke
                        outSecBuffer.size = outUnmanagedBuffer[0].count;
                        outSecBuffer.type = outUnmanagedBuffer[0].type;
                        if (outSecBuffer.size > 0)
                        {
                            outSecBuffer.token = new byte[outSecBuffer.size];
                            Marshal.Copy(outUnmanagedBuffer[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                        }
                        else
                        {
                            outSecBuffer.token = null;
                        }
                    }
                }
            }
            finally {
                if (pinnedInBytes!=null)
                {
                    for (int index=0; index<pinnedInBytes.Length; index++)
                    {
                        if (pinnedInBytes[index].IsAllocated)
                            pinnedInBytes[index].Free();
                    }
                }

                if (pinnedOutBytes.IsAllocated)
                    pinnedOutBytes.Free();

                if (outFreeContextBuffer != null)
                    outFreeContextBuffer.Close();
            }

            GlobalLog.Leave("SafeDeleteContext::AcceptSecurityContex() unmanaged AcceptSecurityContex()", "errorCode:0x" + errorCode.ToString("x8") + " refContext:" + ValidationHelper.ToString(refContext));

            return errorCode;
        }

        //
        // After PINvoke call the method will fix the handleTemplate.handle with the returned value.
        // The caller is responsible for creating a correct SafeFreeContextBuffer_XXX flavour or null can be passed if no handle is returned.
        //
        // Since it has a CER, this method can't have any references to imports from DLLs that may not exist on the system.
        //
        private static unsafe int MustRunAcceptSecurityContext_SECURITY(
                                                  ref SafeFreeCredentials     inCredentials,
                                                  void*            inContextPtr,
                                                  SecurityBufferDescriptor inputBuffer,
                                                  ContextFlags     inFlags,
                                                  Endianness       endianness,
                                                  SafeDeleteContext outContext,
                                                  SecurityBufferDescriptor outputBuffer,
                                                  ref ContextFlags outFlags,
                                                  SafeFreeContextBuffer handleTemplate)
        {
            int errorCode = (int) SecurityStatus.InvalidHandle;
            bool b1 = false;
            bool b2 = false;

            // Run the body of this method as a non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                inCredentials.DangerousAddRef(ref b1);
                outContext.DangerousAddRef(ref b2);
            }
            catch(Exception e)
            {
                if (b1)
                {
                    inCredentials.DangerousRelease();
                    b1 = false;
                }
                if (b2)
                {
                    outContext.DangerousRelease();
                    b2 = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {
                SSPIHandle credentialHandle = inCredentials._handle;
                long timeStamp;

                if (!b1)
                {
                    // caller should retry
                    inCredentials = null;
                }
                else if (b1 && b2)
                {
                    errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcceptSecurityContext(
                                ref credentialHandle,
                                inContextPtr,
                                inputBuffer,
                                inFlags,
                                endianness,
                                ref outContext._handle,
                                outputBuffer,
                                ref outFlags,
                                out timeStamp);

                    //
                    // When a credential handle is first associated with the context we keep credential
                    // ref count bumped up to ensure ordered finalization.
                    // If the credential handle has been changed we de-ref the old one and associate the
                    //  context with the new cred handle but only if the call was successful.
                    if (outContext._EffectiveCredential != inCredentials && (errorCode & 0x80000000) == 0)
                    {
                        // Disassociate the previous credential handle
                        if (outContext._EffectiveCredential != null)
                            outContext._EffectiveCredential.DangerousRelease();
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }

                    outContext.DangerousRelease();

                    // The idea is that SSPI has allocated a block and filled up outUnmanagedBuffer+8 slot with the pointer.
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(((SecurityBufferStruct*)outputBuffer.UnmanagedPointer)->token); //ATTN: on 64 BIT that is still +8 cause of 2* c++ unsigned long == 8 bytes
                        if (handleTemplate.IsInvalid)
                        {
                            handleTemplate.SetHandleAsInvalid();
                        }
                    }
                }

                if (inContextPtr == null && (errorCode & 0x80000000) != 0)
                {
                    // an error on the first call, need to set the out handle to invalid value
                    outContext._handle.SetToInvalid();
                }
            }

            return errorCode;
        }

        //
        //
        //
        internal unsafe static int CompleteAuthToken(
            SecurDll                dll,
            ref SafeDeleteContext   refContext,
            SecurityBuffer[]        inSecBuffers) {

            GlobalLog.Enter("SafeDeleteContext::CompleteAuthToken");
            GlobalLog.Print("    DLL              = " + dll);
            GlobalLog.Print("    refContext       = " + ValidationHelper.ToString(refContext));
#if TRAVE
            GlobalLog.Print("    inSecBuffers[]   = length:" + inSecBuffers.Length);
//            for (int index=0; index<inSecBuffers.Length; index++) { GlobalLog.Print("    inSecBuffers[" + index + "]   = " + SecurityBuffer.ToString(inSecBuffers[index])); }
#endif
            GlobalLog.Assert(inSecBuffers != null, "SafeDeleteContext::CompleteAuthToken()|inSecBuffers == null");
            SecurityBufferDescriptor inSecurityBufferDescriptor = new SecurityBufferDescriptor(inSecBuffers.Length);

            int errorCode = (int)SecurityStatus.InvalidHandle;

            // these are pinned user byte arrays passed along with SecurityBuffers
            GCHandle[] pinnedInBytes = null;

            SecurityBufferStruct[] inUnmanagedBuffer = new SecurityBufferStruct[inSecurityBufferDescriptor.Count];
            fixed (void* inUnmanagedBufferPtr = inUnmanagedBuffer) {
                // Fix Descriptor pointer that points to unmanaged SecurityBuffers
                inSecurityBufferDescriptor.UnmanagedPointer = inUnmanagedBufferPtr;
                pinnedInBytes = new GCHandle[inSecurityBufferDescriptor.Count];
                SecurityBuffer securityBuffer;
                for (int index = 0; index < inSecurityBufferDescriptor.Count; ++index) {
                    securityBuffer = inSecBuffers[index];
                    if (securityBuffer!=null) {
                        inUnmanagedBuffer[index].count = securityBuffer.size;
                        inUnmanagedBuffer[index].type  = securityBuffer.type;

                        // use the unmanaged token if it's not null; otherwise use the managed buffer
                        if (securityBuffer.unmanagedToken != null)
                        {
                            inUnmanagedBuffer[index].token = securityBuffer.unmanagedToken.DangerousGetHandle();
                        }
                        else if (securityBuffer.token==null || securityBuffer.token.Length==0) {
                            inUnmanagedBuffer[index].token = IntPtr.Zero;
                        }
                        else {
                            pinnedInBytes[index] = GCHandle.Alloc(securityBuffer.token, GCHandleType.Pinned);
                            inUnmanagedBuffer[index].token = Marshal.UnsafeAddrOfPinnedArrayElement(securityBuffer.token, securityBuffer.offset);
                        }
#if TRAVE
                        GlobalLog.Print("SecBuffer: cbBuffer:" + securityBuffer.size +  " BufferType:" + securityBuffer.type);
//                        securityBuffer.DebugDump();
#endif
                    }
                }

                SSPIHandle contextHandle = new SSPIHandle();
                if (refContext != null) {
                    contextHandle = refContext._handle;
                }
                try {
                    if (dll==SecurDll.SECURITY) {
                        if (refContext == null || refContext.IsInvalid) {
                            refContext = new SafeDeleteContext_SECURITY();
                        }

                        bool b = false;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {
                            refContext.DangerousAddRef(ref b);
                        }
                        catch(Exception e) {
                            if (b)
                            {
                                refContext.DangerousRelease();
                                b = false;
                            }
                            if (!(e is ObjectDisposedException))
                                throw;
                        }
                        finally {
                            if (b)
                            {
                                errorCode = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.CompleteAuthToken(contextHandle.IsZero? null: &contextHandle, inSecurityBufferDescriptor);
                                refContext.DangerousRelease();
                            }
                        }

                    }
                    else {
                        throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "Dll");
                    }
                }
                finally {
                    if (pinnedInBytes!=null) {
                        for (int index=0; index<pinnedInBytes.Length; index++) {
                            if (pinnedInBytes[index].IsAllocated) {
                                pinnedInBytes[index].Free();
                            }
                        }
                    }
                }
            }

            GlobalLog.Leave("SafeDeleteContext::CompleteAuthToken() unmanaged CompleteAuthToken()", "errorCode:0x" + errorCode.ToString("x8") + " refContext:" + ValidationHelper.ToString(refContext));

            return errorCode;
        }
    }

//======================================================================
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeDeleteContext_SECURITY: SafeDeleteContext {

        internal SafeDeleteContext_SECURITY() : base() {}

        override protected bool ReleaseHandle()
        {
            if (this._EffectiveCredential != null)
                this._EffectiveCredential.DangerousRelease();

            return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.DeleteSecurityContext(ref _handle) == 0;
        }

    }
//======================================================================

#endif // !FEATURE_PAL

    internal class SafeNativeOverlapped : SafeHandle
    {
        internal static readonly SafeNativeOverlapped Zero = new SafeNativeOverlapped();

        internal SafeNativeOverlapped() 
            : this(IntPtr.Zero) 
        { 
        }

        internal unsafe SafeNativeOverlapped(NativeOverlapped* handle)
            : this((IntPtr)handle) 
        {
        }

        internal SafeNativeOverlapped(IntPtr handle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        public void ReinitializeNativeOverlapped()
        {
            IntPtr handleSnapshot = handle;

            if (handleSnapshot != IntPtr.Zero)
            {
                unsafe
                {
                    ((NativeOverlapped*)handleSnapshot)->InternalHigh = IntPtr.Zero;
                    ((NativeOverlapped*)handleSnapshot)->InternalLow = IntPtr.Zero;
                    ((NativeOverlapped*)handleSnapshot)->EventHandle = IntPtr.Zero;
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            IntPtr oldHandle = Interlocked.Exchange(ref handle, IntPtr.Zero);
            // Do not call free durring AppDomain shutdown, there may be an outstanding operation.
            // Overlapped will take care calling free when the native callback completes.
            if (oldHandle != IntPtr.Zero && !NclUtilities.HasShutdownStarted)
            {
                unsafe
                {
                    Overlapped.Free((NativeOverlapped*)oldHandle);
                }
            }
            return true;
        }
    }

    ///////////////////////////////////////////////////////////////
    //
    // This class implements a safe socket handle.
    // It uses an inner and outer SafeHandle to do so.  The inner
    // SafeHandle holds the actual socket, but only ever has one
    // reference to it.  The outer SafeHandle guards the inner
    // SafeHandle with real ref counting.  When the outer SafeHandle
    // is cleaned up, it releases the inner SafeHandle - since
    // its ref is the only ref to the inner SafeHandle, it deterministically
    // gets closed at that point - no ----s with concurrent IO calls.
    // This allows Close() on the outer SafeHandle to deterministically
    // close the inner SafeHandle, in turn allowing the inner SafeHandle
    // to block the user thread in case a g----ful close has been
    // requested.  (It's not legal to block any other thread - such closes
    // are always abortive.)
    //
    ///////////////////////////////////////////////////////////////
    [SuppressUnmanagedCodeSecurity]
#if DEBUG
    internal class SafeCloseSocket : DebugSafeHandleMinusOneIsInvalid
#else
    internal class SafeCloseSocket : SafeHandleMinusOneIsInvalid
#endif
    {
        protected SafeCloseSocket() : base(true) { }

        private InnerSafeCloseSocket m_InnerSocket;
        private volatile bool m_Released;
#if DEBUG
        private InnerSafeCloseSocket m_InnerSocketCopy;
#endif

        public override bool IsInvalid {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get {
                return IsClosed || base.IsInvalid;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void SetInnerSocket(InnerSafeCloseSocket socket)
        {
            m_InnerSocket = socket;
            SetHandle(socket.DangerousGetHandle());
#if DEBUG
            m_InnerSocketCopy = socket;
#endif
        }

        private static SafeCloseSocket CreateSocket(InnerSafeCloseSocket socket)
        {
            SafeCloseSocket ret = new SafeCloseSocket();
            CreateSocket(socket, ret);
            return ret;
        }

        protected static void CreateSocket(InnerSafeCloseSocket socket, SafeCloseSocket target)
        {
            if (socket!=null && socket.IsInvalid) {
                target.SetHandleAsInvalid();
                return;
            }

            bool b = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                socket.DangerousAddRef(ref b);
            }
            catch
            {
                if (b)
                {
                    socket.DangerousRelease();
                    b = false;
                }
            }
            finally
            {
                if (b)
                {
                    target.SetInnerSocket(socket);
                    socket.Close();
                }
                else
                {
                    target.SetHandleAsInvalid();
                }
            }
        }

        internal unsafe static SafeCloseSocket CreateWSASocket(byte* pinnedBuffer)
        {
            return CreateSocket(InnerSafeCloseSocket.CreateWSASocket(pinnedBuffer));
        }

        internal static SafeCloseSocket CreateWSASocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return CreateSocket(InnerSafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType));
        }

        internal static SafeCloseSocket Accept(
                                            SafeCloseSocket socketHandle,
                                            byte[] socketAddress,
                                            ref int socketAddressSize
                                            )
        {
            return CreateSocket(InnerSafeCloseSocket.Accept(socketHandle, socketAddress, ref socketAddressSize));
        }

        protected override bool ReleaseHandle()
        {
            m_Released = true;
            InnerSafeCloseSocket innerSocket = m_InnerSocket == null ? null : Interlocked.Exchange<InnerSafeCloseSocket>(ref m_InnerSocket, null);
            if (innerSocket != null)
            {
                innerSocket.DangerousRelease();
            }
            return true;
        }

        internal void CloseAsIs()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
#if DEBUG
                // If this throws it could be very bad.
                try
                {
#endif
                InnerSafeCloseSocket innerSocket = m_InnerSocket == null ? null : Interlocked.Exchange<InnerSafeCloseSocket>(ref m_InnerSocket, null);
                Close();
                if (innerSocket != null)
                {
                    // Wait until it's safe.
                    while (!m_Released)
                    {
                        Thread.SpinWait(1);
                    }

                    // Now free it with blocking.
                    innerSocket.BlockingRelease();
                }
#if DEBUG
                }
                catch (Exception exception)
                {
                    if (!NclUtilities.IsFatal(exception)){
                        GlobalLog.Assert("SafeCloseSocket::CloseAsIs(handle:" + handle.ToString("x") + ")", exception.Message);
                    }
                    throw;
                }
#endif
            }
        }

        internal class InnerSafeCloseSocket : SafeHandleMinusOneIsInvalid
        {
            protected InnerSafeCloseSocket() : base(true) { }

            private static readonly byte [] tempBuffer = new byte[1];
            private bool m_Blockable;

            public override bool IsInvalid {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get {
                    return IsClosed || base.IsInvalid;
                }
            }

            // This method is implicitly reliable and called from a CER.
            protected override bool ReleaseHandle()
            {
                bool ret = false;

#if DEBUG
                try
                {
#endif
                GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ")");

                SocketError errorCode;

                // If m_Blockable was set in BlockingRelease, it's safe to block here, which means
                // we can honor the linger options set on the socket.  It also means closesocket() might return WSAEWOULDBLOCK, in which
                // case we need to do some recovery.
                if (m_Blockable)
                {
                    GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") Following 'blockable' branch.");

                    errorCode = UnsafeNclNativeMethods.SafeNetHandles.closesocket(handle);
#if DEBUG
                    m_CloseSocketHandle = handle;
                    m_CloseSocketResult = errorCode;
#endif
                    if (errorCode == SocketError.SocketError) errorCode = (SocketError) Marshal.GetLastWin32Error();
                    GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") closesocket()#1:" + errorCode.ToString());

                    // If it's not WSAEWOULDBLOCK, there's no more recourse - we either succeeded or failed.
                    if (errorCode != SocketError.WouldBlock)
                    {
                        return ret = errorCode == SocketError.Success;
                    }

                    // The socket must be non-blocking with a linger timeout set.
                    // We have to set the socket to blocking.
                    int nonBlockCmd = 0;
                    errorCode = UnsafeNclNativeMethods.SafeNetHandles.ioctlsocket(
                        handle,
                        IoctlSocketConstants.FIONBIO,
                        ref nonBlockCmd);
                    if (errorCode == SocketError.SocketError) errorCode = (SocketError) Marshal.GetLastWin32Error();
                    GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") ioctlsocket()#1:" + errorCode.ToString());

                    // This can fail if there's a pending WSAEventSelect.  Try canceling it.
                    if (errorCode == SocketError.InvalidArgument)
                    {
                        errorCode = UnsafeNclNativeMethods.SafeNetHandles.WSAEventSelect(
                            handle,
                            IntPtr.Zero,
                            AsyncEventBits.FdNone);
                        GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") WSAEventSelect():" + (errorCode == SocketError.SocketError ? (SocketError)Marshal.GetLastWin32Error() : errorCode).ToString());

                        // Now retry the ioctl.
                        errorCode = UnsafeNclNativeMethods.SafeNetHandles.ioctlsocket(
                            handle,
                            IoctlSocketConstants.FIONBIO,
                            ref nonBlockCmd);
                        GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") ioctlsocket#2():" + (errorCode == SocketError.SocketError ? (SocketError)Marshal.GetLastWin32Error() : errorCode).ToString());
                    }

                    // If that succeeded, try again.
                    if (errorCode == SocketError.Success)
                    {
                        errorCode = UnsafeNclNativeMethods.SafeNetHandles.closesocket(handle);
#if DEBUG
                        m_CloseSocketHandle = handle;
                        m_CloseSocketResult = errorCode;
#endif
                        if (errorCode == SocketError.SocketError) errorCode = (SocketError) Marshal.GetLastWin32Error();
                        GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") closesocket#2():" + errorCode.ToString());

                        // If it's not WSAEWOULDBLOCK, there's no more recourse - we either succeeded or failed.
                        if (errorCode != SocketError.WouldBlock)
                        {
                            return ret = errorCode == SocketError.Success;
                        }
                    }

                    // It failed.  Fall through to the regular abortive close.
                }

                // By default or if CloseAsIs() path failed, set linger timeout to zero to get an abortive close (RST).
                Linger lingerStruct;
                lingerStruct.OnOff = 1;
                lingerStruct.Time = 0;

                errorCode = UnsafeNclNativeMethods.SafeNetHandles.setsockopt(
                    handle,
                    SocketOptionLevel.Socket,
                    SocketOptionName.Linger,
                    ref lingerStruct,
                    4);
#if DEBUG
                m_CloseSocketLinger = errorCode;
#endif
                if (errorCode == SocketError.SocketError) errorCode = (SocketError) Marshal.GetLastWin32Error();
                GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") setsockopt():" + errorCode.ToString());

                if (errorCode != SocketError.Success && errorCode != SocketError.InvalidArgument && errorCode != SocketError.ProtocolOption)
                {
                    // Too dangerous to try closesocket() - it might block!
                    return ret = false;
                }

                errorCode = UnsafeNclNativeMethods.SafeNetHandles.closesocket(handle);
#if DEBUG
                m_CloseSocketHandle = handle;
                m_CloseSocketResult = errorCode;
#endif
                GlobalLog.Print("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ") closesocket#3():" + (errorCode == SocketError.SocketError ? (SocketError)Marshal.GetLastWin32Error() : errorCode).ToString());

                return ret = errorCode == SocketError.Success;
#if DEBUG
                }
                catch (Exception exception)
                {
                    if (!NclUtilities.IsFatal(exception)){
                        GlobalLog.Assert("SafeCloseSocket::ReleaseHandle(handle:" + handle.ToString("x") + ")", exception.Message);
                    }
                    ret = true;  // Avoid a second assert.
                    throw;
                }
                finally
                {
                    m_CloseSocketThread = Thread.CurrentThread.ManagedThreadId;
                    m_CloseSocketTick = Environment.TickCount;
                    GlobalLog.Assert(ret, "SafeCloseSocket::ReleaseHandle(handle:{0:x})|ReleaseHandle failed.", handle);
                }
#endif
            }

#if DEBUG
            private IntPtr m_CloseSocketHandle;
            private SocketError m_CloseSocketResult = unchecked((SocketError) 0xdeadbeef);
            private SocketError m_CloseSocketLinger = unchecked((SocketError) 0xdeadbeef);
            private int m_CloseSocketThread;
            private int m_CloseSocketTick;
#endif

            // Use this method to close the socket handle using the linger options specified on the socket.
            // Guaranteed to only be called once, under a CER, and not if regular DangerousRelease is called.
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void BlockingRelease()
            {
                m_Blockable = true;
                DangerousRelease();
            }

            internal unsafe static InnerSafeCloseSocket CreateWSASocket(byte* pinnedBuffer)
            {
                //-1 is the value for FROM_PROTOCOL_INFO
                InnerSafeCloseSocket result = UnsafeNclNativeMethods.OSSOCK.WSASocket((AddressFamily) (-1),(SocketType) (-1),(ProtocolType) (-1), pinnedBuffer, 0, SocketConstructorFlags.WSA_FLAG_OVERLAPPED);
                if (result.IsInvalid) {
                    result.SetHandleAsInvalid();
                }
                return result;
            }

            internal static InnerSafeCloseSocket CreateWSASocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                InnerSafeCloseSocket result = UnsafeNclNativeMethods.OSSOCK.WSASocket(addressFamily, socketType, protocolType, IntPtr.Zero, 0, SocketConstructorFlags.WSA_FLAG_OVERLAPPED);
                if (result.IsInvalid) {
                    result.SetHandleAsInvalid();
                }
                return result;
            }

            internal static InnerSafeCloseSocket Accept(SafeCloseSocket socketHandle, byte[] socketAddress, ref int socketAddressSize)
            {
                InnerSafeCloseSocket result = UnsafeNclNativeMethods.SafeNetHandles.accept(socketHandle.DangerousGetHandle(), socketAddress, ref socketAddressSize);
                if (result.IsInvalid) {
                    result.SetHandleAsInvalid();
                }
                return result;
            }
        }
    }


    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCloseSocketAndEvent: SafeCloseSocket {
        internal SafeCloseSocketAndEvent() : base() {}
        private AutoResetEvent waitHandle;

        override protected bool ReleaseHandle()
        {
            bool result = base.ReleaseHandle();
            DeleteEvent();
            return result;
         }

        internal static SafeCloseSocketAndEvent CreateWSASocketWithEvent(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool autoReset, bool signaled){
            SafeCloseSocketAndEvent result = new SafeCloseSocketAndEvent();
            CreateSocket(InnerSafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType), result);
            if (result.IsInvalid) {
                throw new SocketException();
            }

            result.waitHandle = new AutoResetEvent(false);
            CompleteInitialization(result);
            return result;
        }

        internal static void CompleteInitialization(SafeCloseSocketAndEvent socketAndEventHandle){
            SafeWaitHandle handle = socketAndEventHandle.waitHandle.SafeWaitHandle;
            bool b = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle.DangerousAddRef(ref b);
            }
            catch
            {
                if (b)
                {
                    handle.DangerousRelease();
                    socketAndEventHandle.waitHandle = null;
                    b = false;
                }
            }
            finally
            {
                if (b)
                {
                    handle.Dispose();
                }
            }
        }


        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void DeleteEvent(){
            try{
                if(waitHandle != null){
                    waitHandle.SafeWaitHandle.DangerousRelease();
                }
            }
            catch{
            }
        }

        internal WaitHandle GetEventHandle(){
            return waitHandle;
        }
    }

    // Based on SafeLocalFree
    [SuppressUnmanagedCodeSecurity]
    internal class SafeLocalFreeChannelBinding : ChannelBinding
    {
        private const int LMEM_FIXED = 0;
        private int size;

        public override int Size
        {
            get { return size; }
        }

        public static SafeLocalFreeChannelBinding LocalAlloc(int cb)
        {
            SafeLocalFreeChannelBinding result;

            result = UnsafeNclNativeMethods.SafeNetHandles.LocalAllocChannelBinding(LMEM_FIXED, (UIntPtr)cb);
            if (result.IsInvalid)
            {
                result.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }

            result.size = cb;
            return result;
        }

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles.LocalFree(handle) == IntPtr.Zero;
        }
    }

    // Based on SafeFreeContextBuffer
    [SuppressUnmanagedCodeSecurity]
    internal abstract class SafeFreeContextBufferChannelBinding : ChannelBinding
    {
        private int size;

        public override int Size
        {
            get { return size; }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal unsafe void Set(IntPtr value)
        {
            this.handle = value;
        }

        internal static SafeFreeContextBufferChannelBinding CreateEmptyHandle(SecurDll dll)
        {
            switch (dll)
            {
                case SecurDll.SECURITY: return new SafeFreeContextBufferChannelBinding_SECURITY();
                default: throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SecurDll"), "dll");
            }
        }

        public unsafe static int QueryContextChannelBinding(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
        {
            switch (dll)
            {
                case SecurDll.SECURITY:
                    return QueryContextChannelBinding_SECURITY(phContext, contextAttribute, buffer, refHandle);

                default:
                    return -1;
            }
        }

        private unsafe static int QueryContextChannelBinding_SECURITY(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
        {
            int status = (int)SecurityStatus.InvalidHandle;
            bool b = false;

            // SCHANNEL only supports SECPKG_ATTR_ENDPOINT_BINDINGS and SECPKG_ATTR_UNIQUE_BINDINGS which
            // map to our enum ChannelBindingKind.Endpoint and ChannelBindingKind.Unique.
            if (contextAttribute != ContextAttribute.EndpointBindings && contextAttribute != ContextAttribute.UniqueBindings)
            {
                return status;
            }

            // We don't want to be interrupted by thread abort exceptions or unexpected out-of-memory errors failing to jit
            // one of the following methods. So run within a CER non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                phContext.DangerousAddRef(ref b);
            }
            catch(Exception e) {
                if (b)
                {
                    phContext.DangerousRelease();
                    b = false;
                }
                if (!(e is ObjectDisposedException))
                    throw;
            }
            finally {

                if (b)
                {
                    status = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.QueryContextAttributesW(ref phContext._handle, contextAttribute, buffer);
                    phContext.DangerousRelease();
                }

                if (status == 0 && refHandle != null) {
                    refHandle.Set((*buffer).pBindings);
                    refHandle.size = (*buffer).BindingsLength;
                }

                if (status != 0 && refHandle != null) {
                    refHandle.SetHandleAsInvalid();
                }
            }

            return status;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeContextBufferChannelBinding_SECURITY : SafeFreeContextBufferChannelBinding
    {
        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles_SECURITY.FreeContextBuffer(handle) == 0;
        }

    }

#if !FEATURE_PAL

    ///////////////////////////////////////////////////////////////
    //
    // This class implements a safe handle for WinInet cache stream
    //
    ///////////////////////////////////////////////////////////////
#if DEBUG
    internal sealed class SafeUnlockUrlCacheEntryFile : DebugSafeHandle {
#else
    internal sealed class SafeUnlockUrlCacheEntryFile : SafeHandleZeroOrMinusOneIsInvalid {
#endif
        private string      m_KeyString;

        private SafeUnlockUrlCacheEntryFile(string keyString): base(true) {
            m_KeyString = keyString;
        }

#if DEBUG
        //This method should never be called for this type
        public new IntPtr DangerousGetHandle()
        {
            throw new InvalidOperationException();
        }
#endif

        override unsafe protected bool ReleaseHandle()
        {
            fixed (char *ptrStr = m_KeyString) {
                UnsafeNclNativeMethods.SafeNetHandles.UnlockUrlCacheEntryFileW(ptrStr, 0);
            }
            SetHandle(IntPtr.Zero);
            m_KeyString = null;
            return true;
        }

        internal unsafe static _WinInetCache.Status GetAndLockFile(string key, byte* entryPtr, ref int entryBufSize, out SafeUnlockUrlCacheEntryFile handle) {

            if (ValidationHelper.IsBlankString(key)) {
                throw new ArgumentNullException("key");
            }

            handle = new SafeUnlockUrlCacheEntryFile(key);
            fixed (char* keyPtr = key) {
                return MustRunGetAndLockFile(keyPtr, entryPtr, ref entryBufSize, handle);
            }

        }
        //
        // Whis will check the result from PInvoke and make a valid safeHandle on success
        //
        unsafe private static _WinInetCache.Status MustRunGetAndLockFile(char* key, byte* entryPtr, ref int entryBufSize, SafeUnlockUrlCacheEntryFile handle) {
            _WinInetCache.Status error = _WinInetCache.Status.Success;

            // Run the body of this method as a non-interruptible block.
            RuntimeHelpers.PrepareConstrainedRegions();
            try {} finally {

                if (!UnsafeNclNativeMethods.SafeNetHandles.RetrieveUrlCacheEntryFileW(key, entryPtr, ref entryBufSize, 0))
                {
                    error = (_WinInetCache.Status)Marshal.GetLastWin32Error();
                    handle.SetHandleAsInvalid();
                }
                else {
                    // Hack: that will return 1 in place of a handle
                    // The real handle here is a "key" string
                    handle.SetHandle((IntPtr)1);
                }
            }

            return error;
        }

    }


    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed unsafe class SafeRegistryHandle :
#if DEBUG
    DebugSafeHandle
#else
    SafeHandleZeroOrMinusOneIsInvalid
#endif
    {
        private SafeRegistryHandle() : base(true) { }

        internal static uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenKeyEx(key, subKey, ulOptions, samDesired, out resultSubKey);
        }

        internal uint RegOpenKeyEx(string subKey, uint ulOptions, uint samDesired, out SafeRegistryHandle resultSubKey)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenKeyEx(this, subKey, ulOptions, samDesired, out resultSubKey);
        }

        internal uint RegCloseKey()
        {
            Close();
            return resClose;
        }

        internal uint QueryValue(string name, out object data)
        {
            data = null;

            byte[] blob = null;
            uint size = 0;
            uint type;
            uint errorCode;

            while (true)
            {
                errorCode = UnsafeNclNativeMethods.RegistryHelper.RegQueryValueEx(this, name, IntPtr.Zero, out type, blob, ref size);
                if (errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA &&
                    (blob != null || errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS))
                {
                    break;
                }

                blob = new byte[size];
            }

            if (errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
            {
                return errorCode;
            }

            switch (type)
            {
                case UnsafeNclNativeMethods.RegistryHelper.REG_BINARY:
                    if (size != blob.Length)
                    {
                        byte[] oldBlob = blob;
                        blob = new byte[size];
                        Buffer.BlockCopy(oldBlob, 0, blob, 0, (int) size);
                    }
                    data = blob;
                    return UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;

                default:
                    return UnsafeNclNativeMethods.ErrorCodes.ERROR_NOT_SUPPORTED;
            }
        }

        internal uint RegNotifyChangeKeyValue(bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegNotifyChangeKeyValue(this, watchSubTree, notifyFilter, regEvent, async);
        }

        internal static uint RegOpenCurrentUser(uint samDesired, out SafeRegistryHandle resultKey)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenCurrentUser(samDesired, out resultKey);
        }


        override protected bool ReleaseHandle()
        {
            if(!IsInvalid)
                resClose = UnsafeNclNativeMethods.RegistryHelper.RegCloseKey(handle);
            SetHandleAsInvalid();
            return true;
        }

        private uint resClose;
    }

    // This class is a wrapper for a WSPC (WebSocket protocol component) session. WebSocketCreateClientHandle and WebSocketCreateServerHandle return a PVOID and not a real handle
    // but we use a SafeHandle because it provides us the guarantee that WebSocketDeleteHandle will always get called.
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeWebSocketHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeWebSocketHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (this.IsInvalid)
            {
                return true;
            }

            WebSocketProtocolComponent.WebSocketDeleteHandle(this.handle);
            return true;
        }
    }
#endif // !FEATURE_PAL
}
