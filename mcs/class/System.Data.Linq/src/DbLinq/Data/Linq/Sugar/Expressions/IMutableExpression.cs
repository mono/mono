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

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// Allows an Expression to enumerator its Operands and be mutated, ie changing its operands
    /// Depending on the Expression type (such as System.Linq.Expressions), a new copy may be returned
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface IMutableExpression
    {
        /// <summary>
        /// Represents Expression operands, ie anything that is an expression
        /// </summary>
        IEnumerable<Expression> Operands { get; }
        /// <summary>
        /// Replaces operands and returns a corresponding Expression
        /// </summary>
        /// <param name="operands"></param>
        /// <returns></returns>
        Expression Mutate(IList<Expression> operands);
    }
}