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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using DbLinq.Data.Linq.Sugar;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace DbLinq.Data.Linq.Sugar
{
    internal interface IQueryRunner
    {
        /// <summary>
        /// Enumerates all records return by SQL request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        IEnumerable<T> Select<T>(SelectQuery selectQuery);

        /// <summary>
        /// Returns at most one result
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        S SelectScalar<S>(SelectQuery selectQuery);

        /// <summary>
        /// Runs an InsertQuery on a provided object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="insertQuery"></param>
        void Insert(object target, UpsertQuery insertQuery);

        /// <summary>
        /// Performans an update
        /// </summary>
        /// <param name="target">Entity to be flushed</param>
        /// <param name="updateQuery">SQL update query</param>
        /// <param name="modifiedMembers">List of modified members, or null to update all members</param>
        void Update(object target, UpsertQuery updateQuery,IList<MemberInfo> modifiedMembers);

        /// <summary>
        /// Performs a delete
        /// </summary>
        /// <param name="target">Entity to be deleted</param>
        /// <param name="deleteQuery">SQL delete query</param>
        void Delete(object target, DeleteQuery deleteQuery);

        /// <summary>
        /// Runs a direct scalar command
        /// </summary>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int Execute(DirectQuery directQuery, params object[] parameters);

        /// <summary>
        /// Runs a query with a direct statement
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="directQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable ExecuteSelect(Type tableType, DirectQuery directQuery, params object[] parameters);

        /// <summary>
        /// Enumerates results from a request.
        /// The result shape can change dynamically
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataReader"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        IEnumerable EnumerateResult(Type tableType, IDataReader dataReader, DataContext dataContext);
    }
}