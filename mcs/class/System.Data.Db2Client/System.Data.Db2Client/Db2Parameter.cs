using System;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.Db2Client
{

	public sealed class Db2Parameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
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
		IntPtr DataValPtr = IntPtr.Zero;
		byte[] bDataVal;
		private int iDataVal;
		private byte scale, precision;
		private int size;
		bool selfDescribe = false;

		#region Contructors and destructors
		public Db2Parameter()
		{
		}
		public Db2Parameter (string name)
		{
			parameterName = name;
		}

		public Db2Parameter(string name, DbType type)
		{
			parameterName = name;
			dbType = type;
		}
		public Db2Parameter(string name, object data)
		{
			parameterName = name;
			this.Value = data;
		}
		public Db2Parameter(string name, DbType type, string columnName)
		{
			parameterName = name;
			dbType = type;
			sourceColumn = columnName;
		}
		public Db2Parameter(string name, int db2Type)
		{
			parameterName = name;
			dbType = inferTypeFromDb2Type(db2Type);
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
				return dataVal;
			}
			set 
			{
				this.dataVal = value;
				DbType = inferType(dataVal);
				if (dbType==DbType.Int32)
					iDataVal=(int) value;
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
		#region inferTypeFromDb2Type
		///
		/// Determine the DbType from the SQL type returned by SQLDescribeParam
		/// 
		private DbType inferTypeFromDb2Type(int db2Type)
		{
			switch (db2Type) 
			{
				case Db2Constants.SQL_CHAR:
					return DbType.AnsiString;
				case Db2Constants.SQL_NUMERIC:
				case Db2Constants.SQL_DECIMAL:
					return DbType.Decimal;
				case Db2Constants.SQL_DATETIME:
					return DbType.DateTime;
				case Db2Constants.SQL_FLOAT:
				case Db2Constants.SQL_DOUBLE:
					return DbType.Double;
				case Db2Constants.SQL_INTEGER:
					return DbType.Int32;
				case Db2Constants.SQL_SMALLINT:
					return DbType.Int16;
				case Db2Constants.SQL_VARCHAR:
					return DbType.String;
				case Db2Constants.SQL_USER_DEFINED_TYPE:
					return DbType.Object;
				default:
					throw new SystemException("Db2 Data type is unknown.");
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

			sqlRet = Db2CLIWrapper.SQLDescribeParam(hwndStmt, paramNum, ref db2DataType, ref paramSize, ref decimalDigits, ref nullable);
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
			IntPtr StrLen_or_IndPtr = IntPtr.Zero;
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
			
			
			
			sqlRet = Db2CLIWrapper.SQLDescribeParam(hwndStmt, paramNum, DataTypePtr, ParameterSizePtr, DecimalDigitsPtr, NullablePtr);
			
			int _dataType = (int)Marshal.PtrToStructure(DataTypePtr, typeof(int));
			int _parameterSize = (int)Marshal.PtrToStructure(ParameterSizePtr, typeof(int));
			int _decimalDigits = (int)Marshal.PtrToStructure(DecimalDigitsPtr, typeof(int));
			int _nullable = (int)Marshal.PtrToStructure(NullablePtr, typeof(int));
			
			switch ((int)db2DataType) 
			{
					//These types are treated as SQL_C_CHAR for binding purposes
				case Db2Constants.SQL_TYPE_DATE:
				case Db2Constants.SQL_TYPE_TIME:
				case Db2Constants.SQL_TYPE_TIMESTAMP:
				case Db2Constants.SQL_VARCHAR:
				case Db2Constants.SQL_CHAR:
					paramSize = new IntPtr(_parameterSize);
					if(ParameterDirection.Output == direction){
						sqlRet = Db2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), Db2Constants.SQL_C_DEFAULT, 
							(short)db2DataType,  Convert.ToUInt32((int)paramSize) , 
							(short) _decimalDigits, DataValPtr, _parameterSize, StrLen_or_IndPtr);
					}
					else{
						sqlRet = Db2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), Db2Constants.SQL_C_DEFAULT, 
							(short)db2DataType,  Convert.ToUInt32((int)paramSize) , 
							(short) _decimalDigits, bDataVal, _parameterSize, StrLen_or_IndPtr);
					}
					break;
				case Db2Constants.SQL_DECIMAL:
					if(ParameterDirection.Output == direction){
						sqlRet = Db2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), Db2Constants.SQL_C_DEFAULT, 
							(short)db2DataType,  Convert.ToUInt32((int)paramSize + _decimalDigits) , 
							(short) _decimalDigits, DataValPtr, _parameterSize, StrLen_or_IndPtr);
					}
					else{
						sqlRet = Db2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, 
							ConvertParameterDirection(direction), Db2Constants.SQL_C_DEFAULT, 
							(short)db2DataType,  Convert.ToUInt32((int)paramSize + _decimalDigits) , 
							(short) _decimalDigits, bDataVal, _parameterSize, StrLen_or_IndPtr);
					}
					break;
				
				
				default:
					sqlRet = Db2CLIWrapper.SQLBindParameter(hwndStmt, (ushort)paramNum, ConvertParameterDirection(direction), Db2Constants.SQL_C_DEFAULT, (short)db2DataType,  Convert.ToUInt32((int)paramSize) , 0, ref iDataVal, 0, 0);
					break;
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
					return Db2Constants.SQL_PARAM_INPUT;
				case(ParameterDirection.Output):
					return Db2Constants.SQL_PARAM_OUTPUT;
				default:
					return Db2Constants.SQL_PARAM_INPUT;
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
			switch ((int)db2DataType) 
			{
				case Db2Constants.SQL_INTEGER:
					dataVal = Marshal.PtrToStructure(DataValPtr, typeof(int));
					break;
				case Db2Constants.SQL_VARCHAR:
					
					dataVal = Marshal.PtrToStringAnsi(DataValPtr);
					break;
				case Db2Constants.SQL_DECIMAL:
					//dataVal = Marshal.PtrToStructure(bDataVal, typeof(decimal));
					dataVal = Marshal.PtrToStringAnsi(DataValPtr);
					
					break;
				default:
					throw new Db2Exception("Unknown data type");
			}
		}
	}
}
