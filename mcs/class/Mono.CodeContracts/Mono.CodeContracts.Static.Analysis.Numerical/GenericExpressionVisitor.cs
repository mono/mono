#region Copyright Header
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
#endregion

using System;

using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Decoding;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class GenericExpressionVisitor<In, Out, Var, Expr>
    {
        protected IExpressionDecoder<Var, Expr> Decoder { get; private set; }

        protected GenericExpressionVisitor(IExpressionDecoder<Var, Expr> decoder)
        {
            this.Decoder = decoder;
        }

        protected abstract Out Default (In data);

        public virtual Out Visit(Expr expr, In data)
        {
            ExpressionOperator op = Decoder.OperatorFor (expr);
            switch (op)
            {
                case ExpressionOperator.Constant:
                    return this.VisitConstant (expr, data);
                case ExpressionOperator.Variable:
                    return this.VisitVariable (Decoder.UnderlyingVariable (expr), expr, data);
                case ExpressionOperator.Not:
                    return this.DispatchVisitNot(Decoder.LeftExpressionFor(expr), data);
                default:
                    throw new ArgumentOutOfRangeException ();
            }
        }

        private Out DispatchVisitNot(Expr expr, In data)
        {
            switch (Decoder.OperatorFor(expr))
            {
                case ExpressionOperator.Equal:
                case ExpressionOperator.Equal_Obj:
                    return VisitNotEqual(Decoder.LeftExpressionFor(expr), Decoder.RightExpressionFor(expr), expr, data);
                case ExpressionOperator.LessThan: // a < b ==>  b <= a
                    return DispatchCompare(VisitLessEqualThan, Decoder.RightExpressionFor(expr), Decoder.LeftExpressionFor(expr), expr, data);
                case ExpressionOperator.LessEqualThan: // a <= b ==> b < a
                    return DispatchCompare(VisitLessThan, Decoder.LeftExpressionFor(expr), Decoder.RightExpressionFor(expr), expr, data);
                case ExpressionOperator.GreaterThan: // a > b ==> b < a
                    return DispatchCompare(VisitLessThan, Decoder.RightExpressionFor(expr), Decoder.LeftExpressionFor(expr), expr, data);
                case ExpressionOperator.GreaterEqualThan: // a >= b ==> b <= a
                    return DispatchCompare(VisitLessEqualThan, Decoder.RightExpressionFor(expr), Decoder.LeftExpressionFor(expr), expr, data);
                default:
                    return VisitNot(expr, data);
            }
        }

        protected virtual Out DispatchCompare(CompareVisitor cmp, Expr left, Expr right, Expr original, In data)
        {
            if (Decoder.IsConstant(left) && Decoder.OperatorFor(right) == ExpressionOperator.Sub) // const OP (a - b)
            {
                int num;
                if (this.Decoder.TryValueOf(left, ExpressionType.Int32, out num) && num == 0) // 0 OP (a-b) ==> b OP a
                    return cmp(Decoder.RightExpressionFor(right), Decoder.LeftExpressionFor(right), right, data);
            }
            else if (Decoder.IsConstant(right) && Decoder.OperatorFor(left) == ExpressionOperator.Sub) // (a-b) OP const
            {
                int num;
                if (Decoder.TryValueOf(right, ExpressionType.Int32, out num) && num == 0) // (a-b) OP 0 ==> a OP b
                    return cmp(Decoder.LeftExpressionFor(left), Decoder.RightExpressionFor(left), left, data);
            }

            return cmp(left, right, original, data);
        }
        protected delegate Out CompareVisitor (Expr left, Expr right, Expr original, In data);

        public virtual Out VisitNotEqual (Expr left, Expr right, Expr original, In data)
        {
            return this.Default (data);
        }
        public virtual Out VisitVariable (Var var, Expr expr, In data)
        {
            return this.Default (data);
        }
        public virtual Out VisitConstant (Expr expr, In data)
        {
            return this.Default (data);
        }
        public virtual Out VisitNot(Expr expr, In data)
        {
            return Default(data);
        }
        public virtual Out VisitLessThan(Expr left, Expr right, Expr original, In data)
        {
            return Default(data);
        }
        public virtual Out VisitLessEqualThan(Expr left, Expr right, Expr original, In data)
        {
            return Default(data);
        }
        public virtual Out VisitGreaterThan(Expr left, Expr right, Expr original, In data)
        {
            return Default(data);
        }
        public virtual Out VisitGreaterEqualThan(Expr left, Expr right, Expr original, In data)
        {
            return Default(data);
        }
    }

    internal interface IExpressionDecoder<Var, Expr>
    {
        ExpressionOperator OperatorFor(Expr expr);

        Var UnderlyingVariable(Expr expr);
        Expr LeftExpressionFor(Expr expr);
        Expr RightExpressionFor(Expr expr);

        bool IsConstant (Expr expr);
        bool TryValueOf<T> (Expr left, ExpressionType int32, out T num);
    }

    enum ExpressionOperator
    {
        Constant,
        Variable,
        Not,
        And,
        Or,
        Xor,
        LogicalAnd,
        LogicalOr,
        LogicalNot,
        Equal,
        Equal_Obj,
        NotEqual,
        LessThan,
        LessEqualThan,
        GreaterThan,
        GreaterEqualThan,
        Add,
        Sub,
        Mult,
        Div,
        Mod,
        UnaryMinus,
        SizeOf,
        Unknown
    }

    enum ExpressionType
    {
        Unknown,
        Int32,

        Bool
    }
}