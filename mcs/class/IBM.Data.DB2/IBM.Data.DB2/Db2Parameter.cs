using System;
using System.Data;
using System.Runtime.InteropServices;

namespace IBM.Data.DB2
{

	public sealed class DB2Parameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		private DbType dbType;
		private IntPtr DB2DataType;
		private ParameterDirection direction;
		private bool nullable;
		private string parameterName;
		private string sourceColumn;
		private IntPtr paramSize;
		private IntPtr decimalDigits;
		private DataRowVersion sourceVersion;
		object dataVal;
		IntPtr DataValPtr = IntPtr.Zero;
		byte[] bDataVal;
		private int iDataVal;
		private double dblDataVal;
		private byte scale, precision;
		private int size;
		//bool selfDescribe = false;
		
		IntPtr StrLen_or_IndPtr = IntPtr.Zero;

		#region Contructors and destructors
		public DB2Parameter()
		{
		}
		public DB2Parameter (string name)
		{
			parameterName = name;
		}

		public DB2Parameter(string name, DbType type)
		{
			parameterName = name;
			dbType = type;
			int DB2Type = inferDB2TypeFromType(type);
			DB2DataType = new IntPtr(DB2Type);
		}
		public DB2Parameter(string name, object data)
		{
			parameterName = name;
			dbType = inferType(data);
			int DB2Type = inferDB2TypeFromType(dbType);
			DB2DataType = new IntPtr(DB2Type);
			this.Value = data;
		}
		public DB2Parameter(string name, DbType type, string columnName)
		{
			parameterName = name;
			dbType = type;
			sourceColumn = columnName;
		}
		public DB2Parameter(string name, int DB2Type)
		{
			parameterName = name;
			dbType = inferTypeFromDB2Type(DB2Type);
			DB2DataType = new IntPtr(DB2Type);
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
				return dataVal;
			}
			set 
			{
				this.dataVal = value;
				DbType = inferType(dataVal);
				if (dbType==DbType.Int32){
					iDataVal=(int) value;
					//DataValPtr = new IntPtr((int)value);
				}
				else
				{
					if (bDataVal==null || bDataVal.Length< (((int)paramSize>20)?(int)paramSize:20) )
						bDataVal=new byte[((int)paramSize>20)?(int)paramSize:20];
					else
						bDataVal.Initialize();
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
		private DbType inferTypeFromDB2Type(int DB2Type)
		{
			switch (DB2Type) 
			{
				case DB2Constants.SQL_CHAR:
					return DbType.AnsiString;
				case DB2Constants.SQL_NUMERIC:
				case DB2Constants.SQL_DECIMAL:
					return DbType.Decimal;
				case DB2Constants.SQL_DATETIME:
					return DbType.DateTime;
				case DB2Constants.SQL_FLOAT:
				case DB2Constants.SQL_DOUBLE:
					return DbType.Double;
				case DB2Constants.SQL_INTEGER:
					return DbType.Int32;
				case DB2Constants.SQL_SMALLINT:
					return DbType.Int16;
				case DB2Constants.SQL_VARCHAR:
					return DbType.String;
				case DB2Constants.SQL_USER_DEFINED_TYPE:
					return DbType.Object;
				default:
					throw new SystemException("DB2 Data type is unknown.");
			}
		}
		#endregion
		
		private int inferDB2TypeFromType(DbType _dbType)
		{
			switch (_dbType) 
			{
				case DbType.AnsiString:
					return DB2Constants.SQL_CHAR;
				case DbType.Decimal:
					return DB2Constants.SQL_DECIMAL;
				case DbType.DateTime:
					return DB2Constants.SQL_DATETIME;
				case DbType.Double:
					return DB2Constants.SQL_DOUBLE;
				case DbType.Int32:
					return DB2Constants.SQL_INTEGER;
				case DbType.Int16:
					return DB2Constants.SQL_SMALLINT;
				case DbType.String:
					return DB2Constants.SQL_VARCHAR;
				case DbType.Object:
					return DB2Constants.SQL_USER_DEFINED_TYPE;
				default:
					throw new SystemException("DB2 Data type is unknown.");
			}
		}
		
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

			sqlRet = DB2CLIWrapper.SQLDescribeParam(hwndStmt, paramNum, ref DB2DataType, ref paramSize, ref decimalDigits, ref nullable);
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
			
			if(IntPtr.Zero == DataValPtr){
				InitMem(20, ref DataValPtr);
			}
			
			IntPtr DataTypePtr = IntPtr.Zero;
			InitMem(4, ref DataTypePtr);
			
			IntPtr ParameterSizePtr = IntPtr.Zero;
			InitMem(4, ref ParameterSizePtr);
			
			IntPtr DecimalDigitsPtr = IntPtr.Zero;
			InitMem(4, ref DecimalDigitsPtr);
			
			
			IntPtr NullablePtr = IntPtr.Zero;
			InitMem(4, ref NullablePtr);
			
			IntPtr StrLen_or_IndPtr = IntPtr.Zero;
			InitMem(4, ref StrLen_or_IndPtr);
			
			sqlRet = DB2CLIWrapper.SQLDescribeParam(hwndStmt, paramNum, DataTypePtr, ParameterSizePtr, DecimalDigitsPtr, NullablePtr);
			
			int _dataType = (int)Marshal.PtrToStructure(DataTypePtr, typeof(int));
			int _parameterSize = (int)Marshal.PtrToStructure(ParameterSizePtr, typeof(int));
			int _decimalDigits = (int)Marshal.PtrToStructure(DecimalDigitsPtr, typeof(int));
			int _nullable = (int)Marshal.PtrToStructure(NullablePtr, typeof(int));
			paramSize = new IntPtr(_parameterSize);
			switch ((int)DB2DataType) 
			{
					//These types are treated as SQL_C_CHAR for binding purposes
				case DB2Constants.SQL_DATETIME:
				case DB2Constants.SQL_TYPE_DATE:
				case DB2Constants.SQL_TYPE_TIME:
				case DB2Constants.SQL_TYPE_TIMESTAMP:
				case DB2Constants.SQL_VARCHAR:
				case DB2Constants.SQL_CHAR:
				case DB2Constants.SQL_DECIMAL:
					
					if(ParameterDirection.Output == direction || ParameterDirection.InputOutput == direction){
						sqlRet = DB2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), DB2Constants.SQL_C_DEFAULT, 
							(short)DB2DataType,  Convert.ToUInt32((int)paramSize + _decimalDigits) , 
							(short) _decimalDigits,  DataValPtr, _parameterSize, StrLen_or_IndPtr);
					}
					else{
						sqlRet = DB2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), DB2Constants.SQL_C_DEFAULT, 
							(short)DB2DataType,  Convert.ToUInt32((int)paramSize + _decimalDigits) , 
							(short) _decimalDigits, bDataVal, _parameterSize, StrLen_or_IndPtr);
					}
					break;
				
				case DB2Constants.SQL_INTEGER:	
					sqlRet = DB2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), DB2Constants.SQL_C_DEFAULT, 
							(short)DB2DataType,  Convert.ToUInt32((int)paramSize), 
							(short)_decimalDigits,  ref iDataVal, _parameterSize, StrLen_or_IndPtr);
					break;
				case DB2Constants.SQL_DOUBLE:	
					sqlRet = DB2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), DB2Constants.SQL_C_DEFAULT, 
							(short)DB2DataType,  Convert.ToUInt32((int)paramSize), 
							(short)_decimalDigits,  ref dblDataVal, _parameterSize, StrLen_or_IndPtr);
					break;
							
				default:
					throw new DB2Exception ("Unknown data type");
			}		
			return sqlRet;
		}
		#endregion
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}
		
		public short ConvertParameterDirection(ParameterDirection direction){
			switch(direction){
				case(ParameterDirection.Input):
					return DB2Constants.SQL_PARAM_INPUT;
				case(ParameterDirection.Output):
					return DB2Constants.SQL_PARAM_OUTPUT;
				case(ParameterDirection.InputOutput):
					return DB2Constants.SQL_PARAM_INPUT_OUTPUT;
				default:
					return DB2Constants.SQL_PARAM_INPUT;
			}
		}
		
		private void InitMem(int memSize, ref IntPtr ptr){
			if (ptr.ToInt32() == 0){
				unsafe{
					fixed(byte* arr = new byte[memSize]){
						ptr = new IntPtr(arr); 
					}
				}
			}	
		}
		internal void GetOutValue(){
			
			switch ((int)DB2DataType) 
			{
				case DB2Constants.SQL_INTEGER:
					dataVal = iDataVal;
					break;
				case DB2Constants.SQL_DOUBLE:
					dataVal = dblDataVal;
					break;
				case DB2Constants.SQL_VARCHAR:
				case DB2Constants.SQL_CHAR:
				case DB2Constants.SQL_DECIMAL:
					dataVal = Marshal.PtrToStringAnsi(DataValPtr);
					break;
					
				case DB2Constants.SQL_DATETIME:
				case DB2Constants.SQL_TYPE_DATE:
				case DB2Constants.SQL_TYPE_TIME:
				case DB2Constants.SQL_TYPE_TIMESTAMP:
					//dataVal = Marshal.PtrToStringAnsi(DataValPtr);
					short year = Marshal.ReadInt16(DataValPtr, 0);
					short month = Marshal.ReadInt16(DataValPtr, 2);
					short day = Marshal.ReadInt16(DataValPtr, 4);
					dataVal = new DateTime(year, month, day);
					break;
					
				default:
					throw new DB2Exception("Unknown data type");
			}
		}
	}
}
