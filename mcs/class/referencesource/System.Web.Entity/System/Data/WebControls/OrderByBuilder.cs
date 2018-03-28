//---------------------------------------------------------------------
// <copyright file="OrderByBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.Web.UI.WebControls
{
    internal class OrderByBuilder
    {
        private readonly string _argsSortExpression;
        private readonly EntityDataSourceWrapperCollection _wrapperCollection;
        private readonly string _orderBy;
        private readonly bool _autoGenerateOrderByClause;
        private readonly ParameterCollection _orderByParameters;
        private readonly EntityDataSource _owner;
        private readonly bool _generateDefaultOrderByClause;

        internal OrderByBuilder(string argsSortExpression,
            EntityDataSourceWrapperCollection wrapperCollection,
            string orderBy,
            bool autoGenerateOrderByClause,
            ParameterCollection orderByParameters,
            bool generateDefaultOrderByClause,
            EntityDataSource owner)
        {
            _argsSortExpression = argsSortExpression;
            _wrapperCollection = wrapperCollection;
            _orderBy = orderBy;
            _autoGenerateOrderByClause = autoGenerateOrderByClause;
            _orderByParameters = orderByParameters;
            _owner = owner;
            _generateDefaultOrderByClause = generateDefaultOrderByClause;
        }

        internal void Generate(TypeUsage tu, out string orderBy, out ObjectParameter[] orderByParameters, bool applySortExpression)
        {
            Debug.Assert(null != tu, "Type Usage cannot be null");
            GenerateOrderByClause(tu, out orderBy, out orderByParameters, applySortExpression);
        }

        private void GenerateOrderByClause(TypeUsage tu, out string orderByClause, out ObjectParameter[] orderByObjectParameters, bool applySortExpression)
        {
            var orderByClauseBuilder = new StringBuilder();

            if (applySortExpression)
            {
                // This sets the orderBy clause based on a clicked column header in the databound control.
                AppendOrderByKey(orderByClauseBuilder, _argsSortExpression, Strings.EntityDataSourceView_ColumnHeader, tu);
            }

            // AutoGenerateOrderByClause is mutually exclusive with OrderBy.
            // Only one of the following two if statements will execute.
            if (_autoGenerateOrderByClause)
            {
                Debug.Assert(String.IsNullOrEmpty(_orderBy), "If AutoGenerateOrderByClause is true, then OrderBy cannot be set. This should have been caught by a runtime error check");
                IOrderedDictionary paramValues = _orderByParameters.GetValues(_owner.HttpContext, _owner);
                foreach (DictionaryEntry de in paramValues)
                {
                    // Skip AutoGenerateOrderBy on expressions that have a null value.
                    if (!string.IsNullOrEmpty((string)(de.Value)))
                    {
                        if (0 < orderByClauseBuilder.Length)
                        {
                            orderByClauseBuilder.Append(", ");
                        }
                        AppendOrderByKey(orderByClauseBuilder, (string)(de.Value), Strings.EntityDataSourceView_AutoGenerateOrderByParameters, tu);
                    }
                }
            }

            // Append the OrderBy expression, if it's nonzero length.
            if (!String.IsNullOrEmpty(_orderBy))
            {
                orderByObjectParameters = _owner.GetOrderByParameters();
                Debug.Assert(!_autoGenerateOrderByClause, "If OrderBy is set, AutoGenerateOrderBy must be false. This should have been caught by a runtime error check");
                if (0 < orderByClauseBuilder.Length)
                {
                    orderByClauseBuilder.Append(", ");
                }
                orderByClauseBuilder.Append(_orderBy);
            }
            else
            {
                orderByObjectParameters = new ObjectParameter[] { };
            }

            if (orderByClauseBuilder.Length==0 && _generateDefaultOrderByClause)
            {
                // This only occurs if there's no EntitySet, which means entities are not wrapped.
                orderByClauseBuilder.Append(GenerateDefaultOrderByFromTypeUsage(tu));
            }

            orderByClause = orderByClauseBuilder.ToString();
        }

        private void AppendOrderByKey(StringBuilder orderByClauseBuilder, string expression, string errorText, TypeUsage tu)
        {
            if (!String.IsNullOrEmpty(expression))
            {
                string[] statements = expression.Split(',');

                string spacer = String.Empty;
                foreach (string statement in statements)
                {
                    bool isAscending = true;
                    string columnName = ParseStatement(statement.Trim(), out isAscending);

                    if (String.IsNullOrEmpty(columnName))
                    {
                        throw new ArgumentException(Strings.EntityDataSourceView_EmptyPropertyName);
                    }

                    if (EntityDataSourceUtil.PropertyIsOnEntity(columnName, _wrapperCollection, null, tu))
                    {
                        orderByClauseBuilder.Append(spacer);
                        orderByClauseBuilder.Append(EntityDataSourceUtil.GetEntitySqlValueForColumnName(columnName, _wrapperCollection));
                    }
                    else // pass the sort expression through verbatim.
                    {
                        if (!columnName.StartsWith("it.", StringComparison.OrdinalIgnoreCase))
                        {
                            columnName = "it." + columnName;
                        }
                        orderByClauseBuilder.Append(spacer + columnName);
                    }

                    if (!isAscending)
                    {
                        orderByClauseBuilder.Append(c_esqlDescendingTail);
                    }

                    spacer = ",";
                }
            }
        }

        private const string c_esqlAscendingTail = " ASC";
        private const string c_esqlDescendingTail = " DESC";
        private static readonly string[] ascendingTails = new string[] { c_esqlAscendingTail, " ascending" };
        private static readonly string[] descendingTails = new string[] { c_esqlDescendingTail, " descending" };

        private static string ParseStatement(string statement, out bool isAscending)
        {
            foreach (string tail in descendingTails)
            {
                if (statement.EndsWith(tail, StringComparison.OrdinalIgnoreCase))
                {
                    isAscending = false;
                    return statement.Substring(0, statement.Length - tail.Length);
                }
            }

            foreach (string tail in ascendingTails)
            {
                if (statement.EndsWith(tail, StringComparison.OrdinalIgnoreCase))
                {
                    isAscending = true;
                    return statement.Substring(0, statement.Length - tail.Length);
                }
            }

            isAscending = true;
            return statement;
        } 

        private static IQueryable ExpandQueryableOrderBy(IQueryable source, string[] statements)
        {        
            var expression = source.Expression;
            var parameter = Expression.Parameter(source.ElementType, String.Empty);

            for (int idx = 0; idx < statements.Length; idx++)
            {
                bool isAscending = true;
                // Try LINQ ascending/descending suffix
                string memberReference = ParseStatement(statements[idx], out isAscending);
                bool isFirstOrderBy = (idx == 0);

                var methodName = (isFirstOrderBy ? "OrderBy" : "ThenBy") + (isAscending ? String.Empty : "Descending");

                // Unravel nested property accesses
                var memberElements = memberReference.Split('.');
                Expression memberExpression = parameter;
                foreach (string memberElement in memberElements)
                {
                    if (string.IsNullOrEmpty(memberElement))
                    {
                        throw new ArgumentException(Strings.EntityDataSourceView_EmptyPropertyName);
                    }
                    memberExpression = Expression.Property(memberExpression, memberElement.Trim());
                }

                expression = Expression.Call(typeof(Queryable), methodName, new Type[] { source.ElementType, memberExpression.Type },
                                new Expression[] { expression, Expression.Quote(DynamicExpression.Lambda(memberExpression, parameter)) });
            }

            return source.Provider.CreateQuery(expression);
        }

        internal IQueryable<TEntity> BuildQueryableOrderBy<TEntity>(IQueryable<TEntity> source)
        {
            IQueryable query = source;

            // Process control's sort arguments if there are any
            if (_argsSortExpression != null & _argsSortExpression.Trim().Length > 0)
            {
                string[] statements = _argsSortExpression.Split(',');
                if (statements.Length > 0)
                {
                    query = ExpandQueryableOrderBy(query, statements);
                }
            }

            return query as IQueryable<TEntity>;
        }

        private static string GenerateDefaultOrderByFromTypeUsage(TypeUsage tu)
        {
            StringBuilder orderByBuilder = new StringBuilder();
            ReadOnlyMetadataCollection<EdmProperty> propertyCollection;
            List<string> keyMemberNames = null;
            EntityType entityType = tu.EdmType as EntityType;

            if (null != entityType)
            {
                ReadOnlyMetadataCollection<EdmMember> keyMembers;
                keyMembers = entityType.KeyMembers;
                keyMemberNames = new List<string>(entityType.KeyMembers.Count);
                propertyCollection = entityType.Properties;

                foreach (EdmMember edmMember in keyMembers)
                {
                    keyMemberNames.Add(edmMember.Name);
                }
            }
            else
            {
                return String.Empty;
            }

            foreach (EdmProperty property in propertyCollection)
            {
                if (keyMemberNames.Contains(property.Name) && EntityDataSourceUtil.IsScalar(property.TypeUsage.EdmType))
                {
                    if (0 < orderByBuilder.Length)
                    {
                        orderByBuilder.Append(", ");
                    }
                    orderByBuilder.Append(EntityDataSourceUtil.EntitySqlElementAlias);
                    orderByBuilder.Append(".");
                    orderByBuilder.Append(EntityDataSourceUtil.QuoteEntitySqlIdentifier(property.Name));
                }
            }
            return orderByBuilder.ToString();
        }
    }
}
