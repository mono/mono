using System;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class IntervalTestFalseVisitor<TEnv, Var, Expr, TInterval, TNumeric> : AssumeFalseVisitor<TEnv, Var, Expr>
        where TEnv : IntervalEnvironmentBase<TEnv, Var, Expr, TInterval, TNumeric>
        where TInterval : IntervalBase<TInterval, TNumeric> 
        where Var : IEquatable<Var> {
        public IntervalTestFalseVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
        {
        }

        public override TEnv Visit(Expr expr, TEnv data)
        {
            TEnv res = base.Visit(expr, data);

            if (!Decoder.IsBinaryExpression(expr))
                return res;

            var left = Decoder.LeftExpressionFor (expr);
            var right = Decoder.RightExpressionFor (expr);

            var intv = data.Eval (right);
            if (intv.IsBottom)
                return data.Bottom;
            if (!intv.IsSinglePoint)
                return res;

            switch (Decoder.OperatorFor(expr))
            {
                case ExpressionOperator.LessThan:
                    {
                        var leftVar = Decoder.UnderlyingVariable (left);
                        return res.Assumer.AssumeLessEqualThan (intv, leftVar);
                    }
                case ExpressionOperator.LessEqualThan:
                    {
                        var leftVar = Decoder.UnderlyingVariable(left);
                        return res.Assumer.AssumeLessThan(intv, leftVar);
                    }
            }

            return data;
        }


        protected override TEnv DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, TEnv data)
        {
            data = cmp(left, right, original, data);
            return base.DispatchCompare(cmp, left, right, original, data);
        }
    }
}