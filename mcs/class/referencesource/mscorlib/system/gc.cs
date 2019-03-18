// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  GC
**
**
** Purpose: Exposes features of the Garbage Collector through
** the class libraries.  This is a class which cannot be
** instantiated.
**
**
===========================================================*/
namespace System {
    //This class only static members and doesn't require the serializable keyword.

    using System;
#if !MONO
    using System.Security.Permissions;
#endif
    using System.Reflection;
    using System.Security;
    using System.Threading;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

    [Serializable]
    public enum GCCollectionMode
    {
        Default = 0,
        Forced = 1,
        Optimized = 2
    }

    // !!!!!!!!!!!!!!!!!!!!!!!
    // make sure you change the def in vm\gc.h 
    // if you change this!
    [Serializable]
    internal enum InternalGCCollectionMode
    {
        NonBlocking = 0x00000001,
        Blocking = 0x00000002,
        Optimized = 0x00000004,
        Compacting = 0x00000008,
    }

    // !!!!!!!!!!!!!!!!!!!!!!!
    // make sure you change the def in vm\gc.h 
    // if you change this!
    [Serializable]
    public enum GCNotificationStatus
    {
        Succeeded = 0,
        Failed = 1,
        Canceled = 2,
        Timeout = 3,
        NotApplicable = 4
    }

    public static partial class GC 
    {
#if MONO
        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        extern static int GetCollectionCount (int generation);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        extern static int GetMaxGeneration ();

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        extern static void InternalCollect (int generation);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        extern static void RecordPressure (long bytesAllocated);

        // TODO: Move following to ConditionalWeakTable
        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        internal extern static void register_ephemeron_array (Ephemeron[] array);

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        extern static object get_ephemeron_tombstone ();

        internal static readonly object EPHEMERON_TOMBSTONE = get_ephemeron_tombstone ();

        internal static void GetMemoryInfo(out uint highMemLoadThreshold,
                                                  out ulong totalPhysicalMem,
                                                  out uint lastRecordedMemLoad,
                                                  // The next two are size_t
                                                  out UIntPtr lastRecordedHeapSize,
                                                  out UIntPtr lastRecordedFragmentation)
        {
            // TODO: Fill properly
            highMemLoadThreshold = 0;
            totalPhysicalMem = ulong.MaxValue;
            lastRecordedMemLoad = 0;
            lastRecordedHeapSize = UIntPtr.Zero;
            lastRecordedFragmentation = UIntPtr.Zero;
        }

        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        public static extern long GetAllocatedBytesForCurrentThread ();
#else
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int GetGCLatencyMode();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SetGCLatencyMode(int newLatencyMode);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern int _StartNoGCRegion(long totalSize, bool lohSizeKnown, long lohSize, bool disallowFullBlockingGC);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern int _EndNoGCRegion();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int GetLOHCompactionMode();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SetLOHCompactionMode(int newLOHCompactionyMode);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int GetGenerationWR(IntPtr handle);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern long GetTotalMemory();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _Collect(int generation, int mode);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int GetMaxGeneration();
    
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern int _CollectionCount (int generation, int getSpecialGCCount);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool IsServerGC();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        private static extern void _AddMemoryPressure(UInt64 bytesAllocated);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        private static extern void _RemoveMemoryPressure(UInt64 bytesAllocated);
#endif
        
        [System.Security.SecurityCritical]  // auto-generated_required
        public static void AddMemoryPressure (long bytesAllocated) {
            if( bytesAllocated <= 0) {
                throw new ArgumentOutOfRangeException("bytesAllocated", 
                        Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }

            if( (4 == IntPtr.Size) && (bytesAllocated > Int32.MaxValue) ) {
                throw new ArgumentOutOfRangeException("pressure", 
                        Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
            }
            Contract.EndContractBlock();

#if MONO
            RecordPressure (bytesAllocated);
#else
            _AddMemoryPressure((ulong)bytesAllocated);
#endif
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public static void RemoveMemoryPressure (long bytesAllocated) {
            if( bytesAllocated <= 0) {
                throw new ArgumentOutOfRangeException("bytesAllocated", 
                        Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }

            if( (4 == IntPtr.Size)  && (bytesAllocated > Int32.MaxValue) ) {
                throw new ArgumentOutOfRangeException("bytesAllocated", 
                        Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
            }
            Contract.EndContractBlock();

#if MONO
            RecordPressure (-bytesAllocated);
#else
            _RemoveMemoryPressure((ulong) bytesAllocated);
#endif
        }


        // Returns the generation that obj is currently in.
        //
#if FEATURE_LEGACYNETCF
        [System.Security.SecurityCritical]
#else
        [System.Security.SecuritySafeCritical]  // auto-generated
#endif
#if !MONO
        [ResourceExposure(ResourceScope.None)]
#endif
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int GetGeneration(Object obj);

    
        // Forces a collection of all generations from 0 through Generation.
        //
#if FEATURE_LEGACYNETCF
        [System.Security.SecurityCritical]  // auto-generated
#endif
        public static void Collect(int generation) {
            Collect(generation, GCCollectionMode.Default);
        }
    
        // Garbage Collect all generations.
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void Collect() {
#if MONO
            InternalCollect (MaxGeneration);
#else
            //-1 says to GC all generations.
            _Collect(-1, (int)InternalGCCollectionMode.Blocking);
#endif
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void Collect(int generation, GCCollectionMode mode) 
        {
            Collect(generation, mode, true);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void Collect(int generation, GCCollectionMode mode, bool blocking) 
        {
            Collect(generation, mode, blocking, false);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void Collect(int generation, GCCollectionMode mode, bool blocking, bool compacting)
        {
            if (generation<0) 
            {
                throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }

            if ((mode < GCCollectionMode.Default) || (mode > GCCollectionMode.Optimized))
            {
                throw new ArgumentOutOfRangeException("mode", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }

            Contract.EndContractBlock();

            int iInternalModes = 0;

            if (mode == GCCollectionMode.Optimized)
            {
                iInternalModes |= (int)InternalGCCollectionMode.Optimized;
            }

            if (compacting)
                iInternalModes |= (int)InternalGCCollectionMode.Compacting;

            if (blocking)
            {
                iInternalModes |= (int)InternalGCCollectionMode.Blocking;
            }
            else if (!compacting)
            {
                iInternalModes |= (int)InternalGCCollectionMode.NonBlocking;
            }

#if MONO
            InternalCollect (generation);
#else
            _Collect(generation, iInternalModes);
#endif
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int CollectionCount (int generation) 
        {
            if (generation<0) 
            {
                throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            Contract.EndContractBlock();

#if MONO
            return GetCollectionCount(generation);
#else
            return _CollectionCount(generation, 0);
#endif
        }

#if !MONO
        // pass in true to get the BGC or FGC count.
        [System.Security.SecuritySafeCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int CollectionCount (int generation, bool getSpecialGCCount) 
        {
            if (generation<0) 
            {
                throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            Contract.EndContractBlock();
            return _CollectionCount(generation, (getSpecialGCCount ? 1 : 0));
        }
#endif
        
        // This method DOES NOT DO ANYTHING in and of itself.  It's used to 
        // prevent a finalizable object from losing any outstanding references 
        // a touch too early.  The JIT is very aggressive about keeping an 
        // object's lifetime to as small a window as possible, to the point
        // where a 'this' pointer isn't considered live in an instance method
        // unless you read a value from the instance.  So for finalizable 
        // objects that store a handle or pointer and provide a finalizer that 
        // cleans them up, this can cause subtle ----s with the finalizer 
        // thread.  This isn't just about handles - it can happen with just 
        // about any finalizable resource.
        //
        // Users should insert a call to this method near the end of a
        // method where they must keep an object alive for the duration of that
        // method, up until this method is called.  Here is an example:
        // 
        // "...all you really need is one object with a Finalize method, and a 
        // second object with a Close/Dispose/Done method.  Such as the following 
        // contrived example:
        //
        // class Foo {
        //    Stream stream = ...;
        //    protected void Finalize() { stream.Close(); }
        //    void Problem() { stream.MethodThatSpansGCs(); }
        //    static void Main() { new Foo().Problem(); }
        // }
        // 
        //
        // In this code, Foo will be finalized in the middle of 
        // stream.MethodThatSpansGCs, thus closing a stream still in use."
        //
        // If we insert a call to GC.KeepAlive(this) at the end of Problem(), then
        // Foo doesn't get finalized and the stream says open.
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // disable optimizations
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void KeepAlive(Object obj)
        {
        }

        // Returns the generation in which wo currently resides.
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static int GetGeneration(WeakReference wo) {
#if MONO
            object obj = wo.Target;
            if (obj == null)
                throw new ArgumentException ();
            return GetGeneration (obj);
#else
            int result = GetGenerationWR(wo.m_handle);
            KeepAlive(wo);
            return result;
#endif
        }
    
        // Returns the maximum GC generation.  Currently assumes only 1 heap.
        //
        public static int MaxGeneration {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get { return GetMaxGeneration(); }
        }

#if MONO
        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        public extern static void WaitForPendingFinalizers ();
#else
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void _WaitForPendingFinalizers();


        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void WaitForPendingFinalizers() {
            // QCalls can not be exposed from mscorlib directly, need to wrap it.
            _WaitForPendingFinalizers();
        }
#endif
    
        // Indicates that the system should not call the Finalize() method on
        // an object that would normally require this call.
#if !MONO
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
#endif
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern void _SuppressFinalize(Object o);

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void SuppressFinalize(Object obj) {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Contract.EndContractBlock();
            _SuppressFinalize(obj);
        }

        // Indicates that the system should call the Finalize() method on an object
        // for which SuppressFinalize has already been called. The other situation 
        // where calling ReRegisterForFinalize is useful is inside a finalizer that 
        // needs to resurrect itself or an object that it references.
#if !MONO
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
#endif
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void _ReRegisterForFinalize(Object o);
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static void ReRegisterForFinalize(Object obj) {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Contract.EndContractBlock();
            _ReRegisterForFinalize(obj);
        }

        // Returns the total number of bytes currently in use by live objects in
        // the GC heap.  This does not return the total size of the GC heap, but
        // only the live objects in the GC heap.
        //
#if MONO
        [MethodImplAttribute (MethodImplOptions.InternalCall)]
        public extern static long GetTotalMemory (bool forceFullCollection);

        static bool _RegisterForFullGCNotification(int maxGenerationPercentage, int largeObjectHeapPercentage)
        {
            throw new NotImplementedException ();
        }

        static bool _CancelFullGCNotification ()
        {
            throw new NotImplementedException ();
        }

        static int _WaitForFullGCApproach (int millisecondsTimeout)
        {
            throw new NotImplementedException ();
        }

        static int _WaitForFullGCComplete (int millisecondsTimeout)
        {
            throw new NotImplementedException ();
        }

#else 
        [System.Security.SecuritySafeCritical]  // auto-generated
        public static long GetTotalMemory(bool forceFullCollection) {
            long size = GetTotalMemory();
            if (!forceFullCollection)
                return size;
            // If we force a full collection, we will run the finalizers on all 
            // existing objects and do a collection until the value stabilizes.
            // The value is "stable" when either the value is within 5% of the 
            // previous call to GetTotalMemory, or if we have been sitting
            // here for more than x times (we don't want to loop forever here).
            int reps = 20;  // Number of iterations
            long newSize = size;
            float diff;
            do {
                GC.WaitForPendingFinalizers();
                GC.Collect();
                size = newSize;
                newSize = GetTotalMemory();
                diff = ((float)(newSize - size)) / size;
            } while (reps-- > 0 && !(-.05 < diff && diff < .05));
            return newSize;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool _RegisterForFullGCNotification(int maxGenerationPercentage, int largeObjectHeapPercentage);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool _CancelFullGCNotification();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int _WaitForFullGCApproach(int millisecondsTimeout);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int _WaitForFullGCComplete(int millisecondsTimeout);
#endif

        [SecurityCritical]
        public static void RegisterForFullGCNotification(int maxGenerationThreshold, int largeObjectHeapThreshold)
        {
            if ((maxGenerationThreshold <= 0) || (maxGenerationThreshold >= 100))
            {
                throw new ArgumentOutOfRangeException("maxGenerationThreshold", 
                                                      String.Format(
                                                          CultureInfo.CurrentCulture,
                                                          Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), 
                                                          1, 
                                                          99));
            }
            
            if ((largeObjectHeapThreshold <= 0) || (largeObjectHeapThreshold >= 100))
            {
                throw new ArgumentOutOfRangeException("largeObjectHeapThreshold", 
                                                      String.Format(
                                                          CultureInfo.CurrentCulture,
                                                          Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), 
                                                          1, 
                                                          99));
}

            if (!_RegisterForFullGCNotification(maxGenerationThreshold, largeObjectHeapThreshold))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
            }
        }

        [SecurityCritical]
        public static void CancelFullGCNotification()
        {
            if (!_CancelFullGCNotification())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
            }
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCApproach()
        {
            return (GCNotificationStatus)_WaitForFullGCApproach(-1);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCApproach(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));

            return (GCNotificationStatus)_WaitForFullGCApproach(millisecondsTimeout);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCComplete()
        {
            return (GCNotificationStatus)_WaitForFullGCComplete(-1);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCComplete(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            return (GCNotificationStatus)_WaitForFullGCComplete(millisecondsTimeout);
        }

        enum StartNoGCRegionStatus
        {
            Succeeded = 0,
            NotEnoughMemory = 1,
            AmountTooLarge = 2,
            AlreadyInProgress = 3
        }

        enum EndNoGCRegionStatus
        {
            Succeeded = 0,
            NotInProgress = 1,
            GCInduced = 2,
            AllocationExceeded = 3
        }

        [SecurityCritical]
        static bool StartNoGCRegionWorker(long totalSize, bool hasLohSize, long lohSize, bool disallowFullBlockingGC)
        {
#if MONO
            throw new NotImplementedException ();
#else
            StartNoGCRegionStatus status = (StartNoGCRegionStatus)_StartNoGCRegion(totalSize, hasLohSize, lohSize, disallowFullBlockingGC);
            if (status == StartNoGCRegionStatus.AmountTooLarge)
                throw new ArgumentOutOfRangeException("totalSize", 
                    "totalSize is too large. For more information about setting the maximum size, see \"Latency Modes\" in http://go.microsoft.com/fwlink/?LinkId=522706");
            else if (status == StartNoGCRegionStatus.AlreadyInProgress)
                throw new InvalidOperationException("The NoGCRegion mode was already in progress");
            else if (status == StartNoGCRegionStatus.NotEnoughMemory)
                return false;
            return true;
#endif
        }

        [SecurityCritical]
        public static bool TryStartNoGCRegion(long totalSize)
        {
            return StartNoGCRegionWorker(totalSize, false, 0, false);
        }

        [SecurityCritical]
        public static bool TryStartNoGCRegion(long totalSize, long lohSize)
        {
            return StartNoGCRegionWorker(totalSize, true, lohSize, false);
        }

        [SecurityCritical]
        public static bool TryStartNoGCRegion(long totalSize, bool disallowFullBlockingGC)
        {
            return StartNoGCRegionWorker(totalSize, false, 0, disallowFullBlockingGC);
        }

        [SecurityCritical]
        public static bool TryStartNoGCRegion(long totalSize, long lohSize, bool disallowFullBlockingGC)
        {
            return StartNoGCRegionWorker(totalSize, true, lohSize, disallowFullBlockingGC);
        }

        [SecurityCritical]
        static EndNoGCRegionStatus EndNoGCRegionWorker()
        {
#if MONO
            throw new NotImplementedException ();
#else
            EndNoGCRegionStatus status = (EndNoGCRegionStatus)_EndNoGCRegion();
            if (status == EndNoGCRegionStatus.NotInProgress)
                throw new InvalidOperationException("NoGCRegion mode must be set");
            else if (status == EndNoGCRegionStatus.GCInduced)
                throw new InvalidOperationException("Garbage collection was induced in NoGCRegion mode");
            else if (status == EndNoGCRegionStatus.AllocationExceeded)
                throw new InvalidOperationException("Allocated memory exceeds specified memory for NoGCRegion mode");

            return EndNoGCRegionStatus.Succeeded;
#endif
        }

        [SecurityCritical]
        public static void EndNoGCRegion()
        {
            EndNoGCRegionWorker();
        }
    }

#if !MONO
#if !FEATURE_CORECLR
    internal class SizedReference : IDisposable
    {
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern IntPtr CreateSizedRef(Object o);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void FreeSizedRef(IntPtr h);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern Object GetTargetOfSizedRef(IntPtr h);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern Int64 GetApproximateSizeOfSizedRef(IntPtr h);

        #pragma warning disable 420
        [System.Security.SecuritySafeCritical]
        private void Free()
        {
            IntPtr temp = _handle;
            if (temp != IntPtr.Zero && 
                (Interlocked.CompareExchange(ref _handle, IntPtr.Zero, temp) == temp))
            {
                FreeSizedRef(temp);
            }
        }

        internal volatile IntPtr _handle;

        [System.Security.SecuritySafeCritical]
        public SizedReference(Object target)
        {
            IntPtr temp = IntPtr.Zero;
            temp = CreateSizedRef(target);
            _handle = temp;
        }

        ~SizedReference()
        {
            Free();
        }

        public Object Target
        {
            [System.Security.SecuritySafeCritical]
            get 
            {
                IntPtr temp = _handle; 
                if (temp == IntPtr.Zero)
                {
                    return null;
                }

                Object o = GetTargetOfSizedRef(temp);

                return (_handle == IntPtr.Zero) ? null : o;
            }
        }

        public Int64 ApproximateSize
        {
            [System.Security.SecuritySafeCritical]
            get 
            {
                IntPtr temp = _handle; 
                
                if (temp == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }

                Int64 size = GetApproximateSizeOfSizedRef(temp);

                if (_handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
                }
                else
                {
                    return size;
                }
            }
        }

        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
        }
    }
#endif
#endif
}
