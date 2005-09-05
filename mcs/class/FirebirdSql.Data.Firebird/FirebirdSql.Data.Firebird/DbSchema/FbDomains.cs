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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbDomains : FbDbSchema
	{
		#region Constructors

		public FbDomains() : base("Domains")
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
					"null AS DOMAIN_CATALOG, " +
					"null AS DOMAIN_SCHEMA, " +
					"fld.rdb$field_name AS DOMAIN_NAME, " +
					"null AS DOMAIN_DATA_TYPE, " +
					"rdb$field_sub_type AS DOMAIN_SUB_TYPE, " +
					"cast(fld.rdb$field_length AS integer) AS DOMAIN_SIZE, " +
					"cast(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION, " +
					"cast(fld.rdb$field_scale  AS integer) AS NUMERIC_SCALE, " +
                    "cast(fld.rdb$character_length AS integer) AS \"CHARACTER_LENGTH\", " +
					"cast(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH, " +
					"fld.rdb$null_flag AS IS_NULLABLE, " +
					"fld.rdb$dimensions AS ARRAY_DIMENSIONS, " +
					"fld.rdb$description AS DESCRIPTION, " +
					"fld.rdb$field_type AS FIELD_TYPE, " +
					"null AS CHARACTER_SET_CATALOG, " +
					"null AS CHARACTER_SET_SCHEMA, " +
					"cs.rdb$character_set_name AS CHARACTER_SET_NAME, " +
					"null AS COLLATION_CATALOG, " +
					"null AS COLLATION_SCHEMA, " +
					"coll.rdb$collation_name AS COLLATION_NAME " +
				"FROM " +
					"rdb$fields fld " +
					"left join rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id " +
					"left join rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)");

			where.Append("not rdb$field_name starting with 'RDB$'");

			if (restrictions != null)
			{
				int index = 0;

				/* DOMAIN_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* DOMAIN_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* DOMAIN_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " AND rdb$field_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rdb$field_name");

			return sql;
		}

		protected override DataTable ProcessResult(DataTable schema)
		{
			schema.BeginLoadData();

			foreach (DataRow row in schema.Rows)
			{
				int blrType = Convert.ToInt32(row["FIELD_TYPE"], CultureInfo.InvariantCulture);

				int subType = 0;
				if (row["DOMAIN_SUB_TYPE"] != System.DBNull.Value)
				{
					subType = Convert.ToInt32(row["DOMAIN_SUB_TYPE"], CultureInfo.InvariantCulture);
				}

				int scale = 0;
				if (row["NUMERIC_SCALE"] != System.DBNull.Value)
				{
					scale = Convert.ToInt32(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
				}

				FbDbType dbType = (FbDbType)TypeHelper.GetDbDataType(blrType, subType, scale);
				row["DOMAIN_DATA_TYPE"] = TypeHelper.GetDataTypeName((DbDataType)dbType).ToLower(CultureInfo.CurrentCulture);

				if (dbType == FbDbType.Char || dbType == FbDbType.VarChar)
				{
					row["DOMAIN_SIZE"] = row["CHARACTER_LENGTH"];
				}
				else
				{
					row["CHARACTER_OCTET_LENGTH"] = 0;
				}

				if (dbType == FbDbType.Binary || dbType == FbDbType.Text)
				{
					row["DOMAIN_SIZE"] = Int32.MaxValue;
				}

				if (row["NUMERIC_PRECISION"] == System.DBNull.Value &&
				   (dbType == FbDbType.Decimal || dbType == FbDbType.Numeric))
				{
					row["NUMERIC_PRECISION"] = row["DOMAIN_SIZE"];
				}

				row["NUMERIC_SCALE"] = (-1) * scale;

				if (row["IS_NULLABLE"] == DBNull.Value)
				{
					row["IS_NULLABLE"] = true;
				}
				else
				{
					row["IS_NULLABLE"] = false;
				}
			}

			schema.EndLoadData();
			schema.AcceptChanges();

			// Remove not more needed columns
			schema.Columns.Remove("FIELD_TYPE");
			schema.Columns.Remove("CHARACTER_LENGTH");

			return schema;
		}

		#endregion
	}
}