#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
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

namespace DbLinq.Linq.Data.Sugar.Expressions
{
    /// <summary>
    /// ScopeExpression describes a selection.
    /// It can be present at top-level or as subexpressions
    /// </summary>
    public class ScopeExpression : OperandsMutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.Scope;

        // Involved entities
        public IList<TableExpression> Tables { get; private set; }
        public IList<ColumnExpression> Columns { get; private set; }

        // Clauses
        public string ExecuteMethodName { get; set; } // for Execute<> calls, this member is filled with the method name
        public LambdaExpression SelectExpression { get; set; } // Func<IDataRecord,T> --> creates an object from data record
        public IList<Expression> Where { get; private set; }
        public IList<OrderByExpression> OrderBy { get; private set; }
        public IList<GroupExpression> Group { get; private set; }

        public Expression Offset { get; set; }
        public Expression Limit { get; set; }
        public Expression OffsetAndLimit { get; set; }

        // Parent scope: we will climb up to find if we don't find the request table in the current scope
        public ScopeExpression Parent { get; private set; }

        public ScopeExpression()
            : base(ExpressionType, null, null)
        {
            Tables = new List<TableExpression>();
            Columns = new List<ColumnExpression>();
            // Local clauses
            Where = new List<Expression>();
            OrderBy = new List<OrderByExpression>();
            Group = new List<GroupExpression>();
        }

        public ScopeExpression(ScopeExpression parentScopePiece)
            : base(ExpressionType, null, null)
        {
            Parent = parentScopePiece;
            // Tables and columns are empty, since the table/column lookup recurses to parentScopePiece
            Tables = new List<TableExpression>();
            Columns = new List<ColumnExpression>();
            // Local clauses
            Where = new List<Expression>();
            OrderBy = new List<OrderByExpression>();
            Group = new List<GroupExpression>();
        }

        private ScopeExpression(Type type, IList<Expression> operands)
            : base(ExpressionType, type, operands)
        {
        }

        protected override Expression Mutate2(IList<Expression> newOperands)
        {
            Type type;
            if (newOperands.Count > 0)
                type = newOperands[0].Type;
            else
                type = Type;
            var scopeExpression = new ScopeExpression(type, newOperands);
            scopeExpression.Tables = Tables;
            scopeExpression.Columns = Columns;
            scopeExpression.Where = Where;
            scopeExpression.OrderBy = OrderBy;
            scopeExpression.Group = Group;
            scopeExpression.Parent = Parent;
            scopeExpression.ExecuteMethodName = ExecuteMethodName;
            scopeExpression.Limit = Limit;
            scopeExpression.Offset = Offset;
            scopeExpression.OffsetAndLimit = OffsetAndLimit;
            return scopeExpression;
        }
    }
}
