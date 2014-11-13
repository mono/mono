//---------------------------------------------------------------------
// <copyright file="ObjectQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupowner [....]
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Objects.ELinq;
using System.Data.Objects.Internal;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Objects
{
    /// <summary>
    ///   This class implements strongly-typed queries at the object-layer through
    ///   Entity SQL text and query-building helper methods. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public partial class ObjectQuery<T> : ObjectQuery, IEnumerable<T>, IQueryable<T>, IOrderedQueryable<T>, IListSource
    {
        internal ObjectQuery(ObjectQueryState queryState)
            : base(queryState)
        {
        }

        #region Public Methods

        /// <summary>
        ///   This method allows explicit query evaluation with a specified merge
        ///   option which will override the merge option property.
        /// </summary>
        /// <param name="mergeOption">
        ///   The MergeOption to use when executing the query.
        /// </param>
        /// <returns>
        ///   An enumerable for the ObjectQuery results.
        /// </returns>
        public new ObjectResult<T> Execute(MergeOption mergeOption)
        {
            EntityUtil.CheckArgumentMergeOption(mergeOption);
            return this.GetResults(mergeOption);
        }

        /// <summary>
        ///   Adds a path to the set of navigation property span paths included in the results of this query
        /// </summary>
        /// <param name="path">The new span path</param>
        /// <returns>A new ObjectQuery that includes the specified span path</returns>
        public ObjectQuery<T> Include(string path)
        {
            EntityUtil.CheckStringArgument(path, "path");
            return new ObjectQuery<T>(this.QueryState.Include(this, path));
        }
        
        #endregion

        #region IEnumerable<T> implementation

        /// <summary>
        ///   These methods are the "executors" for the query. They can be called
        ///   directly, or indirectly (by foreach'ing through the query, for example).
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ObjectResult<T> disposableEnumerable = this.GetResults(null);
            try
            {
                IEnumerator<T> result = disposableEnumerable.GetEnumerator();
                return result;
            }
            catch
            {
                // if there is a problem creating the enumerator, we should dispose
                // the enumerable (if there is no problem, the enumerator will take 
                // care of the dispose)
                disposableEnumerable.Dispose();
                throw;
            }
        }

        #endregion

        #region ObjectQuery Overrides
        
        internal override IEnumerator GetEnumeratorInternal()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        internal override IList GetIListSourceListInternal()
        {
            return ((IListSource)this.GetResults(null)).GetList();
        }

        internal override ObjectResult ExecuteInternal(MergeOption mergeOption)
        {
            return this.GetResults(mergeOption);
        }

        /// <summary>
        /// Retrieves the LINQ expression that backs this ObjectQuery for external consumption.
        /// It is important that the work to wrap the expression in an appropriate MergeAs call
        /// takes place in this method and NOT in ObjectQueryState.TryGetExpression which allows
        /// the unmodified expression (that does not include the MergeOption-preserving MergeAs call)
        /// to be retrieved and processed by the ELinq ExpressionConverter.
        /// </summary>
        /// <returns>
        ///   The LINQ expression for this ObjectQuery, wrapped in a MergeOption-preserving call
        ///   to the MergeAs method if the ObjectQuery.MergeOption property has been set.
        /// </returns>
        internal override Expression GetExpression()
        {
            // If this ObjectQuery is not backed by a LINQ Expression (it is an ESQL query),
            // then create a ConstantExpression that uses this ObjectQuery as its value.
            Expression retExpr;
            if (!this.QueryState.TryGetExpression(out retExpr))
            {
                retExpr = Expression.Constant(this);
            }

            Type objectQueryType = typeof(ObjectQuery<T>);
            if (this.QueryState.UserSpecifiedMergeOption.HasValue)
            {
                MethodInfo mergeAsMethod = objectQueryType.GetMethod("MergeAs", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(mergeAsMethod != null, "Could not retrieve ObjectQuery<T>.MergeAs method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, mergeAsMethod, Expression.Constant(this.QueryState.UserSpecifiedMergeOption.Value));
            }

            if (null != this.QueryState.Span)
            {
                MethodInfo includeSpanMethod = objectQueryType.GetMethod("IncludeSpan", BindingFlags.Instance | BindingFlags.NonPublic);
                Debug.Assert(includeSpanMethod != null, "Could not retrieve ObjectQuery<T>.IncludeSpan method using reflection?");
                retExpr = TypeSystem.EnsureType(retExpr, objectQueryType);
                retExpr = Expression.Call(retExpr, includeSpanMethod, Expression.Constant(this.QueryState.Span));
            }

            return retExpr;
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        internal ObjectQuery<T> MergeAs(MergeOption mergeOption)
        {
            throw EntityUtil.InvalidOperation(Strings.ELinq_MethodNotDirectlyCallable);
        }

        // Intended for use only in the MethodCallExpression produced for inline queries.
        internal ObjectQuery<T> IncludeSpan(Span span)
        {
            throw EntityUtil.InvalidOperation(Strings.ELinq_MethodNotDirectlyCallable);
        }

        #endregion

        #region Private Methods

        private ObjectResult<T> GetResults(MergeOption? forMergeOption)
        {
            this.QueryState.ObjectContext.EnsureConnection();

            try
            {
                ObjectQueryExecutionPlan execPlan = this.QueryState.GetExecutionPlan(forMergeOption);
                return execPlan.Execute<T>(this.QueryState.ObjectContext, this.QueryState.Parameters);
            }
            catch
            {
                this.QueryState.ObjectContext.ReleaseConnection();
                throw;
            }
        }

        #endregion
    }
}
