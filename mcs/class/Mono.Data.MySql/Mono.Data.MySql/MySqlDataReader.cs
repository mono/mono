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

		private Field[] fields; // metadata
		private string[] fieldNames;
		private uint[] fieldTypes;
		private object[] dataValue; // data
				
		private bool open = false;

		private int recordsAffected = -1; 

		private int currentRow = -1; // 
		IntPtr res;

		private int numFields;
		private int numRows;

		#endregion // Fields

		#region Constructors

		internal MySqlDataReader (MySqlCommand sqlCmd) {

			cmd = sqlCmd;
			open = true;
			// cmd.OpenReader(this);
		}

		#endregion

		#region Public Methods

		[MonoTODO]
		public void Close() {
			open = false;
			
			// free MySqlDataReader resources in SqlCommand
			// and allow SqlConnection to be used again
			//cmd.CloseReader();

			// TODO: get parameters from result

			// clear unmanaged PostgreSQL result set
			if(res != IntPtr.Zero) {
				MySql.FreeResult(res);
				res = IntPtr.Zero;
			}
		}

		[MonoTODO]
		public DataTable GetSchemaTable() {
			return table;
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
			
			// get meta data
			numRows = MySql.NumRows(res);
			numFields = MySql.NumFields(res);

			fieldNames = new string[numFields];

			Field[] fields = new Field[numFields];
			for (int i = 0; i < numFields; i++) {
				fields[i] = (Field) Marshal.PtrToStructure(MySql.FetchField(res), typeof(Field));
				fieldNames[i] = fields[i].Name;
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
						dataValue[i] = cmd.rowVal(row, i);
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal GetDecimal(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public double GetDouble(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetFieldType(int i) {

			DataRow row = table.Rows[i];
			return Type.GetType((string)row["DataType"]);
		}

		[MonoTODO]
		public float GetFloat(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Guid GetGuid(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetInt32(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetInt64(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetName(int i) {

			 // DataRow row = table.Rows[i];
			// return (string) row["ColumnName"];
			return fieldNames[i];
		}

		[MonoTODO]
		public int GetOrdinal(string name) {

			int i;
			DataRow row;

			for(i = 0; i < table.Rows.Count; i++) {
				row = table.Rows[i];
				if(((string) row["ColumnName"]).Equals(name))
					return i;
			}

			for(i = 0; i < table.Rows.Count; i++) {
				string ta;
				string n;
					
				row = table.Rows[i];
				ta = ((string) row["ColumnName"]).ToUpper();
				n = name.ToUpper();
						
				if(ta.Equals(n)) {
					return i;
				}
			}
			
			throw new MissingFieldException("Missing field: " + name);
		}

		[MonoTODO]
		public string GetString(int i) {
			return String.Copy((string)dataValue[i]);
		}

		[MonoTODO]
		public object GetValue(int i) {
			// FIXME: this returns a native type
			//        need to return a .NET type
			return dataValue[i];
		}

		[MonoTODO]
		public int GetValues(object[] values) 
		{
			Array.Copy (fields, values, fields.Length);
			return fields.Length;
		}

		[MonoTODO]
		public bool IsDBNull(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetBoolean(int i) {
			throw new NotImplementedException ();
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
		//~SqlDataReader() {
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
				DataRow row;

				for(i = 0; i < table.Rows.Count; i++) {
					row = table.Rows[i];
					if(row["ColumnName"].Equals(name))
						return fields[i];
				}

				for(i = 0; i < table.Rows.Count; i++) {
					string ta;
					string n;
					
					row = table.Rows[i];
					ta = ((string) row["ColumnName"]).ToUpper();
					n = name.ToUpper();
						
					if(ta.Equals(n)) {
						return fields[i];
					}
				}
			
				throw new MissingFieldException("Missing field: " + name);
			}
		}

		public object this[int i] {
			[MonoTODO]
			get { 
				return fields[i];
			}
		}

		#endregion // Properties
	}
}
