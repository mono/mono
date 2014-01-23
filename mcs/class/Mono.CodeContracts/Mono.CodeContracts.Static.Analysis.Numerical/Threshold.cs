// 
// Threshold.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        public abstract class Threshold<T> {
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