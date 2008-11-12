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
using System.Diagnostics;
using DbLinq.Data.Linq.Database;
using DbLinq.Data.Linq.Mapping;
using System.Data.Linq.Mapping;
using System.Data;
using DbLinq.Vendor;
using System.Data.Linq;
using DbLinq.Data.Linq.Database.Implementation;

namespace DbLinq.Data.Linq
{
    partial class DataContext
    {
        public virtual MappingContext MappingContext { get { return _MappingContext; } set { _MappingContext = value; } }

        public DataContext(IDatabaseContext databaseContext, MappingSource mappingSource, IVendor vendor)
        {
            Init(databaseContext, mappingSource, vendor);
        }

        public DataContext(IDbConnection dbConnection, MappingSource mappingSource, IVendor vendor)
            : this(new DatabaseContext(dbConnection), mappingSource, vendor)
        {
        }

        public DataContext(IDatabaseContext databaseContext, IVendor vendor)
            : this(databaseContext, null, vendor)
        {
        }

        public DataContext(IDbConnection dbConnection, IVendor vendor)
            : this(new DatabaseContext(dbConnection), vendor)
        {
        }

        [Obsolete("Please use the other GetTable() methods")]
        public Table<T> GetTable<T>(string tableName) where T : class
        {
            return GetTable(typeof(T)) as Table<T>;
        }

        /// <summary>
        /// Calls method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="method">The method.</param>
        /// <param name="sqlParams">The SQL params.</param>
        /// <returns></returns>
        protected IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            return _ExecuteMethodCall(context, method, sqlParams);
        }
    }
}
