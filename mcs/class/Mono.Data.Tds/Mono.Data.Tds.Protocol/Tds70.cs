//
// Mono.Data.Tds.Protocol.Tds70.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//   Sebastien Pouliot (sebastien@ximian.com)
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
using System.Text;

namespace Mono.Data.Tds.Protocol {
        public class Tds70 : Tds
	{
		#region Fields

		public readonly static TdsVersion Version = TdsVersion.tds70;

		#endregion // Fields

		#region Constructors

		public Tds70 (string server, int port)
			: this (server, port, 512, 15)
		{
		}

		public Tds70 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
		}

		#endregion // Constructors

		#region Methods

		private string BuildExec (string sql)
		{
			string esql = sql.Replace ("'", "''"); // escape single quote
			if (Parameters != null && Parameters.Count > 0)
				return BuildProcedureCall (String.Format ("sp_executesql N'{0}', N'{1}', ", esql, BuildPreparedParameters ()));
			else
				return BuildProcedureCall (String.Format ("sp_executesql N'{0}'", esql));
		}

		private string BuildParameters ()
		{
			if (Parameters == null || Parameters.Count == 0)
				return String.Empty;

			StringBuilder result = new StringBuilder ();
			foreach (TdsMetaParameter p in Parameters) {
				if (p.Direction != TdsParameterDirection.ReturnValue) {
				if (result.Length > 0)
					result.Append (", ");
					if (p.Direction == TdsParameterDirection.InputOutput)
						result.Append (String.Format("{0}={0} output", p.ParameterName));
					else
				result.Append (FormatParameter (p));
			}
			}
			return result.ToString ();
		}

		private string BuildPreparedParameters ()
		{
			StringBuilder parms = new StringBuilder ();
			foreach (TdsMetaParameter p in Parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (p.Prepare ());
				if (p.Direction == TdsParameterDirection.Output)
					parms.Append (" output");
			}
			return parms.ToString ();
		}

		private string BuildPreparedQuery (string id)
		{
			return BuildProcedureCall (String.Format ("sp_execute {0},", id));
		}

		private string BuildProcedureCall (string procedure)
		{
			string exec = String.Empty;

			StringBuilder declare = new StringBuilder ();
			StringBuilder select = new StringBuilder ();
			StringBuilder set = new StringBuilder ();
			
			int count = 0;
			if (Parameters != null) {
				foreach (TdsMetaParameter p in Parameters) {
					if (p.Direction != TdsParameterDirection.Input) {

						if (count == 0)
							select.Append ("select ");
						else
							select.Append (", ");
						select.Append (p.ParameterName);
							
						declare.Append (String.Format ("declare {0}\n", p.Prepare ()));

						if (p.Direction != TdsParameterDirection.ReturnValue) {
							if( p.Direction == TdsParameterDirection.InputOutput )
								set.Append (String.Format ("set {0}\n", FormatParameter(p)));
							else
						set.Append (String.Format ("set {0}=NULL\n", p.ParameterName));
						}
					
						count += 1;
					}
					
					if (p.Direction == TdsParameterDirection.ReturnValue) {
						exec = p.ParameterName + "=";
					}
				}
			}
			if (count > 0 || exec.Length > 0)
				exec = "exec " + exec;

			return String.Format ("{0}{1}{2}{3} {4}\n{5}", declare.ToString (), set.ToString (), exec, procedure, BuildParameters (), select.ToString ());	
		}

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");
	
			connectionParms = connectionParameters;

			SetLanguage (connectionParameters.Language);
			SetCharset ("utf-8");
		
			byte[] empty = new byte[0];
			short authLen = 0;
			byte pad = (byte) 0;
			
			byte[] domainMagic =  {	6, 0x7d, 0x0f, 0xfd, 0xff, 0x0, 0x0, 0x0,
									0x0, 0xe0, 0x83, 0x0, 0x0,
									0x68, 0x01, 0x00, 0x00, 0x09, 0x04, 0x00, 0x00 };
			byte[] sqlserverMagic = { 6, 0x83, 0xf2, 0xf8,
										0xff, 0x0, 0x0, 0x0,
										0x0, 0xe0, 0x03, 0x0,
										0x0, 0x88, 0xff, 0xff, 0xff, 0x36, 
										0x04, 0x00, 0x00 };
			byte[] magic = null;
			
			if (connectionParameters.DomainLogin == true)
				magic = domainMagic;
			else
				magic = sqlserverMagic;
			
			string username = connectionParameters.User;

			string domain = Environment.UserDomainName;
			domain = connectionParameters.DefaultDomain = Environment.UserDomainName;

			int idx = 0;
			if ((idx = username.IndexOf (@"\")) > -1) {
				domain = username.Substring (0, idx);
				username = username.Substring (idx + 1);

				connectionParameters.DefaultDomain = domain;
				connectionParameters.User = username;
			}
							
			short partialPacketSize = (short) (86 + (
				connectionParameters.Hostname.Length + 		
				connectionParameters.ApplicationName.Length + 
				DataSource.Length +
				connectionParameters.LibraryName.Length +
				Language.Length +
				connectionParameters.Database.Length) * 2); 
			
			if(connectionParameters.DomainLogin == true) {
				authLen = ((short) (32 + (connectionParameters.Hostname.Length +
					domain.Length)));
				partialPacketSize += authLen;
			}
			else 
				partialPacketSize += ((short) ((username.Length + connectionParameters.Password.Length) * 2));
			
			short totalPacketSize = (short) partialPacketSize;
			
			Comm.StartPacket (TdsPacketType.Logon70);
			
			Comm.Append (totalPacketSize);
			Comm.Append (empty, 5, pad);

			Comm.Append ((byte) 0x70); // TDS Version 7
			Comm.Append (empty, 3, pad);
			Comm.Append (empty, 4, pad);
			Comm.Append (magic);

			short curPos = 86;

			// Hostname 
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.Hostname.Length);
			curPos += (short) (connectionParameters.Hostname.Length * 2);

			if(connectionParameters.DomainLogin.Equals(true))
			{
				Comm.Append((short)0);
				Comm.Append((short)0);
				Comm.Append((short)0);
				Comm.Append((short)0);
			}
			else 
			{
				// Username
				Comm.Append (curPos);
				Comm.Append ((short) username.Length);
				curPos += ((short) (username.Length * 2));

				// Password
				Comm.Append (curPos);
				Comm.Append ((short) connectionParameters.Password.Length);
				curPos += (short) (connectionParameters.Password.Length * 2);
			}

			// AppName			
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.ApplicationName.Length);
			curPos += (short) (connectionParameters.ApplicationName.Length * 2);

			// Server Name
			Comm.Append (curPos);
			Comm.Append ((short) DataSource.Length);
			curPos += (short) (DataSource.Length * 2);

			// Unknown
			Comm.Append ((short) 0);
			Comm.Append ((short) 0);

			// Library Name
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.LibraryName.Length);
			curPos += (short) (connectionParameters.LibraryName.Length * 2);

			// Language
			Comm.Append (curPos);
			Comm.Append ((short) Language.Length);
			curPos += (short) (Language.Length * 2);

			// Database
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.Database.Length);
			curPos += (short) (connectionParameters.Database.Length * 2);

			// MAC Address
			Comm.Append((byte) 0);
			Comm.Append((byte) 0);
			Comm.Append((byte) 0);
			Comm.Append((byte) 0);
			Comm.Append((byte) 0);
			Comm.Append((byte) 0);

			// Authentication Stuff
			Comm.Append ((short) curPos);
			if (connectionParameters.DomainLogin == true) 
			{
				Comm.Append ((short) authLen);
				curPos += (short) authLen;
			}
			else
				Comm.Append ((short) 0);
			
			// Unknown
			Comm.Append (curPos);
			Comm.Append ((short) 0);
			
			// Connection Parameters
			Comm.Append (connectionParameters.Hostname);
			if (connectionParameters.DomainLogin == false) 
			{
				// SQL Server Authentication
				Comm.Append (connectionParameters.User);
				string scrambledPwd = EncryptPassword (connectionParameters.Password);
				Comm.Append (scrambledPwd);
			}
			Comm.Append (connectionParameters.ApplicationName);
			Comm.Append (DataSource);
			Comm.Append (connectionParameters.LibraryName);
			Comm.Append (Language);
			Comm.Append (connectionParameters.Database);

			if (connectionParameters.DomainLogin) 
			{
				// the rest of the packet is NTLMSSP authentication
				Type1Message msg = new Type1Message ();
				msg.Domain = domain;
				msg.Host = connectionParameters.Hostname;
				msg.Flags = NtlmFlags.NegotiateUnicode | 
					NtlmFlags.NegotiateNtlm | 
					NtlmFlags.NegotiateDomainSupplied | 
					NtlmFlags.NegotiateWorkstationSupplied | 
					NtlmFlags.NegotiateAlwaysSign; // 0xb201
				Comm.Append (msg.GetBytes ());
			}

			Comm.SendPacket ();
			MoreResults = true;
			SkipToEnd ();
			
			return IsConnected;
		}

		private static string EncryptPassword (string pass)
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

		public override bool Reset ()
		{
			try
			{
				ExecProc ("exec sp_reset_connection");
				return true;
			}
			catch
			{
				Console.WriteLine ("Error reseting");
				return false;
			}
		}

		public override void ExecPrepared (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			ExecuteQuery (BuildPreparedQuery (commandText), timeout, wantResults);
		}
			
		public override void ExecProc (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			ExecuteQuery (BuildProcedureCall (commandText), timeout, wantResults);
		}

		public override void Execute (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			string sql = commandText;
			if (wantResults || (Parameters != null && Parameters.Count > 0))
				sql = BuildExec (commandText);
			ExecuteQuery (sql, timeout, wantResults);
		}

                private bool IsBlobType (TdsColumnType columnType)
		{
			return (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image || columnType == TdsColumnType.NText);
		}

                private bool IsLargeType (TdsColumnType columnType)
		{
			return (columnType == TdsColumnType.NChar || (byte) columnType > 128);
		}

		private string FormatParameter (TdsMetaParameter parameter)
		{
			if (parameter.Direction == TdsParameterDirection.Output)
				return String.Format ("{0}={0} output", parameter.ParameterName);

			if (parameter.Value == null || parameter.Value == DBNull.Value)
				return parameter.ParameterName + "=NULL";

			string value = null;
			switch (parameter.TypeName) {
			case "bigint":
			case "decimal":
			case "float":
			case "int":
			case "money":
			case "real":
			case "smallint":
			case "smallmoney":
			case "tinyint":
				value = parameter.Value.ToString ();
				break;
			case "nvarchar":
			case "nchar":
				value = String.Format ("N'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
				break;
			case "uniqueidentifier":
				value = String.Format ("0x{0}", ((Guid) parameter.Value).ToString ("N"));
				break;
			case "bit":
				if (parameter.Value.GetType () == typeof (bool))
					value = (((bool) parameter.Value) ? "0x1" : "0x0");
				else
					value = parameter.Value.ToString ();

				break;
			case "image":
			case "binary":
			case "varbinary":
				value = String.Format ("0x{0}", BitConverter.ToString ((byte[]) parameter.Value).Replace ("-", "").ToLower ());
				break;
			default:
				value = String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
				break;
			}

			return parameter.ParameterName + "=" + value;
		}

		public override string Prepare (string commandText, TdsMetaParameterCollection parameters)
		{
			Parameters = parameters;

			TdsMetaParameterCollection parms = new TdsMetaParameterCollection ();
			TdsMetaParameter parm = new TdsMetaParameter ("@P1", "int", null);
			parm.Direction = TdsParameterDirection.Output;
			parms.Add (parm);

			parms.Add (new TdsMetaParameter ("@P2", "nvarchar", BuildPreparedParameters ()));
			parms.Add (new TdsMetaParameter ("@P3", "nvarchar", commandText));

			ExecProc ("sp_prepare", parms, 0, true);
			SkipToEnd ();	
			if (ColumnValues [0] == null || ColumnValues [0] == DBNull.Value)
				throw new TdsInternalException ();
			return ColumnValues [0].ToString ();
		}

		protected override TdsDataColumnCollection ProcessColumnInfo ()
		{
			TdsDataColumnCollection result = new TdsDataColumnCollection ();
			int numColumns = Comm.GetTdsShort ();

			for (int i = 0; i < numColumns; i += 1) {
				byte[] flagData = new byte[4];
				for (int j = 0; j < 4; j += 1) 
					flagData[j] = Comm.GetByte ();

				bool nullable = (flagData[2] & 0x01) > 0;
				bool caseSensitive = (flagData[2] & 0x02) > 0;
				bool writable = (flagData[2] & 0x0c) > 0;
				bool autoIncrement = (flagData[2] & 0x10) > 0;
				bool isIdentity = (flagData[2] & 0x10) > 0;

				TdsColumnType columnType = (TdsColumnType) (Comm.GetByte () & 0xff);
				if ((byte) columnType == 0xef)
					columnType = TdsColumnType.NChar;
			
				byte xColumnType = 0;
				if (IsLargeType (columnType)) {
					xColumnType = (byte) columnType;
					if (columnType != TdsColumnType.NChar)
						columnType -= 128;
				}

				int columnSize;
				string tableName = null;

				if (IsBlobType (columnType)) {
					columnSize = Comm.GetTdsInt ();
					tableName = Comm.GetString (Comm.GetTdsShort ());
				}

				else if (IsFixedSizeColumn (columnType))
					columnSize = LookupBufferSize (columnType);
				else if (IsLargeType ((TdsColumnType) xColumnType))
					columnSize = Comm.GetTdsShort ();
				else
					columnSize = Comm.GetByte () & 0xff;

				byte precision = 0;
				byte scale = 0;

				switch (columnType) {
				case TdsColumnType.NText:
				case TdsColumnType.NChar:
				case TdsColumnType.NVarChar:
					columnSize /= 2;
					break;
				case TdsColumnType.Decimal:
				case TdsColumnType.Numeric:
					precision = Comm.GetByte ();
					scale = Comm.GetByte ();
					break;
				}

				string columnName = Comm.GetString (Comm.GetByte ());

				int index = result.Add (new TdsDataColumn ());
				result[index]["AllowDBNull"] = nullable;
				result[index]["ColumnName"] = columnName;
				result[index]["ColumnSize"] = columnSize;
				result[index]["ColumnType"] = columnType;
				result[index]["IsIdentity"] = isIdentity;
				result[index]["IsReadOnly"] = !writable;
				result[index]["NumericPrecision"] = precision;
				result[index]["NumericScale"] = scale;
				result[index]["BaseTableName"] = tableName;
			}

			return result;
		}

		public override void Unprepare (string statementId)
		{
			TdsMetaParameterCollection parms = new TdsMetaParameterCollection ();
			parms.Add (new TdsMetaParameter ("@P1", "int", Int32.Parse (statementId)));
			ExecProc ("sp_unprepare", parms, 0, false);
		}

		#endregion // Methods
	}
}
