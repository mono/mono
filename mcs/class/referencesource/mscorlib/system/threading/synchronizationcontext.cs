// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//
// <OWNER>[....]</OWNER>
/*============================================================
**
** Class:  SynchronizationContext
**
**
** Purpose: Capture synchronization semantics for asynchronous callbacks
**
** 
===========================================================*/

namespace System.Threading
{    
    using Microsoft.Win32.SafeHandles;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
#if FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ExceptionServices;
#endif // FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Runtime.ConstrainedExecution;
    using System.Reflection;
    using System.Security;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.CodeAnalysis;


#if FEATURE_SYNCHRONIZATIONCONTEXT_WAIT
    [Flags]
    enum SynchronizationContextProperties
    {
        None = 0,
        RequireWaitNotification = 0x1
    };
#endif

#if FEATURE_COMINTEROP && FEATURE_APPX
    //
    // This is implemented in System.Runtime.WindowsRuntime, allowing us to ask that assembly for a WinRT-specific SyncCtx.
    // I'd like this to be an interface, or at least an abstract class - but neither seems to play nice with FriendAccessAllowed.
    //
    [FriendAccessAllowed]
    [SecurityCritical]
    internal class WinRTSynchronizationContextFactoryBase
    {
        [SecurityCritical]
        public virtual SynchronizationContext Create(object coreDispatcher) {return null;}
    }
#endif //FEATURE_COMINTEROP

#if !FEATURE_CORECLR
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags =SecurityPermissionFlag.ControlPolicy|SecurityPermissionFlag.ControlEvidence)]
#endif
    public class SynchronizationContext
    {
#if FEATURE_SYNCHRONIZATIONCONTEXT_WAIT
        SynchronizationContextProperties _props = SynchronizationContextProperties.None;
#endif
        
        public SynchronizationContext()
        {
        }
                        
#if FEATURE_SYNCHRONIZATIONCONTEXT_WAIT

        static Type s_cachedPreparedType1;
        static Type s_cachedPreparedType2;
        static Type s_cachedPreparedType3;
        static Type s_cachedPreparedType4;
        static Type s_cachedPreparedType5;

        // protected so that only the derived [....] context class can enable these flags
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "We never dereference s_cachedPreparedType*, so ordering is unimportant")]
        protected void SetWaitNotificationRequired()
        {
            //
            // Prepare the method so that it can be called in a reliable fashion when a wait is needed.
            // This will obviously only make the Wait reliable if the Wait method is itself reliable. The only thing
            // preparing the method here does is to ensure there is no failure point before the method execution begins.
            //
            // Preparing the method in this way is quite expensive, but only needs to be done once per type, per AppDomain.
            // So we keep track of a few types we've already prepared in this AD.  It is uncommon to have more than
            // a few SynchronizationContext implementations, so we only cache the first five we encounter; this lets
            // our cache be much faster than a more general cache might be.  This is important, because this
            // is a *very* hot code path for many WPF and [....] apps.
            //
            Type type = this.GetType();
            if (s_cachedPreparedType1 != type &&
                s_cachedPreparedType2 != type &&
                s_cachedPreparedType3 != type &&
                s_cachedPreparedType4 != type &&
                s_cachedPreparedType5 != type)
            {
                RuntimeHelpers.PrepareDelegate(new WaitDelegate(this.Wait));

                if (s_cachedPreparedType1 == null)      s_cachedPreparedType1  = type;
                else if (s_cachedPreparedType2 == null) s_cachedPreparedType2  = type;
                else if (s_cachedPreparedType3 == null) s_cachedPreparedType3  = type;
                else if (s_cachedPreparedType4 == null) s_cachedPreparedType4  = type;
                else if (s_cachedPreparedType5 == null) s_cachedPreparedType5  = type;
            }

            _props |= SynchronizationContextProperties.RequireWaitNotification;
        }

        public bool IsWaitNotificationRequired()
        {
            return ((_props & SynchronizationContextProperties.RequireWaitNotification) != 0);  
        }
#endif

    
        public virtual void Send(SendOrPostCallback d, Object state)
        {
            d(state);
        }

        public virtual void Post(SendOrPostCallback d, Object state)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(d), state);
        }

        
        /// <summary>
        ///     Optional override for subclasses, for responding to notification that operation is starting.
        /// </summary>
        public virtual void OperationStarted()
        {
        }

        /// <summary>
        ///     Optional override for subclasses, for responding to notification that operation has completed.
        /// </summary>
        public virtual void OperationCompleted()
        {
        }

#if FEATURE_SYNCHRONIZATIONCONTEXT_WAIT
        // Method called when the CLR does a wait operation 
        [System.Security.SecurityCritical]  // auto-generated_required
        [CLSCompliant(false)]
        [PrePrepareMethod]
        public virtual int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException("waitHandles");
            }
            Contract.EndContractBlock();
            return WaitHelper(waitHandles, waitAll, millisecondsTimeout);
        }
                                
        // Static helper to which the above method can delegate to in order to get the default 
        // COM behavior.
        [System.Security.SecurityCritical]  // auto-generated_required
        [CLSCompliant(false)]
        [PrePrepareMethod]
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]       
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected static extern int WaitHelper(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);
#endif

        // set SynchronizationContext on the current thread
        [System.Security.SecurityCritical]  // auto-generated_required
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public static void SetSynchronizationContext(SynchronizationContext syncContext)
        {
            ExecutionContext ec = Thread.CurrentThread.GetMutableExecutionContext();
            ec.SynchronizationContext = syncContext;
            ec.SynchronizationContextNoFlow = syncContext;
        }

#if FEATURE_CORECLR
        //
        // This is a framework-internal method for Jolt's use.  The problem is that SynchronizationContexts set inside of a reverse p/invoke
        // into an AppDomain are not persisted in that AppDomain; the next time the same thread calls into the same AppDomain,
        // the [....] context will be null.  For Silverlight, this means that it's impossible to persist a [....] context on the UI thread,
        // since Jolt is constantly transitioning in and out of each control's AppDomain on that thread.
        //
        // So for Jolt we will track a special thread-static context, which *will* persist across calls from Jolt, and if the thread does not 
        // have a [....] context set in its execution context we'll use the thread-static context instead.
        //
        // This will break any future work that requires SynchronizationContext.Current to be in [....] with the value
        // stored in a thread's ExecutionContext (wait notifications being one such example).  If that becomes a problem, we will
        // need to rework this mechanism (which is one reason it's not being exposed publically).
        //

        [ThreadStatic]
        private static SynchronizationContext s_threadStaticContext;

#if FEATURE_LEGACYNETCF
		//
		// NetCF had a bug where SynchronizationContext.SetThreadStaticContext would set the SyncContext for every thread in the process.  
		// This was because they stored the value in a regular static field (NetCF has no support for ThreadStatic fields).  This was fixed in 
		// Mango, but some apps built against pre-Mango WP7 do depend on the broken behavior.  So for those apps we need an AppDomain-wide static
		// to hold whatever context was last set on any thread.
		//
        private static SynchronizationContext s_appDomainStaticContext;
#endif

        [System.Security.SecurityCritical]
#if FEATURE_LEGACYNETCF
        public static void SetThreadStaticContext(SynchronizationContext syncContext)
#else
        internal static void SetThreadStaticContext(SynchronizationContext syncContext)
#endif
        {
#if FEATURE_LEGACYNETCF
			//
			// If this is a pre-Mango Windows Phone app, we need to set the SC for *all* threads to match the old NetCF behavior.
			//
            if (CompatibilitySwitches.IsAppEarlierThanWindowsPhoneMango)
                s_appDomainStaticContext = syncContext;
            else
#endif
                s_threadStaticContext = syncContext;
        }
#endif

        // Get the current SynchronizationContext on the current thread
        public static SynchronizationContext Current 
        {
#if !FEATURE_CORECLR
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
            get      
            {
                return Thread.CurrentThread.GetExecutionContextReader().SynchronizationContext ?? GetThreadLocalContext();
            }
        }

        // Get the last SynchronizationContext that was set explicitly (not flowed via ExecutionContext.Capture/Run)        
        internal static SynchronizationContext CurrentNoFlow
        {
            [FriendAccessAllowed]
            get
            {
                return Thread.CurrentThread.GetExecutionContextReader().SynchronizationContextNoFlow ?? GetThreadLocalContext();
            }
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        private static SynchronizationContext GetThreadLocalContext()
        {
            SynchronizationContext context = null;
            
#if FEATURE_CORECLR
#if FEATURE_LEGACYNETCF
            if (CompatibilitySwitches.IsAppEarlierThanWindowsPhoneMango)
                context = s_appDomainStaticContext;
            else
#endif //FEATURE_LEGACYNETCF
                context = s_threadStaticContext;
#endif //FEATURE_CORECLR

#if FEATURE_APPX
            if (context == null && Environment.IsWinRTSupported)
                context = GetWinRTContext();
#endif

            return context;
        }

#if FEATURE_APPX
        [SecuritySafeCritical]
        private static SynchronizationContext GetWinRTContext()
        {
            Contract.Assert(Environment.IsWinRTSupported);

            // Temporary hack to avoid loading a bunch of DLLs in every managed process.
            // This disables this feature for non-AppX processes that happen to use CoreWindow/CoreDispatcher,
            // which is not what we want.
            if (!AppDomain.IsAppXModel())
                return null;

            //
            // We call into the VM to get the dispatcher.  This is because:
            //
            //  a) We cannot call the WinRT APIs directly from mscorlib, because we don't have the fancy projections here.
            //  b) We cannot call into System.Runtime.WindowsRuntime here, because we don't want to load that assembly
            //     into processes that don't need it (for performance reasons).
            //
            // So, we check the VM to see if the current thread has a dispatcher; if it does, we pass that along to
            // System.Runtime.WindowsRuntime to get a corresponding SynchronizationContext.
            //
            object dispatcher = GetWinRTDispatcherForCurrentThread();
            if (dispatcher != null)
                return GetWinRTSynchronizationContextFactory().Create(dispatcher);

            return null;
        }

        [SecurityCritical]
        static WinRTSynchronizationContextFactoryBase s_winRTContextFactory;

        [SecurityCritical]
        private static WinRTSynchronizationContextFactoryBase GetWinRTSynchronizationContextFactory()
        {
            //
            // Since we can't directly reference System.Runtime.WindowsRuntime from mscorlib, we have to get the factory via reflection.
            // It would be better if we could just implement WinRTSynchronizationContextFactory in mscorlib, but we can't, because
            // we can do very little with WinRT stuff in mscorlib.
            //
            WinRTSynchronizationContextFactoryBase factory = s_winRTContextFactory;
            if (factory == null)
            {
                Type factoryType = Type.GetType("System.Threading.WinRTSynchronizationContextFactory, " + AssemblyRef.SystemRuntimeWindowsRuntime, true);
                s_winRTContextFactory = factory = (WinRTSynchronizationContextFactoryBase)Activator.CreateInstance(factoryType, true);
            }
            return factory;
        }

        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Interface)]
        private static extern object GetWinRTDispatcherForCurrentThread();
#endif //FEATURE_APPX


        // helper to Clone this SynchronizationContext, 
        public virtual SynchronizationContext CreateCopy()
        {
            // the CLR dummy has an empty clone function - no member data
            return new SynchronizationContext();
        }

#if FEATURE_SYNCHRONIZATIONCONTEXT_WAIT
        [System.Security.SecurityCritical]  // auto-generated
        private static int InvokeWaitMethodHelper(SynchronizationContext syncContext, IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return syncContext.Wait(waitHandles, waitAll, millisecondsTimeout);
        }
#endif
    }
}
