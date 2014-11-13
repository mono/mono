//------------------------------------------------------------------------------
// <copyright file="ActivityIdHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Diagnostics.Tracing;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class ActivityIdHelper {

        private delegate Guid GetCurrentDelegate();
        private delegate void SetAndDestroyDelegate(Guid activityId);
        private delegate void SetAndPreserveDelegate(Guid activityId, out Guid oldActivityThatWillContinue);

        // Note to callers: this field can be null.
        internal static readonly ActivityIdHelper Instance = GetSingleton();

        private static readonly Guid _baseGuid = Guid.NewGuid();
        private static long _counter;

        private readonly GetCurrentDelegate _getCurrentDel;
        private readonly SetAndDestroyDelegate _setAndDestroyDel;
        private readonly SetAndPreserveDelegate _setAndPreserveDel;

        // use the factory to create an instance of this type
        private ActivityIdHelper(GetCurrentDelegate getCurrentDel, SetAndDestroyDelegate setAndDestroyDel, SetAndPreserveDelegate setAndPreserveDel) {
            _getCurrentDel = getCurrentDel;
            _setAndDestroyDel = setAndDestroyDel;
            _setAndPreserveDel = setAndPreserveDel;
        }

        // Gets the current thread's activity ID.
        public Guid CurrentThreadActivityId {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _getCurrentDel(); }
        }

        private static ActivityIdHelper GetSingleton() {
            try {
                // The mscorlib APIs we depend on weren't added until Blue, so we can't
                // take a direct dependency. Need to light up instead.

                var getCurrentDel = (GetCurrentDelegate)Delegate.CreateDelegate(
                    typeof(GetCurrentDelegate), typeof(EventSource), "get_CurrentThreadActivityId", ignoreCase: false, throwOnBindFailure: false);

                var setAndDestroyDel = (SetAndDestroyDelegate)Delegate.CreateDelegate(
                    typeof(SetAndDestroyDelegate), typeof(EventSource), "SetCurrentThreadActivityId", ignoreCase: false, throwOnBindFailure: false);

                var setAndPreserveDel = (SetAndPreserveDelegate)Delegate.CreateDelegate(
                    typeof(SetAndPreserveDelegate), typeof(EventSource), "SetCurrentThreadActivityId", ignoreCase: false, throwOnBindFailure: false);

                if (getCurrentDel != null && setAndDestroyDel != null && setAndPreserveDel != null) {
                    return new ActivityIdHelper(getCurrentDel, setAndDestroyDel, setAndPreserveDel);
                }
            }
            catch {
                // exceptions are not fatal; we just won't be able to call the new APIs
            }

            return null;
        }

        // Disposes of the thread's existing activity ID, then sets the new activity ID on this thread.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCurrentThreadActivityId(Guid activityId) {
            _setAndDestroyDel(activityId);
        }

        // Suspends (but does not dispose of) the thread's existing activity ID, then sets a new activity ID on this thread.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCurrentThreadActivityId(Guid activityId, out Guid oldActivityThatWillContinue) {
            _setAndPreserveDel(activityId, out oldActivityThatWillContinue);
        }

        // !! SECURITY WARNING !!
        // The GUIDs created by this method are predictable and should be used ONLY for tracing.
        // Any other use (such as leaking them to the user) constitutes information disclosure.
        //
        // This is a perf-sensitive method since it could potentially be called many times per
        // request. Guid.NewGuid() is slow since it eventually calls CAPI, and we did actually
        // see it show up as a bottleneck when developing MVC 2. The below implementation has
        // measurably better performance characteristics than calling the other Guid ctors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Guid UnsafeCreateNewActivityId() {
            Guid guidCopy = _baseGuid;
            *(long*)(&guidCopy) ^= Interlocked.Increment(ref _counter); // operate on the copy, not the original
            return guidCopy;
        }

    }
}
