namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal interface IExpressionDecoder<TVariable, TExpression>
    {
        ExpressionOperator OperatorFor (TExpression expr);

        TVariable UnderlyingVariable (TExpression expr);

        TExpression LeftExpressionFor (TExpression expr);

        TExpression RightExpressionFor (TExpression expr);

        bool IsConstant (TExpression expr);
        bool IsVariable (TExpression expr);

        bool TryValueOf<T> (TExpression left, ExpressionType type, out T num);

        bool TrySizeOf (TExpression expr, out int size);

        ExpressionType TypeOf (TExpression expr);

        bool IsNull (TExpression expr);
        
        bool IsConstantInt (TExpression right, out int value);
    }
}