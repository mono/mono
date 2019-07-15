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

namespace DbLinq.Ingres
{
    partial class IngresSchemaLoader
    {
        /// <summary>
        /// row data from tables table_constraints, constraint_column_usage
        /// </summary>
        protected class DataConstraint
        {
            public string TableSchema;
            public string TableName;

            /// <summary>
            /// P = PRIMARY KEY; R = FOREIGN KEY
            /// </summary>
            public string ConstraintType;

            public override string ToString()
            {

                return (ConstraintType == "P" ?
                    "PK(" + TableName + "." + ColumnName + ")"
                        :
                    "FK(" + TableName + "." + ColumnName + " => " + ReferencedTableName + "." + ReferencedColumnName + ")");
            }

            public string ConstraintName;

            /*
            public string[] column_name_primaries
            {
                get
                {
                    string[] tmp = TextSegment
                        .Replace("PRIMARY KEY(", "")
                        .Replace(")", "")
                        .Split(',');
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        tmp[i] = tmp[i].Trim();
                        tmp[i] = tmp[i].Replace("\"", "");
                    }
                    return tmp;
                }
            }
             */

            //public string[] column_name_primaries = new string[] { "", "" };

            public string ColumnName;

            public string ReferencedColumnName;

            public string ReferencedTableSchema;

            public string ReferencedTableName;

        }

        protected virtual DataConstraint ReadContraint(IDataReader rdr)
        {
            var constraint = new DataConstraint();
            int field = 0;
            constraint.ConstraintType = rdr.GetString(field++).Trim();
            constraint.ConstraintName = rdr.GetString(field++).Trim();
            constraint.TableSchema = rdr.GetString(field++).Trim();
            constraint.TableName = rdr.GetString(field++).Trim();
            constraint.ColumnName = rdr.GetString(field++).Trim();
            constraint.ReferencedTableSchema = rdr.GetString(field++).Trim();
            constraint.ReferencedTableName = rdr.GetString(field++).Trim();
            constraint.ReferencedColumnName = rdr.GetString(field++).Trim();
            return constraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            // picrap -> Thomas: I switched the FK orders without really understanding why
            //                   because relations were inversed
            string sql = @"
                SELECT DISTINCT 
		                c.constraint_type as constraint_type,
		                c.constraint_name as constraint_name, 
                        k.schema_name as schema_name,
                        k.table_name as table_name,
		                k.column_name AS column_name,
                        '' as referenced_schema_name,
                        '' as referenced_table_name,
                        '' as referenced_column_name
                FROM 
		                iiconstraints c, 
		                iikeys k 
                WHERE 
		                k.constraint_name = c.constraint_name AND 
		                c.constraint_type = 'P'
                UNION
                SELECT DISTINCT
		                c.constraint_type as constraint_type,
                        squeeze(f.table_name) || '_' || p.constraint_name as constraint_name,
                        f.schema_name as referenced_schema_name,
                        f.table_name as referenced_table_name,
                        f.column_name as referenced_column_name,
                        p.schema_name as schema_name,
                        p.table_name as table_name,
                        p.column_name as column_name
                FROM
                        iikeys p,
                        iiconstraints c,
                        iiref_constraints rc,
                        iikeys f
                WHERE
                        c.constraint_type = 'R' and
                        c.constraint_name = rc.ref_constraint_name AND
                        p.constraint_name = rc.unique_constraint_name AND
                        f.constraint_name = rc.ref_constraint_name AND
                        p.key_position = f.key_position
                ";

            return DataCommand.Find<DataConstraint>(conn, sql, ReadContraint);
        }
    }
}
