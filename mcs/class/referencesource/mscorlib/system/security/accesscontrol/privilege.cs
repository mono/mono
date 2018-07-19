// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Privilege
**
** Purpose: Managed wrapper for NT privileges.
**
** Date:  July 1, 2004
**
===========================================================*/

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

namespace System.Security.AccessControl
{
    using CultureInfo = System.Globalization.CultureInfo;
    using FCall = System.Security.Principal.Win32;
    using Luid = Microsoft.Win32.Win32Native.LUID;

#if false
    internal delegate void PrivilegedHelper();
#endif

    internal sealed class Privilege
    {
        private static LocalDataStoreSlot tlsSlot = Thread.AllocateDataSlot();
        private static Hashtable privileges = new Hashtable();
        private static Hashtable luids = new Hashtable();
        private static ReaderWriterLock privilegeLock = new ReaderWriterLock();

        private bool needToRevert = false;
        private bool initialState = false;
        private bool stateWasChanged = false;
        [System.Security.SecurityCritical] // auto-generated
        private Luid luid;
        private readonly Thread currentThread = Thread.CurrentThread;
        private TlsContents tlsContents = null;

        public const string CreateToken                     = "SeCreateTokenPrivilege";
        public const string AssignPrimaryToken              = "SeAssignPrimaryTokenPrivilege";
        public const string LockMemory                      = "SeLockMemoryPrivilege";
        public const string IncreaseQuota                   = "SeIncreaseQuotaPrivilege";
        public const string UnsolicitedInput                = "SeUnsolicitedInputPrivilege";
        public const string MachineAccount                  = "SeMachineAccountPrivilege";
        public const string TrustedComputingBase            = "SeTcbPrivilege";
        public const string Security                        = "SeSecurityPrivilege";
        public const string TakeOwnership                   = "SeTakeOwnershipPrivilege";
        public const string LoadDriver                      = "SeLoadDriverPrivilege";
        public const string SystemProfile                   = "SeSystemProfilePrivilege";
        public const string SystemTime                      = "SeSystemtimePrivilege";
        public const string ProfileSingleProcess            = "SeProfileSingleProcessPrivilege";
        public const string IncreaseBasePriority            = "SeIncreaseBasePriorityPrivilege";
        public const string CreatePageFile                  = "SeCreatePagefilePrivilege";
        public const string CreatePermanent                 = "SeCreatePermanentPrivilege";
        public const string Backup                          = "SeBackupPrivilege";
        public const string Restore                         = "SeRestorePrivilege";
        public const string Shutdown                        = "SeShutdownPrivilege";
        public const string Debug                           = "SeDebugPrivilege";
        public const string Audit                           = "SeAuditPrivilege";
        public const string SystemEnvironment               = "SeSystemEnvironmentPrivilege";
        public const string ChangeNotify                    = "SeChangeNotifyPrivilege";
        public const string RemoteShutdown                  = "SeRemoteShutdownPrivilege";
        public const string Undock                          = "SeUndockPrivilege";
        public const string SyncAgent                       = "SeSyncAgentPrivilege";
        public const string EnableDelegation                = "SeEnableDelegationPrivilege";
        public const string ManageVolume                    = "SeManageVolumePrivilege";
        public const string Impersonate                     = "SeImpersonatePrivilege";
        public const string CreateGlobal                    = "SeCreateGlobalPrivilege";
        public const string TrustedCredentialManagerAccess  = "SeTrustedCredManAccessPrivilege";
        public const string ReserveProcessor                = "SeReserveProcessorPrivilege";

        //
        // This routine is a wrapper around a hashtable containing mappings
        // of privilege names to LUIDs
        //

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        private static Luid LuidFromPrivilege( string privilege )
        {
            Luid luid;
            luid.LowPart = 0;
            luid.HighPart = 0;

            //
            // Look up the privilege LUID inside the cache
            //

            RuntimeHelpers.PrepareConstrainedRegions();

            try
            {
                privilegeLock.AcquireReaderLock( -1 );

                if ( luids.Contains( privilege ))
                {
                    luid = ( Luid )luids[ privilege ];

                    privilegeLock.ReleaseReaderLock();
                }
                else
                {
                    privilegeLock.ReleaseReaderLock();

                    if ( false == Win32Native.LookupPrivilegeValue( null, privilege, ref luid ))
                    {
                        int error = Marshal.GetLastWin32Error();

                        if ( error == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
                        {
                            throw new OutOfMemoryException();
                        }
                        else if ( error == Win32Native.ERROR_ACCESS_DENIED )
                        {
                            throw new UnauthorizedAccessException();
                        }
                        else if ( error == Win32Native.ERROR_NO_SUCH_PRIVILEGE )
                        {
                            throw new ArgumentException(
                                Environment.GetResourceString( "Argument_InvalidPrivilegeName",
                                privilege ));
                        }
                        else
                        {
                            Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "LookupPrivilegeValue() failed with unrecognized error code {0}", error ));
                            throw new InvalidOperationException();
                        }
                    }

                    privilegeLock.AcquireWriterLock( -1 );
                }
            }
            finally
            {
                if ( privilegeLock.IsReaderLockHeld )
                {
                    privilegeLock.ReleaseReaderLock();
                }

                if ( privilegeLock.IsWriterLockHeld )
                {
                    if ( !luids.Contains( privilege ))
                    {
                        luids[ privilege ] = luid;
                        privileges[ luid ] = privilege;
                    }

                    privilegeLock.ReleaseWriterLock();
                }
            }

            return luid;
        }

        private sealed class TlsContents : IDisposable
        {
            private bool disposed = false;
            private int referenceCount = 1;
            [System.Security.SecurityCritical] // auto-generated
            private SafeAccessTokenHandle threadHandle = new SafeAccessTokenHandle( IntPtr.Zero );
            private bool isImpersonating = false;

            [System.Security.SecurityCritical] // auto-generated
            private static volatile SafeAccessTokenHandle processHandle = new SafeAccessTokenHandle( IntPtr.Zero );
            private static readonly object syncRoot = new object();

#region Constructor and Finalizer
            
            [System.Security.SecuritySafeCritical]  // auto-generated
            static TlsContents()
            {
            }

            [System.Security.SecurityCritical]  // auto-generated
            [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
            public TlsContents()
            {
                int error = 0;
                int cachingError = 0;
                bool success = true;

                if ( processHandle.IsInvalid)
                {
                    lock( syncRoot )
                    {
                        if ( processHandle.IsInvalid)
                        {
                            SafeAccessTokenHandle localProcessHandle;
                            if ( false == Win32Native.OpenProcessToken(
                                            Win32Native.GetCurrentProcess(),
                                            TokenAccessLevels.Duplicate,
                                            out localProcessHandle))
                            {
                                cachingError = Marshal.GetLastWin32Error();
                                success = false;
                            }
                            processHandle = localProcessHandle;
                        }
                    }
                }

                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    // Make the sequence non-interruptible
                }
                finally
                {
                    try
                    {
                        //
                        // Open the thread token; if there is no thread token, get one from
                        // the process token by impersonating self.
                        //

                        SafeAccessTokenHandle threadHandleBefore = this.threadHandle;
                        error = FCall.OpenThreadToken(
                                      TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                                      WinSecurityContext.Process,
                                      out this.threadHandle );
                        unchecked { error &= ~(int)0x80070000; }

                        if ( error != 0 )
                        {
                            if ( success == true )
                            {
                                this.threadHandle = threadHandleBefore;

                                if ( error != Win32Native.ERROR_NO_TOKEN )
                                {
                                    success = false;
                                }

                                Contract.Assert( this.isImpersonating == false, "Incorrect isImpersonating state" );

                                if ( success == true )
                                {
                                    error = 0;
                                    if ( false == Win32Native.DuplicateTokenEx(
                                                    processHandle,
                                                    TokenAccessLevels.Impersonate | TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges,
                                                    IntPtr.Zero,
                                                    Win32Native.SECURITY_IMPERSONATION_LEVEL.Impersonation,
                                                    System.Security.Principal.TokenType.TokenImpersonation,
                                                    ref this.threadHandle ))
                                    {
                                        error = Marshal.GetLastWin32Error();
                                        success = false;
                                    }
                                }

                                if ( success == true )
                                {
                                    error = FCall.SetThreadToken( this.threadHandle );
                                    unchecked { error &= ~(int)0x80070000; }

                                    if ( error != 0 )
                                    {
                                        success = false;
                                    }
                                }

                                if ( success == true )
                                {
                                    this.isImpersonating = true;
                                }
                            }
                            else
                            {
                                error = cachingError;
                            }
                        }
                        else
                        {
                            success = true;
                        }
                    }
                    finally
                    {
                        if ( !success )
                        {
                            Dispose();
                        }
                    }
                }

                if ( error == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
                {
                    throw new OutOfMemoryException();
                }
                else if ( error == Win32Native.ERROR_ACCESS_DENIED ||
                    error == Win32Native.ERROR_CANT_OPEN_ANONYMOUS )
                {
                    throw new UnauthorizedAccessException();
                }
                else if ( error != 0 )
                {
                    Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "WindowsIdentity.GetCurrentThreadToken() failed with unrecognized error code {0}", error ));
                    throw new InvalidOperationException();
                }
            }

            [System.Security.SecuritySafeCritical]
            ~TlsContents()
            {
                if ( !this.disposed )
                {
                    Dispose( false );
                }
            }
#endregion

#region IDisposable implementation

            [System.Security.SecuritySafeCritical] // overrides public transparent member
            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }

            [System.Security.SecurityCritical]  // auto-generated
            private void Dispose( bool disposing )
            {
                if ( this.disposed ) return;

                if ( disposing )
                {
                    if ( this.threadHandle != null )
                    {
                        this.threadHandle.Dispose();
                        this.threadHandle = null;
                    }
                }

                if ( this.isImpersonating )
                {
                    FCall.RevertToSelf();
                }

                this.disposed = true;
            }
#endregion

#region Reference Counting

            public void IncrementReferenceCount()
            {
                this.referenceCount++;
            }

            [System.Security.SecurityCritical]  // auto-generated
            public int DecrementReferenceCount()
            {
                int result = --this.referenceCount;

                if ( result == 0 )
                {
                    Dispose();
                }

                return result;
            }

            public int ReferenceCountValue
            {
                get { return this.referenceCount; }
            }
#endregion

#region Properties

            public SafeAccessTokenHandle ThreadHandle
            {
                [System.Security.SecurityCritical]  // auto-generated
                get { return this.threadHandle; }
            }

            public bool IsImpersonating
            {
                get { return this.isImpersonating; }
            }
#endregion
        }

#region Constructors

        [System.Security.SecurityCritical]  // auto-generated
        public Privilege( string privilegeName )
        {
            if ( privilegeName == null )
            {
                throw new ArgumentNullException( "privilegeName" );
            }
            Contract.EndContractBlock();

            this.luid = LuidFromPrivilege( privilegeName );
        }
#endregion

        //
        // Finalizer simply ensures that the privilege was not leaked
        //

        [System.Security.SecuritySafeCritical]
        ~Privilege()
        {
            Contract.Assert( !this.needToRevert, "Must revert privileges that you alter!" );

            if ( this.needToRevert )
            {
                Revert();
            }
        }

#region Public interface
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public void Enable()
        {
            this.ToggleState( true );
        }

        public bool NeedToRevert
        {
            get { return this.needToRevert; }
        }

#endregion

//      [SecurityPermission( SecurityAction.Demand, TogglePrivileges=true )]
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain | ResourceScope.Assembly)]
        private void ToggleState( bool enable )
        {
            int error = 0;

            //
            // All privilege operations must take place on the same thread
            //

            if ( !this.currentThread.Equals( Thread.CurrentThread ))
            {
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_MustBeSameThread" ));
            }

            //
            // This privilege was already altered and needs to be reverted before it can be altered again
            //

            if ( this.needToRevert )
            {
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_MustRevertPrivilege" ));
            }

            //
            // Need to make this block of code non-interruptible so that it would preserve
            // consistency of thread oken state even in the face of catastrophic exceptions
            //

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                //
                // The payload is entirely in the finally block
                // This is how we ensure that the code will not be
                // interrupted by catastrophic exceptions
                //
            }
            finally 
            {
                try
                {
                    //
                    // Retrieve TLS state
                    //

                    this.tlsContents = Thread.GetData( tlsSlot ) as TlsContents;

                    if ( this.tlsContents == null )
                    {
                        this.tlsContents = new TlsContents();
                        Thread.SetData( tlsSlot, this.tlsContents );
                    }
                    else
                    {
                        this.tlsContents.IncrementReferenceCount();
                    }

                    Win32Native.TOKEN_PRIVILEGE newState = new Win32Native.TOKEN_PRIVILEGE();
                    newState.PrivilegeCount = 1;
                    newState.Privilege.Luid = this.luid;
                    newState.Privilege.Attributes = enable ? Win32Native.SE_PRIVILEGE_ENABLED : Win32Native.SE_PRIVILEGE_DISABLED;
                    
                    Win32Native.TOKEN_PRIVILEGE previousState = new Win32Native.TOKEN_PRIVILEGE();
                    uint previousSize = 0;

                    //
                    // Place the new privilege on the thread token and remember the previous state.
                    //

                    if ( false == Win32Native.AdjustTokenPrivileges(
                                      this.tlsContents.ThreadHandle,
                                      false,
                                      ref newState,
                                      ( uint )Marshal.SizeOf( previousState ),
                                      ref previousState,
                                      ref previousSize ))
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                    else if ( Win32Native.ERROR_NOT_ALL_ASSIGNED == Marshal.GetLastWin32Error())
                    {
                        error = Win32Native.ERROR_NOT_ALL_ASSIGNED;
                    }
                    else
                    {
                        //
                        // This is the initial state that revert will have to go back to
                        //

                        this.initialState = (( previousState.Privilege.Attributes & Win32Native.SE_PRIVILEGE_ENABLED ) != 0 );

                        //
                        // Remember whether state has changed at all
                        //

                        this.stateWasChanged = ( this.initialState != enable );

                        //
                        // If we had to impersonate, or if the privilege state changed we'll need to revert
                        //

                        this.needToRevert = this.tlsContents.IsImpersonating || this.stateWasChanged;
                    }
                }
                finally
                {
                    if ( !this.needToRevert )
                    {
                        this.Reset();
                    }
                }
            }

            if ( error == Win32Native.ERROR_NOT_ALL_ASSIGNED )
            {
                throw new PrivilegeNotHeldException( privileges[this.luid] as string );
            }
            if ( error == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                throw new OutOfMemoryException();
            }
            else if ( error == Win32Native.ERROR_ACCESS_DENIED ||
                error == Win32Native.ERROR_CANT_OPEN_ANONYMOUS )
            {
                throw new UnauthorizedAccessException();
            }
            else if ( error != 0 )
            {
                Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "AdjustTokenPrivileges() failed with unrecognized error code {0}", error ));
                throw new InvalidOperationException();
            }
        }

//      [SecurityPermission( SecurityAction.Demand, TogglePrivileges=true )]
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public void Revert()
        {
            int error = 0;

            if ( !this.currentThread.Equals( Thread.CurrentThread ))
            {
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_MustBeSameThread" ));
            }

            if ( !this.NeedToRevert )
            {
                return;
            }

            //
            // This code must be eagerly prepared and non-interruptible.
            //

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                //
                // The payload is entirely in the finally block
                // This is how we ensure that the code will not be
                // interrupted by catastrophic exceptions
                //
            }
            finally
            {
                bool success = true;

                try
                {
                    //
                    // Only call AdjustTokenPrivileges if we're not going to be reverting to self,
                    // on this Revert, since doing the latter obliterates the thread token anyway
                    //

                    if ( this.stateWasChanged &&
                        ( this.tlsContents.ReferenceCountValue > 1 ||
                          !this.tlsContents.IsImpersonating ))
                    {
                        Win32Native.TOKEN_PRIVILEGE newState = new Win32Native.TOKEN_PRIVILEGE();
                        newState.PrivilegeCount = 1;
                        newState.Privilege.Luid = this.luid;
                        newState.Privilege.Attributes = ( this.initialState ? Win32Native.SE_PRIVILEGE_ENABLED : Win32Native.SE_PRIVILEGE_DISABLED );

                        Win32Native.TOKEN_PRIVILEGE previousState = new Win32Native.TOKEN_PRIVILEGE();
                        uint previousSize = 0;

                        if ( false == Win32Native.AdjustTokenPrivileges(
                                          this.tlsContents.ThreadHandle,
                                          false,
                                          ref newState,
                                          ( uint )Marshal.SizeOf( previousState ),
                                          ref previousState,
                                          ref previousSize ))
                        {
                            error = Marshal.GetLastWin32Error();
                            success = false;
                        }
                    }
                }
                finally
                {
                    if ( success )
                    {
                        this.Reset();
                    }
                }
            }

            if ( error == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                throw new OutOfMemoryException();
            }
            else if ( error == Win32Native.ERROR_ACCESS_DENIED )
            {
                throw new UnauthorizedAccessException();
            }
            else if ( error != 0 )
            {
                Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "AdjustTokenPrivileges() failed with unrecognized error code {0}", error ));
                throw new InvalidOperationException();
            }
        }
#if false
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static void RunWithPrivilege( string privilege, bool enabled, PrivilegedHelper helper )
        {
            if ( helper == null )
            {
                throw new ArgumentNullException( "helper" );
            }
            Contract.EndContractBlock();
            
            Privilege p = new Privilege( privilege );

            RuntimeHelpers.PrepareConstrainedRegions();

            try 
            {
                if (enabled)
                {
                    p.Enable();
                }
                else
                {
                    p.Disable();
                }

                helper();
            }
            finally
            {
                p.Revert();
            }
        }
#endif

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
        private void Reset()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.stateWasChanged = false;
                this.initialState = false;
                this.needToRevert = false;

                if ( this.tlsContents != null )
                {
                    if ( 0 == this.tlsContents.DecrementReferenceCount())
                    {
                        this.tlsContents = null;
                        Thread.SetData( tlsSlot, null );
                    }
                }
            }
        }
    }
}
