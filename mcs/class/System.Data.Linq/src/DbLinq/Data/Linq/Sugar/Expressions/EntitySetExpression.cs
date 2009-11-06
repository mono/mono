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
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using System.Reflection;

using DbLinq.Util;
using DbLinq.Data.Linq.Sugar.ExpressionMutator;
using DbLinq.Data.Linq.Sugar.Implementation;

namespace DbLinq.Data.Linq.Sugar.Expressions
{
    /// <summary>
    /// A GroupExpression holds a grouped result
    /// It is usually transparent, except for return value, where it mutates the type to IGrouping
    /// </summary>
    [DebuggerDisplay("EntitySetExpression: {TableExpression.Type}")]
#if !MONO_STRICT
    public
#endif
    class EntitySetExpression : MutableExpression
    {
        public const ExpressionType ExpressionType = (ExpressionType)CustomExpressionType.EntitySet;

        TableExpression tableExpression;

        public TableExpression TableExpression {
            get {
                if (tableExpression != null)
                    return tableExpression;
                var entityType = EntitySetType.GetGenericArguments()[0];
                tableExpression = dispatcher.RegisterAssociation(sourceTable, memberInfo, entityType, builderContext);
                return tableExpression;
            }
            set {
                tableExpression = value;
            }
        }

        BuilderContext builderContext;
        public Type EntitySetType;
        ExpressionDispatcher dispatcher;
        TableExpression sourceTable;
        MemberInfo memberInfo;

        public List<KeyValuePair<ColumnExpression, MetaDataMember>> Columns = new List<KeyValuePair<ColumnExpression, MetaDataMember>>();

        internal EntitySetExpression(TableExpression sourceTable, MemberInfo memberInfo, Type entitySetType, BuilderContext builderContext, ExpressionDispatcher dispatcher)
            : base(ExpressionType, entitySetType)
        {
            this.builderContext = builderContext;
            this.EntitySetType = entitySetType;
            this.dispatcher = dispatcher;
            this.sourceTable = sourceTable;
            this.memberInfo = memberInfo;
            ParseExpression(sourceTable);
        }

        private void ParseExpression(TableExpression sourceTable)
        {
            // var sourceTable = targetTable.JoinedTable;
            var entityType = EntitySetType.GetGenericArguments()[0];

            // BUG: This is ignoring External Mappings from XmlMappingSource.
            var mappingType = builderContext.QueryContext.DataContext.Mapping.GetMetaType(entityType);
            var foreignKeys = mappingType.Associations.Where(a => a.IsForeignKey && a.OtherType.Type == sourceTable.Type);

            foreach (var fk in foreignKeys)
            {
                var oke = fk.OtherKey.GetEnumerator();
                var tke = fk.ThisKey.GetEnumerator();
                bool ho, ht;
                while ((ho = oke.MoveNext()) && (ht = tke.MoveNext()))
                {
                    var ok = oke.Current;
                    var tk = tke.Current;
                    var column = dispatcher.RegisterColumn(sourceTable, ok.Member, builderContext);
                    Columns.Add(new KeyValuePair<ColumnExpression, MetaDataMember>(column, tk));
                }
            }
        }

        public override Expression Mutate(IList<Expression> newOperands)
        {
            if (newOperands.Count != 1)
                throw Error.BadArgument("S0063: Bad argument count");
            TableExpression = (TableExpression)newOperands[0];
            // ParseExpression(TableExpression);
            return this;
        }
    }
}
