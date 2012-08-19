namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class AssumeFalseVisitor<TDomain, TVar, TExpr> :
                GenericExpressionVisitor<TDomain, TDomain, TVar, TExpr>
                where TDomain : IEnvironmentDomain<TDomain, TVar, TExpr> {
                protected AssumeFalseVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public AssumeTrueVisitor<TDomain, TVar, TExpr> TrueVisitor { get; set; }

                protected override TDomain Default (TDomain data)
                {
                        return data;
                }

                public override TDomain VisitConstant (TExpr left, TDomain data)
                {
                        bool boolValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Bool, out boolValue))
                                return boolValue ? data : data.Bottom;

                        int intValue;
                        if (Decoder.TryValueOf (left, ExpressionType.Int32, out intValue))
                                return intValue != 0 ? data : data.Bottom;

                        return data;
                }

                public override TDomain VisitNot (TExpr expr, TDomain data)
                {
                        return TrueVisitor.Visit (expr, data);
                }

                public override TDomain VisitEqual (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        int value;
                        if (Decoder.TryValueOf (right, ExpressionType.Int32, out value) && value == 0)
                                // test (left :neq: 0) ==> test (left)
                                return TrueVisitor.Visit (left, data);

                        return TrueVisitor.VisitNotEqual (left, right, original, data);
                }

                public override TDomain VisitLessThan (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        // !(left < right) ==> right <= left
                        return TrueVisitor.VisitLessEqualThan (right, left, original, data);
                }

                public override TDomain VisitLessEqualThan (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        // !(left <= right) ==> right < left
                        return TrueVisitor.VisitLessThan (right, left, original, data);
                }
                }
}