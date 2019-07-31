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
using System.Diagnostics;
using System.Linq.Expressions;

using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// A table expression produced by a sub select, which work almost like any other table
    /// Different joins specify different tables
    /// </summary>
    [DebuggerDisplay("SubSelectExpression {Name} (as {Alias})")]
#if !MONO_STRICT
    public
#endif
    class SubSelectExpression : TableExpression
    {
        public SelectExpression Select { get; private set; }

        public SubSelectExpression(SelectExpression select, Type type, string alias)
            : base(type, alias)
        {
            this.Select = select;
            this.Alias = alias;
        }

        public override bool IsEqualTo(TableExpression expression)
        {
            SubSelectExpression subSelectTable = expression as SubSelectExpression;
            if (subSelectTable == null)
                return false;
            return Name == expression.Name && JoinID == expression.JoinID && Select == subSelectTable.Select;
        }
    }
}
