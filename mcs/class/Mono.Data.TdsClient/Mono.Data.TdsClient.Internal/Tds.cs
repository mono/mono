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
		string database;
		string charset;
		string host;
		string language;
		string libraryName;
		int packetSize;
		string password;
		string progName;
		string user;

		string databaseProductName;
		string databaseProductVersion;
		int databaseMajorVersion;

		DataTable table = new DataTable ();

		bool moreResults;
		bool moreResults2;

		Encoding encoding;
		TdsServerType serverType;
		TdsComm comm;
		TdsConnectionParameters parms;
		//TdsCommand command;
		IsolationLevel isolationLevel;
		bool autoCommit;
                Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

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

		#endregion // Properties

		#region Constructors

		public Tds (TdsConnectionParameters parms)
		{
			this.applicationName = parms.ApplicationName;
			this.database = parms.Database;
			this.encoding = Encoding.GetEncoding (parms.Encoding);
			this.charset = parms.Encoding;
			this.host = parms.Host;
			this.language = parms.Language;
			this.libraryName = parms.LibraryName;
			this.packetSize = parms.PacketSize;
			this.password = parms.Password;
			this.progName = parms.ProgName;
			this.tdsVersion = parms.TdsVersion;
			this.user = parms.User;

                        IPHostEntry hostEntry = Dns.GetHostByName (parms.Host);
                        IPAddress[] addresses = hostEntry.AddressList;

                        IPEndPoint endPoint;

                        foreach (IPAddress address in addresses) {
                                endPoint = new IPEndPoint (address, parms.Port);
                                socket.Connect (endPoint);

                                if (socket.Connected)
                                        break;
                        }
	
			comm = new TdsComm (socket, parms.PacketSize, tdsVersion);
		}	

		#endregion // Constructors

		#region Methods

		public void ChangeDatabase (string databaseName)
		{
                        string query = String.Format ("use {0}", databaseName);
                        comm.StartPacket (TdsPacketType.Query);
                        if (tdsVersion == TdsVersion.tds70)
                                comm.AppendChars (query);
                        else {
                                byte[] queryBytes = encoding.GetBytes (query);
                                comm.AppendBytes (queryBytes, queryBytes.Length, (byte) 0);
                        }
                        comm.SendPacket ();
		}

		public void ChangeSettings (bool autoCommit, IsolationLevel isolationLevel)
		{
			string query = SqlStatementForSettings (autoCommit, isolationLevel);
			if (query != null)
				ChangeSettings (query);
		}

		private bool ChangeSettings (string query)
		{
			bool isOkay = true;
			if (query.Length == 0)
				return true;

			comm.StartPacket (TdsPacketType.Query);
			if (tdsVersion == TdsVersion.tds70) {
				comm.AppendChars (query);
			}
			else {
				byte[] queryBytes = encoding.GetBytes (query);
				comm.AppendBytes (queryBytes, queryBytes.Length, (byte) 0);
			}
			comm.SendPacket ();

			return isOkay;
		}

		private object GetCharValue (bool wideChars, bool outputParam)
		{
			object result = null;
			bool shortLen = (tdsVersion == TdsVersion.tds70) && (wideChars || !outputParam);
			int len = shortLen ? comm.GetTdsShort () : (comm.GetByte () & 0xff);

			if (((byte) tdsVersion < (byte) TdsVersion.tds70 && len == 0) || (tdsVersion == TdsVersion.tds70 && len == 0xffff))
				result = null;
			else if (len >= 0) {
				if (wideChars)
					result = comm.GetString (len / 2);
				else
					result = encoding.GetString (comm.GetBytes (len, false), 0, len);

				if ((byte) tdsVersion < (byte) TdsVersion.tds70 && result == " ")
					result = "";
			}
			else
				throw new TdsException ("");
			return result;
		}

		[System.MonoTODO]
		private object GetDecimalValue (int scale)
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
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

		[System.MonoTODO]
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
					result = encoding.GetString (comm.GetBytes (len, false), 0, len);

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

		[System.MonoTODO]
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
				string columnName = encoding.GetString (comm.GetBytes (columnNameLength, false), 0, columnNameLength);
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
					tableName = encoding.GetString (comm.GetBytes (tableNameLength, false), 0, tableNameLength);
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

		[System.MonoTODO]
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
					blockSize = encoding.GetString (comm.GetBytes (cLen, false), 0, cLen);
					comm.Skip (len - 2 - cLen);
				}

				comm.ResizeOutBuf (Int32.Parse (blockSize));
				break;
			case TdsEnvPacketSubType.CharSet :
				string charset;
				cLen = comm.GetByte () & 0xff;
				if (tdsVersion == TdsVersion.tds70) {
					charset = comm.GetString (cLen);
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					charset = encoding.GetString (comm.GetBytes (cLen, false), 0, cLen);
					comm.Skip (len - 2 - cLen);
				}

				SetCharset (charset);
				break;
			case TdsEnvPacketSubType.Database :
				cLen = comm.GetByte () & 0xff;
				string newDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoding.GetString (comm.GetBytes (cLen, false), 0, cLen);
				cLen = comm.GetByte () & 0xff;
				string oldDB = tdsVersion == TdsVersion.tds70 ? comm.GetString (cLen) : encoding.GetString (comm.GetBytes (cLen, false), 0, cLen);
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

		[System.MonoTODO()]
		private TdsPacketMessageResult ProcessMessage (TdsPacketSubType subType)
		{
			return null;
		}

		private TdsPacketOutputParam ProcessOutputParam ()
		{
			GetSubPacketLength ();
			comm.GetString (comm.GetByte () & 0xff);
			comm.Skip (5);
			TdsColumnType colType = (TdsColumnType) comm.GetByte ();

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
				int len = comm.GetTdsShort ();
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
				int len = (comm.GetByte () & 0xff);
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
				int len = comm.GetByte () & 0xff;
				element = (len == 0 ? null : new Guid (comm.GetBytes (len, false)));
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

		[System.MonoTODO]
		private TdsPacketResult ProcessSubPacket ()
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
				moreResults2 = ((TdsPacketEndTokenResult) result).MoreResults;
				break;
			case TdsPacketSubType.ColumnNameToken :
				result = ProcessColumnNames ();
				break;
			case TdsPacketSubType.ColumnInfoToken :
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
				result = LoadRow (new TdsPacketRowResult (null));
				break;
			case TdsPacketSubType.ColumnMetadata :
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

		[System.MonoTODO ("complete")]
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

		private void SetCharset (string charset)
		{
			if (charset == null || charset.Length > 30)
				charset = "iso-8859-1";
			if (this.charset != charset) {
				encoding = Encoding.GetEncoding (charset);
				this.charset = charset;
			}
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
				byte[] tmp = encoding.GetBytes (host);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// username (offset 31 0x1f)
				tmp = encoding.GetBytes (user);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// password (offset 62 0x3e)
				tmp = encoding.GetBytes (password);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// hostproc (offset 93 0x5d)
				tmp = encoding.GetBytes ("00000116");
				comm.AppendBytes (tmp, 8, pad);

				// unused (offset 109 0x6d)
				comm.AppendBytes (empty, (30-14), pad);

				// apptype 
				comm.AppendByte ((byte) 0x0);
				comm.AppendByte ((byte) 0xa0);
				comm.AppendByte ((byte) 0x24);
				comm.AppendByte ((byte) 0xcc);
				comm.AppendByte ((byte) 0x50);
				comm.AppendByte ((byte) 0x12);

				// hostproc length 
				comm.AppendByte ((byte) 8);

				// type of int2
				comm.AppendByte ((byte) 3);

				// type of int4
				comm.AppendByte ((byte) 1);

				// type of char
				comm.AppendByte ((byte) 6);

				// type of flt
				comm.AppendByte ((byte) 10);

				// type of date
				comm.AppendByte ((byte) 9);
				
				// notify of use db
				comm.AppendByte ((byte) 1);

				// disallow dump/load and bulk insert
				comm.AppendByte ((byte) 1);

				// sql interface type
				comm.AppendByte ((byte) 0);

				// type of network connection
				comm.AppendByte ((byte) 0);


				// spare [7]
				comm.AppendBytes (empty, 7, pad);
				// appname
				tmp = encoding.GetBytes (applicationName);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// server name
				tmp = encoding.GetBytes (host);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// remote passwords
				comm.AppendBytes (empty, 2, pad);
				tmp = encoding.GetBytes (password);
				comm.AppendBytes (tmp, 253, pad);
				comm.AppendByte ((byte) (tmp.Length < 253 ? tmp.Length + 2 : 253 + 2));

				// tds version
				comm.AppendByte ((byte) (((byte) tdsVersion) / 10));
				comm.AppendByte ((byte) (((byte) tdsVersion) % 10));
				comm.AppendByte ((byte) 0);
				comm.AppendByte ((byte) 0);

				// prog name
				tmp = encoding.GetBytes (progName);
				comm.AppendBytes (tmp, 10, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// prog version
				comm.AppendByte ((byte) 6);

				// Tell the server we can handle SQLServer version 6
				comm.AppendByte ((byte) 0);

				// Send zero to tell the server we can't handle any other version
				comm.AppendByte ((byte) 0);
				comm.AppendByte ((byte) 0);

				// auto convert short
				comm.AppendByte ((byte) 0);

				// type of flt4
				comm.AppendByte ((byte) 0x0d);

				// type of date4
				comm.AppendByte ((byte) 0x11);

				// language
				tmp = encoding.GetBytes (language);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// notify on lang change
				comm.AppendByte ((byte) 1);

				// security label hierarchy
				comm.AppendShort ((short) 0);

				// security components
				comm.AppendBytes (empty, 8, pad);

				// security spare
				comm.AppendShort ((short) 0);

				// security login role
				comm.AppendByte ((byte) 0);

				// charset
				tmp = encoding.GetBytes (charset);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// notify on charset change
				comm.AppendByte ((byte) 1);

				// length of tds packets
				tmp = encoding.GetBytes (packetSize.ToString ());
				comm.AppendBytes (tmp, 6, pad);
				comm.AppendByte ((byte) 3);

				// pad out to a longword
				comm.AppendBytes (empty, 8, pad);
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

		public void Send70Logon (TdsConnectionParameters parms)
		{
			short packSize = (short) (86 + 2 * (parms.User.Length + parms.Password.Length + parms.ApplicationName.Length + parms.Host.Length + parms.LibraryName.Length + parms.Database.Length));
			byte[] empty = new byte[0];
			byte pad = (byte) 0;

			comm.StartPacket (TdsPacketType.Logon70);
			comm.AppendTdsInt (packSize);

			// TDS Version
			comm.AppendTdsInt (0x70000000);

			comm.AppendBytes (empty, 16, pad);

			// Magic!
			comm.AppendByte ((byte) 0xe0);
			comm.AppendByte ((byte) 0x03);
			comm.AppendBytes (empty, 10, pad);

			// Pack up value lengths, positions
			short curPos = 86;

			// Hostname
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) 0);

			// Username
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) user.Length);
			curPos += (short) (user.Length * 2);

			// Password
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) password.Length);
			curPos += (short) (password.Length * 2);

			// AppName
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) applicationName.Length);
			curPos += (short) (applicationName.Length * 2);

			// Server Name
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) host.Length);
			curPos += (short) (host.Length * 2);

			// Unknown
			comm.AppendTdsShort ((short) 0);
			comm.AppendTdsShort ((short) 0);

			// Library Name
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) libraryName.Length);
			curPos += (short) (libraryName.Length * 2);

			// Unknown
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) 0);

			// Database
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) database.Length);
			curPos += (short) (database.Length * 2);

			// MAC Address
			comm.AppendBytes (empty, 6, pad);
			comm.AppendTdsShort (curPos);

			// Some sort of appended magic
			comm.AppendTdsShort ((short) 0);
			comm.AppendTdsInt (packSize);

			string scrambledPwd = Tds7CryptPass (password);
			comm.AppendChars (user);
			comm.AppendChars (scrambledPwd);
			comm.AppendChars (applicationName);
			comm.AppendChars (host);
			comm.AppendChars (libraryName);
			comm.AppendChars (database);
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

		[System.MonoTODO]
		private string SqlStatementToSetIsolationLevel ()
		{
			string result = "";
			return result;
		}

		private static string Tds7CryptPass (string pass)
		{
			int xormask = 0x5a5a;
			int len = pass.Length;
			StringBuilder sb = new StringBuilder ();
			int i;
			int m1;
			int m2;

			foreach (char c in pass)
			{
				i = (int) (c ^ xormask);
				m1 = (i >> 4) & 0x0f0f;
				m2 = (i << 4) & 0xf0f0;
				sb.Append ((char) (m1 | m2));
			}
			return sb.ToString ();
		}

		#endregion // Methods
	}
}
