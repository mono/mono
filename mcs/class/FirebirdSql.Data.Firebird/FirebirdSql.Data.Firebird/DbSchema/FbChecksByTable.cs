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
using System.Text;
using System.Globalization;

namespace FirebirdSql.Data.Firebird.DbSchema
{
	internal class FbChecksByTable : FbDbSchema
	{
		#region Constructors

		public FbChecksByTable() : base("CheckConstraintsByTable")
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
					"null AS CONSTRAINT_CATALOG, " +
					"null AS CONSTRAINT_SCHEMA, " +
					"chktb.rdb$constraint_name AS CONSTRAINT_NAME, " +
					"chktb.rdb$relation_name AS TABLE_NAME, " +
					"trig.rdb$trigger_source AS CHECK_CLAUSULE, " +
					"trig.rdb$description AS DESCRIPTION " +
				"FROM rdb$relation_constraints chktb " +
					"left join rdb$check_constraints chk ON (chktb.rdb$constraint_name = chk.rdb$constraint_name AND chktb.rdb$constraint_type = 'CHECK') " +
					"left join rdb$triggers trig ON chk.rdb$trigger_name = trig.rdb$trigger_name");

			if (restrictions != null)
			{
				int index = 0;

				/* CONSTRAINT_CATALOG */
				if (restrictions.Length >= 1 && restrictions[0] != null)
				{
				}

				/* CONSTRAINT_SCHEMA */
				if (restrictions.Length >= 2 && restrictions[1] != null)
				{
				}

				/* CONSTRAINT_NAME */
				if (restrictions.Length >= 3 && restrictions[1] != null)
				{
					where.AppendFormat(CultureInfo.CurrentCulture, "chktb.rdb$constraint_name = @p{0}", index++);
				}
			}

			if (where.Length > 0)
			{
				sql.AppendFormat(CultureInfo.CurrentCulture, " WHERE {0} ", where.ToString());
			}

			sql.Append(" ORDER BY chktb.rdb$relation_name, chktb.rdb$constraint_name");

			return sql;
		}

		protected override object[] ParseRestrictions(object[] restrictions)
		{
			object[] parsed = restrictions;

			if (parsed != null)
			{
				if (parsed.Length == 7 && parsed[6] != null)
				{
					switch (parsed[6].ToString().ToUpper(CultureInfo.CurrentCulture))
					{
						case "UNIQUE":
							parsed[3] = "u";
							break;

						case "PRIMARY KEY":
							parsed[3] = "p";
							break;

						case "FOREIGN KEY":
							parsed[3] = "f";
							break;

						case "CHECK":
							parsed[3] = "c";
							break;
					}
				}
			}

			return parsed;
		}

		#endregion
	}
}