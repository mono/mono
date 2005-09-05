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
	internal class FbIndexes : FbDbSchema
	{
		#region Constructors

		public FbIndexes() : base("Indexes")
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
					"idx.rdb$relation_name AS TABLE_NAME, " +
					"idx.rdb$index_name AS INDEX_NAME, " +
					"idx.rdb$index_type AS INDEX_TYPE, " +
					"seg.rdb$field_name AS COLUMN_NAME, " +
					"seg.rdb$field_position AS ORDINAL_POSITION, " +
					"idx.rdb$index_inactive AS IS_INACTIVE, " +
					"idx.rdb$system_flag AS IS_SYSTEM_INDEX, " +
					"idx.rdb$unique_flag AS IS_UNIQUE, " +
					"idx.rdb$statistics AS INDEX_STATISTICS, " +
					"idx.rdb$description AS DESCRIPTION " +
				"FROM " +
					"rdb$indices idx " +
					"left join rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name");

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

				/* INDEX_NAME */
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
					if (where.Length > 0)
					{
						where.Append(" AND ");
					}

					where.AppendFormat(CultureInfo.CurrentCulture, "rdb$index_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY idx.rdb$relation_name, idx.rdb$index_name, seg.rdb$field_position");

			return sql;
		}

		protected override DataTable ProcessResult(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				if (row["IS_UNIQUE"] == DBNull.Value ||
					Convert.ToInt32(row["IS_UNIQUE"], CultureInfo.InvariantCulture) == 0)
				{
					row["IS_UNIQUE"] = true;
				}
				else
				{
					row["IS_UNIQUE"] = false;
				}
				
				if (row["IS_INACTIVE"] == DBNull.Value ||
					Convert.ToInt32(row["IS_INACTIVE"], CultureInfo.InvariantCulture) == 0)
				{
					row["IS_INACTIVE"] = true;
				}
				else
				{
					row["IS_INACTIVE"] = false;
				}

				if (row["IS_SYSTEM_INDEX"] == DBNull.Value ||
					Convert.ToInt32(row["IS_SYSTEM_INDEX"], CultureInfo.InvariantCulture) == 0)
				{
					row["IS_SYSTEM_INDEX"] = true;
				}
				else
				{
					row["IS_SYSTEM_INDEX"] = false;
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();
		
			return schema;
		}

		#endregion
	}
}