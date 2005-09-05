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
	internal class FbCharacterSets : FbDbSchema
	{
		#region Constructors

		public FbCharacterSets() : base("CharacterSets")
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
					"null AS CHARACTER_SET_CATALOG, " +
					"null AS CHARACTER_SET_SCHEMA, " +
					"rdb$character_set_name AS CHARACTER_SET_NAME, " +
					"rdb$character_set_id AS CHARACTER_SET_ID, " +
					"rdb$default_collate_name AS DEFAULT_COLLATION," +
					"rdb$bytes_per_character AS BYTES_PER_CHARACTER, " +
					"rdb$description AS DESCRIPTION " +
				 "FROM " +
					"rdb$character_sets");

			if (restrictions != null)
			{
				int index = 0;

				/* CHARACTER_SET_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* CHARACTER_SET_SCHEMA	*/
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* CHARACTER_SET_NAME */
				if (restrictions.Length >= 3 && restrictions[2] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, "rdb$character_set_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY rdb$character_set_name");

			return sql;
		}

		#endregion
	}
}