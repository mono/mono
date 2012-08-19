using System;

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class BoxedExpressionEncoder<TVar> : IExpressionEncoder<BoxedVariable<TVar>, BoxedExpression> {
                readonly IMetaDataProvider metadata;

                public BoxedExpressionEncoder (IMetaDataProvider metadata)
                {
                        this.metadata = metadata;
                }

                public void ResetFreshVariableCounter ()
                {
                        BoxedVariable<TVar>.ResetFreshVariableCounter ();
                }

                public BoxedVariable<TVar> FreshVariable ()
                {
                        return BoxedVariable<TVar>.SlackVariable ();
                }

                public BoxedExpression VariableFor (BoxedVariable<TVar> var)
                {
                        return BoxedExpression.Var (var);
                }

                public BoxedExpression ConstantFor (object value)
                {
                        if (value is int)
                                return BoxedExpression.Const (value, metadata.System_Int32);

                        if (value is long) {
                                var val = (long) value;
                                return val < int.MaxValue && val > int.MinValue
                                               ? BoxedExpression.Const (val, metadata.System_Int32)
                                               : BoxedExpression.Const (val, metadata.System_Int64);
                        }

                        if (value is short)
                                return BoxedExpression.Const (value, metadata.System_Int16);
                        if (value is sbyte)
                                return BoxedExpression.Const (value, metadata.System_Int8);
                        if (value is bool)
                                return BoxedExpression.Const (value, metadata.System_Boolean);

                        throw new NotSupportedException ();
                }

                public BoxedExpression CompoundFor (ExpressionType type, ExpressionOperator op, BoxedExpression arg)
                {
                        return BoxedExpression.Unary (ToUnaryOperator (op), arg);
                }

                public BoxedExpression CompoundFor (ExpressionType type, ExpressionOperator op, BoxedExpression left,
                                                    BoxedExpression right)
                {
                        return BoxedExpression.Binary (ToBinaryOperator (op), left, right);
                }

                public static IExpressionEncoder<BoxedVariable<TVar>, BoxedExpression> Encoder (
                        IMetaDataProvider provider)
                {
                        return new BoxedExpressionEncoder<TVar> (provider);
                }

                BinaryOperator ToBinaryOperator (ExpressionOperator op)
                {
                        switch (op) {
                        case ExpressionOperator.And:
                                return BinaryOperator.And;
                        case ExpressionOperator.Or:
                                return BinaryOperator.Or;
                        case ExpressionOperator.Xor:
                                return BinaryOperator.Xor;
                        case ExpressionOperator.Equal:
                                return BinaryOperator.Ceq;
                        case ExpressionOperator.Equal_Obj:
                                return BinaryOperator.Cobjeq;
                        case ExpressionOperator.NotEqual:
                                return BinaryOperator.Cne_Un;
                        case ExpressionOperator.LessThan:
                                return BinaryOperator.Clt;
                        case ExpressionOperator.LessEqualThan:
                                return BinaryOperator.Cle_Un;
                        case ExpressionOperator.GreaterThan:
                                return BinaryOperator.Cgt;
                        case ExpressionOperator.GreaterEqualThan:
                                return BinaryOperator.Cge;
                        case ExpressionOperator.Add:
                                return BinaryOperator.Add;
                        case ExpressionOperator.Sub:
                                return BinaryOperator.Sub;
                        case ExpressionOperator.Mult:
                                return BinaryOperator.Mul;
                        case ExpressionOperator.Div:
                                return BinaryOperator.Div;
                        case ExpressionOperator.Mod:
                                return BinaryOperator.Rem;

                        default:
                                throw new ArgumentOutOfRangeException ("op");
                        }
                }

                UnaryOperator ToUnaryOperator (ExpressionOperator op)
                {
                        switch (op) {
                        case ExpressionOperator.Not:
                                return UnaryOperator.Not;
                        case ExpressionOperator.UnaryMinus:
                                return UnaryOperator.Neg;
                        default:
                                throw new ArgumentOutOfRangeException ("op");
                        }
                }
        }
}