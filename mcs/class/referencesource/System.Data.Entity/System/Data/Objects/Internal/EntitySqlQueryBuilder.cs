//---------------------------------------------------------------------
// <copyright file="EntitySqlQueryBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Provides Entity-SQL query building services for <see cref="EntitySqlQueryState"/>. 
    /// Knowledge of how to compose Entity-SQL fragments using query builder operators resides entirely in this class.
    /// </summary>
    internal static class EntitySqlQueryBuilder
    {        
        /// <summary>
        /// Helper method to extract the Entity-SQL command text from an <see cref="ObjectQueryState"/> instance if that
        /// instance models an Entity-SQL-backed ObjectQuery, or to throw an exception indicating that query builder methods
        /// are not supported on this query.
        /// </summary>
        /// <param name="query">The instance from which the Entity-SQL command text should be retrieved</param>
        /// <returns>The Entity-SQL command text, if the specified query state instance is based on Entity-SQL</returns>
        /// <exception cref="NotSupportedException">
        ///     If the specified instance is not based on Entity-SQL command text, and so does not support Entity-SQL query builder methods
        /// </exception>
        private static string GetCommandText(ObjectQueryState query)
        {
            string commandText = null;
            if(!query.TryGetCommandText(out commandText))
            {
                throw EntityUtil.NotSupported(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_NotSupportedLinqSource);
            }

            return commandText;
        }

        /// <summary>
        /// Merges <see cref="ObjectParameter"/>s from a source ObjectQuery with ObjectParameters specified as an argument to a builder method.
        /// A new <see cref="ObjectParameterCollection"/> is returned that contains copies of parameters from both <paramref name="sourceQueryParams"/> and <paramref name="builderMethodParams"/>.
        /// </summary>
        /// <param name="context">The <see cref="ObjectContext"/> to use when constructing the new parameter collection</param>
        /// <param name="sourceQueryParams">ObjectParameters from the ObjectQuery on which the query builder method was called</param>
        /// <param name="builderMethodParams">ObjectParameters that were specified as an argument to the builder method</param>
        /// <returns>A new ObjectParameterCollection containing copies of all parameters</returns>
        private static ObjectParameterCollection MergeParameters(ObjectContext context, ObjectParameterCollection sourceQueryParams, ObjectParameter[] builderMethodParams)
        {
            Debug.Assert(builderMethodParams != null, "params array argument should not be null");
            if (sourceQueryParams == null && builderMethodParams.Length == 0)
            {
                return null;
            }

            ObjectParameterCollection mergedParams = ObjectParameterCollection.DeepCopy(sourceQueryParams);
            if (mergedParams == null)
            {
                mergedParams = new ObjectParameterCollection(context.Perspective);
            }

            foreach (ObjectParameter builderParam in builderMethodParams)
            {
                mergedParams.Add(builderParam);
            }

            return mergedParams;
        }

        /// <summary>
        /// Merges <see cref="ObjectParameter"/>s from two ObjectQuery arguments to SetOp builder methods (Except, Intersect, Union, UnionAll).
        /// A new <see cref="ObjectParameterCollection"/> is returned that contains copies of parameters from both <paramref name="query1Params"/> and <paramref name="query2Params"/>.
        /// </summary>
        /// <param name="query1Params">ObjectParameters from the first ObjectQuery argument (on which the query builder method was called)</param>
        /// <param name="query2Params">ObjectParameters from the second ObjectQuery argument (specified as an argument to the builder method)</param>
        /// <returns>A new ObjectParameterCollection containing copies of all parameters</returns>
        private static ObjectParameterCollection MergeParameters(ObjectParameterCollection query1Params, ObjectParameterCollection query2Params)
        {
            if (query1Params == null && query2Params == null)
            {
                return null;
            }

            ObjectParameterCollection mergedParams;
            ObjectParameterCollection sourceParams;
            if (query1Params != null)
            {
                mergedParams = ObjectParameterCollection.DeepCopy(query1Params);
                sourceParams = query2Params;
            }
            else
            {
                mergedParams = ObjectParameterCollection.DeepCopy(query2Params);
                sourceParams = query1Params;
            }

            if (sourceParams != null)
            {
                foreach (ObjectParameter sourceParam in sourceParams)
                {
                    mergedParams.Add(sourceParam.ShallowCopy());
                }
            }

            return mergedParams;
        }

        private static ObjectQueryState NewBuilderQuery(ObjectQueryState sourceQuery, Type elementType, StringBuilder queryText, Span newSpan, IEnumerable<ObjectParameter> enumerableParams)
        {
            return NewBuilderQuery(sourceQuery, elementType, queryText, false, newSpan, enumerableParams);
        }

        private static ObjectQueryState NewBuilderQuery(ObjectQueryState sourceQuery, Type elementType, StringBuilder queryText, bool allowsLimit, Span newSpan, IEnumerable<ObjectParameter> enumerableParams)
        {
            ObjectParameterCollection queryParams = enumerableParams as ObjectParameterCollection;
            if (queryParams == null && enumerableParams != null)
            {
                queryParams = new ObjectParameterCollection(sourceQuery.ObjectContext.Perspective);
                foreach (ObjectParameter objectParam in enumerableParams)
                {
                    queryParams.Add(objectParam);
                }
            }

            EntitySqlQueryState newState = new EntitySqlQueryState(elementType, queryText.ToString(), allowsLimit, sourceQuery.ObjectContext, queryParams, newSpan);
            
            sourceQuery.ApplySettingsTo(newState);
            
            return newState;
        }

        // Note that all query builder string constants contain embedded newlines to prevent manipulation of the
        // query text by single line comments (--) that might appear in user-supplied portions of the string such
        // as a filter predicate, projection list, etc.
        
        #region SetOp Helpers

        private const string _setOpEpilog =
@"
)";

        private const string _setOpProlog =
@"(
";

        // SetOp helper - note that this doesn't merge Spans, since Except uses the original query's Span
        // while Intersect/Union/UnionAll use the merged Span.
        private static ObjectQueryState BuildSetOp(ObjectQueryState leftQuery, ObjectQueryState rightQuery, Span newSpan, string setOp)
        {
            // Assert that the arguments aren't null (should have been verified by ObjectQuery)
            Debug.Assert(leftQuery != null, "Left query is null?");
            Debug.Assert(rightQuery != null, "Right query is null?");
            Debug.Assert(leftQuery.ElementType.Equals(rightQuery.ElementType), "Incompatible element types in arguments to Except<T>/Intersect<T>/Union<T>/UnionAll<T>?");

            // Retrieve the left and right arguments to the set operation - 
            // this will throw if either input query is not an Entity-SQL query.
            string left = GetCommandText(leftQuery);
            string right = GetCommandText(rightQuery);
                        
            // ObjectQuery arguments must be associated with the same ObjectContext instance as the implemented query
            if (!object.ReferenceEquals(leftQuery.ObjectContext, rightQuery.ObjectContext))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectQuery_QueryBuilder_InvalidQueryArgument, "query"); 
            }
                                    
            // Create a string builder only large enough to contain the new query text
            int queryLength = _setOpProlog.Length + left.Length + setOp.Length + right.Length + _setOpEpilog.Length;
            StringBuilder builder = new StringBuilder(queryLength);

            // Build the new query
            builder.Append(_setOpProlog);
            builder.Append(left);
            builder.Append(setOp);
            builder.Append(right);
            builder.Append(_setOpEpilog);

            // Create a new query implementation and apply the state of this implementation to it.
            // The Span of the query argument will be merged into the new query's Span by the caller, iff the Set Op is NOT Except.
            // See the Except, Intersect, Union and UnionAll methods in this class for examples.
            return NewBuilderQuery(leftQuery, leftQuery.ElementType, builder, newSpan, MergeParameters(leftQuery.Parameters, rightQuery.Parameters));
        }
        #endregion

        #region Select/SelectValue Helpers
        
        private const string _fromOp =
@"
FROM (
";

        private const string _asOp = 
@"
) AS ";

        private static ObjectQueryState BuildSelectOrSelectValue(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters, string projectOp, Type elementType)
        {
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(alias), "Invalid alias");
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(projection), "Invalid projection");

            string queryText = GetCommandText(query);

            // Build the new query string - "<project op> <projection> FROM (<this query>) AS <alias>"
            int queryLength = projectOp.Length +
                              projection.Length +
                              _fromOp.Length +
                              queryText.Length +
                              _asOp.Length +
                              alias.Length;

            StringBuilder builder = new StringBuilder(queryLength);
            builder.Append(projectOp);
            builder.Append(projection);
            builder.Append(_fromOp);
            builder.Append(queryText);
            builder.Append(_asOp);
            builder.Append(alias);

            // Create a new EntitySqlQueryImplementation that uses the new query as its command text.
            // Span should not be carried over from a Select or SelectValue operation.
            return NewBuilderQuery(query, elementType, builder, null, MergeParameters(query.ObjectContext, query.Parameters, parameters));
        }

        #endregion

        #region OrderBy/Where Helper

        private static ObjectQueryState BuildOrderByOrWhere(ObjectQueryState query, string alias, string predicateOrKeys, ObjectParameter[] parameters, string op, string skipCount, bool allowsLimit)
        {
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(alias), "Invalid alias");
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(predicateOrKeys), "Invalid predicate/keys");
            Debug.Assert(null == skipCount || op == _orderByOp, "Skip clause used with WHERE operator?");

            string queryText = GetCommandText(query);

            // Build the new query string:
            // Either: "SELECT VALUE <alias> FROM (<this query>) AS <alias> WHERE <predicate>"
            //  (for Where)
            // Or:  "SELECT VALUE <alias> FROM (<this query>) AS <alias> ORDER BY <keys> <optional: SKIP <skip>>"
            // Depending on the value of 'op'
            int queryLength = _selectValueOp.Length +
                              alias.Length +
                              _fromOp.Length +
                              queryText.Length +
                              _asOp.Length +
                              alias.Length +
                              op.Length +
                              predicateOrKeys.Length;
            
            if (skipCount != null)
            {
                queryLength += (_skipOp.Length + skipCount.Length);
            }

            StringBuilder builder = new StringBuilder(queryLength);
            builder.Append(_selectValueOp);
            builder.Append(alias);
            builder.Append(_fromOp);
            builder.Append(queryText);
            builder.Append(_asOp);
            builder.Append(alias);
            builder.Append(op);
            builder.Append(predicateOrKeys);
            if (skipCount != null)
            {
                builder.Append(_skipOp);
                builder.Append(skipCount);
            }

            // Create a new EntitySqlQueryImplementation that uses the new query as its command text.
            // Span is carried over, no adjustment is needed.
            return NewBuilderQuery(query, query.ElementType, builder, allowsLimit, query.Span, MergeParameters(query.ObjectContext, query.Parameters,  parameters));
        }

        #endregion
        
        
        #region Distinct

        private const string _distinctProlog = 
@"SET(
";

        private const string _distinctEpilog = 
@"
)";

        internal static ObjectQueryState Distinct(ObjectQueryState query)
        {
            // Build the new query string - "SET(<this query>)"
            string queryText = GetCommandText(query);
            StringBuilder builder = new StringBuilder(_distinctProlog.Length + queryText.Length + _distinctEpilog.Length);
            builder.Append(_distinctProlog);
            builder.Append(queryText);
            builder.Append(_distinctEpilog);

            // Span is carried over, no adjustment is needed

            return NewBuilderQuery(query, query.ElementType, builder, query.Span, ObjectParameterCollection.DeepCopy(query.Parameters));
        }

        #endregion

        #region Except

        private const string _exceptOp = 
@"
) EXCEPT (
";
                
        internal static ObjectQueryState Except(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
        {
            // Call the SetOp helper.
            // Span is taken from the leftmost query.
            return EntitySqlQueryBuilder.BuildSetOp(leftQuery, rightQuery, leftQuery.Span, _exceptOp);
        }

        #endregion

        #region GroupBy

        private const string _groupByOp = 
@"
GROUP BY
";

        internal static ObjectQueryState GroupBy(ObjectQueryState query, string alias, string keys, string projection, ObjectParameter[] parameters)
        {
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(alias), "Invalid alias");
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(alias), "Invalid keys");
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(projection), "Invalid projection");

            string queryText = GetCommandText(query);

            // Build the new query string:
            // "SELECT <projection> FROM (<this query>) AS <alias> GROUP BY <keys>"
            int queryLength = _selectOp.Length +
                              projection.Length +
                              _fromOp.Length +
                              queryText.Length +
                              _asOp.Length +
                              alias.Length +
                              _groupByOp.Length +
                              keys.Length;

            StringBuilder builder = new StringBuilder(queryLength);
            builder.Append(_selectOp);
            builder.Append(projection);
            builder.Append(_fromOp);
            builder.Append(queryText);
            builder.Append(_asOp);
            builder.Append(alias);
            builder.Append(_groupByOp);
            builder.Append(keys);

            // Create a new EntitySqlQueryImplementation that uses the new query as its command text.
            // Span should not be carried over from a GroupBy operation.
            return NewBuilderQuery(query, typeof(DbDataRecord), builder, null, MergeParameters(query.ObjectContext, query.Parameters, parameters));
        }

        #endregion

        #region Intersect

        private const string _intersectOp = 
@"
) INTERSECT (
";
        
        internal static ObjectQueryState Intersect(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
        {
            // Ensure the Spans of the query arguments are merged into the new query's Span.
            Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
            // Call the SetOp helper.
            return BuildSetOp(leftQuery, rightQuery, newSpan, _intersectOp);
        }

        #endregion

        #region OfType

        private const string _ofTypeProlog = 
@"OFTYPE(
(
";
        
        private const string _ofTypeInfix = 
@"
),
[";

        private const string _ofTypeInfix2 = "].[";

        private const string _ofTypeEpilog = 
@"]
)";

        internal static ObjectQueryState OfType(ObjectQueryState query, EdmType newType, Type clrOfType)
        {
            Debug.Assert(newType != null, "OfType cannot be null");
            Debug.Assert(Helper.IsEntityType(newType) || Helper.IsComplexType(newType), "OfType must be Entity or Complex type");

            string queryText = GetCommandText(query);

            // Build the new query string - "OFTYPE((<query>), [<type namespace>].[<type name>])"
            int queryLength = _ofTypeProlog.Length +
                              queryText.Length +
                              _ofTypeInfix.Length +
                              newType.NamespaceName.Length +
                              (newType.NamespaceName != string.Empty ? _ofTypeInfix2.Length : 0) +
                              newType.Name.Length +
                              _ofTypeEpilog.Length;

            StringBuilder builder = new StringBuilder(queryLength);
            builder.Append(_ofTypeProlog);
            builder.Append(queryText);
            builder.Append(_ofTypeInfix);
            if (newType.NamespaceName != string.Empty)
            {
                builder.Append(newType.NamespaceName);
                builder.Append(_ofTypeInfix2);
            }
            builder.Append(newType.Name);
            builder.Append(_ofTypeEpilog);

            // Create a new EntitySqlQueryImplementation that uses the new query as its command text.
            // Span is carried over, no adjustment is needed
            return NewBuilderQuery(query, clrOfType, builder, query.Span, ObjectParameterCollection.DeepCopy(query.Parameters));
        }

        #endregion

        #region OrderBy

        private const string _orderByOp = 
@"
ORDER BY
";

        internal static ObjectQueryState OrderBy(ObjectQueryState query, string alias, string keys, ObjectParameter[] parameters)
        {
            return BuildOrderByOrWhere(query, alias, keys, parameters, _orderByOp, null, true);
        }

        #endregion

        #region Select

        private const string _selectOp = "SELECT ";

        internal static ObjectQueryState Select(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters)
        {
            return BuildSelectOrSelectValue(query, alias, projection, parameters, _selectOp, typeof(DbDataRecord));
        }

        #endregion

        #region SelectValue

        private const string _selectValueOp = "SELECT VALUE ";

        internal static ObjectQueryState SelectValue(ObjectQueryState query, string alias, string projection, ObjectParameter[] parameters, Type projectedType)
        {
            return BuildSelectOrSelectValue(query, alias, projection, parameters, _selectValueOp, projectedType);
        }

        #endregion

        #region Skip

        private const string _skipOp = 
@"
SKIP
";

        internal static ObjectQueryState Skip(ObjectQueryState query, string alias, string keys, string count, ObjectParameter[] parameters)
        {
            Debug.Assert(!StringUtil.IsNullOrEmptyOrWhiteSpace(count), "Invalid skip count");
            return BuildOrderByOrWhere(query, alias, keys, parameters, _orderByOp, count, true);
        }

        #endregion

        #region Top

        private const string _limitOp = 
@"
LIMIT
";
        private const string _topOp = 
@"SELECT VALUE TOP(
";

        private const string _topInfix = 
@"
) ";

        internal static ObjectQueryState Top(ObjectQueryState query, string alias, string count, ObjectParameter[] parameters)
        {
            int queryLength = count.Length;
            string queryText = GetCommandText(query);
            bool limitAllowed = ((EntitySqlQueryState)query).AllowsLimitSubclause;

            if (limitAllowed)
            {
                // Build the new query string:
                // <this query> LIMIT <count>
                queryLength += (queryText.Length +
                                _limitOp.Length
                    // + count.Length is added above
                                );
            }
            else
            {
                // Build the new query string:
                // "SELECT VALUE TOP(<count>) <alias> FROM (<this query>) AS <alias>"
                queryLength += (_topOp.Length +
                    // count.Length + is added above
                               _topInfix.Length +
                               alias.Length +
                               _fromOp.Length +
                               queryText.Length +
                               _asOp.Length +
                               alias.Length);
            }

            StringBuilder builder = new StringBuilder(queryLength);
            if (limitAllowed)
            {
                builder.Append(queryText);
                builder.Append(_limitOp);
                builder.Append(count);
            }
            else
            {
                builder.Append(_topOp);
                builder.Append(count);
                builder.Append(_topInfix);
                builder.Append(alias);
                builder.Append(_fromOp);
                builder.Append(queryText);
                builder.Append(_asOp);
                builder.Append(alias);
            }

            // Create a new EntitySqlQueryImplementation that uses the new query as its command text.
            // Span is carried over, no adjustment is needed.
            return NewBuilderQuery(query, query.ElementType, builder, query.Span, MergeParameters(query.ObjectContext, query.Parameters, parameters));
        }

        #endregion

        #region Union

        private const string _unionOp = 
@"
) UNION (
";

        internal static ObjectQueryState Union(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
        {
            // Ensure the Spans of the query arguments are merged into the new query's Span.
            Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
            // Call the SetOp helper.
            return BuildSetOp(leftQuery, rightQuery, newSpan, _unionOp);
        }

        #endregion

        #region Union

        private const string _unionAllOp = 
@"
) UNION ALL (
";

        internal static ObjectQueryState UnionAll(ObjectQueryState leftQuery, ObjectQueryState rightQuery)
        {
            // Ensure the Spans of the query arguments are merged into the new query's Span.
            Span newSpan = Span.CopyUnion(leftQuery.Span, rightQuery.Span);
            // Call the SetOp helper.
            return BuildSetOp(leftQuery, rightQuery, newSpan, _unionAllOp);
        }

        #endregion

        #region Where

        private const string _whereOp = 
@"
WHERE
";

        internal static ObjectQueryState Where(ObjectQueryState query, string alias, string predicate, ObjectParameter[] parameters)
        {
            return BuildOrderByOrWhere(query, alias, predicate, parameters, _whereOp, null, false);
        }
        #endregion
    }
}
