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
using System.Globalization;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbDbSchemaFactory
	{
		#region Constructors

		private FbDbSchemaFactory()
		{
		}

		#endregion

		#region Methods

		public static DataTable GetSchema(
			FbConnection connection,
			string collectionName,
			object[] restrictions)
		{
			FbDbSchema returnSchema = null;

			switch (collectionName.ToLower(CultureInfo.CurrentCulture))
			{
				case "charactersets":
					returnSchema = new FbCharacterSets();
					break;

				case "checkconstraints":
					returnSchema = new FbCheckConstraints();
					break;

				case "checkconstraintsbytable":
					returnSchema = new FbChecksByTable();
					break;

				case "collations":
					returnSchema = new FbCollations();
					break;

				case "columns":
					returnSchema = new FbColumns();
					break;

				case "columnprivileges":
					returnSchema = new FbColumnPrivileges();
					break;

				case "datatypes":
					return FbDataTypes.GetSchema();

				case "domains":
					returnSchema = new FbDomains();
					break;

				case "foreignkeys":
					returnSchema = new FbForeignKeys();
					break;

				case "functions":
					returnSchema = new FbFunctions();
					break;

				case "generators":
					returnSchema = new FbGenerators();
					break;

				case "indexes":
					returnSchema = new FbIndexes();
					break;

				case "metadatacollections":
					return FbMetaDataCollections.GetSchema();

				case "primarykeys":
					returnSchema = new FbPrimaryKeys();
					break;

				case "procedures":
					returnSchema = new FbProcedures();
					break;

				case "procedureparameters":
					returnSchema = new FbProcedureParameters();
					break;

				case "procedureprivileges":
					returnSchema = new FbProcedurePrivilegesSchema();
					break;

				case "restrictions":
					return FbRestrictions.GetSchema();

				case "roles":
					returnSchema = new FbRoles();
					break;

				case "tables":
					returnSchema = new FbTables();
					break;

				case "tableconstraints":
					returnSchema = new FbTableConstraints();
					break;

				case "tableprivileges":
					returnSchema = new FbTablePrivileges();
					break;

				case "triggers":
					returnSchema = new FbTriggers();
					break;

				case "uniquekeys":
					returnSchema = new FbUniqueKeys();
					break;

				case "viewcolumnusage":
					returnSchema = new FbViewColumnUsage();
					break;

				case "views":
					returnSchema = new FbViews();
					break;

				case "viewprivileges":
					returnSchema = new FbViewPrivileges();
					break;

				default:
					throw new NotSupportedException("The specified metadata collection is not supported.");
			}

			return returnSchema.GetSchema(connection, restrictions);
		}

		#endregion
	}
}