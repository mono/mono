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
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Data.TdsClient.Internal {
        internal class Tds
	{
		#region Fields

		TdsVersion tdsVersion;

		string applicationName;
		string database = String.Empty;
		string connectDB;
		string charset;
		string hostname;
		string server;
		string language;
		string libraryName;
		int packetSize;
		string password;
		int port;
		string progName;
		string user;

		string databaseProductName;
		string databaseProductVersion;
		int databaseMajorVersion;

		DataTable table = new DataTable ();

		bool moreResults;
		bool moreResults2;

		TdsMessage lastServerMessage;

		Encoding encoder;
		TdsServerType serverType;
		TdsComm comm;
		TdsConnectionParameters parms;
		//TdsCommand command;
		IsolationLevel isolationLevel;
		bool autoCommit;
                Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		bool inUse = false;

		#endregion // Fields

		#region Properties

		//public TdsCommandInternal Command {
			//get { return command; }
			//set { command = value; }
		//}

		public string Database {
			get { return database; }
			set { database = value; }
		}

		public TdsVersion TdsVersion {
			get { return tdsVersion; }
		}

		public bool InUse {
			get { return inUse; }
			set { inUse = value; }
		}

		#endregion // Properties

		#region Constructors

		public Tds (TdsConnectionParameters parms)
		{
			applicationName = parms.ApplicationName;
			connectDB = parms.Database;
			encoder = Encoding.GetEncoding (parms.Encoding);
			charset = parms.Encoding;
			hostname = parms.Hostname;
			server = parms.DataSource;
			language = parms.Language;
			libraryName = parms.LibraryName;
			packetSize = parms.PacketSize;
			password = parms.Password;
			port = parms.Port;
			progName = parms.ProgName;
			tdsVersion = parms.TdsVersion;
			user = parms.User;

			IPHostEntry hostEntry = Dns.Resolve (server);
			IPAddress[] addresses = hostEntry.AddressList;

			IPEndPoint endPoint;

			foreach (IPAddress address in addresses) {
				endPoint = new IPEndPoint (address, port);
				socket.Connect (endPoint);

				if (socket.Connected)
					break;
			}
	
			comm = new TdsComm (encoder, socket, packetSize, tdsVersion);
		}	

		#endregion // Constructors

		#region Methods
		
		public void BeginTransaction ()
		{
			SubmitProcedure ("BEGIN TRANSACTION");
		}
		public void ChangeDatabase (string databaseName)
		{
			TdsPacketResult result;
			bool isOkay;

			string query = String.Format ("use {0}", databaseName);
			comm.StartPacket (TdsPacketType.Query);
			comm.Append (query);
			comm.SendPacket ();

			while (!((result = ProcessSubPacket ()) is TdsPacketEndTokenResult)) {
				if (result is TdsPacketErrorResult) {
					isOkay = false;
				}
			}
		}

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
			while (!done) {
				result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult) && !((TdsPacketEndTokenResult) result).MoreResults;
				if (result is TdsPacketErrorResult) {
					done = true;
					isOkay = false;
				}
			}

			return isOkay;
		}

		public void CommitTransaction ()
		{
			string sql = "IF @@TRANCOUNT>0 COMMIT TRAN";
			SubmitProcedure (sql);
		}

		[MonoTODO ("fixme")]
		public int ExecuteNonQuery (string sql)
		{
			TdsPacketResult result;
			bool done = false;

			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				moreResults2 = true;
				comm.SendPacket ();
			}

			do {
				result = ProcessSubPacket ();
				if (result is TdsPacketMessageResult) {
					Console.WriteLine (((TdsPacketMessageResult) result).Message);
				}
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);

			return -1;
		}

		[MonoTODO ("fixme")]
		public void ExecuteQuery (string sql)
		{
			if (sql.Length > 0) {
				comm.StartPacket (TdsPacketType.Query);
				comm.Append (sql);
				moreResults2 = true;
				comm.SendPacket ();
			}
		}

		private object GetCharValue (bool wideChars, bool outputParam)
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

		[MonoTODO]
		private object GetDecimalValue (int scale)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private object GetDateTimeValue (TdsColumnType type)
		{
			throw new NotImplementedException ();
		}

		private object GetImageValue ()
		{
			byte[] result;
			byte hasValue = comm.GetByte ();
			if (hasValue == 0)
				return null;
			
			comm.Skip (24);
			int len = comm.GetTdsInt ();
			if (len >= 0) 
				result = comm.GetBytes (len, true);
			else
				throw new TdsException ("");
			return result;
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
				result = comm.GetTdsInt ();
				break;
			case 2 :
				result = comm.GetTdsShort ();
				break;
			case 1 :
				result = (byte) comm.GetByte ();
				break;
			case 0 :
				result = null;
				break;
			default:
				throw new TdsException ("Bad integer length");
			}

			return result;
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

			bool newTable = (table.Columns.Count > 0);

			while (bytesRead < totalLength) {
				int columnNameLength = comm.GetByte ();
				string columnName = encoder.GetString (comm.GetBytes (columnNameLength, false), 0, columnNameLength);
				bytesRead = bytesRead + 1 + columnNameLength;
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

			Console.WriteLine ("Row count {0}", rowCount);

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
					this.language = comm.GetString (cLen);
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					this.charset = encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
					comm.Skip (len - 2 - cLen);
					SetCharset (charset);
				}

				break;
			case TdsEnvPacketSubType.Database :
				cLen = comm.GetByte () & 0xff;
				string newDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
				cLen = comm.GetByte () & 0xff;
				string oldDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoder.GetString (comm.GetBytes (cLen, false), 0, cLen);
				if (database != String.Empty && database != oldDB)
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
			Console.WriteLine ("Connected to {0} {1}", databaseProductName, databaseProductVersion);

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

			Console.WriteLine (message.ToString ());

			lastServerMessage = message;
			if (subType == TdsPacketSubType.Error)
				return new TdsPacketErrorResult (subType, message);
			else
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
				element = GetCharValue (false, true);
				break;
			case TdsColumnType.BigVarBinary :
				comm.GetTdsShort ();
				len = comm.GetTdsShort ();
				element = comm.GetBytes (len, true);
				break;
			case TdsColumnType.BigVarChar :
				comm.GetTdsShort ();
				element = GetCharValue (false, false);
				break;
			case TdsColumnType.NChar :
			case TdsColumnType.NVarChar :
				comm.GetByte ();
				element = GetCharValue (true, true);
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
		private TdsPacketResult ProcessSubPacket ()
		{
			TdsPacketResult result = null;
			moreResults = false;

			TdsPacketSubType subType = (TdsPacketSubType) comm.GetByte ();

			switch (subType) {
			case TdsPacketSubType.EnvChange :
Console.WriteLine ("Environment change");
				result = ProcessEnvChange ();
				break;
			case TdsPacketSubType.Error :
			case TdsPacketSubType.Info :
			case TdsPacketSubType.Msg50Token :
Console.WriteLine ("Error/info/message");
				result = ProcessMessage (subType);
				break;
			case TdsPacketSubType.Param :
Console.WriteLine ("Param");
				result = ProcessOutputParam ();
				break;
			case TdsPacketSubType.LoginAck :
Console.WriteLine ("Login Ack");
				result = ProcessLoginAck ();
				break;
			case TdsPacketSubType.ReturnStatus :
Console.WriteLine ("Return Status");
				result = ProcessReturnStatus ();
				break;
			case TdsPacketSubType.ProcId :
Console.WriteLine ("Proc Id");
				result = ProcessProcId ();
				break;
			case TdsPacketSubType.Done :
			case TdsPacketSubType.DoneProc :
			case TdsPacketSubType.DoneInProc :
Console.WriteLine ("Done");
				result = ProcessEndToken (subType);
				moreResults2 = ((TdsPacketEndTokenResult) result).MoreResults;
				break;
			case TdsPacketSubType.ColumnNameToken :
Console.WriteLine ("Column Name");
				result = ProcessColumnNames ();
				break;
			case TdsPacketSubType.ColumnInfoToken :
Console.WriteLine ("Column Info");
				result = ProcessColumnInfo ();
				break;
			case TdsPacketSubType.Unknown0xA5 :
			case TdsPacketSubType.Unknown0xA7 :
			case TdsPacketSubType.Unknown0xA8 :
Console.WriteLine ("Unknown");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketUnknown (subType);
				break;
			case TdsPacketSubType.TableName :
Console.WriteLine ("Table Name");
				result = ProcessTableName ();
				break;
			case TdsPacketSubType.Order :
Console.WriteLine ("Order");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketColumnOrderResult ();
				break;
			case TdsPacketSubType.Control :
Console.WriteLine ("Control");
				comm.Skip (comm.GetTdsShort ());
				result = new TdsPacketControlResult ();
				break;
			case TdsPacketSubType.Row :
Console.WriteLine ("Row");
				result = LoadRow (new TdsPacketRowResult (null));
				break;
			case TdsPacketSubType.ColumnMetadata :
Console.WriteLine ("Column Metadata");
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

		[MonoTODO]
		public void RollbackTransaction ()
		{
			SubmitProcedure ("IF @@TRANCOUNT>0 ROLLBACK TRAN");
		}

		[MonoTODO]
		public void SaveTransaction (string savePointName)
		{
			string sql = String.Format ("SAVE TRAN {0}", savePointName);
			SubmitProcedure (sql);
		}

		private void SetCharset (string charset)
		{
			if (charset == null || charset.Length > 30)
				charset = "iso-8859-1";
			if (this.charset != charset) {
				encoder = Encoding.GetEncoding (charset);
				this.charset = charset;
			}
		}

		public void SubmitProcedure (string sql)
		{
			TdsPacketResult result;
			ExecuteQuery (sql);
			bool done;
			do {
				result = ProcessSubPacket ();
				if (result is TdsPacketMessageResult) {
					Console.WriteLine (((TdsPacketMessageResult) result).Message);
				}
				done = (result is TdsPacketEndTokenResult) && (!((TdsPacketEndTokenResult) result).MoreResults);
				
			} while (!done);
		}

		public void Close ()
		{
		}

		public bool Logon (TdsConnectionParameters parms)
		{
			byte pad = (byte) 0;
			byte[] empty = new byte[0];
			bool isOkay = true;

			if (tdsVersion == TdsVersion.tds70) {
				Send70Logon (parms);
			} else {
				comm.StartPacket (TdsPacketType.Logon);

				// hostname (offset 0)
				byte[] tmp = comm.Append (hostname, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// username (offset 31 0x1f)
				tmp = comm.Append (user, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// password (offset 62 0x3e)
				tmp = comm.Append (password, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// hostproc (offset 93 0x5d)
				comm.Append ("00000116", 8, pad);

				// unused (offset 109 0x6d)
				comm.Append (empty, (30-14), pad);

				// apptype 
				comm.Append ((byte) 0x0);
				comm.Append ((byte) 0xa0);
				comm.Append ((byte) 0x24);
				comm.Append ((byte) 0xcc);
				comm.Append ((byte) 0x50);
				comm.Append ((byte) 0x12);

				// hostproc length 
				comm.Append ((byte) 8);

				// type of int2
				comm.Append ((byte) 3);

				// type of int4
				comm.Append ((byte) 1);

				// type of char
				comm.Append ((byte) 6);

				// type of flt
				comm.Append ((byte) 10);

				// type of date
				comm.Append ((byte) 9);
				
				// notify of use db
				comm.Append ((byte) 1);

				// disallow dump/load and bulk insert
				comm.Append ((byte) 1);

				// sql interface type
				comm.Append ((byte) 0);

				// type of network connection
				comm.Append ((byte) 0);


				// spare [7]
				comm.Append (empty, 7, pad);
				// appname
				tmp = comm.Append (applicationName, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// server name
				tmp = comm.Append (server, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// remote passwords
				comm.Append (empty, 2, pad);
				tmp = comm.Append (password, 253, pad);
				comm.Append ((byte) (tmp.Length < 253 ? tmp.Length + 2 : 253 + 2));

				// tds version
				comm.Append ((byte) (((byte) tdsVersion) / 10));
				comm.Append ((byte) (((byte) tdsVersion) % 10));
				comm.Append ((byte) 0);
				comm.Append ((byte) 0);

				// prog name
				tmp = comm.Append (progName, 10, pad);
				comm.Append ((byte) (tmp.Length < 10 ? tmp.Length : 10));

				// prog version
				comm.Append ((byte) 6);

				// Tell the server we can handle SQLServer version 6
				comm.Append ((byte) 0);

				// Send zero to tell the server we can't handle any other version
				comm.Append ((byte) 0);
				comm.Append ((byte) 0);

				// auto convert short
				comm.Append ((byte) 0);

				// type of flt4
				comm.Append ((byte) 0x0d);

				// type of date4
				comm.Append ((byte) 0x11);

				// language
				tmp = comm.Append (language, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// notify on lang change
				comm.Append ((byte) 1);

				// security label hierarchy
				comm.Append ((short) 0);

				// security components
				comm.Append (empty, 8, pad);

				// security spare
				comm.Append ((short) 0);

				// security login role
				comm.Append ((byte) 0);

				// charset
				tmp = comm.Append (charset, 30, pad);
				comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// notify on charset change
				comm.Append ((byte) 1);

				// length of tds packets
				tmp = comm.Append (packetSize.ToString (), 6, pad);
				comm.Append ((byte) 3);

				// pad out to a longword
				comm.Append (empty, 8, pad);
			}

			comm.SendPacket ();

			TdsPacketResult result;

			while (!((result = ProcessSubPacket()) is TdsPacketEndTokenResult)) {
				if (result is TdsPacketErrorResult) {
					isOkay = false;
				}
				// XXX Should really process some more types of packets.
			}

			if (isOkay) {
				// XXX Should we move this to the Connection class?
				//isOkay = initSettings(_database);
			}

			// XXX Possible bug.  What happend if this is cancelled before the logon
			// takes place?  Should isOkay be false?
			return isOkay;
			
		}

		// This packet is documented at 
		// http://www.freetds.org/tds.htm#login7
		public void Send70Logon (TdsConnectionParameters parms)
		{
			byte[] empty = new byte[0];
			byte pad = (byte) 0;

			byte[] magic1 = {0x06, 0x83, 0xf2, 0xf8, 0xff, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x03, 0x00, 0x00, 0x88, 0xff, 0xff, 0xff, 0x36, 0x04, 0x00, 0x00};
			byte[] magic2 = {0x00, 0x40, 0x33, 0x9a, 0x6b, 0x50};
			byte[] magic3 = {0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50}; // NTLMSSP
			short partialPacketSize = (short) (86 + 2 * (
					hostname.Length + 
					user.Length + 
					applicationName.Length + 
					password.Length + 
					server.Length +
					libraryName.Length +
					language.Length +
					connectDB.Length)); 
			short totalPacketSize = (short) (partialPacketSize + 48);
			comm.StartPacket (TdsPacketType.Logon70);
			comm.Append (totalPacketSize);
			comm.Append (empty, 5, pad);

			if (tdsVersion == TdsVersion.tds80)
				comm.Append ((byte) 0x80);
			else
				comm.Append ((byte) 0x70);

			comm.Append (empty, 7, pad);
			comm.Append (magic1);

			short curPos = 86;

			// Hostname 
			comm.Append (curPos);
			comm.Append ((short) hostname.Length);
			curPos += (short) (hostname.Length * 2);

			// Username
			comm.Append (curPos);
			comm.Append ((short) user.Length);
			curPos += (short) (user.Length * 2);

			// Password
			comm.Append (curPos);
			comm.Append ((short) password.Length);
			curPos += (short) (password.Length * 2);

			// AppName
			comm.Append (curPos);
			comm.Append ((short) applicationName.Length);
			curPos += (short) (applicationName.Length * 2);

			// Server Name
			comm.Append (curPos);
			comm.Append ((short) server.Length);
			curPos += (short) (server.Length * 2);

			// Unknown
			comm.Append ((short) 0);
			comm.Append ((short) 0);

			// Library Name
			comm.Append (curPos);
			comm.Append ((short) libraryName.Length);
			curPos += (short) (libraryName.Length * 2);

			// Character Set
			comm.Append (curPos);
			comm.Append ((short) language.Length);
			curPos += (short) (language.Length * 2);

			// Database
			comm.Append (curPos);
			comm.Append ((short) connectDB.Length);
			curPos += (short) (connectDB.Length * 2);

			comm.Append (magic2);
			comm.Append (partialPacketSize);
			comm.Append ((short) 48);
			comm.Append (totalPacketSize);
			comm.Append ((short) 0);

			string scrambledPwd = Tds7CryptPass (password);

			comm.Append (hostname);
			comm.Append (user);
			comm.Append (scrambledPwd);
			comm.Append (applicationName);
			comm.Append (server);
			comm.Append (libraryName);
			comm.Append (language);
			comm.Append (connectDB);
			comm.Append (magic3);

			comm.Append ((byte) 0x0);
			comm.Append ((byte) 0x1);
			comm.Append (empty, 3, pad);
			comm.Append ((byte) 0x6);
			comm.Append ((byte) 0x82);
			comm.Append (empty, 22, pad);
			comm.Append ((byte) 0x30);
			comm.Append (empty, 7, pad);
			comm.Append ((byte) 0x30);
			comm.Append (empty, 3, pad);
		}

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
