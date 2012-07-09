namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class TestFalseVisitor<Domain, Var, Expr> : GenericNormalizingExpressionVisistor<Domain, Var, Expr>
        where Domain : IAbstractDomainForEnvironments<Domain, Var, Expr>
    {
        private Domain result;

        public TestTrueVisitor<Domain, Var, Expr> TrueVisitor { get; set; }

        protected TestFalseVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
        {
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
            return TrueVisitor.Visit (expr, data);
        }
    }
}