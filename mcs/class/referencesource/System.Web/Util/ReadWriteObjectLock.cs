//------------------------------------------------------------------------------
// <copyright file="ReadWriteObjectLock.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ReadWriteObjectLock
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Util {

    using System.Runtime.Serialization.Formatters;
    using System.Threading;

    class ReadWriteObjectLock {
        // Assumption:
        // -1 = a writer has the lock
        //  0 = no one has the lock
        // >0 = number of readers using the lock
        private int _lock;

        internal ReadWriteObjectLock() {
        }

        internal virtual void AcquireRead() {
            lock(this) {
                while (_lock == -1) {
                    try {
                        Monitor.Wait(this);
                    }
                    catch (ThreadInterruptedException) {
                        // Just keep looping
                    }
                }

                _lock++;
            }                   
        }

        internal virtual void ReleaseRead() {
            lock(this) {
                Debug.Assert(_lock > 0);

                _lock--;
                if (_lock == 0) {
                    Monitor.PulseAll(this);
                }
            }
        }

        internal virtual void AcquireWrite() {
            lock(this) {
                while (_lock != 0) {
                    try {
                        Monitor.Wait(this);
                    }
                    catch (ThreadInterruptedException) {
                        // Just keep looping
                    }
                }

                _lock = -1;
            }                   
        }

        internal virtual void ReleaseWrite() {
            lock(this) {
                Debug.Assert(_lock == -1);

                _lock = 0;
                Monitor.PulseAll(this);
            }
        }
/*
        internal virtual void AssertReadLock() {
#if DBG
            Debug.Assert(_lock > 0);
#endif
        }


        internal virtual void AssertWriteLock() {
#if DBG
            Debug.Assert(_lock == -1);
#endif
        }
*/
    }

}
