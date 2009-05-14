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
using System.Diagnostics;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar.ExpressionMutator;
#else
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
    /// <summary>
    /// A GroupExpression holds a grouped result
    /// It is usually transparent, except for return value, where it mutates the type to IGrouping
    /// </summary>
    [DebuggerDisplay("EntityExpression: {TableExpression.Type}")]
#if !MONO_STRICT
    public
#endif
    class EntitySetExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.EntitySet;

        public TableExpression TableExpression { get; set; }

        public EntitySetExpression(TableExpression tableExpression, Type entitySetType)
            : base(ExpressionType, entitySetType)
        {
            TableExpression = tableExpression;
        }

        public override Expression Mutate(IList<Expression> newOperands)
        {
            if (newOperands.Count != 1)
                throw Error.BadArgument("S0063: Bad argument count");
            TableExpression = (TableExpression)newOperands[0];
            return this;
        }
    }
}