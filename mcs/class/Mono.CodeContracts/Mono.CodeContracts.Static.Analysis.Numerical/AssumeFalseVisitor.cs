namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class AssumeFalseVisitor<Domain, Var, Expr> : GenericExpressionVisitor<Domain, Domain, Var, Expr>
        where Domain : IAbstractDomainForEnvironments<Domain, Var, Expr>
    {
        private Domain result;

        public AssumeTrueVisitor<Domain, Var, Expr> TrueVisitor { get; set; }

        protected AssumeFalseVisitor(IExpressionDecoder<Var, Expr> decoder)
            : base(decoder)
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
            return TrueVisitor.Visit (expr, data);
        }

        public override Domain VisitEqual(Expr left, Expr right, Expr original, Domain data)
        {
            int value;
            if (Decoder.TryValueOf(right, ExpressionType.Int32, out value) && value == 0) // test (left :neq: 0) ==> test (left)
                return TrueVisitor.Visit (left, data);

            return TrueVisitor.VisitNotEqual (left, right, original, data);
        }

        public override Domain VisitLessThan(Expr left, Expr right, Expr original, Domain data)
        {// !(left < right) ==> right <= left
            return TrueVisitor.VisitLessEqualThan (right, left, original, data);
        }

        public override Domain VisitLessEqualThan(Expr left, Expr right, Expr original, Domain data)
        {// !(left <= right) ==> right < left
            return TrueVisitor.VisitLessThan (right, left, original, data);
        }
    }
}