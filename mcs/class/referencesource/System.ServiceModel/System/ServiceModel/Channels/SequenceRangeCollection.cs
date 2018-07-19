//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using System.ServiceModel;

    abstract class SequenceRangeCollection
    {
        static EmptyRangeCollection empty = new EmptyRangeCollection();
        static LowerComparer lowerComparer = new LowerComparer();
        static UpperComparer upperComparer = new UpperComparer();

        public abstract SequenceRange this[int index] { get; }
        public abstract int Count { get; }

        public static SequenceRangeCollection Empty
        {
            get
            {
                return empty;
            }
        }

        public abstract bool Contains(Int64 number);

        public abstract SequenceRangeCollection MergeWith(Int64 number);
        public abstract SequenceRangeCollection MergeWith(SequenceRange range);

        static SequenceRangeCollection GeneralCreate(SequenceRange[] sortedRanges)
        {
            if (sortedRanges.Length == 0)
            {
                return empty;
            }
            else if (sortedRanges.Length == 1)
            {
                return new SingleItemRangeCollection(sortedRanges[0]);
            }
            else
            {
                return new MultiItemRangeCollection(sortedRanges);
            }
        }

        static SequenceRangeCollection GeneralMerge(SequenceRange[] sortedRanges, SequenceRange range)
        {
            if (sortedRanges.Length == 0)
            {
                return new SingleItemRangeCollection(range);
            }

            int lowerBound;

            if (sortedRanges.Length == 1)
            {
                // Avoid performance hit of binary search in single range case
                if (range.Lower == sortedRanges[0].Upper)
                {
                    lowerBound = 0;
                }
                else if (range.Lower < sortedRanges[0].Upper)
                {
                    lowerBound = ~0;
                }
                else
                {
                    lowerBound = ~1;
                }
            }
            else
            {
                lowerBound = Array.BinarySearch(sortedRanges, new SequenceRange(range.Lower), upperComparer);
            }

            if (lowerBound < 0)
            {
                lowerBound = ~lowerBound;

                if ((lowerBound > 0) && (sortedRanges[lowerBound - 1].Upper == range.Lower - 1))
                {
                    lowerBound--;
                }

                if (lowerBound == sortedRanges.Length)
                {
                    SequenceRange[] returnedRanges = new SequenceRange[sortedRanges.Length + 1];
                    Array.Copy(sortedRanges, returnedRanges, sortedRanges.Length);
                    returnedRanges[sortedRanges.Length] = range;
                    return GeneralCreate(returnedRanges);
                }
            }

            int upperBound;

            if (sortedRanges.Length == 1)
            {
                // Avoid performance hit of binary search in single range case
                if (range.Upper == sortedRanges[0].Lower)
                {
                    upperBound = 0;
                }
                else if (range.Upper < sortedRanges[0].Lower)
                {
                    upperBound = ~0;
                }
                else
                {
                    upperBound = ~1;
                }
            }
            else
            {
                upperBound = Array.BinarySearch(sortedRanges, new SequenceRange(range.Upper), lowerComparer);
            }

            if (upperBound < 0)
            {
                upperBound = ~upperBound;

                if (upperBound > 0)
                {
                    if ((upperBound == sortedRanges.Length) || (sortedRanges[upperBound].Lower != range.Upper + 1))
                    {
                        upperBound--;
                    }
                }
                else if (sortedRanges[0].Lower > range.Upper + 1)
                {
                    SequenceRange[] returnedRanges = new SequenceRange[sortedRanges.Length + 1];
                    Array.Copy(sortedRanges, 0, returnedRanges, 1, sortedRanges.Length);
                    returnedRanges[0] = range;
                    return GeneralCreate(returnedRanges);
                }
            }

            Int64 newLower = (range.Lower < sortedRanges[lowerBound].Lower) ? range.Lower : sortedRanges[lowerBound].Lower;
            Int64 newUpper = (range.Upper > sortedRanges[upperBound].Upper) ? range.Upper : sortedRanges[upperBound].Upper;

            int rangesRemoved = upperBound - lowerBound + 1;
            int rangesRemaining = sortedRanges.Length - rangesRemoved + 1;
            if (rangesRemaining == 1)
            {
                return new SingleItemRangeCollection(newLower, newUpper);
            }
            else
            {
                SequenceRange[] returnedRanges = new SequenceRange[rangesRemaining];
                Array.Copy(sortedRanges, returnedRanges, lowerBound);
                returnedRanges[lowerBound] = new SequenceRange(newLower, newUpper);
                Array.Copy(sortedRanges, upperBound + 1, returnedRanges, lowerBound + 1, sortedRanges.Length - upperBound - 1);
                return GeneralCreate(returnedRanges);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                SequenceRange range = this[i];
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(range.Lower);
                builder.Append('-');
                builder.Append(range.Upper);
            }
            return builder.ToString();
        }

        class EmptyRangeCollection : SequenceRangeCollection
        {
            public override SequenceRange this[int index]
            {
                get
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
                }
            }

            public override int Count
            {
                get
                {
                    return 0;
                }
            }

            public override bool Contains(Int64 number)
            {
                return false;
            }

            public override SequenceRangeCollection MergeWith(Int64 number)
            {
                return new SingleItemRangeCollection(number, number);
            }

            public override SequenceRangeCollection MergeWith(SequenceRange range)
            {
                return new SingleItemRangeCollection(range);
            }
        }

        class MultiItemRangeCollection : SequenceRangeCollection
        {
            SequenceRange[] ranges;

            public MultiItemRangeCollection(SequenceRange[] sortedRanges)
            {
                this.ranges = sortedRanges;
            }

            public override SequenceRange this[int index]
            {
                get
                {
                    if (index < 0 || index >= ranges.Length)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", index,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, ranges.Length - 1)));
                    return this.ranges[index];
                }
            }

            public override int Count
            {
                get
                {
                    return this.ranges.Length;
                }
            }

            public override bool Contains(Int64 number)
            {
                if (this.ranges.Length == 0)
                {
                    return false;
                }
                else if (this.ranges.Length == 1)
                {
                    return this.ranges[0].Contains(number);
                }

                SequenceRange searchFor = new SequenceRange(number);
                int searchValue = Array.BinarySearch(this.ranges, searchFor, lowerComparer);

                if (searchValue >= 0)
                {
                    return true;
                }

                searchValue = ~searchValue;

                if (searchValue == 0)
                {
                    return false;
                }

                return (this.ranges[searchValue - 1].Upper >= number);
            }

            public override SequenceRangeCollection MergeWith(Int64 number)
            {
                return MergeWith(new SequenceRange(number));
            }

            public override SequenceRangeCollection MergeWith(SequenceRange newRange)
            {
                return GeneralMerge(this.ranges, newRange);
            }
        }

        class SingleItemRangeCollection : SequenceRangeCollection
        {
            SequenceRange range;

            public SingleItemRangeCollection(SequenceRange range)
            {
                this.range = range;
            }

            public SingleItemRangeCollection(Int64 lower, Int64 upper)
            {
                this.range = new SequenceRange(lower, upper);
            }

            public override SequenceRange this[int index]
            {
                get
                {
                    if (index != 0)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index"));
                    return this.range;
                }
            }

            public override int Count
            {
                get
                {
                    return 1;
                }
            }

            public override bool Contains(Int64 number)
            {
                return this.range.Contains(number);
            }

            public override SequenceRangeCollection MergeWith(Int64 number)
            {
                if (number == this.range.Upper + 1)
                {
                    return new SingleItemRangeCollection(range.Lower, number);
                }
                else
                {
                    return MergeWith(new SequenceRange(number));
                }
            }

            public override SequenceRangeCollection MergeWith(SequenceRange newRange)
            {
                if (newRange.Lower == this.range.Upper + 1)
                {
                    return new SingleItemRangeCollection(range.Lower, newRange.Upper);
                }
                else if (this.range.Contains(newRange))
                {
                    return this;
                }
                else if (newRange.Contains(this.range))
                {
                    return new SingleItemRangeCollection(newRange);
                }
                else if (newRange.Upper == this.range.Lower - 1)
                {
                    return new SingleItemRangeCollection(newRange.Lower, this.range.Upper);
                }
                else
                {
                    return GeneralMerge(new SequenceRange[] { this.range }, newRange);
                }
            }
        }

        class LowerComparer : IComparer<SequenceRange>
        {
            public int Compare(SequenceRange x, SequenceRange y)
            {
                if (x.Lower < y.Lower)
                {
                    return -1;
                }
                else if (x.Lower > y.Lower)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        class UpperComparer : IComparer<SequenceRange>
        {
            public int Compare(SequenceRange x, SequenceRange y)
            {
                if (x.Upper < y.Upper)
                {
                    return -1;
                }
                else if (x.Upper > y.Upper)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
