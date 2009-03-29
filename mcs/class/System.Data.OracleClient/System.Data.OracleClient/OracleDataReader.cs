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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient.Oci;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient
{
	public sealed class OracleDataReader :
#if NET_2_0
		DbDataReader
#else
		MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
#endif
	{
		#region Fields

		OracleCommand command;
		ArrayList dataTypeNames;
#if !NET_2_0
		bool disposed;
#endif
		bool isClosed;
		bool hasRows;
		DataTable schemaTable;
		CommandBehavior behavior;

		int recordsAffected = -1;
		OciStatementType statementType;
		OciStatementHandle statement;

		#endregion // Fields

		#region Constructors

		internal OracleDataReader (OracleCommand command, OciStatementHandle statement, bool extHasRows, CommandBehavior behavior)
		{
			this.command = command;
			this.hasRows = extHasRows;
			this.schemaTable = ConstructSchemaTable ();
			this.statement = statement;
			this.statementType = statement.GetStatementType ();
			this.behavior = behavior;
		}

		~OracleDataReader ()
		{
			Dispose (false);
		}

		#endregion // Constructors

		#region Properties

		public
#if NET_2_0
		override
#endif
		int Depth {
			get { return 0; }
		}

		public
#if NET_2_0
		override
#endif
		int FieldCount {
			get { return statement.ColumnCount; }
		}

		public
#if NET_2_0
		override
#endif
		bool HasRows {
			get { return hasRows; }
		}

		public
#if NET_2_0
		override
#endif
		bool IsClosed {
			get { return isClosed; }
		}

		public
#if NET_2_0
		override
#endif
		object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}

		public
#if NET_2_0
		override
#endif
		object this [int i] {
			get { return GetValue (i); }
		}

		public
#if NET_2_0
		override
#endif
		int RecordsAffected {
			get {
				return GetRecordsAffected ();
			}
		}

		#endregion // Properties

		#region Methods

		public
#if NET_2_0
		override
#endif
		void Close ()
		{
			if (!isClosed) {
				GetRecordsAffected ();
				if (command != null)
					command.CloseDataReader ();
			}
			if (statement != null) {
				statement.Dispose();
				statement = null;
			}
#if NET_2_0
			if (schemaTable != null) {
				schemaTable.Dispose ();
				schemaTable = null;
			}
#endif
			isClosed = true;
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = typeof (bool);
			Type stringType = typeof (string);
			Type intType = typeof (int);
			Type typeType = typeof (Type);
			Type shortType = typeof (short);

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("ProviderType", intType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("IsAliased", booleanType);
			schemaTable.Columns.Add ("IsExpression", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);

			return schemaTable;
		}

#if !NET_2_0
		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing)
					Close ();
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
#endif

		public
#if NET_2_0
		override
#endif
		bool GetBoolean (int i)
		{
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif
		byte GetByte (int i)
		{
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif
		long GetBytes (int i, long fieldOffset, byte[] buffer2, int bufferoffset, int length)
		{
			byte[] value = (byte[]) GetValue (i);

			if (buffer2 == null)
				return value.Length; // Return length of data

			// Copy data into buffer
			long lobLength = value.Length;
			if ((lobLength - fieldOffset) < length)
				length = (int) (lobLength - fieldOffset);
			Array.Copy (value, (int) fieldOffset, buffer2,
				bufferoffset, length);
			return length; // return actual read count
		}

		public
#if NET_2_0
		override
#endif
		char GetChar (int i)
		{
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif
		long GetChars (int i, long fieldOffset, char[] buffer2, int bufferoffset, int length)
		{
			char [] value = (char[]) GetValue (i);
			Array.Copy (value, (int) fieldOffset, buffer2,
				bufferoffset, length);
			return (value.Length - fieldOffset);
		}

#if !NET_2_0
		[MonoTODO]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}
#endif

		public
#if NET_2_0
		override
#endif
		string GetDataTypeName (int i)
		{
			return dataTypeNames [i].ToString ().ToUpper ();
		}

		public
#if NET_2_0
		override
#endif
		DateTime GetDateTime (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDateTime (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		decimal GetDecimal (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDecimal (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		double GetDouble (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToDouble (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		Type GetFieldType (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];
			return defineHandle.FieldType;
		}

		public
#if NET_2_0
		override
#endif
		float GetFloat (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToSingle (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		Guid GetGuid (int i)
		{
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif
		short GetInt16 (int i)
		{
			throw new NotSupportedException ();
		}

		public
#if NET_2_0
		override
#endif
		int GetInt32 (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToInt32 (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		long GetInt64 (int i)
		{
			IConvertible c = (IConvertible) GetValue (i);
			return c.ToInt64 (CultureInfo.CurrentCulture);
		}

		public
#if NET_2_0
		override
#endif
		string GetName (int i)
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

			OracleLob output = (OracleLob) ((OciDefineHandle) statement.Values [i]).GetValue (
				command.Connection.SessionFormatProvider, command.Connection);
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

		public OracleMonthSpan GetOracleMonthSpan (int i)
		{
			if (IsDBNull (i))
				throw new InvalidOperationException("The value is null");

			OracleMonthSpan output = (OracleMonthSpan) ((OciDefineHandle) statement.Values [i]).GetValue (
				command.Connection.SessionFormatProvider, command.Connection);
			return output;
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
			case OciDataType.IntervalDayToSecond:
				return GetOracleTimeSpan (i);
			case OciDataType.IntervalYearToMonth:
				return GetOracleMonthSpan (i);
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

		public OracleTimeSpan GetOracleTimeSpan (int i)
		{
			return new OracleTimeSpan (GetTimeSpan (i));
		}

		public
#if NET_2_0
		override
#endif
		int GetOrdinal (string name)
		{
			int i = GetOrdinalInternal (name);
			if (i == -1)
				throw new IndexOutOfRangeException ();
			return i;
		}

		private int GetOrdinalInternal (string name)
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

			return -1;
		}

		private int GetRecordsAffected ()
		{
			if (statementType == OciStatementType.Select)
				return -1;
			else {
				if (!isClosed) {
					if (recordsAffected == -1)
						if (statement != null)
							recordsAffected = statement.GetAttributeInt32 (OciAttributeType.RowCount, command.ErrorHandle);
				}
			}
			
			return recordsAffected;
		}

		// get the KeyInfo about table columns (primary key)
		private StringCollection GetKeyInfo (out string ownerName, out string tableName) 
		{
			ArrayList tables = new ArrayList ();
			ParseSql (command.CommandText, ref tables);
			// TODO: handle multiple tables
			GetOwnerAndName ((string)tables[0], out ownerName, out tableName);
			return GetKeyColumns (ownerName, tableName);
		}

		// get the columns in a table that have a primary key
		private StringCollection GetKeyColumns(string owner, string table) 
		{
			OracleCommand cmd = command.Connection.CreateCommand ();

			StringCollection columns = new StringCollection ();

			if (command.Transaction != null)
				cmd.Transaction = command.Transaction;

			cmd.CommandText = "select col.column_name " +
				"from all_constraints pk, all_cons_columns col " +
				"where pk.owner = '" + owner + "' " +
				"and pk.table_name = '" + table + "' " +
				"and pk.constraint_type = 'P' " +
				"and pk.owner = col.owner " +
				"and pk.table_name = col.table_name " +
				"and pk.constraint_name = col.constraint_name";

			OracleDataReader rdr = cmd.ExecuteReader ();
			while (rdr.Read ())
				columns.Add (rdr.GetString (0));

			rdr.Close();
			rdr = null;
			cmd.Dispose();
			cmd = null;

			return columns;
		}

		// parse the list of table names in the SQL
		// TODO: parse the column aliases and table aliases too
		//       and determine if a column is a true table column
		//       or an expression
		private void ParseSql (string sql, ref ArrayList tables) {
			if (sql == String.Empty)
				return;

			char[] chars = sql.ToCharArray ();
			StringBuilder wb = new StringBuilder ();

			bool bFromFound = false;
			bool bEnd = false;
			int i = 0;
			bool bTableFound = false;
		
			for (; !bEnd && i < chars.Length; i++) {
				char ch = chars[i];
			
				if (Char.IsLetter (ch)) {
					wb.Append (ch);
				} else if (Char.IsWhiteSpace (ch)) {
					if (wb.Length > 0) {
						if (!bFromFound) {
							string word = wb.ToString ().ToUpper ();
							if (word.Equals ("FROM")) {
								bFromFound = true;
							}
							wb = null;
							wb = new StringBuilder ();
							bTableFound = false;
						} else {
							switch (wb.ToString ().ToUpper ()) {
							case "WHERE":
							case "ORDER":
							case "GROUP":
								bEnd = true;
								bTableFound = false;
								break;
							default:
								if (bTableFound)
									bTableFound = false; // this is done in case of a table alias
								else {
									bTableFound = true;
									tables.Add (wb.ToString ().ToUpper ());
								}
								wb = null;
								wb = new StringBuilder ();
								break;
							}
						}
					}
				} else if (bFromFound) {
					switch (ch) {
					case ',': 
						if (bTableFound)
							bTableFound = false;
						else
							tables.Add (wb.ToString ().ToUpper ());
						wb = null;
						wb = new StringBuilder ();
						break;
					case '$':
					case '_':
					case '.':
						wb.Append (ch);
						break;
					}
				}
			}
			if (!bEnd) {
				if (wb.Length > 0) {
					if (!bFromFound && wb.ToString ().ToUpper ().Equals ("FROM"))
						bFromFound = true;
					if (bFromFound) {
						switch(wb.ToString ().ToUpper ()) {
						case "WHERE":
						case "ORDER":
						case "GROUP":
							bEnd = true;
							break;
						default:
							if (!bTableFound)
								tables.Add (wb.ToString ().ToUpper ());
							break;
						}
					}
				}
			}
		}

		// takes a object name like "owner.name" and parses it into "owner" and "name" strings
		// if object name is only "name", then it gets the username as the owner and returns
		// the name
		private void GetOwnerAndName (string objectName, out string owner, out string name) 
		{
			int idx = objectName.IndexOf (".");
			if (idx == -1) {
				OracleCommand cmd = command.Connection.CreateCommand ();
				if (command.Transaction != null)
					cmd.Transaction = command.Transaction;

				cmd.CommandText = "SELECT USER FROM DUAL";
				owner = (string) cmd.ExecuteScalar();
				name = objectName;
				cmd.Dispose();
				cmd = null;
			} else {
				owner = objectName.Substring (0, idx);
				name = objectName.Substring (idx + 1);
			}
		}

		[MonoTODO("Implement this properly, with all needed information.")]
		public
#if NET_2_0
		override
#endif
		DataTable GetSchemaTable ()
		{
			StringCollection keyinfo = null;

			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;

			string owner = String.Empty;
			string table = String.Empty;
			if ((behavior & CommandBehavior.KeyInfo) != 0)
				keyinfo = GetKeyInfo (out owner, out table);

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
				
				string sDataTypeName		= parameter.GetDataTypeName ();
				row ["DataType"]		= parameter.GetFieldType (sDataTypeName);
				
				OciDataType ociType = parameter.GetDataType();
				OracleType oraType = OciParameterDescriptor.OciDataTypeToOracleType (ociType);
				row ["ProviderType"]		= (int) oraType;
				
				if (ociType == OciDataType.Blob || ociType == OciDataType.Clob)
					row ["IsLong"]		= true;
				else
					row ["IsLong"]		= false;

				row ["AllowDBNull"]		= parameter.GetIsNull ();

				row ["IsAliased"]		= DBNull.Value; // TODO:
				row ["IsExpression"]		= DBNull.Value; // TODO:
				
				if ((behavior & CommandBehavior.KeyInfo) != 0) {
					if (keyinfo.IndexOf ((string)row ["ColumnName"]) >= 0)
						row ["IsKey"] = true;
					else
						row ["IsKey"] = false;

					row ["IsUnique"]	= DBNull.Value; // TODO: only set this if CommandBehavior.KeyInfo, otherwise, null
					row ["BaseSchemaName"]	= owner;
					row ["BaseTableName"]	= table;
					row ["BaseColumnName"]	= row ["ColumnName"];
				} else {
					row ["IsKey"]		= DBNull.Value;	
					row ["IsUnique"]	= DBNull.Value; 
					row ["BaseSchemaName"]	= DBNull.Value; 
					row ["BaseTableName"]	= DBNull.Value; 
					row ["BaseColumnName"]	= DBNull.Value; 
				}

				schemaTable.Rows.Add (row);
			}

			return schemaTable;
		}

		public
#if NET_2_0
		override
#endif
		string GetString (int i)
		{
			return (string) GetValue (i);
		}

		public TimeSpan GetTimeSpan (int i)
		{
			return (TimeSpan) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif
		object GetValue (int i)
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
				return defineHandle.GetValue (command.Connection.SessionFormatProvider, command.Connection);
			}
		}

		public
#if NET_2_0
		override
#endif
		int GetValues (object [] values)
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

#if NET_2_0
		public override IEnumerator GetEnumerator ()
#else
		IEnumerator IEnumerable.GetEnumerator ()
#endif
		{
			return new DbEnumerator (this);
		}

		public
#if NET_2_0
		override
#endif
		bool IsDBNull (int i)
		{
			OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [i];
			return defineHandle.IsNull;
		}

		void ValidateState ()
		{
			if (IsClosed)
				throw new InvalidOperationException ("Invalid attempt to read data when reader is closed");
		}

		public
#if NET_2_0
		override
#endif
		bool NextResult ()
		{
			ValidateState ();

			if (statement == null)
				return false;

			statement.Dispose ();
			statement = null;
			
			statement = command.GetNextResult ();
			
			if (statement == null)
				return false;

			return true; 
		}

		public
#if NET_2_0
		override
#endif
		bool Read ()
		{
			ValidateState ();

			if (hasRows) {
				bool retval = statement.Fetch ();
				hasRows = retval;
				return retval;
			}
			return false;
		}

#if NET_2_0
		[MonoTODO]
		public override Type GetProviderSpecificFieldType (int i)
		{
			return GetOracleValue (i).GetType ();
		}

		[MonoTODO]
		public override object GetProviderSpecificValue (int i)
		{
			return GetOracleValue (i);
		}

		[MonoTODO]
		public override int GetProviderSpecificValues (object [] values)
		{
			return GetOracleValues (values);
		}
#endif

		#endregion // Methods
	}
}
