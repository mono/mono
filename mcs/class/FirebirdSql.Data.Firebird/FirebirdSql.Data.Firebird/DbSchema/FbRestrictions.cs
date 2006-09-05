/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal sealed class FbRestrictions
	{
		private FbRestrictions()
		{
		}

		public static DataTable GetSchema()
		{
			DataTable table = new DataTable("Restrictions");

			table.Columns.Add("CollectionName", typeof(System.String));
			table.Columns.Add("RestrictionName", typeof(System.String));
			table.Columns.Add("RestrictionDefault", typeof(System.String));
			table.Columns.Add("RestrictionNumber", typeof(System.Int32));

			table.Rows.Add(new object[] { "CharacterSets", "Catalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "CharacterSets", "Schema", "table_schema", 2 });
			table.Rows.Add(new object[] { "CharacterSets", "Name", "character_set_name", 3 });
			table.Rows.Add(new object[] { "CheckConstraints", "Catalog", "constraint_catalog", 1 });
			table.Rows.Add(new object[] { "CheckConstraints", "Schema", "constraint_schema", 2 });
			table.Rows.Add(new object[] { "CheckConstraints", "Name", "constraint_name", 3 });
			table.Rows.Add(new object[] { "CheckConstraintsByTable", "Catalog", "constraint_catalog", 1 });
			table.Rows.Add(new object[] { "CheckConstraintsByTable", "Schema", "constraint_schema", 2 });
			table.Rows.Add(new object[] { "CheckConstraintsByTable", "Name", "constraint_name", 3 });
			table.Rows.Add(new object[] { "Collations", "Catalog", "collation_catalog", 1 });
			table.Rows.Add(new object[] { "Collations", "Schema", "collation_schema", 2 });
			table.Rows.Add(new object[] { "Collations", "Name", "collation_name", 3 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "ColumnName", "column_name", 4 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "Grantor", "grantor", 5 });
			table.Rows.Add(new object[] { "ColumnPrivileges", "Grantee", "grantee", 6 });
			table.Rows.Add(new object[] { "Columns", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "Columns", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "Columns", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "Columns", "ColumnName", "column_name", 4 });
			table.Rows.Add(new object[] { "Domains", "Catalog", "domain_catalog", 1 });
			table.Rows.Add(new object[] { "Domains", "Schema", "domain_schema", 2 });
			table.Rows.Add(new object[] { "Domains", "Name", "domain_name", 3 });
			table.Rows.Add(new object[] { "ForeignKeys", "PkTableCatalog", "pk_table_catalog", 1 });
			table.Rows.Add(new object[] { "ForeignKeys", "PkTableSchema", "pk_table_schema", 2 });
			table.Rows.Add(new object[] { "ForeignKeys", "PkTableName", "pk_table_name", 3 });
			table.Rows.Add(new object[] { "ForeignKeys", "FkTableCatalog", "fk_table_catalog", 4 });
			table.Rows.Add(new object[] { "ForeignKeys", "FkTableSchema", "fk_table_schema", 5 });
			table.Rows.Add(new object[] { "ForeignKeys", "FkTableName", "fk_table_name", 6 });
			table.Rows.Add(new object[] { "Functions", "Catalog", "function_catalog", 1 });
			table.Rows.Add(new object[] { "Functions", "Schema", "function_schema", 2 });
			table.Rows.Add(new object[] { "Functions", "Name", "function_name", 3 });
			table.Rows.Add(new object[] { "Functions", "IsSystemFunction", "is_system_function", 4 });
			table.Rows.Add(new object[] { "Generators", "Catalog", "generator_catalog", 1 });
			table.Rows.Add(new object[] { "Generators", "Schema", "generator_schema", 2 });
			table.Rows.Add(new object[] { "Generators", "Name", "generator_name", 3 });
			table.Rows.Add(new object[] { "Generators", "IsSystemGenerator", "is_system_generator", 4 });
			table.Rows.Add(new object[] { "Indexes", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "Indexes", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "Indexes", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "Indexes", "Name", "index_name", 4 });
			table.Rows.Add(new object[] { "PrimaryKeys", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "PrimaryKeys", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "PrimaryKeys", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "ProcedureParameters", "ProcedureCatalog", "procedure_catalog", 1 });
			table.Rows.Add(new object[] { "ProcedureParameters", "ProcedureSchema", "procedure_schema", 2 });
			table.Rows.Add(new object[] { "ProcedureParameters", "ProcedureName", "procedure_name", 3 });
			table.Rows.Add(new object[] { "ProcedureParameters", "Name", "procedure_parameter", 4 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", "ProcedureCatalog", "procedure_catalog", 1 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", "ProcedureSchema", "procedure_schema", 2 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", "ProcedureName", "procedure_name", 3 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", "Grantor", "grantor", 4 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", "Grantee", "grantee", 5 });
			table.Rows.Add(new object[] { "Procedures", "ProcedureCatalog", "procedure_catalog", 1 });
			table.Rows.Add(new object[] { "Procedures", "ProcedureSchema", "procedure_schema", 2 });
			table.Rows.Add(new object[] { "Procedures", "Name", "procedure_name", 3 });
			table.Rows.Add(new object[] { "Roles", "Name", "role_name", 1 });
			table.Rows.Add(new object[] { "TableConstraints", "ConstraintCatalog", "constraint_catalog", 1 });
			table.Rows.Add(new object[] { "TableConstraints", "ConstraintSchema", "constraint_schema", 2 });
			table.Rows.Add(new object[] { "TableConstraints", "ConstraintName", "constraint_name", 3 });
			table.Rows.Add(new object[] { "TableConstraints", "TableCatalog", "table_catalog", 4 });
			table.Rows.Add(new object[] { "TableConstraints", "TableSchema", "table_schema", 5 });
			table.Rows.Add(new object[] { "TableConstraints", "TableName", "table_name", 6 });
			table.Rows.Add(new object[] { "TableConstraints", "ConstraintType", "constraint_type", 7 });
			table.Rows.Add(new object[] { "TablePrivileges", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "TablePrivileges", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "TablePrivileges", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "TablePrivileges", "Grantor", "grantor", 4 });
			table.Rows.Add(new object[] { "TablePrivileges", "Grantee", "grantee", 5 });
			table.Rows.Add(new object[] { "Tables", "Catalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "Tables", "Schema", "table_schema", 2 });
			table.Rows.Add(new object[] { "Tables", "Name", "table_name", 3 });
			table.Rows.Add(new object[] { "Tables", "Type", "table_type", 4 });
			table.Rows.Add(new object[] { "Triggers", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "Triggers", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "Triggers", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "Triggers", "Name", "trigger_name", 4 });
			table.Rows.Add(new object[] { "UniqueKeys", "TableCatalog", "table_catalog", 1 });
			table.Rows.Add(new object[] { "UniqueKeys", "TableSchema", "table_schema", 2 });
			table.Rows.Add(new object[] { "UniqueKeys", "TableName", "table_name", 3 });
			table.Rows.Add(new object[] { "ViewColumnUsage", "ViewCatalog", "view_catalog", 1 });
			table.Rows.Add(new object[] { "ViewColumnUsage", "ViewSchema", "view_schema", 2 });
			table.Rows.Add(new object[] { "ViewColumnUsage", "ViewName", "view_name", 3 });
			table.Rows.Add(new object[] { "ViewPrivileges", "ViewCatalog", "view_catalog", 1 });
			table.Rows.Add(new object[] { "ViewPrivileges", "ViewSchema", "view_schema", 2 });
			table.Rows.Add(new object[] { "ViewPrivileges", "ViewName", "view_name", 3 });
			table.Rows.Add(new object[] { "ViewPrivileges", "Grantor", "grantor", 4 });
			table.Rows.Add(new object[] { "ViewPrivileges", "Grantee", "grantee", 5 });
			table.Rows.Add(new object[] { "Views", "Catalog", "view_catalog", 1 });
			table.Rows.Add(new object[] { "Views", "Schema", "view_schema", 2 });
			table.Rows.Add(new object[] { "Views", "Name", "view_name", 3 });

			return table;
		}
	}
}
