// <copyright file="SafeBitVector32.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
using System;
using System.Threading;

namespace System.Runtime.Caching {
    //
    // This is a multithreadsafe version of System.Collections.Specialized.BitVector32.
    //
    [Serializable]
    internal struct SafeBitVector32 {
        private volatile int _data;
    
        internal bool this[int bit] {
            get {
                int data = _data;
                return (data & bit) == bit;
            }
            set {
                for (;;) {
                    int oldData = _data;
                    int newData;
                    if (value) {
                        newData = oldData | bit;
                    }
                    else {
                        newData = oldData & ~bit;
                    }

#pragma warning disable 0420
                    int result = Interlocked.CompareExchange(ref _data, newData, oldData);
#pragma warning restore 0420

                    if (result == oldData) {
                        break;
                    }
                }
            }
        }

        internal bool ChangeValue(int bit, bool value) {
            for (;;) {
                int oldData = _data;
                int newData;
                if (value) {
                    newData = oldData | bit;
                }
                else {
                    newData = oldData & ~bit;
                }

                if (oldData == newData) {
                    return false;
                }

#pragma warning disable 0420
                int result = Interlocked.CompareExchange(ref _data, newData, oldData);
#pragma warning restore 0420

                if (result == oldData) {
                    return true;
                }
            }
        }
    }
}

