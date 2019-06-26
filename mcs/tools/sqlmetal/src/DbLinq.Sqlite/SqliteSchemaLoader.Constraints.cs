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
using System.Text;
using DbLinq.Sqlite.Schema;
using DbLinq.Util;
using DataCommand = DbLinq.Sqlite.Schema.DataCommand;

namespace DbLinq.Sqlite
{
    partial class SqliteSchemaLoader
    {
        /// <summary>
        /// represents one row from MySQL's information_schema.`Key_Column_Usage` table
        /// </summary>
        public class DataConstraint
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

        protected virtual DataConstraint ReadConstraint(IDataReader rdr, string table)
        {
            DataConstraint t = new DataConstraint();
            const int K_ID = 0;
            //const int K_SEQ = 1;
            const int K_TABLE = 2;
            const int K_FROM = 3;
            const int K_TO = 4;

            t.TableSchema = "main";
            t.ReferencedTableSchema = "main";

            t.ConstraintName = "fk_" + table + "_" + rdr.GetAsNumeric<int>(K_ID).ToString();
            t.TableName = table;
            t.ColumnName = rdr.GetAsString(K_FROM);

            t.ReferencedTableName = UnquoteSqlName(rdr.GetAsString(K_TABLE));
            t.ReferencedColumnName = rdr.GetAsString(K_TO);
            return t;

        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            //Could perhaps use conn.GetSchema() instead 
            //Warning... Sqlite doesnt enforce constraints unless you define some triggers

            string sql = @" SELECT tbl_name FROM sqlite_master WHERE type='table' order by tbl_name";
            string sqlPragma = @"PRAGMA foreign_key_list('{0}');";

            return DataCommand.Find<DataConstraint>(conn, sql, sqlPragma, ReadConstraint);
        }
    }
}
