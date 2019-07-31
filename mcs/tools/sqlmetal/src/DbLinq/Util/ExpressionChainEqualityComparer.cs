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

using System.Collections.Generic;
using System.Linq.Expressions;

using DbLinq.Data.Linq.Sugar;


namespace DbLinq.Util
{
    /// <summary>
    /// Provides IEqualityComparer for ExpressionChain class
    /// </summary>
    internal class ExpressionChainEqualityComparer : IEqualityComparer<ExpressionChain>
    {
        private readonly IEqualityComparer<Expression> expressionComparer = new ExpressionEqualityComparer();

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(ExpressionChain x, ExpressionChain y)
        {
            // if not same count, expression chains are different
            if (x.Expressions.Count != y.Expressions.Count)
                return false;

            for (int index = 0; index < x.Expressions.Count; index++)
            {
                if (!expressionComparer.Equals(x.Expressions[index], y.Expressions[index]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(ExpressionChain obj)
        {
            int hash = 0;
            foreach (var expression in obj.Expressions)
            {
                hash <<= 3;
                hash ^= expressionComparer.GetHashCode(expression);
            }
            return hash;
        }
    }
}
