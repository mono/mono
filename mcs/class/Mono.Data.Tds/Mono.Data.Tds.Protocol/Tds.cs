//
// Mono.Data.TdsClient.Internal.Tds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Data.TdsClient.Internal {
        internal abstract class Tds : ITds
	{
		#region Fields

		TdsComm comm;
		TdsVersion tdsVersion;

		int packetSize;
		string dataSource;
		string database;
		string databaseProductName;
		string databaseProductVersion;
		int databaseMajorVersion;

		string charset;
		string language;

		DataTable table = new DataTable ();

		bool connected = false;
		bool moreResults;
		bool moreResults2;

		TdsMessage lastServerMessage;

		Encoding encoder;
		TdsServerType serverType;
		IsolationLevel isolationLevel;
		bool autoCommit;
                Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		bool inUse = false;

		#endregion // Fields

		#region Properties

		protected string Charset {
			get { return charset; }
		}

		protected string Language {
			get { return language; }
		}

		protected TdsComm Comm {
			get { return comm; }
		}

		public string Database {
			get { return database; }
		}

		public string DataSource {
			get { return dataSource; }
		}

		public bool InUse {
			get { return inUse; }
			set { inUse = value; }
		}

		public bool IsConnected {
			get { return connected; }
			set { connected = value; }
		}

		public int PacketSize {
			get { return packetSize; }
		}

		public string ServerVersion {
			get { return databaseProductVersion; }
		}

		public TdsVersion TdsVersion {
			get { return tdsVersion; }
		}

		#endregion // Properties

		#region Constructors

		public Tds (string dataSource, int port, int packetSize, TdsVersion tdsVersion)
		{
			this.tdsVersion = tdsVersion;
			this.packetSize = packetSize;
			this.dataSource = dataSource;

			IPHostEntry hostEntry = Dns.Resolve (dataSource);
			IPAddress[] addresses = hostEntry.AddressList;

			IPEndPoint endPoint;

			foreach (IPAddress address in addresses) {
				endPoint = new IPEndPoint (address, port);
				socket.Connect (endPoint);

				if (socket.Connected)
					break;
			}
	
			comm = new TdsComm (socket, packetSize, tdsVersion);
		}

		#endregion // Constructors

		#region Methods
		
		public void ChangeSettings (bool autoCommit, IsolationLevel isolationLevel)
		{
			/*string query = SqlStatementForSettings (autoCommit, isolationLevel);
			if (query != null)
				ChangeSettings (query);
				*/
		}

		private bool ChangeSettings (string query)
		{
			TdsPacketResult result;
			bool isOkay = true;
			if (query.Length == 0)
				return true;

			comm.StartPacket (TdsPacketType.Query);
			comm.Append (query);
			comm.SendPacket ();

			bool done = false;
			do {
				result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);

			return isOkay;
		}

		[MonoTODO ("fixme")]
		public int ExecuteNonQuery (string sql)
		{
			TdsPacketResult result;

			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				moreResults2 = true;
				comm.SendPacket ();
			}

			bool done = false;
			do {
				result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);

			if (sql.Trim ().ToUpper ().StartsWith ("INSERT") || sql.Trim ().ToUpper ().StartsWith ("UPDATE") || sql.Trim ().ToUpper ().StartsWith ("DELETE"))
				return ((TdsPacketEndTokenResult) result).RowCount;
			else
				return -1;
		}

		[MonoTODO ("fixme")]
		public void ExecuteQuery (string sql)
		{
			TdsPacketResult result = null;

			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				moreResults2 = true;
				comm.SendPacket ();
			}

			bool done = false;
			do {
				result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);
		}

		private object GetStringValue (bool wideChars, bool outputParam)
		{
			object result = null;
			bool shortLen = (tdsVersion == TdsVersion.tds70) && (wideChars || !outputParam);
			int len = shortLen ? comm.GetTdsShort () : (comm.GetByte () & 0xff);

			if ((tdsVersion < TdsVersion.tds70 && len == 0) || (tdsVersion == TdsVersion.tds70 && len == 0xffff))
				result = null;
			else if (len >= 0) {
				if (wideChars)
					result = comm.GetString (len / 2);
				else
					result = encoder.GetString (comm.GetBytes (len, false), 0, len);

				if (tdsVersion < TdsVersion.tds70 && ((string) result).Equals (" "))
					result = "";
			}
			else
				throw new TdsException ("");
			return result;
		}

		private object GetDecimalValue (int scale)
		{
			throw new NotImplementedException ();
		}

		private object GetDateTimeValue (TdsColumnType type)
		{
			throw new NotImplementedException ();
		}

		private object GetImageValue ()
		{
			byte[] result;
			byte hasValue = comm.GetByte ();
			if (hasValue == 0)
				return SqlBinary.Null;
			
			comm.Skip (24);
			int len = comm.GetTdsInt ();

			if (len < 0)
				throw new TdsException ("");

			return new SqlBinary (comm.GetBytes (len, true));
		}

		private object GetIntValue (TdsColumnType type)
		{
			object result;
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
				throw new TdsException ("");
			}

			switch (len) {
			case 4 :
				return new SqlInt32 (comm.GetTdsInt ());
			case 2 :
				return new SqlInt16 (comm.GetTdsShort ());
			case 1 :
				return new SqlByte (comm.GetByte ());
			case 0 :
				return SqlInt32.Null;
			default:
				throw new TdsException ("Bad integer length");
			}
		}

		[MonoTODO]
		private object GetMoneyValue (TdsColumnType type)
		{
			int len;
			object result;

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
				throw new TdsException ("not a money value");
			}

			if (len == 0)
				result = null;
			else {
				throw new NotImplementedException ();
			}

			return result;
		}

		private int GetSubPacketLength ()
		{
			return comm.GetTdsShort ();
		}

		private object GetTextValue (bool wideChars)
		{
			string result;
			byte hasValue = comm.GetByte ();

			if (hasValue == 0)
				return null;

			comm.Skip (24);
			int len = comm.GetTdsInt ();

			if (len >= 0) {
				if (wideChars)
					result = comm.GetString (len / 2);
				else
					result = encoder.GetString (comm.GetBytes (len, false), 0, len);

				if ((byte) tdsVersion < (byte) TdsVersion.tds70 && result == " ")
					result = "";
			} 
			else 
				throw new TdsException ("");

			return result;
		}

		private bool IsFixedSizeColumn (TdsColumnType columnType)
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
					throw new TdsException ("bad type");
			}
		}

		[MonoTODO]
		private TdsPacketRowResult LoadRow (TdsPacketRowResult result)
		{
			throw new NotImplementedException ();
		}

		private int LookupBufferSize (TdsColumnType columnType)
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
					throw new TdsException ("");
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
					throw new TdsException ("");
			}
		}

		private TdsPacketColumnNamesResult ProcessColumnNames ()
		{
			int totalLength = comm.GetTdsShort ();
			int bytesRead = 0;
			int i = 0;

			bool newTable = (table.Columns.Count == 0);

			while (bytesRead < totalLength) {
				int columnNameLength = comm.GetByte ();
				string columnName = encoder.GetString (comm.GetBytes (columnNameLength, false), 0, columnNameLength);
				bytesRead = bytesRead + 1 + columnNameLength;

				Console.WriteLine ("Column: {0}", columnName);

				if (newTable)
					table.Columns.Add (columnName);
				else
					table.Columns[i].ColumnName = columnName;
				i += 1;
			}

			return new TdsPacketColumnNamesResult (table.Columns);
		}

		private TdsPacketColumnInfoResult ProcessColumnInfo ()
		{
			int precision;
			int scale;
			int totalLength = comm.GetTdsShort ();
			int bytesRead = 0;
			int numColumns = 0;

			bool newTable = (table.Columns.Count > 0);

			while (bytesRead < totalLength) {
				scale = -1;
				precision = -1;

				int bufLength = -1;
				int dispSize = -1;
				byte[] flagData = new byte[4];
				for (int i = 0; i < 4; i += 1) {
					flagData[i] = comm.GetByte ();
					bytesRead += 1;
				}
				bool nullable = (flagData[2] & 0x01) > 0;
				bool caseSensitive = (flagData[2] & 0x02) > 0;
				bool writable = (flagData[2] & 0x0c) > 0;
				bool autoIncrement = (flagData[2] & 0x10) > 0;
				string tableName = String.Empty;
				TdsColumnType columnType = (TdsColumnType) comm.GetByte ();

				Console.WriteLine (columnType);

				bytesRead += 1;

				if (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image) {
					comm.Skip (4);
					bytesRead += 4;

					int tableNameLength = comm.GetTdsShort ();
					bytesRead += 2;
					tableName = encoder.GetString (comm.GetBytes (tableNameLength, false), 0, tableNameLength);
					bytesRead += tableNameLength;
					bufLength = 2 << 31 - 1;
				}
				else if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric) {
					bufLength = comm.GetByte ();
					bytesRead += 1;
					precision = comm.GetByte ();
					bytesRead += 1;
					scale = comm.GetByte ();
					bytesRead += 1;
				}
				else if (IsFixedSizeColumn (columnType))
					bufLength = LookupBufferSize (columnType);
				else {
					bufLength = (int) comm.GetByte () & 0xff;
					bytesRead += 1;
				}

				DataColumn column;

				if (newTable) 
					column = table.Columns.Add ();
				else
					column = table.Columns[numColumns];

				numColumns += 1;

				column.AllowDBNull = nullable;
				column.AutoIncrement = autoIncrement;
				column.ReadOnly = !writable;
			}

			int skipLength = totalLength - bytesRead;
			if (skipLength != 0)
				throw new TdsException ("skipping");
			return new TdsPacketColumnInfoResult (table.Columns);
		}

		[MonoTODO]
		private TdsPacketEndTokenResult ProcessEndToken (TdsPacketSubType type)
		{
			byte status = comm.GetByte ();
			comm.GetByte ();
			byte op = comm.GetByte ();
			comm.GetByte ();
			int rowCount = comm.GetTdsInt ();
			if (op == (byte) 0xc1) 
				rowCount = 0;

			if (type == TdsPacketSubType.DoneInProc) 
				rowCount = -1;

			TdsPacketEndTokenResult result = new TdsPacketEndTokenResult (type, status, rowCount);

			moreResults = result.MoreResults;

			// FIXME: Finish the query

			return result;
		}

		private TdsPacketResult ProcessEnvChange ()
		{
			int len = GetSubPacketLength ();
			TdsEnvPacketSubType type = (TdsEnvPacketSubType) comm.GetByte ();
			int cLen;

			switch (type) {
			case TdsEnvPacketSubType.BlockSize :
				string blockSize;
				cLen = comm.GetByte () & 0xff;
				if (tdsVersion == TdsVersion.tds70) {
					blockSize = comm.GetString (cLen);
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					blockSize = encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
					comm.Skip (len - 2 - cLen);
				}
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
					SetCharset (encoder.GetString (comm.GetBytes (cLen, false), 0, cLen));
					comm.Skip (len - 2 - cLen);
				}

				break;
			case TdsEnvPacketSubType.Database :
				cLen = comm.GetByte () & 0xff;
				string newDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
				cLen = comm.GetByte () & 0xff;
				string oldDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
				if (database != null && database != oldDB)
					throw new TdsException ("Database mismatch.");
				database = newDB;
				break;
			default:
				comm.Skip (len - 1);
				break;
			}

			return new TdsPacketResult (TdsPacketSubType.EnvChange);
		}

		private TdsPacketResult ProcessLoginAck ()
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

			return new TdsPacketResult (TdsPacketSubType.LoginAck);
		}

		private TdsPacketMessageResult ProcessMessage (TdsPacketSubType subType)
		{
			TdsMessage message = new TdsMessage ();
			GetSubPacketLength ();
			int len;

			message.Number = comm.GetTdsInt ();
			message.State = comm.GetByte ();
			message.Severity = comm.GetByte ();
			
			len = comm.GetTdsShort ();

			message.Message = comm.GetString (len);

			len = comm.GetByte ();

			message.Server = comm.GetString (len);

			if (subType == TdsPacketSubType.Error | subType == TdsPacketSubType.Info) {
				len = comm.GetByte ();
				message.ProcName = comm.GetString (len);
			}
			else 
				throw new TdsException ("Invalid subtype");
			message.Line = comm.GetByte ();
			comm.GetByte ();

			lastServerMessage = message;
			if (subType == TdsPacketSubType.Error)
				throw new TdsException (message.ToString ());

			Console.WriteLine (message.ToString ());

			return new TdsPacketMessageResult (subType, message);
		}

		private TdsPacketOutputParam ProcessOutputParam ()
		{
			GetSubPacketLength ();
			comm.GetString (comm.GetByte () & 0xff);
			comm.Skip (5);
			TdsColumnType colType = (TdsColumnType) comm.GetByte ();
			int len;

			object element = null;
			switch (colType) {
			case TdsColumnType.IntN :
				comm.GetByte ();
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Int1 :
			case TdsColumnType.Int2 :
			case TdsColumnType.Int4 :
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Image :
				comm.GetByte ();
				element = GetImageValue ();
				break;
			case TdsColumnType.Text :
				comm.GetByte ();
				element = GetTextValue (false);
				break;
			case TdsColumnType.NText :
				comm.GetByte ();
				element = GetTextValue (true);
				break;
			case TdsColumnType.Char :
			case TdsColumnType.VarChar :
				comm.GetByte ();
				element = GetStringValue (false, true);
				break;
			case TdsColumnType.BigVarBinary :
				comm.GetTdsShort ();
				len = comm.GetTdsShort ();
				element = comm.GetBytes (len, true);
				break;
			case TdsColumnType.BigVarChar :
				comm.GetTdsShort ();
				element = GetStringValue (false, false);
				break;
			case TdsColumnType.NChar :
			case TdsColumnType.NVarChar :
				comm.GetByte ();
				element = GetStringValue (true, true);
				break;
			case TdsColumnType.Real :
				element = ReadFloatN (4);
				break;
			case TdsColumnType.Float8 :
				element = ReadFloatN (8);
				break;
			case TdsColumnType.FloatN :
				comm.GetByte ();
				int actualSize = comm.GetByte ();
				element = ReadFloatN (actualSize);
				break;
			case TdsColumnType.SmallMoney :
			case TdsColumnType.Money :
			case TdsColumnType.MoneyN :
				comm.GetByte ();
				element = GetMoneyValue (colType);
				break;
			case TdsColumnType.Numeric :
			case TdsColumnType.Decimal :
				comm.GetByte ();
				comm.GetByte ();
				int scale = comm.GetByte ();
				element = GetDecimalValue (scale);
				break;
			case TdsColumnType.DateTimeN :
				comm.GetByte ();
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.DateTime4 :
			case TdsColumnType.DateTime :
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.VarBinary :
			case TdsColumnType.Binary :
				comm.GetByte ();
				len = (comm.GetByte () & 0xff);
				element = comm.GetBytes (len, true);
				break;
			case TdsColumnType.BitN :
				comm.GetByte ();
				if (comm.GetByte () == 0)
					element = null;
				else
					element = (comm.GetByte() != 0);
				break;
			case TdsColumnType.Bit :
				int columnSize = comm.GetByte ();
				element = (columnSize != 0);
				break;
			case TdsColumnType.UniqueIdentifier :
				len = comm.GetByte () & 0xff;
				//element = (len == 0 ? null : new Guid (comm.GetBytes (len, false)));
				break;
			default :
				throw new TdsException ("");
			}

			return new TdsPacketOutputParam (element);
		}

		private TdsPacketResult ProcessProcId ()
		{
			comm.Skip (8);
			return new TdsPacketResult (TdsPacketSubType.ProcId);
		}

		private TdsPacketRetStatResult ProcessReturnStatus ()
		{
			return new TdsPacketRetStatResult (comm.GetTdsInt ());
		}

		[MonoTODO]
		protected TdsPacketResult ProcessSubPacket ()
		{
			TdsPacketResult result = null;
			moreResults = false;

			TdsPacketSubType subType = (TdsPacketSubType) comm.GetByte ();

			switch (subType) {
			case TdsPacketSubType.EnvChange :
				result = ProcessEnvChange ();
				break;
			case TdsPacketSubType.Error :
			case TdsPacketSubType.Info :
			case TdsPacketSubType.Msg50Token :
				result = ProcessMessage (subType);
				break;
			case TdsPacketSubType.Param :
Console.WriteLine ("OUTPUT PARAMETER PACKET RECEIVED");
				result = ProcessOutputParam ();
				break;
			case TdsPacketSubType.LoginAck :
				result = ProcessLoginAck ();
				break;
			case TdsPacketSubType.ReturnStatus :
Console.WriteLine ("RETURN STATUS PACKET RECEIVED");
				result = ProcessReturnStatus ();
				break;
			case TdsPacketSubType.ProcId :
Console.WriteLine ("PROC ID PACKET RECEIVED");
				result = ProcessProcId ();
				break;
			case TdsPacketSubType.Done :
			case TdsPacketSubType.DoneProc :
			case TdsPacketSubType.DoneInProc :
				result = ProcessEndToken (subType);
				moreResults2 = ((TdsPacketEndTokenResult) result).MoreResults;
				break;
			case TdsPacketSubType.ColumnNameToken :
Console.WriteLine ("COLUMN NAME TOKEN PACKET RECEIVED");
				result = ProcessColumnNames ();
				break;
			case TdsPacketSubType.ColumnInfoToken :
Console.WriteLine ("COLUMN INFO TOKEN PACKET RECEIVED");
				result = ProcessColumnInfo ();
				break;
			case TdsPacketSubType.Unknown0xA5 :
			case TdsPacketSubType.Unknown0xA7 :
			case TdsPacketSubType.Unknown0xA8 :
Console.WriteLine ("UNKNOWN PACKET RECEIVED");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketUnknown (subType);
				break;
			case TdsPacketSubType.TableName :
Console.WriteLine ("TABLE NAME PACKET RECEIVED");
				result = ProcessTableName ();
				break;
			case TdsPacketSubType.Order :
Console.WriteLine ("COLUMN ORDER PACKET RECEIVED");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketColumnOrderResult ();
				break;
			case TdsPacketSubType.Control :
Console.WriteLine ("CONTROL PACKET RECEIVED");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketControlResult ();
				break;
			case TdsPacketSubType.Row :
Console.WriteLine ("ROW RESULT PACKET RECEIVED");
				result = LoadRow (new TdsPacketRowResult (null));
				break;
			case TdsPacketSubType.ColumnMetadata :
Console.WriteLine ("COLUMN METADATA PACKET RECEIVED");
				result = ProcessTds7Result ();
				break;
			default:
				throw new TdsException ("oops!");
			}

			return result;
		}

		private TdsPacketTableNameResult ProcessTableName ()
		{
			int totalLength = comm.GetTdsShort ();
			comm.Skip (totalLength);
			return new TdsPacketTableNameResult ();
		}

		[MonoTODO ("complete")]
		private TdsPacketResult ProcessTds7Result ()
		{
			int numColumns = comm.GetTdsShort ();

			return null;

		}

		private object ReadFloatN (int len)
		{
			object tmp;
			long l;
			switch (len) {
			case 8 :
				l = comm.GetTdsInt64 ();
				tmp = BitConverter.Int64BitsToDouble (l);
				break;
			case 4 :
				l = comm.GetTdsInt ();
				tmp = BitConverter.Int64BitsToDouble (l);
				break;
			case 0 :
				tmp = null;
				break;
			default:
				throw new TdsException ("");
			}

			return tmp;
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

		public void SubmitProcedure (string sql)
		{
			TdsPacketResult result;
			ExecuteQuery (sql);

			bool done = false;
			do {
				result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);
		}

		public void Disconnect ()
		{
		}

		public abstract bool Connect (TdsConnectionParameters connectionParameters);

		private string SqlStatementForSettings (bool autoCommit, IsolationLevel isolationLevel)
		{
			if (autoCommit == this.autoCommit && isolationLevel == this.isolationLevel)
				return null;
			StringBuilder res = new StringBuilder ();
			if (autoCommit != this.autoCommit) {
				this.autoCommit = autoCommit;
				res.Append (SqlStatementToSetCommit ());
				res.Append (' ');
			}
			if (isolationLevel != this.isolationLevel) {
				this.isolationLevel = isolationLevel;
				res.Append (SqlStatementToSetIsolationLevel ());
				res.Append (' ');
			}
			return res.ToString ();
		}

		private string SqlStatementToSetCommit ()
		{
			string result;
			if (serverType == TdsServerType.Sybase) {
				if (autoCommit) 
					result = "set CHAINED off";
				else
					result = "set CHAINED on";
			}
			else {
				if (autoCommit)
					result = "set implicit_transactions off";
				else
					result = "set implicit_transactions on";
			}
			return result;
		}

		[MonoTODO]
		private string SqlStatementToSetIsolationLevel ()
		{
			string result = "";
			return result;
		}

		private static string Tds7CryptPass (string pass)
		{
			int xormask = 0x5a5a;
			int len = pass.Length;
			char[] chars = new char[len];

			for (int i = 0; i < len; ++i) {
				int c = ((int) (pass[i])) ^ xormask;
				int m1 = (c >> 4) & 0x0f0f;
				int m2 = (c << 4) & 0xf0f0;
				chars[i] = (char) (m1 | m2);
			}

			return new String (chars);
		}

		#endregion // Methods
	}
}
