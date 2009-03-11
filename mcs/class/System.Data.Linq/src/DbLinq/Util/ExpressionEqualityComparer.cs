#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Implementation;
#else
using DbLinq.Data.Linq.Implementation;
#endif

namespace DbLinq.Util
{
    /// <summary>
    /// IEqualityComparer implementation for Expression
    /// </summary>
    internal class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(Expression x, Expression y)
        {
            // both nulls? comparison is OK
            if (x == null && y == null)
                return true;

            if (x == null && y != null)
                return false;

            if (x != null && y == null)
                return false;

            // first thing: check types
            if (x.GetType() != y.GetType())
                return false;

            // then check node types
            if (x.NodeType != y.NodeType)
                return false;

            // finally, check expression types
            if (x.Type != y.Type)
                return false;

            // then for each expression subtype, check members
            if (x is BinaryExpression)
                return Equals2((BinaryExpression)x, (BinaryExpression)y);
            if (x is ConditionalExpression)
                return Equals2((ConditionalExpression)x, (ConditionalExpression)y);
            if (x is ConstantExpression)
                return Equals2((ConstantExpression)x, (ConstantExpression)y);
            if (x is InvocationExpression)
                return Equals2((InvocationExpression)x, (InvocationExpression)y);
            if (x is LambdaExpression)
                return Equals2((LambdaExpression)x, (LambdaExpression)y);
            if (x is ListInitExpression)
                return Equals2((ListInitExpression)x, (ListInitExpression)y);
            if (x is MemberExpression)
                return Equals2((MemberExpression)x, (MemberExpression)y);
            if (x is MemberInitExpression)
                return Equals2((MemberInitExpression)x, (MemberInitExpression)y);
            if (x is MethodCallExpression)
                return Equals2((MethodCallExpression)x, (MethodCallExpression)y);
            if (x is NewArrayExpression)
                return Equals2((NewArrayExpression)x, (NewArrayExpression)y);
            if (x is NewExpression)
                return Equals2((NewExpression)x, (NewExpression)y);
            if (x is ParameterExpression)
                return Equals2((ParameterExpression)x, (ParameterExpression)y);
            if (x is TypeBinaryExpression)
                return Equals2((TypeBinaryExpression)x, (TypeBinaryExpression)y);
            if (x is UnaryExpression)
                return Equals2((UnaryExpression)x, (UnaryExpression)y);
            throw new ArgumentException(string.Format("Unknown Expression type ({0})", x.GetType()));
        }

        private bool Equals2(BinaryExpression x, BinaryExpression y)
        {
            if (x.IsLifted != y.IsLifted)
                return false;

            if (x.IsLiftedToNull != y.IsLiftedToNull)
                return false;

            if (x.Method != y.Method)
                return false;

            if (!Equals(x.Conversion, y.Conversion))
                return false;

            if (!Equals(x.Left, y.Left))
                return false;

            if (!Equals(x.Right, y.Right))
                return false;

            return true;
        }

        private bool Equals2(ConditionalExpression x, ConditionalExpression y)
        {
            if (!Equals(x.Test, y.Test))
                return false;

            if (!Equals(x.IfTrue, y.IfTrue))
                return false;

            if (!Equals(x.IfFalse, y.IfFalse))
                return false;

            return true;
        }

        /// <summary>
        /// Objects comparer, with special hints
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool ObjectsEquals(object x, object y)
        {
            if (x == null && y == null)
                return true;

            if (x != null && y == null)
                return false;
            if (x == null && y != null)
                return false;

            if (x.GetType() != y.GetType())
                return false;

            if (x is Expression)
                return Equals((Expression)x, (Expression)y);

            if (x is QueryProvider)
                return new ExpressionChainEqualityComparer().Equals(((QueryProvider)x).ExpressionChain, ((QueryProvider)y).ExpressionChain);

            return Equals(x, y);
        }

        private bool Equals2(ConstantExpression x, ConstantExpression y)
        {
            return ObjectsEquals(x.Value, y.Value);
        }

        /// <summary>
        /// Determines if the two lists contain indentical data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xs">The xs.</param>
        /// <param name="ys">The ys.</param>
        /// <param name="equals2">The equals2.</param>
        /// <returns></returns>
        private bool Equals2<T>(IList<T> xs, IList<T> ys, Func<T, T, bool> equals2)
        {
            if (xs == null || ys == null)
                return false;
            if (xs.Count != ys.Count)
                return false;

            for (int index = 0; index < xs.Count; index++)
            {
                if (!equals2(xs[index], ys[index]))
                    return false;
            }
            return true;
        }

        private bool Equals2(InvocationExpression x, InvocationExpression y)
        {
            if (!Equals(x.Expression, y.Expression))
                return false;

            if (!Equals2(x.Arguments, y.Arguments))
                return false;

            return true;
        }

        private bool Equals2(LambdaExpression x, LambdaExpression y)
        {
            if (!Equals(x.Body, y.Body))
                return false;

            if (!Equals2(x.Parameters, y.Parameters, Equals2))
                return false;

            return true;
        }

        private bool Equals2(ListInitExpression x, ListInitExpression y)
        {
            if (!Equals(x.NewExpression, y.NewExpression))
                return false;

            if (!Equals(x.Initializers, y.Initializers))
                return false;

            return true;
        }

        private bool Equals2(ElementInit x, ElementInit y)
        {
            if (x.AddMethod != y.AddMethod)
                return false;

            if (!Equals2(x.Arguments, y.Arguments))
                return false;

            return true;
        }

        private bool Equals2(MemberExpression x, MemberExpression y)
        {
            if (x.Member != y.Member)
                return false;

            if (!Equals(x.Expression, y.Expression))
                return false;

            return true;
        }

        private bool Equals2(MemberInitExpression x, MemberInitExpression y)
        {
            if (!Equals(x.NewExpression, y.NewExpression))
                return false;

            if (!Equals2(x.Bindings, y.Bindings))
                return false;

            return true;
        }

        private bool Equals2(IList<MemberBinding> xs, IList<MemberBinding> ys)
        {
            return Equals2(xs, ys, Equals2);
        }

        private bool Equals2(MemberBinding x, MemberBinding y)
        {
            if (x.BindingType != y.BindingType)
                return false;

            if (x.Member != y.Member)
                return false;

            if (x.GetType() != y.GetType())
                return false;

            if (x is MemberAssignment)
                return Equals2((MemberAssignment)x, (MemberAssignment)y);
            if (x is MemberListBinding)
                return Equals2((MemberListBinding)x, (MemberListBinding)y);
            if (x is MemberMemberBinding)
                return Equals2((MemberMemberBinding)x, (MemberMemberBinding)y);

            throw new ArgumentException(string.Format("Equals2(): unsupported MemberBinding subtype ({0})", x.GetType()));
        }

        private bool Equals2(MemberAssignment x, MemberAssignment y)
        {
            return Equals(x.Expression, y.Expression);
        }

        private bool Equals2(MemberListBinding x, MemberListBinding y)
        {
            if (!Equals2(x.Initializers, y.Initializers, Equals2))
                return false;

            return true;
        }

        private bool Equals2(MemberMemberBinding x, MemberMemberBinding y)
        {
            if (!Equals2(x.Bindings, y.Bindings, Equals2))
                return false;

            return true;
        }

        private bool Equals2(MethodCallExpression x, MethodCallExpression y)
        {
            if (x.Method != y.Method)
                return false;

            if (!Equals(x.Object, y.Object))
                return false;

            if (!Equals2(x.Arguments, y.Arguments))
                return false;

            return true;
        }

        private bool Equals2(NewArrayExpression x, NewArrayExpression y)
        {
            if (!Equals2(x.Expressions, y.Expressions))
                return false;

            return true;
        }

        private bool Equals2(IList<Expression> xs, IList<Expression> ys)
        {
            return Equals2(xs, ys, Equals);
        }

        private bool Equals2(NewExpression x, NewExpression y)
        {
            if (x.Constructor != y.Constructor)
                return false;

            if (!Equals2(x.Arguments, y.Arguments))
                return false;

            if (!Equals2(x.Members, y.Members, (mx, my) => mx == my))
                return false;

            return true;
        }

        private bool Equals2(ParameterExpression x, ParameterExpression y)
        {
            if (x.Name != y.Name)
                return false;

            return true;
        }

        private bool Equals2(TypeBinaryExpression x, TypeBinaryExpression y)
        {
            if (x.TypeOperand != y.TypeOperand)
                return false;

            if (!Equals(x.Expression, y.Expression))
                return false;

            return true;
        }

        private bool Equals2(UnaryExpression x, UnaryExpression y)
        {
            if (x.IsLifted != y.IsLifted)
                return false;

            if (x.IsLiftedToNull != y.IsLiftedToNull)
                return false;

            if (x.Method != y.Method)
                return false;

            if (!Equals(x.Operand, y.Operand))
                return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
        /// </exception>
        public int GetHashCode(Expression obj)
        {
            return (int)obj.NodeType ^ obj.GetType().GetHashCode();
        }
    }
}
