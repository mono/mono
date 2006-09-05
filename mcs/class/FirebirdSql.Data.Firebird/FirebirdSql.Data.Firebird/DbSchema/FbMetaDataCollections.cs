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
	internal sealed class FbMetaDataCollections
	{
		private FbMetaDataCollections()
		{
		}

		public static DataTable GetSchema()
		{
			DataTable table = new DataTable("MetaDataCollections");

			table.Columns.Add("CollectionName", typeof(string));
			table.Columns.Add("NumberOfRestrictions", typeof(int));
			table.Columns.Add("NumberOfIdentifierParts", typeof(int));

			table.Rows.Add(new object[] { "CharacterSets", 3, 0 });
			table.Rows.Add(new object[] { "CheckConstraints", 3, 0 });
			table.Rows.Add(new object[] { "CheckConstraintsByTable", 3, 0 });
			table.Rows.Add(new object[] { "Collations", 3, 0 });
			table.Rows.Add(new object[] { "ColumnPrivileges", 6, 0 });
			table.Rows.Add(new object[] { "Columns", 4, 0 });
			table.Rows.Add(new object[] { "DataTypes", 0, 0 });
			table.Rows.Add(new object[] { "Domains", 3, 0 });
			table.Rows.Add(new object[] { "ForeignKeys", 6, 0 });
			table.Rows.Add(new object[] { "Functions", 4, 0 });
			table.Rows.Add(new object[] { "Generators", 4, 0 });
			table.Rows.Add(new object[] { "Indexes", 4, 0 });
			table.Rows.Add(new object[] { "MetaDataCollections", 0, 0 });
			table.Rows.Add(new object[] { "PrimaryKeys", 3, 0 });
			table.Rows.Add(new object[] { "ProcedureParameters", 4, 0 });
			table.Rows.Add(new object[] { "ProcedurePrivileges", 5, 0 });
			table.Rows.Add(new object[] { "Procedures", 3, 0 });
			table.Rows.Add(new object[] { "Restrictions", 0, 0 });
			table.Rows.Add(new object[] { "Roles", 1, 0 });
			table.Rows.Add(new object[] { "Tables", 4, 0 });
			table.Rows.Add(new object[] { "TableConstraints", 7, 0 });
			table.Rows.Add(new object[] { "TablePrivileges", 5, 0 });
			table.Rows.Add(new object[] { "Triggers", 4, 0 });
			table.Rows.Add(new object[] { "UniqueKeys", 3, 0 });
			table.Rows.Add(new object[] { "ViewColumnUsage", 3, 0 });
			table.Rows.Add(new object[] { "ViewPrivileges", 5, 0 });
			table.Rows.Add(new object[] { "Views", 3, 0 });

			return table;
		}
	}
}
