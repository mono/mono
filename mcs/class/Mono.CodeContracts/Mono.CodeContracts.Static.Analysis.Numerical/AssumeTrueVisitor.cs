using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class AssumeTrueVisitor<TDomain, TVar, TExpr> : GenericExpressionVisitor<TDomain, TDomain, TVar, TExpr>
                where TDomain : IEnvironmentDomain<TDomain, TVar, TExpr> {
                protected AssumeTrueVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public AssumeFalseVisitor<TDomain, TVar, TExpr> FalseVisitor { get; set; }

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

                public override TDomain VisitLogicalAnd (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        var leftIsVariable = Decoder.IsVariable (left);
                        var rightIsVariable = Decoder.IsVariable (right);

                        var leftIsConstant = Decoder.IsConstant (left);
                        var rightIsConstant = Decoder.IsConstant (right);

                        if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                                return data;

                        return data.AssumeTrue (left).AssumeTrue (right);
                }

                public override TDomain VisitLogicalOr (TExpr left, TExpr right, TExpr original, TDomain data)
                {
                        var leftIsVariable = Decoder.IsVariable (left);
                        var rightIsVariable = Decoder.IsVariable (right);

                        var leftIsConstant = Decoder.IsConstant (left);
                        var rightIsConstant = Decoder.IsConstant (right);

                        if (leftIsVariable && rightIsConstant || leftIsConstant && rightIsVariable)
                                return data;

                        var leftBranch = data.AssumeTrue (left);
                        var rightBranch = data.AssumeTrue (right);

                        return leftBranch.Join (rightBranch);
                }

                public override TDomain VisitNot (TExpr expr, TDomain data)
                {
                        return FalseVisitor.Visit (expr, data);
                }

                protected override bool TryPolarity (TExpr expr, TDomain data, out bool shouldNegate)
                {
                        if (base.TryPolarity (expr, data, out shouldNegate))
                                return true;

                        var holds = data.CheckIfHolds (expr);
                        if (!holds.IsNormal ())
                                return false.Without (out shouldNegate);

                        return true.With (!holds.Value, out shouldNegate);
                }
                }
}