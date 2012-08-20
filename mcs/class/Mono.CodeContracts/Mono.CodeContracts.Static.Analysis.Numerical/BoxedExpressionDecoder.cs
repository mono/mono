// 
// BoxedExpressionDecoder.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class BoxedExpressionDecoder<TVar, TExpr> : IExpressionDecoder<BoxedVariable<TVar>, BoxedExpression> {
                public IFullExpressionDecoder<TVar, TExpr> ExternalDecoder { get; private set; }

                public BoxedExpressionDecoder (IFullExpressionDecoder<TVar, TExpr> externalDecoder)
                {
                        ExternalDecoder = externalDecoder;
                }

                public ExpressionOperator OperatorFor (BoxedExpression expr)
                {
                        if (expr.IsVariable)
                                return ExpressionOperator.Variable;
                        if (expr.IsConstant)
                                return ExpressionOperator.Constant;
                        if (expr.IsSizeof)
                                return ExpressionOperator.SizeOf;
                        if (expr.IsUnary)
                                switch (expr.UnaryOperator) {
                                case UnaryOperator.Conv_i:
                                case UnaryOperator.Conv_i4:
                                case UnaryOperator.Conv_i8:
                                        return ExpressionOperator.ConvertToInt32;
                                case UnaryOperator.Neg:
                                        return ExpressionOperator.UnaryMinus;
                                case UnaryOperator.Not:
                                        return ExpressionOperator.Not;
                                default:
                                        return ExpressionOperator.Unknown;
                                }

                        if (!expr.IsBinary)
                                return ExpressionOperator.Unknown;

                        switch (expr.BinaryOperator) {
                        case BinaryOperator.Add:
                                return ExpressionOperator.Add;
                        case BinaryOperator.And:
                                return ExpressionOperator.And;
                        case BinaryOperator.Ceq:
                                return ExpressionOperator.Equal;
                        case BinaryOperator.Cobjeq:
                                return ExpressionOperator.Equal_Obj;
                        case BinaryOperator.Cne_Un:
                                return ExpressionOperator.NotEqual;
                        case BinaryOperator.Cge:
                                return ExpressionOperator.GreaterEqualThan;
                        case BinaryOperator.Cgt:
                                return ExpressionOperator.GreaterThan;
                        case BinaryOperator.Cle:
                                return ExpressionOperator.LessEqualThan;
                        case BinaryOperator.Clt:
                                return ExpressionOperator.LessThan;
                        case BinaryOperator.Div:
                                return ExpressionOperator.Div;
                        case BinaryOperator.LogicalAnd:
                                return ExpressionOperator.LogicalAnd;
                        case BinaryOperator.LogicalOr:
                                return ExpressionOperator.LogicalOr;
                        case BinaryOperator.Mul:
                                return ExpressionOperator.Mult;
                        case BinaryOperator.Or:
                                return ExpressionOperator.Or;
                        case BinaryOperator.Rem:
                                return ExpressionOperator.Mod;
                        case BinaryOperator.Sub:
                                return ExpressionOperator.Sub;
                        case BinaryOperator.Xor:
                                return ExpressionOperator.Xor;
                        default:
                                return ExpressionOperator.Unknown;
                        }
                }

                public BoxedExpression LeftExpressionFor (BoxedExpression expr)
                {
                        if (expr.IsBinary)
                                return expr.BinaryLeftArgument;
                        if (expr.IsUnary)
                                return expr.UnaryArgument;

                        throw new InvalidOperationException ();
                }

                public BoxedExpression RightExpressionFor (BoxedExpression expr)
                {
                        if (expr.IsBinary)
                                return expr.BinaryRightArgument;

                        throw new InvalidOperationException ();
                }

                public ExpressionType TypeOf (BoxedExpression expr)
                {
                        if (expr.IsConstant) {
                                var constant = expr.Constant;
                                if (constant == null)
                                        return ExpressionType.Unknown;

                                var convertible = constant as IConvertible;
                                if (convertible != null)
                                        switch (convertible.GetTypeCode ()) {
                                        case TypeCode.Boolean:
                                                return ExpressionType.Bool;
                                        case TypeCode.Int32:
                                                return ExpressionType.Int32;
                                        case TypeCode.Single:
                                                return ExpressionType.Float32;
                                        case TypeCode.Double:
                                                return ExpressionType.Float64;
                                        }

                                return ExpressionType.Unknown;
                        }

                        if (expr.IsUnary) {
                                switch (expr.UnaryOperator) {
                                case UnaryOperator.Conv_i4:
                                        return ExpressionType.Int32;
                                case UnaryOperator.Conv_r4:
                                        return ExpressionType.Float32;
                                case UnaryOperator.Conv_r8:
                                case UnaryOperator.Conv_r_un:
                                        return ExpressionType.Float64;
                                case UnaryOperator.Not:
                                        return ExpressionType.Bool;
                                default:
                                        return ExpressionType.Int32;
                                }
                        }

                        if (expr.IsBinary)
                                switch (expr.BinaryOperator) {
                                case BinaryOperator.Add:
                                case BinaryOperator.Add_Ovf:
                                case BinaryOperator.Add_Ovf_Un:
                                case BinaryOperator.Div:
                                case BinaryOperator.Div_Un:
                                case BinaryOperator.Mul:
                                case BinaryOperator.Mul_Ovf:
                                case BinaryOperator.Mul_Ovf_Un:
                                case BinaryOperator.Rem:
                                case BinaryOperator.Rem_Un:
                                case BinaryOperator.Sub:
                                case BinaryOperator.Sub_Ovf:
                                case BinaryOperator.Sub_Ovf_Un:
                                        return Join (TypeOf (expr.BinaryLeftArgument),
                                                     TypeOf (expr.BinaryRightArgument));
                                case BinaryOperator.Ceq:
                                case BinaryOperator.Cobjeq:
                                case BinaryOperator.Cne_Un:
                                case BinaryOperator.Cge:
                                case BinaryOperator.Cge_Un:
                                case BinaryOperator.Cgt:
                                case BinaryOperator.Cgt_Un:
                                case BinaryOperator.Cle:
                                case BinaryOperator.Cle_Un:
                                case BinaryOperator.Clt:
                                case BinaryOperator.Clt_Un:
                                        return ExpressionType.Bool;
                                default:
                                        return ExpressionType.Int32;
                                }

                        return ExpressionType.Unknown;
                }

                public BoxedVariable<TVar> UnderlyingVariable (BoxedExpression expr)
                {
                        var uv = expr.UnderlyingVariable;
                        if (uv is TVar)
                                return new BoxedVariable<TVar> ((TVar) expr.UnderlyingVariable);

                        var boxed = uv as BoxedVariable<TVar>;
                        return boxed ?? BoxedVariable<TVar>.SlackVariable ();
                }

                public bool IsConstant (BoxedExpression expr)
                {
                        return expr.IsConstant;
                }

                public bool IsVariable (BoxedExpression expr)
                {
                        return expr.IsVariable;
                }

                public bool TryValueOf<T> (BoxedExpression expr, ExpressionType type, out T result)
                {
                        if (!expr.IsConstant)
                                return false.Without (out result);

                        var constant = expr.Constant;
                        if (constant == null)
                                return false.Without (out result);

                        if (constant is T)
                                return true.With ((T) constant, out result);

                        if (constant is string)
                                return false.Without (out result);

                        var convertible = constant as IConvertible;
                        if (convertible != null)
                                try {
                                        return true.With ((T) convertible.ToType (typeof (T), null), out result);
                                }
                                catch {
                                }

                        return false.Without (out result);
                }

                public bool TrySizeOf (BoxedExpression expr, out int size)
                {
                        return expr.Sizeof (out size);
                }

                public bool IsNull (BoxedExpression expr)
                {
                        return expr.IsConstant && expr.IsNull;
                }

                public bool IsConstantInt (BoxedExpression expr, out int value)
                {
                        return expr.IsConstantIntOrNull (out value);
                }

                public string NameOf (BoxedVariable<TVar> variable)
                {
                        return variable.ToString ();
                }

                public bool IsBinaryExpression (BoxedExpression expr)
                {
                        return expr.IsBinary;
                }

                ExpressionType Join (ExpressionType left, ExpressionType right)
                {
                        if (left == right)
                                return left;
                        if (left == ExpressionType.Unknown)
                                return right;
                        if (right == ExpressionType.Unknown)
                                return left;

                        if (left == ExpressionType.Float32 || right == ExpressionType.Float32)
                                return ExpressionType.Float32;
                        if (left == ExpressionType.Float64 || right == ExpressionType.Float64)
                                return ExpressionType.Float64;

                        return left;
                }
        }
}