// 
// Copyright (c) 2006 Mainsoft Co.
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.Data.OracleClient;

namespace MonoTests.System.Data.Utils.Data
{
	/// <summary>
	/// Represents a parameter type for use in tests of System.Data.
	/// </summary>
	public class DbTypeParameter
	{
		#region Members
		//Name of the Database type of this parameter.
		private string m_sDbTypeName;
		//Value of this parameter.
		private object m_oValue;
		//Size of this parameter.
		private int m_iSize;
		//Indicates wheather the size of this DbTypeParameter was initialized.
		private bool m_bIsSizeSet = false;
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor.
		/// </summary>
		public DbTypeParameter()
		{
		}

		/// <summary>
		/// Constructor, Initializes the DbTypeParameter's properties according to specified values.
		/// </summary>
		/// <param name="a_sTypeName">Specifies the initial parameter type Name for the DbTypeParameter.</param>
		/// <param name="a_oValue">Specifies the initial value for the DbTypeParameter.</param>
		public DbTypeParameter(string a_sTypeName, object a_oValue)
		{
			DbTypeName = a_sTypeName;
			Value = a_oValue;
		}
		/// <summary>
		/// Constructor, Initializes the DbTypeParameter's properties according to specified values.
		/// </summary>
		/// <param name="a_sTypeName">Specifies the initial parameter type Name for the DbTypeParameter.</param>
		/// <param name="a_oValue">Specifies the initial value for the DbTypeParameter.</param>
		/// <param name="a_iSize">Specifies the initial size for the DbTypeParameter.</param>
		public DbTypeParameter(string a_sTypeName, object a_oValue, int a_iSize)
		{
			DbTypeName = a_sTypeName;
			Value = a_oValue;
			Size = a_iSize;
		}
		#endregion

		#region Properties
		public string DbColumnName
		{
			get
			{
				return string.Format("T_{0}", m_sDbTypeName);
			}
		}

		public string ParameterName
		{
			get
			{
				return String.Format(":T_{0}", m_sDbTypeName);
			}
		}
		public string DbTypeName
		{
			get
			{
				return m_sDbTypeName;
			}
			set
			{
				m_sDbTypeName = value;
			}
		}

		public object Value
		{
			get
			{
				return m_oValue;
			}
			set
			{
				m_oValue = value;
			}
		}

		public int Size
		{
			get
			{
				if (IsSizeSet)
				{
					return m_iSize;
				}
				else
				{
					throw new InvalidOperationException("DbTypeParameter size was not set.");
				}
			}
			set
			{
				m_iSize = value;
				m_bIsSizeSet = true;
			}
		}
		public bool IsSizeSet
		{
			get
			{
				return m_bIsSizeSet;
			}
		}
		public DbType DbType
		{
			get
			{
				return GetDefaultDbType(DbTypeName);
			}
		}
		#endregion

		#region Methods
		public static DbType GetDefaultDbType(string dbTypeName)
		{
			switch (dbTypeName.ToUpper())
			{
				case "BIT":	//SQLServer.
					return DbType.Boolean;
				case "TINYINT":	//SQLServer.
					return DbType.Byte;
				case "SMALLINT":	//SQLServer & DB2.
					return DbType.Int16;
				case "INT":	//SQLServer.
					return DbType.Int32;
				case "INTEGER": //DB2
					return DbType.Int32;
				case "BIGINT":	//MSSQLServer &DB2
					return DbType.Int64;
				case "NUMERIC": //MSSQLServer.
					return DbType.Decimal;
				case "NUMBER": //Oracle.
					return DbType.VarNumeric;
			case "DECIMAL":	//MSSQLServer & DB2
				return DbType.Decimal;
				case "FLOAT":	//MSSQLServer & Oracle
						return DbType.Double;
				case "REAL": //MSSQLServer & DB2
					return DbType.Single;
				case "DOUBLE":
					return DbType.Double;
				case "CHAR":	//MSSQLServer & Oracle.
					return DbType.AnsiStringFixedLength;
				case "NCHAR": //MSSQLServer & Oracle.
					return DbType.AnsiStringFixedLength;
				case "VARCHAR": //MSSQLServer, Oracle & DB2.
					return DbType.AnsiString;
				case "NVARCHAR": //MSSQLServer & Oracle.
					return DbType.AnsiString;
				case "CHARACTER": //DB2
					return DbType.AnsiStringFixedLength;
				case "LONGVARCHAR": //DB2
					return DbType.String;
				case "LONG":	//Oracle.
					return DbType.AnsiString;
				default:
					throw new ApplicationException(string.Format("Dont know the default DbType for {0}.", dbTypeName));
			}
		}
		public object ApplyDefaultDataTransformation()
		{
			if (Value == DBNull.Value)
			{
				return DBNull.Value;
			}
			else if (Value.GetType() == typeof(bool))
			{
				return DefaultBooleanTransformation((bool)Value);
			}
			else if (Value.GetType() == typeof(byte))
			{
				return DefaultByteTransformation((byte)Value);
			}
			else if (Value.GetType() == typeof(Int16))
			{
				return DefaultInt16Transformation((Int16)Value);
			}
			else if (Value.GetType() == typeof(int))
			{
				return DefaultIntTransformation((int)Value);
			}
			else if (Value.GetType() == typeof(Int64))
			{
				return DefaultInt64Transformation((Int64)Value);
			}
			else if (Value.GetType() == typeof(decimal))
			{
				return DefaultDecimalTransformation((decimal)Value);
			}
			else if (Value.GetType() == typeof(double))
			{
				return DefaultDoubleTransformation((double)Value);
			}
			else if (Value.GetType() == typeof(float))
			{
				return DefaultFloatTransformation((float)Value);
			}
			else if (Value.GetType() == typeof(string))
			{
				return DefaultStringTransformation((string)Value);
			}
			else
			{
				throw new ApplicationException(string.Format("No default transformation for type {0}.", Value));
			}
		}
		public static bool DefaultBooleanTransformation(bool val)
		{
			return !val;
		}
		public static byte DefaultByteTransformation(byte val)
		{
			return (byte)(val*2);;
		}
		public static Int16 DefaultInt16Transformation(Int16 val)
		{
			return (Int16)(val*2);
		}
		public static int DefaultIntTransformation(int val)
		{
			return (int)(val*2);;
		}
		public static Int64 DefaultInt64Transformation(Int64 val)
		{
			return (Int64)(val*2);;
		}
		public static decimal DefaultDecimalTransformation(decimal val)
		{
			return (decimal)(val*2);;
		}
		public static double DefaultDoubleTransformation(double val)
		{
			return (double)(val*2);;
		}
		public static float DefaultFloatTransformation(float val)
		{
			return (float)(val*2);;
		}
		public static string DefaultStringTransformation(string val)
		{
			return val.ToUpper();;
		}
		/// <summary>
		/// Invalidates the size of this DbTypeParameter.
		/// </summary>
		public void InvalidateSize()
		{
			m_bIsSizeSet = false;
		}
		#endregion

	}
}
