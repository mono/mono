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

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbTriggers : FbDbSchema
	{
		#region Constructors

		public FbTriggers() : base("Triggers")
		{
		}

		#endregion

		#region Protected Methods

		protected override StringBuilder GetCommandText(object[] restrictions)
		{
			StringBuilder sql = new StringBuilder();
			StringBuilder where = new StringBuilder();

			sql.Append(
				@"SELECT " +
					"null AS TABLE_CATALOG, " +
					"null AS TABLE_SCHEMA, " +
					"rdb$relation_name AS TABLE_NAME, " +
					"rdb$trigger_name AS TRIGGER_NAME, " +
					"rdb$system_flag AS IS_SYSTEM_TRIGGER, " +
					"rdb$trigger_type AS TRIGGER_TYPE, " +
					"rdb$trigger_inactive AS IS_INACTIVE, " +
					"rdb$trigger_sequence AS SEQUENCE, " +
					"rdb$trigger_source AS SOURCE, " +
					"rdb$description AS DESCRIPTION " +
				"FROM " +
					"rdb$triggers");

			if (restrictions != null)
			{
				int index = 0;

				/* TABLE_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* TABLE_SCHEMA	*/
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* TABLE_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, "rdb$relation_name = @p{0}", index++);
				}

				/* TRIGGER_NAME	*/
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
					{
						where.Append(" AND ");
					}

					where.AppendFormat(CultureInfo.CurrentCulture, "rdb$trigger_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rdb$relation_name, rdb$trigger_name");

			return sql;
		}

		protected override DataTable ProcessResult(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				if (row["IS_SYSTEM_TRIGGER"] == DBNull.Value ||
					Convert.ToInt32(row["IS_SYSTEM_TRIGGER"], CultureInfo.InvariantCulture) == 0)
				{
					row["IS_SYSTEM_TRIGGER"] = false;
				}
				else
				{
					row["IS_SYSTEM_TRIGGER"] = true;
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();

			return schema;
		}

		#endregion
	}
}