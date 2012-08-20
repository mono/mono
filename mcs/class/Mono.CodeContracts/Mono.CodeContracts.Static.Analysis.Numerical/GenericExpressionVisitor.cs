//
// GenericExpressionVisitor.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using Mono.CodeContracts.Static.DataStructures;

using Op = Mono.CodeContracts.Static.Analysis.Numerical.ExpressionOperator;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        abstract class GenericExpressionVisitor<In, Out, Var, Expr> {
                protected readonly IExpressionDecoder<Var, Expr> Decoder;

                protected GenericExpressionVisitor (IExpressionDecoder<Var, Expr> decoder)
                {
                        Decoder = decoder;
                }

                public virtual Out Visit (Expr expr, In data)
                {
                        var op = Decoder.OperatorFor (expr);
                        switch (op) {
                        case Op.Constant:
                                return VisitConstant (expr, data);
                        case Op.Variable:
                                return VisitVariable (Decoder.UnderlyingVariable (expr), expr, data);
                        case Op.Not:
                                return DispatchVisitNot (Decoder.LeftExpressionFor (expr), data);
                        case Op.And:
                                return VisitAnd (Decoder.LeftExpressionFor (expr), Decoder.RightExpressionFor (expr),
                                                 expr, data);
                        case Op.Or:
                                return VisitOr (Decoder.LeftExpressionFor (expr), Decoder.RightExpressionFor (expr),
                                                expr, data);
                        case Op.Xor:
                                return VisitXor (Decoder.LeftExpressionFor (expr), Decoder.RightExpressionFor (expr),
                                                 expr, data);
                        case Op.LogicalAnd:
                                return VisitLogicalAnd (Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);
                        case Op.LogicalOr:
                                return VisitLogicalOr (Decoder.LeftExpressionFor (expr),
                                                       Decoder.RightExpressionFor (expr), expr, data);
                        case Op.NotEqual:
                                return VisitNotEqual (Decoder.LeftExpressionFor (expr),
                                                      Decoder.RightExpressionFor (expr), expr, data);

                        case Op.Equal:
                        case Op.Equal_Obj:
                                return DispatchVisitEqual (op, Decoder.LeftExpressionFor (expr),
                                                           Decoder.RightExpressionFor (expr), expr, data);

                        case Op.LessThan:
                                return DispatchCompare (VisitLessThan, Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);
                        case Op.LessEqualThan:
                                return DispatchCompare (VisitLessEqualThan, Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);
                        case Op.GreaterThan:
                                return DispatchCompare (VisitGreaterThan, Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);
                        case Op.GreaterEqualThan:
                                return DispatchCompare (VisitGreaterEqualThan, Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);

                        case Op.Add:
                                return VisitAddition (Decoder.LeftExpressionFor (expr),
                                                      Decoder.RightExpressionFor (expr), expr, data);
                        case Op.Div:
                                return VisitDivision (Decoder.LeftExpressionFor (expr),
                                                      Decoder.RightExpressionFor (expr), expr, data);
                        case Op.Sub:
                                return VisitSubtraction (Decoder.LeftExpressionFor (expr),
                                                         Decoder.RightExpressionFor (expr), expr, data);
                        case Op.Mod:
                                return VisitModulus (Decoder.LeftExpressionFor (expr), Decoder.RightExpressionFor (expr),
                                                     expr, data);
                        case Op.Mult:
                                return VisitMultiply (Decoder.LeftExpressionFor (expr),
                                                      Decoder.RightExpressionFor (expr), expr, data);

                        case Op.SizeOf:
                                return VisitSizeOf (Decoder.LeftExpressionFor (expr), data);
                        case Op.UnaryMinus:
                                return VisitUnaryMinus (Decoder.LeftExpressionFor (expr), expr, data);
                        case Op.LogicalNot:
                                return VisitLogicalNot (Decoder.LeftExpressionFor (expr), expr, data);
                        case Op.Unknown:
                                return VisitUnknown (expr, data);
                        default:
                                throw new ArgumentOutOfRangeException ();
                        }
                }

                public virtual Out VisitVariable (Var var, Expr expr, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitConstant (Expr expr, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitLogicalNot (Expr left, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitUnaryMinus (Expr left, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitNot (Expr expr, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitSizeOf (Expr expr, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitUnknown (Expr expr, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitLessThan (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitLessEqualThan (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitGreaterThan (Expr left, Expr right, Expr original, In data)
                {
                        return DispatchCompare (VisitLessEqualThan, right, left, original, data);
                }

                public virtual Out VisitGreaterEqualThan (Expr left, Expr right, Expr original, In data)
                {
                        return DispatchCompare (VisitLessThan, right, left, original, data);
                }

                public virtual Out VisitNotEqual (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitAddition (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitSubtraction (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitMultiply (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitModulus (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitDivision (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitEqual (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitLogicalOr (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitLogicalAnd (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitXor (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitOr (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                public virtual Out VisitAnd (Expr left, Expr right, Expr original, In data)
                {
                        return Default (data);
                }

                protected virtual bool TryPolarity (Expr expr, In data, out bool shouldNegate)
                {
                        if (Decoder.IsConstant (expr)) {
                                int intValue;
                                if (Decoder.TryValueOf (expr, ExpressionType.Int32, out intValue))
                                        return true.With (intValue == 0, out shouldNegate);

                                bool boolValue;
                                if (Decoder.TryValueOf (expr, ExpressionType.Bool, out boolValue))
                                        return true.With (boolValue, out shouldNegate);
                        }

                        return false.Without (out shouldNegate);
                }

                protected delegate Out CompareVisitor (Expr left, Expr right, Expr original, In data);

                protected virtual Out DispatchCompare (CompareVisitor cmp, Expr left, Expr right, Expr original, In data)
                {
                        if (Decoder.IsConstant (left) && Decoder.OperatorFor (right) == ExpressionOperator.Sub)
                                // const OP (a - b)
                        {
                                int num;
                                if (Decoder.TryValueOf (left, ExpressionType.Int32, out num) && num == 0)
                                        // 0 OP (a-b) ==> b OP a
                                        return cmp (Decoder.RightExpressionFor (right),
                                                    Decoder.LeftExpressionFor (right), right, data);
                        }
                        else if (Decoder.IsConstant (right) && Decoder.OperatorFor (left) == ExpressionOperator.Sub)
                                // (a-b) OP const
                        {
                                int num;
                                if (Decoder.TryValueOf (right, ExpressionType.Int32, out num) && num == 0)
                                        // (a-b) OP 0 ==> a OP b
                                        return cmp (Decoder.LeftExpressionFor (left),
                                                    Decoder.RightExpressionFor (left), left, data);
                        }

                        return cmp (left, right, original, data);
                }

                protected abstract Out Default (In data);

                Out DispatchVisitNot (Expr expr, In data)
                {
                        switch (Decoder.OperatorFor (expr)) {
                        case ExpressionOperator.Equal:
                        case ExpressionOperator.Equal_Obj:
                                return VisitNotEqual (Decoder.LeftExpressionFor (expr),
                                                      Decoder.RightExpressionFor (expr), expr, data);
                        case ExpressionOperator.LessThan: // a < b ==>  b <= a
                                return DispatchCompare (VisitLessEqualThan, Decoder.RightExpressionFor (expr),
                                                        Decoder.LeftExpressionFor (expr), expr, data);
                        case ExpressionOperator.LessEqualThan: // a <= b ==> b < a
                                return DispatchCompare (VisitLessThan, Decoder.LeftExpressionFor (expr),
                                                        Decoder.RightExpressionFor (expr), expr, data);
                        case ExpressionOperator.GreaterThan: // a > b ==> b < a
                                return DispatchCompare (VisitLessThan, Decoder.RightExpressionFor (expr),
                                                        Decoder.LeftExpressionFor (expr), expr, data);
                        case ExpressionOperator.GreaterEqualThan: // a >= b ==> b <= a
                                return DispatchCompare (VisitLessEqualThan, Decoder.RightExpressionFor (expr),
                                                        Decoder.LeftExpressionFor (expr), expr, data);
                        default:
                                return VisitNot (expr, data);
                        }
                }

                Out DispatchVisitEqual (ExpressionOperator eqKind, Expr left, Expr right, Expr original, In data)
                {
                        // { left :eq: right }
                        switch (Decoder.OperatorFor (left)) {
                        case Op.GreaterEqualThan:
                        case Op.GreaterThan:
                        case Op.LessThan:
                        case Op.LessEqualThan:
                        case Op.Equal:
                        case Op.Equal_Obj:
                                // { ( a ?= b ) :eq: right } 
                                bool shouldNegate;
                                if (TryPolarity (right, data, out shouldNegate))
                                        // { (a ?= b) :eq: true => (a ?= b) }; (a ?= b) :eq: false => !(a ?= b) }
                                        return shouldNegate ? DispatchVisitNot (left, data) : Visit (left, data);
                                break;
                        }

                        switch (Decoder.OperatorFor (right)) {
                        case Op.GreaterEqualThan:
                        case Op.GreaterThan:
                        case Op.LessThan:
                        case Op.LessEqualThan:
                        case Op.Equal:
                        case Op.Equal_Obj:
                                // { left :eq: (a ?= b) }
                                bool shouldNegate;
                                if (TryPolarity (left, data, out shouldNegate))
                                        // { true :eq: (a ?= b) => (a ?= b) }; false :eq: (a ?= b) => !(a ?= b) }
                                        return shouldNegate ? DispatchVisitNot (right, data) : Visit (right, data);
                                break;
                        }

                        return VisitEqual (left, right, original, data);
                }
        }
}