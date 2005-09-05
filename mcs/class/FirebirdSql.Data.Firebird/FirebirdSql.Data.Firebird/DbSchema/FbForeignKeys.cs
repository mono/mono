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
	internal class FbForeignKeys : FbDbSchema
	{
		#region Constructors

		public FbForeignKeys() : base("ForeignKeys")
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
					"null AS PK_TABLE_CATALOG, " +
					"null AS PK_TABLE_SCHEMA, " +
					"pk.rdb$relation_name AS PK_TABLE_NAME, " +
					"pkseg.rdb$field_name AS PK_COLUMN_NAME, " +
					"pk.rdb$constraint_name AS PK_NAME, " +
					"null AS FK_TABLE_CATALOG, " +
					"null AS FK_TABLE_SCHEMA, " +
					"fk.rdb$relation_name AS FK_TABLE_NAME, " +
					"fkseg.rdb$field_name AS FK_COLUMN_NAME, " +
					"fk.rdb$constraint_name AS FK_NAME, " +
					"pkseg.rdb$field_position AS ORDINAL_POSITION, " +
					"ref.rdb$match_option AS MATCH_OPTION, " +
					"ref.rdb$update_rule AS UPDATE_RULE, " +
					"ref.rdb$delete_rule AS DELETE_RULE, " +
					"fk.rdb$deferrable AS IS_DEFERRABLE, " +
					"fk.rdb$initially_deferred AS INITIALLY_DEFERRED " +
				"FROM " +
					"rdb$relation_constraints fk, " +
					"rdb$index_segments fkseg, " +
					"rdb$relation_constraints pk, " +
					"rdb$index_segments pkseg, " +
					"rdb$ref_constraints ref ");

			where.Append(
					"fk.rdb$constraint_name = ref.rdb$constraint_name and " +
					"fk.rdb$index_name = fkseg.rdb$index_name and " +
					"pk.rdb$constraint_name = ref.rdb$const_name_uq and " +
					"pk.rdb$index_name = pkseg.rdb$index_name and " +
					"pkseg.rdb$field_position = fkseg.rdb$field_position ");

			if (restrictions != null)
			{
				int index = 0;

				/* PK_TABLE_CATALOG	*/
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* PK_TABLE_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* PK_TABLE_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " and pk.rdb$relation_name = @p{0}", index++);
				}

				/* FK_TABLE_CATALOG	*/
				if (restrictions.Length >= 4 && restrictions[3] != null)
				{
				}

				/* FK_TABLE_SCHEMA */
				if (restrictions.Length >= 5 && restrictions[4] != null)
				{
				}

				/* FK_TABLE_NAME */
				if (restrictions.Length >= 6 && restrictions[5] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, " and fk.rdb$relation_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY fk.rdb$constraint_name, pk.rdb$relation_name, pkseg.rdb$field_position");

			return sql;
		}

		#endregion
	}
}