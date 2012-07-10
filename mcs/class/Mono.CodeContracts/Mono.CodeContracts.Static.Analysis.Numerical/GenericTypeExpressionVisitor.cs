namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal abstract class GenericTypeExpressionVisitor<Var, Expr, In, Out>
    {
        protected IExpressionDecoder<Var, Expr> Decoder { get; private set; }

        protected GenericTypeExpressionVisitor (IExpressionDecoder<Var, Expr> decoder)
        {
            this.Decoder = decoder;
        }

        public virtual Out Visit(Expr e, In input)
        {
            switch (Decoder.TypeOf(e))
            {
                case ExpressionType.Unknown:
                    return this.Default (e, input);
                case ExpressionType.Int32:
                    return this.VisitInt32 (e, input);
                case ExpressionType.Bool:
                    return this.VisitBool (e, input);
                default:
                    throw new AbstractInterpretationException ("Unknown type for expressions " + Decoder.TypeOf (e));
            }
        }

        protected virtual Out VisitBool (Expr expr, In input)
        {
            return this.Default (expr, input);
        }
        protected virtual Out VisitInt32 (Expr expr, In input)
        {
            return this.Default(expr, input);
        }

        protected abstract Out Default (Expr expr, In input);
    }
}