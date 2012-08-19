using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class Threshold<T> {
                protected int NextFree;
                protected readonly List<T> Values;

                protected abstract T MinusInfinity { get; }
                protected abstract T PlusInfinity { get; }
                protected abstract T Zero { get; }

                public int Count { get { return Values.Count; } }

                protected Threshold (int size)
                {
                        Values = new List<T> (size) {MinusInfinity, Zero, PlusInfinity};
                        NextFree = 3;
                }

                public bool Add (T value)
                {
                        if (NextFree == Values.Count)
                                return false;

                        var idx = 0;
                        while (idx < NextFree && LessThan (Values[idx], value))
                                idx++;

                        if (Values[idx].Equals (value))
                                return false;

                        Values.Insert (idx, value);
                        NextFree++;
                        return true;
                }

                protected abstract bool LessThan (T a, T b);

                public T GetNext (T value)
                {
                        var index = BinarySearch (value, 0, NextFree - 1);

                        if (index >= 0)
                                return Values[index];

                        var nextIndex = ~index; //because binary search returns position of next item if not found
                        return Values[nextIndex];
                }

                public T GetPrevious (T value)
                {
                        var index = BinarySearch (value, 0, NextFree - 1);

                        if (index >= 0)
                                return Values[index];

                        var nextIndex = ~index;
                        return Values[nextIndex - 1];
                }

                public int BinarySearch (T value, int low, int hi)
                {
                        while (low <= hi) {
                                var median = low + ((hi - low) >> 1);
                                if (Values[median].Equals (value))
                                        return median;
                                if (LessThan (Values[median], value))
                                        low = median + 1;
                                else
                                        hi = median - 1;
                        }

                        return ~low;
                }
        }
}