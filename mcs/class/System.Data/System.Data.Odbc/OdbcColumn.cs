using System;

namespace System.Data.Odbc
{
	/// <summary>
	/// Summary description for OdbcColumn.
	/// </summary>
	internal class OdbcColumn
	{
		internal string ColumnName;
		internal OdbcType OdbcType;
		internal bool AllowDBNull;
		internal int MaxLength;
		internal int Digits;
		internal object Value;

		internal OdbcColumn(string Name, OdbcType Type)
		{
			this.ColumnName=Name;
			this.OdbcType=Type;
		}

		internal Type DataType
		{
			get
			{
				return libodbchelper.ODBCTypeToCILType(OdbcType);
			}
		}

		internal bool IsDateType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.Time:
					case OdbcType.Timestamp:
					case OdbcType.DateTime:
					case OdbcType.Date:
					case OdbcType.SmallDateTime:
						return true;
					default:
						return false;
				}
			}
		}

		internal bool IsStringType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.Text:
					case OdbcType.NText:
					case OdbcType.NVarChar:
					case OdbcType.VarChar:
						return true;
					default:
						return false;
				}
			}
		}

	}
}
