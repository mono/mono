//---------------------------------------------------------------------
// <copyright file="CompiledELinqQueryState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.ELinq
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.QueryCache;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Models a compiled Linq to Entities ObjectQuery
    /// </summary>
    internal sealed class CompiledELinqQueryState : ELinqQueryState
    {
        private readonly Guid _cacheToken;
        private readonly object[] _parameterValues;
        private CompiledQueryCacheEntry _cacheEntry;

        /// <summary>
        /// Factory method to create a new compiled query state instance
        /// </summary>
        /// <param name="elementType">The element type of the new instance (the 'T' of the ObjectQuery&lt;T&gt; that the new state instance will back)"</param>
        /// <param name="context">The object context with which the new instance should be associated</param>
        /// <param name="lambda">The compiled query definition, as a <see cref="LambdaExpression"/></param>
        /// <param name="cacheToken">The cache token to use when retrieving or storing the new instance's execution plan in the query cache</param>
        /// <param name="parameterValues">The values passed into the CompiledQuery delegate</param>
        internal CompiledELinqQueryState(Type elementType, ObjectContext context, LambdaExpression lambda, Guid cacheToken, object[] parameterValues)
            : base(elementType, context, lambda)
        {
            EntityUtil.CheckArgumentNull(parameterValues, "parameterValues");

            _cacheToken = cacheToken;
            _parameterValues = parameterValues;

            this.EnsureParameters();
            this.Parameters.SetReadOnly(true);
        }

        internal override ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption)
        {
            Debug.Assert(this.Span == null, "Include span specified on compiled LINQ-based ObjectQuery instead of within the expression tree?");
            Debug.Assert(this._cachedPlan == null, "Cached plan should not be set on compiled LINQ queries");

            // Metadata is required to generate the execution plan or to retrieve it from the cache.
            this.ObjectContext.EnsureMetadata();

            ObjectQueryExecutionPlan plan = null;
            CompiledQueryCacheEntry cacheEntry = this._cacheEntry;
            bool useCSharpNullComparisonBehavior = this.ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior;
            if (cacheEntry != null)
            {
                // The cache entry has already been retrieved, so compute the effective merge option with the following precedence:
                // 1. The merge option specified as the argument to Execute(MergeOption), and so to this method
                // 2. The merge option set using ObjectQuery.MergeOption
                // 3. The propagated merge option as recorded in the cache entry
                // 4. The global default merge option.
                MergeOption mergeOption = EnsureMergeOption(forMergeOption, this.UserSpecifiedMergeOption, cacheEntry.PropagatedMergeOption);

                // Ask for the corresponding execution plan
                plan = cacheEntry.GetExecutionPlan(mergeOption, useCSharpNullComparisonBehavior);
                if (plan == null)
                {
                    // Convert the LINQ expression to produce a command tree
                    ExpressionConverter converter = this.CreateExpressionConverter();
                    DbExpression queryExpression = converter.Convert();
                    ReadOnlyCollection<KeyValuePair<ObjectParameter, QueryParameterExpression>> parameters = converter.GetParameters();

                    // Prepare the execution plan using the command tree and the computed effective merge option
                    DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(this.ObjectContext.MetadataWorkspace, DataSpace.CSpace, queryExpression);
                    plan = ObjectQueryExecutionPlan.Prepare(this.ObjectContext, tree, this.ElementType, mergeOption, converter.PropagatedSpan, parameters, converter.AliasGenerator);

                    // Update and retrieve the execution plan
                    plan = cacheEntry.SetExecutionPlan(plan, useCSharpNullComparisonBehavior);
                }
            }
            else
            {
                // This instance does not yet have a reference to a cache entry.
                // First, attempt to retrieve an existing cache entry.
                QueryCacheManager cacheManager = this.ObjectContext.MetadataWorkspace.GetQueryCacheManager();
                CompiledQueryCacheKey cacheKey = new CompiledQueryCacheKey(this._cacheToken);

                if (cacheManager.TryCacheLookup(cacheKey, out cacheEntry))
                {
                    // An entry was found in the cache, so compute the effective merge option based on its propagated merge option,
                    // and use the UseCSharpNullComparisonBehavior flag to retrieve the corresponding execution plan.
                    this._cacheEntry = cacheEntry;
                    MergeOption mergeOption = EnsureMergeOption(forMergeOption, this.UserSpecifiedMergeOption, cacheEntry.PropagatedMergeOption);
                    plan = cacheEntry.GetExecutionPlan(mergeOption, useCSharpNullComparisonBehavior);
                }

                // If no cache entry was found or if the cache entry did not contain the required execution plan, the plan is still null at this point.
                if (plan == null)
                {
                    // The execution plan needs to be produced, so create an appropriate expression converter and generate the query command tree.
                    ExpressionConverter converter = this.CreateExpressionConverter();
                    DbExpression queryExpression = converter.Convert();
                    ReadOnlyCollection<KeyValuePair<ObjectParameter, QueryParameterExpression>> parameters = converter.GetParameters();
                    DbQueryCommandTree tree = DbQueryCommandTree.FromValidExpression(this.ObjectContext.MetadataWorkspace, DataSpace.CSpace, queryExpression);

                    // If a cache entry for this compiled query's cache key was not successfully retrieved, then it must be created now.
                    // Note that this is only possible after converting the LINQ expression and discovering the propagated merge option,
                    // which is required in order to create the cache entry.
                    if (cacheEntry == null)
                    {
                        // Create the cache entry using this instance's cache token and the propagated merge option (which may be null)
                        cacheEntry = new CompiledQueryCacheEntry(cacheKey, converter.PropagatedMergeOption);

                        // Attempt to add the entry to the cache. If an entry was added in the meantime, use that entry instead.
                        QueryCacheEntry foundEntry;
                        if (cacheManager.TryLookupAndAdd(cacheEntry, out foundEntry))
                        {
                            cacheEntry = (CompiledQueryCacheEntry)foundEntry;
                        }

                        // We now have a cache entry, so hold onto it for future use.
                        this._cacheEntry = cacheEntry;
                    }

                    // Recompute the effective merge option in case a cache entry was just constructed above
                    MergeOption mergeOption = EnsureMergeOption(forMergeOption, this.UserSpecifiedMergeOption, cacheEntry.PropagatedMergeOption);

                    // Ask the (retrieved or constructed) cache entry for the corresponding execution plan.
                    plan = cacheEntry.GetExecutionPlan(mergeOption, useCSharpNullComparisonBehavior);
                    if (plan == null)
                    {
                        // The plan is not present, so prepare it now using the computed effective merge option
                        plan = ObjectQueryExecutionPlan.Prepare(this.ObjectContext, tree, this.ElementType, mergeOption, converter.PropagatedSpan, parameters, converter.AliasGenerator);

                        // Update the execution plan on the cache entry.
                        // If the execution plan was set in the meantime, SetExecutionPlan will return that value, otherwise it will return 'plan'.
                        plan = cacheEntry.SetExecutionPlan(plan, useCSharpNullComparisonBehavior);
                    }
                }
            }

            // Get parameters from the plan and set them.
            ObjectParameterCollection currentParams = this.EnsureParameters();
            if (plan.CompiledQueryParameters != null && plan.CompiledQueryParameters.Count > 0)
            {
                currentParams.SetReadOnly(false);
                currentParams.Clear();
                foreach (KeyValuePair<ObjectParameter, QueryParameterExpression> pair in plan.CompiledQueryParameters)
                {
                    // Parameters retrieved from the CompiledQueryParameters collection must be cloned before being added to the query.
                    // The cached plan is shared and when used in multithreaded scenarios failing to clone the parameter would result
                    // in the code below updating the values of shared parameter instances saved in the cached plan and used by all
                    // queries using that plan, regardless of the values they were actually invoked with, causing incorrect results
                    // when those queries were later executed.
                    //
                    ObjectParameter convertedParam = pair.Key.ShallowCopy();
                    QueryParameterExpression parameterExpression = pair.Value;
                    currentParams.Add(convertedParam);
                    if (parameterExpression != null)
                    {
                        convertedParam.Value = parameterExpression.EvaluateParameter(_parameterValues);
                    }
                }
            }
            currentParams.SetReadOnly(true);

            Debug.Assert(plan != null, "Failed to produce an execution plan?");
            return plan;
        }

        /// <summary>
        /// Overrides GetResultType and attempts to first retrieve the result type from the cache entry.
        /// </summary>
        /// <returns>The query result type from this compiled query's cache entry, if possible; otherwise defers to <see cref="ELinqQueryState.GetResultType"/></returns>
        protected override TypeUsage GetResultType()
        {
            CompiledQueryCacheEntry cacheEntry = this._cacheEntry;
            TypeUsage resultType;
            if (cacheEntry != null &&
                cacheEntry.TryGetResultType(out resultType))
            {
                return resultType;
            }

            return base.GetResultType();
        }

        /// <summary>
        /// Gets a LINQ expression that defines this query. 
        /// This is overridden to remove parameter references from the underlying expression,
        /// producing an expression that contains the values of those parameters as <see cref="ConstantExpression"/>s.
        /// </summary>
        internal override Expression Expression
        {
            get
            {
                return CreateDonateableExpressionVisitor.Replace((LambdaExpression)base.Expression, ObjectContext, _parameterValues);
            }
        }

        /// <summary>
        /// Overrides CreateExpressionConverter to return a converter that uses a binding context based on the compiled query parameters,
        /// rather than a default binding context.
        /// </summary>
        /// <returns>An expression converter appropriate for converting this compiled query state instance</returns>
        protected override ExpressionConverter CreateExpressionConverter()
        {
            LambdaExpression lambda = (LambdaExpression)base.Expression;
            Funcletizer funcletizer = Funcletizer.CreateCompiledQueryEvaluationFuncletizer(this.ObjectContext, lambda.Parameters.First(), lambda.Parameters.Skip(1).ToList().AsReadOnly());
            // Return a new expression converter that uses the initialized command tree and binding context.
            return new ExpressionConverter(funcletizer, lambda.Body);
        }

        /// <summary>
        /// Replaces ParameterExpresion with ConstantExpression
        /// to make the expression usable as a donor expression
        /// </summary>
        private sealed class CreateDonateableExpressionVisitor : EntityExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, object> _parameterToValueLookup;

            private CreateDonateableExpressionVisitor(Dictionary<ParameterExpression, object> parameterToValueLookup)
            {
                _parameterToValueLookup = parameterToValueLookup;
            }

            internal static Expression Replace(LambdaExpression query, ObjectContext objectContext, object[] parameterValues)
            {
                Dictionary<ParameterExpression, object> parameterLookup = query
                    .Parameters
                    .Skip(1)
                    .Zip(parameterValues)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                parameterLookup.Add(query.Parameters.First(), objectContext);
                var replacer = new CreateDonateableExpressionVisitor(parameterLookup);
                return replacer.Visit(query.Body);
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                object value;
                Expression result;
                if (_parameterToValueLookup.TryGetValue(p, out value))
                {
                    result = Expression.Constant(value, p.Type);
                }
                else
                {
                    result = base.VisitParameter(p);
                }
                return result;
            }
        }
    }
}
