//
// Mono.Data.TdsClient.Internal.Tds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
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

		bool connected = false;
		bool moreResults;

		TdsMessage lastServerMessage;

		Encoding encoder;
		TdsServerType serverType;
		IsolationLevel isolationLevel;
		bool autoCommit;
                Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		bool doneProc;
		TdsPacketRowResult currentRow = null;
		TdsPacketColumnNamesResult columnNames;
		TdsPacketColumnInfoResult columnInfo;
		TdsPacketErrorResultCollection errors = new TdsPacketErrorResultCollection ();


		bool queryInProgress;
		int cancelsRequested;
		int cancelsProcessed;

		bool isDone;
		bool isDoneInProc;

		ArrayList outputParameters = new ArrayList ();

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

		protected TdsPacketColumnNamesResult ColumnNames {
			get { return columnNames; }
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

		public TdsPacketErrorResultCollection Errors {
			get { return errors; }
		}

		public bool IsConnected {
			get { return connected; }
			set { connected = value; }
		}

		public bool MoreResults {
			get { return moreResults; }
		}

		public int PacketSize {
			get { return packetSize; }
		}

		public string ServerVersion {
			get { return databaseProductVersion; }
		}

		public TdsPacketColumnInfoResult Schema {
			get { return columnInfo; }
		}

		public TdsPacketRowResult ColumnValues {
			get { return currentRow; }
		}

		public TdsVersion TdsVersion {
			get { return tdsVersion; }
		}

		public ArrayList OutputParameters {
			get { return outputParameters; }
			set { outputParameters = value; }
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

		public void Disconnect ()
		{
			TdsPacketResult result = null;

			comm.StartPacket (TdsPacketType.Logoff);
			comm.Append ((byte) 0);
			comm.SendPacket ();	

			bool done = false;
			do {
				result = ProcessSubPacket ();
				if (result != null) {
					switch (result.GetType ().ToString ()) {
					case "Mono.Data.TdsClient.Internal.TdsPacketEndTokenResult" :
						done = !((TdsPacketEndTokenResult) result).MoreResults;
						break;
					}
				}
			} while (!done);
		}

		public int ExecuteNonQuery (string sql)
		{
			TdsPacketResult result = null;
			doneProc = false;

			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				comm.SendPacket ();
			}

			bool done = false;
			while (!done) {
				result = ProcessSubPacket ();

				if (result != null) {
					switch (result.GetType ().ToString ()) {
					case "Mono.Data.TdsClient.Internal.TdsPacketColumnNamesResult" :
						columnNames = (TdsPacketColumnNamesResult) result;
						break;
					case "Mono.Data.TdsClient.Internal.TdsPacketColumnInfoResult" :
						columnInfo = (TdsPacketColumnInfoResult) result;
						break;
					case "Mono.Data.TdsClient.Internal.TdsPacketRowResult" :
						currentRow = (TdsPacketRowResult) result;
						break;
					case "Mono.Data.TdsClient.Internal.TdsPacketEndTokenResult" :
						done = !((TdsPacketEndTokenResult) result).MoreResults;
						break;
					}
				}
			}

			if (sql.Trim ().ToUpper ().StartsWith ("SELECT"))
				return -1;
			else
				return ((TdsPacketEndTokenResult) result).RowCount;
		}

		public void ExecuteQuery (string sql)
		{
			moreResults = true;
			doneProc = false;
			outputParameters.Clear ();

			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				comm.SendPacket ();
			}
		}

		public bool NextResult ()
		{
			if (!moreResults)
				return false;
			TdsPacketResult result = null;

			bool done = false;
			while (!done) {
				result = ProcessSubPacket ();

				if (result != null) {
					switch (result.GetType ().ToString ()) {
					case "Mono.Data.TdsClient.Internal.TdsPacketColumnNamesResult" :
						columnNames = (TdsPacketColumnNamesResult) result;
						break;
					case "Mono.Data.TdsClient.Internal.TdsPacketColumnInfoResult" :
						columnInfo = (TdsPacketColumnInfoResult) result;
						return true;
					case "Mono.Data.TdsClient.Internal.TdsPacketRowResult" :
						currentRow = (TdsPacketRowResult) result;
						break;
					case "Mono.Data.TdsClient.Internal.TdsPacketEndTokenResult" :
						done = !((TdsPacketEndTokenResult) result).MoreResults;
						break;
					}
				}
			}
			return false;
		}

		public bool NextRow ()
		{
			TdsPacketResult result = null;
			bool done = false;
			do {
				result = ProcessSubPacket ();
				if (result != null) {
					switch (result.GetType ().ToString ()) {
					case "Mono.Data.TdsClient.Internal.TdsPacketRowResult" :
						currentRow = (TdsPacketRowResult) result;
						return true;
					case "Mono.Data.TdsClient.Internal.TdsPacketEndTokenResult" :
						return false;
					}
				}
			} while (!done);

			return false;
		}

		public void SkipToEnd ()
		{
			while (moreResults)
				NextResult ();
		}

		#endregion // Public Methods

		#region // Private Methods

		private void FinishQuery (bool wasCancelled, bool moreResults)
		{
			if (!moreResults)
				queryInProgress = false;
			if (wasCancelled)
				cancelsProcessed += 1;
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
					comm.GetByte (); // column size
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Int1 :
			case TdsColumnType.Int2 :
			case TdsColumnType.Int4 :
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Image :
				if (outParam) 
					comm.GetByte (); // column size
				element = GetImageValue ();
				break;
			case TdsColumnType.Text :
				if (outParam) 
					comm.GetByte (); // column size
				element = GetTextValue (false);
				break;
			case TdsColumnType.NText :
				if (outParam) 
					comm.GetByte (); // column size
				element = GetTextValue (true);
				break;
			case TdsColumnType.Char :
			case TdsColumnType.VarChar :
				if (outParam)
					comm.GetByte (); // column size
				element = GetStringValue (false, false);
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
				if (outParam) 
					comm.GetByte (); // column size
				element = GetStringValue (true, false);
				break;
			case TdsColumnType.Real :
				element = ReadFloatN (4);
				break;
			case TdsColumnType.Float8 :
				element = ReadFloatN (8);
				break;
			case TdsColumnType.FloatN :
				if (outParam) 
					comm.GetByte (); // column size
				int actualSize = comm.GetByte ();
				element = ReadFloatN (actualSize);
				break;
			case TdsColumnType.SmallMoney :
			case TdsColumnType.Money :
				element = GetMoneyValue (colType);
				break;
			case TdsColumnType.MoneyN :
				if (outParam)
					comm.GetByte (); // column size
				element = GetMoneyValue (colType);
				break;
			case TdsColumnType.Numeric :
			case TdsColumnType.Decimal :
				byte scale;
				if (outParam) {
					comm.GetByte (); // column size
					comm.GetByte (); // precision
					scale = comm.GetByte ();
				}
				else 
					scale = columnInfo[ordinal].NumericScale;

				element = GetDecimalValue (scale);
				break;
			case TdsColumnType.DateTimeN :
				if (outParam) 
					comm.GetByte (); // column size
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.DateTime4 :
			case TdsColumnType.DateTime :
				element = GetDateTimeValue (colType);
				break;
			case TdsColumnType.VarBinary :
			case TdsColumnType.Binary :
				if (outParam) 
					comm.GetByte (); // column size
				element = GetBinaryValue ();
				break;
			case TdsColumnType.BitN :
				if (outParam) 
					comm.GetByte (); // column size
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
				return null;
			}

			return element;
		}

		private object GetBinaryValue ()
		{
			int len;
			object result = null;
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
			int len;
			object result = null;
			
			if (type == TdsColumnType.DateTimeN)
				len = comm.GetByte ();
			else if (type == TdsColumnType.DateTime4)
				len = 4;
			else
				len = 8;
			
			switch (len) {
			case 8 :
				int tdsDaysInt = comm.GetTdsInt ();
				int tdsTimeInt = comm.GetTdsInt ();
				result = new DateTime (1900,1,1);
				result = ((DateTime) result).AddDays (tdsDaysInt);
				if (tdsTimeInt != 0) {
					result = ((DateTime) result).AddSeconds (tdsTimeInt);
					result = TimeZone.CurrentTimeZone.ToLocalTime ((DateTime) result);
				}
				break;
			case 4 :
				short tdsDaysShort = comm.GetTdsShort ();
				short tdsTimeShort = comm.GetTdsShort ();
				result = new DateTime (1900,1,1);
				result = ((DateTime) result).AddDays ((int) tdsDaysShort);
				if (tdsTimeShort != 0) {
					result = ((DateTime) result).AddSeconds ((int) tdsTimeShort);
					result = TimeZone.CurrentTimeZone.ToLocalTime ((DateTime) result);
				}
				break;
			default :
				break;
			}

			return result;
		}

		private object GetDecimalValue (byte scale)
		{
			int[] bits = new int[3] {0,0,0};

			int len = (comm.GetByte() & 0xff) - 1;
			bool positive = (comm.GetByte () == 1);

			if (len > 16)
				throw new OverflowException ();

			for (int i = 0, index = 0; i < len && i < 12; i += 4, index += 1) 
				bits[index] = comm.GetTdsInt ();

			return new Decimal (bits[0], bits[1], bits[2], !positive, scale);
		}

		private object GetImageValue ()
		{
			byte hasValue = comm.GetByte ();
			if (hasValue == 0)
				return SqlBinary.Null;
			
			comm.Skip (24);
			int len = comm.GetTdsInt ();

			if (len < 0)
				return null;

			return new SqlBinary (comm.GetBytes (len, true));
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
				return null;
			}

			switch (len) {
			case 4 :
				return (comm.GetTdsInt ());
			case 2 :
				return (comm.GetTdsShort ());
			case 1 :
				return (comm.GetByte ());
			default:
				return null;
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
				return null;
			}

			if (len == 0)
				result = null;
			else {
				throw new NotImplementedException ();
			}

			return result;
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
					result = comm.GetString (len, false);

				if (tdsVersion < TdsVersion.tds70 && ((string) result).Equals (" "))
					result = "";
			}
			else
				result = null;
			return result;
		}

		private int GetSubPacketLength ()
		{
			return comm.GetTdsShort ();
		}

		private object GetTextValue (bool wideChars)
		{
			string result = null;
			byte hasValue = comm.GetByte ();

			if (hasValue == 0)
				return null;

			comm.Skip (24);
			int len = comm.GetTdsInt ();

			if (len >= 0) {
				if (wideChars)
					result = comm.GetString (len / 2);
				else
					result = comm.GetString (len, false);
					len /= 2;

				if ((byte) tdsVersion < (byte) TdsVersion.tds70 && result == " ")
					result = "";
			} 

			return result;
		}

		protected bool IsFixedSizeColumn (TdsColumnType columnType)
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

		private TdsPacketRowResult LoadRow (TdsContext context)
		{
			TdsPacketRowResult result = new TdsPacketRowResult (context);

			int i = 0;
			foreach (TdsSchemaInfo schema in columnInfo) {
				object o = GetColumnValue (schema.ColumnType, false, i);
				result.Add (o);
				i += 1;
			}

			return result;
		}

		protected int LookupBufferSize (TdsColumnType columnType)
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

		protected abstract TdsPacketColumnInfoResult ProcessColumnInfo ();

		private TdsPacketColumnNamesResult ProcessColumnNames ()
		{
			TdsPacketColumnNamesResult result = new TdsPacketColumnNamesResult ();

			int totalLength = comm.GetTdsShort ();
			int bytesRead = 0;
			int i = 0;

			while (bytesRead < totalLength) {
				int columnNameLength = comm.GetByte ();
				string columnName = comm.GetString (columnNameLength);
				bytesRead = bytesRead + 1 + columnNameLength;
				result.Add (columnName);
				i += 1;
			}

			return result;
		}

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

			if (type == TdsPacketSubType.DoneProc)
				doneProc = true;

			moreResults = result.MoreResults;

			FinishQuery (result.Cancelled, result.MoreResults);

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
				return null;

			message.Line = comm.GetByte ();
			comm.GetByte ();

			lastServerMessage = message;
			if (subType == TdsPacketSubType.Error) 
				errors.Add (new TdsPacketErrorResult (subType, message));

			return new TdsPacketMessageResult (subType, message);
		}

		private TdsPacketOutputParam ProcessOutputParam ()
		{
			GetSubPacketLength ();
			comm.GetString (comm.GetByte () & 0xff);
			comm.Skip (5);

			TdsColumnType colType = (TdsColumnType) comm.GetByte ();
			object value = GetColumnValue (colType, true);

			outputParameters.Add (value);
			return null;
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
				result = ProcessOutputParam ();
				break;
			case TdsPacketSubType.LoginAck :
				result = ProcessLoginAck ();
				break;
			case TdsPacketSubType.ReturnStatus :
				result = ProcessReturnStatus ();
				break;
			case TdsPacketSubType.ProcId :
				result = ProcessProcId ();
				break;
			case TdsPacketSubType.Done :
			case TdsPacketSubType.DoneProc :
			case TdsPacketSubType.DoneInProc :
				result = ProcessEndToken (subType);
				break;
			case TdsPacketSubType.ColumnNameToken :
				result = ProcessProcId ();
				result = ProcessColumnNames ();
				break;
			case TdsPacketSubType.ColumnInfoToken :
			case TdsPacketSubType.ColumnMetadata :
				result = ProcessColumnInfo ();
				break;
			case TdsPacketSubType.Unknown0xA5 :
			case TdsPacketSubType.Unknown0xA7 :
			case TdsPacketSubType.Unknown0xA8 :
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketUnknown (subType);
				break;
			case TdsPacketSubType.TableName :
				result = ProcessTableName ();
				break;
			case TdsPacketSubType.Order :
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketColumnOrderResult ();
				break;
			case TdsPacketSubType.Control :
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketControlResult ();
				break;
			case TdsPacketSubType.Row :
				result = LoadRow (null);
				break;
			default:
				return null;
			}

			return result;
		}

		private TdsPacketTableNameResult ProcessTableName ()
		{
			int totalLength = comm.GetTdsShort ();
			comm.Skip (totalLength);
			return new TdsPacketTableNameResult ();
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
				return null;
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

		#endregion // Private Methods
	}

}
