//
// OracleDataReader.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: Tim Coleman <tim@timcoleman.com>
//          Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Daniel Morgan, 2003, 2005
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient.Oci;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient {
	public sealed class OracleDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
	{
		#region Fields

		OracleCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		bool isClosed;
		bool hasRows;
		DataTable schemaTable;

		int recordsAffected = -1;
		OciStatementType statementType;
		OciStatementHandle statement;

		#endregion // Fields

		#region Constructors

		internal OracleDataReader (OracleCommand command, OciStatementHandle statement)
		{
			this.command = command;
			this.hasRows = false;
			this.isClosed = false;
			this.schemaTable = ConstructSchemaTable ();
			this.statement = statement;
			this.statementType = statement.GetStatementType ();
		}

		internal OracleDataReader (OracleCommand command, OciStatementHandle statement, bool extHasRows ) 
		{
			this.command = command;
			this.hasRows = extHasRows;
			this.isClosed = false;
			this.schemaTable = ConstructSchemaTable ();
			this.statement = statement;
			this.statementType = statement.GetStatementType ();
	        }


		~OracleDataReader ()
		{
			Dispose (false);
		}

		#endregion // Constructors

		#region Properties

		public int Depth {
			get { return 0; }
		}

		public int FieldCount {
			get { return statement.ColumnCount; }
		}

		public bool HasRows {
			get { return hasRows; }
		}

		public bool IsClosed {
			get { return isClosed; }
		}

		public object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}

		public object this [int i] {
			get { return GetValue (i); }
		}

		public int RecordsAffected {
			get { 
				if (statementType == OciStatementType.Select)
					return -1;
				else
					return GetRecordsAffected ();
			}
		}

		#endregion // Properties

		#region Methods

		public void Close ()
		{
			statement.Dispose();
			if (!isClosed) 
				command.CloseDataReader ();
			isClosed = true;
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = Type.GetType ("System.Boolean");
			Type stringType = Type.GetType ("System.String");
			Type intType = Type.GetType ("System.Int32");
			Type typeType = Type.GetType ("System.Type");
			Type shortType = Type.GetType ("System.Int16");

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("IsReadOnly", booleanType);
			schemaTable.Columns.Add ("BaseSchemaTable", stringType);
			schemaTable.Columns.Add ("BaseCatalogName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);

			return schemaTable;
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					schemaTable.Dispose ();
					Close ();
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public bool GetBoolean (int i)
		{
			throw new NotSupportedException ();
		}

		public byte GetByte (int i)
		{
			throw new NotSupportedException ();
		}

		public long GetBytes (int i, long fieldOffset, byte[] buffer2, int bufferoffset, int length)
		{
			object value = GetValue (i);
			if (!(value is byte[]))
				throw new InvalidCastException ();

                        if ( buffer2 == null )
				return ((byte []) value).Length; // Return length of data

                        // Copy data into buffer
                        long lobLength = ((byte []) value).Length;
                        if ( (lobLength - fieldOffset) < length)
                                length = (int) (lobLength - fieldOffset);
                        Array.Copy ( (byte[]) value, (int) fieldOffset, buffer2, bufferoffset, length);
                        return length; // return actual read count
                }

		public char GetChar (int i)
		{
			throw new NotSupportedException ();
		}

		public long GetChars (int i, long fieldOffset, char[] buffer2, int bufferoffset, int length)
		{
			object value = GetValue (i);
			if (!(value is char[]))
				throw new InvalidCastException ();
			Array.Copy ((char[]) value, (int) fieldOffset, buffer2, bufferoffset, length);
			return ((char[]) value).Length - fieldOffset;
		}

		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public string GetDataTypeName (int i)
		{
			return dataTypeNames [i].ToString ().ToUpper ();
		}

		public DateTime GetDateTime (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDateTime (CultureInfo.CurrentCulture);
		}

		public decimal GetDecimal (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDecimal (CultureInfo.CurrentCulture);
		}

		public double GetDouble (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDouble (CultureInfo.CurrentCulture);
		}

		public Type GetFieldType (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];
			return defineHandle.FieldType;
		}

		public float GetFloat (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToSingle (CultureInfo.CurrentCulture);
		}

		public Guid GetGuid (int i)
		{
			throw new NotSupportedException ();
		}

		public short GetInt16 (int i)
		{
			throw new NotSupportedException ();
		}

		public int GetInt32 (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToInt32 (CultureInfo.CurrentCulture);
		}

		public long GetInt64 (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToInt64 (CultureInfo.CurrentCulture);
		}

		public string GetName (int i)
		{
			return statement.GetParameter (i).GetName ();
		}

		[MonoTODO]
		public OracleBFile GetOracleBFile (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OracleBinary GetOracleBinary (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			return new OracleBinary ((byte[]) GetValue (i));
		}

		public OracleLob GetOracleLob (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			OracleLob output = (OracleLob) ((OciDefineHandle) statement.Values [i]).GetValue();
			output.connection = command.Connection;
			return output;
		}

		public OracleNumber GetOracleNumber (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			return new OracleNumber (GetDecimal (i));
		}

		public OracleDateTime GetOracleDateTime (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			return new OracleDateTime (GetDateTime (i));
		}

		[MonoTODO]
		public OracleMonthSpan GetOracleMonthSpan (int i)
		{
			throw new NotImplementedException ();
		}

		public OracleString GetOracleString (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			return new OracleString (GetString (i));
		}

		public object GetOracleValue (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];

			switch (defineHandle.DataType) {
			case OciDataType.Raw:
				return GetOracleBinary (i);
			case OciDataType.Date:
				return GetOracleDateTime (i);
			case OciDataType.Clob:
			case OciDataType.Blob:
				return GetOracleLob (i);
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				return GetOracleNumber (i);
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.LongVarChar:
			case OciDataType.Long:
			case OciDataType.RowIdDescriptor:
				return GetOracleString (i);
			default:
				throw new NotImplementedException ();
			}
		}

		public int GetOracleValues (object[] values)
		{
			int len = values.Length;
			int count = statement.ColumnCount;
			int retval = 0;

			if (len > count)
				retval = count;
			else
				retval = len;

			for (int i = 0; i < retval; i += 1) 
				values [i] = GetOracleValue (i);

			return retval;
		}

		[MonoTODO]
		public OracleTimeSpan GetOracleTimeSpan (int i)
		{
			return new OracleTimeSpan (GetTimeSpan (i));
		}

		public int GetOrdinal (string name)
		{
			int i;
			
			for (i = 0; i < statement.ColumnCount; i += 1) {
				if (String.Compare (statement.GetParameter(i).GetName(), name, false) == 0)
					return i;
			}

			for (i = 0; i < statement.ColumnCount; i += 1) {
				if (String.Compare (statement.GetParameter(i).GetName(), name, true) == 0)
					return i;
			}

			throw new IndexOutOfRangeException ();
		}

		private int GetRecordsAffected ()
		{
			if (recordsAffected == -1) 
				recordsAffected = statement.GetAttributeInt32 (OciAttributeType.RowCount, command.ErrorHandle);
			return recordsAffected;
		}

		public DataTable GetSchemaTable ()
		{
			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;

			dataTypeNames = new ArrayList ();

			for (int i = 0; i < statement.ColumnCount; i += 1) {
				DataRow row = schemaTable.NewRow ();

				OciParameterDescriptor parameter = statement.GetParameter (i);

				dataTypeNames.Add (parameter.GetDataTypeName ());

				row ["ColumnName"]		= parameter.GetName ();
				row ["ColumnOrdinal"]		= i + 1;
				row ["ColumnSize"]		= parameter.GetDataSize ();
				row ["NumericPrecision"]	= parameter.GetPrecision ();
				row ["NumericScale"]		= parameter.GetScale ();
				string sDataTypeName = parameter.GetDataTypeName ();
				row ["DataType"]		= parameter.GetFieldType (sDataTypeName);
				row ["AllowDBNull"]		= parameter.GetIsNull ();
				row ["BaseColumnName"]		= parameter.GetName ();
				row ["IsReadOnly"] 		= true;

				schemaTable.Rows.Add (row);
			}

			return schemaTable;
		}

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string))
				throw new InvalidCastException ();
			return (string) value;
		}

		public TimeSpan GetTimeSpan (int i)
		{
			object value = GetValue (i);
			if (!(value is TimeSpan))
				throw new InvalidCastException ();
			return (TimeSpan) value;
		}

		public object GetValue (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];

			if (defineHandle.IsNull)
				return DBNull.Value;

			switch (defineHandle.DataType) {
			case OciDataType.Blob:
			case OciDataType.Clob:
				OracleLob lob = GetOracleLob (i);
				object value = lob.Value;
				lob.Close ();
				return value;
			default:
				return defineHandle.GetValue ();
			}
		}

		public int GetValues (object[] values)
		{
			int len = values.Length;
			int count = statement.ColumnCount;
			int retval = 0;

			if (len > count)
				retval = count;
			else
				retval = len;

			for (int i = 0; i < retval; i += 1) 
				values [i] = GetValue (i);

			return retval;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];
			return defineHandle.IsNull;
		}

		[MonoTODO]
		public bool NextResult ()
		{
			// FIXME: get next result
			return false; 
		}

		public bool Read ()
		{
			bool retval = statement.Fetch ();
			hasRows = retval;
			return retval;
		}

		#endregion // Methods
	}
}
