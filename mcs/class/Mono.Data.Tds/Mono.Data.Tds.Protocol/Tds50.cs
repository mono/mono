//
// Mono.Data.Tds.Protocol.Tds50.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
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

using Mono.Data.Tds;
using System;
using System.Text;

namespace Mono.Data.Tds.Protocol
{
	[MonoTODO ("FIXME: Can packetsize be anything other than 512?")]
	public sealed class Tds50 : Tds
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds50;
		int packetSize;
		bool isSelectQuery;

		#endregion // Fields

		#region Constructors

		public Tds50 (string server, int port)
			: this (server, port, 512, 15)
		{
		}

		public Tds50 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
			this.packetSize = packetSize;
		}

		#endregion // Constructors
	
		#region Methods

		public string BuildExec (string sql)
		{
			if (Parameters == null || Parameters.Count == 0) 
				return sql;

			StringBuilder select = new StringBuilder ();
			StringBuilder set = new StringBuilder ();
			StringBuilder declare = new StringBuilder ();
			int count = 0;
			foreach (TdsMetaParameter p in Parameters) {
				declare.Append (String.Format ("declare {0}\n", p.Prepare ()));
				set.Append (String.Format ("select {0}=", p.ParameterName));
				if (p.Direction == TdsParameterDirection.Input)
					set.Append (FormatParameter (p));
				else {
					set.Append ("NULL");
					select.Append (p.ParameterName);
					if (count == 0)
						select.Append ("select ");
					else
						select.Append (", ");
					count += 1;
				}
				set.Append ("\n");
			}	
			return String.Format ("{0}{1}{2}\n{3}", declare.ToString (), set.ToString (), sql, select.ToString ());
		}

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");

			byte[] capabilityRequest = {0x03, 0xef, 0x65, 0x41, 0xff, 0xff, 0xff, 0xd6};
			byte[] capabilityResponse = {0x00, 0x00, 0x00, 0x06, 0x48, 0x00, 0x00, 0x08};

			SetCharset (connectionParameters.Charset);
			SetLanguage (connectionParameters.Language);

			byte pad = (byte) 0;
			byte[] empty = new byte[0];

			Comm.StartPacket (TdsPacketType.Logon);

			// hostname (offset 0)
			// 0-30
			byte[] tmp = Comm.Append (connectionParameters.Hostname, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// username (offset 31 0x1f)
			// 31-61
			tmp = Comm.Append (connectionParameters.User, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// password (offset 62 0x3e)
			// 62-92
			tmp = Comm.Append (connectionParameters.Password, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// hostproc (offset 93 0x5d)
			// 93-123
			tmp = Comm.Append ("37876", 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// Byte order of 2 byte ints
			// 2 = <MSB, LSB>, 3 = <LSB, MSB>
			// 124
			Comm.Append ((byte) 3);

			// Byte order of 4 byte ints
			// 0 = <MSB, LSB>, 1 = <LSB, MSB>
			// 125
			Comm.Append ((byte) 1);

			// Character representation
			// (6 = ASCII, 7 = EBCDIC)
			// 126
			Comm.Append ((byte) 6);

			// Eight byte floating point representation
			// 4 = IEEE <MSB, ..., LSB>
			// 5 = VAX 'D'
			// 10 = IEEE <LSB, ..., MSB>
			// 11 = ND5000
			// 127
			Comm.Append ((byte) 10);

			// Eight byte date format
			// 8 = <MSB, ..., LSB>
			// 128
			Comm.Append ((byte) 9);
		
			// notify of use db
			// 129
			Comm.Append ((byte) 1);

			// disallow dump/load and bulk insert
			// 130
			Comm.Append ((byte) 1);

			// sql interface type
			// 131
			Comm.Append ((byte) 0);

			// type of network connection
			// 132
			Comm.Append ((byte) 0);

			// spare [7]
			// 133-139
			Comm.Append (empty, 7, pad);

			// appname
			// 140-170
			tmp = Comm.Append (connectionParameters.ApplicationName, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// server name
			// 171-201
			tmp = Comm.Append (DataSource, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// remote passwords
			// 202-457	
			Comm.Append (empty, 2, pad);
			tmp = Comm.Append (connectionParameters.Password, 253, pad);
			Comm.Append ((byte) (tmp.Length < 253 ? tmp.Length + 2 : 253 + 2));

			// tds version
			// 458-461
			Comm.Append ((byte) 5);
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);

			// prog name
			// 462-472
			tmp = Comm.Append (connectionParameters.ProgName, 10, pad);
			Comm.Append ((byte) (tmp.Length < 10 ? tmp.Length : 10));

			// prog version
			// 473-476
			Comm.Append ((byte) 6);
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);

			// auto convert short
			// 477
			Comm.Append ((byte) 0);

			// type of flt4
			// 478
			Comm.Append ((byte) 0x0d);

			// type of date4
			// 479
			Comm.Append ((byte) 0x11);

			// language
			// 480-510
			tmp = Comm.Append (Language, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// notify on lang change
			// 511
			Comm.Append ((byte) 1);

			// security label hierarchy
			// 512-513
			Comm.Append ((short) 0);

			// security components
			// 514-521
			Comm.Append (empty, 8, pad);

			// security spare
			// 522-523
			Comm.Append ((short) 0);

			// security login role
			// 524
			Comm.Append ((byte) 0);

			// charset
			// 525-555
			tmp = Comm.Append (Charset, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// notify on charset change
			// 556
			Comm.Append ((byte) 1);

			// length of tds packets
			// 557-563
			tmp = Comm.Append (this.packetSize.ToString (), 6, pad);
			Comm.Append ((byte) (tmp.Length < 6 ? tmp.Length : 6));

			Comm.Append (empty, 8, pad);
			// Padding...
			// 564-567
			//Comm.Append (empty, 4, pad);

			// Capabilities
			Comm.Append ((byte) TdsPacketSubType.Capability);
			Comm.Append ((short) 20);
			Comm.Append ((byte) 0x01); // TDS_CAP_REQUEST
			Comm.Append (capabilityRequest);
			Comm.Append ((byte) 0x02);
			Comm.Append (capabilityResponse);

			Comm.SendPacket ();

			MoreResults = true;
			SkipToEnd ();

			return IsConnected;
		}

		public override void ExecPrepared (string id, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			bool hasParameters = (Parameters != null && Parameters.Count > 0);

			Comm.StartPacket (TdsPacketType.Normal);

			Comm.Append ((byte) TdsPacketSubType.Dynamic);
			Comm.Append ((short) (id.Length + 5));
			Comm.Append ((byte) 0x02);                  // TDS_DYN_EXEC
			Comm.Append ((byte) (hasParameters ? 0x01 : 0x00));
			Comm.Append ((byte) id.Length);
			Comm.Append (id);
			Comm.Append ((short) 0);

			if (hasParameters) {
				SendParamFormat ();
				SendParams ();
			}

			MoreResults = true;
			Comm.SendPacket ();
			CheckForData (timeout);
			if (!wantResults)
				SkipToEnd ();
		}

		public override void Execute (string sql, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			string ex = BuildExec (sql);
			ExecuteQuery (ex, timeout, wantResults);
		}

		public override void ExecProc (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults)
		{
			Parameters = parameters;
			ExecuteQuery (BuildProcedureCall (commandText), timeout, wantResults);
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
					
					if (p.Direction == TdsParameterDirection.ReturnValue)
						exec = p.ParameterName + "=";
				}
			}
			exec = "exec " + exec;

			string sql = String.Format ("{0}{1}{2}{3} {4}\n{5}", declare.ToString (),
				set.ToString (),
				exec, procedure,
				BuildParameters (), select.ToString ());
			return sql;
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


		private string FormatParameter (TdsMetaParameter parameter)
		{
			if (parameter.Direction == TdsParameterDirection.Output)
				return String.Format ("{0} output", parameter.ParameterName);
		
			if (parameter.Value == null || parameter.Value == DBNull.Value)
				return "NULL";
		
			switch (parameter.TypeName) {
			case "smalldatetime":
			case "datetime":
				DateTime d = (DateTime)parameter.Value;
				return String.Format(System.Globalization.CultureInfo.InvariantCulture, 
						     "'{0:MMM dd yyyy hh:mm:ss tt}'", d );
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
				return String.Format ("0x{0}", BitConverter.ToString ((byte[]) parameter.Value).Replace ("-", string.Empty).ToLower ());
			default:
				return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
		}

		public override string Prepare (string sql, TdsMetaParameterCollection parameters)
		{
			Parameters = parameters;

			Random rand = new Random ();
			StringBuilder idBuilder = new StringBuilder ();
			for (int i = 0; i < 25; i += 1)
				idBuilder.Append ((char) (rand.Next (26) + 65));
			string id = idBuilder.ToString ();

			//StringBuilder declare = new StringBuilder ();

		
			sql = String.Format ("create proc {0} as\n{1}", id, sql);
			short len = (short) ((id.Length) + sql.Length + 5);

			Comm.StartPacket (TdsPacketType.Normal);
			Comm.Append ((byte) TdsPacketSubType.Dynamic);
			Comm.Append (len);
			Comm.Append ((byte) 0x1); // PREPARE
			Comm.Append ((byte) 0x0); // UNUSED
			Comm.Append ((byte) id.Length);
			Comm.Append (id);
			Comm.Append ((short) sql.Length);
			Comm.Append (sql);

			Comm.SendPacket ();
			MoreResults = true;
			SkipToEnd ();

			return id;
		}

		protected override void ProcessColumnInfo ()
		{
			isSelectQuery = true; 
			/*int totalLength = */Comm.GetTdsShort ();
			int count = Comm.GetTdsShort ();
			for (int i = 0; i < count; i += 1) {
				string columnName = Comm.GetString (Comm.GetByte ());
				int status = Comm.GetByte ();
				bool hidden = (status & 0x01) > 0;
				bool isKey = (status & 0x02) > 0;
				bool isRowVersion = (status & 0x04) > 0;
				bool isUpdatable = (status & 0x10) > 0;
				bool allowDBNull = (status & 0x20) > 0;
				bool isIdentity = (status & 0x40) > 0;

				Comm.Skip (4); // User type

				byte type = Comm.GetByte ();
				bool isBlob = (type == 0x24);

				TdsColumnType columnType = (TdsColumnType) type;
				int bufLength = 0;

				byte precision = 0;
				byte scale = 0;

				if (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image) {
					bufLength = Comm.GetTdsInt ();
					Comm.Skip (Comm.GetTdsShort ());
				}
				else if (IsFixedSizeColumn (columnType))
					bufLength = LookupBufferSize (columnType);
				else
					//bufLength = Comm.GetTdsShort ();
					bufLength = Comm.GetByte ();

				if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric) {
					precision = Comm.GetByte ();
					scale = Comm.GetByte ();
				}

				Comm.Skip (Comm.GetByte ()); // Locale
				if (isBlob)
					Comm.Skip (Comm.GetTdsShort ()); // Class ID

				TdsDataColumn col = new TdsDataColumn ();
				Columns.Add (col);
#if NET_2_0
				col.ColumnType = columnType;
				col.ColumnName = columnName;
				col.IsIdentity = isIdentity;
				col.IsRowVersion = isRowVersion;
				col.ColumnType = columnType;
				col.ColumnSize = bufLength;
				col.NumericPrecision = precision;
				col.NumericScale = scale;
				col.IsReadOnly = !isUpdatable;
				col.IsKey = isKey;
				col.AllowDBNull = allowDBNull;
				col.IsHidden = hidden;
#else
				col ["ColumnType"] = columnType;
				col ["ColumnName"] = columnName;
				col ["IsIdentity"] = isIdentity;
				col ["IsRowVersion"] = isRowVersion;
				col ["ColumnType"] = columnType;
				col ["ColumnSize"] = bufLength;
				col ["NumericPrecision"] = precision;
				col ["NumericScale"] = scale;
				col ["IsReadOnly"] = !isUpdatable;
				col ["IsKey"] = isKey;
				col ["AllowDBNull"] = allowDBNull;
				col ["IsHidden"] = hidden;
#endif
			}
		}

		private void SendParamFormat ()
		{
			Comm.Append ((byte) TdsPacketSubType.ParamFormat);

			int len = 2 + (8 * Parameters.Count);
			TdsColumnType metaType;
			foreach (TdsMetaParameter p in Parameters) {
				metaType = p.GetMetaType ();
				if (!IsFixedSizeColumn (metaType))
					len += 1;
				if (metaType == TdsColumnType.Numeric || metaType == TdsColumnType.Decimal)
					len += 2;
			}

			Comm.Append ((short) len);
			Comm.Append ((short) Parameters.Count);

			foreach (TdsMetaParameter p in Parameters) {
				string locale = String.Empty;
				string parameterName = String.Empty;
				int userType = 0;

				byte status = 0x00;
				if (p.IsNullable)
					status |= 0x20;
				if (p.Direction == TdsParameterDirection.Output)
					status |= 0x01;

				metaType = p.GetMetaType ();

				Comm.Append ((byte) parameterName.Length);
				Comm.Append (parameterName);
				Comm.Append (status);
				Comm.Append (userType);
				Comm.Append ((byte) metaType);

				if (!IsFixedSizeColumn (metaType))
					Comm.Append ((byte) p.Size);         // MAXIMUM SIZE
				if (metaType == TdsColumnType.Numeric || metaType == TdsColumnType.Decimal) {
					Comm.Append (p.Precision);
					Comm.Append (p.Scale);
				}
				Comm.Append ((byte) locale.Length);
				Comm.Append (locale);
			}
		}

		private void SendParams ()
		{
			Comm.Append ((byte) TdsPacketSubType.Parameters);

			TdsColumnType metaType;
			foreach (TdsMetaParameter p in Parameters) {
				metaType = p.GetMetaType ();
				bool isNull = (p.Value == DBNull.Value || p.Value == null);
				if (!IsFixedSizeColumn (metaType))
					Comm.Append ((byte) p.GetActualSize ());
				if (!isNull)
					Comm.Append (p.Value);
			}
		}

		public override void Unprepare (string statementId)
		{
			Comm.StartPacket (TdsPacketType.Normal);
			Comm.Append ((byte) TdsPacketSubType.Dynamic);
			Comm.Append ((short) (3 + statementId.Length));
			Comm.Append ((byte) 0x04);
			Comm.Append ((byte) 0x00);
			Comm.Append ((byte) statementId.Length);
			Comm.Append (statementId);
			//Comm.Append ((short) 0);

			MoreResults = true;
			Comm.SendPacket ();
			SkipToEnd ();
		}

		protected override bool IsValidRowCount (byte status, byte op)
		{
			if (isSelectQuery)
				return (isSelectQuery = false);

			// TODO : Need to figure out how to calculate rowcount inside stored 
			// procedures. For now, Ignoring RowCount if they are returned by 
			// statements executing inside a StoredProcedure

			if (((status & (byte)0x40) != 0) || ((status & (byte)0x10) == 0))
				return false;

			return true;
		}

		#endregion // Methods
	}
}
