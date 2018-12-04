using System;
using MS.Win32 ; 
using System.Runtime.InteropServices;

namespace MS.Win32
{
    internal static class HandleCollector
    {
        private static HandleType[]             handleTypes;
        private static int                      handleTypeCount = 0;

        private static Object handleMutex = new Object();

        /// <devdoc>
        ///     Adds the given handle to the handle collector.  This keeps the
        ///     handle on a "hot list" of objects that may need to be garbage
        ///     collected.
        /// </devdoc>
        internal static IntPtr Add(IntPtr handle, int type) {
            handleTypes[type - 1].Add();
            return handle;
        }

        /// <SecurityNote>
        /// Critical - Accepts and returns critical SafeHandle type.
        /// Safe - Does not perform operations on the critical handle, does not leak handle information.
        /// </SecurityNote>
        [System.Security.SecuritySafeCritical]
        internal static SafeHandle Add(SafeHandle handle, int type) {
            handleTypes[type - 1].Add();
            return handle;
        }

        internal static void Add(int type) {
            handleTypes[type - 1].Add();
        }

        /// <devdoc>
        ///     Registers a new type of handle with the handle collector.
        /// </devdoc>
        internal static int RegisterType(string typeName, int expense, int initialThreshold) {
            lock (handleMutex)
            {
                if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
                {
                    HandleType[] newTypes = new HandleType[handleTypeCount + 10];
                    if (handleTypes != null) {
                        Array.Copy(handleTypes, 0, newTypes, 0, handleTypeCount);
                    }
                    handleTypes = newTypes;
                }

                handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
                return handleTypeCount;
            }
        }

        /// <devdoc>
        ///     Removes the given handle from the handle collector.  Removing a
        ///     handle removes it from our "hot list" of objects that should be
        ///     frequently garbage collected.
        /// </devdoc>
        internal static IntPtr Remove(IntPtr handle, int type) {
            handleTypes[type - 1].Remove();
            return handle ; 
        }

        /// <SecurityNote>
        /// Critical - Accepts and returns critical SafeHandle type.
        /// Safe - Does not perform operations on the critical handle, does not leak handle information.
        /// </SecurityNote>
        [System.Security.SecuritySafeCritical]
        internal static SafeHandle Remove(SafeHandle handle, int type) {
            handleTypes[type - 1].Remove();
            return handle ; 
        }

        internal static void Remove(int type) {
            handleTypes[type - 1].Remove();
        }

        /// <devdoc>
        ///     Represents a specific type of handle.
        /// </devdoc>
        private class HandleType
        {
            internal readonly string name;

            private int initialThreshHold;
            private int threshHold;
            private int handleCount;
            private readonly int deltaPercent;

            /// <devdoc>
            ///     Creates a new handle type.
            /// </devdoc>
            internal HandleType(string name, int expense, int initialThreshHold) {
                this.name = name;
                this.initialThreshHold = initialThreshHold;
                this.threshHold = initialThreshHold;
                this.deltaPercent = 100 - expense;
            }

            /// <devdoc>
            ///     Adds a handle to this handle type for monitoring.
            /// </devdoc>
            internal void Add() {
                bool performCollect = false;

                lock(this) {
                    handleCount++;
                    performCollect = NeedCollection();

                    if (!performCollect) {
                        return;
                    }
                }

                if (performCollect) {
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> Forcing garbage collect");
                    Debug.WriteLine("HC>     name        :" + name);
                    Debug.WriteLine("HC>     threshHold  :" + (threshHold).ToString());
                    Debug.WriteLine("HC>     handleCount :" + (handleCount).ToString());
                    Debug.WriteLine("HC>     deltaPercent:" + (deltaPercent).ToString());
#endif                  
                    GC.Collect();

                    // We just performed a GC.  If the main thread is in a tight
                    // loop there is a this will cause us to increase handles forever and prevent handle collector
                    // from doing its job.  Yield the thread here.  This won't totally cause
                    // a finalization pass but it will effectively elevate the priority
                    // of the finalizer thread just for an instant.  But how long should
                    // we sleep?  We base it on how expensive the handles are because the
                    // more expensive the handle, the more critical that it be reclaimed.
                    int sleep = (100 - deltaPercent) / 4;
                    System.Threading.Thread.Sleep(sleep);
                }
            }


            /// <devdoc>
            ///     Determines if this handle type needs a garbage collection pass.
            /// </devdoc>
            internal bool NeedCollection() {

                if (handleCount > threshHold) {
                    threshHold = handleCount + ((handleCount * deltaPercent) / 100);
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> NeedCollection: increase threshHold to " + threshHold);
#endif                  
                    return true;
                }

                // If handle count < threshHold, we don't
                // need to collect, but if it 10% below the next lowest threshhold we
                // will bump down a rung.  We need to choose a percentage here or else
                // we will oscillate.
                //
                int oldThreshHold = (100 * threshHold) / (100 + deltaPercent);
                if (oldThreshHold >= initialThreshHold && handleCount <  (int)(oldThreshHold * .9F)) {
#if DEBUG_HANDLECOLLECTOR
                    Debug.WriteLine("HC> NeedCollection: throttle threshhold " + threshHold + " down to " + oldThreshHold);
#endif                  
                    threshHold = oldThreshHold;
                }

                return false;
            }

            /// <devdoc>
            ///     Removes the given handle from our monitor list.
            /// </devdoc>
            internal void Remove() {
                lock(this) {
                    handleCount--;

                    handleCount = Math.Max(0, handleCount);
                }
            }
        }
    }

}
