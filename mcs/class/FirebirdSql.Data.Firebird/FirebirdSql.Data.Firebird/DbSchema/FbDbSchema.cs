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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal abstract class FbDbSchema
	{
		#region Fields

		private string schemaName;

		#endregion

		#region Constructors

		public FbDbSchema(string schemaName)
		{
			this.schemaName = schemaName;
		}

		#endregion

		#region Abstract Methods

		protected abstract StringBuilder GetCommandText(object[] restrictions);

		#endregion

		#region Methods

		public virtual DataTable GetSchema(FbConnection connection, object[] restrictions)
		{
			restrictions = this.ParseRestrictions(restrictions);

			FbCommand		command = this.BuildCommand(connection, restrictions);
			FbDataAdapter	adapter = new FbDataAdapter(command);
			DataSet			dataSet = new DataSet(this.schemaName);

			try
			{
				adapter.Fill(dataSet, this.schemaName);
			}
			catch (Exception ex)
			{
				throw new FbException(ex.Message);
			}
			finally
			{
				adapter.Dispose();
				command.Dispose();
			}

			TrimStringFields(dataSet.Tables[this.schemaName]);

			return this.ProcessResult(dataSet.Tables[this.schemaName]);
		}

		#endregion

		#region Protected Methods

		protected FbCommand BuildCommand(FbConnection connection, object[] restrictions)
		{
			DataView collections = FbMetaDataCollections.GetSchema().DefaultView;
			collections.RowFilter = "CollectionName = '" + this.schemaName + "'";

			if (collections.Count == 0)
			{
				throw new NotSupportedException("Unsupported collection name.");
			}

			if (restrictions != null &&
				restrictions.Length > (int)collections[0]["NumberOfRestrictions"])
			{
				throw new InvalidOperationException("The number of specified restrictions is not valid.");
			}

			DataView restriction = FbRestrictions.GetSchema().DefaultView;
			restriction.RowFilter = "CollectionName = '" + this.schemaName + "'";

			if (restriction.Count != (int)collections[0]["NumberOfRestrictions"])
			{
				throw new InvalidOperationException("Incorrect restriction definitions.");
			}

			StringBuilder builder = this.GetCommandText(restrictions);
			FbCommand schema = connection.CreateCommand();

			schema.CommandText = builder.ToString();

			if (connection.InnerConnection.HasActiveTransaction)
			{
				schema.Transaction = connection.InnerConnection.ActiveTransaction;
			}

			if (restrictions != null && restrictions.Length > 0)
			{
				// Add parameters
				int index = 0;

				for (int i = 0; i < restrictions.Length; i++)
				{
					string rname = restriction[i]["RestrictionDefault"].ToString().ToLower(CultureInfo.CurrentCulture);
					if (restrictions[i] != null &&
						!rname.EndsWith("_catalog") &&
						!rname.EndsWith("_schema") &&
						rname != "table_type")
					{
						string pname = String.Format(CultureInfo.CurrentCulture, "@p{0}", index++);

						FbParameter p = schema.Parameters.Add(pname, restrictions[i].ToString());
						p.FbDbType = FbDbType.VarChar;
						p.Size = 255;
					}
				}
			}

			return schema;
		}

		protected virtual DataTable ProcessResult(DataTable schema)
		{
			return schema;
		}

		protected virtual object[] ParseRestrictions(object[] restrictions)
		{
			return restrictions;
		}

		#endregion

		#region Private	Static Methods

		private static void TrimStringFields(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				for (int i = 0; i < schema.Columns.Count; i++)
				{
					if (schema.Columns[i].DataType == typeof(System.String))
					{
						row[schema.Columns[i]] = row[schema.Columns[i]].ToString().Trim();
					}
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();
		}

		#endregion
	}
}
