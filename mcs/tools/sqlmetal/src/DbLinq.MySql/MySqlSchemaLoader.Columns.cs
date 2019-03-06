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
using System.Data;
using System.Linq;
using System.Text;
using DbLinq.Util;
using DbLinq.Vendor;

namespace DbLinq.MySql
{
    partial class MySqlSchemaLoader
    {
        protected virtual string FormatFullType(string fullType)
        {
            fullType = fullType.Replace("int(11)", "int") //remove some default sizes
                .Replace("int(10) unsigned", "int unsigned")
                .Replace("mediumint(8) unsigned", "mediumint unsigned")
                .Replace("decimal(10,0)", "decimal")
                ;
            return fullType;
        }

        protected virtual IDataTableColumn ReadColumn(IDataReader rdr)
        {
            var column = new DataTableColumn();
            int field = 0;
            column.TableSchema = rdr.GetAsString(field++);
            column.TableName = rdr.GetAsString(field++);
            column.ColumnName = rdr.GetAsString(field++);
            string nullableStr = rdr.GetAsString(field++);
            column.Nullable = nullableStr == "YES";
            column.SqlType = rdr.GetAsString(field++);
            var extra = rdr.GetAsString(field++); 
            column.Generated = extra == "auto_increment";
            column.FullType = FormatFullType(rdr.GetAsString(field++));
            column.Unsigned = column.FullType.Contains("unsigned");
            string columnKey = rdr.GetAsString(field++);
            column.PrimaryKey = columnKey == "PRI";
            column.Length = rdr.GetAsNullableNumeric<long>(field++);
            column.Precision = rdr.GetAsNullableNumeric<int>(field++);
            column.Scale = rdr.GetAsNullableNumeric<int>(field++);
            column.DefaultValue = rdr.GetAsString(field++);
            return column;
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            const string sql = @"
SELECT table_schema,table_name,column_name
    ,is_nullable,data_type,extra,column_type
    ,column_key,character_maximum_length,numeric_precision,numeric_scale,
    column_default
FROM information_schema.`COLUMNS`
WHERE table_schema=?db";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, "?db", databaseName, ReadColumn);
        }
    }
}
