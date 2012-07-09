namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class GenericNormalizingExpressionVisistor<Data, Var, Expr> : GenericExpressionVisitor<Data, Data, Var, Expr>
    {
        protected GenericNormalizingExpressionVisistor (IExpressionDecoder<Var, Expr> decoder)
            : base (decoder)
        {
        }

        public override Data VisitGreaterThan(Expr left, Expr right, Expr original, Data data)
        {
            return VisitLessEqualThan (right, left, original, data);
        }
        public override Data VisitGreaterEqualThan(Expr left, Expr right, Expr original, Data data)
        {
            return VisitLessThan (right, left, original, data);
        }
    }
}