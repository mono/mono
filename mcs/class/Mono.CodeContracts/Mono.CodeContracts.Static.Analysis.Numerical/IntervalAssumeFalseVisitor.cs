using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class IntervalAssumeFalseVisitor<TVar, TExpr, TInterval, TNumeric> :
                AssumeFalseVisitor<IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric>, TVar, TExpr>
                where TInterval : IntervalBase<TInterval, TNumeric>
                where TVar : IEquatable<TVar> {
                public IntervalAssumeFalseVisitor (IExpressionDecoder<TVar, TExpr> decoder)
                        : base (decoder)
                {
                }

                public override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> Visit (TExpr expr,
                                                                                               IntervalEnvironmentBase
                                                                                                       <TVar, TExpr,
                                                                                                       TInterval,
                                                                                                       TNumeric> data)
                {
                        var res = base.Visit (expr, data);

                        if (!Decoder.IsBinaryExpression (expr))
                                return res;

                        var left = Decoder.LeftExpressionFor (expr);
                        var right = Decoder.RightExpressionFor (expr);

                        var intv = data.Eval (right);
                        if (intv.IsBottom)
                                return data.Bottom;
                        if (!intv.IsSinglePoint)
                                return res;

                        switch (Decoder.OperatorFor (expr)) {
                        case ExpressionOperator.LessThan: {
                                var leftVar = Decoder.UnderlyingVariable (left);
                                return res.Assumer.AssumeLessEqualThan (intv, leftVar, res);
                        }
                        case ExpressionOperator.LessEqualThan: {
                                var leftVar = Decoder.UnderlyingVariable (left);
                                return res.Assumer.AssumeLessThan (intv, leftVar, res);
                        }
                        }

                        return data;
                }

                protected override IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> DispatchCompare (
                        CompareVisitor cmp, TExpr left, TExpr right, TExpr original,
                        IntervalEnvironmentBase<TVar, TExpr, TInterval, TNumeric> data)
                {
                        data = cmp (left, right, original, data);
                        return base.DispatchCompare (cmp, left, right, original, data);
                }
                }
}