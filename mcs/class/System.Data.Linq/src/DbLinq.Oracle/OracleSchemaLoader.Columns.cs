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
using System.Collections.Generic;
using System.Data;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.Oracle
{
    partial class OracleSchemaLoader
    {
        protected virtual IDataTableColumn ReadColumn(IDataReader rdr)
        {
            var column = new DataTableColumn();
            int field = 0;
            column.TableSchema = rdr.GetString(field++);
            column.TableName = rdr.GetString(field++);
            column.ColumnName = rdr.GetString(field++);
            column.SqlType = rdr.GetString(field++);
            column.Length = rdr.GetAsNullableNumeric<long>(field++);
            column.Precision = rdr.GetAsNullableNumeric<int>(field++);
            column.Scale = rdr.GetAsNullableNumeric<int>(field++);
            string nullableStr = rdr.GetString(field++);
            column.Nullable = nullableStr == "Y";
            column.FullType = column.SqlType;
            return column;
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            const string sql = @"
SELECT owner, table_name, column_name, 
  case when data_type like 'TIMESTAMP%' then 'TIMESTAMP'
       when data_type like 'INTERVAL%' then 'INTERVAL'
  else data_type
  end data_type , 
    data_length, data_precision, data_scale, nullable
FROM all_tab_columns
WHERE table_name NOT LIKE '%$%' 
    AND table_name NOT LIKE 'LOGMNR%' 
    AND table_name NOT LIKE 'MVIEW%' 
    AND table_name NOT IN ('SQLPLUS_PRODUCT_PROFILE','HELP', 'PRODUCT_PRIVS')
    -- skip nested table columns
    AND data_type_owner is null
    AND lower(owner) = :owner
ORDER BY table_name, column_id";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, ":owner", databaseName.ToLower(), ReadColumn);
        }
    }
}
