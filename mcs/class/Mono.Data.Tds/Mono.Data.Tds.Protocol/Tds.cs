//
// Mono.Data.Tds.Protocol.Tds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Sebastien Pouliot (spouliot@motus.com)
//   Daniel Morgan (danielmorgan@verizon.net)
// 	 Veerapuram Varadhan  (vvaradhan@novell.com)
//
// Copyright (C) 2002 Tim Coleman
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Portions (C) 2003,2005 Daniel Morgan
// Portions (C) 2008,2009 Novell Inc. 

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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Globalization;
using System.Text;

namespace Mono.Data.Tds.Protocol
{
	public abstract class Tds
	{
		#region Fields

		TdsComm comm;
		TdsVersion tdsVersion;
		
		protected internal TdsConnectionParameters connectionParms;
		protected readonly byte[] NTLMSSP_ID = new byte[] {0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00};

		int packetSize;
		string dataSource;
		string database;
		string originalDatabase = string.Empty;
		string databaseProductName;
		string databaseProductVersion;
		int databaseMajorVersion;
		CultureInfo locale = CultureInfo.InvariantCulture;

		string charset;
		string language;

		bool connected;
		bool moreResults;

		Encoding encoder;
//		bool autoCommit;

		bool doneProc;
		bool pooling = true;
		TdsDataRow currentRow;
		TdsDataColumnCollection columns;

		ArrayList tableNames;
		ArrayList columnNames;

		TdsMetaParameterCollection parameters = new TdsMetaParameterCollection ();

		bool queryInProgress;
		int cancelsRequested;
		int cancelsProcessed;

//		bool isDone;
//		bool isDoneInProc;

		ArrayList outputParameters = new ArrayList ();
		protected TdsInternalErrorCollection messages = new TdsInternalErrorCollection ();

		int recordsAffected = -1;

		long StreamLength;
		long StreamIndex;
		int StreamColumnIndex;

		bool sequentialAccess;
		bool isRowRead;
		bool isResultRead;
		bool LoadInProgress;
		byte [] collation;

		internal int poolStatus = 0;

		#endregion // Fields

		#region Properties

		protected string Charset {
			get { return charset; }
		}

		protected CultureInfo Locale {
			get { return locale; }
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
			get { return connected && comm != null && comm.IsConnected (); }
			set { connected = value; }
		}

		public bool Pooling {
			get { return pooling; }
			set { pooling = value; }
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

		public bool SequentialAccess {
			get { return sequentialAccess; }
			set { sequentialAccess = value; }
		}

		public byte[] Collation {
			get {return collation; }
		}

		public TdsVersion ServerTdsVersion {
			get { 
				switch (databaseMajorVersion) {
				case 4: return TdsVersion.tds42;
				case 5: return TdsVersion.tds50;
				case 7: return TdsVersion.tds70;
				case 8: return TdsVersion.tds80;
				case 9: return TdsVersion.tds90;
				case 10: return TdsVersion.tds100;
				default: return tdsVersion; // return client's version
				}
			}
		}

		private void SkipRow ()
		{
			SkipToColumnIndex (Columns.Count);

			StreamLength = 0;
			StreamColumnIndex = 0;
			StreamIndex = 0;
			LoadInProgress = false;
		}

		private void SkipToColumnIndex (int colIndex)
		{
			if (LoadInProgress)
				EndLoad ();

			if (colIndex < StreamColumnIndex)
				throw new Exception ("Cannot Skip to a colindex less than the curr index");

			while (colIndex != StreamColumnIndex) {
#if NET_2_0
				TdsColumnType? colType = Columns[StreamColumnIndex].ColumnType;
				if (colType == null)
					throw new Exception ("Column type unset.");
#else
				TdsColumnType colType = (TdsColumnType) Columns [StreamColumnIndex]["ColumnType"];
#endif
				if (!(colType == TdsColumnType.Image ||
					colType == TdsColumnType.Text ||
					colType == TdsColumnType.NText)) {
					GetColumnValue (colType, false, StreamColumnIndex);
					StreamColumnIndex ++;
				}
				else {
					BeginLoad (colType);
					Comm.Skip (StreamLength);
					StreamLength = 0;
					EndLoad ();
				}
			}
		}

		public object GetSequentialColumnValue (int colIndex)
		{
			if (colIndex < StreamColumnIndex)
				throw new InvalidOperationException ("Invalid attempt tp read from column ordinal" + colIndex); 

			if (LoadInProgress)
				EndLoad ();

			if (colIndex != StreamColumnIndex)
				SkipToColumnIndex (colIndex);

#if NET_2_0
			object o = GetColumnValue (Columns[colIndex].ColumnType, false, colIndex);
#else
			object o = GetColumnValue ((TdsColumnType)Columns[colIndex]["ColumnType"], false, colIndex);
#endif
			StreamColumnIndex++;
			return o;
		}

		public long GetSequentialColumnValue (int colIndex, long fieldIndex, byte[] buffer, int bufferIndex, int size) 
		{
			if (colIndex < StreamColumnIndex)
				throw new InvalidOperationException ("Invalid attempt to read from column ordinal" + colIndex);
			try {
				if (colIndex != StreamColumnIndex)
					SkipToColumnIndex (colIndex);

				if (!LoadInProgress) {
#if NET_2_0
					BeginLoad (Columns[colIndex].ColumnType);
#else
					BeginLoad ((TdsColumnType)Columns[colIndex]["ColumnType"]);
#endif
				}

				if (buffer == null)
					return StreamLength;
				return LoadData (fieldIndex, buffer, bufferIndex, size);
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
		}

		private void BeginLoad (
#if NET_2_0
			TdsColumnType? colType
#else
			TdsColumnType colType
#endif
		) 
		{
			if (LoadInProgress)
				EndLoad ();

			StreamLength = 0;

#if NET_2_0
			if (colType == null)
				throw new ArgumentNullException ("colType");
#endif

			switch (colType) {
			case TdsColumnType.Text :
			case TdsColumnType.NText:
			case TdsColumnType.Image:
				if (Comm.GetByte () != 0) {
					Comm.Skip (24);
					StreamLength = Comm.GetTdsInt ();
				} else {
					// use -2 to indicate that we're dealing
					// with a NULL value
					StreamLength = -2;
				}
				break;
			case TdsColumnType.BigVarChar:
			case TdsColumnType.BigChar:
			case TdsColumnType.BigBinary:
			case TdsColumnType.BigVarBinary:
				Comm.GetTdsShort ();
				StreamLength = Comm.GetTdsShort ();
				break;
			case TdsColumnType.VarChar :
			case TdsColumnType.NVarChar :
			case TdsColumnType.Char:
			case TdsColumnType.NChar:
			case TdsColumnType.Binary:
			case TdsColumnType.VarBinary:
				StreamLength = Comm.GetTdsShort ();
				break;
			default :
				StreamLength = -1;
				break;
			}

			StreamIndex = 0;
			LoadInProgress = true;
		}

		private void EndLoad()
		{
			if (StreamLength > 0)
				Comm.Skip (StreamLength);
			StreamLength = 0;
			StreamIndex = 0;
			StreamColumnIndex++;
			LoadInProgress = false;
		}

		private long LoadData (long fieldIndex, byte[] buffer, int bufferIndex, int size)
		{
			if (StreamLength <= 0)
				return StreamLength;

			if (fieldIndex < StreamIndex)
				throw new InvalidOperationException (string.Format (
					"Attempting to read at dataIndex '{0}' is " +
					"not allowed as this is less than the " +
					"current position. You must read from " +
					"dataIndex '{1}' or greater.",
					fieldIndex, StreamIndex));

			if (fieldIndex >= (StreamLength + StreamIndex))
				return 0;

			// determine number of bytes to skip
			int skip = (int) (fieldIndex - StreamIndex);
			// skip bytes
			Comm.Skip (skip);
			// update the current position
			StreamIndex += (fieldIndex - StreamIndex);
			// update the remaining length
			StreamLength -= skip;

			// Load the reqd amt of bytes
			int loadlen = (int) ((size > StreamLength) ? StreamLength : size);
			byte[] arr = Comm.GetBytes (loadlen, true);

			// update the index and stream length
			StreamIndex +=  loadlen + (fieldIndex - StreamIndex);
			StreamLength -= loadlen;
			arr.CopyTo (buffer, bufferIndex);

			return arr.Length;
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
			this.columns = new TdsDataColumnCollection ();

			comm = new TdsComm (dataSource, port, packetSize, timeout, tdsVersion);
		}

		#endregion // Constructors

		#region Public Methods

		internal protected void InitExec () 
		{
			// clean up
			moreResults = true;
			doneProc = false;

			// Reset "read" status variables  - used in case of SequentialAccess
			isResultRead = false;
			isRowRead = false;
			StreamLength = 0;
			StreamIndex = 0;
			StreamColumnIndex = 0;
			LoadInProgress = false;
			
			// Reset more variables
			queryInProgress = false;
			cancelsRequested = 0;
			cancelsProcessed = 0;
			recordsAffected = -1;
			
			messages.Clear ();
			outputParameters.Clear ();
		}

		public void Cancel ()
		{
			if (queryInProgress) {
				if (cancelsRequested == cancelsProcessed) {
					comm.StartPacket (TdsPacketType.Cancel);
					try {
						Comm.SendPacket ();
					} catch (IOException ex) {
						connected = false;
						throw new TdsInternalException ("Server closed the connection.", ex);
					}
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
			try {
				comm.StartPacket (TdsPacketType.Logoff);
				comm.Append ((byte) 0);
				comm.SendPacket ();
			} catch {
				// We're closing the socket anyway
			}
			connected = false;
			comm.Close ();
		}
		
		public virtual bool Reset ()
		{
			database = originalDatabase;
			return true;
		}

		protected virtual bool IsValidRowCount (byte status, byte op)
		{
			return ((status & (0x10)) != 0) ;
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

		internal void ExecBulkCopyMetaData (int timeout, bool wantResults)
		{
			moreResults = true;
			try {
				Comm.SendPacket ();
				CheckForData (timeout);
				if (!wantResults) 
					SkipToEnd ();
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
		}

		internal void ExecBulkCopy (int timeout, bool wantResults)
		{
			moreResults = true;
			try {
				Comm.SendPacket ();
				CheckForData (timeout);
				if (!wantResults) 
					SkipToEnd ();
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
		}

		protected void ExecuteQuery (string sql, int timeout, bool wantResults)
		{
			InitExec ();

			Comm.StartPacket (TdsPacketType.Query);
			Comm.Append (sql);
			try {
				Comm.SendPacket ();
				CheckForData (timeout);
				if (!wantResults) 
					SkipToEnd ();
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
		}

		protected virtual void ExecRPC (string rpcName, TdsMetaParameterCollection parameters,
						int timeout, bool wantResults)
		{
			Comm.StartPacket (TdsPacketType.DBRPC);

			byte [] rpcNameBytes = Comm.Encoder.GetBytes (rpcName);
			byte rpcNameLength = (byte) rpcNameBytes.Length;
			ushort mask = 0x0000;
			ushort packetLength =  (ushort) (sizeof (byte) + rpcNameLength +
						sizeof (ushort));

			Comm.Append (packetLength);
			Comm.Append (rpcNameLength);
			Comm.Append (rpcNameBytes);
			Comm.Append (mask);
			
			try {
				Comm.SendPacket ();
				CheckForData (timeout);
				if (!wantResults) 
					SkipToEnd ();
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
		}

		public bool NextResult ()
		{
			if (SequentialAccess) {
				if (isResultRead) {
					while (NextRow ()) {}
					isRowRead = false;
					isResultRead = false;
				}
			}
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
			if (SequentialAccess) {
				if (isRowRead) {
					SkipRow ();
					isRowRead = false;
				}
			}

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
			try {
				while (NextResult ()) { /* DO NOTHING */ }
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
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

		private Encoding GetEncodingFromColumnCollation (int lcid, int sortId)
		{
			if (sortId != 0) 
				return TdsCharset.GetEncodingFromSortOrder (sortId);
			else
				return TdsCharset.GetEncodingFromLCID (lcid);
		}
		
		protected object GetColumnValue (
#if NET_2_0
			TdsColumnType? colType,
#else
			TdsColumnType colType,
#endif
			bool outParam)
		{
			return GetColumnValue (colType, outParam, -1);
		}

		private object GetColumnValue (
#if NET_2_0
			TdsColumnType? colType,
#else
			TdsColumnType colType,
#endif
			bool outParam, int ordinal)
		{
			int len;
			object element = null;
			Encoding enc = null;
			int lcid = 0, sortId = 0;

#if NET_2_0
			if (colType == null)
				throw new ArgumentNullException ("colType");
#endif
			if (ordinal > -1 && tdsVersion > TdsVersion.tds70) {
#if NET_2_0
				lcid = (int) columns[ordinal].LCID;
				sortId = (int) columns[ordinal].SortOrder; 
#else
				lcid = (int) columns[ordinal]["LCID"];
				sortId = (int) columns[ordinal]["SortOrder"];
#endif 			
			}
			
			switch (colType) {
			case TdsColumnType.IntN :
				if (outParam)
					comm.Skip (1);
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Int1 :
			case TdsColumnType.Int2 :
			case TdsColumnType.Int4 :
			case TdsColumnType.BigInt :
				element = GetIntValue (colType);
				break;
			case TdsColumnType.Image :
				if (outParam)
					comm.Skip (1);
				element = GetImageValue ();
				break;
			case TdsColumnType.Text :
				enc = GetEncodingFromColumnCollation (lcid, sortId);			
				if (outParam) 
					comm.Skip (1);
				element = GetTextValue (false, enc);
				break;
			case TdsColumnType.NText :
				enc = GetEncodingFromColumnCollation (lcid, sortId);
				if (outParam) 
					comm.Skip (1);
				element = GetTextValue (true, enc);
				break;
			case TdsColumnType.Char :
			case TdsColumnType.VarChar :
				enc = GetEncodingFromColumnCollation (lcid, sortId);			
				if (outParam)
					comm.Skip (1);
				element = GetStringValue (colType, false, outParam, enc);
				break;
			case TdsColumnType.BigVarBinary :
				if (outParam)
					comm.Skip (1);
				len = comm.GetTdsShort ();
				element = comm.GetBytes (len, true);
				break;
				/*
			case TdsColumnType.BigBinary :
				if (outParam)
					comm.Skip (2);
				len = comm.GetTdsShort ();
				element = comm.GetBytes (len, true);
				break;
				*/
			case TdsColumnType.BigBinary :
				if (outParam)
					comm.Skip (2);
				element = GetBinaryValue ();
				break;
			case TdsColumnType.BigChar :
			case TdsColumnType.BigVarChar :
				enc = GetEncodingFromColumnCollation (lcid, sortId);				
				if (outParam)
					comm.Skip (2);
				element = GetStringValue (colType, false, outParam, enc);
				break;
			case TdsColumnType.NChar :
			case TdsColumnType.BigNVarChar :
				enc = GetEncodingFromColumnCollation (lcid, sortId);				
				if (outParam)
					comm.Skip(2);
				element = GetStringValue (colType, true, outParam, enc);
				break;
			case TdsColumnType.NVarChar :
				enc = GetEncodingFromColumnCollation (lcid, sortId);				
				if (outParam) 
					comm.Skip (1);
				element = GetStringValue (colType, true, outParam, enc);
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
#if NET_2_0
					precision = (byte) columns[ordinal].NumericPrecision;
					scale = (byte) columns[ordinal].NumericScale;
#else
					precision = (byte) columns[ordinal]["NumericPrecision"];
					scale = (byte) columns[ordinal]["NumericScale"];
#endif
				}

				element = GetDecimalValue (precision, scale);
				
				// workaround for fact that TDS 7.0 returns
				// bigint as decimal (19,0), and client code
				// expects it to be returned as a long
				if (scale == 0 && precision <= 19 && tdsVersion == TdsVersion.tds70) {
					if (!(element is System.DBNull))
						element = Convert.ToInt64 (element);
				}
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
					/*byte swallowed =*/ comm.GetByte();
					element = DBNull.Value;
					break;
				}
				if (outParam)
					comm.Skip (1);
				
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

			if (tdsVersion >= TdsVersion.tds70) {
				len = comm.GetTdsShort ();
				if (len != 0xffff && len >= 0)
					result = comm.GetBytes (len, true);
			} else {
				len = (comm.GetByte () & 0xff);
				if (len != 0)
					result = comm.GetBytes (len, true);
			}

			return result;
		}

		private object GetDateTimeValue (
#if NET_2_0
			TdsColumnType? type
#else
			TdsColumnType type
#endif
		)
		{
			int len = 0;
			object result;

#if NET_2_0
			if (type == null)
				throw new ArgumentNullException ("type");
#endif
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
				long millis = (long) System.Math.Round (((((long) seconds) % 300L) * 1000L) / 300f);
				if (seconds != 0 || millis != 0) {
					result = ((DateTime) result).AddSeconds (seconds / 300);
					result = ((DateTime) result).AddMilliseconds (millis);
				}
				break;
			case 4 :
				// MSDN says small datetime is stored in 2 bytes as no of days
				// *after* 1/1/1900. so, cast to unsigned short
				result = epoch.AddDays ((ushort) comm.GetTdsShort ());
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
			if (tdsVersion < TdsVersion.tds70)
				return GetDecimalValueTds50 (precision, scale);
			else
				return GetDecimalValueTds70 (precision, scale);
		}
		
		private object GetDecimalValueTds70 (byte precision, byte scale)
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

		private object GetDecimalValueTds50 (byte precision, byte scale)
		{
			int[] bits = new int[4] {0,0,0,0};

			int len = (comm.GetByte() & 0xff);
			if (len == 0)
				return DBNull.Value;

			byte[] dec_bytes=comm.GetBytes(len,false);
		
			byte[] easy=new byte[4];

			bool positive = dec_bytes[0]==1;

			if (len > 17)
				throw new OverflowException ();

			for (int i = 1, index = 0; i < len && i < 16; i += 
				4, index += 1) {
				for(int j=0; j<4; j++)
					if(i+j<len)
						easy[j]=dec_bytes[len-
							(i+j)];
					else
						easy[j]=0;
				if(!BitConverter.IsLittleEndian)
					easy=comm.Swap(easy);
				bits[index] = BitConverter.ToInt32(easy,0);
			}
			if (bits [3] != 0) 
				return new TdsBigDecimal (precision, 
					scale, positive, bits);
			else
				return new Decimal(bits[0], bits[1], bits
					[2], positive, scale);
			
		}

		private object GetFloatValue (
#if NET_2_0
			TdsColumnType? columnType
#else
			TdsColumnType columnType
#endif
		)
		{
#if NET_2_0
			if (columnType == null)
				throw new ArgumentNullException ("columnType");
#endif
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

		private object GetIntValue (
#if NET_2_0
			TdsColumnType? type
#else
			TdsColumnType type
#endif
		)
		{
			int len;

#if NET_2_0
			if (type == null)
				throw new ArgumentNullException ("type");
#endif
			switch (type) {
			case TdsColumnType.BigInt :
				len = 8;
				break;
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
			case 8:
				return (comm.GetTdsInt64 ());
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

		private object GetMoneyValue (
#if NET_2_0
			TdsColumnType? type
#else
			TdsColumnType type
#endif
		)
		{
			int len;

#if NET_2_0
			if (type == null)
				throw new ArgumentNullException ("type");
#endif
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

			switch (len) {
			case 4: {
				int val = Comm.GetTdsInt ();
				bool negative = val < 0;
				if (negative)
					val = ~(val - 1);
				return new Decimal (val, 0, 0, negative, 4);
			}
			case 8:
				int hi = Comm.GetTdsInt ();
				int lo = Comm.GetTdsInt ();
				bool negative = hi < 0;

				if (negative) {
					hi = ~hi;
					lo = ~(lo - 1);
				}
				return new Decimal (lo, hi, 0, negative, 4);
			default:
				return DBNull.Value;
			}
		}

		protected object GetStringValue (
#if NET_2_0
			TdsColumnType? colType,
#else
			TdsColumnType colType,
#endif
		    bool wideChars, bool outputParam, Encoding encoder)
		{
			bool shortLen = false;
			Encoding enc = encoder;
		
			if (tdsVersion > TdsVersion.tds70 && outputParam && 
			    (colType == TdsColumnType.BigChar || colType == TdsColumnType.BigNVarChar || 
			     colType == TdsColumnType.BigVarChar || colType == TdsColumnType.NChar ||
				 colType == TdsColumnType.NVarChar)) {
				// Read collation for SqlServer 2000 and beyond
				byte[] collation;
				collation = Comm.GetBytes (5, true);
				enc = TdsCharset.GetEncoding (collation);
				shortLen = true;
			} else {
				shortLen = (tdsVersion >= TdsVersion.tds70) && (wideChars || !outputParam);
			}
			
			int len = shortLen ? comm.GetTdsShort () : (comm.GetByte () & 0xff);
			return GetStringValue (wideChars, len, enc);
		}

		protected object GetStringValue (bool wideChars, int len, Encoding enc)
		{
			if (tdsVersion < TdsVersion.tds70 && len == 0)
				return DBNull.Value;
			
			else if (len >= 0) {
				object result;
				if (wideChars)
					result = comm.GetString (len / 2, enc);
				else
					result = comm.GetString (len, false, enc);
				if (tdsVersion < TdsVersion.tds70 && ((string) result).Equals (" "))
					result = string.Empty;
				return result;
			}
			else
				return DBNull.Value;
		}

		protected int GetSubPacketLength ()
		{
			return comm.GetTdsShort ();
		}

		private object GetTextValue (bool wideChars, Encoding encoder)
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
				return string.Empty;

			if (wideChars)
				result = comm.GetString (len / 2, encoder);
			else
				result = comm.GetString (len, false, encoder);
				len /= 2;

			if ((byte) tdsVersion < (byte) TdsVersion.tds70 && result == " ")
				result = string.Empty;

			return result;			
		}
		
		internal bool IsBlobType (TdsColumnType columnType)
		{
			return (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image || columnType == TdsColumnType.NText);
		}

		internal bool IsLargeType (TdsColumnType columnType)
		{
			return ((byte) columnType > 128);
		}

		protected bool IsWideType (TdsColumnType columnType)
		{
			switch (columnType) {
			case TdsColumnType.NChar:
			case TdsColumnType.NText:
			case TdsColumnType.NVarChar:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsFixedSizeColumn (TdsColumnType columnType)
		{
			switch (columnType) {
				case TdsColumnType.Int1 :
				case TdsColumnType.Int2 :
				case TdsColumnType.Int4 :
				case TdsColumnType.BigInt :
				case TdsColumnType.Float8 :
				case TdsColumnType.DateTime :
				case TdsColumnType.Bit :
				case TdsColumnType.Money :
				case TdsColumnType.Money4 :
				case TdsColumnType.SmallMoney :
				case TdsColumnType.Real :
				case TdsColumnType.DateTime4 :
				  /*
				case TdsColumnType.Decimal:
				case TdsColumnType.Numeric:
				  */
					return true;
				default :
					return false;
			}
		}

		protected void LoadRow ()
		{
			if (SequentialAccess) {
				if (isRowRead)
					SkipRow ();
				isRowRead = true;
				isResultRead = true;
				return;
			}

			currentRow = new TdsDataRow ();

			int i = 0;
			foreach (TdsDataColumn column in columns) {
#if NET_2_0
				object o = GetColumnValue (column.ColumnType, false, i);
#else
				object o = GetColumnValue ((TdsColumnType)column["ColumnType"], false, i);
#endif
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
				case TdsColumnType.BigInt :
					return 8;
				default :
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
			try {
				Comm.SendPacket ();
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}
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

				bool isAlias = ((values[2] & (byte) TdsColumnStatus.Rename) != 0);
				if (isAlias) {
					if (tdsVersion >= TdsVersion.tds70) {
						columnNameLength = comm.GetByte ();
						position += 2 * columnNameLength + 1;
					}
					else {
						columnNameLength = comm.GetByte ();
						position += columnNameLength + 1;
					}
					baseColumnName = comm.GetString (columnNameLength);
				}

				byte index = (byte) (values[0] - (byte) 1);
				byte tableIndex = (byte) (values[1] - (byte) 1);
				bool isExpression = ((values[2] & (byte) TdsColumnStatus.IsExpression) != 0);

				TdsDataColumn column = columns [index];
#if NET_2_0
				column.IsHidden = ((values[2] & (byte) TdsColumnStatus.Hidden) != 0);
				column.IsExpression = isExpression;
				column.IsKey = ((values[2] & (byte) TdsColumnStatus.IsKey) != 0);
				column.IsAliased = isAlias;
				column.BaseColumnName = ((isAlias) ? baseColumnName : null);
				column.BaseTableName = ((!isExpression) ? (string) tableNames [tableIndex] : null);
#else
				column ["IsHidden"] = ((values [2] & (byte) TdsColumnStatus.Hidden) != 0);
				column ["IsExpression"] = isExpression;
				column ["IsKey"] = ((values [2] & (byte) TdsColumnStatus.IsKey) != 0);
				column ["IsAliased"] = isAlias;
				column ["BaseColumnName"] = ((isAlias) ? baseColumnName : null);
				column ["BaseTableName"] = ((!isExpression) ? tableNames [tableIndex] : null);
#endif
			}
		}

		protected abstract void ProcessColumnInfo ();

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
			bool validRowCount = IsValidRowCount (status,op);
			moreResults = ((status & 0x01) != 0);
			bool cancelled = ((status & 0x20) != 0);

			switch (type) {
			case TdsPacketSubType.DoneProc:
				doneProc = true;
				goto case TdsPacketSubType.Done;
			case TdsPacketSubType.Done:
			case TdsPacketSubType.DoneInProc:
				if (validRowCount) {
					if (recordsAffected == -1) 
						recordsAffected = rowCount;
					else
						recordsAffected += rowCount;
				}
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
			// VARADHAN: TDS 8 Debugging
			//Console.WriteLine ("In ProcessEnvironmentChange... entry");
			int len = GetSubPacketLength ();
			TdsEnvPacketSubType type = (TdsEnvPacketSubType) comm.GetByte ();
			int cLen;

			switch (type) {
			case TdsEnvPacketSubType.BlockSize :
				string blockSize;
				cLen = comm.GetByte ();
				blockSize = comm.GetString (cLen);

				if (tdsVersion >= TdsVersion.tds70) 
					comm.Skip (len - 2 - cLen * 2);
				else 
					comm.Skip (len - 2 - cLen);

				packetSize = Int32.Parse (blockSize);	
				comm.ResizeOutBuf (packetSize);
				break;
			case TdsEnvPacketSubType.CharSet :
				cLen = comm.GetByte ();
				if (tdsVersion == TdsVersion.tds70) {
					SetCharset (comm.GetString (cLen));
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					SetCharset (comm.GetString (cLen));
					comm.Skip (len - 2 - cLen);
				}

				break;
			case TdsEnvPacketSubType.Locale :
				cLen = comm.GetByte ();
				int lcid = 0;
				if (tdsVersion >= TdsVersion.tds70) {
					lcid = (int) Convert.ChangeType (comm.GetString (cLen), typeof (int));
					comm.Skip (len - 2 - cLen * 2);
				}
				else {
					lcid = (int) Convert.ChangeType (comm.GetString (cLen), typeof (int));
					comm.Skip (len - 2 - cLen);
				}
				locale = new CultureInfo (lcid);
				break;
			case TdsEnvPacketSubType.Database :
				cLen = comm.GetByte ();
				string newDB = comm.GetString (cLen);
				cLen = comm.GetByte () & 0xff;
				comm.GetString (cLen);
				if (originalDatabase == string.Empty)
					originalDatabase = newDB;
				database = newDB;
				break;
			case TdsEnvPacketSubType.CollationInfo:
				//Console.WriteLine ("ProcessEnvironmentChange::Got collation info");
				cLen = comm.GetByte ();
				collation = comm.GetBytes (cLen, true);
				lcid = TdsCollation.LCID (collation);
				locale = new CultureInfo (lcid);
				SetCharset (TdsCharset.GetEncoding (collation));
				break;
				
			default:
				comm.Skip (len - 1);
				break;
			}
			// VARADHAN: TDS 8 Debugging
			//Console.WriteLine ("In ProcessEnvironmentChange... exit");
		}

		protected void ProcessLoginAck ()
		{
			uint srvVersion = 0;
			GetSubPacketLength ();
			
			//Console.WriteLine ("ProcessLoginAck: B4 tdsVersion:{0}", tdsVersion);
			// Valid only for a Login7 request
			if (tdsVersion >= TdsVersion.tds70) {
				comm.Skip (1);
				srvVersion = (uint)comm.GetTdsInt ();

				//Console.WriteLine ("srvVersion: {0}", srvVersion);
				switch (srvVersion) {
				case 0x00000007: 
					tdsVersion = TdsVersion.tds70;
					break;
				case 0x00000107:
					tdsVersion = TdsVersion.tds80;
					break;
				case 0x01000071:
					tdsVersion = TdsVersion.tds81;
					break;
				case 0x02000972:
					tdsVersion = TdsVersion.tds90;
					break;
				}
				//Console.WriteLine ("ProcessLoginAck: after tdsVersion:{0}", tdsVersion);				
			}
			
			if (tdsVersion >= TdsVersion.tds70) {
				int nameLength = comm.GetByte ();
				databaseProductName = comm.GetString (nameLength);
				databaseMajorVersion = comm.GetByte ();
				databaseProductVersion = String.Format ("{0}.{1}.{2}", databaseMajorVersion.ToString("00"),
								comm.GetByte ().ToString("00"), 
								(256 * comm.GetByte () + comm.GetByte ()).ToString("0000"));
			} else {
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
			//Console.WriteLine ("databaseProductVersion:{0}", databaseProductVersion);
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

		protected virtual void ProcessOutputParam ()
		{
			GetSubPacketLength ();
			/*string paramName = */comm.GetString (comm.GetByte () & 0xff);
			comm.Skip (5);

			TdsColumnType colType = (TdsColumnType) comm.GetByte ();
			object value = GetColumnValue (colType, true);
			outputParameters.Add (value);
		}

		protected void ProcessDynamic ()
		{
			Comm.Skip (2);
			/*byte type =*/ Comm.GetByte ();
			/*byte status =*/ Comm.GetByte ();
			/*string id =*/ Comm.GetString (Comm.GetByte ());
		}

		protected virtual TdsPacketSubType ProcessSubPacket ()
		{
			// VARADHAN: TDS 8 Debugging
			// Console.WriteLine ("In ProcessSubPacket... entry");
			
			TdsPacketSubType subType = (TdsPacketSubType) comm.GetByte ();

			// VARADHAN: TDS 8 Debugging
			//Console.WriteLine ("Subpacket type: {0}", subType);
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
				ProcessReturnStatus ();
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
				Columns.Clear ();
				ProcessColumnInfo ();
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

			// VARADHAN: TDS 8 Debugging
			//Console.WriteLine ("In ProcessSubPacket... exit");
			return subType;
		}

		protected void ProcessTableName ()
		{
			tableNames = new ArrayList ();
			int totalLength = comm.GetTdsShort ();
			int position = 0;
			int len;

			while (position < totalLength) {
				if (tdsVersion >= TdsVersion.tds70) {
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

		protected void SetCharset (Encoding encoder)
		{
			comm.Encoder = encoder;
		}
		
		protected void SetCharset (string charset)
		{
			if (charset == null || charset.Length > 30)
				charset = "iso_1";

			if (this.charset != null && this.charset == charset)
				return;

			if (charset.StartsWith ("cp")) {
				encoder = Encoding.GetEncoding (Int32.Parse (charset.Substring (2)));
				this.charset = charset;
			}
			else {
				encoder = Encoding.GetEncoding ("iso-8859-1");
				this.charset = "iso_1";
			}
			SetCharset (encoder);
		}

		protected void SetLanguage (string language)
		{
			if (language == null || language.Length > 30)
				language = "us_english";

			this.language = language;
		}

		protected virtual void ProcessReturnStatus () 
		{
			comm.Skip(4);
		}

		#endregion // Private Methods

#if NET_2_0
                #region asynchronous methods
                protected IAsyncResult BeginExecuteQueryInternal (string sql, bool wantResults, 
                                                          AsyncCallback callback, object state)
                {
			InitExec ();

                        TdsAsyncResult ar = new TdsAsyncResult (callback, state);
                        ar.TdsAsyncState.WantResults = wantResults;

			Comm.StartPacket (TdsPacketType.Query);
			Comm.Append (sql);
			try {
				Comm.SendPacket ();
				Comm.BeginReadPacket (new AsyncCallback(OnBeginExecuteQueryCallback), 
						      ar);
			} catch (IOException ex) {
				connected = false;
				throw new TdsInternalException ("Server closed the connection.", ex);
			}

                        return ar;
                }
                
                protected void EndExecuteQueryInternal (IAsyncResult ar)
                {
                        if (!ar.IsCompleted)
                                ar.AsyncWaitHandle.WaitOne ();
                        TdsAsyncResult result = (TdsAsyncResult) ar;
                        if (result.IsCompletedWithException)
                                throw result.Exception;
                }

                protected void OnBeginExecuteQueryCallback (IAsyncResult ar)
                {
                        TdsAsyncResult result = (TdsAsyncResult) ar.AsyncState;
                        TdsAsyncState tdsState = (TdsAsyncState) result.TdsAsyncState;

                        try {
                                Comm.EndReadPacket (ar);
                                if (!tdsState.WantResults)
                                        SkipToEnd ();
                        } catch (Exception e) {
                                result.MarkComplete (e);
                                return;
                        }
                        result.MarkComplete ();
                }
                

                public virtual IAsyncResult BeginExecuteNonQuery (string sql,
                                                                  TdsMetaParameterCollection parameters,
                                                                  AsyncCallback callback,
                                                                  object state)
                {
                        // abstract, kept to be backward compatiable.
                        throw new NotImplementedException ("should not be called!");
                }
                
                public virtual void EndExecuteNonQuery (IAsyncResult ar)
                {
                        // abstract method
                        throw new NotImplementedException ("should not be called!");
                }
                
                public virtual IAsyncResult BeginExecuteQuery (string sql,
                                                                  TdsMetaParameterCollection parameters,
                                                                  AsyncCallback callback,
                                                                  object state)
                {
                        // abstract, kept to be backward compatiable.
                        throw new NotImplementedException ("should not be called!");
                }
                
                public virtual void EndExecuteQuery (IAsyncResult ar)
                {
                        // abstract method
                        throw new NotImplementedException ("should not be called!");
                }

                public virtual IAsyncResult BeginExecuteProcedure (string prolog,
                                                                    string epilog,
                                                                    string cmdText,
                                                                    bool IsNonQuery,
                                                                    TdsMetaParameterCollection parameters,
                                                                    AsyncCallback callback,
                                                                    object state)
                {
                        throw new NotImplementedException ("should not be called!");
                }
                
                public virtual void EndExecuteProcedure (IAsyncResult ar)
                {
                        // abstract method
                        throw new NotImplementedException ("should not be called!");
                }
                
                public void WaitFor (IAsyncResult ar)
                {
                        if (! ar.IsCompleted)
                                ar.AsyncWaitHandle.WaitOne ();
                }

                public void CheckAndThrowException (IAsyncResult ar)
                {
                        TdsAsyncResult result = (TdsAsyncResult) ar;
                        if (result.IsCompleted && result.IsCompletedWithException)
                                throw result.Exception;
                }

                #endregion // asynchronous methods
#endif // NET_2_0


	}
}
