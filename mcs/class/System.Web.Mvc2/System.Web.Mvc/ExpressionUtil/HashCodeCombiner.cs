/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Collections;

    // based on System.Web.Util.HashCodeCombiner
    internal class HashCodeCombiner {

        private long _combinedHash64 = 0x1505L;

        public void AddFingerprint(ExpressionFingerprint fingerprint) {
            if (fingerprint != null) {
                fingerprint.AddToHashCodeCombiner(this);
            }
            else {
                AddInt32(0);
            }
        }

        public void AddEnumerable(IEnumerable e) {
            if (e == null) {
                AddInt32(0);
            }
            else {
                int count = 0;
                foreach (object o in e) {
                    AddObject(o);
                    count++;
                }
                AddInt32(count);
            }
        }

        public void AddInt32(int i) {
            _combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ i;
        }

        public void AddObject(object o) {
            int oHashCode = (o != null) ? o.GetHashCode() : 0;
            AddInt32(oHashCode);
        }

        public int CombinedHash {
            get {
                return _combinedHash64.GetHashCode();
            }
        }

    }
}
