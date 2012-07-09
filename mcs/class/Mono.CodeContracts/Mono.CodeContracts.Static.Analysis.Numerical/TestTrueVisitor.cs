namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class TestTrueVisitor<Domain, Var, Expr> : GenericNormalizingExpressionVisistor<Domain, Var, Expr>
        where Domain : IAbstractDomainForEnvironments<Domain, Var, Expr>
    {
        private Domain result;

        public TestFalseVisitor<Domain, Var,Expr> FalseVisitor { get; set; }

        protected TestTrueVisitor (IExpressionDecoder<Var, Expr> decoder)
            : base (decoder)
        {
        }

        protected override Domain Default(Domain data)
        {
            return data;
        }

        public override Domain VisitConstant(Expr left, Domain data)
        {
            bool valueBool;
            int valueInt;
            
            result = data;
            if (this.Decoder.TryValueOf(left, ExpressionType.Bool, out valueBool))
                result = valueBool ? data : data.Bottom;
            else if (this.Decoder.TryValueOf(left, ExpressionType.Int32, out valueInt))
                result = valueInt != 0 ? data : data.Bottom;

            return result;
        }

        public override Domain VisitNot(Expr expr, Domain data)
        {
            return FalseVisitor.Visit (expr, data);
        }
    }
}