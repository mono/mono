﻿#region MIT license
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

using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DbLinq.Data.Linq.Sugar
{
    [DebuggerDisplay("ExpressionChain {Expressions.Count} Expression(s)")]
    internal class ExpressionChain : IEnumerable<Expression>
    {
        private Expression[] expressions;
        private List<Expression> expressionList;
        public List<Expression> Expressions 
        {
            get
            {
                if (expressionList == null)
                    if (expressions == null)
                        expressionList = new List<Expression>();
                    else
                        expressionList = new List<Expression>(expressions);
                return expressionList;
            }
        }

        public ExpressionChain()
        {
            expressions = new Expression[] { };
        }

        public ExpressionChain(ExpressionChain chain)
        {
            this.expressions = new Expression[chain.expressions.Length];
            chain.expressions.CopyTo(this.expressions, 0);
        }

        public ExpressionChain(ExpressionChain chain, Expression expression)
        {
            this.expressions = new Expression[chain.expressions.Length + 1];
            chain.expressions.CopyTo(this.expressions, 0);
            this.expressions[chain.expressions.Length] = expression;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExpressionChain;
            if (other == null)
                return false;
            if (Expressions.Count != other.Expressions.Count)
                return false;
            for (int expressionIndex = 0; expressionIndex < Expressions.Count; expressionIndex++)
            {
                if (!Expressions[expressionIndex].Equals(other.Expressions[expressionIndex]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var expression in Expressions)
            {
                hash <<= 10;
                hash ^= (int)expression.NodeType;
                hash ^= expression.Type.GetHashCode();
            }
            return hash;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Expression>)this).GetEnumerator();
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            return Expressions.GetEnumerator();
        }
    }
}