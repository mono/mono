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
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

using DbLinq.Logging;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    partial class QueryBuilder
    {
        protected class UpsertParameters
        {
            public MetaTable Table;
            public readonly IList<ObjectInputParameterExpression> InputParameters = new List<ObjectInputParameterExpression>();
            public readonly IList<ObjectOutputParameterExpression> OutputParameters = new List<ObjectOutputParameterExpression>();
            public readonly IList<SqlStatement> InputColumns = new List<SqlStatement>();
            public readonly IList<SqlStatement> InputValues = new List<SqlStatement>();
            public readonly IList<SqlStatement> OutputValues = new List<SqlStatement>();
            public readonly IList<SqlStatement> OutputExpressions = new List<SqlStatement>();
            public readonly IList<SqlStatement> InputPKColumns = new List<SqlStatement>();
            public readonly IList<SqlStatement> InputPKValues = new List<SqlStatement>();
        }

        // SQLite:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null
        // INSERT INTO main.Products (CategoryID, Discontinued, ProductName, QuantityPerUnit) 
        //                  VALUES (@P1, @P2, @P3, @P4) ;SELECT last_insert_rowid()
        //
        // Ingres:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, 
        //                       Expression = "next value for \"linquser\".\"products_seq\"")]
        // INSERT INTO linquser.products (categoryid, discontinued, productid, productname, quantityperunit) 
        //                  VALUES ($param_000001_param$, $param_000002_param$, 
        //                          next value for "linquser"."products_seq", $param_000004_param$, $param_000005_param$) 
        //
        // Oracle:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = null
        // BEGIN 
        // INSERT INTO NORTHWIND."Products" ("CategoryID", "Discontinued", "ProductID", "ProductName", "QuantityPerUnit") 
        //                  VALUES (:P1, :P2, NORTHWIND."Products_SEQ".NextVal, :P4, :P5)
        //               ;SELECT NORTHWIND."Products_SEQ".CurrVal INTO :P3 FROM DUAL; END;
        //
        // PostgreSQL:
        // IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false, Expression = "nextval('\"Products_ProductID_seq\"')"
        // INSERT INTO public."Products" ("CategoryID", "Discontinued", "ProductName", "QuantityPerUnit") 
        //                  VALUES (:P1, :P2, :P3, :P4) 
        //               ;SELECT currval('"Products_ProductID_seq"')
        //
        // SQL Server (bogus):
        // IsPrimaryKey = true, IsDbGenerated = true
        // INSERT INTO [dbo].[Products] (, , , ) VALUES (@P1, @P2, @P3, @P4) 
        //                  ; SELECT @@IDENTITY
        //
        // Column:               default --> use value
        //          PK: Expression !null --> use parameter (Oracle is wrong here)
        //              Expression  null --> ignore
        // SQL: wrap clause with PK information


        /// <summary>
        /// Creates a query for insertion
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public UpsertQuery GetInsertQuery(object objectToInsert, QueryContext queryContext)
        {
            // TODO: cache
            var upsertParameters = GetUpsertParameters(objectToInsert, false, null, queryContext);
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var insertSql = sqlProvider.GetInsert(
                sqlProvider.GetTable(upsertParameters.Table.TableName),
                upsertParameters.InputColumns,
                upsertParameters.InputValues);
            var insertIdSql = sqlProvider.GetInsertIds(
                upsertParameters.OutputValues,
                upsertParameters.OutputExpressions);
            queryContext.DataContext.Logger.Write(Level.Debug, "Insert SQL: {0}", insertSql);
            queryContext.DataContext.WriteLog(insertSql.ToString());
            return new UpsertQuery(queryContext.DataContext, insertSql, insertIdSql, upsertParameters.InputParameters, upsertParameters.OutputParameters);
        }

        protected enum ParameterType
        {
            Input,
            InputPK,
            Output
        }

        /// <summary>
        /// Gets values for insert/update
        /// </summary>
        /// <param name="objectToUpsert"></param>
        /// <param name="queryContext"></param>
        /// <param name="update"></param>
        /// <param name="modifiedMembers"></param>
        /// <returns></returns>
        protected virtual UpsertParameters GetUpsertParameters(object objectToUpsert, bool update, IList<MemberInfo> modifiedMembers, QueryContext queryContext)
        {
            var rowType = objectToUpsert.GetType();
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var upsertParameters = new UpsertParameters
                                       {
                                           Table = queryContext.DataContext.Mapping.GetTable(rowType)
                                       };
            foreach (var dataMember in upsertParameters.Table.RowType.PersistentDataMembers)
            {
                var column = sqlProvider.GetColumn(dataMember.MappedName);
                ParameterType type = GetParameterType(objectToUpsert, dataMember, update);
                var memberInfo = dataMember.Member;
                // if the column is generated AND not specified, we may have:
                // - an explicit generation (Expression property is not null, so we add the column)
                // - an implicit generation (Expression property is null
                // in all cases, we want to get the value back
                if (type == ParameterType.Output)
                {
                    if (dataMember.Expression != null)
                    {
                        upsertParameters.InputColumns.Add(column);
                        upsertParameters.InputValues.Add(dataMember.Expression);
                    }
                    var setter = (Expression<Action<object, object>>)((o, v) => memberInfo.SetMemberValue(o, v));
                    var outputParameter = new ObjectOutputParameterExpression(setter,
                                                                              memberInfo.GetMemberType(),
                                                                              dataMember.Name);
                    upsertParameters.OutputParameters.Add(outputParameter);
                    upsertParameters.OutputValues.Add(sqlProvider.GetParameterName(outputParameter.Alias));
                    upsertParameters.OutputExpressions.Add(dataMember.Expression);
                }
                else // standard column
                {
                    var getter = (Expression<Func<object, object>>)(o => memberInfo.GetMemberValue(o));
                    var inputParameter = new ObjectInputParameterExpression(
                        getter,
                        memberInfo.GetMemberType(), dataMember.Name);
                    if (type == ParameterType.InputPK)
                    {
                        upsertParameters.InputPKColumns.Add(column);
                        upsertParameters.InputPKValues.Add(sqlProvider.GetParameterName(inputParameter.Alias));
                        upsertParameters.InputParameters.Add(inputParameter);
                    }
                    // for a standard column, we keep it only if modifiedMembers contains the specified memberInfo
                    // caution: this makes the cache harder to maintain
                    else if (modifiedMembers == null || modifiedMembers.Contains(memberInfo))
                    {
                        upsertParameters.InputColumns.Add(column);
                        upsertParameters.InputValues.Add(sqlProvider.GetParameterName(inputParameter.Alias));
                        upsertParameters.InputParameters.Add(inputParameter);
                    }
                }
            }
            return upsertParameters;
        }

        /// <summary>
        /// Provides the parameter type for a given data member
        /// </summary>
        /// <param name="objectToUpsert"></param>
        /// <param name="dataMember"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        protected virtual ParameterType GetParameterType(object objectToUpsert, MetaDataMember dataMember, bool update)
        {
            var memberInfo = dataMember.Member;
            // the deal with columns is:
            // PK only:  explicit for INSERT, criterion for UPDATE
            // PK+GEN:   implicit/explicit for INSERT, criterion for UPDATE
            // GEN only: implicit for both
            // -:        explicit for both
            //
            // explicit is input,
            // implicit is output, 
            // criterion is input PK
            ParameterType type;
            if (dataMember.IsPrimaryKey)
            {
                if (update)
                    type = ParameterType.InputPK;
                else
                {
                    if (dataMember.IsDbGenerated)
                    {
                        if (IsSpecified(objectToUpsert, memberInfo))
                            type = ParameterType.Input;
                        else
                            type = ParameterType.Output;
                    }
                    else
                        type = ParameterType.Input;
                }
            }
            else
            {
                if (dataMember.IsDbGenerated)
                    type = ParameterType.Output;
                else
                    type = ParameterType.Input;
            }
            return type;
        }

        /// <summary>
        /// Determines if a property is different from its default value
        /// </summary>
        /// <param name="target"></param>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        protected virtual bool IsSpecified(object target, MemberInfo memberInfo)
        {
            object value = memberInfo.GetMemberValue(target);
            if (value == null)
                return false;
            if (Equals(value, TypeConvert.GetDefault(memberInfo.GetMemberType())))
                return false;
            return true;
        }

        /// <summary>
        /// Creates or gets an UPDATE query
        /// </summary>
        /// <param name="objectToUpdate"></param>
        /// <param name="modifiedMembers">List of modified members, or NULL</param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public UpsertQuery GetUpdateQuery(object objectToUpdate, IList<MemberInfo> modifiedMembers, QueryContext queryContext)
        {
            var upsertParameters = GetUpsertParameters(objectToUpdate, true, modifiedMembers, queryContext);
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var updateSql = sqlProvider.GetUpdate(sqlProvider.GetTable(upsertParameters.Table.TableName),
                upsertParameters.InputColumns, upsertParameters.InputValues,
                upsertParameters.OutputValues, upsertParameters.OutputExpressions,
                upsertParameters.InputPKColumns, upsertParameters.InputPKValues
                );
            queryContext.DataContext.Logger.Write(Level.Debug, "Update SQL: {0}", updateSql);
            queryContext.DataContext.WriteLog(updateSql.ToString());
            return new UpsertQuery(queryContext.DataContext, updateSql, "", upsertParameters.InputParameters, upsertParameters.OutputParameters);
        }

        /// <summary>
        /// Creates or gets a DELETE query
        /// </summary>
        /// <param name="objectToDelete"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        public DeleteQuery GetDeleteQuery(object objectToDelete, QueryContext queryContext)
        {
            var sqlProvider = queryContext.DataContext.Vendor.SqlProvider;
            var rowType = objectToDelete.GetType();
            var table = queryContext.DataContext.Mapping.GetTable(rowType);
            var deleteParameters = new List<ObjectInputParameterExpression>();
            var pkColumns = new List<SqlStatement>();
            var pkValues = new List<SqlStatement>();
            foreach (var pkMember in table.RowType.IdentityMembers)
            {
                var memberInfo = pkMember.Member;
                var getter = (Expression<Func<object, object>>)(o => memberInfo.GetMemberValue(o));
                var inputParameter = new ObjectInputParameterExpression(
                    getter,
                    memberInfo.GetMemberType(), pkMember.Name);
                var column = sqlProvider.GetColumn(pkMember.MappedName);
                pkColumns.Add(column);
                pkValues.Add(sqlProvider.GetParameterName(inputParameter.Alias));
                deleteParameters.Add(inputParameter);
            }
            var deleteSql = sqlProvider.GetDelete(sqlProvider.GetTable(table.TableName), pkColumns, pkValues);
            queryContext.DataContext.Logger.Write(Level.Debug, "Delete SQL: {0}", deleteSql);
            queryContext.DataContext.WriteLog(deleteSql.ToString());
            return new DeleteQuery(queryContext.DataContext, deleteSql, deleteParameters);
        }
    }
}
