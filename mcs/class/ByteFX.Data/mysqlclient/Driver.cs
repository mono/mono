// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Security.Cryptography;
using ByteFX.Data.Common;
using System.Text;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for Driver.
	/// </summary>
	internal class Driver
	{
		protected const int HEADER_LEN = 4;
		protected const int MIN_COMPRESS_LENGTH = 50;

		protected MySqlStream		stream;
		protected Encoding			encoding;
		protected byte				packetSeq;
		protected int				timeOut;
		protected long				maxPacketSize;
		protected Packet			peekedPacket = null;
		protected ByteFX.Data.Common.Version	serverVersion;
		protected bool				isOpen;

		int		protocol;
		uint	threadID;
		String	encryptionSeed;
		int		serverCaps;
		bool	useCompression = false;


		public Driver()
		{
			packetSeq = 0;
			encoding = System.Text.Encoding.Default;
			isOpen = false;
		}

		#region Properties
		public bool IsDead
		{
			get 
			{ 
				return stream.IsClosed;
			}
		}
		#endregion

		public Encoding Encoding 
		{
			get { return encoding; }
			set { encoding = value; }
		}

		public long MaxPacketSize 
		{
			get { return maxPacketSize; }
			set { maxPacketSize = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="userid"></param>
		/// <param name="password"></param>
		public void Open( String host, int port, String userid, String password, 
			bool UseCompression, int connectTimeout ) 
		{
			timeOut = connectTimeout;
			stream = new MySqlStream( host, port, timeOut );

			Packet packet = ReadPacket();

			// read off the protocol version
			protocol = packet.ReadByte();
			serverVersion = ByteFX.Data.Common.Version.Parse( packet.ReadString() );
			threadID = packet.ReadInteger(4);
			encryptionSeed = packet.ReadString();

			// read in Server capabilities if they are provided
			serverCaps = 0;
			if (packet.CanRead)
				serverCaps = (int)packet.ReadInteger(2);

			Authenticate( userid, password, UseCompression );
			isOpen = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="password"></param>
		private void Authenticate( String userid, String password, bool UseCompression )
		{
			ClientParam clientParam = ClientParam.CLIENT_FOUND_ROWS | ClientParam.CLIENT_LONG_FLAG;

			if ((serverCaps & (int)ClientParam.CLIENT_COMPRESS) != 0 && UseCompression)
			{
				clientParam |= ClientParam.CLIENT_COMPRESS;
			}

			clientParam |= ClientParam.CLIENT_LONG_PASSWORD;
			clientParam |= ClientParam.CLIENT_LOCAL_FILES;
//			if (serverVersion.isAtLeast(4,1,0))
//				clientParam |= ClientParam.CLIENT_PROTOCOL_41;
			if ( (serverCaps & (int)ClientParam.CLIENT_SECURE_CONNECTION ) != 0 && password.Length > 0 )
				clientParam |= ClientParam.CLIENT_SECURE_CONNECTION;

			int packetLength = userid.Length + 16 + 6 + 4;  // Passwords can be 16 chars long

			Packet packet = new Packet();// packetLength );

			if ((clientParam & ClientParam.CLIENT_PROTOCOL_41) != 0)
			{
				packet.WriteInteger( (int)clientParam, 4 );
				packet.WriteInteger( (256*256*256)-1, 4 );
			}
			else
			{
				packet.WriteInteger( (int)clientParam, 2 );
				packet.WriteInteger( 255*255*255, 3 );
			}

			packet.WriteString( userid, encoding  );
			if ( (clientParam & ClientParam.CLIENT_SECURE_CONNECTION ) != 0 )
			{
				// use the new authentication system
				AuthenticateSecurely( packet, password );
			}
			else
			{
				// use old authentication system
				packet.WriteString( EncryptPassword(password, encryptionSeed, protocol > 9), encoding );
				SendPacket(packet);
			}

			packet = ReadPacket();
			if ((clientParam & ClientParam.CLIENT_COMPRESS) != 0)
				useCompression = true;
		}

		/// <summary>
		/// AuthenticateSecurity implements the new 4.1 authentication scheme
		/// </summary>
		/// <param name="password"></param>
		private void AuthenticateSecurely( Packet packet, string password )
		{
			packet.WriteString("xxxxxxxx", encoding );
			SendPacket(packet);

			packet = ReadPacket();

			// compute pass1 hash
			string newPass = password.Replace(" ","").Replace("\t","");
			SHA1 sha = new SHA1CryptoServiceProvider(); 
			byte[] firstPassBytes = sha.ComputeHash( System.Text.Encoding.Default.GetBytes(newPass));

			byte[] salt = packet.GetBytes();
			byte[] input = new byte[ firstPassBytes.Length + 4 ];
			salt.CopyTo( input, 0 );
			firstPassBytes.CopyTo( input, 4 );
			byte[] outPass = new byte[100];
			byte[] secondPassBytes = sha.ComputeHash( input );

			byte[] cryptSalt = new byte[20];
			Security.ArrayCrypt( salt, 4, cryptSalt, 0, secondPassBytes, 20 );

			Security.ArrayCrypt( cryptSalt, 0, firstPassBytes, 0, firstPassBytes, 20 );

			// send the packet
			packet = new Packet();
			packet.WriteBytes( firstPassBytes, 0, 20 );
			SendPacket(packet);
		}


		/// <summary>
		/// 
		/// </summary>
		private Packet ReadRawPacket()
		{
			int packetLength = stream.ReadInt24();
			int unCompressedLen = 0;

			// read the packet sequence and make sure it makes sense
			byte seq = (byte)stream.ReadByte();
			if (seq != packetSeq) 
				throw new MySqlException("Unknown transmission status: sequence out of order");
	
			if (useCompression) 
				unCompressedLen = stream.ReadInt24();

			byte[] buffer;
			if (useCompression && unCompressedLen > 0)
			{
				byte[] compressed_buffer = new Byte[packetLength];
				buffer = new Byte[unCompressedLen];

				// read in the compressed data
				stream.Read( compressed_buffer, 0, packetLength );

				// inflate it
				Inflater i = new Inflater();
				i.SetInput( compressed_buffer );
				i.Inflate( buffer );
			}
			else 
			{
				buffer = new Byte[packetLength];
				stream.Read( buffer, 0, packetLength);
			}

			packetSeq++;
			Packet packet = new Packet( buffer );
			packet.Encoding = encoding;
			return packet;
		}

		/// <summary>
		/// 
		/// </summary>
		public void SendFileToServer()
		{
		}

		public void ClearPeekedPacket()
		{
			peekedPacket = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Packet PeekPacket()
		{
			// we can peek the same packet more than once
			if (peekedPacket != null)
				return peekedPacket;

			peekedPacket = ReadPacket();
			return peekedPacket;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Packet ReadPacket()
		{
			// if we have a peeked packet, return it now
			if (peekedPacket != null) 
			{
				Packet p = peekedPacket;
				peekedPacket = null;
				return p;
			}

			Packet packet = ReadRawPacket();

			if (packet.Type == PacketType.Error)
			{
				int errorCode = (int)packet.ReadInteger(2);
				string msg = packet.ReadString();
				throw new MySqlException( msg, errorCode );
			}
			else 
				packet.Position = 0;

			return packet;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="packet"></param>
		private Packet LoadSchemaIntoPacket( Packet packet, int count )
		{
			for (int i=0; i < count; i++) 
			{
				Packet colPacket = ReadRawPacket();
				packet.AppendPacket( colPacket );
			}
			Packet lastPacket = ReadRawPacket();
			if (lastPacket.Type != PacketType.Last)
				throw new MySqlException("Last packet not received when expected");

			packet.Type = PacketType.ResultSchema;
			packet.Position = 0;
			return packet;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
/*		protected byte[] CompressPacket(Packet packet)
		{
			// compress the entire packet except the length

			// make sure we are using a packet prep'ed for compression
			// and that our packet is large enough to warrant compression
			// re: my_compress.c from mysql src
			int offset = HEADER_LEN + COMPRESS_HEADER_LEN;
			int original_len = (int)(_packet.Length - offset);
			if (original_len < MIN_COMPRESS_LEN) return 0;

			byte[] packetData = _packet.ToArray();

			byte[] output = new Byte[ original_len * 2 ];
			Deflater d = new Deflater();
			d.SetInput( packetData, offset, original_len );
			d.Finish();
			int comp_len = d.Deflate( output, offset, output.Length - offset  );

			if (comp_len > original_len) return 0;
			_packet = new MemoryStream( output, 0, comp_len + offset );
			return (int)comp_len;
		}
*/
		protected byte[] CompressPacket(Packet packet)
		{
			if (packet.Length < MIN_COMPRESS_LENGTH) return null;

			byte[] compressed_buffer = new byte[packet.Length * 2];
			Deflater deflater = new Deflater();
			deflater.SetInput( packet.GetBytes(), 0, packet.Length );
			deflater.Finish();
			int comp_len = deflater.Deflate( compressed_buffer, 0, compressed_buffer.Length );
			if (comp_len > packet.Length) return null;
			return compressed_buffer;
		}

		protected void SendPacket(Packet packet)
		{
			Packet header = null;
			byte[] buffer = null;

			if (useCompression)
			{
				byte[] compressed_bytes = CompressPacket(packet);
				header = new Packet();
				
				// if we succeeded in compressing
				if (compressed_bytes != null) 
				{
					header.WriteInteger( compressed_bytes.Length, 3 );
					header.WriteByte( packetSeq );
					header.WriteInteger( packet.Length + HEADER_LEN, 3 );
					buffer = compressed_bytes;
				}
				else
				{
					header.WriteInteger( packet.Length + HEADER_LEN, 3 );
					header.WriteByte( packetSeq );
					header.WriteInteger( 0, 3 );
					buffer = packet.GetBytes();
				}
				// now write the internal header
				header.WriteInteger( packet.Length, 3 );
				header.WriteByte( 0 );
			}
			else 
			{
				header = new Packet();
				header.WriteInteger( packet.Length, 3 );
				header.WriteByte( packetSeq );
				buffer = packet.GetBytes();
			}
			packetSeq++;

			// send the data to eth server
			stream.Write( header.GetBytes(), 0, header.Length );
			stream.Write( buffer, 0, buffer.Length );
			stream.Flush();
		}


		public void Close() 
		{
			stream.Close();
		}


		/// <summary>
		/// Sends the specified command to the database
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="text">Text attribute of command</param>
		/// <returns>Result packet returned from database server</returns>
		public void SendCommand( DBCmd command, String text ) 
		{
			Packet packet = new Packet();
			packetSeq = 0;
			packet.WriteByte( (byte)command );
			packet.WriteStringNoNull( text, encoding );
			SendPacket(packet);
			
			packet = ReadPacket();
			if (packet.Type != PacketType.UpdateOrOk)
				throw new MySqlException("SendCommand failed for command " + text );
		}

		/// <summary>
		/// SendQuery sends a byte array of SQL to the server
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>A packet containing the bytes returned by the server</returns>
		public Packet SendQuery( byte[] sql )
		{
			Packet packet = new Packet();
			packetSeq = 0;
			packet.WriteByte( (byte)DBCmd.QUERY );
			packet.WriteBytes( sql, 0, sql.Length );

			SendPacket( packet );
			return ReadPacket();
		}

		public Packet SendSql( string sql )
		{
			byte[] bytes = encoding.GetBytes(sql);

			Packet packet = new Packet();
			packetSeq = 0;
			packet.WriteByte( (byte)DBCmd.QUERY );
			packet.WriteBytes( bytes, 0, bytes.Length );

			SendPacket( packet );
			packet = ReadPacket();

			switch (packet.Type)
			{
				case PacketType.LoadDataLocal:
					SendFileToServer();
					return null;

				case PacketType.Other:
					packet.Position = 0;
					int count = (int)packet.ReadLenInteger();
					if (count > 0) 
						return LoadSchemaIntoPacket( packet, count );
					else
						return packet;
			}

			return packet;
		}

		#region PasswordStuff
		private static double rand(ref long seed1, ref long seed2)
		{
			seed1 = (seed1 * 3) + seed2;
			seed1 %= 0x3fffffff;
			seed2 = (seed1 + seed2 + 33) % 0x3fffffff;
			return (seed1 / (double)0x3fffffff);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="password"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static String EncryptPassword(String password, String message, bool new_ver)
		{
			if (password == null || password.Length == 0)
				return password;

			long[] hash_message = Hash(message);
			long[] hash_pass = Hash(password);

			long seed1 = (hash_message[0]^hash_pass[0]) % 0x3fffffff;
			long seed2 = (hash_message[1]^hash_pass[1]) % 0x3fffffff;

			char[] scrambled = new char[message.Length];
			for (int x=0; x < message.Length; x++) 
			{
				double r = rand(ref seed1, ref seed2);
				scrambled[x] = (char)(Math.Floor(r*31) + 64);
			}

			if (new_ver)
			{						/* Make it harder to break */
				char extra = (char)Math.Floor( rand(ref seed1, ref seed2) * 31 );
				for (int x=0; x < scrambled.Length; x++)
					scrambled[x] ^= extra;
			}

			return new string(scrambled);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="P"></param>
		/// <returns></returns>
		static long[] Hash(String P) 
		{
			long val1 = 1345345333;
			long val2 = 0x12345671;
			long inc  = 7;

			for (int i=0; i < P.Length; i++) 
			{
				if (P[i] == ' ' || P[i] == '\t') continue;
				long temp = (long)(0xff & P[i]);
				val1 ^= (((val1 & 63)+inc)*temp) + (val1 << 8);
				val2 += (val2 << 8) ^ val1;
				inc += temp;
			}

			long[] hash = new long[2];
			hash[0] = val1 & 0x7fffffff;
			hash[1] = val2 & 0x7fffffff;
			return hash;
		}
		#endregion
	}
}
