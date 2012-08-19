namespace Mono.CodeContracts.Static.Analysis.Numerical {
        interface IExpressionDecoder<TVariable, TExpression> {
                ExpressionOperator OperatorFor (TExpression expr);
                TExpression LeftExpressionFor (TExpression expr);
                TExpression RightExpressionFor (TExpression expr);

                TVariable UnderlyingVariable (TExpression expr);

                bool IsConstant (TExpression expr);
                bool IsVariable (TExpression expr);

                bool TryValueOf<T> (TExpression left, ExpressionType type, out T result);

                bool TrySizeOf (TExpression expr, out int size);

                ExpressionType TypeOf (TExpression expr);

                bool IsNull (TExpression expr);

                bool IsConstantInt (TExpression expr, out int value);
                string NameOf (TVariable variable);

                bool IsBinaryExpression (TExpression expr);
        }
}