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
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Oracle
{
    partial class OracleSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new OracleVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(OracleDataContext); } }

        protected override void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
            var constraints = ReadConstraints(conn, schemaName.DbName);

            foreach (DataConstraint constraint in constraints)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(constraint.TableName, constraint.TableSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    WriteErrorLine("ERROR L100: Table '" + constraint.TableName + "' not found for column " + constraint.ColumnName);
                    continue;
                }

                //if (table.Name.StartsWith("E"))
                //    Logger.Write("---Dbg");

                if (constraint.ConstraintType == "P")
                {
                    //A) add primary key
                    DbLinq.Schema.Dbml.Column pkColumn = table.Type.Columns.Where(c => c.Name == constraint.ColumnName).First();
                    pkColumn.IsPrimaryKey = true;
                }
                else if (constraint.ConstraintType == "R")
                {
                    //if not PRIMARY, it's a foreign key. (constraint_type=="R")
                    //both parent and child table get an [Association]
                    DataConstraint referencedConstraint = constraints.FirstOrDefault(c => c.ConstraintName == constraint.ReverseConstraintName);
                    if (constraint.ReverseConstraintName == null || referencedConstraint == null)
                    {
                        WriteErrorLine("ERROR L127: given R_contraint_name='" + constraint.ReverseConstraintName + "', unable to find parent constraint");
                        continue;
                    }

                    LoadForeignKey(schema, table, constraint.ColumnName, constraint.TableName, constraint.TableSchema,
                                   referencedConstraint.ColumnName, referencedConstraint.TableName,
                                   referencedConstraint.TableSchema,
                                   constraint.ConstraintName, nameFormat, names);

                }
                // custom type, this is a trigger
                else if (constraint.ConstraintType == "T" && constraint.ColumnName != null)
                {
                    var column = table.Type.Columns.Where(c => c.Name == constraint.ColumnName).First();
                    column.Expression = constraint.Expression;
                    column.IsDbGenerated = true;
                }
            }

            //GuessSequencePopulatedFields(schema);
        }

        /// <summary>
        /// guess which fields are populated by sequences.
        /// Mark them with [AutoGenId].
        /// </summary>
        public static void GuessSequencePopulatedFields(DbLinq.Schema.Dbml.Database schema)
        {
            if (schema == null)
                return;
            foreach (DbLinq.Schema.Dbml.Table tbl in schema.Tables)
            {
                var q = from col in tbl.Type.Columns
                        where col.IsPrimaryKey
                        select col;
                List<DbLinq.Schema.Dbml.Column> cols = q.ToList();
                bool canBeFromSequence = cols.Count == 1
                    && (!cols[0].CanBeNull)
                    && (cols[0].DbType == "NUMBER" || cols[0].DbType == "INTEGER");
                if (canBeFromSequence)
                {
                    //TODO: query sequences, store sequence name.
                    //in the meantime, assume naming convention similar to 'Products_seq'
                    cols[0].IsDbGenerated = true;
                }
            }
        }
    }
}
