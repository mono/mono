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

namespace DbLinq.PostgreSql
{
    partial class PgsqlSchemaLoader
    {
        protected virtual string GetColumnFullType(string domain_name, string domain_schema, IDataTableColumn column)
        {
            // TODO: uncomment
            if (/* mmConfig.useDomainTypes && */domain_name != null)
                return domain_schema + "." + domain_name; //without precision - precision is already defined in CREATE DOMAIN

            if (column.Length != null)
                return column.Type + "(" + column.Length + ")";
            if (column.Precision != null && column.Scale != null)
                return column.Type + "(" + column.Precision + "," + column.Scale + ")";
            return column.Type;
        }

        protected virtual string GetColumnDefaultValue(string defaultValue)
        {
            if (defaultValue == null)
                return defaultValue;
            // nextval('suppliers_supplierid_seq'::regclass)
            return defaultValue.Replace("::regclass)", ")");
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
            column.Type = rdr.GetAsString(field++);
            var domain_schema = rdr.GetAsString(field++);
            var domain_name = rdr.GetAsString(field++);
            column.DefaultValue = GetColumnDefaultValue(rdr.GetAsString(field++));
            column.Generated = column.DefaultValue != null && column.DefaultValue.StartsWith("nextval(");

            column.Length = rdr.GetAsNullableNumeric<long>(field++);
            column.Precision = rdr.GetAsNullableNumeric<int>(field++);
            column.Scale = rdr.GetAsNullableNumeric<int>(field++);

            column.FullType = GetColumnFullType(domain_name, domain_schema, column);

            return column;
        }

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connectionString, string databaseName)
        {
            const string sql = @"
SELECT table_schema, table_name, column_name
    ,is_nullable, data_type, domain_schema, domain_name, column_default
    ,character_maximum_length, numeric_precision, numeric_scale
FROM information_schema.COLUMNS
WHERE table_catalog=:db
    AND table_schema NOT IN ('pg_catalog','information_schema')
ORDER BY ordinal_position
";

            return DataCommand.Find<IDataTableColumn>(connectionString, sql, ":db", databaseName, ReadColumn);
        }
    }
}
