//
// Mono.Data.Tds.Protocol.Tds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Sebastien Pouliot (spouliot@motus.com)
//   Daniel Morgan (danielmorgan@verizon.net)
//
// Copyright (C) 2002 Tim Coleman
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003 Daniel Morgan
//

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

using Mono.Security.Protocol.Ntlm;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Mono.Data.Tds.Protocol {
        public abstract class Tds : Component, ITds
	{
		#region Fields

		TdsComm comm;
		TdsVersion tdsVersion;
		
		protected internal TdsConnectionParameters connectionParms;
		protected readonly byte[] NTLMSSP_ID = new byte[] {0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00};

		int packetSize;
		string dataSource;
		string database;
		string databaseProductName;
		string databaseProductVersion;
		int databaseMajorVersion;

		string charset;
		string language;

		bool connected = false;
		bool moreResults;

		Encoding encoder;
//		bool autoCommit;

		bool doneProc;
		TdsDataRow currentRow = null;
		TdsDataColumnCollection columns;

		ArrayList tableNames;
		ArrayList columnNames;

		TdsMetaParameterCollection parameters;

		bool queryInProgress;
		int cancelsRequested;
		int cancelsProcessed;

//		bool isDone;
//		bool isDoneInProc;

		ArrayList outputParameters = new ArrayList ();
		protected TdsInternalErrorCollection messages = new TdsInternalErrorCollection ();

		int recordsAffected = 0;

		#endregion // Fields

		#region Properties

		protected string Charset {
			get { return charset; }
		}

		public bool DoneProc {
			get { return doneProc; }
		}

		protected string Language {
			get { return language; }
		}

		protected ArrayList ColumnNames {
			get { return columnNames; }
		}

		public TdsDataRow ColumnValues {
			get { return currentRow; }
		}

		internal TdsComm Comm {
			get { return comm; }
		}

		public string Database {
			get { return database; }
		}

		public string DataSource {
			get { return dataSource; }
		}

		public bool IsConnected {
			get { return connected; }
			set { connected = value; }
		}

		public bool MoreResults {
			get { return moreResults; }
			set { moreResults = value; }
		}

		public int PacketSize {
			get { return packetSize; }
		}

		public int RecordsAffected {
			get { return recordsAffected; }
			set { recordsAffected = value; }
		}

		public string ServerVersion {
			get { return databaseProductVersion; }
		}

		public TdsDataColumnCollection Columns {
			get { return columns; }
		}

		public TdsVersion TdsVersion {
			get { return tdsVersion; }
		}

		public ArrayList OutputParameters {
			get { return outputParameters; }
			set { outputParameters = value; }
		}

		protected TdsMetaParameterCollection Parameters {
			get { return parameters; }
			set { parameters = value; }
		}

		#endregion // Properties

		#region Events

		public event TdsInternalErrorMessageEventHandler TdsErrorMessage;
		public event TdsInternalInfoMessageEventHandler TdsInfoMessage;

		#endregion // Events

		#region Constructors

		public Tds (string dataSource, int port, int packetSize, int timeout, TdsVersion tdsVersion)
		{
			this.tdsVersion = tdsVersion;
			this.packetSize = packetSize;
			this.dataSource = dataSource;

			comm = new TdsComm (dataSource, port, packetSize, timeout, tdsVersion);
		}

		#endregion // Constructors

		#region Public Methods

		public void Cancel ()
		{
			if (queryInProgress) {
				if (cancelsRequested == cancelsProcessed) {
					comm.StartPacket (TdsPacketType.Cancel);
					comm.SendPacket ();
					cancelsRequested += 1;
				}
			}	
		}
	
		public abstract bool Connect (TdsConnectionParameters connectionParameters);

		public static TdsTimeoutException CreateTimeoutException (string dataSource, string method)
		{
			string message = "Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.";
			return new TdsTimeoutException (0, 0, message, -2, method, dataSource, "Mono TdsClient Data Provider", 0);
		}

		public void Disconnect ()
		{
			comm.StartPacket (TdsPacketType.Logoff);
			comm.Append ((byte) 0);
			comm.SendPacket ();	
			comm.Close ();
			connected = false;
		}
		
		public virtual bool Reset ()
		{
			return true;
		}

		public void Execute (string sql)
		{
			Execute (sql, null, 0, false);
		}

		public void ExecProc (string sql)
		{
			ExecProc (sql, null, 0, false);
		}

		public virtual void Execute (string sql, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			ExecuteQuery (sql, timeout, wantResults);	
		}

		public virtual void ExecProc (string sql, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			ExecuteQuery (String.Format ("exec {0}", sql), timeout, wantResults);
		}

		public virtual void ExecPrepared (string sql, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			throw new NotSupportedException ();
		}

		protected void ExecuteQuery (string sql, int timeout, bool wantResults)
		{
			moreResults = true;
			doneProc = false;
			messages.Clear ();
			outputParameters.Clear ();

			Comm.StartPacket (TdsPacketType.Query);
			Comm.Append (sql);
			Comm.SendPacket ();

			CheckForData (timeout);
			if (!wantResults) 
				SkipToEnd ();
		}

		public bool NextResult ()
		{
			if (!moreResults)
				return false;

			TdsPacketSubType subType;

			bool done = false;
			bool outputParams = false;

			while (!done) {
				subType = ProcessSubPacket ();
				if (outputParams) {
					moreResults = false;
					break;
				}

				switch (subType) {
				case TdsPacketSubType.ColumnInfo:
				case TdsPacketSubType.ColumnMetadata: 
				case TdsPacketSubType.RowFormat: 
					byte peek = Comm.Peek ();
					done = (peek != (byte) TdsPacketSubType.TableName);
					if (done && doneProc && peek == (byte) TdsPacketSubType.Row) {
						outputParams = true;
						done = false;
					}

					break;
				case TdsPacketSubType.TableName:
				//	done = true;
					peek = Comm.Peek ();
					done = (peek != (byte) TdsPacketSubType.ColumnDetail);

					break;
				case TdsPacketSubType.ColumnDetail:
					done = true;
					break;
				default:
					done = !moreResults;
					break;
				}
			}

			return moreResults;
		}

		public bool NextRow ()
		{
			TdsPacketSubType subType;
			bool done = false;
			bool result = false;

			do {
				subType = ProcessSubPacket ();
				switch (subType) {
				case TdsPacketSubType.Row:
					result = true;
					done = true;
					break;
				case TdsPacketSubType.Done:
				case TdsPacketSubType.DoneProc:
				case TdsPacketSubType.DoneInProc:
					result = false;
					done = true;
					break;
				}
			} while (!done);

			return result;
		}

		public virtual string Prepare (string sql, TdsMetaParameterCollection parameters)
		{
			throw new NotSupportedException ();
		}

		public void SkipToEnd ()
		{
			while (NextResult ()) { /* DO NOTHING */ }
		}

		public virtual void Unprepare (string statementId) 
		{
			throw new NotSupportedException ();
		}

		#endregion // Public Methods

		#region // Private Methods

		[MonoTODO ("Is cancel enough, or do we need to drop the connection?")]
		protected void CheckForData (int timeout) 
		{
			if (timeout > 0 && !comm.Poll (timeout, SelectMode.SelectRead)) {
				Cancel ();
				throw CreateTimeoutException (dataSource, "CheckForData()");
			}
		}
	
		protected TdsInternalInfoMessageEventArgs CreateTdsInfoMessageEvent (TdsInternalErrorCollection errors)
		{
			return new TdsInternalInfoMessageEventArgs (errors);
		}

		protected TdsInternalErrorMessageEventArgs CreateTdsErrorMessageEvent (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			return new TdsInternalErrorMessageEventArgs (new TdsInternalError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		private object GetColumnValue (TdsColumnType colType, bool outParam)
		{
			return GetColumnValue (colType, outParam, -1);
		}

		private object GetColumnValue (TdsColumnType colType, bool outParam, int ordinal)
		{
			int len;
			object element = null;

			switch (colType) {
			case TdsColumnType.IntN :
				if (outParam)
					comm.Skip (1);
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Int1 :
			case TdsColumnType.Int2 :
			case TdsColumnType.Int4 :
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Image :
				if (outParam) 
					comm.Skip (1);
				element = GetImageValue ();
				break;
			case TdsColumnType.Text :
				if (outParam) 
					comm.Skip (1);
				element = GetTextValue (false);
				break;
			case TdsColumnType.NText :
				if (outParam) 
					comm.Skip (1);
				element = GetTextValue (true);
				break;
			case TdsColumnType.Char :
			case TdsColumnType.VarChar :
				if (outParam)
					comm.Skip (1);
				element = GetStringValue (false, false);
				break;
			case TdsColumnType.BigVarBinary :
				comm.GetTdsShort ();
				len = comm.GetTdsShort ();
				element = comm.GetBytes (len, true);
				break;
			case TdsColumnType.BigVarChar :
				comm.Skip (2);
				element = GetStringValue (false, false);
				break;
			case TdsColumnType.NChar :
			case TdsColumnType.NVarChar :
				if (outParam) 
					comm.Skip (1);
				element = GetStringValue (true, false);
				break;
			case TdsColumnType.Real :
			case TdsColumnType.Float8 :
				element = GetFloatValue (colType);
				break;
			case TdsColumnType.FloatN :
				if (outParam) 
					comm.Skip (1);
				element = GetFloatValue (colType);
				break;
			case TdsColumnType.SmallMoney :
			case TdsColumnType.Money :
				element = GetMoneyValue (colType);
				break;
			case TdsColumnType.MoneyN :
				if (outParam)
					comm.Skip (1);
				element = GetMoneyValue (colType);
				break;
			case TdsColumnType.Numeric :
			case TdsColumnType.Decimal :
				byte precision;
				byte scale;
				if (outParam) {
					comm.Skip (1);
					precision = comm.GetByte ();
					scale = comm.GetByte ();
				}
				else {
					precision = (byte) columns[ordinal]["NumericPrecision"];
					scale = (byte) columns[ordinal]["NumericScale"];
				}

				element = GetDecimalValue (precision, scale);
				break;
			case TdsColumnType.DateTimeN :
				if (outParam) 
					comm.Skip (1);
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.DateTime4 :
			case TdsColumnType.DateTime :
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.VarBinary :
			case TdsColumnType.Binary :
				if (outParam) 
					comm.Skip (1);
				element = GetBinaryValue ();
				break;
			case TdsColumnType.BitN :
				if (outParam) 
					comm.Skip (1);
				if (comm.GetByte () == 0)
					element = DBNull.Value;
				else
					element = (comm.GetByte() != 0);
				break;
			case TdsColumnType.Bit :
				int columnSize = comm.GetByte ();
				element = (columnSize != 0);
				break;
			case TdsColumnType.UniqueIdentifier :
				if (comm.Peek () != 16) { // If it's null, then what to do?
					byte swallowed = comm.GetByte();	
					element = DBNull.Value;
					break;
				}
				len = comm.GetByte () & 0xff;
				if (len > 0) {
					byte[] guidBytes = comm.GetBytes (len, true);
					if (!BitConverter.IsLittleEndian) {
						byte[] swappedguidBytes = new byte[len];
						for (int i = 0; i < 4; i++)
							swappedguidBytes[i] = guidBytes[4-i-1];
						for (int i = 4; i < 6; i++)
							swappedguidBytes[i] = guidBytes[6-(i-4)-1];
						for (int i = 6; i < 8; i++)
							swappedguidBytes[i] = guidBytes[8-(i-6)-1];
						for (int i = 8; i < 16; i++)
							swappedguidBytes[i] = guidBytes[i];
						Array.Copy(swappedguidBytes, 0, guidBytes, 0, len);
					}
					element = new Guid (guidBytes);
				}
				break;
			default :
				return DBNull.Value;
			}

			return element;
		}

		private object GetBinaryValue ()
		{
			int len;
			object result = DBNull.Value;
			if (tdsVersion == TdsVersion.tds70) {
				len = comm.GetTdsShort ();
				if (len != 0xffff && len > 0)
					result = comm.GetBytes (len, true);
			} 
			else {
				len = (comm.GetByte () & 0xff);
				if (len != 0)
					result = comm.GetBytes (len, true);
			}
			return result;
		}

		private object GetDateTimeValue (TdsColumnType type)
		{
			int len = 0;
			object result;
		
			switch (type) {
			case TdsColumnType.DateTime4:
				len = 4;
				break;
			case TdsColumnType.DateTime:
				len = 8;
				break;
			case TdsColumnType.DateTimeN:
				byte tmp = comm.Peek ();
				if (tmp != 0 && tmp != 4 && tmp != 8)
					break;
				len = comm.GetByte ();
				break;
			}
	
			DateTime epoch = new DateTime (1900, 1, 1);
	
			switch (len) {
			case 8 :
				result = epoch.AddDays (comm.GetTdsInt ());
				int seconds = comm.GetTdsInt ();
				long millis = ((((long) seconds) % 300L) * 1000L) / 300L;
				if (seconds != 0 || millis != 0) {
					result = ((DateTime) result).AddSeconds (seconds / 300);
					result = ((DateTime) result).AddMilliseconds (millis);
				}
				break;
			case 4 :
				result = epoch.AddDays ((int) comm.GetTdsShort ());
				short minutes = comm.GetTdsShort ();
				if (minutes != 0) 
					result = ((DateTime) result).AddMinutes ((int) minutes);
				break;
			default:
				result = DBNull.Value;
				break;
			}

			return result;
		}

		private object GetDecimalValue (byte precision, byte scale)
		{
			int[] bits = new int[4] {0,0,0,0};

			int len = (comm.GetByte() & 0xff) - 1;
			if (len < 0)
				return DBNull.Value;
			
			bool positive = (comm.GetByte () == 1);

			if (len > 16)
				throw new OverflowException ();

			for (int i = 0, index = 0; i < len && i < 16; i += 4, index += 1) 
				bits[index] = comm.GetTdsInt ();

			if (bits [3] != 0) 
				return new TdsBigDecimal (precision, scale, !positive, bits);
			else
				return new Decimal (bits[0], bits[1], bits[2], !positive, scale);
		}

		private object GetFloatValue (TdsColumnType columnType)
		{
			int columnSize = 0;

			switch (columnType) {
			case TdsColumnType.Real:
				columnSize = 4;
				break;
			case TdsColumnType.Float8:
				columnSize = 8;
				break;
			case TdsColumnType.FloatN:
				columnSize = comm.GetByte ();
				break;
			}

			switch (columnSize) {
			case 8 :
				return BitConverter.Int64BitsToDouble (comm.GetTdsInt64 ());
			case 4 :
				return BitConverter.ToSingle (BitConverter.GetBytes (comm.GetTdsInt ()), 0);
			default :
				return DBNull.Value;
			}
		}

		private object GetImageValue ()
		{
			byte hasValue = comm.GetByte ();

			if (hasValue == 0)
				return DBNull.Value;
			
			comm.Skip (24);
			int len = comm.GetTdsInt ();

			if (len < 0)
				return DBNull.Value;

			return (comm.GetBytes (len, true));
		}

		private object GetIntValue (TdsColumnType type)
		{
			int len;

			switch (type) {
			case TdsColumnType.IntN :
				len = comm.GetByte ();
				break;
			case TdsColumnType.Int4 :
				len = 4; 
				break;
			case TdsColumnType.Int2 :
				len = 2; 
				break;
			case TdsColumnType.Int1 :
				len = 1; 
				break;
			default:
				return DBNull.Value;
			}

			switch (len) {
			case 4 :
				return (comm.GetTdsInt ());
			case 2 :
				return (comm.GetTdsShort ());
			case 1 :
				return (comm.GetByte ());
			default:
				return DBNull.Value;
			}
		}

		[MonoTODO]
		private object GetMoneyValue (TdsColumnType type)
		{
			int len;
			object result = null;

			switch (type) {
			case TdsColumnType.SmallMoney :
			case TdsColumnType.Money4 :
				len = 4;
				break;
			case TdsColumnType.Money :
				len = 8;
				break;
			case TdsColumnType.MoneyN :
				len = comm.GetByte ();
				break;
			default:
				return DBNull.Value;
			}

			long rawValue = 0;

			switch (len) {
			case 4:
				rawValue = comm.GetTdsInt ();
				break;
			case 8:
				byte[] bits = new byte[8];
				bits[4] = comm.GetByte ();
				bits[5] = comm.GetByte ();
				bits[6] = comm.GetByte ();
				bits[7] = comm.GetByte ();
				bits[0] = comm.GetByte ();
				bits[1] = comm.GetByte ();
				bits[2] = comm.GetByte ();
				bits[3] = comm.GetByte ();
				rawValue = BitConverter.ToInt64 (bits, 0);
				break;
			default:
				return DBNull.Value;
			}

			result = new Decimal (rawValue);

			return (((decimal) result) / 10000);
		}

		private object GetStringValue (bool wideChars, bool outputParam)
		{
			bool shortLen = (tdsVersion == TdsVersion.tds70) && (wideChars || !outputParam);
			int len = shortLen ? comm.GetTdsShort () : (comm.GetByte () & 0xff);

			if (tdsVersion < TdsVersion.tds70 && len == 0)
				return DBNull.Value;
			else if (len >= 0) {
				object result;
				if (wideChars)
					result = comm.GetString (len / 2);
				else
					result = comm.GetString (len, false);
				if (tdsVersion < TdsVersion.tds70 && ((string) result).Equals (" "))
					result = "";
				return result;
			}
			else
				return DBNull.Value;
		}

		protected int GetSubPacketLength ()
		{
			return comm.GetTdsShort ();
		}

		private object GetTextValue (bool wideChars)
		{
			string result = null;
			byte hasValue = comm.GetByte ();

			if (hasValue != 16)
				return DBNull.Value;

			// 16 Byte TEXTPTR, 8 Byte TIMESTAMP
			comm.Skip (24);

			int len = comm.GetTdsInt ();

			//if the len is 0 , then the string can be a '' string 
			// this method is called only for Text and NText. Hence will
			// return a empty string
			if (len == 0)
				return "";

			if (wideChars)
				result = comm.GetString (len / 2);
			else
				result = comm.GetString (len, false);
				len /= 2;

			if ((byte) tdsVersion < (byte) TdsVersion.tds70 && result == " ")
				result = "";

			return result;
		}

		internal static bool IsFixedSizeColumn (TdsColumnType columnType)
		{
			switch (columnType) {
				case TdsColumnType.Int1 :
				case TdsColumnType.Int2 :
				case TdsColumnType.Int4 :
				case TdsColumnType.Float8 :
				case TdsColumnType.DateTime :
				case TdsColumnType.Bit :
				case TdsColumnType.Money :
				case TdsColumnType.Money4 :
				case TdsColumnType.SmallMoney :
				case TdsColumnType.Real :
				case TdsColumnType.DateTime4 :
					return true;
				case TdsColumnType.IntN :
				case TdsColumnType.MoneyN :
				case TdsColumnType.VarChar :
				case TdsColumnType.NVarChar :
				case TdsColumnType.DateTimeN :
				case TdsColumnType.FloatN :
				case TdsColumnType.Char :
				case TdsColumnType.NChar :
				case TdsColumnType.NText :
				case TdsColumnType.Image :
				case TdsColumnType.VarBinary :
				case TdsColumnType.Binary :
				case TdsColumnType.Decimal :
				case TdsColumnType.Numeric :
				case TdsColumnType.BitN :
				case TdsColumnType.UniqueIdentifier :
					return false;
				default :
					return false;
			}
		}

		protected void LoadRow ()
		{
			currentRow = new TdsDataRow ();

			int i = 0;
			foreach (TdsDataColumn column in columns) {
				object o = GetColumnValue ((TdsColumnType) column["ColumnType"], false, i);
				currentRow.Add (o);
				if (doneProc)
					outputParameters.Add (o);

				if (o is TdsBigDecimal && currentRow.BigDecimalIndex < 0) 
					currentRow.BigDecimalIndex = i;
				i += 1;
			}
		}

		internal static int LookupBufferSize (TdsColumnType columnType)
		{
			switch (columnType) {
				case TdsColumnType.Int1 :
				case TdsColumnType.Bit :
					return 1;
				case TdsColumnType.Int2 :
					return 2;
				case TdsColumnType.Int4 :
				case TdsColumnType.Real :
				case TdsColumnType.DateTime4 :
				case TdsColumnType.Money4 :
				case TdsColumnType.SmallMoney :
					return 4;
				case TdsColumnType.Float8 :
				case TdsColumnType.DateTime :
				case TdsColumnType.Money :
					return 8;
				default :
					return 0;
			}
		}

		private int LookupDisplaySize (TdsColumnType columnType) 
		{
			switch (columnType) {
				case TdsColumnType.Int1 :
					return 3;
				case TdsColumnType.Int2 :
					return 6;
				case TdsColumnType.Int4 :
					return 11;
				case TdsColumnType.Real :
					return 14;
				case TdsColumnType.Float8 :
					return 24;
				case TdsColumnType.DateTime :
					return 23;
				case TdsColumnType.DateTime4 :
					return 16;
				case TdsColumnType.Bit :
					return 1;
				case TdsColumnType.Money :
					return 21;
				case TdsColumnType.Money4 :
				case TdsColumnType.SmallMoney :
					return 12;
				default:
					return 0;
			}
		}

		protected internal int ProcessAuthentication ()
		{
			int pdu_size = Comm.GetTdsShort ();
			byte[] msg2 = Comm.GetBytes (pdu_size, true);

			Type2Message t2 = new Type2Message (msg2);
			// 0x0001	Negotiate Unicode
			// 0x0200	Negotiate NTLM
			// 0x8000	Negotiate Always Sign

			Type3Message t3 = new Type3Message ();
			t3.Challenge = t2.Nonce;
			
			t3.Domain = this.connectionParms.DefaultDomain;
			t3.Host = this.connectionParms.Hostname;
			t3.Username = this.connectionParms.User;
			t3.Password = this.connectionParms.Password;

			Comm.StartPacket (TdsPacketType.SspAuth); // 0x11
			Comm.Append (t3.GetBytes ());
			Comm.SendPacket ();
			return 1; // TDS_SUCCEED
		}

		protected void ProcessColumnDetail ()
		{
			int len = GetSubPacketLength ();
			byte[] values = new byte[3];
			int columnNameLength;
			string baseColumnName = String.Empty;
			int position = 0;

			while (position < len) {
				for (int j = 0; j < 3; j += 1) 
					values[j] = comm.GetByte ();
				position += 3;

				if ((values[2] & (byte) TdsColumnStatus.Rename) != 0) {
					if (tdsVersion == TdsVersion.tds70) {
						columnNameLength = comm.GetByte ();
						position += 2 * len + 1;
					}
					else {
						columnNameLength = comm.GetByte ();
						position += len + 1;
					}
					baseColumnName = comm.GetString (columnNameLength);
				}

				if ((values[2] & (byte) TdsColumnStatus.Hidden) == 0) {
					byte index = (byte) (values[0] - (byte) 1);
					byte tableIndex = (byte) (values[1] - (byte) 1);

					columns [index]["IsExpression"] = ((values[2] & (byte) TdsColumnStatus.IsExpression) != 0);
					columns [index]["IsKey"] = ((values[2] & (byte) TdsColumnStatus.IsKey) != 0);

					if ((values[2] & (byte) TdsColumnStatus.Rename) != 0)
						columns [index]["BaseColumnName"] = baseColumnName;
					columns [index]["BaseTableName"] = tableNames [tableIndex];
				}
			}
		}

		protected abstract TdsDataColumnCollection ProcessColumnInfo ();

		protected void ProcessColumnNames ()
		{
			columnNames = new ArrayList ();

			int totalLength = comm.GetTdsShort ();
			int bytesRead = 0;
			int i = 0;

			while (bytesRead < totalLength) {
				int columnNameLength = comm.GetByte ();
				string columnName = comm.GetString (columnNameLength);
				bytesRead = bytesRead + 1 + columnNameLength;
				columnNames.Add (columnName);
				i += 1;
			}
		}

		[MonoTODO ("Make sure counting works right, especially with multiple resultsets.")]
		protected void ProcessEndToken (TdsPacketSubType type)
		{
			byte status = Comm.GetByte ();
			Comm.Skip (1);
			byte op = comm.GetByte ();
			Comm.Skip (1);

			int rowCount = comm.GetTdsInt ();

			if (op == (byte) 0xc1) 
				rowCount = 0;
			if (type == TdsPacketSubType.DoneInProc) 
				rowCount = -1;

			moreResults = ((status & 0x01) != 0);
			bool cancelled = ((status & 0x20) != 0);

			switch (type) {
				case TdsPacketSubType.DoneProc:
					doneProc = true;
					goto case TdsPacketSubType.Done;

				case TdsPacketSubType.Done:
					if (rowCount > 0)
						recordsAffected += rowCount;
					break;
			}

			if (moreResults) 
				queryInProgress = false;
			if (cancelled)
				cancelsProcessed += 1;
			if (messages.Count > 0 && !moreResults) 
				OnTdsInfoMessage (CreateTdsInfoMessageEvent (messages));
		}

		protected void ProcessEnvironmentChange ()
		{
			int len = GetSubPacketLength ();
			TdsEnvPacketSubType type = (TdsEnvPacketSubType) comm.GetByte ();
			int cLen;

			switch (type) {
			case TdsEnvPacketSubType.BlockSize :
				string blockSize;
				cLen = comm.GetByte () & 0xff;
				blockSize = comm.GetString (cLen);

				if (tdsVersion == TdsVersion.tds70) 
					comm.Skip (len - 2 - cLen * 2);
				else 
					comm.Skip (len - 2 - cLen);
				
				comm.ResizeOutBuf (Int32.Parse (blockSize));
				break;
			case TdsEnvPacketSubType.CharSet :
				cLen = comm.GetByte () & 0xff;
				if (tdsVersion == TdsVersion.tds70) {
					//this.language = comm.GetString (cLen); // FIXME
					comm.GetString (cLen);
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					SetCharset (comm.GetString (cLen));
					comm.Skip (len - 2 - cLen);
				}

				break;
			case TdsEnvPacketSubType.Database :
				cLen = comm.GetByte () & 0xff;
				string newDB = comm.GetString (cLen);
				cLen = comm.GetByte () & 0xff;
				string oldDB = comm.GetString (cLen);
				database = newDB;
				break;
			default:
				comm.Skip (len - 1);
				break;
			}
		}

		protected void ProcessLoginAck ()
		{
			GetSubPacketLength ();

			if (tdsVersion == TdsVersion.tds70) {
				comm.Skip (5);
				int nameLength = comm.GetByte ();
				databaseProductName = comm.GetString (nameLength);
				databaseMajorVersion = comm.GetByte ();
				databaseProductVersion = String.Format ("0{0}.0{1}.0{2}", databaseMajorVersion, comm.GetByte (), ((256 * (comm.GetByte () + 1)) + comm.GetByte ()));
			}
			else {
				comm.Skip (5);
				short nameLength = comm.GetByte ();
				databaseProductName = comm.GetString (nameLength);
				comm.Skip (1);
				databaseMajorVersion = comm.GetByte ();
				databaseProductVersion = String.Format ("{0}.{1}", databaseMajorVersion, comm.GetByte ());
				comm.Skip (1);
			}

			if (databaseProductName.Length > 1 && -1 != databaseProductName.IndexOf ('\0')) {
				int last = databaseProductName.IndexOf ('\0');
				databaseProductName = databaseProductName.Substring (0, last);
			}

			connected = true;
		}

		protected void OnTdsErrorMessage (TdsInternalErrorMessageEventArgs e)
		{
			if (TdsErrorMessage != null)
				TdsErrorMessage (this, e);
		}

		protected void OnTdsInfoMessage (TdsInternalInfoMessageEventArgs e)
		{
			if (TdsInfoMessage != null)
				TdsInfoMessage (this, e);
			messages.Clear ();
		}

		protected void ProcessMessage (TdsPacketSubType subType)
		{
			GetSubPacketLength ();

			int number = comm.GetTdsInt ();
			byte state = comm.GetByte ();
			byte theClass = comm.GetByte ();
			string message;
			string server;
			string procedure;
			byte lineNumber;
			string source;
			bool isError = false;

			if (subType == TdsPacketSubType.EED) {
				isError = (theClass > 10);
				comm.Skip (comm.GetByte ()); // SQL State
				comm.Skip (1);               // Status
				comm.Skip (2);               // TranState
			} else 
				isError = (subType == TdsPacketSubType.Error);

			message = comm.GetString (comm.GetTdsShort ());
			server = comm.GetString (comm.GetByte ());
			procedure = comm.GetString (comm.GetByte ());
			lineNumber = comm.GetByte ();
			comm.Skip (1);
			source = String.Empty; // FIXME

			if (isError)
				OnTdsErrorMessage (CreateTdsErrorMessageEvent (theClass, lineNumber, message, number, procedure, server, source, state));
			else
				messages.Add (new TdsInternalError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		protected void ProcessOutputParam ()
		{
			GetSubPacketLength ();
			comm.GetString (comm.GetByte () & 0xff);
			comm.Skip (5);

			TdsColumnType colType = (TdsColumnType) comm.GetByte ();
			object value = GetColumnValue (colType, true);

			outputParameters.Add (value);
		}

		protected void ProcessDynamic ()
		{
			Comm.Skip (2);
			byte type = Comm.GetByte ();
			byte status = Comm.GetByte ();
			string id = Comm.GetString (Comm.GetByte ());
		}

		protected virtual TdsPacketSubType ProcessSubPacket ()
		{
			TdsPacketSubType subType = (TdsPacketSubType) comm.GetByte ();

			switch (subType) {
			case TdsPacketSubType.Dynamic2:
				comm.Skip (comm.GetTdsInt ());
				break;
			case TdsPacketSubType.AltName:
			case TdsPacketSubType.AltFormat:
			case TdsPacketSubType.Capability:
			case TdsPacketSubType.ParamFormat:
				comm.Skip (comm.GetTdsShort ());
				break;
			case TdsPacketSubType.Dynamic:
				ProcessDynamic ();
				break;
			case TdsPacketSubType.EnvironmentChange:
				ProcessEnvironmentChange ();
				break;
			case TdsPacketSubType.Info:  // TDS 4.2/7.0
			case TdsPacketSubType.EED:   // TDS 5.0
			case TdsPacketSubType.Error: // TDS 4.2/7.0
				ProcessMessage (subType);
				break;
			case TdsPacketSubType.Param:
				ProcessOutputParam ();
				break;
			case TdsPacketSubType.LoginAck:
				ProcessLoginAck ();
				break;
			case TdsPacketSubType.Authentication: // TDS 7.0
				ProcessAuthentication ();
				break;
			case TdsPacketSubType.ReturnStatus :
				Comm.Skip (4);
				break;
			case TdsPacketSubType.ProcId:
				Comm.Skip (8);
				break;
			case TdsPacketSubType.Done:
			case TdsPacketSubType.DoneProc:
			case TdsPacketSubType.DoneInProc:
				ProcessEndToken (subType);
				break;
			case TdsPacketSubType.ColumnName:
				Comm.Skip (8);
				ProcessColumnNames ();
				break;
			case TdsPacketSubType.ColumnInfo:      // TDS 4.2
			case TdsPacketSubType.ColumnMetadata:  // TDS 7.0
			case TdsPacketSubType.RowFormat:       // TDS 5.0
				columns = ProcessColumnInfo ();
				break;
			case TdsPacketSubType.ColumnDetail:
				ProcessColumnDetail ();
				break;
			case TdsPacketSubType.TableName:
				ProcessTableName ();
				break;
			case TdsPacketSubType.ColumnOrder:
				comm.Skip (comm.GetTdsShort ());
				break;
			case TdsPacketSubType.Control:
				comm.Skip (comm.GetTdsShort ());
				break;
			case TdsPacketSubType.Row:
				LoadRow ();
				break;
			}

			return subType;
		}

		protected void ProcessTableName ()
		{
			tableNames = new ArrayList ();
			int totalLength = comm.GetTdsShort ();
			int position = 0;
			int len;

			while (position < totalLength) {
				if (tdsVersion == TdsVersion.tds70) {
					len = comm.GetTdsShort ();
					position += 2 * (len + 1);
				}
				else {
					len = comm.GetByte ();
					position += len + 1;
				}
				tableNames.Add (comm.GetString (len));
			}	
		}

		protected void SetCharset (string charset)
		{
			if (charset == null || charset.Length > 30)
				charset = "iso_1";

			if (this.charset != null && this.charset != charset)
				return;

			if (charset.StartsWith ("cp")) {
				encoder = Encoding.GetEncoding (Int32.Parse (charset.Substring (2)));
				this.charset = charset;
			}
			else {
				encoder = Encoding.GetEncoding ("iso-8859-1");
				this.charset = "iso_1";
			}
			comm.Encoder = encoder;
		}

		protected void SetLanguage (string language)
		{
			if (language == null || language.Length > 30)
				language = "us_english";

			this.language = language;
		}

		#endregion // Private Methods
	}
}
