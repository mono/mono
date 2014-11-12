// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Classes:  AnonymousPipeServerStream
**           AnonymousPipeClientStream
**           NamedPipeServerStream
**           NamedPipeClientStream
**
** Purpose: pipe stream classes.
**
**
===========================================================*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Pipes {

    /// <summary>
    /// Anonymous pipe server stream
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class AnonymousPipeServerStream : PipeStream {

        private SafePipeHandle m_clientHandle;
        private bool m_clientHandleExposed;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public AnonymousPipeServerStream()
            : this(PipeDirection.Out, HandleInheritability.None, 0, null) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public AnonymousPipeServerStream(PipeDirection direction)
            : this(direction, HandleInheritability.None, 0) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability)
            : this(direction, inheritability, 0) { }

        // bufferSize is used as a suggestion; specify 0 to let OS decide
        // This constructor instantiates the PipeSecurity using just the inheritability flag
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize)
            : base(direction, bufferSize) {
            if (direction == PipeDirection.InOut) {
                throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeUnidirectional));
            }
            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability", SR.GetString(SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable));
            }

            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability);
            Create(direction, secAttrs, bufferSize);
        }

        // bufferSize is used as a suggestion; specify 0 to let OS decide
        // pipeSecurity of null is default security descriptor
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
            : base(direction, bufferSize) {
            if (direction == PipeDirection.InOut) {
                throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeUnidirectional));
            }
            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability", SR.GetString(SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable));
            }

            Object pinningHandle;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability, pipeSecurity, out pinningHandle);

            try {
                Create(direction, secAttrs, bufferSize);
            }
            finally {
                if (pinningHandle != null) {
                    GCHandle pinHandle = (GCHandle)pinningHandle;
                    pinHandle.Free();
                }
            }
        }

        ~AnonymousPipeServerStream() {
            Dispose(false);
        }

        // Create an AnonymousPipeServerStream from two existing pipe handles.
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, SafePipeHandle serverSafePipeHandle, SafePipeHandle clientSafePipeHandle)
            : base(direction, 0) {
            if (direction == PipeDirection.InOut) {
                throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeUnidirectional));
            }
            if (serverSafePipeHandle == null) {
                throw new ArgumentNullException("serverSafePipeHandle");
            }
            if (clientSafePipeHandle == null) {
                throw new ArgumentNullException("clientSafePipeHandle");
            }
            if (serverSafePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "serverSafePipeHandle");
            }
            if (clientSafePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "clientSafePipeHandle");
            }

            // Check that these handles are in fact a handles to a pipe.
            if (UnsafeNativeMethods.GetFileType(serverSafePipeHandle) != UnsafeNativeMethods.FILE_TYPE_PIPE) {
                throw new IOException(SR.GetString(SR.IO_IO_InvalidPipeHandle));
            }
            if (UnsafeNativeMethods.GetFileType(clientSafePipeHandle) != UnsafeNativeMethods.FILE_TYPE_PIPE) {
                throw new IOException(SR.GetString(SR.IO_IO_InvalidPipeHandle));
            }

            InitializeHandle(serverSafePipeHandle, true, false);

            m_clientHandle = clientSafePipeHandle;
            m_clientHandleExposed = true;
            State = PipeState.Connected;
        }

        // This method should exist until we add a first class way of passing handles between parent and child
        // processes. For now, people do it via command line arguments. 
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Reliability","CA2001:AvoidCallingProblematicMethods", MessageId="System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification="By design")]
        public String GetClientHandleAsString() {
            m_clientHandleExposed = true;
            return m_clientHandle.DangerousGetHandle().ToString();
        }

        public SafePipeHandle ClientSafePipeHandle {
            [System.Security.SecurityCritical]
            get {
                m_clientHandleExposed = true;
                return m_clientHandle;
            }
        }

        // This method is an annoying one but it has to exist at least until we make passing handles between 
        // processes first class.  We need this because once the child handle is inherited, the OS considers
        // the parent and child's handles to be different.  Therefore, if a child closes its handle, our 
        // Read/Write methods won't throw because the OS will think that there is still a child handle around
        // that can still Write/Read to/from the other end of the pipe.
        //
        // Ideally, we would want the Process class to close this handle after it has been inherited.  See
        // the pipe spec future features section for more information.
        // 
        // Right now, this is the best signal to set the anonymous pipe as connected; if this is called, we
        // know the client has been passed the handle and so the connection is live.
        [System.Security.SecurityCritical]
        public void DisposeLocalCopyOfClientHandle() {
            if (m_clientHandle != null && !m_clientHandle.IsClosed) {
                m_clientHandle.Dispose();
            }
        }

        [System.Security.SecurityCritical]
        protected override void Dispose(bool disposing) {
            try {
                // We should dispose of the client handle if it was not exposed. 
                if (!m_clientHandleExposed && m_clientHandle != null && !m_clientHandle.IsClosed) {
                    m_clientHandle.Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        // Creates the anonymous pipe.
        [System.Security.SecurityCritical]
        private void Create(PipeDirection direction, UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs, int bufferSize) {
            Debug.Assert(direction != PipeDirection.InOut, "Anonymous pipe direction shouldn't be InOut");
            Debug.Assert(bufferSize >= 0, "bufferSize is negative");

            bool bSuccess;
            SafePipeHandle serverHandle;
            SafePipeHandle newServerHandle;

            // Create the two pipe handles that make up the anonymous pipe.
            if (direction == PipeDirection.In) {
                bSuccess = UnsafeNativeMethods.CreatePipe(out serverHandle, out m_clientHandle, secAttrs, bufferSize);
            }
            else {
                bSuccess = UnsafeNativeMethods.CreatePipe(out m_clientHandle, out serverHandle, secAttrs, bufferSize);
            }

            if (!bSuccess) {
                __Error.WinIOError(Marshal.GetLastWin32Error(), String.Empty);
            }

            // Duplicate the server handle to make it not inheritable.  Note: We need to do this so that the child 
            // process doesn't end up getting another copy of the server handle.  If it were to get a copy, the
            // OS wouldn't be able to inform the child that the server has closed its handle because it will see
            // that there is still one server handle that is open.  
            bSuccess = UnsafeNativeMethods.DuplicateHandle(UnsafeNativeMethods.GetCurrentProcess(), serverHandle, UnsafeNativeMethods.GetCurrentProcess(),
                    out newServerHandle, 0, false, UnsafeNativeMethods.DUPLICATE_SAME_ACCESS);

            if (!bSuccess) {
                __Error.WinIOError(Marshal.GetLastWin32Error(), String.Empty);
            }

            // Close the inheritable server handle.
            serverHandle.Dispose();

            InitializeHandle(newServerHandle, false, false);

            State = PipeState.Connected;
        }

        // Anonymous pipes do not support message mode so there is no need to use the base version that P/Invokes here.
        public override PipeTransmissionMode TransmissionMode {
            [System.Security.SecurityCritical]
            get { 
                return PipeTransmissionMode.Byte; 
            }
        }

        public override PipeTransmissionMode ReadMode {
            [System.Security.SecurityCritical]
            set {
                CheckPipePropertyOperations();

                if (value < PipeTransmissionMode.Byte || value > PipeTransmissionMode.Message) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ArgumentOutOfRange_TransmissionModeByteOrMsg));
                }

                if (value == PipeTransmissionMode.Message) {
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeMessagesNotSupported));
                }
            }
        }
    }


    /// <summary>
    /// Anonymous pipe client. Use this to open the client end of an anonymous pipes created with AnonymousPipeServerStream.
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class AnonymousPipeClientStream : PipeStream {

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Naming","CA1720:IdentifiersShouldNotContainTypeNames", MessageId="string", Justification="By design")]
        public AnonymousPipeClientStream(String pipeHandleAsString)
            : this(PipeDirection.In, pipeHandleAsString) { }

        [System.Security.SecurityCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SuppressMessage("Microsoft.Naming","CA1720:IdentifiersShouldNotContainTypeNames", MessageId="string", Justification="By design")]
        public AnonymousPipeClientStream(PipeDirection direction, String pipeHandleAsString)
            : base(direction, 0) {

            if (direction == PipeDirection.InOut) {
                throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeUnidirectional));
            }
            if (pipeHandleAsString == null) {
                throw new ArgumentNullException("pipeHandleAsString");
            }

            // Initialize SafePipeHandle from String and check if it's valid. First see if it's parseable
            long result = 0;
            bool parseable = long.TryParse(pipeHandleAsString, out result);
            if (!parseable) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "pipeHandleAsString");
            }

            // next check whether the handle is invalid
            SafePipeHandle safePipeHandle = new SafePipeHandle((IntPtr)result, true);
            if (safePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "pipeHandleAsString");
            }

            Init(direction, safePipeHandle);
        }

        [System.Security.SecurityCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public AnonymousPipeClientStream(PipeDirection direction, SafePipeHandle safePipeHandle)
            : base(direction, 0) {

            if (direction == PipeDirection.InOut) {
                throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeUnidirectional));
            }
            if (safePipeHandle == null) {
                throw new ArgumentNullException("safePipeHandle");
            }
            if (safePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "safePipeHandle");
            }

            Init(direction, safePipeHandle);
        }

        [System.Security.SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void Init(PipeDirection direction, SafePipeHandle safePipeHandle) {
            Debug.Assert(direction != PipeDirection.InOut, "anonymous pipes are unidirectional, caller should have verified before calling Init");
            Debug.Assert(safePipeHandle != null && !safePipeHandle.IsInvalid, "safePipeHandle must be valid");

            // Check that this handle is infact a handle to a pipe.
            if (UnsafeNativeMethods.GetFileType(safePipeHandle) != UnsafeNativeMethods.FILE_TYPE_PIPE) {
                throw new IOException(SR.GetString(SR.IO_IO_InvalidPipeHandle));
            }

            InitializeHandle(safePipeHandle, true, false);
            State = PipeState.Connected;
        }


        ~AnonymousPipeClientStream() {
            Dispose(false);
        }

        // Anonymous pipes do not support message readmode so there is no need to use the base version
        // which P/Invokes (and sometimes fails).
        public override PipeTransmissionMode TransmissionMode {
            [System.Security.SecurityCritical]
            get { 
                return PipeTransmissionMode.Byte; 
            }
        }

        public override PipeTransmissionMode ReadMode {
            [System.Security.SecurityCritical]
            set {
                CheckPipePropertyOperations();

                if (value < PipeTransmissionMode.Byte || value > PipeTransmissionMode.Message) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.ArgumentOutOfRange_TransmissionModeByteOrMsg));
                }
                if (value == PipeTransmissionMode.Message) {
                    throw new NotSupportedException(SR.GetString(SR.NotSupported_AnonymousPipeMessagesNotSupported));
                }
            }
        }

    }

    // Users will use this delegate to specify a method to call while impersonating the client 
    // (see NamedPipeServerStream.RunAsClient).
    public delegate void PipeStreamImpersonationWorker();


  
    /// <summary>
    /// Named pipe server
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class NamedPipeServerStream : PipeStream {

        // Use the maximum number of server instances that the system resources allow
        public const int MaxAllowedServerInstances = -1;

        [SecurityCritical]
        private unsafe static readonly IOCompletionCallback WaitForConnectionCallback =
            new IOCompletionCallback(NamedPipeServerStream.AsyncWaitForConnectionCallback);

        [System.Security.SecurityCritical]
        static NamedPipeServerStream()
        {
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName)
            : this(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, 
            HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction)
            : this(pipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, 
            HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances)
            : this(pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.None, 
            0, 0, null, HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode)
            : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None, 0, 0,
                null, HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options)
            : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, 0, 0,
                null, HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
            : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize,
                null, HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
                PipeSecurity pipeSecurity)
            : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize,
                pipeSecurity, HandleInheritability.None, (PipeAccessRights)0) { }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
                PipeSecurity pipeSecurity, HandleInheritability inheritability)
            : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize,
                pipeSecurity, inheritability, (PipeAccessRights)0) { }

        /// <summary>
        /// Full named pipe server constructor
        /// </summary>
        /// <param name="pipeName">Pipe name</param>
        /// <param name="direction">Pipe direction: In, Out or InOut (duplex). 
        /// Win32 note: this gets OR'd into dwOpenMode to CreateNamedPipe
        /// </param>
        /// <param name="maxNumberOfServerInstances">Maximum number of server instances. Specify a fixed value between 
        /// 1 and 254, or use NamedPipeServerStream.MaxAllowedServerInstances to use the maximum amount allowed by 
        /// system resources.</param>
        /// <param name="transmissionMode">Byte mode or message mode.
        /// Win32 note: this gets used for dwPipeMode. CreateNamedPipe allows you to specify PIPE_TYPE_BYTE/MESSAGE
        /// and PIPE_READMODE_BYTE/MESSAGE independently, but this sets type and readmode to match.
        /// </param>
        /// <param name="options">PipeOption enum: None, Asynchronous, or Writethrough
        /// Win32 note: this gets passed in with dwOpenMode to CreateNamedPipe. Asynchronous corresponds to 
        /// FILE_FLAG_OVERLAPPED option. PipeOptions enum doesn't expose FIRST_PIPE_INSTANCE option because
        /// this sets that automatically based on the number of instances specified.
        /// </param>
        /// <param name="inBufferSize">Incoming buffer size, 0 or higher.
        /// Note: this size is always advisory; OS uses a suggestion.
        /// </param>
        /// <param name="outBufferSize">Outgoing buffer size, 0 or higher (see above)</param>
        /// <param name="pipeSecurity">PipeSecurity, or null for default security descriptor</param>
        /// <param name="inheritability">Whether handle is inheritable</param>
        /// <param name="additionalAccessRights">Combination (logical OR) of PipeAccessRights.TakeOwnership, 
        /// PipeAccessRights.AccessSystemSecurity, and PipeAccessRights.ChangePermissions</param>
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(String pipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
                PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
            : base(direction, transmissionMode, outBufferSize) {
            if (pipeName == null) {
                throw new ArgumentNullException("pipeName");
            }
            if (pipeName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyPipeName));
            }
            if ((options & ~(PipeOptions.WriteThrough | PipeOptions.Asynchronous)) != 0) {
                throw new ArgumentOutOfRangeException("options", SR.GetString(SR.ArgumentOutOfRange_OptionsInvalid));
            }
            if (inBufferSize < 0) {
                throw new ArgumentOutOfRangeException("inBufferSize", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
            }
            // win32 allows fixed values of 1-254 or 255 to mean max allowed by system. We expose 255 as -1 (unlimited)
            // through the MaxAllowedServerInstances constant. This is consistent e.g. with -1 as infinite timeout, etc
            if ((maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) && (maxNumberOfServerInstances != MaxAllowedServerInstances)) {
                throw new ArgumentOutOfRangeException("maxNumberOfServerInstances", SR.GetString(SR.ArgumentOutOfRange_MaxNumServerInstances));
            }
            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability", SR.GetString(SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable));
            }
            // ChangePermissions, TakeOwnership, and AccessSystemSecurity are only legal values user may provide;
            // internally this is set to 0 if not provided. This handles both cases.
            if ((additionalAccessRights & ~(PipeAccessRights.ChangePermissions | PipeAccessRights.TakeOwnership |
                       PipeAccessRights.AccessSystemSecurity)) != 0) {
                throw new ArgumentOutOfRangeException("additionalAccessRights", SR.GetString(SR.ArgumentOutOfRange_AdditionalAccessLimited));
            }

            // Named Pipe Servers require Windows NT
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows) {
                throw new PlatformNotSupportedException(SR.GetString(SR.PlatformNotSupported_NamedPipeServers));
            }

            string normalizedPipePath = Path.GetFullPath(@"\\.\pipe\" + pipeName);

            // Make sure the pipe name isn't one of our reserved names for anonymous pipes.
            if (String.Compare(normalizedPipePath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0) {
                throw new ArgumentOutOfRangeException("pipeName", SR.GetString(SR.ArgumentOutOfRange_AnonymousReserved));
            }
            
            Object pinningHandle = null;
            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability, pipeSecurity, out pinningHandle);

            try {
                Create(normalizedPipePath, direction, maxNumberOfServerInstances, transmissionMode,
                        options, inBufferSize, outBufferSize, additionalAccessRights, secAttrs);
            }
            finally {
                if (pinningHandle != null) {
                    GCHandle pinHandle = (GCHandle)pinningHandle;
                    pinHandle.Free();
                }
            }
        }

        // Create a NamedPipeServerStream from an existing server pipe handle.
        [System.Security.SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeServerStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
            : base(direction, PipeTransmissionMode.Byte, 0) {

            if (safePipeHandle == null) {
                throw new ArgumentNullException("safePipeHandle");
            }
            if (safePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "safePipeHandle");
            }
            // Check that this handle is infact a handle to a pipe.
            if (UnsafeNativeMethods.GetFileType(safePipeHandle) != UnsafeNativeMethods.FILE_TYPE_PIPE) {
                throw new IOException(SR.GetString(SR.IO_IO_InvalidPipeHandle));
            }

            InitializeHandle(safePipeHandle, true, isAsync);

            if (isConnected) {
                State = PipeState.Connected;
            }
        }

        ~NamedPipeServerStream() {
            Dispose(false);
        }

        [System.Security.SecurityCritical]
        private void Create(String fullPipeName, PipeDirection direction, int maxNumberOfServerInstances,
                PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
                PipeAccessRights rights, UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs) {
            Debug.Assert(fullPipeName != null && fullPipeName.Length != 0, "fullPipeName is null or empty");
            Debug.Assert(direction >= PipeDirection.In && direction <= PipeDirection.InOut, "invalid pipe direction");
            Debug.Assert(inBufferSize >= 0, "inBufferSize is negative");
            Debug.Assert(outBufferSize >= 0, "outBufferSize is negative");
            Debug.Assert((maxNumberOfServerInstances >= 1 && maxNumberOfServerInstances <= 254) || (maxNumberOfServerInstances == MaxAllowedServerInstances), "maxNumberOfServerInstances is invalid");
            Debug.Assert(transmissionMode >= PipeTransmissionMode.Byte && transmissionMode <= PipeTransmissionMode.Message, "transmissionMode is out of range");

            int openMode = ((int)direction) |
                           (maxNumberOfServerInstances == 1 ? UnsafeNativeMethods.FILE_FLAG_FIRST_PIPE_INSTANCE : 0) |
                           (int)options |
                           (int)rights;

            // We automatically set the ReadMode to match the TransmissionMode.
            int pipeModes = (int)transmissionMode << 2 | (int)transmissionMode << 1;

            // Convert -1 to 255 to match win32 (we asserted that it is between -1 and 254).
            if (maxNumberOfServerInstances == MaxAllowedServerInstances) {
                maxNumberOfServerInstances = 255;
            }

            SafePipeHandle handle = UnsafeNativeMethods.CreateNamedPipe(fullPipeName, openMode, pipeModes,
                    maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, secAttrs);

            if (handle.IsInvalid) {
                __Error.WinIOError(Marshal.GetLastWin32Error(), String.Empty);
            }

            InitializeHandle(handle, false, (options & PipeOptions.Asynchronous) != 0);
        }

        // This will wait until the client calls Connect().  If we return from this method, we guarantee that
        // the client has returned from its Connect call.   The client may have done so before this method 
        // was called (but not before this server is been created, or, if we were servicing another client, 
        // not before we called Disconnect), in which case, there may be some buffer already in the pipe waiting
        // for us to read.  See NamedPipeClientStream.Connect for more information.
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
        public void WaitForConnection() {
            CheckConnectOperationsServer();

            if (IsAsync) {
                IAsyncResult result = BeginWaitForConnection(null, null);
                EndWaitForConnection(result);
            }
            else {
                if (!UnsafeNativeMethods.ConnectNamedPipe(InternalHandle, UnsafeNativeMethods.NULL)) {
                    int errorCode = Marshal.GetLastWin32Error();

                    if (errorCode != UnsafeNativeMethods.ERROR_PIPE_CONNECTED) {
                        __Error.WinIOError(errorCode, String.Empty);
                    }

                    // pipe already connected
                    if (errorCode == UnsafeNativeMethods.ERROR_PIPE_CONNECTED && State == PipeState.Connected) {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeAlreadyConnected));
                    }

                    // If we reach here then a connection has been established.  This can happen if a client 
                    // connects in the interval between the call to CreateNamedPipe and the call to ConnectNamedPipe. 
                    // In this situation, there is still a good connection between client and server, even though 
                    // ConnectNamedPipe returns zero.
                }
                State = PipeState.Connected;
            }
        }

        // Async version of WaitForConnection.  See the comments above for more info.
        [System.Security.SecurityCritical]
        [HostProtection(ExternalThreading = true)]
        public unsafe IAsyncResult BeginWaitForConnection(AsyncCallback callback, Object state) {
            CheckConnectOperationsServer();

            if (!IsAsync) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotAsync));
            }

            // Create and store async stream class library specific data in the 
            // async result
            PipeAsyncResult asyncResult = new PipeAsyncResult();
            asyncResult._handle = InternalHandle;
            asyncResult._userCallback = callback;
            asyncResult._userStateObject = state;

            // Create wait handle and store in async result
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            asyncResult._waitHandle = waitHandle;

            // Create a managed overlapped class
            // We will set the file offsets later
            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, asyncResult);

            // Pack the Overlapped class, and store it in the async result
            NativeOverlapped* intOverlapped = overlapped.Pack(WaitForConnectionCallback, null);
            asyncResult._overlapped = intOverlapped;

            if (!UnsafeNativeMethods.ConnectNamedPipe(InternalHandle, intOverlapped)) {
                int errorCode = Marshal.GetLastWin32Error();

                if (errorCode == UnsafeNativeMethods.ERROR_IO_PENDING)
                    return asyncResult;

                // WaitForConnectionCallback will not be called becasue we completed synchronously.
                // Either the pipe is already connected, or there was an error. Unpin and free the overlapped again.
                Overlapped.Free(intOverlapped);
                asyncResult._overlapped = null;

                // Did the client already connect to us?
                if (errorCode == UnsafeNativeMethods.ERROR_PIPE_CONNECTED) {

                    if (State == PipeState.Connected) {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeAlreadyConnected));
                    }
                    asyncResult.CallUserCallback();
                    return asyncResult;
                }
                
                __Error.WinIOError(errorCode, String.Empty);                
            }
            // will set state to Connected when EndWait is called

            return asyncResult;
        }

        // Async version of WaitForConnection.  See comments for WaitForConnection for more info.
        [System.Security.SecurityCritical]
        public unsafe void EndWaitForConnection(IAsyncResult asyncResult) {
            CheckConnectOperationsServer();

            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }
            if (!IsAsync) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotAsync));
            }

            PipeAsyncResult afsar = asyncResult as PipeAsyncResult;
            if (afsar == null) {
                __Error.WrongAsyncResult();
            }

            // Ensure we can't get into any ----s by doing an interlocked
            // CompareExchange here.  Avoids corrupting memory via freeing the
            // NativeOverlapped class or GCHandle twice.  -- 
            if (1 == Interlocked.CompareExchange(ref afsar._EndXxxCalled, 1, 0)) {
                __Error.EndWaitForConnectionCalledTwice();
            }

            // Obtain the WaitHandle, but don't use public property in case we
            // delay initialize the manual reset event in the future.
            WaitHandle wh = afsar._waitHandle;
            if (wh != null) {
                // We must block to ensure that ConnectionIOCallback has completed,
                // and we should close the WaitHandle in here.  AsyncFSCallback
                // and the hand-ported imitation version in COMThreadPool.cpp 
                // are the only places that set this event.
                try {
                    wh.WaitOne();
                    Debug.Assert(afsar._isComplete == true, "NamedPipeServerStream::EndWaitForConnection - AsyncFSCallback didn't set _isComplete to true!");
                }
                finally {
                    wh.Close();
                }
            }

            // We should have freed the overlapped and set it to null either in the Begin
            // method (if ConnectNamedPipe completed synchronously) or in AsyncWaitForConnectionCallback.
            // If it is not nulled out, we should not be past the above wait:
            Debug.Assert(afsar._overlapped == null);            

            // Now check for any error during the read.
            if (afsar._errorCode != 0) {
                __Error.WinIOError(afsar._errorCode, String.Empty);
            }

            // Success
            State = PipeState.Connected;
        }

        [System.Security.SecurityCritical]
        public void Disconnect() {
            CheckDisconnectOperations();

            // Disconnect the pipe.
            if (!UnsafeNativeMethods.DisconnectNamedPipe(InternalHandle)) {
                __Error.WinIOError(Marshal.GetLastWin32Error(), String.Empty);
            }

            State = PipeState.Disconnected;
        }
        

        // This method calls a delegate while impersonating the client. Note that we will not have
        // access to the client's security token until it has written at least once to the pipe 
        // (and has set its impersonationLevel argument appropriately). 
        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
        public void RunAsClient(PipeStreamImpersonationWorker impersonationWorker) {
             CheckWriteOperations();
             ExecuteHelper execHelper = new ExecuteHelper(impersonationWorker, InternalHandle);
             RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, execHelper);

             // now handle win32 impersonate/revert specific errors by throwing corresponding exceptions
             if (execHelper.m_impersonateErrorCode != 0) {
                 WinIOError(execHelper.m_impersonateErrorCode);
             }
             else if (execHelper.m_revertImpersonateErrorCode != 0) {
                 WinIOError(execHelper.m_revertImpersonateErrorCode);
             }
        }

        // the following are needed for CER
        
        private static RuntimeHelpers.TryCode tryCode = new RuntimeHelpers.TryCode(ImpersonateAndTryCode);
        private static RuntimeHelpers.CleanupCode cleanupCode = new RuntimeHelpers.CleanupCode(RevertImpersonationOnBackout);

        [System.Security.SecurityCritical]
        private static void ImpersonateAndTryCode(Object helper) {
            ExecuteHelper execHelper = (ExecuteHelper)helper;

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally {
                if (UnsafeNativeMethods.ImpersonateNamedPipeClient(execHelper.m_handle)) {
                    execHelper.m_mustRevert = true;
                }
                else {
                    execHelper.m_impersonateErrorCode = Marshal.GetLastWin32Error();
                }

            }

            if (execHelper.m_mustRevert) { // impersonate passed so run user code
                execHelper.m_userCode();
            }
        }

        [System.Security.SecurityCritical]
        [PrePrepareMethod]
        private static void RevertImpersonationOnBackout(Object helper, bool exceptionThrown) {
            ExecuteHelper execHelper = (ExecuteHelper)helper;

            if (execHelper.m_mustRevert) {
                if (!UnsafeNativeMethods.RevertToSelf()) {
                    execHelper.m_revertImpersonateErrorCode = Marshal.GetLastWin32Error();
                }
            }
        }

        internal class ExecuteHelper {
            internal PipeStreamImpersonationWorker m_userCode;
            internal SafePipeHandle m_handle;
            internal bool m_mustRevert;
            internal int m_impersonateErrorCode;
            internal int m_revertImpersonateErrorCode;

            [System.Security.SecurityCritical]
             internal ExecuteHelper(PipeStreamImpersonationWorker userCode, SafePipeHandle handle) {
                m_userCode = userCode;
                m_handle = handle;
            }
        }


        // Gets the username of the connected client.  Not that we will not have access to the client's 
        // username until it has written at least once to the pipe (and has set its impersonationLevel 
        // argument appropriately). 
        [System.Security.SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
        public String GetImpersonationUserName() {
            CheckWriteOperations();

            StringBuilder userName = new StringBuilder(UnsafeNativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);

            if (!UnsafeNativeMethods.GetNamedPipeHandleState(InternalHandle, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL,
                UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL, userName, userName.Capacity)) {
                WinIOError(Marshal.GetLastWin32Error());
            }

            return userName.ToString();
        }

        // Callback to be called by the OS when completing the async WaitForConnection operation.
        [System.Security.SecurityCritical]
        unsafe private static void AsyncWaitForConnectionCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped) {
            // Unpack overlapped
            Overlapped overlapped = Overlapped.Unpack(pOverlapped);

            // Extract async result from overlapped 
            PipeAsyncResult asyncResult = (PipeAsyncResult)overlapped.AsyncResult;

            // Free the pinned overlapped:            
            Debug.Assert(asyncResult._overlapped == pOverlapped);
            Overlapped.Free(pOverlapped);            
            asyncResult._overlapped = null;

            // Special case for when the client has already connected to us.
            if (errorCode == UnsafeNativeMethods.ERROR_PIPE_CONNECTED) {
                errorCode = 0;
            }

            asyncResult._errorCode = (int)errorCode;

            // Call the user-provided callback.  It can and often should
            // call EndWaitForConnection.  There's no reason to use an async 
            // delegate here - we're already on a threadpool thread.  
            // IAsyncResult's completedSynchronously property must return
            // false here, saying the user callback was called on another thread.
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;

            // The OS does not signal this event.  We must do it ourselves.
            ManualResetEvent wh = asyncResult._waitHandle;
            if (wh != null) {
                Debug.Assert(!wh.SafeWaitHandle.IsClosed, "ManualResetEvent already closed!");
                bool r = wh.Set();
                Debug.Assert(r, "ManualResetEvent::Set failed!");
                if (!r) { 
                    __Error.WinIOError(); 
                }
            }

            AsyncCallback userCallback = asyncResult._userCallback;

            if (userCallback != null) {
                userCallback(asyncResult);
            }
        }

        // Server can only connect from Disconnected state
        [System.Security.SecurityCritical]
        [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Consistent with security model")]
        private void CheckConnectOperationsServer() {
            // we're not checking whether already connected; this allows us to throw IOException
            // "pipe is being closed" if other side is closing (as does win32) or no-op if
            // already connected

            if (InternalHandle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }
            // object disposed
            if (State == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (InternalHandle.IsClosed) {
                __Error.PipeNotOpen();
            }
            // IOException
            if (State == PipeState.Broken) {
                throw new IOException(SR.GetString(SR.IO_IO_PipeBroken));
            }
        }

        // Server is allowed to disconnect from connected and broken states
        [System.Security.SecurityCritical]
        private void CheckDisconnectOperations() {

            // invalid operation
            if (State== PipeState.WaitingToConnect) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotYetConnected));
            }
            if (State == PipeState.Disconnected) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeAlreadyDisconnected));
            }
            if (InternalHandle == null) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeHandleNotSet));
            }
            // object disposed
            if (State == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
            if (InternalHandle.IsClosed) {
                __Error.PipeNotOpen();
            }
        }

    }

    // Named pipe client. Use this to open the client end of a named pipes created with 
    // NamedPipeServerStream.
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class NamedPipeClientStream : PipeStream {

        private string m_normalizedPipePath;
        private TokenImpersonationLevel m_impersonationLevel;
        private PipeOptions m_pipeOptions;
        private HandleInheritability m_inheritability;
        private int m_access;

        // Creates a named pipe client using default server (same machine, or "."), and PipeDirection.InOut 
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String pipeName)
            : this(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName)
            : this(serverName, pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName, PipeDirection direction)
            : this(serverName, pipeName, direction, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName, PipeDirection direction,
               PipeOptions options)
            : this(serverName, pipeName, direction, options, TokenImpersonationLevel.None, HandleInheritability.None) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName, PipeDirection direction,
               PipeOptions options, TokenImpersonationLevel impersonationLevel)
            : this(serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None) { }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName, PipeDirection direction,
               PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
            : base(direction, 0) {

            if (pipeName == null) {
                throw new ArgumentNullException("pipeName");
            }
            if (serverName == null) {
                throw new ArgumentNullException("serverName", SR.GetString(SR.ArgumentNull_ServerName));
            }
            if (pipeName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyPipeName));
            }
            if (serverName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_EmptyServerName));
            }
            if ((options & ~(PipeOptions.WriteThrough | PipeOptions.Asynchronous)) != 0) {
                throw new ArgumentOutOfRangeException("options", SR.GetString(SR.ArgumentOutOfRange_OptionsInvalid));
            }
            if (impersonationLevel < TokenImpersonationLevel.None || impersonationLevel > TokenImpersonationLevel.Delegation) {
                throw new ArgumentOutOfRangeException("impersonationLevel", SR.GetString(SR.ArgumentOutOfRange_ImpersonationInvalid));
            }
            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability", SR.GetString(SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable));
            }

            m_normalizedPipePath = Path.GetFullPath(@"\\" + serverName + @"\pipe\" + pipeName);

            if (String.Compare(m_normalizedPipePath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0) {
                throw new ArgumentOutOfRangeException("pipeName", SR.GetString(SR.ArgumentOutOfRange_AnonymousReserved));
            }

            m_inheritability = inheritability;
            m_impersonationLevel = impersonationLevel;
            m_pipeOptions = options;

            if ((PipeDirection.In & direction) != 0) {
                m_access |= UnsafeNativeMethods.GENERIC_READ;
            }
            if ((PipeDirection.Out & direction) != 0) {
                m_access |= UnsafeNativeMethods.GENERIC_WRITE;
            }
        }

        // This constructor is for advanced users that want to specify their PipeAccessRights explcitly.  It can be used
        // to open pipes with, for example, WritePermissions access in the case that they want to play with the pipe's
        // ACL.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        [SecuritySafeCritical]
        public NamedPipeClientStream(String serverName, String pipeName, PipeAccessRights desiredAccessRights,
               PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
            : base(DirectionFromRights(desiredAccessRights), 0) {

            if (pipeName == null) {
                throw new ArgumentNullException("pipeName");
            }
            if (serverName == null) {
                throw new ArgumentNullException("serverName", SR.GetString(SR.ArgumentNull_ServerName));
            }
            if (pipeName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyPipeName));
            }
            if (serverName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Argument_EmptyServerName));
            }
            if ((options & ~(PipeOptions.WriteThrough | PipeOptions.Asynchronous)) != 0) {
                throw new ArgumentOutOfRangeException("options", SR.GetString(SR.ArgumentOutOfRange_OptionsInvalid));
            }
            if (impersonationLevel < TokenImpersonationLevel.None || impersonationLevel > TokenImpersonationLevel.Delegation) {
                throw new ArgumentOutOfRangeException("impersonationLevel", SR.GetString(SR.ArgumentOutOfRange_ImpersonationInvalid));
            }
            if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
                throw new ArgumentOutOfRangeException("inheritability", SR.GetString(SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable));
            }
            if ((desiredAccessRights & ~(PipeAccessRights.FullControl | PipeAccessRights.AccessSystemSecurity)) != 0) {
                throw new ArgumentOutOfRangeException("desiredAccessRights", SR.GetString(SR.ArgumentOutOfRange_InvalidPipeAccessRights));
            }

            m_normalizedPipePath = Path.GetFullPath(@"\\" + serverName + @"\pipe\" + pipeName);

            if (String.Compare(m_normalizedPipePath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0) {
                throw new ArgumentOutOfRangeException("pipeName", SR.GetString(SR.ArgumentOutOfRange_AnonymousReserved));
            }

            m_inheritability = inheritability;
            m_impersonationLevel = impersonationLevel;
            m_pipeOptions = options;
            m_access = (int)desiredAccessRights;
        }

        // Helper method for the constructor above.  The PipeStream protected constructor takes in a PipeDirection so we need
        // to convert the access rights to a direction.  Usually, PipeDirection.In/Out maps to GENERIC_READ/WRITE but in the
        // other direction, READ/WRITE_DATA is sufficient.
        private static PipeDirection DirectionFromRights(PipeAccessRights rights) {
            PipeDirection direction = 0;
            if ((rights & PipeAccessRights.ReadData) != 0) {
                direction |= PipeDirection.In;
            }
            if ((rights & PipeAccessRights.WriteData) != 0) {
                direction |= PipeDirection.Out;
            }

            return direction;
        }

        // Create a NamedPipeClientStream from an existing server pipe handle.
        [System.Security.SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public NamedPipeClientStream(PipeDirection direction, bool isAsync, bool isConnected,
                SafePipeHandle safePipeHandle)
            : base(direction, 0) {
 
            if (safePipeHandle == null) {
                throw new ArgumentNullException("safePipeHandle");
            }

            if (safePipeHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidHandle), "safePipeHandle");
            }
            // Check that this handle is infact a handle to a pipe.
            if (UnsafeNativeMethods.GetFileType(safePipeHandle) != UnsafeNativeMethods.FILE_TYPE_PIPE) {
                throw new IOException(SR.GetString(SR.IO_IO_InvalidPipeHandle));
            }

            InitializeHandle(safePipeHandle, true, isAsync);
            if (isConnected) {
                State = PipeState.Connected;
            }
        }

        ~NamedPipeClientStream() {
            Dispose(false);
        }

        // See below
        public void Connect() {
            Connect(Timeout.Infinite);
        }

        // Waits for a pipe instance to become available.  This method may return before WaitForConnection is called
        // on the server end, but WaitForConnection will not return until we have returned.  Any data writen to the
        // pipe by us after we have connected but before the server has called WaitForConnection will be available
        // to the server after it calls WaitForConnection. 
        [System.Security.SecurityCritical]
        public void Connect(int timeout) {
            CheckConnectOperationsClient();

            if (timeout < 0 && timeout != Timeout.Infinite) {
                throw new ArgumentOutOfRangeException("timeout", SR.GetString(SR.ArgumentOutOfRange_InvalidTimeout));
            }

            UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(m_inheritability);

            int _pipeFlags = (int)m_pipeOptions;
            if (m_impersonationLevel != TokenImpersonationLevel.None) {
                _pipeFlags |= UnsafeNativeMethods.SECURITY_SQOS_PRESENT;
                _pipeFlags |= (((int)m_impersonationLevel - 1) << 16);
            }

            // This is the main connection loop. It will loop until the timeout expires.  Most of the 
            // time, we will be waiting in the WaitNamedPipe win32 blocking function; however, there are
            // cases when we will need to loop: 1) The server is not created (WaitNamedPipe returns 
            // straight away in such cases), and 2) when another client connects to our server in between 
            // our WaitNamedPipe and CreateFile calls.
            int startTime = Environment.TickCount;
            int elapsed = 0;
            do {
                // Wait for pipe to become free (this will block unless the pipe does not exist).
                if (!UnsafeNativeMethods.WaitNamedPipe(m_normalizedPipePath, timeout - elapsed)) {
                    int errorCode = Marshal.GetLastWin32Error();

                    // Server is not yet created so let's keep looping.
                    if (errorCode == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND) {
                        continue; 
                    }

                    // The timeout has expired.
                    if (errorCode == UnsafeNativeMethods.ERROR_SUCCESS) {
                        break;
                    }

                    __Error.WinIOError(errorCode, String.Empty);
                }

                // Pipe server should be free.  Let's try to connect to it.
                SafePipeHandle handle = UnsafeNativeMethods.CreateNamedPipeClient(m_normalizedPipePath, 
                                            m_access,           // read and write access
                                            0,                  // sharing: none
                                            secAttrs,           // security attributes
                                            FileMode.Open,      // open existing 
                                            _pipeFlags,         // impersonation flags
                                            UnsafeNativeMethods.NULL);  // template file: null

                if (handle.IsInvalid) {
                    int errorCode = Marshal.GetLastWin32Error();

                    // Handle the possible race condition of someone else connecting to the server 
                    // between our calls to WaitNamedPipe & CreateFile.
                    if (errorCode == UnsafeNativeMethods.ERROR_PIPE_BUSY) {
                        continue;
                    }

                    __Error.WinIOError(errorCode, String.Empty);
                }

                // Success! 
                InitializeHandle(handle, false, (m_pipeOptions & PipeOptions.Asynchronous) != 0);
                State = PipeState.Connected;

                return;
            }
            while (timeout == Timeout.Infinite || (elapsed = unchecked(Environment.TickCount - startTime)) < timeout);
            // BUGBUG: SerialPort does not use unchecked arithmetic when calculating elapsed times.  This is needed
            //         because Environment.TickCount can overflow (though only every 49.7 days).

            throw new TimeoutException();
        }

        public int NumberOfServerInstances {
            [System.Security.SecurityCritical]
            [SuppressMessage("Microsoft.Security","CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="Security model of pipes: demand at creation but no subsequent demands")]
            get {
                CheckPipePropertyOperations();

                // NOTE: MSDN says that GetNamedPipeHandleState requires that the pipe handle has 
                // GENERIC_READ access, but we don't check for that because sometimes it works without
                // GERERIC_READ access. [Edit: Seems like CreateFile slaps on a READ_ATTRIBUTES 
                // access request before calling NTCreateFile, so all NamedPipeClientStreams can read
                // this if they are created (on WinXP SP2 at least)] 
                int numInstances;
                if (!UnsafeNativeMethods.GetNamedPipeHandleState(InternalHandle, UnsafeNativeMethods.NULL, out numInstances,
                    UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL, UnsafeNativeMethods.NULL, 0)) {
                    WinIOError(Marshal.GetLastWin32Error());
                }

                return numInstances;
            }
        }

        // override because named pipe clients can't get/set properties when waiting to connect
        // or broken
        [System.Security.SecurityCritical]
        protected override internal void CheckPipePropertyOperations() {
            base.CheckPipePropertyOperations();

            // Invalid operation
            if (State == PipeState.WaitingToConnect) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeNotYetConnected));
            }

            // IOException
            if (State == PipeState.Broken) {
                throw new IOException(SR.GetString(SR.IO_IO_PipeBroken));
            }
        }

        // named client is allowed to connect from broken
        private void CheckConnectOperationsClient() {
            
            if (State == PipeState.Connected) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_PipeAlreadyConnected));
            }
            if (State == PipeState.Closed) {
                __Error.PipeNotOpen();
            }
        }
    }

    unsafe internal sealed class PipeAsyncResult : IAsyncResult {
        internal AsyncCallback _userCallback;   // User code callback
        internal Object _userStateObject;
        internal ManualResetEvent _waitHandle;
        [SecurityCritical]
        internal SafePipeHandle _handle;
        [SecurityCritical]
        internal NativeOverlapped* _overlapped;

        internal int _EndXxxCalled;             // Whether we've called EndXxx already.
        internal int _errorCode;

        internal bool _isComplete;              // Value for IsCompleted property        
        internal bool _completedSynchronously;  // Which thread called callback

        public Object AsyncState {
            get { 
                return _userStateObject; 
            }
        }

        public bool IsCompleted {
            get { 
                return _isComplete; 
            }
        }

        public WaitHandle AsyncWaitHandle {
            [System.Security.SecurityCritical]
            get {
                if (_waitHandle == null) {
                    ManualResetEvent mre = new ManualResetEvent(false);
                    if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero) {
                        mre.SafeWaitHandle = new SafeWaitHandle(_overlapped->EventHandle, true);
                    }
                    if (_isComplete) {
                        mre.Set();
                    }
                    _waitHandle = mre;
                }
                return _waitHandle;
            }
        }

        public bool CompletedSynchronously {
            get { 
                return _completedSynchronously; 
            }
        }

        private void CallUserCallbackWorker(Object callbackState) {
            _isComplete = true;
            if (_waitHandle != null) {
                _waitHandle.Set();
            }
            _userCallback(this);
        }

        internal void CallUserCallback() {
            if (_userCallback != null) {
                _completedSynchronously = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(CallUserCallbackWorker));
            }
            else {
                _isComplete = true;
                if (_waitHandle != null) {
                    _waitHandle.Set();
                }
            }
        }
    }
}
