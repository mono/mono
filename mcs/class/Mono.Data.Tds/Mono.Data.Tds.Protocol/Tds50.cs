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
			TdsPacketColumnInfoResult result = new TdsPacketColumnInfoResult ();
			int totalLength = Comm.GetTdsShort ();	
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
					bufLength = Comm.GetTdsShort ();

				if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric) {
					precision = Comm.GetByte ();
					scale = Comm.GetByte ();
				}

				Comm.Skip (Comm.GetByte ()); // Locale
				if (isBlob)
					Comm.Skip (Comm.GetTdsShort ()); // Class ID

				int index = result.Add (new TdsSchemaInfo ());
				result[index]["NumericPrecision"] = precision;
				result[index]["NumericScale"] = scale;
				result[index]["ColumnSize"] = bufLength;
				result[index]["ColumnName"] = columnName;
				result[index]["AllowDBNull"] = allowDBNull;
				result[index]["IsReadOnly"] = !isUpdatable;
				result[index]["IsIdentity"] = isIdentity;
				result[index]["IsRowVersion"] = isRowVersion;
				result[index]["IsKey"] = isKey;
				result[index]["Hidden"] = hidden;
				result[index]["ColumnType"] = columnType;
			}
			return result;
		}

		#endregion // Methods
	}
}
