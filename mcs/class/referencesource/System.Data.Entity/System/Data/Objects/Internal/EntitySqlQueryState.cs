//---------------------------------------------------------------------
// <copyright file="EntitySqlQueryState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.EntitySql;
    using System.Data.Common.QueryCache;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.Internal;
    using System.Diagnostics;

    /// <summary>
    /// ObjectQueryState based on Entity-SQL query text.
    /// </summary>
    internal sealed class EntitySqlQueryState : ObjectQueryState
    {
        /// <summary>
        /// The Entity-SQL text that defines the query.
        /// </summary>
        /// <remarks>
        /// It is important that this field is readonly for consistency reasons wrt <see cref="_queryExpression"/>.
        /// If this field becomes read-write, then write should be allowed only when <see cref="_queryExpression"/> is null, 
        /// or there should be a mechanism keeping both fields consistent.
        /// </remarks>
        private readonly string _queryText;

        /// <summary>
        /// Optional <see cref="DbExpression"/> that defines the query. Must be semantically equal to the <see cref="_queryText"/>.
        /// </summary>
        /// <remarks>
        /// It is important that this field is readonly for consistency reasons wrt <see cref="_queryText"/>.
        /// If this field becomes read-write, then there should be a mechanism keeping both fields consistent.
        /// </remarks>
        private readonly DbExpression _queryExpression;

        /// <summary>
        ///     Can a Limit subclause be appended to the text of this query?
        /// </summary>
        private readonly bool _allowsLimit;

        /// <summary>
        /// Initializes a new query EntitySqlQueryState instance.
        /// </summary>
        /// <param name="context">
        ///     The ObjectContext containing the metadata workspace the query was
        ///     built against, the connection on which to execute the query, and the
        ///     cache to store the results in. Must not be null.
        /// </param>
        /// <param name="commandText">
        ///     The Entity-SQL text of the query
        /// </param>
        /// <param name="mergeOption">
        ///     The merge option to use when retrieving results if an explicit merge option is not specified
        /// </param>
        internal EntitySqlQueryState(Type elementType, string commandText, bool allowsLimit, ObjectContext context, ObjectParameterCollection parameters, Span span)
            : this(elementType, commandText, /*expression*/ null, allowsLimit, context, parameters, span)
        { }
        
        /// <summary>
        /// Initializes a new query EntitySqlQueryState instance.
        /// </summary>
        /// <param name="context">
        ///     The ObjectContext containing the metadata workspace the query was
        ///     built against, the connection on which to execute the query, and the
        ///     cache to store the results in. Must not be null.
        /// </param>
        /// <param name="commandText">
        ///     The Entity-SQL text of the query
        /// </param>
        /// <param name="expression">
        ///     Optional <see cref="DbExpression"/> that defines the query. Must be semantically equal to the <paramref name="commandText"/>.
        /// </param>
        /// <param name="mergeOption">
        ///     The merge option to use when retrieving results if an explicit merge option is not specified
        /// </param>
        internal EntitySqlQueryState(Type elementType, string commandText, DbExpression expression, bool allowsLimit, ObjectContext context, ObjectParameterCollection parameters, Span span)
            : base(elementType, context, parameters, span)
        {
            EntityUtil.CheckArgumentNull(commandText, "commandText");
            if (string.IsNullOrEmpty(commandText))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_InvalidEmptyQuery, "commandText");
            }

            _queryText = commandText;
            _queryExpression = expression;
            _allowsLimit = allowsLimit;
        }

        /// <summary>
        ///     Determines whether or not the current query is a 'Skip' or 'Sort' operation
        ///     and so would allow a 'Limit' clause to be appended to the current query text.
        /// </summary>
        /// <returns>
        ///     <c>True</c> if the current query is a Skip or Sort expression, or a
        ///     Project expression with a Skip or Sort expression input.
        /// </returns>
        internal bool AllowsLimitSubclause { get { return _allowsLimit; } }

        /// <summary>
        /// Always returns the Entity-SQL text of the implemented ObjectQuery.
        /// </summary>
        /// <param name="commandText">Always set to the Entity-SQL text of this ObjectQuery.</param>
        /// <returns>Always returns <c>true</c>.</returns>
        internal override bool TryGetCommandText(out string commandText)
        {
            commandText = this._queryText;
            return true;
        }

        internal override bool TryGetExpression(out System.Linq.Expressions.Expression expression)
        {
            expression = null;
            return false;
        }

        protected override TypeUsage GetResultType()
        {
            DbExpression query = this.Parse();
            return query.ResultType;
        }

        internal override ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath)
        {
            ObjectQueryState retState = new EntitySqlQueryState(this.ElementType, _queryText, _queryExpression, _allowsLimit, this.ObjectContext, ObjectParameterCollection.DeepCopy(this.Parameters), Span.IncludeIn(this.Span, includePath));
            this.ApplySettingsTo(retState);
            return retState;
        }

        internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
        {
            // Metadata is required to generate the execution plan or to retrieve it from the cache.
            this.ObjectContext.EnsureMetadata();

            // Determine the required merge option, with the following precedence:
            // 1. The merge option specified to Execute(MergeOption) as forMergeOption.
            // 2. The merge option set via ObjectQuery.MergeOption.
            // 3. The global default merge option.
            MergeOption mergeOption = EnsureMergeOption(forMergeOption, this.UserSpecifiedMergeOption);

            // If a cached plan is present, then it can be reused if it has the required merge option
            // (since span and parameters cannot change between executions). However, if the cached
            // plan does not have the required merge option we proceed as if it were not present.
            ObjectQueryExecutionPlan plan = this._cachedPlan;
            if (plan != null)
            {
                if (plan.MergeOption == mergeOption)
                {
                    return plan;
                }
                else
                {
                    plan = null;
                }
            }

            // There is no cached plan (or it was cleared), so the execution plan must be retrieved from
            // the global query cache (if plan caching is enabled) or rebuilt for the required merge option.
            QueryCacheManager cacheManager = null;
            EntitySqlQueryCacheKey cacheKey = null;
            if (this.PlanCachingEnabled)
            {
                // Create a new cache key that reflects the current state of the Parameters collection
                // and the Span object (if any), and uses the specified merge option.
                cacheKey = new EntitySqlQueryCacheKey(
                                   this.ObjectContext.DefaultContainerName,
                                   _queryText,
                                   (null == this.Parameters ? 0 : this.Parameters.Count),
                                   (null == this.Parameters ? null : this.Parameters.GetCacheKey()),
                                   (null == this.Span ? null : this.Span.GetCacheKey()),
                                   mergeOption,
                                   this.ElementType);

                cacheManager = this.ObjectContext.MetadataWorkspace.GetQueryCacheManager();
                ObjectQueryExecutionPlan executionPlan = null;
                if (cacheManager.TryCacheLookup(cacheKey, out executionPlan))
                {
                    plan = executionPlan;
                }
            }

            if (plan == null)
            {
                // Either caching is not enabled or the execution plan was not found in the cache
                DbExpression queryExpression = this.Parse();
                Debug.Assert(queryExpression != null, "EntitySqlQueryState.Parse returned null expression?");
                DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(this.ObjectContext.MetadataWorkspace, DataSpace.CSpace, queryExpression);
                plan = ObjectQueryExecutionPlan.Prepare(this.ObjectContext, tree, this.ElementType, mergeOption, this.Span, null, DbExpressionBuilder.AliasGenerator);

                // If caching is enabled then update the cache now.
                // Note: the logic is the same as in ELinqQueryState.
                if (cacheKey != null)
                {
                    var newEntry = new QueryCacheEntry(cacheKey, plan);
                    QueryCacheEntry foundEntry = null;
                    if (cacheManager.TryLookupAndAdd(newEntry, out foundEntry))
                    {
                        // If TryLookupAndAdd returns 'true' then the entry was already present in the cache when the attempt to add was made.
                        // In this case the existing execution plan should be used.
                        plan = (ObjectQueryExecutionPlan)foundEntry.GetTarget();
                    }
                }
            }

            if (this.Parameters != null)
            {
                this.Parameters.SetReadOnly(true);
            }

            // Update the cached plan with the newly retrieved/prepared plan
            this._cachedPlan = plan;

            // Return the execution plan
            return plan;
        }

        internal DbExpression Parse()
        {
            if (_queryExpression != null)
            {
                return _queryExpression;
            }

            List<DbParameterReferenceExpression> parameters = null;
            if (this.Parameters != null)
            {
                parameters = new List<DbParameterReferenceExpression>(this.Parameters.Count);
                foreach (ObjectParameter parameter in this.Parameters)
                {
                    TypeUsage typeUsage = parameter.TypeUsage;
                    if (null == typeUsage)
                    {
                        // Since ObjectParameters do not allow users to specify 'facets', make 
                        // sure that the parameter TypeUsage is not populated with the provider
                        // default facet values.
                        this.ObjectContext.Perspective.TryGetTypeByName(
                                        parameter.MappableType.FullName,
                                        false /* bIgnoreCase */,
                                        out typeUsage);
                    }

                    Debug.Assert(typeUsage != null, "typeUsage != null");
                    
                    parameters.Add(typeUsage.Parameter(parameter.Name));
                }
            }

            DbLambda lambda =
                CqlQuery.CompileQueryCommandLambda(
                    _queryText,                     // Command Text
                    this.ObjectContext.Perspective, // Perspective
                    null,                           // Parser options - null indicates 'use default'
                    parameters,                     // Parameters
                    null                            // Variables
                );

            Debug.Assert(lambda.Variables == null || lambda.Variables.Count == 0, "lambda.Variables must be empty");

            return lambda.Body;
        }
    }
}
