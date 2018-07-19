// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: CompressedStack
**
** <OWNER>Microsoft</OWNER>
** <OWNER>Microsoft</OWNER>
**
** Purpose: Managed wrapper for the security stack compression implementation
**
=============================================================================*/

namespace System.Threading
{
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
#if FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ExceptionServices;
#endif // FEATURE_CORRUPTING_EXCEPTIONS
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Reflection;
    using System.Collections;    
    using System.Threading;    
    using System.Runtime.Serialization;
    using System.Diagnostics.Contracts;


    internal struct CompressedStackSwitcher: IDisposable 
    {
        internal CompressedStack curr_CS;
        internal CompressedStack prev_CS;
        internal IntPtr prev_ADStack;

        
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is CompressedStackSwitcher))
                return false;
            CompressedStackSwitcher sw = (CompressedStackSwitcher)obj;
            return (this.curr_CS == sw.curr_CS && this.prev_CS == sw.prev_CS && this.prev_ADStack == sw.prev_ADStack);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(CompressedStackSwitcher c1, CompressedStackSwitcher c2) 
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(CompressedStackSwitcher c1, CompressedStackSwitcher c2) 
        {
            return !c1.Equals(c2);
        }

        [System.Security.SecuritySafeCritical] // overrides public transparent member
        public void Dispose()
        {
            Undo();
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal bool UndoNoThrow()
        {
            try
            {
                Undo();
            }
            catch
            {
                return false;
            }
            return true;
        }


        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void Undo()
        {
            if (curr_CS == null && prev_CS == null)
                return;
            if (prev_ADStack != (IntPtr)0)
                CompressedStack.RestoreAppDomainStack(prev_ADStack);
            CompressedStack.SetCompressedStackThread(prev_CS);

            prev_CS = null;
            curr_CS = null;
            prev_ADStack = (IntPtr)0;
        }
    }

     [System.Security.SecurityCritical]  // auto-generated
     internal class SafeCompressedStackHandle : SafeHandle
     {
        public SafeCompressedStackHandle() : base(IntPtr.Zero, true)
        {       
        }

        public override bool IsInvalid {
            [System.Security.SecurityCritical]
            get { return handle == IntPtr.Zero; }
        }

        [System.Security.SecurityCritical]
        override protected bool ReleaseHandle()
        {
            CompressedStack.DestroyDelayedCompressedStack(handle);
            handle = IntPtr.Zero;
            return true;
        }
     }   



     [Serializable]
    public sealed class CompressedStack:ISerializable
    {

        private volatile PermissionListSet m_pls;
        [System.Security.SecurityCritical] // auto-generated
        private volatile SafeCompressedStackHandle m_csHandle;
        private bool m_canSkipEvaluation = false;

        internal bool CanSkipEvaluation
        {
            get
            {
                return m_canSkipEvaluation;
            }
            private set
            {
                m_canSkipEvaluation = value;
            }
        }

        internal PermissionListSet PLS
        {
            get
            {
                return m_pls;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal CompressedStack( SafeCompressedStackHandle csHandle )
        {
            m_csHandle = csHandle;            
        }

        [System.Security.SecurityCritical]  // auto-generated
        private CompressedStack(SafeCompressedStackHandle csHandle, PermissionListSet pls)
        {
            this.m_csHandle = csHandle;
            this.m_pls = pls;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info==null) 
                throw new ArgumentNullException("info");
            Contract.EndContractBlock();
            CompleteConstruction(null);
            info.AddValue("PLS", this.m_pls);
        }

        private CompressedStack(SerializationInfo info, StreamingContext context) 
        {
            this.m_pls = (PermissionListSet)info.GetValue("PLS", typeof(PermissionListSet));
        }

        internal SafeCompressedStackHandle CompressedStackHandle
        {
            [System.Security.SecurityCritical]  // auto-generated
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return m_csHandle;
            }
            [System.Security.SecurityCritical]  // auto-generated
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            private set
            {
                m_csHandle = value;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static CompressedStack GetCompressedStack()
        {
            // This is a Capture()
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return CompressedStack.GetCompressedStack(ref stackMark);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static CompressedStack GetCompressedStack(ref StackCrawlMark stackMark)
        {
            CompressedStack cs;
            CompressedStack innerCS = null;
            if (CodeAccessSecurityEngine.QuickCheckForAllDemands())
            {
                cs = new CompressedStack(null);
                cs.CanSkipEvaluation = true;
            }
            else if (CodeAccessSecurityEngine.AllDomainsHomogeneousWithNoStackModifiers())
            {
                // if all AppDomains on the stack are homogeneous, we don't need to walk the stack
                // however, we do need to capture the AppDomain stack.
                cs = new CompressedStack(GetDelayedCompressedStack(ref stackMark, false));
                cs.m_pls = PermissionListSet.CreateCompressedState_HG();
            }
            else
            {
                // regular stackwalking case
                // We want this to complete without ThreadAborts - if we're in a multiple AD callstack and an intermediate AD gets unloaded,
                // preventing TAs here prevents a race condition where a SafeCompressedStackHandle is created to a DCS belonging to an AD that's 
                // gone away
                cs = new CompressedStack(null);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    // Empty try block to ensure no ThreadAborts in the finally block
                }
                finally
                {
                    
                    cs.CompressedStackHandle = GetDelayedCompressedStack(ref stackMark, true);
                    if (cs.CompressedStackHandle != null && IsImmediateCompletionCandidate(cs.CompressedStackHandle, out innerCS))
                    {
                        try
                        {
                            cs.CompleteConstruction(innerCS);
                        }
                        finally
                        {
                            DestroyDCSList(cs.CompressedStackHandle);
                        }
                    }
                }
            }
            return cs;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static CompressedStack Capture()
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return GetCompressedStack(ref stackMark);
        }

        // This method is special from a security perspective - the VM will not allow a stack walk to
        // continue past the call to CompressedStack.Run.  If you change the signature to this method, or
        // provide an alternate way to do a CompressedStack.Run make sure to update
        // SecurityStackWalk::IsSpecialRunFrame in the VM to search for the new method.
        [System.Security.SecurityCritical]  // auto-generated_required
        [DynamicSecurityMethodAttribute()]
        public static void Run(CompressedStack compressedStack, ContextCallback callback, Object state)
        {
            
            if (compressedStack == null )
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamNull"),"compressedStack");
            }
            Contract.EndContractBlock();
            if (cleanupCode == null)
            {
                tryCode = new RuntimeHelpers.TryCode(runTryCode);
                cleanupCode = new RuntimeHelpers.CleanupCode(runFinallyCode);
            }
            
            CompressedStackRunData runData = new CompressedStackRunData(compressedStack, callback, state);
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, runData);
        }
            
        internal class CompressedStackRunData
        {
            internal CompressedStack cs;
            internal ContextCallback callBack;
            internal Object state;
            internal CompressedStackSwitcher cssw;
            internal CompressedStackRunData(CompressedStack cs, ContextCallback cb, Object state)
            {
                this.cs = cs;
                this.callBack = cb;
                this.state = state;
                this.cssw = new CompressedStackSwitcher();
            }
        }
        [System.Security.SecurityCritical]  // auto-generated
        static internal void runTryCode(Object userData)
        {
            CompressedStackRunData rData = (CompressedStackRunData) userData;
            rData.cssw = SetCompressedStack(rData.cs, GetCompressedStackThread());
            rData.callBack(rData.state);

        }

        [System.Security.SecurityCritical]  // auto-generated
        [PrePrepareMethod]
        static internal void runFinallyCode(Object userData, bool exceptionThrown)
        {
            CompressedStackRunData rData = (CompressedStackRunData) userData;
            rData.cssw.Undo();
        }

        static internal volatile RuntimeHelpers.TryCode tryCode;
        static internal volatile RuntimeHelpers.CleanupCode cleanupCode;

        
        [System.Security.SecurityCritical]  // auto-generated
#if FEATURE_CORRUPTING_EXCEPTIONS
        [HandleProcessCorruptedStateExceptions] // 
#endif // FEATURE_CORRUPTING_EXCEPTIONS
        internal static CompressedStackSwitcher SetCompressedStack(CompressedStack cs, CompressedStack prevCS)
        {
            CompressedStackSwitcher cssw = new CompressedStackSwitcher();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                // Order is important in this block.
                // Also, we dont want any THreadAborts happening when we try to set it
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    // Empty try block to ensure no ThreadAborts in the finally block
                }
                finally
                {
                    // SetCompressedStackThread can throw - only if it suceeds we shd update the switcher and overrides
                    SetCompressedStackThread(cs);
                    cssw.prev_CS = prevCS;
                    cssw.curr_CS = cs;
                    cssw.prev_ADStack = SetAppDomainStack(cs);                
                }
            }
            catch
            {
                cssw.UndoNoThrow();
                throw; // throw the original exception
            }
            return cssw;
        }
        

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ComVisible(false)]
        public CompressedStack CreateCopy()
        {
            return new CompressedStack(this.m_csHandle, this.m_pls);
        }
                
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static IntPtr SetAppDomainStack(CompressedStack cs)
        {
            //Update the AD Stack on the thread and return the previous AD Stack
            return Thread.CurrentThread.SetAppDomainStack((cs == null ? null:cs.CompressedStackHandle)); 
        }

        
        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void RestoreAppDomainStack(IntPtr appDomainStack)
        {
            Thread.CurrentThread.RestoreAppDomainStack(appDomainStack); //Restore the previous AD Stack
        }

        [SecurityCritical]
        internal static CompressedStack GetCompressedStackThread()
        {
            return Thread.CurrentThread.GetExecutionContextReader().SecurityContext.CompressedStack;
        }

        [SecurityCritical]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static void SetCompressedStackThread(CompressedStack cs)
        {
            Thread currentThread = Thread.CurrentThread;
            if (currentThread.GetExecutionContextReader().SecurityContext.CompressedStack != cs)
            {
                ExecutionContext ec = currentThread.GetMutableExecutionContext();
                if (ec.SecurityContext != null)
                    ec.SecurityContext.CompressedStack = cs;
                else if (cs != null)
                {
                    SecurityContext sc = new SecurityContext();
                    sc.CompressedStack = cs;
                    ec.SecurityContext = sc;
                }
            }
        }
        

        [System.Security.SecurityCritical]  // auto-generated
        internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            CompleteConstruction(null);

            if (PLS == null)
                return SecurityRuntime.StackHalt;
            else
            {
                PLS.CheckDemand(demand, permToken, rmh);
                return SecurityRuntime.StackHalt; //  CS demand check always terminates the stackwalk
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal bool CheckDemandNoHalt(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            CompleteConstruction(null);

            if (PLS == null)
                return SecurityRuntime.StackContinue;
            else
            {
                return PLS.CheckDemand(demand, permToken, rmh);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal bool CheckSetDemand(PermissionSet pset , RuntimeMethodHandleInternal rmh)
        {
            CompleteConstruction(null);

            if (PLS == null)
                return SecurityRuntime.StackHalt;
            else
                return PLS.CheckSetDemand(pset, rmh);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal bool CheckSetDemandWithModificationNoHalt(PermissionSet pset, out PermissionSet alteredDemandSet, RuntimeMethodHandleInternal rmh)
        {
            alteredDemandSet = null;
            CompleteConstruction(null);

            if (PLS == null)
                return SecurityRuntime.StackContinue;
            else
                return PLS.CheckSetDemandWithModification(pset, out alteredDemandSet, rmh);
        }

        /// <summary>
        ///     Demand which succeeds if either a set of special permissions or a permission set is granted
        ///     to the call stack
        /// </summary>
        /// <param name="flags">set of flags to check (See PermissionType)</param>
        /// <param name="grantSet">alternate permission set to check</param>
        [System.Security.SecurityCritical]  // auto-generated
        internal void DemandFlagsOrGrantSet(int flags, PermissionSet grantSet)
        {
            CompleteConstruction(null);
            if (PLS == null)
                return;

            PLS.DemandFlagsOrGrantSet(flags, grantSet);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void GetZoneAndOrigin(ArrayList zoneList, ArrayList originList, PermissionToken zoneToken, PermissionToken originToken)
        {
            CompleteConstruction(null);
            if (PLS != null)
                PLS.GetZoneAndOrigin(zoneList,originList,zoneToken,originToken);
            return;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void CompleteConstruction(CompressedStack innerCS)
        {
            if (PLS != null)
                return;
            PermissionListSet pls = PermissionListSet.CreateCompressedState(this, innerCS);
            lock (this)
            {
                if (PLS == null)
                    m_pls = pls;
            }
        }
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal extern static SafeCompressedStackHandle GetDelayedCompressedStack(ref StackCrawlMark stackMark,
                                                                                   bool walkStack);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void DestroyDelayedCompressedStack( IntPtr unmanagedCompressedStack );
       
    [System.Security.SecurityCritical]  // auto-generated
    [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]       
        internal extern static void DestroyDCSList( SafeCompressedStackHandle compressedStack );
        

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static int GetDCSCount(SafeCompressedStackHandle compressedStack);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal extern static bool IsImmediateCompletionCandidate(SafeCompressedStackHandle compressedStack, out CompressedStack innerCS);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static DomainCompressedStack GetDomainCompressedStack(SafeCompressedStackHandle compressedStack, int index);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void GetHomogeneousPLS(PermissionListSet hgPLS);

        
    }

    //**********************************************************
    // New Implementation of CompressedStack creation/demand eval - NewCompressedStack/DomainCompressedStack
    //**********************************************************
    [Serializable]
    internal sealed class DomainCompressedStack
    {
        // Managed equivalent of DomainCompressedStack - used to perform demand evaluation
        private PermissionListSet m_pls;
        // Did we terminate construction on this DCS and therefore, should we terminate construction on the rest of the CS?
        private bool m_bHaltConstruction;


        // CompresedStack interacts with this class purely through the three properties marked internal
        // Zone, Origin, AGRList.
        internal PermissionListSet PLS 
        {
            get
            {
                 return m_pls;
            }
        }

        internal bool ConstructionHalted 
        {
            get
            {
                 return m_bHaltConstruction;
            }
        }



        // Called from the VM only.
        [System.Security.SecurityCritical]  // auto-generated
        private static DomainCompressedStack CreateManagedObject(IntPtr unmanagedDCS)
        {
            DomainCompressedStack newDCS = new DomainCompressedStack();
            newDCS.m_pls = PermissionListSet.CreateCompressedState(unmanagedDCS, out newDCS.m_bHaltConstruction);
            // return the created object
            return newDCS;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static int GetDescCount(IntPtr dcs);
        
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static void GetDomainPermissionSets(IntPtr dcs, out PermissionSet granted, out PermissionSet refused);

        // returns true if the descriptor is a FrameSecurityDescriptor
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool GetDescriptorInfo(IntPtr dcs, int index, out PermissionSet granted, out PermissionSet refused, out Assembly assembly, out FrameSecurityDescriptor fsd);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static bool IgnoreDomain(IntPtr dcs);
    }      
    
}
