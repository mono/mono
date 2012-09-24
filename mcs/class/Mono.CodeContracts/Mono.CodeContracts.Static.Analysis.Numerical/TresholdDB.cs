// 
// TresholdDB.cs
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

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static class ThresholdDB {
                static RationalThreshold rational = new RationalThreshold (10);

                public static void Reset ()
                {
                        rational = new RationalThreshold (10);
                }

                public static void Add (IEnumerable<int> values)
                {
                        foreach (var value in values) {
                                rational.Add (Rational.For (value));
                        }
                }

                public static Rational GetNext (Rational value)
                {
                        return rational.GetNext (value);
                }

                public static Rational GetPrevious (Rational value)
                {
                        return rational.GetPrevious (value);
                }

                public static bool TryGetAThreshold<TVar, TExpr> (TExpr e, IExpressionDecoder<TVar, TExpr> decoder, out List<int> thresholds)
                {
                        var visitor = new GetThresholdVisitor<TVar, TExpr> (decoder);
                        if (visitor.Visit (e, Dummy.Value)) {
                                thresholds = new List<int> ();
                                foreach (var threshold in visitor.Thresholds) {
                                        thresholds.Add (threshold - 1);
                                        thresholds.Add (threshold);
                                        thresholds.Add (threshold + 1);
                                }
                                return true;
                        }

                        return false.Without (out thresholds);
                }
        }
}