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

namespace DbLinq.MySql
{
    partial class MySqlSchemaLoader
    {
        /// <summary>
        /// represents one row from MySQL's information_schema.`Key_Column_Usage` table
        /// </summary>
        protected class DataConstraint
        {
            public string ConstraintName;
            public string TableSchema;
            public string TableName;
            public string ColumnName;
            public string ReferencedTableSchema;
            public string ReferencedTableName;
            public string ReferencedColumnName;

            public override string ToString()
            {
                string detail = ConstraintName == "PRIMARY"
                                    ? TableName + " PK"
                                    : ConstraintName;
                return "KeyColUsage " + detail;
            }
        }

        protected virtual DataConstraint ReadConstraint(IDataReader rdr)
        {
            DataConstraint constraint = new DataConstraint();
            int field = 0;
            constraint.ConstraintName = rdr.GetAsString(field++);
            constraint.TableSchema = rdr.GetAsString(field++);
            constraint.TableName = rdr.GetAsString(field++);
            constraint.ColumnName = rdr.GetAsString(field++);
            constraint.ReferencedTableSchema = rdr.GetAsString(field++);
            constraint.ReferencedTableName = rdr.GetAsString(field++);
            constraint.ReferencedColumnName = rdr.GetAsString(field++);
            return constraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_name,table_schema,table_name
    ,GROUP_CONCAT(column_name SEPARATOR ',') AS column_name,referenced_table_schema,referenced_table_name,GROUP_CONCAT(referenced_column_name SEPARATOR ',') AS referenced_column_name
FROM information_schema.`KEY_COLUMN_USAGE`
WHERE table_schema=?db GROUP BY constraint_name,table_schema,table_name,referenced_table_name";

            return DataCommand.Find<DataConstraint>(conn, sql, "?db", db, ReadConstraint);
        }
    }
}
