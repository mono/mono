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
using System.Linq;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Linq.Mapping;

using DbLinq.Data.Linq;
using DbLinq.Data.Linq.SqlClient;
using DbLinq.Util;
using DbLinq.Vendor;

#if MONO_STRICT
using DataContext = System.Data.Linq.DataContext;
using DataLinq    = System.Data.Linq;
using ITable      = System.Data.Linq.ITable;
using System.Data.Linq.SqlClient;
#else
using DataContext = DbLinq.Data.Linq.DataContext;
using DataLinq    = DbLinq.Data.Linq;
using ITable      = DbLinq.Data.Linq.ITable;
using DbLinq.Data.Linq.SqlClient;
#endif

namespace DbLinq.SqlServer
{
    [Vendor(typeof(SqlServerProvider), typeof(Sql2000Provider), typeof(Sql2005Provider))]
#if !MONO_STRICT
    public 
#endif
    class SqlServerVendor : Vendor.Implementation.Vendor
    {
        public override string VendorName { get { return "SqlServer"; } }

        protected readonly SqlServerSqlProvider sqlProvider = new SqlServerSqlProvider();
        public override ISqlProvider SqlProvider { get { return sqlProvider; } }

        //NOTE: for Oracle, we want to consider 'Array Binding'
        //http://download-west.oracle.com/docs/html/A96160_01/features.htm#1049674

        /// <summary>
        /// for large number of rows, we want to use BULK INSERT, 
        /// because it does not fill up the translation log.
        /// This is enabled for tables where Vendor.UserBulkInsert[db.Table] is true.
        /// </summary>
        public override void BulkInsert<T>(DataLinq.Table<T> table, List<T> rows, int pageSize, IDbTransaction transaction)
        {
            //use TableLock for speed:
            var bulkCopy = new SqlBulkCopy((SqlConnection)transaction.Connection, SqlBulkCopyOptions.TableLock, null);

            bulkCopy.DestinationTableName = table.Context.Mapping.GetTable(typeof(T)).TableName;
            //bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(bulkCopy_SqlRowsCopied);

            var dt = new DataTable();

            //KeyValuePair<PropertyInfo, ColumnAttribute>[] columns = AttribHelper.GetColumnAttribs2(typeof(T));
            var columns = table.Context.Mapping.GetTable(typeof(T)).RowType.PersistentDataMembers;

            foreach (var column in columns)
            {
                //if (pair.Value.IsDbGenerated)
                //    continue; //don't skip - all fields would be shifted

                var dc = new DataColumn();
                dc.ColumnName = column.MappedName;
                dc.DataType = column.Member.GetMemberType();
                dt.Columns.Add(dc);
            }

            //TODO: cross-check null values against CanBeNull specifier
            //object[] indices = new object[] { };
            foreach (T row in rows)
            {
                DataRow dr = dt.NewRow();
                //use reflection to retrieve object's fields (TODO: optimize this later)
                foreach (var pair in columns)
                {
                    //if (pair.Value.IsDbGenerated)
                    //    continue; //don't assign IDENTITY col
                    object value = pair.Member.GetMemberValue(row);
                    dr[pair.MappedName] = value;
                }
                //dr[1
                dt.Rows.Add(dr);
            }
            bulkCopy.WriteToServer(dt);

        }

        public override System.Data.Linq.IExecuteResult ExecuteMethodCall(DataContext context, System.Reflection.MethodInfo method, params object[] sqlParams)
        {
            throw new NotImplementedException();
        }
    }
}