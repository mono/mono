//
// Mono.Data.TdsClient.Internal.Tds70.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace Mono.Data.TdsClient.Internal {
        internal class Tds70 : Tds, ITds
	{
		#region Fields

		public readonly static TdsVersion Version = TdsVersion.tds70;

		#endregion // Fields

		#region Constructors

		public Tds70 (string server, int port)
			: this (server, port, 512)
		{
		}

		public Tds70 (string server, int port, int packetSize)
			: base (server, port, packetSize, Version)
		{
		}

		#endregion // Constructors

		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
Console.WriteLine ("Connecting...");
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");

			bool isOkay = true;

			SetLanguage (connectionParameters.Language);

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

                        while (!((result = ProcessSubPacket()) is TdsPacketEndTokenResult)) {
				if (result is TdsPacketErrorResult) {
					isOkay = false;
					break;
				}
				// XXX Should really process some more types of packets.
			}

			IsConnected = isOkay;
			return isOkay;
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

		#endregion // Methods
	}
}
