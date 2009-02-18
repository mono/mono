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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if MONO_STRICT
using System.Data.Linq.Sugar;
#else
using DbLinq.Data.Linq.Sugar;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Implementation
#else
namespace DbLinq.Data.Linq.Implementation
#endif
{
    /// <summary>
    /// QueryProvider is used by both DataContext and Table
    /// to build queries
    /// It is split is two parts (non-generic and generic) for copy reasons
    /// </summary>
    internal abstract class QueryProvider
    {
        /// <summary>
        /// Gets or sets the expression chain.
        /// </summary>
        /// <value>The expression chain.</value>
        public ExpressionChain ExpressionChain { get; set; }
        /// <summary>
        /// Gets or sets the type of the table.
        /// </summary>
        /// <value>The type of the table.</value>
        public Type TableType { get; set; }
        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public abstract SelectQuery GetQuery(Expression expression);
    }

    /// <summary>
    /// QueryProvider, generic version
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QueryProvider<T> : QueryProvider, IQueryProvider, IQueryable<T>, IOrderedQueryable<T>
    {
        /// <summary>
        /// Holder current datancontext
        /// </summary>
        protected readonly DataContext _dataContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProvider&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public QueryProvider(DataContext dataContext)
        {
            _dataContext = dataContext;
            TableType = typeof(T);
            ExpressionChain = new ExpressionChain();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProvider&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="tableType">Type of the table.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="expressionChain">The expression chain.</param>
        /// <param name="expression">The expression.</param>
        public QueryProvider(Type tableType, DataContext dataContext, ExpressionChain expressionChain, Expression expression)
        {
            _dataContext = dataContext;
            TableType = tableType;
            ExpressionChain = new ExpressionChain(expressionChain, expression);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="t">The t.</param>
        /// <param name="tableType">Type of the table.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="expressionChain">The expression chain.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected S CreateQuery<S>(Type t, Type tableType, DataContext dataContext, ExpressionChain expressionChain, Expression expression)
        {
            // no way to work differently
            var typedQueryProviderType = typeof(QueryProvider<>).MakeGenericType(t);
            var queryProvider = (S)Activator.CreateInstance(typedQueryProviderType, tableType, dataContext,
                                                             expressionChain, expression);
            return queryProvider;
        }

        /// <summary>
        /// Builds the query, given a LINQ expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            var type = expression.Type;
            if (!type.IsGenericType)
                throw Error.BadArgument("S0066: Don't know how to handle non-generic type '{0}'", type);
            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(IQueryable<>) || genericType == typeof(IOrderedQueryable<>))
                type = type.GetGenericArguments()[0];
            else
                Error.BadArgument("S0068: Don't know how to handle type '{0}'", type);
            return CreateQuery<IQueryable>(type, TableType, _dataContext, ExpressionChain, expression);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new QueryProvider<TElement>(TableType, _dataContext, ExpressionChain, expression);
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public override SelectQuery GetQuery(Expression expression)
        {
            var expressionChain = ExpressionChain;
            if (expression != null)
                expressionChain = new ExpressionChain(expressionChain, expression);
            return _dataContext.QueryBuilder.GetSelectQuery(expressionChain, new QueryContext(_dataContext));
        }

        /// <summary>
        /// Runs query
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }

        /// <summary>
        /// Runs query
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            var query = GetQuery(expression);
            return _dataContext.QueryRunner.SelectScalar<TResult>(query);
        }

        /// <summary>
        /// Enumerates all query items
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var enumerator = GetEnumerator();
            return enumerator;
        }

        /// <summary>
        /// Enumerates all query items
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            var query = GetQuery(null);
            return _dataContext.QueryRunner.Select<T>(query).GetEnumerator();
        }

        /// <summary>
        /// Returns this QueryProvider as an exception
        /// </summary>
        public Expression Expression
        {
            get { return Expression.Constant(this); }
        }

        public Type ElementType
        {
            get { return (typeof(T)); }
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }
    }
}
