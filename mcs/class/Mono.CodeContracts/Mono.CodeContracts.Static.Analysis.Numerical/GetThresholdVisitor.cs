using System.Collections.Generic;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class GetThresholdVisitor<TVar, TExpr> : GenericExpressionVisitor<Dummy, bool, TVar, TExpr> {
                public List<int> Thresholds { get; private set; }

                public GetThresholdVisitor (IExpressionDecoder<TVar, TExpr> decoder) : base (decoder)
                {
                        Thresholds = new List<int> ();
                }

                protected override bool Default (Dummy data)
                {
                        return false;
                }

                public override bool VisitConstant (TExpr expr, Dummy data)
                {
                        int value;
                        if (Decoder.IsConstantInt (expr, out value))
                                return false;

                        Thresholds.Add (value);
                        return true;
                }

                public override bool VisitLessThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitLessEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitGreaterThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitGreaterEqualThan (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitNotEqual (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                public override bool VisitEqual (TExpr left, TExpr right, TExpr original, Dummy data)
                {
                        return VisitBinary (left, right, data);
                }

                bool VisitBinary (TExpr left, TExpr right, Dummy data)
                {
                        var gatheredFromLeft = Visit (left, data);
                        var gatheredFromRight = Visit (right, data);

                        return gatheredFromLeft || gatheredFromRight;
                }
        }
}