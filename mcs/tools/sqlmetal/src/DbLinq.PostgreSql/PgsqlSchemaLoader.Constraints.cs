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

namespace DbLinq.PostgreSql
{
    partial class PgsqlSchemaLoader
    {
        /// <summary>
        /// row data from tables table_constraints, constraint_column_usage
        /// </summary>
        protected class DataForeignConstraint
        {
            public string ConstraintName;
            public string TableName;
            public string ConstraintType;
            public string ReferencedTableSchema;
            public string ReferencedTableName;
            public string ColumnName;

            public override string ToString()
            {
                return "ForKeyXR " + ConstraintName + ": " + ConstraintType + "  " + TableName + "->" + ReferencedTableName;
            }
        }

        protected virtual DataForeignConstraint ReadForeignConstraint(IDataReader rdr)
        {
            var t = new DataForeignConstraint();
            int field = 0;
            t.ConstraintName = rdr.GetAsString(field++);
            t.TableName = rdr.GetAsString(field++);
            t.ConstraintType = rdr.GetAsString(field++);
            t.ReferencedTableSchema = rdr.GetAsString(field++);
            t.ReferencedTableName = rdr.GetAsString(field++);
            t.ColumnName = rdr.GetAsString(field++);
            return t;
        }

        protected virtual List<DataForeignConstraint> ReadForeignConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT t.constraint_name, t.table_name, t.constraint_type,
    c.table_schema, c.table_name, c.column_name
FROM information_schema.table_constraints t,
    information_schema.constraint_column_usage c
WHERE t.constraint_name = c.constraint_name
    and t.constraint_type IN  ('FOREIGN KEY','PRIMARY KEY')";

            return DataCommand.Find<DataForeignConstraint>(conn, sql, ":db", db, ReadForeignConstraint);
        }


        /// <summary>
        /// represents one row from Postgres' information_schema.`Key_Column_Usage` table
        /// </summary>
        protected class DataConstraint
        {
            public string ConstraintName;
            public string TableSchema;
            public string TableName;
            public string ColumnName;

            public override string ToString()
            {
                return "KeyColUsage " + ConstraintName + ":  " + TableName + "." + ColumnName;
            }
        }

        protected virtual DataConstraint ReadConstraint(IDataReader rdr)
        {
            DataConstraint t = new DataConstraint();
            int field = 0;
            t.ConstraintName = rdr.GetAsString(field++);
            t.TableSchema = rdr.GetAsString(field++);
            t.TableName = rdr.GetAsString(field++);
            t.ColumnName = rdr.GetAsString(field++);
            return t;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            string sql = @"
SELECT constraint_name,table_schema,table_name
    ,column_name
FROM information_schema.KEY_COLUMN_USAGE
WHERE constraint_catalog=:db";

            return DataCommand.Find<DataConstraint>(conn, sql, ":db", db, ReadConstraint);
        }

    }
}
