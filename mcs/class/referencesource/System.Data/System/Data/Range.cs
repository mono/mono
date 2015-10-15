//------------------------------------------------------------------------------
// <copyright file="Range.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;

    internal struct Range {

        private int min;
        private int max;
        private bool isNotNull; // zero bit pattern represents null

        public Range(int min, int max) {
            if (min > max) {
                throw ExceptionBuilder.RangeArgument(min, max);
            }
            this.min = min;
            this.max = max;
            isNotNull = true;
        }

        public int Count {
            get {
                if (IsNull)
                    return 0;
                return max - min + 1;
            }
        }

        public bool IsNull {
            get {
                return !isNotNull;
            }
        }

        public int Max {
            get {
                CheckNull();
                return max;
            }
        }

        public int Min {
            get {
                CheckNull();
                return min;
            }
        }

        internal void CheckNull() {
            if (this.IsNull) {
                throw ExceptionBuilder.NullRange();
            }
        }
    }
}
