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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DbLinq.Linq.Data.Sugar.Expressions
{
    /// <summary>
    /// Represents a GROUP BY
    /// </summary>
    [DebuggerDisplay("GroupByExpression {Name} (as {Alias})")]
    public class GroupByExpression : TableExpression
    {
        public new const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.GroupBy;

        public ColumnExpression SimpleGroup { get; private set; }
        public IDictionary<MemberInfo, ColumnExpression> MultipleGroup { get; private set; }
        public Expression KeyExpression { get; private set; }
        public TableExpression Table { get; set; }

        protected bool HasKey { get; private set; }

        public GroupByExpression(ColumnExpression simpleGroup, Expression keyExpression)
            : base(ExpressionType, simpleGroup.Table)
        {
            SimpleGroup = simpleGroup;
            HasKey = false;
            KeyExpression = keyExpression;
            Table = SimpleGroup.Table;
        }

        public GroupByExpression(IDictionary<MemberInfo, ColumnExpression> multipleGroup, Expression keyExpression)
            : base(ExpressionType, multipleGroup.Values.First().Table)
        {
            MultipleGroup = new Dictionary<MemberInfo, ColumnExpression>(multipleGroup);
            HasKey = true;
            KeyExpression = keyExpression;
            Table = MultipleGroup.Values.First().Table;
        }

        private GroupByExpression(TableExpression tableExpression)
            : base(ExpressionType, tableExpression)
        {
            Table = tableExpression;
        }

        /// <summary>
        /// Returns the request member.
        /// For a MultipleGroup case, we return a modified copy of the current expression
        ///                             this copy we be called again with the final MemberInfo
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public Expression GetMember(MemberInfo memberInfo)
        {
            // simple groupe case here, we accept only one request: the "Key"
            if (SimpleGroup != null)
            {
                if (IsKeyRequest(memberInfo))
                {
                    return SimpleGroup;
                }
                throw Error.BadArgument("S0077: Unknown member '{0}' for simple GroupByExpression", memberInfo.Name);
            }
            // multiple group, we accept only Key at first time, then try any request
            if (HasKey)
            {
                if (IsKeyRequest(memberInfo))
                {
                    return GetKey();
                }
                throw Error.BadArgument("S0087: Only 'Key' member can be requested here", memberInfo.Name);
            }
            ColumnExpression member;
            if (!MultipleGroup.TryGetValue(memberInfo, out member))
                throw Error.BadArgument("S0091: Unknown member '{0}' for multiple GroupByExpression", memberInfo.Name);
            return member;
        }

        /// <summary>
        /// The only member a IGrouping has is the "Key", and we're accepting only this one at the beginning
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool IsKeyRequest(MemberInfo memberInfo)
        {
            return memberInfo.Name == "Key";
        }

        public GroupByExpression GetKey()
        {
            var newGroupBy = new GroupByExpression(Table)
                                 {
                                     SimpleGroup = SimpleGroup,
                                     MultipleGroup = MultipleGroup,
                                     HasKey = false,
                                     Table = Table,
                                     KeyExpression = KeyExpression,
                                 };
            return newGroupBy;
        }

        public IEnumerable<ColumnExpression> Columns
        {
            get
            {
                if (SimpleGroup != null)
                    yield return SimpleGroup;
                else
                {
                    foreach (var column in MultipleGroup.Values)
                        yield return column;
                }
            }
        }

        #region Expression mutation

        public override Expression Mutate(IList<Expression> newOperands)
        {
            return this;
        }

        public override IEnumerable<Expression> Operands
        {
            get
            {
                yield break;
            }
        }

        #endregion
    }
}
