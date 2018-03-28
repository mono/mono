//------------------------------------------------------------------------------
// <copyright file="SimpleBitVector32.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;

    //
    // This is a cut down copy of System.Collections.Specialized.BitVector32. The
    // reason this is here is because it is used rather intensively by Control and
    // WebControl. As a result, being able to inline this operations results in a
    // measurable performance gain, at the expense of some maintainability.
    //
    [Serializable]
    internal struct SimpleBitVector32 {
        private int data;

        internal SimpleBitVector32(int data) {
            this.data = data;
        }

        internal int Data {
            get { return data; }
#if UNUSED_CODE
            set { data = value; }
#endif
        }

        internal bool this[int bit] {
            get {
                return (data & bit) == bit;
            }
            set {
                int _data = data;
                if(value) {
                    data = _data | bit;
                }
                else {
                    data = _data & ~bit;
                }
            }
        }

#if UNUSED_CODE
        internal void Set(int bit) {
            data |= bit;
        }

        internal void Clear(int bit) {
            data &= ~bit;
        }

        internal void Toggle(int bit) {
            data ^= bit;
        }

        /*
         * COPY_FLAG copies the value of flags from a source field
         * into a destination field.
         *
         * In the macro:
         * + "&flag" limits the outer xor operation to just the flag we're interested in.
         * + These are the results of the two xor operations:
         *
         * fieldDst    fieldSrc    inner xor   outer xor
         * 0           0           0           0
         * 0           1           1           1
         * 1           0           1           0
         * 1           1           0           1
         */
        internal void Copy(SimpleBitVector32 src, int bit) {
            data ^= (data ^ src.data) & bit;
        }
#endif
    }
}
