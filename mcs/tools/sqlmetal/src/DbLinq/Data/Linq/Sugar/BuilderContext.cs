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
using System.Reflection;

using DbLinq.Data.Linq.Sugar;
using DbLinq.Data.Linq.Sugar.Expressions;

namespace DbLinq.Data.Linq.Sugar
{
    internal class BuilderContext
    {
        public Stack<MethodInfo> CallStack { get; private set; }

        // Global context
        public QueryContext QueryContext { get; private set; }

        // Current expression being built
        public ExpressionQuery ExpressionQuery { get; private set; }

        // Build context: values here are related to current context, and can change with it
        private int currentScopeIndex;
        public SelectExpression CurrentSelect
        {
            get { return SelectExpressions[currentScopeIndex]; }
            set { SelectExpressions[currentScopeIndex] = value; }
        }
        public IList<SelectExpression> SelectExpressions { get; private set; }
        public IDictionary<Type, MetaTableExpression> MetaTables { get; private set; }
        public IDictionary<string, Expression> Parameters { get; private set; }

        public bool ExpectMetaTableDefinition { get; set; }

        /// <summary>
        /// Helper to enumerate all registered tables
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TableExpression> EnumerateAllTables()
        {
            foreach (var scopePiece in SelectExpressions)
            {
                foreach (var table in scopePiece.Tables)
                    yield return table;
            }
        }

        /// <summary>
        /// Helper to enumerate all registered columns
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TableExpression> EnumerateScopeTables()
        {
            for (SelectExpression currentSelect = CurrentSelect; currentSelect != null; currentSelect = currentSelect.Parent)
            {
                foreach (var table in currentSelect.Tables)
                    yield return table;
            }
        }

        /// <summary>
        /// Helper to enumerate all registered columns
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnExpression> EnumerateScopeColumns()
        {
            for (SelectExpression currentSelect = CurrentSelect; currentSelect != null; currentSelect = currentSelect.Parent)
            {
                foreach (var column in currentSelect.Columns)
                    yield return column;
            }
        }

        public BuilderContext(QueryContext queryContext)
        {
            CallStack = new Stack<MethodInfo>();
            SelectExpressions = new List<SelectExpression>();
            currentScopeIndex = SelectExpressions.Count;
            SelectExpressions.Add(new SelectExpression());
            QueryContext = queryContext;
            ExpressionQuery = new ExpressionQuery();
            MetaTables = new Dictionary<Type, MetaTableExpression>();
            Parameters = new Dictionary<string, Expression>();
        }

        private BuilderContext()
        { }

        /// <summary>
        /// Creates a new BuilderContext where parameters have a local scope
        /// </summary>
        /// <returns></returns>
        public BuilderContext NewQuote()
        {
            var builderContext = new BuilderContext();

            // scope independent Parts
            builderContext.CallStack = CallStack;
            builderContext.QueryContext = QueryContext;
            builderContext.ExpressionQuery = ExpressionQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.currentScopeIndex = currentScopeIndex;
            builderContext.SelectExpressions = SelectExpressions;
            builderContext.ExpectMetaTableDefinition = ExpectMetaTableDefinition;

            // scope dependent Parts
            builderContext.Parameters = new Dictionary<string, Expression>(Parameters);

            return builderContext;
        }

        /// <summary>
        /// Creates a new BuilderContext with a new query scope
        /// </summary>
        /// <returns></returns>
        public BuilderContext NewSelect()
        {
            var builderContext = new BuilderContext();

            // we basically copy everything
            builderContext.CallStack = CallStack;
            builderContext.QueryContext = QueryContext;
            builderContext.ExpressionQuery = ExpressionQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.Parameters = Parameters;
            builderContext.SelectExpressions = SelectExpressions;
            builderContext.ExpectMetaTableDefinition = ExpectMetaTableDefinition;

            // except CurrentScope, of course
            builderContext.currentScopeIndex = SelectExpressions.Count;
            SelectExpressions.Add(new SelectExpression(CurrentSelect));

            return builderContext;
        }

        /// <summary>
        /// Creates a new BuilderContext with a new query scope with the same parent of the CurrentSelect
        /// </summary>
        /// <returns></returns>
        public BuilderContext NewSisterSelect()
        {
            var builderContext = new BuilderContext();

            // we basically copy everything
            builderContext.CallStack = CallStack;
            builderContext.QueryContext = QueryContext;
            builderContext.ExpressionQuery = ExpressionQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.Parameters = Parameters;
            builderContext.SelectExpressions = SelectExpressions;
            builderContext.ExpectMetaTableDefinition = ExpectMetaTableDefinition;

            // except CurrentScope, of course
            builderContext.currentScopeIndex = SelectExpressions.Count;
            SelectExpressions.Add(new SelectExpression(CurrentSelect.Parent));

            return builderContext;
        }

        /// <summary>
        /// Creates a new BuilderContext with a new query scope which is parent of the current one
        /// </summary>
        /// <returns></returns>
        public void NewParentSelect()
        {
            SelectExpression currentSelect = this.CurrentSelect;
            SelectExpression newParentSelect = new SelectExpression(currentSelect.Parent);

            while (currentSelect != null)
            {
                currentSelect.Parent = newParentSelect;
                currentSelect = currentSelect.NextSelectExpression;
            }
            this.currentScopeIndex = SelectExpressions.Count;
            SelectExpressions.Add(newParentSelect);
        }

        public BuilderContext Clone()
        {
            var builderContext = new BuilderContext();

            builderContext.CallStack = CallStack;
            builderContext.QueryContext = QueryContext;
            builderContext.ExpressionQuery = ExpressionQuery;
            builderContext.MetaTables = MetaTables;
            builderContext.Parameters = Parameters;
            builderContext.SelectExpressions = SelectExpressions;
            builderContext.currentScopeIndex = currentScopeIndex;
            builderContext.ExpectMetaTableDefinition = ExpectMetaTableDefinition;

            return builderContext;
        }

        public bool IsExternalInExpressionChain { get; set; }
    }
}
