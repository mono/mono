//
// Mono.Data.Tds.Protocol.Tds70.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

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
			if (Parameters != null && Parameters.Count > 0)
				return BuildProcedureCall (String.Format ("sp_executesql N'{0}', N'{1}', ", sql, BuildPreparedParameters ()));
			else
				return BuildProcedureCall (String.Format ("sp_executesql N'{0}'", sql));
		}

		private string BuildParameters ()
		{
			StringBuilder result = new StringBuilder ();
			foreach (TdsMetaParameter p in Parameters) {
				if (result.Length > 0)
					result.Append (", ");
				result.Append (FormatParameter (p));
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
			StringBuilder declare = new StringBuilder ();
			StringBuilder select = new StringBuilder ();
			StringBuilder set = new StringBuilder ();
			int count = 0;
			if (Parameters != null) {
				foreach (TdsMetaParameter p in Parameters) {
					if (p.Direction == TdsParameterDirection.Output) {
						declare.Append (String.Format ("declare {0}\n", p.Prepare ()));
						if (count == 0)
							select.Append ("select ");
						else
							select.Append (", ");
							
						set.Append (String.Format ("set {0}=NULL\n", p.ParameterName));
						select.Append (p.ParameterName);
						count += 1;
					}
				}
			}
			string exec = String.Empty;
			if (count > 0)
				exec = "exec ";

			return String.Format ("{0}{1}{2}{3} {4}\n{5}", declare.ToString (), set.ToString (), exec, procedure, BuildParameters (), select.ToString ());	
		}

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");
	
			SetLanguage (connectionParameters.Language);
			SetCharset ("utf-8");

			byte[] empty = new byte[0];
			byte pad = (byte) 0;

			byte[] magic1 = {0x06, 0x83, 0xf2, 0xf8, 0xff, 0x00, 0x00, 0x00, 0x00, 0xe0, 0x03, 0x00, 0x00, 0x88, 0xff, 0xff, 0xff, 0x36, 0x04, 0x00, 0x00};
			byte[] magic2 = {0x00, 0x40, 0x33, 0x9a, 0x6b, 0x50};
			byte[] magic3 = {0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50}; // NTLMSSP
			short partialPacketSize = (short) (86 + 2 * (
					connectionParameters.Hostname.Length + 
					connectionParameters.User.Length + 
					connectionParameters.ApplicationName.Length + 
					connectionParameters.Password.Length + 
					DataSource.Length +
					connectionParameters.LibraryName.Length +
					Language.Length +
					connectionParameters.Database.Length)); 
			short totalPacketSize = (short) (partialPacketSize + 48);
			Comm.StartPacket (TdsPacketType.Logon70);
			Comm.Append (totalPacketSize);
			Comm.Append (empty, 5, pad);

			Comm.Append ((byte) 0x70); // TDS VERSION 7
			Comm.Append (empty, 7, pad);
			Comm.Append (magic1);

			short curPos = 86;

			// Hostname 
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.Hostname.Length);
			curPos += (short) (connectionParameters.Hostname.Length * 2);

			// Username
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.User.Length);
			curPos += (short) (connectionParameters.User.Length * 2);

			// Password
			Comm.Append (curPos);
			Comm.Append ((short) connectionParameters.Password.Length);
			curPos += (short) (connectionParameters.Password.Length * 2);

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

			Comm.Append (magic2);
			Comm.Append (partialPacketSize);
			Comm.Append ((short) 48);
			Comm.Append (totalPacketSize);
			Comm.Append ((short) 0);

			string scrambledPwd = EncryptPassword (connectionParameters.Password);

			Comm.Append (connectionParameters.Hostname);
			Comm.Append (connectionParameters.User);
			Comm.Append (scrambledPwd);
			Comm.Append (connectionParameters.ApplicationName);
			Comm.Append (DataSource);
			Comm.Append (connectionParameters.LibraryName);
			Comm.Append (Language);
			Comm.Append (connectionParameters.Database);
			Comm.Append (magic3);

			Comm.Append ((byte) 0x0);
			Comm.Append ((byte) 0x1);
			Comm.Append (empty, 3, pad);
			Comm.Append ((byte) 0x6);
			Comm.Append ((byte) 0x82);
			Comm.Append (empty, 22, pad);
			Comm.Append ((byte) 0x30);
			Comm.Append (empty, 7, pad);
			Comm.Append ((byte) 0x30);
			Comm.Append (empty, 3, pad);
                        Comm.SendPacket ();

                        TdsPacketResult result;

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
				return String.Format ("{0} output", parameter.ParameterName);

			if (parameter.Value == null)
				return "NULL";

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
				return parameter.Value.ToString ();
			case "nvarchar":
			case "nchar":
				return String.Format ("N'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			case "uniqueidentifier":
				return String.Format ("0x{0}", ((Guid) parameter.Value).ToString ("N"));
			case "bit":
				if (parameter.Value.GetType () == typeof (bool))
					return (((bool) parameter.Value) ? "0x1" : "0x0");
				return parameter.Value.ToString ();
			case "image":
			case "binary":
			case "varbinary":
				return String.Format ("0x{0}", BitConverter.ToString ((byte[]) parameter.Value).Replace ("-", "").ToLower ());
			default:
				return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
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
			if (!NextResult () || !NextRow () || ColumnValues [0] == null)
				throw new TdsInternalException ();
			SkipToEnd ();	
			return ColumnValues [0].ToString ();
		}

		protected override TdsPacketColumnInfoResult ProcessColumnInfo ()
		{
			TdsPacketColumnInfoResult result = new TdsPacketColumnInfoResult ();
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

				int index = result.Add (new TdsSchemaInfo ());
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
