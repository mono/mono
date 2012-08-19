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