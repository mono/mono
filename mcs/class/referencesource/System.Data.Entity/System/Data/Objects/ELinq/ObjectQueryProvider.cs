//---------------------------------------------------------------------
// <copyright file="ObjectQueryProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// LINQ query provider implementation.
    /// </summary>
    internal sealed class ObjectQueryProvider : IQueryProvider
    {
        // Although ObjectQuery contains a reference to ObjectContext, it is possible
        // that IQueryProvider methods be directly invoked from the ObjectContext.
        // This requires having a separate field to store ObjectContext reference.
        private readonly ObjectContext _context;
        private readonly ObjectQuery _query;

        /// <summary>
        /// Constructs a new provider with the given context. This constructor can be
        /// called directly when initializing ObjectContext or indirectly when initializing
        /// ObjectQuery.
        /// </summary>
        /// <param name="context">The ObjectContext of the provider.</param>
        internal ObjectQueryProvider(ObjectContext context)
        {
            Debug.Assert(null != context, "context must be given");
            _context = context;
        }

        /// <summary>
        /// Constructs a new provider with the given ObjectQuery. This ObjectQuery instance
        /// is used to transfer state information to the new ObjectQuery instance created using 
        /// the private CreateQuery method overloads.
        /// </summary>
        /// <param name="query"></param>
        internal ObjectQueryProvider(ObjectQuery query)
            : this(query.Context)
        {
            Debug.Assert(null != query, "query must be given");
            _query = query;
        }

        /// <summary>
        /// Creates a new query instance using the given LINQ expresion.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <typeparam name="S">Element type for query result.</typeparam>
        /// <param name="expression">LINQ expression forming the query.</param>
        /// <returns>ObjectQuery implementing the expression logic.</returns>
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");
            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ELinq_ExpressionMustBeIQueryable, "expression");
            }

            ObjectQuery<S> query = CreateQuery<S>(expression);

            return query;
        }

        /// <summary>
        /// Executes the given LINQ expression returning a single value, or null if the query yields
        /// no results. If the return type is unexpected, raises a cast exception.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <typeparam name="S">Type of returned value.</typeparam>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>Single result from execution.</returns>
        S IQueryProvider.Execute<S>(Expression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");
            ObjectQuery<S> query = CreateQuery<S>(expression);

            return ExecuteSingle<S>(query, expression);
        }

        /// <summary>
        /// Creates a new query instance using the given LINQ expresion.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <param name="expression">Expression forming the query.</param>
        /// <returns>ObjectQuery instance implementing the given expression.</returns>
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");
            if (!typeof(IQueryable).IsAssignableFrom(expression.Type))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ELinq_ExpressionMustBeIQueryable, "expression");
            }

            // Determine the type of the query instance by binding generic parameter in Query<>.Queryable
            // (based on element type of expression)
            Type elementType = TypeSystem.GetElementType(expression.Type);
            ObjectQuery query = CreateQuery(expression, elementType);

            return query;
        }

        /// <summary>
        /// Executes the given LINQ expression returning a single value, or null if the query yields
        /// no results.
        /// The current query is used to produce the context for the new query, but none of its logic
        /// is used.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>Single result from execution.</returns>
        object IQueryProvider.Execute(Expression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");

            ObjectQuery query = CreateQuery(expression, expression.Type);
            IEnumerable<object> objQuery = Enumerable.Cast<object>(query);
            return ExecuteSingle<object>(objQuery, expression);
        }

        /// <summary>
        /// Creates a new query from an expression.
        /// </summary>
        /// <typeparam name="S">The element type of the query.</typeparam>
        /// <param name="expression">Expression forming the query.</param>
        /// <returns>A new ObjectQuery&lt;S&gt; instance.</returns>
        private ObjectQuery<S> CreateQuery<S>(Expression expression)
        {
            ObjectQueryState queryState;
            if (_query == null)
            {
                queryState = new ELinqQueryState(typeof(S), _context, expression);
            }
            else
            {
                queryState = new ELinqQueryState(typeof(S), _query, expression);
            }
            return new ObjectQuery<S>(queryState);
        }

        /// <summary>
        /// Provides an untyped method capable of creating a strong-typed ObjectQuery
        /// (based on the <paramref name="ofType"/> argument) and returning it as an
        /// instance of the untyped (in a generic sense) ObjectQuery base class.
        /// </summary>
        /// <param name="expression">The LINQ expression that defines the new query</param>
        /// <param name="ofType">The result type of the new ObjectQuery</param>
        /// <returns>A new ObjectQuery&lt;ofType&gt;, as an instance of ObjectQuery</returns>
        private ObjectQuery CreateQuery(Expression expression, Type ofType)
        {
            ObjectQueryState queryState;
            if (_query == null)
            {
                queryState = new ELinqQueryState(ofType, _context, expression);
            }
            else
            {
                queryState = new ELinqQueryState(ofType, _query, expression);
            }
            return queryState.CreateQuery();
        }

        #region Internal Utility API

        /// <summary>
        /// Uses an expression-specific 'materialization' function to produce
        /// a singleton result from an IEnumerable query result. The function
        /// used depends on the semantics required by the expression that is
        /// the root of the query. First,FirstOrDefault and SingleOrDefault are
        /// currently handled as special cases, and the default behavior is to 
        /// use the Enumerable.Single materialization pattern.
        /// </summary>
        /// <typeparam name="TResult">The expected result type and the required element type of the IEnumerable collection</typeparam>
        /// <param name="query">The query result set</param>
        /// <param name="queryRoot">The expression that is the root of the LINQ query expression tree</param>
        /// <returns>An instance of TResult if evaluation of the expression-specific singleton-producing function is successful</returns>
        internal static TResult ExecuteSingle<TResult>(IEnumerable<TResult> query, Expression queryRoot)
        {
            return GetElementFunction<TResult>(queryRoot)(query);
        }

        private static Func<IEnumerable<TResult>, TResult> GetElementFunction<TResult>(Expression queryRoot)
        {
            SequenceMethod seqMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(queryRoot, true /*unwrapLambdas*/, out seqMethod))
            {
                switch (seqMethod)
                {
                    case SequenceMethod.First:
                    case SequenceMethod.FirstPredicate:
                            return (sequence) => { return Enumerable.First(sequence); };

                    case SequenceMethod.FirstOrDefault:
                    case SequenceMethod.FirstOrDefaultPredicate:
                            return (sequence) => { return Enumerable.FirstOrDefault(sequence); };

                    case SequenceMethod.SingleOrDefault:
                    case SequenceMethod.SingleOrDefaultPredicate:
                            return (sequence) => { return Enumerable.SingleOrDefault(sequence); };
                }
            }

            return (sequence) => { return Enumerable.Single(sequence); };
        }

        #endregion
    }
}
