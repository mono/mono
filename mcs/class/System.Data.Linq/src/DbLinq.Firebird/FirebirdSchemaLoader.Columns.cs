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

namespace DbLinq.Firebird
{
    partial class FirebirdSchemaLoader
    {
        protected virtual IDataTableColumn ReadColumn(IDataReader rdr)
        {
            var column = new DataTableColumn();
            int field = 0;
            column.TableSchema = rdr.GetAsString(field++);
            column.TableName = rdr.GetAsString(field++).Trim();
            column.ColumnName = rdr.GetAsString(field++).Trim();
            column.Nullable = rdr.GetAsBool(field++);
            column.SqlType = rdr.GetAsString(field++).Trim();
            column.Generated = rdr.GetAsBool(field++);
            //column.Unsigned = column.FullType.Contains("unsigned");
            column.PrimaryKey = rdr.GetAsBool(field++);
            column.Length = rdr.GetAsNullableNumeric<long>(field++);
            column.Precision = rdr.GetAsNullableNumeric<int>(field++);
            column.Scale = rdr.GetAsNullableNumeric<int>(field++);
            column.DefaultValue = rdr.GetAsString(field++);
            FormatFullType(column);
            return column;
        }

        private void FormatFullType(DataTableColumn column)
        {
            // TODO: Implement.
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            const string sql = @"
select 'Foo' ""TableSchema""
        , rf.RDB$RELATION_NAME ""TableName""
        , rf.RDB$FIELD_NAME ""ColumnName""
        , case when rf.RDB$NULL_FLAG is null then 1 else 0 end ""Nullable""
        , t.RDB$TYPE_NAME ""Type""
        , case when f.RDB$COMPUTED_SOURCE is null then 0 else 1 end ""Generated""
        , case when exists(select *
            from RDB$RELATION_CONSTRAINTS rc
                inner join RDB$INDEX_SEGMENTS xs on xs.RDB$INDEX_NAME = rc.RDB$INDEX_NAME
            where rc.RDB$RELATION_NAME = rf.RDB$RELATION_NAME and xs.RDB$FIELD_NAME = rf.RDB$FIELD_NAME
                and rc.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY') then 1 else 0 end ""PrimaryKey""
        , f.RDB$FIELD_LENGTH ""Length""
        , f.RDB$FIELD_PRECISION ""Precision""
        , f.RDB$FIELD_SCALE ""Scale""
        , rf.RDB$DEFAULT_VALUE ""DefaultValue""
    from RDB$RELATION_FIELDS rf
        inner join RDB$FIELDS f on f.RDB$FIELD_NAME = rf.RDB$FIELD_SOURCE
        inner join RDB$TYPES t on t.RDB$TYPE = f.RDB$FIELD_TYPE and t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
    where rf.RDB$SYSTEM_FLAG = 0
    order by rf.RDB$RELATION_NAME, rf.RDB$FIELD_POSITION
";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, "@db", databaseName, ReadColumn);
        }
    }
}
