//
// Mono.Data.PostgreSqlClient.PgSqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
//
// Credits:
//    SQL and concepts were used from libgda 0.8.190 (GNOME Data Access)
//    http://www.gnome-db.org/
//    with permission from the authors of the
//    PostgreSQL provider in libgda:
//        Michael Lausch <michael@lausch.at>
//        Rodrigo Moya <rodrigo@gnome-db.org>
//        Vivien Malerba <malerba@gnome-db.org>
//        Gonzalo Paniagua Javier <gonzalo@gnome-db.org>
//

// *** uncomment #define to get debug messages, comment for production ***
//#define DEBUG_SqlDataReader


using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.PostgreSqlClient {
	/// <summary>
	/// Provides a means of reading one or more forward-only streams
	/// of result sets obtained by executing a command 
	/// at a SQL database.
	/// </summary>
	public sealed class PgSqlDataReader : MarshalByRefObject,
		IEnumerable, IDataReader, IDisposable, IDataRecord {

		#region Fields

		private PgSqlCommand cmd;
		private DataTable table = null;

		// columns in a row
		private object[] fields; // data value in a .NET type
		private string[] types; // PostgreSQL Type
		private bool[] isNull; // is NULL?
		private int[] actualLength; // ActualLength of data
		private DbType[] dbTypes; // DB data type
		// actucalLength = -1 is variable-length
				
		private bool open = false;
		IntPtr pgResult; // PGresult
		private int rows;
		private int cols;

		private int recordsAffected = -1; // TODO: get this value

		private int currentRow = -1; // no Read() has been done yet

		private bool disposed = false;

		#endregion // Fields

		#region Constructors

		internal PgSqlDataReader (PgSqlCommand sqlCmd) {

			cmd = sqlCmd;
			open = true;
			cmd.OpenReader(this);
		}

		#endregion

		#region Public Methods

		[MonoTODO]
		public void Close() {
			open = false;
			
			// free PgSqlDataReader resources in PgSqlCommand
			// and allow PgSqlConnection to be used again
			cmd.CloseReader();

			// TODO: get parameters from result

			Dispose (true);
		}

		[MonoTODO]
		public DataTable GetSchemaTable() {
			return table;
		}

		[MonoTODO]
		public bool NextResult() {
			PgSqlResult res;
			currentRow = -1;
			bool resultReturned;
			
			// reset
			table = null;
			pgResult = IntPtr.Zero;
			rows = 0;
			cols = 0;
			types = null;
			recordsAffected = -1;

			res = cmd.NextResult();
			resultReturned = res.ResultReturned;

			if(resultReturned == true) {
				table = res.Table;
				pgResult = res.PgResult;
				rows = res.RowCount;
				cols = res.FieldCount;
				types = res.PgTypes;
				recordsAffected = res.RecordsAffected;
			}
			
			res = null;
			return resultReturned;
		}

		[MonoTODO]
		public bool Read() {
			
			string dataValue;
			int c = 0;
			
			if(currentRow < rows - 1)  {
				
				currentRow++;
			
				// re-init row
				fields = new object[cols];
				//dbTypes = new DbType[cols];
				actualLength = new int[cols];
				isNull = new bool[cols];
			
				for(c = 0; c < cols; c++) {

					// get data value
					dataValue = PostgresLibrary.
						PQgetvalue(
						pgResult,
						currentRow, c);

					// is column NULL?
					//isNull[c] = PostgresLibrary.
					//	PQgetisnull(pgResult,
					//	currentRow, c);

					// get Actual Length
					actualLength[c] = PostgresLibrary.
						PQgetlength(pgResult,
						currentRow, c);

					DbType dbType;	
					dbType = PostgresHelper.
						TypnameToSqlDbType(types[c]);

					if(dataValue == null) {
						fields[c] = null;
						isNull[c] = true;
					}
					else if(dataValue.Equals("")) {
						fields[c] = null;
						isNull[c] = true;
					}
					else {
						isNull[c] = false;
						fields[c] = PostgresHelper.
							ConvertDbTypeToSystem (
							dbType,
							dataValue);
					}
				}
				return true;
			}
			return false; // EOF
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
			return types[i];
		}

		[MonoTODO]
		public DateTime GetDateTime(int i) {
			return (DateTime) fields[i];
		}

		[MonoTODO]
		public decimal GetDecimal(int i) {
			return (decimal) fields[i];
		}

		[MonoTODO]
		public double GetDouble(int i) {
			return (double) fields[i];
		}

		[MonoTODO]
		public Type GetFieldType(int i) {

			DataRow row = table.Rows[i];
			return Type.GetType((string)row["DataType"]);
		}

		[MonoTODO]
		public float GetFloat(int i) {
			return (float) fields[i];
		}

		[MonoTODO]
		public Guid GetGuid(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public short GetInt16(int i) {
			return (short) fields[i];
		}

		[MonoTODO]
		public int GetInt32(int i) {
			return (int) fields[i];
		}

		[MonoTODO]
		public long GetInt64(int i) {
			return (long) fields[i];
		}

		[MonoTODO]
		public string GetName(int i) {

			DataRow row = table.Rows[i];
			return (string) row["ColumnName"];
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
			return (string) fields[i];
		}

		[MonoTODO]
		public object GetValue(int i) {
			return fields[i];
		}

		[MonoTODO]
		public int GetValues(object[] values) 
		{
			Array.Copy (fields, values, fields.Length);
			return fields.Length;
		}

		[MonoTODO]
		public bool IsDBNull(int i) {
			return isNull[i];
		}

		[MonoTODO]
		public bool GetBoolean(int i) {
			return (bool) fields[i];
		}

		[MonoTODO]
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
					table = null;
					fields = null;
					types = null;
					isNull = null;
					actualLength = null;
					dbTypes = null;
				}
				// release any unmanaged resources

				// clear unmanaged PostgreSQL result set
				if (pgResult != IntPtr.Zero) {
					PostgresLibrary.PQclear (pgResult);
					pgResult = IntPtr.Zero;
				}

				// close any handles
				this.disposed = true;
			}
		}

		void IDisposable.Dispose() {
			Dispose(true);
		}

		~PgSqlDataReader() {
			Dispose(false);
		}

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
				return cols;
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
