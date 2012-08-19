namespace Mono.CodeContracts.Static.Analysis.Numerical {
        interface IExpressionEncoder<TVar, TExpr> {
                void ResetFreshVariableCounter ();
                TVar FreshVariable ();
                TExpr VariableFor (TVar var);
                TExpr ConstantFor (object value);
                TExpr CompoundFor (ExpressionType type, ExpressionOperator op, TExpr arg);
                TExpr CompoundFor (ExpressionType type, ExpressionOperator op, TExpr left, TExpr right);
        }
}