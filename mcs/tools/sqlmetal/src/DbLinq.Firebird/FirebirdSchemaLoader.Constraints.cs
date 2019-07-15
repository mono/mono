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

namespace DbLinq.Firebird
{
    partial class FirebirdSchemaLoader
    {
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
            constraint.TableName = rdr.GetAsString(field++).Trim();
            constraint.ColumnName = rdr.GetAsString(field++).Trim();
            constraint.ReferencedTableSchema = rdr.GetAsString(field++);
            constraint.ReferencedTableName = rdr.GetAsString(field++).Trim();
            constraint.ReferencedColumnName = rdr.GetAsString(field++).Trim();
            return constraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            // TODO: Only supports constrains where the columns have the same name.
            string sql = @"
select tbl.RDB$CONSTRAINT_NAME ""ConstraintName""
       , 'Foo' ""TableSchema""
       , tbl.RDB$RELATION_NAME ""TableName""
       , col.RDB$FIELD_NAME ""ColumnName""
       , 'Foo'  ""ReferencedTableSchema""
       , rtbl.RDB$RELATION_NAME ""ReferencedTableName""
       , rcol.RDB$FIELD_NAME ""ReferencedColumnName""
    from RDB$RELATION_CONSTRAINTS tbl
        inner join RDB$INDEX_SEGMENTS col on col.RDB$INDEX_NAME = tbl.RDB$INDEX_NAME
        inner join RDB$REF_CONSTRAINTS ref on ref.RDB$CONSTRAINT_NAME = tbl.RDB$CONSTRAINT_NAME
        inner join RDB$RELATION_CONSTRAINTS rtbl on rtbl.RDB$CONSTRAINT_NAME = ref.RDB$CONST_NAME_UQ
        inner join RDB$INDEX_SEGMENTS rcol on rcol.RDB$INDEX_NAME = rtbl.RDB$INDEX_NAME
    where tbl.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY' and col.RDB$FIELD_NAME = rcol.RDB$FIELD_NAME
    order by tbl.RDB$RELATION_NAME
";

            return DataCommand.Find<DataConstraint>(conn, sql, "@db", db, ReadConstraint);
        }
    }
}
