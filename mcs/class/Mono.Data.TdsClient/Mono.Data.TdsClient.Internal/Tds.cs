//
// Mono.Data.TdsClient.Internal.TdsInternal.cs
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
        internal class TdsInternal 
	{
		#region Fields

		TdsServerTypeInternal serverType;
		TdsCommInternal comm;
		TdsVersionInternal tdsVersion;
		TdsConnectionParametersInternal parms;
		TdsCommandInternal command;
		Encoding encoding;
		IsolationLevel isolationLevel;
		bool autoCommit;
                Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

		#endregion // Fields

		#region Properties

		public TdsCommandInternal Command {
			get { return command; }
			set { command = value; }
		}

		public string Database {
			get { return parms.Database; }
			set { parms.Database = value; }
		}

		public TdsVersionInternal TdsVersion {
			get { return tdsVersion; }
			set { tdsVersion = value; }
		}

		#endregion // Properties

		#region Constructors

		public TdsInternal (TdsConnectionInternal connection, TdsConnectionParametersInternal parms)
		{
                        IPHostEntry hostEntry = Dns.GetHostByName (parms.Host);
                        IPAddress[] addresses = hostEntry.AddressList;

                        IPEndPoint endPoint;

                        foreach (IPAddress address in addresses) {
                                endPoint = new IPEndPoint (address, parms.Port);
                                socket.Connect (endPoint);

                                if (socket.Connected)
                                        break;
                        }

			encoding = Encoding.GetEncoding (parms.Encoding);
			comm = new TdsCommInternal (socket, parms.PacketSize, tdsVersion);
		}	

		#endregion // Constructors

		#region Methods

		public void ChangeDatabase (string databaseName)
		{
                        string query = String.Format ("use {0}", databaseName);
                        comm.StartPacket (TdsPacketTypeInternal.Query);
                        if (tdsVersion == TdsVersionInternal.tds70)
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
			if( query.Length == 0)
				return true;

			comm.StartPacket (TdsPacketTypeInternal.Query);
			if (tdsVersion == TdsVersionInternal.tds70)
				comm.AppendChars (query);
			else {
				byte[] queryBytes = encoding.GetBytes (query);
				comm.AppendBytes (queryBytes, queryBytes.Length, (byte) 0);
			}
			comm.SendPacket ();

			return isOkay;
		}

		public bool Logon (TdsConnectionParametersInternal parms)
		{
			byte pad = (byte) 0;
			byte[] empty = new byte[0];

			if (tdsVersion == TdsVersionInternal.tds70) {
				Send70Logon (parms);
			} else {
				comm.StartPacket (TdsPacketTypeInternal.Logon);

				// hostname (offset 0)
				byte[] tmp = encoding.GetBytes (parms.Host);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// username (offset 31 0x1f)
				tmp = encoding.GetBytes (parms.User);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// password (offset 62 0x3e)
				tmp = encoding.GetBytes (parms.Password);
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
				tmp = encoding.GetBytes (parms.ApplicationName);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// server name
				tmp = encoding.GetBytes (parms.Host);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// remote passwords
				comm.AppendBytes (empty, 2, pad);
				tmp = encoding.GetBytes (parms.Password);
				comm.AppendBytes (tmp, 253, pad);
				comm.AppendByte ((byte) (tmp.Length < 253 ? tmp.Length + 2 : 253 + 2));

				// tds version
				comm.AppendByte ((byte) (((byte) parms.TdsVersion) / 10));
				comm.AppendByte ((byte) (((byte) parms.TdsVersion) % 10));
				comm.AppendByte ((byte) 0);
				comm.AppendByte ((byte) 0);

				// prog name
				tmp = encoding.GetBytes (parms.ProgName);
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
				tmp = encoding.GetBytes (parms.Language);
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
				tmp = encoding.GetBytes (parms.Encoding);
				comm.AppendBytes (tmp, 30, pad);
				comm.AppendByte ((byte) (tmp.Length < 30 ? tmp.Length : 30));

				// notify on charset change
				comm.AppendByte ((byte) 1);

				// length of tds packets
				tmp = encoding.GetBytes (parms.PacketSize.ToString ());
				comm.AppendBytes (tmp, 6, pad);
				comm.AppendByte ((byte) 3);

				// pad out to a longword
				comm.AppendBytes (empty, 8, pad);
			}

			comm.SendPacket ();
			return true;
			
		}

		public void Send70Logon (TdsConnectionParametersInternal parms)
		{
			short packSize = (short) (86 + 2 * (parms.User.Length + parms.Password.Length + parms.ApplicationName.Length + parms.Host.Length + parms.LibraryName.Length + parms.Database.Length));
			byte[] empty = new byte[0];
			byte pad = (byte) 0;

			comm.StartPacket (TdsPacketTypeInternal.Logon70);
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
			comm.AppendTdsShort ((short) parms.User.Length);
			curPos += (short) (parms.User.Length * 2);

			// Password
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) parms.Password.Length);
			curPos += (short) (parms.Password.Length * 2);

			// AppName
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) parms.ApplicationName.Length);
			curPos += (short) (parms.ApplicationName.Length * 2);

			// Server Name
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) parms.Host.Length);
			curPos += (short) (parms.Host.Length * 2);

			// Unknown
			comm.AppendTdsShort ((short) 0);
			comm.AppendTdsShort ((short) 0);

			// Library Name
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) parms.LibraryName.Length);
			curPos += (short) (parms.LibraryName.Length * 2);

			// Unknown
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) 0);

			// Database
			comm.AppendTdsShort (curPos);
			comm.AppendTdsShort ((short) parms.Database.Length);
			curPos += (short) (parms.Database.Length * 2);

			// MAC Address
			comm.AppendBytes (empty, 6, pad);
			comm.AppendTdsShort (curPos);

			// Some sort of appended magic
			comm.AppendTdsShort ((short) 0);
			comm.AppendTdsInt (packSize);

			string scrambledPwd = Tds7CryptPass (parms.Password);
			comm.AppendChars (parms.User);
			comm.AppendChars (scrambledPwd);
			comm.AppendChars (parms.ApplicationName);
			comm.AppendChars (parms.Host);
			comm.AppendChars (parms.LibraryName);
			comm.AppendChars (parms.Database);
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
			if (serverType == TdsServerTypeInternal.Sybase) {
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
