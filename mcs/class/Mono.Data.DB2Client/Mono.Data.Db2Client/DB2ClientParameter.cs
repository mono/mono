#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;

namespace DB2ClientCS
{
	/// <summary>
	/// Parameter object for DB2 client
	/// </summary>
	public sealed class DB2ClientParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		private DbType dbType;
		private IntPtr db2DataType;
		private ParameterDirection direction;
		private bool nullable;
		private string parameterName;
		private string sourceColumn;
		private IntPtr paramSize;
		private IntPtr decimalDigits;
		private DataRowVersion sourceVersion;
		object dataVal;
		byte[] bDataVal;
		private int iDataVal;
		private byte scale, precision;
		private int size;
		bool selfDescribe = false;

		#region Contructors and destructors
		public DB2ClientParameter()
		{
		}
		public DB2ClientParameter (string name)
		{
			parameterName = name;
		}

		public DB2ClientParameter(string name, DbType type)
		{
			parameterName = name;
			dbType = type;
		}
		public DB2ClientParameter(string name, object data)
		{
			parameterName = name;
			this.Value = data;
		}
		public DB2ClientParameter(string name, DbType type, string columnName)
		{
			parameterName = name;
			dbType = type;
			sourceColumn = columnName;
		}
		public DB2ClientParameter(string name, int db2Type)
		{
			parameterName = name;
			dbType = inferTypeFromDB2Type(db2Type);
			db2DataType = new IntPtr(db2Type);
		}
		#endregion
		#region Properties
		#region DbType Property
		///
		/// Parameter data type
		/// 
		public DbType DbType
		{
			get 
			{
				return dbType;
			}
			set 
			{
				dbType = value;
			}
		}
		#endregion
		#region Direction
		///
		/// In or out parameter, or both
		/// 
		public ParameterDirection Direction
		{
			get 
			{
				return direction;
			}
			set 
			{
				direction = value;
			}
		}
		#endregion
		#region IsNullable
		///
		/// Does this parameter support a null value
		/// 
		public bool IsNullable 
		{
			get 
			{
				return nullable;
			}
			set 
			{
				nullable = value;
			}
		}
		#endregion
		#region ParameterName
		public string ParameterName
		{
			get 
			{
				return parameterName;
			}
			set 
			{
				parameterName = value;
			}
		}
		#endregion
		#region SourceColumn
		///
		/// Gets or sets the name of the source column that is mapped to the DataSet
		/// 
		public string SourceColumn
		{
			get 
			{
				return sourceColumn;
			}
			set 
			{
				sourceColumn = value;
			}
		}
		#endregion
		#region SourceVersion
		///
		/// DataRowVersion property
		/// 
		public DataRowVersion SourceVersion 
		{
			get 
			{
				return sourceVersion;
			}
			set 
			{
				sourceVersion = value;
			}
		}
		#endregion
		#region IDbDataParameter properties
		public byte Precision 
		{
			get 
			{ 
				return precision; 
			}
			set 
			{ 
				precision = value; 
			}
		}
		
		public byte Scale 
		{
			get 
			{ 
				return scale; 
			}
			set 
			{ 
				scale = value; 
			}
		}
		
		public int Size 
		{
			get 
			{ 
				return size;
			}
			set 
			{ 
				size = value;
			}
		}
		#endregion
		#region Value
		///
		/// The actual parameter data
		/// 
		public object Value 
		{
			get
			{
				return Value;
			}
			set 
			{
				this.dataVal = value;
				DbType = inferType(dataVal);
				// Load buffer with new value
				if (dbType==DbType.Int32)
					iDataVal=(int) value;
				else
				{
					// Treat everything else as a string
					// Init string buffer
					if (bDataVal==null || bDataVal.Length< (((int)paramSize>20)?(int)paramSize:20) )
						bDataVal=new byte[((int)paramSize>20)?(int)paramSize:20];
					else
						bDataVal.Initialize();
					// Convert value into string and store into buffer
					byte[] strValueBuffer=System.Text.Encoding.ASCII.GetBytes(dataVal.ToString());
					strValueBuffer.CopyTo(bDataVal,0);
				}
			}
		}
		#endregion
		#endregion
		#region inferType Method
		/// <summary>
		/// Determine the data type based on the value
		/// </summary>
		private DbType inferType (object Data)
		{
			switch (Type.GetTypeCode(Data.GetType()))
			{
				case TypeCode.Empty:
					throw new SystemException("Invalid data type");

				case TypeCode.Object:
					return DbType.Object;

				case TypeCode.DBNull:
				case TypeCode.Char:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					// Throw a SystemException for unsupported data types.
					throw new SystemException("Invalid data type");

				case TypeCode.Boolean:
					return DbType.Boolean;

				case TypeCode.Byte:
					return DbType.Byte;

				case TypeCode.Int16:
					return DbType.Int16;

				case TypeCode.Int32:
					return DbType.Int32;

				case TypeCode.Int64:
					return DbType.Int64;

				case TypeCode.Single:
					return DbType.Single;

				case TypeCode.Double:
					return DbType.Double;

				case TypeCode.Decimal:
					return DbType.Decimal;

				case TypeCode.DateTime:
					return DbType.DateTime;

				case TypeCode.String:
					return DbType.String;

				default:
					throw new SystemException("Value is of unknown data type");
			}
		}
		#endregion
		#region inferTypeFromDB2Type
		///
		/// Determine the DbType from the SQL type returned by SQLDescribeParam
		/// 
		private DbType inferTypeFromDB2Type(int db2Type)
		{
			switch (db2Type) 
			{
				case DB2ClientConstants.SQL_CHAR:
					return DbType.AnsiString;
				case DB2ClientConstants.SQL_NUMERIC:
				case DB2ClientConstants.SQL_DECIMAL:
					return DbType.Decimal;
				case DB2ClientConstants.SQL_DATETIME:
					return DbType.DateTime;
				case DB2ClientConstants.SQL_FLOAT:
				case DB2ClientConstants.SQL_DOUBLE:
					return DbType.Double;
				case DB2ClientConstants.SQL_INTEGER:
					return DbType.Int32;
				case DB2ClientConstants.SQL_SMALLINT:
					return DbType.Int16;
				case DB2ClientConstants.SQL_VARCHAR:
					return DbType.String;
				case DB2ClientConstants.SQL_USER_DEFINED_TYPE:
					return DbType.Object;
				default:
					throw new SystemException("DB2 Data type is unknown.");
			}
		}
		#endregion
		#region Describe
		///
		/// Describe the parameter.  Use at the caller's discretion
		/// 
		public short Describe(IntPtr hwndStmt, short paramNum)
		{
			IntPtr nullable = IntPtr.Zero;
			paramSize = IntPtr.Zero;
			decimalDigits = IntPtr.Zero;
			short sqlRet = 0;

			sqlRet = DB2ClientPrototypes.SQLDescribeParam(hwndStmt, paramNum, ref db2DataType, ref paramSize, ref decimalDigits, ref nullable);
			return sqlRet;
		}
		#endregion
		#region Bind 
		///
		/// Bind this parameter
		/// 
		public short Bind(IntPtr hwndStmt, short paramNum)
		{
			short sqlRet = 0;

			switch ((int)db2DataType) 
			{
				case DB2ClientConstants.SQL_DECIMAL:	//These types are treated as SQL_C_CHAR for binding purposes
				case DB2ClientConstants.SQL_TYPE_DATE:
				case DB2ClientConstants.SQL_TYPE_TIME:
				case DB2ClientConstants.SQL_TYPE_TIMESTAMP:
				case DB2ClientConstants.SQL_VARCHAR:
				case DB2ClientConstants.SQL_CHAR:
					sqlRet = DB2ClientPrototypes.SQLBindParameter(hwndStmt, (ushort)paramNum, DB2ClientConstants.SQL_PARAM_INPUT, DB2ClientConstants.SQL_C_DEFAULT, (short)db2DataType,  Convert.ToUInt32((int)paramSize) , (short) decimalDigits, bDataVal, 0, 0);
					break;
				default:
					sqlRet = DB2ClientPrototypes.SQLBindParameter(hwndStmt, (ushort)paramNum, DB2ClientConstants.SQL_PARAM_INPUT, DB2ClientConstants.SQL_C_DEFAULT, (short)db2DataType,  Convert.ToUInt32((int)paramSize) , 0, ref iDataVal, 0, 0);
					break;
			}		
			return sqlRet;
		}
		#endregion
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}
