//---------------------------------------------------------------------
// <copyright file="EntityDataSourceQueryBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Web.UI.WebControls
{
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal abstract class EntityDataSourceQueryBuilder<T>
    {
        private readonly DataSourceSelectArguments _arguments;
        private readonly string _commandText;
        private readonly ObjectParameter[] _commandParameters;
        private readonly string _whereExpression;
        private readonly ObjectParameter[] _whereParameters;
        private readonly string _entitySetQueryExpression;
        private readonly OrderByBuilder _orderByBuilder;
        private string _includePaths;
        private TypeUsage _resultType;
        private Nullable<int> _count;

        protected EntityDataSourceQueryBuilder(DataSourceSelectArguments arguments,
                                              string commandText, ObjectParameter[] commandParameters,
                                              string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                                              string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                                              OrderByBuilder orderByBuilder,
                                              string includePaths)
        {
            _arguments = arguments;
            _commandText = commandText;
            _commandParameters = commandParameters;
            _whereExpression = whereExpression;
            _whereParameters = whereParameters;
            _entitySetQueryExpression = entitySetQueryExpression;
            _orderByBuilder = orderByBuilder;
            _includePaths = includePaths;
        }

        internal delegate EntityDataSourceQueryBuilder<T> Creator(DataSourceSelectArguments arguments,
                                      string commandText, ObjectParameter[] commandParameters,
                                      string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                                      string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                                      OrderByBuilder orderByBuilder,
                                      string includePaths);

        internal TypeUsage ResultType
        {
            get
            {
                Debug.Assert(_resultType != null, "ResultType is only valid after Build()");
                return _resultType;
            }
        }
        internal int TotalCount
        {
            get
            {
                Debug.Assert(_count.HasValue, "Count is not valid until after Build. And only then if computeCount is true");
                return _count.Value;
            }
        }
        internal IEnumerable Execute(ObjectQuery<T> queryT)
        {
            return (IEnumerable)(((IListSource)(queryT)).GetList());
        }

        internal ObjectQuery<T> BuildBasicQuery(ObjectContext context, bool computeCount)
        {
            ObjectQuery<T> queryT = QueryBuilderUtils.ConstructQuery<T>(context, _entitySetQueryExpression, _commandText, _commandParameters);
            queryT = ApplyWhere(queryT);
            queryT = ApplySelect(queryT); // Select and/or GroupBy application
            _resultType = queryT.GetResultType();
            return queryT;
        }

        internal ObjectQuery<T> CompleteBuild(ObjectQuery<T> queryT, ObjectContext context, bool computeCount, bool wasExtended)
        {
            if (computeCount)
            {
                _count = queryT.Count();
            }

            queryT = wasExtended ? ApplyQueryableOrderByAndPaging(queryT) : ApplyOrderByAndPaging(queryT);
            queryT = ApplyIncludePaths(queryT);

            return queryT;
        }

        private ObjectQuery<T> ApplyWhere(ObjectQuery<T> queryT)
        {
            if (!String.IsNullOrEmpty(_whereExpression))
            {
                queryT = queryT.Where(_whereExpression, _whereParameters);
            }
            return queryT;
        }

        protected abstract ObjectQuery<T> ApplySelect(ObjectQuery<T> queryT);

        internal ObjectQuery<T> ApplyOrderBy(ObjectQuery<T> queryT)
        {
            string orderByClause;
            ObjectParameter[] orderByParameters;
            // Apply all possible ordering except the sort expression, because it might only be valid after the query has been extended
            _orderByBuilder.Generate(_resultType, out orderByClause, out orderByParameters, false /*applySortExpression*/);

            return String.IsNullOrEmpty(orderByClause) ? queryT : queryT.OrderBy(orderByClause, orderByParameters);
        }

        private ObjectQuery<T> ApplyOrderByAndPaging(ObjectQuery<T> queryT)
        {
            // This re-applys the order-by as part of the skip
            string orderByClause;
            ObjectParameter[] orderByParameters;
            _orderByBuilder.Generate(_resultType, out orderByClause, out orderByParameters, true /*applySortExpression*/);
            bool paging = _arguments.MaximumRows > 0 && _arguments.StartRowIndex >= 0;
            var hasOrderByClause = !String.IsNullOrEmpty(orderByClause);

            if (paging)
            {
                if (!hasOrderByClause)
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceQueryBuilder_PagingRequiresOrderBy);
                }
                queryT = queryT.Skip(orderByClause, _arguments.StartRowIndex.ToString(CultureInfo.InvariantCulture), orderByParameters).Top(_arguments.MaximumRows.ToString(CultureInfo.InvariantCulture), QueryBuilderUtils.EmptyObjectParameters);
            }
            else
            {
                if (hasOrderByClause)
                {
                    queryT = queryT.OrderBy(orderByClause, orderByParameters);
                }
            }
            
            return queryT;
        }

        private ObjectQuery<T> ApplyQueryableOrderByAndPaging(ObjectQuery<T> queryT)
        {
            queryT = _orderByBuilder.BuildQueryableOrderBy(queryT) as ObjectQuery<T>;
            bool paging = _arguments.MaximumRows > 0 && _arguments.StartRowIndex >= 0;
            if (paging)
            {
                queryT = queryT.Skip(_arguments.StartRowIndex).Take(_arguments.MaximumRows) as ObjectQuery<T>;
            }

            return queryT;
        }

        private ObjectQuery<T> ApplyIncludePaths(ObjectQuery<T> objectQuery)
        {
            if (!string.IsNullOrEmpty(_includePaths))
            {
                foreach (string include in _includePaths.Split(','))
                {
                    string trimmedInclude = include.Trim();
                    if (!string.IsNullOrEmpty(trimmedInclude))
                    {
                        objectQuery = objectQuery.Include(trimmedInclude);
                    }
                }
            }
            return objectQuery;
        }
    }

    internal class EntityDataSourceObjectQueryBuilder<T> : EntityDataSourceQueryBuilder<T>
    {
        private EntityDataSourceObjectQueryBuilder(DataSourceSelectArguments arguments,
                                                    string commandText, ObjectParameter[] commandParameters,
                                                    string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                                                    string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                                                    OrderByBuilder orderByBuilder,
                                                    string includePaths)
            : base(arguments,
                   commandText, commandParameters,
                   whereExpression, whereParameters, entitySetQueryExpression,
                   selectExpression, groupByExpression, selectParameters,
                   orderByBuilder,
                   includePaths)
        {
        }

        static internal EntityDataSourceQueryBuilder<T>.Creator GetCreator()
        {
            return Create;
        }

        static internal EntityDataSourceQueryBuilder<T> Create(DataSourceSelectArguments arguments,
                              string commandText, ObjectParameter[] commandParameters,
                              string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                              string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                              OrderByBuilder orderByBuilder,
                              string includePaths)
        {
            return new EntityDataSourceObjectQueryBuilder<T>(arguments,
                   commandText, commandParameters,
                   whereExpression, whereParameters, entitySetQueryExpression,
                   selectExpression, groupByExpression, selectParameters,
                   orderByBuilder,
                   includePaths);
        }

        protected override ObjectQuery<T> ApplySelect(ObjectQuery<T> queryT)
        {
            return queryT;
        }
    }


    internal class EntityDataSourceRecordQueryBuilder : EntityDataSourceQueryBuilder<DbDataRecord>
    {
        private readonly string _selectExpression;
        private readonly string _groupByExpression;
        private readonly ObjectParameter[] _selectParameters;

        private EntityDataSourceRecordQueryBuilder(DataSourceSelectArguments arguments,
                                                    string commandText, ObjectParameter[] commandParameters,
                                                    string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                                                    string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                                                    OrderByBuilder orderByBuilder,
                                                    string includePaths)
            : base(arguments,
                   commandText, commandParameters,
                   whereExpression, whereParameters, entitySetQueryExpression,
                   selectExpression, groupByExpression, selectParameters,
                   orderByBuilder,
                   includePaths)
        {
            _selectExpression = selectExpression;
            _groupByExpression = groupByExpression;
            _selectParameters = selectParameters;
        }

        static internal EntityDataSourceQueryBuilder<DbDataRecord> Create(DataSourceSelectArguments arguments,
                      string commandText, ObjectParameter[] commandParameters,
                      string whereExpression, ObjectParameter[] whereParameters, string entitySetQueryExpression,
                      string selectExpression, string groupByExpression, ObjectParameter[] selectParameters,
                      OrderByBuilder orderByBuilder,
                      string includePaths)
        {
            return new EntityDataSourceRecordQueryBuilder(arguments,
                   commandText, commandParameters,
                   whereExpression, whereParameters, entitySetQueryExpression,
                   selectExpression, groupByExpression, selectParameters,
                   orderByBuilder,
                   includePaths);
        }

        protected override ObjectQuery<DbDataRecord> ApplySelect(ObjectQuery<DbDataRecord> queryT)
        {
            Debug.Assert(!String.IsNullOrEmpty(_selectExpression), "Select expression should not be of zero length.");

            if (!string.IsNullOrEmpty(_groupByExpression))
            {
                queryT = queryT.GroupBy(_groupByExpression, _selectExpression, _selectParameters);
            }
            else
            {
                queryT = queryT.Select(_selectExpression, _selectParameters);
            }
            return queryT;
        }
    }

    internal static class QueryBuilderUtils
    {
        internal static readonly ObjectParameter[] EmptyObjectParameters = new ObjectParameter[] { };

        internal static ObjectQuery<T> ConstructQuery<T>(ObjectContext context,
                                                  string entitySetQueryExpression,
                                                  string commandText,
                                                  ObjectParameter[] commandParameters)
        {
            string queryExpression;
            ObjectParameter[] queryParameters;
            if (!string.IsNullOrEmpty(commandText))
            {
                queryExpression = commandText;
                queryParameters = commandParameters;
            }
            else
            {
                queryExpression = entitySetQueryExpression;
                queryParameters = QueryBuilderUtils.EmptyObjectParameters;
            }

            return context.CreateQuery<T>(queryExpression, queryParameters);
        }
    }
}
