//
// System.Data.OleDb.OleDbSchemaGuid
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbSchemaGuid
	{
		#region Fields

		public static readonly Guid Assertions;
		public static readonly Guid Catalogs;
		public static readonly Guid Character_Sets;
		public static readonly Guid Check_Constraints;
		public static readonly Guid Check_Constraints_By_Table;
		public static readonly Guid Collations;
		public static readonly Guid Columns;
		public static readonly Guid Column_Domain_Usage;
		public static readonly Guid Column_Privileges;
		public static readonly Guid Constraint_Column_Usage;
		public static readonly Guid Constraint_Table_Usage;
		public static readonly Guid DbInfoLiterals;
		public static readonly Guid Foreign_Keys;
		public static readonly Guid Indexes;
		public static readonly Guid Key_Column_Usage;
		public static readonly Guid Primary_Keys;
		public static readonly Guid Procedures;
		public static readonly Guid Procedure_Columns;
		public static readonly Guid Procedure_Parameters;
		public static readonly Guid Provider_Types;
		public static readonly Guid Referential_Constraints;
		public static readonly Guid Schemata;
		public static readonly Guid Sql_Languages;
		public static readonly Guid Statistics;
		public static readonly Guid Tables;
		public static readonly Guid Tables_Info;
		public static readonly Guid Table_Constraints;
		public static readonly Guid Table_Privileges;
		public static readonly Guid Table_Statistics;
		public static readonly Guid Translations;
		public static readonly Guid Trustee;
		public static readonly Guid Usage_Privileges;
		public static readonly Guid Views;
		public static readonly Guid View_Column_Usage;
		public static readonly Guid View_Table_Usage;

		#endregion

		#region Constructors

		[MonoTODO]
		public OleDbSchemaGuid ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
