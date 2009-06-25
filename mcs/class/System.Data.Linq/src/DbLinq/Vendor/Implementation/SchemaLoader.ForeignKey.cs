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

using System.Data;
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using System.Text;
using System.Collections.Generic;

namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        /// <summary>
        /// Loads the constraints.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="conn">The conn.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <param name="names">The names.</param>
        protected abstract void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names);

        protected string BuildForeignKey(IDictionary<string, ColumnName> table, string key)
        {
            string[] keys = key.Split(',');
            StringBuilder result = new StringBuilder();
            foreach (string lookupKey in keys)
            {
                if (result.Length != 0)
                    result.Append(',');
                result.Append(table[lookupKey].PropertyName);
            }

            return result.ToString();
        }

        /// <summary>
        /// Loads the foreign key.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="table">The table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableSchema">The table schema.</param>
        /// <param name="referencedColumnName">Name of the referenced column.</param>
        /// <param name="referencedTableName">Name of the referenced table.</param>
        /// <param name="referencedTableSchema">The referenced table schema.</param>
        /// <param name="constraintName">Name of the constraint.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <param name="names">The names.</param>
        protected virtual void LoadForeignKey(Database schema, Table table, string columnName, string tableName, string tableSchema,
            string referencedColumnName, string referencedTableName, string referencedTableSchema,
            string constraintName,
            NameFormat nameFormat, Names names)
        {
            var foreignKey = BuildForeignKey(names.ColumnsNames[tableName], columnName);
            var reverseForeignKey = BuildForeignKey(names.ColumnsNames[referencedTableName], referencedColumnName);

            var associationName = CreateAssociationName(tableName, tableSchema,
                referencedTableName, referencedTableSchema, constraintName, foreignKey, nameFormat);

            //both parent and child table get an [Association]
            var assoc = new Association();
            assoc.IsForeignKey = true;
            assoc.Name = constraintName;
            assoc.Type = null;
            assoc.ThisKey = foreignKey;
            assoc.OtherKey = reverseForeignKey;
            assoc.Member = associationName.ManyToOneMemberName;
            assoc.Cardinality = Cardinality.Many; // TODO: check this is the right direction (even if it appears to be useless)
            table.Type.Associations.Add(assoc);

            //and insert the reverse association:
            var reverseAssociation = new Association();
            reverseAssociation.Name = constraintName;
            reverseAssociation.Type = table.Type.Name;
            reverseAssociation.Member = associationName.OneToManyMemberName;
            reverseAssociation.Cardinality = Cardinality.One;
            reverseAssociation.ThisKey = reverseForeignKey;
            reverseAssociation.OtherKey = foreignKey;
            reverseAssociation.DeleteRule = "NO ACTION";

            string referencedFullDbName = GetFullDbName(referencedTableName, referencedTableSchema);
            var referencedTable = schema.Tables.FirstOrDefault(t => referencedFullDbName == t.Name);
            if (referencedTable == null)
            {
                //try case-insensitive match 
                //reason: MySql's Key_Column_Usage table contains both 'Northwind' and 'northwind'
                referencedTable = schema.Tables.FirstOrDefault(t => referencedFullDbName.ToLower() == t.Name.ToLower());
            }

            if (referencedTable == null)
            {
                ReportForeignKeyError(schema, referencedFullDbName);
            }
            else
            {
                referencedTable.Type.Associations.Add(reverseAssociation);
                assoc.Type = referencedTable.Type.Name;
            }
        }

        /// <summary>
        /// Reports a foreign key error.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="referencedTableFull">The referenced table full.</param>
        private void ReportForeignKeyError(Database schema, string referencedTableFull)
        {
            var tablesMap = schema.Tables.ToDictionary(t => t.Name.ToLower());
            var referencedTableFullL = referencedTableFull.ToLower();

            string msg = "ERROR L91: parent table not found: " + referencedTableFull;
            Table matchedTable;
            if (tablesMap.TryGetValue(referencedTableFullL, out matchedTable))
            {
                //case problems arise from various reasons,
                //e.g. different capitalization on WIndows vs Linux,
                //bugs in DbLinq etc
                msg += " - however, schema lists a table spelled as " + matchedTable.Name;
            }
            WriteErrorLine(msg);
        }
    }
}
