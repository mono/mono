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
	internal class FbUniqueKeys : FbDbSchema
	{
		#region Constructors

		public FbUniqueKeys() : base("UniqueKeys")
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
					"rel.rdb$relation_name AS TABLE_NAME, " +
					"seg.rdb$field_name AS COLUMN_NAME, " +
					"seg.rdb$field_position AS ORDINAL_POSITION, " +
					"rel.rdb$constraint_name AS UK_NAME " +
				"FROM " +
					"rdb$relation_constraints rel " +
					"left join rdb$indices idx ON rel.rdb$index_name = idx.rdb$index_name " +
					"left join rdb$index_segments seg ON idx.rdb$index_name = seg.rdb$index_name");

			where.Append("rel.rdb$constraint_type = 'UNIQUE'");

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
					where.AppendFormat(CultureInfo.CurrentCulture, " AND rel.rdb$relation_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rel.rdb$relation_name, rel.rdb$constraint_name, seg.rdb$field_position");

			return sql;
		}

		#endregion
	}
}