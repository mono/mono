using System;

namespace System.Data.Odbc
{
	/// <summary>
	/// Summary description for libodbchelper.
	/// </summary>
	internal class libodbchelper
	{
		public static void DisplayError(string Msg, OdbcReturn Ret)
		{
			if ((Ret!=OdbcReturn.Success) && (Ret!=OdbcReturn.SuccessWithInfo)) 
			{
				Console.WriteLine("ERROR: {0}: <{1}>",Msg,Ret);
		
			}
		}

		internal static Type ODBCTypeToCILType(OdbcType type)
		{
			switch (type)
			{
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
}
