namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class GenericTypeExpressionVisitor<TVariable, TExpression, TIn, TOut> {
                protected readonly IExpressionDecoder<TVariable, TExpression> Decoder;

                protected GenericTypeExpressionVisitor (IExpressionDecoder<TVariable, TExpression> decoder)
                {
                        Decoder = decoder;
                }

                public virtual TOut Visit (TExpression e, TIn input)
                {
                        switch (Decoder.TypeOf (e)) {
                        case ExpressionType.Unknown:
                                return Default (e, input);
                        case ExpressionType.Int32:
                                return VisitInt32 (e, input);
                        case ExpressionType.Bool:
                                return VisitBool (e, input);
                        default:
                                throw new AbstractInterpretationException ("Unknown type for expressions " +
                                                                           Decoder.TypeOf (e));
                        }
                }

                protected virtual TOut VisitBool (TExpression expr, TIn input)
                {
                        return Default (expr, input);
                }

                protected virtual TOut VisitInt32 (TExpression expr, TIn input)
                {
                        return Default (expr, input);
                }

                protected abstract TOut Default (TExpression expr, TIn input);
        }
}