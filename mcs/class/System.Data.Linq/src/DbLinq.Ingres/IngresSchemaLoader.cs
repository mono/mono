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
using DbLinq.Logging;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Ingres
{
    public partial class IngresSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new IngresVendor();
        public override Vendor.IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(IngresDataContext); } }

        protected override void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            var foreignKeys = ReadConstraints(conn, schemaName.DbName);

            foreach (DataConstraint keyColRow in foreignKeys)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(keyColRow.TableName, keyColRow.TableSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L138: Table '"
                                              + keyColRow.TableName
                                              + "' not found for column "
                                              + keyColRow.ColumnName);
                    continue;
                }

                if (keyColRow.ConstraintType.Equals("P")) //'PRIMARY KEY'
                {
                    //foreach (string pk_name in keyColRow.column_name_primaries)
                    //{
                        DbLinq.Schema.Dbml.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.ColumnName);
                        primaryKeyCol.IsPrimaryKey = true;
                    //}
                    continue;
                }

                if (keyColRow.ConstraintType.Equals("R")) //'FOREIGN KEY'
                {
                    // This is very bad...
                    if (!names.ColumnsNames[keyColRow.ReferencedTableName].ContainsKey(keyColRow.ReferencedColumnName))
                        continue;

                    LoadForeignKey(schema, table, keyColRow.ColumnName, keyColRow.TableName,
                                   keyColRow.TableSchema,
                                   keyColRow.ReferencedColumnName, keyColRow.ReferencedTableName,
                                   keyColRow.ReferencedTableSchema,
                                   keyColRow.ConstraintName, nameFormat, names);

                }

            }
        }

        protected override System.Type MapDbType(string columnName, IDataType dataType)
        {
            switch (dataType.Type.ToLower())
            {
            case "float":
                return typeof(Double);
            case "integer":
                switch (dataType.Length)
                {
                case 1:
                    return typeof(Byte);
                case 2:
                    return typeof(Int16);
                case 4:
                    return typeof(Int32);
                case 8:
                    return typeof(Int64);
                }
                return MapDbType(columnName, null);
            default:
                return base.MapDbType(columnName, dataType);
            }
        }
    }
}
