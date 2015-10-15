//------------------------------------------------------------------------------
// <copyright file="XmlSortKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// Base internal class for all sort keys.
    /// Inherits from IComparable, so that Array.Sort can perform comparison.
    /// </summary>
    internal abstract class XmlSortKey : IComparable {
        private int priority;           // Original input ordering used to ensure that sort is stable
        private XmlSortKey nextKey;     // Next sort key if there are multiple keys (null otherwise)

        /// <summary>
        /// Get or set this key's index, relative to other keys involved in a sort.  This priority will
        /// break ties.  If the priority is not set, then the sort will not be stable.
        /// </summary>
        public int Priority {
            //get { return this.priority; }
            set {
                // All linked keys have same priority
                XmlSortKey key = this;

                while (key != null) {
                    key.priority = value;
                    key = key.nextKey;
                }
            }
        }

        /// <summary>
        /// Sometimes a key is composed of multiple parts.  For example: (LastName, FirstName).  Multi-part
        /// keys are linked together in a list.  This method recursively adds a new key part to the end of the list.
        /// Returns the first (primary) key in the list.
        /// </summary>
        public XmlSortKey AddSortKey(XmlSortKey sortKey) {
            if (this.nextKey != null) {
                // Add to end of list--this is not it
                this.nextKey.AddSortKey(sortKey);
            }
            else {
                // This is the end of the list
                this.nextKey = sortKey;
            }

            return this;
        }

        /// <summary>
        /// When two keys are compared and found to be equal, the tie must be broken.  If there is a secondary key,
        /// then use that to break the tie.  Otherwise, use the input ordering to break the tie.  Since every key
        /// has a unique index, this is guaranteed to always break the tie.
        /// </summary>
        protected int BreakSortingTie(XmlSortKey that) {
            if (this.nextKey != null) {
                // There are multiple keys, so break tie using next key
                Debug.Assert(this.nextKey != null && that.nextKey != null);
                return this.nextKey.CompareTo(that.nextKey);
            }

            Debug.Assert(this.priority != that.priority);
            return (this.priority < that.priority) ? -1 : 1;
        }

        /// <summary>
        /// Compare a non-empty key (this) to an empty key (obj).  The empty sequence always sorts either before all
        /// other values, or after all other values.
        /// </summary>
        protected int CompareToEmpty(object obj) {
            XmlEmptySortKey that = obj as XmlEmptySortKey;
            Debug.Assert(that != null && !(this is XmlEmptySortKey));
            return that.IsEmptyGreatest ? -1 : 1;
        }

        /// <summary>
        /// Base internal class is abstract and doesn't actually implement CompareTo; derived classes must do this.
        /// </summary>
        public abstract int CompareTo(object that);
    }


    /// <summary>
    /// Sort key for the empty sequence.  Empty sequence always compares sorts either before all other values,
    /// or after all other values.
    /// </summary>
    internal class XmlEmptySortKey : XmlSortKey {
        private bool isEmptyGreatest;

        public XmlEmptySortKey(XmlCollation collation) {
            // Greatest, Ascending: isEmptyGreatest = true
            // Greatest, Descending: isEmptyGreatest = false
            // Least, Ascending: isEmptyGreatest = false
            // Least, Descending: isEmptyGreatest = true
            this.isEmptyGreatest = collation.EmptyGreatest != collation.DescendingOrder;
        }

        public bool IsEmptyGreatest {
            get { return this.isEmptyGreatest; }
        }

        public override int CompareTo(object obj) {
            XmlEmptySortKey that = obj as XmlEmptySortKey;

            if (that == null) {
                // Empty compared to non-empty
                Debug.Assert(obj is XmlSortKey);
                return -(obj as XmlSortKey).CompareTo(this);
            }

            // Empty compared to empty
            return BreakSortingTie(that);
        }
    }


    /// <summary>
    /// Sort key for xs:decimal values.
    /// </summary>
    internal class XmlDecimalSortKey : XmlSortKey {
        private decimal decVal;

        public XmlDecimalSortKey(decimal value, XmlCollation collation) {
            // Invert decimal if sorting in descending order
            this.decVal = collation.DescendingOrder ? -value : value;
        }

        public override int CompareTo(object obj) {
            XmlDecimalSortKey that = obj as XmlDecimalSortKey;
            int cmp;

            if (that == null)
                return CompareToEmpty(obj);

            cmp = Decimal.Compare(this.decVal, that.decVal);
            if (cmp == 0)
                return BreakSortingTie(that);

            return cmp;
        }
    }


    /// <summary>
    /// Sort key for xs:integer values.
    /// </summary>
    internal class XmlIntegerSortKey : XmlSortKey {
        private long longVal;

        public XmlIntegerSortKey(long value, XmlCollation collation) {
            // Invert long if sorting in descending order
            this.longVal = collation.DescendingOrder ? ~value : value;
        }

        public override int CompareTo(object obj) {
            XmlIntegerSortKey that = obj as XmlIntegerSortKey;

            if (that == null)
                return CompareToEmpty(obj);

            if (this.longVal == that.longVal)
                return BreakSortingTie(that);

            return (this.longVal < that.longVal) ? -1 : 1;
        }
    }


    /// <summary>
    /// Sort key for xs:int values.
    /// </summary>
    internal class XmlIntSortKey : XmlSortKey {
        private int intVal;

        public XmlIntSortKey(int value, XmlCollation collation) {
            // Invert integer if sorting in descending order
            this.intVal = collation.DescendingOrder ? ~value : value;
        }

        public override int CompareTo(object obj) {
            XmlIntSortKey that = obj as XmlIntSortKey;

            if (that == null)
                return CompareToEmpty(obj);

            if (this.intVal == that.intVal)
                return BreakSortingTie(that);

            return (this.intVal < that.intVal) ? -1 : 1;
        }
    }


    /// <summary>
    /// Sort key for xs:string values.  Strings are sorted according to a byte-wise sort key calculated by caller.
    /// </summary>
    internal class XmlStringSortKey : XmlSortKey {
        private SortKey sortKey;
        private byte[] sortKeyBytes;
        private bool descendingOrder;

        public XmlStringSortKey(SortKey sortKey, bool descendingOrder) {
            this.sortKey = sortKey;
            this.descendingOrder = descendingOrder;
        }

        public XmlStringSortKey(byte[] sortKey, bool descendingOrder) {
            this.sortKeyBytes = sortKey;
            this.descendingOrder = descendingOrder;
        }

        public override int CompareTo(object obj) {
            XmlStringSortKey that = obj as XmlStringSortKey;
            int idx, cntCmp, result;

            if (that == null)
                return CompareToEmpty(obj);

            // Compare either using SortKey.Compare or byte arrays
            if (this.sortKey != null) {
                Debug.Assert(that.sortKey != null, "Both keys must have non-null sortKey field");
                result = SortKey.Compare(this.sortKey, that.sortKey);
            }
            else {
                Debug.Assert(this.sortKeyBytes != null && that.sortKeyBytes != null, "Both keys must have non-null sortKeyBytes field");

                cntCmp = (this.sortKeyBytes.Length < that.sortKeyBytes.Length) ? this.sortKeyBytes.Length : that.sortKeyBytes.Length;
                for (idx = 0; idx < cntCmp; idx++) {
                    if (this.sortKeyBytes[idx] < that.sortKeyBytes[idx]) {
                        result = -1;
                        goto Done;
                    }

                    if (this.sortKeyBytes[idx] > that.sortKeyBytes[idx]) {
                        result = 1;
                        goto Done;
                    }
                }

                // So far, keys are equal, so now test length of each key
                if (this.sortKeyBytes.Length < that.sortKeyBytes.Length)
                    result = -1;
                else if (this.sortKeyBytes.Length > that.sortKeyBytes.Length)
                    result = 1;
                else
                    result = 0;
            }

        Done:
            // Use document order to break sorting tie
            if (result == 0)
                return BreakSortingTie(that);

            return this.descendingOrder ? -result : result;
        }
    }


    /// <summary>
    /// Sort key for Double values.
    /// </summary>
    internal class XmlDoubleSortKey : XmlSortKey {
        private double dblVal;
        private bool isNaN;

        public XmlDoubleSortKey(double value, XmlCollation collation) {
            if (Double.IsNaN(value)) {
                // Treat NaN as if it were the empty sequence
                this.isNaN = true;

                // Greatest, Ascending: isEmptyGreatest = true
                // Greatest, Descending: isEmptyGreatest = false
                // Least, Ascending: isEmptyGreatest = false
                // Least, Descending: isEmptyGreatest = true
                this.dblVal = (collation.EmptyGreatest != collation.DescendingOrder) ? Double.PositiveInfinity : Double.NegativeInfinity;
            }
            else {
                this.dblVal = collation.DescendingOrder ? -value : value;
            }
        }

        public override int CompareTo(object obj) {
            XmlDoubleSortKey that = obj as XmlDoubleSortKey;

            if (that == null) {
                // Compare to empty sequence
                if (this.isNaN)
                    return BreakSortingTie(obj as XmlSortKey);

                return CompareToEmpty(obj);
            }

            if (this.dblVal == that.dblVal) {
                if (this.isNaN) {
                    // NaN sorts equal to NaN
                    if (that.isNaN)
                        return BreakSortingTie(that);

                    // NaN sorts before or after all non-NaN values
                    Debug.Assert(this.dblVal == Double.NegativeInfinity || this.dblVal == Double.PositiveInfinity);
                    return (this.dblVal == Double.NegativeInfinity) ? -1 : 1;
                }
                else if (that.isNaN) {
                    // NaN sorts before or after all non-NaN values
                    Debug.Assert(that.dblVal == Double.NegativeInfinity || that.dblVal == Double.PositiveInfinity);
                    return (that.dblVal == Double.NegativeInfinity) ? 1 : -1;
                }

                return BreakSortingTie(that);
            }

            return (this.dblVal < that.dblVal) ? -1 : 1;
        }
    }


    /// <summary>
    /// Sort key for DateTime values (just convert DateTime to ticks and use Long sort key).
    /// </summary>
    internal class XmlDateTimeSortKey : XmlIntegerSortKey {
        public XmlDateTimeSortKey(DateTime value, XmlCollation collation) : base(value.Ticks, collation) {
        }
    }
}
