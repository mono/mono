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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sql;
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

using DbLinq.Data.Linq.Database;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class QueryRunner : IQueryRunner
    {
        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Select<T>(SelectQuery selectQuery)
        {
            var rowObjectCreator = selectQuery.GetRowObjectCreator<T>();

            IList<T> results = new List<T>();

            // handle the special case where the query is empty, meaning we don't need the DB
            if (string.IsNullOrEmpty(selectQuery.Sql.ToString()))
            {
                results.Add(rowObjectCreator(null, null));
            }
            else
            {
                using (var dbCommand = selectQuery.GetCommand())
                {
                    // write query to log
                    selectQuery.DataContext.WriteLog(dbCommand.Command);

                    using (var reader = dbCommand.Command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // someone told me one day this could happen (in SQLite)
                            if (reader.FieldCount == 0)
                                continue;

                            var row = rowObjectCreator(reader, selectQuery.DataContext._MappingContext);
                            // the conditions to register and watch an entity are:
                            // - not null (can this happen?)
                            // - registered in the model
                            if (row != null && selectQuery.DataContext.Mapping.GetTable(row.GetType()) != null)
                            {
                                row = (T)selectQuery.DataContext.Register(row);
                            }
                            results.Add(row);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Returns a unique row (common reference)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="t"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual object GetUniqueRow(object row, Type t, DataContext dataContext)
        {
            if (row != null && dataContext.Mapping.GetTable(row.GetType()) != null)
                row = dataContext.Register(row);
            return row;
        }

        /// <summary>
        /// Returns a unique row (common reference)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual T GetUniqueRow<T>(object row, DataContext dataContext)
        {
            return (T)GetUniqueRow(row, typeof(T), dataContext);
        }

        public virtual S SelectScalar<S>(SelectQuery selectQuery)
        {
            switch (selectQuery.ExecuteMethodName)
            {
                case null: // some calls, like Count() generate SQL and the resulting projection method name is null (never initialized)
                    return SelectSingle<S>(selectQuery, false); // Single() for safety, but First() should work
                case "First":
                    return SelectFirst<S>(selectQuery, false);
                case "FirstOrDefault":
                    return SelectFirst<S>(selectQuery, true);
                case "Single":
                    return SelectSingle<S>(selectQuery, false);
                case "SingleOrDefault":
                    return SelectSingle<S>(selectQuery, true);
                case "Last":
                    return SelectLast<S>(selectQuery, false);
            }
            throw Error.BadArgument("S0077: Unhandled method '{0}'", selectQuery.ExecuteMethodName);
        }

        /// <summary>
        /// Returns first item in query.
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectFirst<S>(SelectQuery selectQuery, bool allowDefault)
        {
            foreach (var row in Select<S>(selectQuery))
                return row;
            if (!allowDefault)
                throw new InvalidOperationException();
            return default(S);
        }

        /// <summary>
        /// Returns single item in query
        /// If more than one item is found, throws an exception
        /// If no row is found then if default allowed returns default(S), throws exception otherwise
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectSingle<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S firstRow = default(S);
            int rowCount = 0;
            foreach (var row in Select<S>(selectQuery))
            {
                if (rowCount > 1)
                    throw new InvalidOperationException();
                firstRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return firstRow;
        }

        /// <summary>
        /// Returns last item in query
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <param name="allowDefault"></param>
        /// <returns></returns>
        protected virtual S SelectLast<S>(SelectQuery selectQuery, bool allowDefault)
        {
            S lastRow = default(S);
            int rowCount = 0;
            foreach (var row in Select<S>(selectQuery))
            {
                lastRow = row;
                rowCount++;
            }
            if (!allowDefault && rowCount == 0)
                throw new InvalidOperationException();
            return lastRow;
        }

        /// <summary>
        /// Runs an InsertQuery on a provided object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="insertQuery"></param>
        public void Insert(object target, UpsertQuery insertQuery)
        {
            Upsert(target, insertQuery);
        }

        private void Upsert(object target, UpsertQuery insertQuery)
        {
            insertQuery.Target = target;
            var dataContext = insertQuery.DataContext;
            using (var dbCommand = insertQuery.GetCommand())
            {

                // log first command
                dataContext.WriteLog(dbCommand.Command);

                // we may have two commands
                int rowsCount = dbCommand.Command.ExecuteNonQuery();
                // the second reads output parameters
                if (!string.IsNullOrEmpty(insertQuery.IdQuerySql.ToString()))
                {
                    var outputCommand = dbCommand.Command.Connection.CreateCommand();

                    // then run commands
                    outputCommand.Transaction = dbCommand.Command.Transaction;
                    outputCommand.CommandText = insertQuery.IdQuerySql.ToString();

                    // log second command
                    dataContext.WriteLog(outputCommand);

                    using (var dataReader = outputCommand.ExecuteReader())
                    {
                        // TODO: check if this is needed
                        dataReader.Read();

                        for (int outputParameterIndex = 0;
                             outputParameterIndex < insertQuery.OutputParameters.Count;
                             outputParameterIndex++)
                        {
                            var outputParameter = insertQuery.OutputParameters[outputParameterIndex];
                            var outputDbParameter = dataReader.GetValue(outputParameterIndex);
                            SetOutputParameterValue(target, outputParameter, outputDbParameter);
                        }
                    }
                }
                dbCommand.Commit();
            }
        }

        protected virtual void SetOutputParameterValue(object target, ObjectOutputParameterExpression outputParameter, object value)
        {
            // depending on vendor, we can have DBNull or null
            // so we handle both
            if (value is DBNull || value == null)
                outputParameter.SetValue(target, null);
            else
                outputParameter.SetValue(target, TypeConvert.To(value, outputParameter.ValueType));
        }

        /// <summary>
        /// Performs an update
        /// </summary>
        /// <param name="target">Entity to be flushed</param>
        /// <param name="updateQuery">SQL update query</param>
        /// <param name="modifiedMembers">List of modified members, or null to update all members</param>
        public void Update(object target, UpsertQuery updateQuery, IList<MemberInfo> modifiedMembers)
        {
            Upsert(target, updateQuery);
        }

        /// <summary>
        /// Performs a delete
        /// </summary>
        /// <param name="target">Entity to be deleted</param>
        /// <param name="deleteQuery">SQL delete query</param>
        public void Delete(object target, DeleteQuery deleteQuery)
        {
            deleteQuery.Target = target;
            using (var dbCommand = deleteQuery.GetCommand())
            {

                // log command
                deleteQuery.DataContext.WriteLog(dbCommand.Command);

                int rowsCount = dbCommand.Command.ExecuteNonQuery();
                dbCommand.Commit();
            }
        }

        /// <summary>
        /// Fills dbCommand parameters, given names and values
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="parameterNames"></param>
        /// <param name="parameterValues"></param>
        private void FeedParameters(IDbCommand dbCommand, IList<string> parameterNames, IList<object> parameterValues)
        {
            for (int parameterIndex = 0; parameterIndex < parameterNames.Count; parameterIndex++)
            {
                var dbParameter = dbCommand.CreateParameter();
                dbParameter.ParameterName = parameterNames[parameterIndex];
                dbParameter.SetValue(parameterValues[parameterIndex]);
                dbCommand.Parameters.Add(dbParameter);
            }
        }

        /// <summary>
        /// Runs a direct scalar command
        /// </summary>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Execute(DirectQuery directQuery, params object[] parameters)
        {
            directQuery.parameterValues = parameters;
            using (var dbCommand = directQuery.GetCommand())
            {

                // log command
                directQuery.DataContext.WriteLog(dbCommand.Command);

                var result = dbCommand.Command.ExecuteScalar();
                if (result == null || result is DBNull)
                    return 0;
                var intResult = TypeConvert.ToNumber<int>(result);
                return intResult;
            }
        }

        // TODO: move method?
        protected virtual Delegate GetTableBuilder(Type elementType, IDataReader dataReader, DataContext dataContext)
        {
            var fields = new List<string>();
            for (int fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                fields.Add(dataReader.GetName(fieldIndex));
            return dataContext.QueryBuilder.GetTableReader(elementType, fields, new QueryContext(dataContext));
        }

        /// <summary>
        /// Runs a query with a direct statement
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable ExecuteSelect(Type tableType, DirectQuery directQuery, params object[] parameters)
        {
            directQuery.parameterValues = parameters;
            using (var dbCommand = directQuery.GetCommand())
            {

                // log query
                directQuery.DataContext.WriteLog(dbCommand.Command);

                using (var dataReader = dbCommand.Command.ExecuteReader())
                {
                    // Did you know? "return EnumerateResult(tableType, dataReader, dataContext);" disposes resources first
                    // before the enumerator is used
                    foreach (var result in EnumerateResult(tableType, dataReader, directQuery.DataContext))
                        yield return result;
                }
            }
        }

        /// <summary>
        /// Enumerates results from a request.
        /// The result shape can change dynamically
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataReader"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public IEnumerable EnumerateResult(Type tableType, IDataReader dataReader, DataContext dataContext)
        {
            return EnumerateResult(tableType, true, dataReader, dataContext);
        }

        /// <summary>
        /// Enumerates results from a request.
        /// The result shape can change dynamically
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dynamicallyReadShape">Set True to change reader shape dynamically</param>
        /// <param name="dataReader"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        protected virtual IEnumerable EnumerateResult(Type tableType, bool dynamicallyReadShape, IDataReader dataReader, DataContext dataContext)
        {
            Delegate tableBuilder = null;
            while (dataReader.Read())
            {
                if (tableBuilder == null || dynamicallyReadShape)
                    tableBuilder = GetTableBuilder(tableType, dataReader, dataContext);
                var row = tableBuilder.DynamicInvoke(dataReader, dataContext._MappingContext);
                row = GetUniqueRow(row, tableType, dataContext);
                yield return row;
            }
        }
    }
}
