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

// *** uncomment #define to get debug messages, comment for production ***
//#define DEBUG_MySqlDataReader

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace Mono.Data.MySql {
	/// <summary>
	/// Provides a means of reading one or more forward-only streams
	/// of result sets obtained by executing a command 
	/// at a SQL database.
	/// </summary>
	//public sealed class MySqlDataReader : MarshalByRefObject,
	//	IEnumerable, IDataReader, IDisposable, IDataRecord
	public sealed class MySqlDataReader : IEnumerable, 
		IDataReader, IDataRecord {
		
		#region Fields

		private MySqlCommand cmd;
		private DataTable table = null;

		// field meta data
		string[] fieldName;
		int[] fieldType;
		uint[] fieldLength;
		uint[] fieldMaxLength;
		uint[] fieldFlags;
		// field data value
		private object[] dataValue;
				
		private bool open = false;

		private int recordsAffected = -1; 
		private int currentQuery = 0;

		private int currentRow = -1;
		IntPtr res;

		private int numFields;
		private int numRows;

		private CommandBehavior cmdBehavior;

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

			// clear unmanaged MySQL result set
			if(res != IntPtr.Zero) {
				MySql.FreeResult(res);
				res = IntPtr.Zero;
			}
		}

		[MonoTODO]
		public DataTable GetSchemaTable() {	

			DataTable dataTableSchema = null;
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if(numFields > 0) {
				
				dataTableSchema = new DataTable ();
				
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
				dataTableSchema.Columns.Add ("DataType", typeof(string));
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

				// TODO: for CommandBehavior.SingleRow
				//       use IRow, otherwise, IRowset
				if(numFields > 0)
					if((cmdBehavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow)
						numFields = 1;

				// TODO: for CommandBehavior.SchemaInfo
				if((cmdBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
					numFields = 0;

				// TODO: for CommandBehavior.SingleResult
				if((cmdBehavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult)
					if(currentQuery > 0)
						numFields = 0;

				// TODO: for CommandBehavior.SequentialAccess - used for reading Large OBjects
				//if((cmdBehavior & CommandBehavior.SequentialAccess) == CommandBehavior.SequentialAccess) {
				//}

				DataRow schemaRow;
				DbType dbType;
				Type typ;
								
				for (int i = 0; i < numFields; i += 1 ) {
					
					schemaRow = dataTableSchema.NewRow ();
										
					schemaRow["ColumnName"] = fieldName[i];
					schemaRow["ColumnOrdinal"] = i + 1;
					
					schemaRow["ColumnSize"] = (int) fieldLength[i];
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;
					// TODO: need to get KeyInfo
					if((cmdBehavior & CommandBehavior.KeyInfo) == CommandBehavior.KeyInfo) {
						// bool IsUnique, IsKey;
						// GetKeyInfo(field[i].Name, out IsUnique, out IsKey);
					}
					else {
						schemaRow["IsUnique"] = false;
						schemaRow["IsKey"] = DBNull.Value;
					}
					schemaRow["BaseCatalogName"] = "";
					
					schemaRow["BaseColumnName"] = fieldName[i];
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";

					// do translation from MySQL type 
					// to .NET Type and then convert the result
					// to a string
					enum_field_types fieldEnum;
					
					fieldEnum = (enum_field_types) fieldType[i];
					dbType = MySqlHelper.MySqlTypeToDbType(fieldEnum);
					typ = MySqlHelper.DbTypeToSystemType (dbType);
					string st = typ.ToString();
					schemaRow["DataType"] = st;

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
					
                                        schemaRow.AcceptChanges();
					
					dataTableSchema.Rows.Add (schemaRow);
				}
				
#if DEBUG_MySqlCommand
				Console.WriteLine("********** DEBUG Table Schema BEGIN ************");
				foreach (DataRow myRow in dataTableSchema.Rows) {
					foreach (DataColumn myCol in dataTableSchema.Columns)
						Console.WriteLine(myCol.ColumnName + " = " + myRow[myCol]);
					Console.WriteLine();
				}
				Console.WriteLine("********** DEBUG Table Schema END ************");
#endif // DEBUG_MySqlCommand

			}
			
			return dataTableSchema;
		}

		[MonoTODO]
		public bool NextResult() {
			currentRow = -1;
			bool resultReturned = false;		

			// reset
			table = null;
			recordsAffected = -1;
			
			// store the result set
			res = MySql.StoreResult(cmd.Connection.NativeMySqlInitStruct);

			if(res.Equals(IntPtr.Zero)) {
				// no result set returned
				recordsAffected = (int) MySql.AffectedRows(cmd.Connection.NativeMySqlInitStruct);

				numRows = 0;
				numFields = 0;
				fieldName = null;
				fieldType = null;
				fieldLength = null;
				fieldMaxLength = null;
				fieldFlags = null;
				dataValue = null;
			}
			else {
				dataValue = null;

				// get meta data about result set
				numRows = MySql.NumRows(res);
				numFields = MySql.NumFields(res);						
				// get meta data about each field
				fieldName = new string[numFields];
				fieldType = new int[numFields];
				fieldLength = new uint[numFields];
				fieldMaxLength = new uint[numFields];
				fieldFlags = new uint[numFields];
				
				// marshal each meta data field
				// into field* arrays
				for (int i = 0; i < numFields; i++) {
					// marshal field
					MySqlMarshalledField marshField = null;
					marshField = (MySqlMarshalledField) Marshal.PtrToStructure(MySql.FetchField(res), 
						typeof(MySqlMarshalledField));
					
					// copy memebers in marshalField to fields[i]
					fieldName[i] = marshField.Name;
					fieldType[i] = marshField.FieldType;
					fieldLength[i] = marshField.Length;
					fieldMaxLength[i] = marshField.MaxLength;
					fieldFlags[i] = marshField.Flags;
				}
			}
			return resultReturned;
		}

		[MonoTODO]
		public bool Read() {		
			
			if(currentRow < numRows - 1)  {
				
				currentRow++;

				dataValue = null;

				dataValue = new object[numFields];
									
				IntPtr row;
				row = MySql.FetchRow(res);
				if(row.Equals(IntPtr.Zero)) {
					MySql.FreeResult(res);
					res = IntPtr.Zero;
					return false; // EOF
				}
				else {
					for (int i = 0; i < numFields; i++) {
						dataValue[i] = cmd.GetColumnData(row, i);
						// maybe some tranlation needs to be
						// done from the native MySql c type
						// to a .NET type here
					}
				}
				return true;
			}
			return false;
		}

		[MonoTODO]
		public byte GetByte(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes(int i, long fieldOffset, 
			byte[] buffer, int bufferOffset, 
			int length) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public char GetChar(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars(int i, long fieldOffset, 
			char[] buffer, int bufferOffset, 
			int length) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IDataReader GetData(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDataTypeName(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DateTime GetDateTime(int i) {
			return (DateTime) dataValue[i];
		}

		[MonoTODO]
		public decimal GetDecimal(int i) {
			return (decimal) dataValue[i];
		}

		[MonoTODO]
		public double GetDouble(int i) {
			return (double) dataValue[i];
		}

		[MonoTODO]
		public Type GetFieldType(int i) {
			enum_field_types fieldEnum;
			DbType dbType;
			Type typ;

			fieldEnum = (enum_field_types) fieldType[i];		
			dbType = MySqlHelper.MySqlTypeToDbType(fieldEnum);
			typ = MySqlHelper.DbTypeToSystemType (dbType);

			return typ;
		}

		[MonoTODO]
		public float GetFloat(int i) {
			return (float) dataValue[i];
		}

		[MonoTODO]
		public Guid GetGuid(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16(int i) {
			return (short) dataValue[i];
		}

		[MonoTODO]
		public int GetInt32(int i) {
			return (int) dataValue[i];
		}

		[MonoTODO]
		public long GetInt64(int i) {
			return (long) dataValue[i];
		}

		[MonoTODO]
		public string GetName(int i) {
			return fieldName[i];
		}

		[MonoTODO]
		public int GetOrdinal(string name) {

			int i;
			
			for(i = 0; i < numFields; i++) {
				if(fieldName[i].Equals(name))
					return i;
			}

			for(i = 0; i < numFields; i++) {
				string ta;
				string n;
								
				ta = fieldName[i].ToUpper();
				n = name.ToUpper();
						
				if(ta.Equals(n)) {
					return i;
				}
			}
			
			throw new MissingFieldException("Missing field: " + name);
		}

		[MonoTODO]
		public string GetString(int i) {
			return (string) dataValue[i];
		}

		[MonoTODO]
		public object GetValue(int i) {
			// FIXME: this returns a native type
			//        need to return a .NET type
			if(MySqlFieldHelper.IsNotNull(fieldFlags[i]) == true)
				return dataValue[i];
			else
				return DBNull.Value;
		}

		[MonoTODO]
		public int GetValues(object[] values) 
		{
			Array.Copy (dataValue, values, dataValue.Length);
			return dataValue.Length;
		}

		[MonoTODO]
		public bool IsDBNull(int i) {
			return !MySqlFieldHelper.IsNotNull(fieldFlags[i]);
		}

		[MonoTODO]
		public bool GetBoolean(int i) {
			return (bool) dataValue[i];
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			throw new NotImplementedException ();
		}

		#endregion // Public Methods

		#region Destructors

		[MonoTODO]
		public void Dispose () {
		}

		//[MonoTODO]
		//~MySqlDataReader() {
		//}

		#endregion // Destructors

		#region Properties

		public int Depth {
			[MonoTODO]
			get { 
				return 0; // always return zero, unless
				          // this provider will allow
				          // nesting of a row
			}
		}

		public bool IsClosed {
			[MonoTODO]
			get {
				if(open == false)
					return true;
				else
					return false;
			}
		}

		public int RecordsAffected {
			[MonoTODO]
			get { 
				return recordsAffected;
			}
		}
	
		public int FieldCount {
			[MonoTODO]
			get { 
				return numFields;
			}
		}

		public object this[string name] {
			[MonoTODO]
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
			[MonoTODO]
			get { 
				return dataValue[i];
			}
		}

		#endregion // Properties
	}
}
