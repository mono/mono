//
// Mono.Data.TdsClient.Internal.Tds50.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace Mono.Data.TdsClient.Internal {
        internal class Tds50 : Tds
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds50;

		#endregion // Fields

		#region Constructors

		public Tds50 (string server, int port)
			: this (server, port, 8192, 15)
		{
		}

		public Tds50 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
		}

		#endregion // Constructors
	
		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");

			// some voodoo magic here.
			byte[] capabilities = {0x01,0x07,0x03,109,127,0xFF,0xFF,0xFF,0xFE,0x02,0x07,0x00,0x00,0x0A,104,0x00,0x00,0x00};

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
			tmp = Comm.Append (PacketSize.ToString (), 6, pad);
			Comm.Append ((byte) (tmp.Length < 6 ? tmp.Length : 6));

			Comm.Append (empty, 8, pad);
			// Padding...
			// 564-567
			//Comm.Append (empty, 4, pad);

			// Capabilities
			//Comm.Append ((byte) TdsPacketType.Capability);
			//Comm.Append ((short) 18);
			//Comm.Append (capabilities, 18, pad);

			Comm.SendPacket ();

			bool done = false;
			while (!done) {
				TdsPacketResult result = ProcessSubPacket ();
				done = (result is TdsPacketEndTokenResult);
			}
			return IsConnected;
		}

		protected override TdsPacketColumnInfoResult ProcessColumnInfo ()
		{
			byte precision;
			byte scale;
			int totalLength = Comm.GetTdsShort ();
			int bytesRead = 0;

			TdsPacketColumnInfoResult result = new TdsPacketColumnInfoResult ();

			while (bytesRead < totalLength) {
				scale = 0;
				precision = 0;

				int bufLength = -1;
				//int dispSize = -1;
				byte[] flagData = new byte[4];
				for (int i = 0; i < 4; i += 1) {
					flagData[i] = Comm.GetByte ();
					bytesRead += 1;
				}
				bool nullable = (flagData[2] & 0x01) > 0;
				bool caseSensitive = (flagData[2] & 0x02) > 0;
				bool writable = (flagData[2] & 0x0c) > 0;
				bool autoIncrement = (flagData[2] & 0x10) > 0;

				string tableName = String.Empty;
				TdsColumnType columnType = (TdsColumnType) Comm.GetByte ();

				bytesRead += 1;

				if (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image) {
					Comm.Skip (4);
					bytesRead += 4;

					int tableNameLength = Comm.GetTdsShort ();
					bytesRead += 2;
					tableName = Comm.GetString (tableNameLength);
					bytesRead += tableNameLength;
					bufLength = 2 << 31 - 1;
				}
				else if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric) {
					bufLength = Comm.GetByte ();
					bytesRead += 1;
					precision = Comm.GetByte ();
					bytesRead += 1;
					scale = Comm.GetByte ();
					bytesRead += 1;
				}
				else if (IsFixedSizeColumn (columnType))
					bufLength = LookupBufferSize (columnType);
				else {
					bufLength = (int) Comm.GetByte () & 0xff;
					bytesRead += 1;
				}

				int index = result.Add (new TdsSchemaInfo ());
				result[index]["NumericPrecision"] = precision;
				result[index]["NumericScale"] = scale;
				result[index]["ColumnSize"] = bufLength;
				result[index]["ColumnName"] = ColumnNames[index];
				result[index]["BaseTableName"] = tableName;
				result[index]["AllowDBNull"] = nullable;
				result[index]["IsReadOnly"] = !writable;
				result[index]["ColumnType"] = columnType;
			}

			//int skipLength = totalLength - bytesRead;
			//if (skipLength != 0)
				//throw new TdsException ("skipping");

			return result;
		}

		#endregion // Methods
	}
}
