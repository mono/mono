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
using System.Globalization;
using System.Data;
using System.Text;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbCollations : FbDbSchema
	{
		#region Constructors

		public FbCollations() : base("Collations")
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
					"null AS COLLATION_CATALOG, " +
					"null AS COLLATION_SCHEMA, " +
					"coll.rdb$collation_name AS COLLATION_NAME, " +
					"cs.rdb$character_set_name AS CHARACTER_SET_NAME, " +
					"coll.rdb$description AS DESCRIPTION " +
				"FROM rdb$collations coll " +
					"left join rdb$character_sets cs ON coll.rdb$character_set_id = cs.rdb$character_set_id"
				);

			if (restrictions != null)
			{
				int index = 0;

				/* COLLATION_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* COLLATION_SCHEMA	*/
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* COLLATION_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, "coll.rdb$collation_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY cs.rdb$character_set_name, coll.rdb$collation_name");

			return sql;
		}

		#endregion
	}
}