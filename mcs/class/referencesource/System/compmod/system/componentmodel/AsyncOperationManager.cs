//------------------------------------------------------------------------------
// <copyright file="AsyncOperationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel
{
    using System.Collections;
    using System.Threading;
    using System.Diagnostics;
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public static class AsyncOperationManager {
        public static AsyncOperation CreateOperation(object userSuppliedState) {
            return AsyncOperation.CreateOperation(userSuppliedState, SynchronizationContext);
        }

        /// <include file='doc\AsyncOperationManager.uex' path='docs/doc[@for="AsyncOperationManager.SynchronizationContext"]/*' />
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static SynchronizationContext SynchronizationContext {
            get {
                if (SynchronizationContext.Current == null) {
                    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                }

                return SynchronizationContext.Current;
            }

#if SILVERLIGHT
            // a thread should set this to null  when it is done, else the context will never be disposed/GC'd
            [SecurityCritical]
            [FriendAccessAllowed]
            internal set {
                SynchronizationContext.SetSynchronizationContext(value);
            }
#else
            // a thread should set this to null  when it is done, else the context will never be disposed/GC'd
            [PermissionSetAttribute(SecurityAction.LinkDemand, Name="FullTrust")]
            set {
                SynchronizationContext.SetSynchronizationContext(value);
            }
#endif
        }
    }
}

        
