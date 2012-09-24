// 
// ValueExpressionDecoder.cs
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
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class ValueExpressionDecoder<TVar, TExpr> : FullExpressionDecoder<TVar, TExpr>, IExpressionDecoder<TVar, TExpr>
                where TVar : IEquatable<TVar>
                where TExpr : IEquatable<TExpr> {
                readonly VisitorForOperatorFor operator_for;
                readonly VisitorForTypeOf type_of;

                public ValueExpressionDecoder (IMetaDataProvider metaDataProvider, IExpressionContextProvider<TExpr, TVar> contextProvider)
                        : base (metaDataProvider, contextProvider)
                {
                        operator_for = new VisitorForOperatorFor ();
                        type_of = new VisitorForTypeOf ();
                }

                IExpressionContext<TExpr, TVar> ExpressionContext { get { return ContextProvider.ExpressionContext; } }

                #region IExpressionDecoder<TVar,TExpr> Members

                public ExpressionOperator OperatorFor (TExpr expr)
                {
                        return ExpressionContext.Decode<TExpr, ExpressionOperator, VisitorForOperatorFor> (expr, operator_for, expr);
                }

                public TExpr LeftExpressionFor (TExpr expr)
                {
                        UnaryOperator op;
                        BinaryOperator bop;
                        TExpr left;
                        TExpr right;

                        if (IsUnaryExpression (expr, out op, out left) || IsBinaryExpression (expr, out bop, out left, out right))
                                return left;

                        throw new InvalidOperationException ();
                }

                public TExpr RightExpressionFor (TExpr expr)
                {
                        BinaryOperator bop;
                        TExpr left;
                        TExpr right;
                        if (IsBinaryExpression (expr, out bop, out left, out right))
                                return right;

                        throw new InvalidOperationException ();
                }

                public bool IsConstant (TExpr expr)
                {
                        return OperatorFor (expr) == ExpressionOperator.Constant;
                }

                public bool IsVariable (TExpr expr)
                {
                        object variable;
                        return base.IsVariable (expr, out variable);
                }

                public bool TryValueOf<T> (TExpr e, ExpressionType expectedType, out T result)
                {
                        object value;
                        TypeNode actualType;
                        if (base.IsConstant (e, out value, out actualType)) {
                                if (value is T)
                                        return true.With ((T) value, out result);
                                if (value is int && expectedType == ExpressionType.Bool) {
                                        result = (T) (object) ((int) value != 0);
                                        return true;
                                }
                        }
                        return false.Without (out result);
                }

                public bool TrySizeOf (TExpr expr, out int size)
                {
                        TypeNode type;
                        if (VisitorForSizeOf<TVar, TExpr>.IsSizeOf (expr, out type, this)) {
                                size = MetaDataProvider.TypeSize (type);
                                return size != -1;
                        }

                        return false.Without (out size);
                }

                public ExpressionType TypeOf (TExpr expr)
                {
                        if (IsConstant (expr))
                                return ExpressionContext.Decode<Dummy, ExpressionType, VisitorForTypeOf> (expr, type_of, Dummy.Value);

                        var abstractType = ExpressionContext.GetType (expr);

                        if (abstractType.IsNormal ()) {
                                var type = abstractType.Value;

                                if (MetaDataProvider.IsPrimitive (type)) {
                                        if (MetaDataProvider.Equal (type, MetaDataProvider.System_Int32))
                                                return ExpressionType.Int32;
                                        if (MetaDataProvider.Equal (type, MetaDataProvider.System_Single))
                                                return ExpressionType.Float32;
                                        if (MetaDataProvider.Equal (type, MetaDataProvider.System_Double))
                                                return ExpressionType.Float64;
                                        if (MetaDataProvider.Equal (type, MetaDataProvider.System_Boolean))
                                                return ExpressionType.Bool;
                                }
                        }

                        return ExpressionType.Unknown;
                }

                public bool IsConstantInt (TExpr expr, out int value)
                {
                        TypeNode type;
                        object objValue;
                        if (IsConstant (expr, out objValue, out type))
                                return true.With ((int) objValue, out value);

                        return false.Without (out value);
                }

                public string NameOf (TVar variable)
                {
                        return variable.ToString ();
                }

                public bool IsBinaryExpression (TExpr expr)
                {
                        BinaryOperator op;
                        TExpr left;
                        TExpr right;
                        return IsBinaryExpression (expr, out op, out left, out right);
                }

                #endregion

                #region Nested type: VisitorForOperatorFor

                class VisitorForOperatorFor : ISymbolicExpressionVisitor<TExpr, TExpr, TVar, TExpr, ExpressionOperator> {
                        #region ISymbolicExpressionVisitor<TExpr,TExpr,TVar,TExpr,ExpressionOperator> Members

                        public ExpressionOperator Binary (TExpr pc, BinaryOperator bop, TVar dest, TExpr src1, TExpr src2, TExpr data)
                        {
                                switch (bop) {
                                case BinaryOperator.Add:
                                case BinaryOperator.Add_Ovf:
                                case BinaryOperator.Add_Ovf_Un:
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
                                case BinaryOperator.Cge_Un:
                                        return ExpressionOperator.GreaterEqualThan;
                                case BinaryOperator.Cgt:
                                case BinaryOperator.Cgt_Un:
                                        return ExpressionOperator.GreaterThan;
                                case BinaryOperator.Cle:
                                case BinaryOperator.Cle_Un:
                                        return ExpressionOperator.LessEqualThan;
                                case BinaryOperator.Clt:
                                case BinaryOperator.Clt_Un:
                                        return ExpressionOperator.LessThan;
                                case BinaryOperator.Div:
                                case BinaryOperator.Div_Un:
                                        return ExpressionOperator.Div;
                                case BinaryOperator.LogicalAnd:
                                        return ExpressionOperator.LogicalAnd;
                                case BinaryOperator.LogicalOr:
                                        return ExpressionOperator.LogicalOr;
                                case BinaryOperator.Mul:
                                case BinaryOperator.Mul_Ovf:
                                case BinaryOperator.Mul_Ovf_Un:
                                        return ExpressionOperator.Mult;
                                case BinaryOperator.Or:
                                        return ExpressionOperator.Or;
                                case BinaryOperator.Rem:
                                case BinaryOperator.Rem_Un:
                                        return ExpressionOperator.Mod;
                                case BinaryOperator.Sub:
                                case BinaryOperator.Sub_Ovf:
                                case BinaryOperator.Sub_Ovf_Un:
                                        return ExpressionOperator.Sub;
                                case BinaryOperator.Xor:
                                        return ExpressionOperator.Xor;
                                default:
                                        return ExpressionOperator.Unknown;
                                }
                        }

                        public ExpressionOperator Unary (TExpr pc, UnaryOperator uop, bool unsigned, TVar dest, TExpr source, TExpr data)
                        {
                                switch (uop) {
                                case UnaryOperator.Conv_i:
                                case UnaryOperator.Conv_i1:
                                case UnaryOperator.Conv_i2:
                                case UnaryOperator.Conv_i4:
                                case UnaryOperator.Conv_i8:
                                        return ExpressionOperator.ConvertToInt32;
                                case UnaryOperator.Neg:
                                        return ExpressionOperator.Not;
                                case UnaryOperator.Not:
                                        return ExpressionOperator.Not;
                                default:
                                        return ExpressionOperator.Unknown;
                                }
                        }

                        public ExpressionOperator LoadNull (TExpr pc, TVar dest, TExpr polarity)
                        {
                                return ExpressionOperator.Constant;
                        }

                        public ExpressionOperator LoadConst (TExpr pc, TypeNode type, object constant, TVar dest, TExpr data)
                        {
                                return ExpressionOperator.Constant;
                        }

                        public ExpressionOperator Sizeof (TExpr pc, TypeNode type, TVar dest, TExpr data)
                        {
                                return ExpressionOperator.SizeOf;
                        }

                        public ExpressionOperator Isinst (TExpr pc, TypeNode type, TVar dest, TExpr obj, TExpr data)
                        {
                                return ExpressionOperator.Unknown;
                        }

                        public ExpressionOperator SymbolicConstant (TExpr pc, TVar variable, TExpr data)
                        {
                                return ExpressionOperator.Variable;
                        }

                        #endregion
                }

                #endregion

                #region Nested type: VisitorForTypeOf

                class VisitorForTypeOf : ISymbolicExpressionVisitor<TExpr, TExpr, TVar, Dummy, ExpressionType> {
                        #region ISymbolicExpressionVisitor<TExpr,TExpr,TVar,Dummy,ExpressionType> Members

                        public ExpressionType Binary (TExpr pc, BinaryOperator bop, TVar dest, TExpr src1, TExpr src2, Dummy data)
                        {
                                return ExpressionType.Unknown;
                        }

                        public ExpressionType Unary (TExpr pc, UnaryOperator uop, bool unsigned, TVar dest, TExpr source, Dummy data)
                        {
                                return ExpressionType.Unknown;
                        }

                        public ExpressionType LoadNull (TExpr pc, TVar dest, Dummy polarity)
                        {
                                return ExpressionType.Unknown;
                        }

                        public ExpressionType LoadConst (TExpr pc, TypeNode type, object constant, TVar dest, Dummy data)
                        {
                                if (constant is int)
                                        return ExpressionType.Int32;
                                if (constant is float)
                                        return ExpressionType.Float32;
                                if (constant is double)
                                        return ExpressionType.Float64;
                                if (constant is bool)
                                        return ExpressionType.Bool;

                                return ExpressionType.Unknown;
                        }

                        public ExpressionType Sizeof (TExpr pc, TypeNode type, TVar dest, Dummy data)
                        {
                                return ExpressionType.Int32;
                        }

                        public ExpressionType Isinst (TExpr pc, TypeNode type, TVar dest, TExpr obj, Dummy data)
                        {
                                return ExpressionType.Unknown;
                        }

                        public ExpressionType SymbolicConstant (TExpr pc, TVar variable, Dummy data)
                        {
                                return ExpressionType.Unknown;
                        }

                        #endregion
                }

                #endregion
        }
}