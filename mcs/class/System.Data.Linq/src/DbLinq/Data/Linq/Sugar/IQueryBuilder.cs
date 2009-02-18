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
using System.Reflection;

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal interface IQueryBuilder
    {
        /// <summary>
        /// Creates a query for selection
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        SelectQuery GetSelectQuery(ExpressionChain expressions, QueryContext queryContext);

        /// <summary>
        /// Creates a query for insertion
        /// </summary>
        /// <param name="objectToInsert"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        UpsertQuery GetInsertQuery(object objectToInsert, QueryContext queryContext);

        /// <summary>
        /// Creates or gets an UPDATE query
        /// </summary>
        /// <param name="objectToUpdate"></param>
        /// <param name="modifiedMembers">List of modified members, or NULL</param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        UpsertQuery GetUpdateQuery(object objectToUpdate, IList<MemberInfo> modifiedMembers, QueryContext queryContext);

        /// <summary>
        /// Creates or gets a DELETE query
        /// </summary>
        /// <param name="objectToDelete"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        DeleteQuery GetDeleteQuery(object objectToDelete, QueryContext queryContext);


        /// <summary>
        /// Converts a direct SQL query to a safe query with named parameters
        /// </summary>
        /// <param name="sql">Raw SQL query</param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        DirectQuery GetDirectQuery(string sql, QueryContext queryContext);

        /// <summary>
        /// Returns a Delegate to create a row for a given IDataRecord
        /// The Delegate is Func&lt;IDataRecord,MappingContext,"tableType">
        /// </summary>
        /// <param name="tableType">The table type (must be managed by DataContext)</param>
        /// <param name="parameters"></param>
        /// <param name="queryContext"></param>
        /// <returns></returns>
        Delegate GetTableReader(Type tableType, IList<string> parameters, QueryContext queryContext);
    }
}
