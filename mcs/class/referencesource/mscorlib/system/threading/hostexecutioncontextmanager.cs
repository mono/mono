using System.Diagnostics.Contracts;
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//
// <OWNER>Microsoft</OWNER>
/*============================================================
**
** Class:  ExecutionContext
**
**
** Purpose: Capture Host execution  context for a thread
**
** 
===========================================================*/
namespace System.Threading
{    
    using System.Security;
    using System.Runtime.Remoting;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Runtime.InteropServices;



    internal class HostExecutionContextSwitcher
    {
        internal ExecutionContext            executionContext;
        internal HostExecutionContext        previousHostContext;
        internal HostExecutionContext        currentHostContext;

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        static public void Undo(Object switcherObject)
        {
            if (switcherObject == null) 
               return;
            // otherwise call the host
            HostExecutionContextManager hostMgr = HostExecutionContextManager.GetCurrentHostExecutionContextManager();
            if (hostMgr != null)
            {
                hostMgr.Revert(switcherObject);    
            }
        }        
    }
    


    public class HostExecutionContext : IDisposable
    {        
        private Object state;
        
        protected internal Object State {
            get {            
                return state;
            }
            set {
                state = value;
            }
        }
        
        public HostExecutionContext() 
        {
        }            

        public HostExecutionContext(Object state)
        {
            this.state = state;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public virtual HostExecutionContext CreateCopy()
        {
                     Object newState = state;
            if (state is IUnknownSafeHandle)
            {                            
                            // Clone the IUnknown handle
                            newState = ((IUnknownSafeHandle)state).Clone();
            }                    
            return new HostExecutionContext(state);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            return;
        }
    }

    [System.Security.SecurityCritical]  // auto-generated
    internal class IUnknownSafeHandle : SafeHandle
    {
        public IUnknownSafeHandle() : base(IntPtr.Zero, true)
        {
        }
        
        public override bool IsInvalid {
            [System.Security.SecurityCritical]
            get { return handle == IntPtr.Zero; }
            }

        [System.Security.SecurityCritical]
        override protected bool ReleaseHandle()
        {
            HostExecutionContextManager.ReleaseHostSecurityContext(this.handle);
            return true;
        }

             internal Object Clone()
             {
                IUnknownSafeHandle unkSafeHandleCloned = new IUnknownSafeHandle();                                
                // call into the Hosting API to CLONE the host context                            
                // stores the output IUnknown in the safehandle,
                if (!IsInvalid)
                {
                    HostExecutionContextManager.CloneHostSecurityContext(this, unkSafeHandleCloned);    
                }
                return unkSafeHandleCloned;
             }
    }    



    public class HostExecutionContextManager
    {
        private static volatile bool _fIsHostedChecked;
        private static volatile bool _fIsHosted;
        private static HostExecutionContextManager _hostExecutionContextManager;

              [System.Security.SecurityCritical]  // auto-generated
              [ResourceExposure(ResourceScope.None)]
              [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
              [MethodImplAttribute(MethodImplOptions.InternalCall)]
            static private extern bool HostSecurityManagerPresent();

              [System.Security.SecurityCritical]  // auto-generated
              [ResourceExposure(ResourceScope.None)]
              [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static internal extern int ReleaseHostSecurityContext(IntPtr context);

              [System.Security.SecurityCritical]  // auto-generated
              [ResourceExposure(ResourceScope.None)]
              [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static internal extern int CloneHostSecurityContext(SafeHandle context, SafeHandle clonedContext);
        
               [System.Security.SecurityCritical]  // auto-generated
               [ResourceExposure(ResourceScope.None)]
              [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static private extern int CaptureHostSecurityContext(SafeHandle capturedContext);

              [System.Security.SecurityCritical]  // auto-generated
              [ResourceExposure(ResourceScope.None)]
              [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static private extern int SetHostSecurityContext(SafeHandle context, bool fReturnPrevious, SafeHandle prevContext);

        [System.Security.SecurityCritical]  // auto-generated
        internal static bool CheckIfHosted()
        {
            if (!_fIsHostedChecked)
            {
                _fIsHosted = HostSecurityManagerPresent();
                _fIsHostedChecked = true;
            }
            return _fIsHosted;
        }
        
         // capture Host SecurityContext
        [System.Security.SecuritySafeCritical]  // auto-generated
        public virtual  HostExecutionContext Capture()
        {
            HostExecutionContext context =  null;
            // check if we are hosted
            if (CheckIfHosted())
            {
                IUnknownSafeHandle unkSafeHandle = new IUnknownSafeHandle();    
                            context = new HostExecutionContext(unkSafeHandle);
                            
                // call into the Hosting API to capture the host context                            
                // stores the output IUnknown in the safehandle,
                CaptureHostSecurityContext(unkSafeHandle);    
            }

            // otherwise
            return context;
            
        }
        // Set Host SecurityContext
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object SetHostExecutionContext(HostExecutionContext hostExecutionContext)
        {            
            if (hostExecutionContext == null) 
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
            }
            Contract.EndContractBlock();
            
            HostExecutionContextSwitcher switcher = new HostExecutionContextSwitcher();    
            ExecutionContext currentExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
                     
            switcher.executionContext = currentExecutionContext;    
            switcher.currentHostContext = hostExecutionContext;     
                     switcher.previousHostContext = null;
            
            if (CheckIfHosted())
            {                        
                if (hostExecutionContext.State is IUnknownSafeHandle)
                {    
                                    // setup the previous unknown handle
                                   IUnknownSafeHandle unkPrevSafeHandle = new IUnknownSafeHandle();
                            switcher.previousHostContext = new HostExecutionContext(unkPrevSafeHandle);    

                                    // get the current handle
                    IUnknownSafeHandle unkSafeHandle  = (IUnknownSafeHandle)hostExecutionContext.State;
                    
                                   // call into the Hosting API to set the host context
                                   // second arg indicates whether we want to retrieve the previous context
                      SetHostSecurityContext(unkSafeHandle,true,unkPrevSafeHandle);                                   
                }                                                                                
            }

                    // store the current HostExecutionContext in the ExecutionContext.
            currentExecutionContext.HostExecutionContext = hostExecutionContext;                
            
            return switcher;            
        }
        
        // this method needs to be reliable
        [System.Security.SecurityCritical]  // auto-generated_required
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public virtual void Revert(Object previousState)
        {
            HostExecutionContextSwitcher hostContextSwitcher = previousState as HostExecutionContextSwitcher;
            if (hostContextSwitcher == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotOverrideSetWithoutRevert"));
            }
            
            // check Undo is happening on the correct thread
            ExecutionContext executionContext = Thread.CurrentThread.GetMutableExecutionContext();
            
            if (executionContext != hostContextSwitcher.executionContext)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
            }
            hostContextSwitcher.executionContext = null; // Make sure switcher cannot be re-used.

            HostExecutionContext revertFromHostContext = executionContext.HostExecutionContext;
            // if the current host context is not the same as the one in the switcher, then revert is being called out of order
            if (revertFromHostContext != hostContextSwitcher.currentHostContext)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
            }
            
            // get the previous host context
            HostExecutionContext revertToHostContext = hostContextSwitcher.previousHostContext;
        
            // now check if we are hosted and revert the context in the host
            if (CheckIfHosted())
            {
                // try restore the previous context as the current context
                if (revertToHostContext != null && revertToHostContext.State is IUnknownSafeHandle)
                {
                    IUnknownSafeHandle unkprevSafeHandle = (IUnknownSafeHandle)revertToHostContext.State;        
                    // call into the Hosting API to set the host context 
                    SetHostSecurityContext(unkprevSafeHandle, false, null); 
                }                                                                      
            }
            
            //restore the previous host context in the executioncontext
            executionContext.HostExecutionContext = revertToHostContext;

        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static HostExecutionContext CaptureHostExecutionContext()
        {
            HostExecutionContext hostContext = null;
            // capture the host execution context
            HostExecutionContextManager hostMgr = HostExecutionContextManager.GetCurrentHostExecutionContextManager();
            if (hostMgr != null)
            {
                hostContext = hostMgr.Capture();
            }
            return hostContext;
        }        

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object SetHostExecutionContextInternal(HostExecutionContext hostContext)
        {
            HostExecutionContextManager hostMgr = HostExecutionContextManager.GetCurrentHostExecutionContextManager();
            Object switcher = null;
            if (hostMgr != null) 
            {  
                switcher = hostMgr.SetHostExecutionContext(hostContext);
                // 
            }
            return switcher;
        }

              // retun the HostExecutionContextManager for the current AppDomain
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static HostExecutionContextManager GetCurrentHostExecutionContextManager() 
        {
            // this is called during AppDomainManager initialization, this is a thread safe place
            // to setup the HostExecutionContextManager for the current AppDomain
            AppDomainManager mgr = AppDomainManager.CurrentAppDomainManager;
            if (mgr != null)
                return mgr.HostExecutionContextManager;
            return null;
        }        

              // retun the HostExecutionContextManager for the current AppDomain
        internal static HostExecutionContextManager GetInternalHostExecutionContextManager() 
        {
            if (_hostExecutionContextManager == null) {
                // setup the HostExecutionContextManager for the current AppDomain
                Contract.Assert(_hostExecutionContextManager == null, "HostExecutionContextManager should be null");
                _hostExecutionContextManager = new HostExecutionContextManager();
            }
            return _hostExecutionContextManager;
        }
    }
}
