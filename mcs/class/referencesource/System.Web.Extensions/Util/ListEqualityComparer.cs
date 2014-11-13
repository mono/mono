//------------------------------------------------------------------------------
// <copyright file="ListEqualityComparer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Collections;

    // Compares two non-generic IList objects for equality.

    internal sealed class ListEqualityComparer : IEqualityComparer {

        internal static readonly ListEqualityComparer Instance = new ListEqualityComparer();

        private ListEqualityComparer() {
        }

        bool IEqualityComparer.Equals(object x, object y) {
            if (Object.ReferenceEquals(x, y)) {
                return true;
            }

            IList xList = (IList)x;
            IList yList = (IList)y;

            if (xList.Count != yList.Count) {
                return false;
            }

            for (int i = 0; i < xList.Count; i++) {
                if (!Object.Equals(xList[i], yList[i])) {
                    return false;
                }
            }

            return true;
        }

        int IEqualityComparer.GetHashCode(object obj) {
            if (obj == null) {
                return 0;
            }

            HashCodeCombiner combiner = new HashCodeCombiner();
            foreach (object item in (IList)obj) {
                combiner.AddObject(item);
            }

            return combiner.CombinedHash32;
        }

    }
}
