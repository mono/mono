//
// Mono.Data.MySql.MySqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace Mono.Data.MySql {
	/// <summary>
	/// Provides a means of reading one or more forward-only streams
	/// of result sets obtained by executing a command 
	/// at a SQL database.
	/// </summary>
	public sealed class MySqlDataReader : MarshalByRefObject,
		IEnumerable, IDataReader, IDisposable, IDataRecord 
	{
		
		#region Fields

		private MySqlCommand cmd;
		
		// field meta data
		private string[] fieldName;
		private MySqlEnumFieldTypes[] fieldType; // MySQL data type
		private DbType[] fieldDbType; // DbType translated from MySQL type
		private uint[] fieldLength;
		private uint[] fieldMaxLength;
		private uint[] fieldFlags;
		// field data value
		private object[] dataValue;
						
		private bool open = false;

		private int recordsAffected = -1; 
		private int currentQuery = 0;

		private int currentRow = -1;
		private IntPtr res = IntPtr.Zero;
		private IntPtr row = IntPtr.Zero;

		private int numFields;
		private int numRows;

		private CommandBehavior cmdBehavior;

		private bool disposed = false;

		#endregion // Fields

		#region Constructors

		internal MySqlDataReader (MySqlCommand sqlCmd, CommandBehavior behavior) {

			cmd = sqlCmd;
			open = true;
			// cmd.OpenReader(this);
			cmdBehavior = behavior;
		}

		#endregion // Fields

		#region Public Methods

		[MonoTODO]
		public void Close() {
			open = false;
			
			// free MySqlDataReader resources in SqlCommand
			// and allow SqlConnection to be used again
			//cmd.CloseReader();

			// TODO: get parameters from result

			Dispose(true);
		}

		public DataTable GetSchemaTable() {	

			DataTable dataTableSchema = null;
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if(numFields > 0) {
				
				dataTableSchema = new DataTable ("SchemaTable");
				
				dataTableSchema.Columns.Add ("ColumnName", typeof (string));
				dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
				dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
				dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
				dataTableSchema.Columns.Add ("NumericScale", typeof (int));
				dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
				dataTableSchema.Columns.Add ("IsKey", typeof (bool));
				DataColumn dc = dataTableSchema.Columns["IsKey"];
				dc.AllowDBNull = true; // IsKey can have a DBNull
				dataTableSchema.Columns.Add ("BaseCatalogName", typeof (string));
				dataTableSchema.Columns.Add ("BaseColumnName", typeof (string));
				dataTableSchema.Columns.Add ("BaseSchemaName", typeof (string));
				dataTableSchema.Columns.Add ("BaseTableName", typeof (string));
				dataTableSchema.Columns.Add ("DataType", typeof(Type));
				dataTableSchema.Columns.Add ("AllowDBNull", typeof (bool));
				dataTableSchema.Columns.Add ("ProviderType", typeof (int));
				dataTableSchema.Columns.Add ("IsAliased", typeof (bool));
				dataTableSchema.Columns.Add ("IsExpression", typeof (bool));
				dataTableSchema.Columns.Add ("IsIdentity", typeof (bool));
				dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (bool));
				dataTableSchema.Columns.Add ("IsRowVersion", typeof (bool));
				dataTableSchema.Columns.Add ("IsHidden", typeof (bool));
				dataTableSchema.Columns.Add ("IsLong", typeof (bool));
				dataTableSchema.Columns.Add ("IsReadOnly", typeof (bool));

				DataRow schemaRow;
				
				Type typ;
								
				for (int i = 0; i < numFields; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();
										
					schemaRow["ColumnName"] = fieldName[i];
					schemaRow["ColumnOrdinal"] = i + 1;
					
					schemaRow["ColumnSize"] = (int) fieldMaxLength[i];
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;
					
					if((cmdBehavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo) {
						// TODO: need to get KeyInfo
						schemaRow["IsUnique"] = false;
						schemaRow["IsKey"] = false;
					}
					else {
						schemaRow["IsUnique"] = false;
						schemaRow["IsKey"] = DBNull.Value;
					}
					schemaRow["BaseCatalogName"] = "";
					
					schemaRow["BaseColumnName"] = fieldName[i];
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";

					typ = MySqlHelper.DbTypeToSystemType (fieldDbType[i]);
					schemaRow["DataType"] = typ;
					
					schemaRow["AllowDBNull"] = false;
					
					schemaRow["ProviderType"] = (int) fieldType[i];
					schemaRow["IsAliased"] = false;
					schemaRow["IsExpression"] = false;
					schemaRow["IsIdentity"] = false;
					schemaRow["IsAutoIncrement"] = false;
					schemaRow["IsRowVersion"] = false;
					schemaRow["IsHidden"] = false;
					schemaRow["IsLong"] = false;
					schemaRow["IsReadOnly"] = false;
					
					dataTableSchema.Rows.Add (schemaRow);
				}
			}
			
			return dataTableSchema;
		}

		private void ClearFields () {
			numRows = 0;
			numFields = 0;
			fieldName = null;
			fieldType = null;
			fieldLength = null;
			fieldMaxLength = null;
			fieldFlags = null;
			dataValue = null;
		}

		public bool NextResult () {
			
			// reset
			recordsAffected = -1;
			currentRow = -1;
			
			bool resultReturned;
			res = cmd.NextResult (out resultReturned);
                        if (resultReturned == false)
				return false; // no result returned

			if ((cmdBehavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult)
				if (currentQuery > 0) {
					if(res == IntPtr.Zero)						
						recordsAffected = (int) MySql.AffectedRows(cmd.Connection.NativeMySqlInitStruct);
					ClearFields();
					return true; // result returned
				}

			if((cmdBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly) {
				ClearFields ();
				return false; // no result returned
			}

			if(res == IntPtr.Zero) {
				// no result set returned
				recordsAffected = (int) MySql.AffectedRows (cmd.Connection.NativeMySqlInitStruct);
				ClearFields();
			}
			else {
				dataValue = null;

				// get meta data about result set
				numRows = MySql.NumRows(res);
				numFields = MySql.NumFields(res);						
				// get meta data about each field
				fieldName = new string[numFields];
				fieldType = new MySqlEnumFieldTypes[numFields];
				fieldLength = new uint[numFields];
				fieldMaxLength = new uint[numFields];
				fieldFlags = new uint[numFields];

				fieldDbType = new DbType[numFields];
								
				// marshal each meta data field
				// into field* arrays
				MySqlMarshalledField marshField = null;
				for (int i = 0; i < numFields; i++) {
					// marshal field
					marshField = (MySqlMarshalledField) Marshal.PtrToStructure(MySql.FetchField(res), 
						typeof(MySqlMarshalledField));
					
					// copy memebers in marshalField to fields[i]
					fieldName[i] = marshField.Name;
					int myType = marshField.FieldType;
					fieldType[i] = (MySqlEnumFieldTypes) myType;
					fieldLength[i] = marshField.Length;
					fieldMaxLength[i] = marshField.MaxLength;
					fieldFlags[i] = marshField.Flags;

					fieldDbType[i] = MySqlHelper.MySqlTypeToDbType((MySqlEnumFieldTypes)fieldType[i]);
					marshField = null;
				}
			}
			return true; // result returned
		}

		public bool Read() {
	
			dataValue = null;
						
			if(currentRow < numRows - 1)  {
				
				currentRow++;

				if(numFields > 0 && currentRow > 0)
					if((cmdBehavior & CommandBehavior.SingleRow) == 
						CommandBehavior.SingleRow) {

						currentRow = numRows - 1;
						return false; // EOF
					}
				
				row = MySql.FetchRow (res);
				if (row == IntPtr.Zero) {
					MySql.FreeResult (res);
					res = IntPtr.Zero;
					return false; // EOF
				}
				else {
					dataValue = new object[numFields];
					for (int col = 0; col < numFields; col++) {
						GetDataValue (row, col);
					}
				}
				return true; // not EOF
			}
			return false; // EOF
		}

		void GetDataValue (IntPtr row, int col) {
			// marshal column data value
			string objValue = cmd.GetColumnData(row, col);
						
			// tranlate from native MySql c type
			// to a .NET type here
			dataValue[col] = MySqlHelper.ConvertDbTypeToSystem (fieldType[col], 
				fieldDbType[col], objValue);
						
			// TODO: for CommandBehavior.SequentialAccess - 
			//       used for reading Large OBjects
			//if((cmdBehavior & CommandBehavior.SequentialAccess) == 
			//		CommandBehavior.SequentialAccess) {
			//}
		}

		[MonoTODO]
		public byte GetByte (int i) {
			throw new NotImplementedException ();
		}

		// TODO: CommandBehavior.SequentialAccess
		//       and handling LOBs
		[MonoTODO]
		public long GetBytes (int i, long fieldOffset, 
			byte[] buffer, int bufferOffset, 
			int length) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar (int i) {
			throw new NotImplementedException ();
		}

		// TODO: CommandBehavior.SequentialAccess
		//       and handling LOBs
		[MonoTODO]
		public long GetChars (int i, long fieldOffset, 
			char[] buffer, int bufferOffset, 
			int length) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData (int i) {
			throw new NotImplementedException ();
		}
		
		public string GetDataTypeName (int i) {
			return MySqlHelper.GetMySqlTypeName (fieldType[i]);
		}

		public DateTime GetDateTime(int i) {
			return (DateTime) dataValue[i];
		}

		public decimal GetDecimal(int i) {
			return (decimal) dataValue[i];
		}

		public double GetDouble(int i) {
			return (double) dataValue[i];
		}

		public Type GetFieldType(int i) {
			return MySqlHelper.DbTypeToSystemType (fieldDbType[i]);
		}

		public float GetFloat(int i) {
			return (float) dataValue[i];
		}

		public Guid GetGuid(int i) {
			throw new NotImplementedException ();
		}

		public short GetInt16(int i) {
			return (short) dataValue[i];
		}

		public int GetInt32(int i) {
			return (int) dataValue[i];
		}

		public long GetInt64(int i) {
			return (long) dataValue[i];
		}

		public string GetName(int i) {
			return fieldName[i];
		}

		public int GetOrdinal (string name) {

			int i;
			
			for(i = 0; i < numFields; i++) {
				if(fieldName[i].Equals (name))
					return i;
			}

			for(i = 0; i < numFields; i++) {
				string ta;
				string n;
								
				ta = fieldName[i].ToUpper ();
				n = name.ToUpper ();
						
				if(ta.Equals (n)) {
					return i;
				}
			}
			
			throw new MissingFieldException ("Missing field: " + name);
		}

		public string GetString (int i) {
			return (string) dataValue[i];
		}

		public object GetValue (int i) {			
			return dataValue[i];
		}

		public int GetValues(object[] values) 
		{
			Array.Copy (dataValue, values, dataValue.Length);
			return dataValue.Length;
		}

		public bool IsDBNull(int i) {
			if(dataValue[i] == DBNull.Value)
				return true;
			return false;
		}
		
		public bool GetBoolean(int i) {
			return (bool) dataValue[i];
		}
		
		IEnumerator IEnumerable.GetEnumerator () {
			return new DbEnumerator (this);
		}

		#endregion // Public Methods

		#region Destructors

		private void Dispose(bool disposing) {
			if(!this.disposed) {
				if(disposing) {
					// release any managed resources
					cmd = null;
					fieldName = null;
					fieldType = null;
					fieldDbType = null;
					fieldLength = null;
					fieldMaxLength = null;
					fieldFlags = null;
					dataValue = null;
				}
				// release any unmanaged resources

				// clear unmanaged MySQL result set
				row = IntPtr.Zero;
				if(res != IntPtr.Zero) {
					MySql.FreeResult(res);
					res = IntPtr.Zero;
				}

				// close any handles
				this.disposed = true;
			}
		}

		void IDisposable.Dispose() {
			Dispose(true);
		}

		// aka Finalize
		~MySqlDataReader() {
			Dispose (false);
		}

		#endregion // Destructors

		#region Properties

		public int Depth {
			get { 
				return 0; // always return zero, unless
				          // this provider will allow
				          // nesting of a row
			}
		}

		public bool IsClosed {
			get {
				if(open == false)
					return true;
				else
					return false;
			}
		}

		public int RecordsAffected {
			get { 
				return recordsAffected;
			}
		}
	
		public int FieldCount {
			get { 
				return numFields;
			}
		}

		public object this[string name] {
			get { 
				int i;
				
				for(i = 0; i < numFields; i++) {
					if(fieldName[i].Equals(name))
						return dataValue[i];
				}

				for(i = 0; i < numFields; i++) {
					string ta;
					string n;
					
					ta = fieldName[i].ToUpper();
					n = name.ToUpper();
						
					if(ta.Equals(n)) {
						return dataValue[i];
					}
				}
			
				throw new MissingFieldException("Missing field: " + name);
			}
		}

		public object this[int i] {
			get { 
				return dataValue[i];
			}
		}

		#endregion // Properties
	}
}
