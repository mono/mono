namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal abstract class GenericTypeExpressionVisitor<TVariable, TExpression, TIn, TOut>
    {
        protected readonly IExpressionDecoder<TVariable, TExpression> Decoder;

        protected GenericTypeExpressionVisitor (IExpressionDecoder<TVariable, TExpression> decoder)
        {
            this.Decoder = decoder;
        }

        public    virtual TOut Visit(TExpression e, TIn input)
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

        protected virtual TOut VisitBool  (TExpression expr, TIn input)
        {
            return this.Default (expr, input);
        }
        protected virtual TOut VisitInt32 (TExpression expr, TIn input)
        {
            return this.Default(expr, input);
        }

        protected abstract TOut Default   (TExpression expr, TIn input);
    }
}