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
			AllowDBNull=false;
			MaxLength=0;
			Digits=0;
			Value=null;
		}

		internal Type DataType
		{
			get
			{
				switch (OdbcType)
				{
					case OdbcType.TinyInt:
						return typeof(System.Byte);
					case OdbcType.BigInt: 
						return typeof(System.Int64);
					case OdbcType.Image:
					case OdbcType.VarBinary:
					case OdbcType.Binary:
						return typeof(byte[]);
					case OdbcType.Bit:
						return typeof(bool);
					case OdbcType.NChar:
					case OdbcType.Char:
						return typeof(char);
					case OdbcType.Time:
					case OdbcType.Timestamp:
					case OdbcType.DateTime:
					case OdbcType.Date:
					case OdbcType.SmallDateTime:
						return typeof(DateTime);
					case OdbcType.Decimal:
						return typeof(Decimal);
					case OdbcType.Numeric:
					case OdbcType.Double:
						return typeof(Double);
					case OdbcType.Int:
						return typeof(System.Int32);
					case OdbcType.Text:
					case OdbcType.NText:
					case OdbcType.NVarChar:
					case OdbcType.VarChar:
						return typeof(string);
					case OdbcType.Real:
						return typeof(float);
					case OdbcType.SmallInt:
						return typeof(System.Int16);
					case OdbcType.UniqueIndetifier:
						return typeof(Guid);
				}
				throw new InvalidCastException();
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
