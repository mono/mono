// 
// Analysis.ConstantEvaluator.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static partial class AnalysisFacade {
                static partial class Bind<TVar, TExpr>
                        where TExpr : IEquatable<TExpr>
                        where TVar : IEquatable<TVar> {
                        class ConstantEvaluator {
                                readonly IExpressionContextProvider<TExpr, TVar> context_provider;
                                readonly IMetaDataProvider meta_data_provider;

                                public ConstantEvaluator (IExpressionContextProvider<TExpr, TVar> contextProvider,
                                                          IMetaDataProvider metaDataProvider)
                                {
                                        context_provider = contextProvider;
                                        meta_data_provider = metaDataProvider;
                                }

                                public bool TryEvaluateToConstant (APC pc, TVar dest, BinaryOperator op,
                                                                   BoxedExpression left, BoxedExpression right,
                                                                   out long value)
                                {
                                        var type =
                                                context_provider.ValueContext.GetType (
                                                        context_provider.MethodContext.CFG.Post (pc), dest);
                                        long l;
                                        long r;

                                        if (type.IsNormal () && TryEvaluateToConstant (pc, left, out l) &&
                                            TryEvaluateToConstant (pc, right, out r))
                                                return TryEvaluate (type.Value, op, l, r, out value);

                                        return false.Without (out value);
                                }

                                bool TryEvaluate (TypeNode type, BinaryOperator op, long l, long r, out long value)
                                {
                                        if (TryEvaluateIndependent (op, l, r, out value))
                                                return true;

                                        if (meta_data_provider.System_Int32.Equals (type))
                                                return TryEvaluateInt32 (op, (int) l, (int) r, out value);

                                        return false.Without (out value);
                                }

                                static bool TryEvaluateInt32 (BinaryOperator op, int l, int r, out long value)
                                {
                                        int result;
                                        if (EvaluateArithmeticWithOverflow.TryBinary (op.ToExpressionOperator (), l, r,
                                                                                      out result))
                                                return true.With (result, out value);

                                        return false.Without (out value);
                                }

                                bool TryEvaluateIndependent (BinaryOperator op, long l, long r, out long result)
                                {
                                        switch (op) {
                                        case BinaryOperator.And:
                                                return true.With (l & r, out result);
                                        case BinaryOperator.Ceq:
                                                return true.With (ToInt (l == r), out result);
                                        case BinaryOperator.Cne_Un:
                                                return true.With (ToInt (l != r), out result);
                                        case BinaryOperator.Cge:
                                                return true.With (ToInt (l >= r), out result);
                                        case BinaryOperator.Cgt:
                                                return true.With (ToInt (l > r), out result);
                                        case BinaryOperator.Cle:
                                                return true.With (ToInt (l <= r), out result);
                                        case BinaryOperator.Clt:
                                                return true.With (ToInt (l < r), out result);
                                        case BinaryOperator.LogicalAnd:
                                                return true.With (ToInt (l != 0 && r != 0), out result);
                                        case BinaryOperator.LogicalOr:
                                                return true.With (ToInt (l != 0 || r != 0), out result);
                                        case BinaryOperator.Or:
                                                return true.With (l | r, out result);
                                        case BinaryOperator.Xor:
                                                return true.With (l ^ r, out result);
                                        default:
                                                return false.Without (out result);
                                        }
                                }

                                static long ToInt (bool value)
                                {
                                        return value ? 1 : 0;
                                }

                                static bool TryEvaluateToConstant (APC pc, BoxedExpression e, out long result)
                                {
                                        int res;
                                        if (e.IsConstantIntOrNull (out res))
                                                return true.With (res, out result);

                                        long argValue;
                                        if (e.IsUnary && TryEvaluateToConstant (pc, e.UnaryArgument, out argValue)) {
                                                switch (e.UnaryOperator) {
                                                case UnaryOperator.Neg:
                                                        return true.With (-argValue, out result);
                                                case UnaryOperator.Not:
                                                        return true.With (argValue != 0 ? 0L : 1L, out result);
                                                default:
                                                        throw new ArgumentOutOfRangeException ();
                                                }
                                        }

                                        return false.Without (out result);
                                }
                        }
                 }
        }
}