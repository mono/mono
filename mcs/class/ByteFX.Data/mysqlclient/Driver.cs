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
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Security.Cryptography;
using ByteFX.Data.Common;
using System.Collections;
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
		protected const int MAX_PACKET_SIZE = 256*256*256-1;

		protected Stream			stream;
		protected BufferedStream	writer;
		protected Encoding			encoding;
		protected byte				packetSeq;
		protected long				maxPacketSize;
		protected DBVersion			serverVersion;
		protected bool				isOpen;
		protected string			versionString;
		protected Packet			peekedPacket;

		protected int				protocol;
		protected uint				threadID;
		protected String			encryptionSeed;
		protected int				serverCaps;
		protected bool				useCompression = false;


		public Driver()
		{
			packetSeq = 0;
			encoding = System.Text.Encoding.Default;
			isOpen = false;
		}

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

		public string VersionString 
		{
			get { return versionString; }
		}

		public DBVersion Version 
		{
			get { return serverVersion; }
		}

		public void Open( MySqlConnectionString settings )
		{
			// connect to one of our specified hosts
			try 
			{
				StreamCreator sc = new StreamCreator( settings.Server, settings.Port, settings.PipeName );
				stream = sc.GetStream( settings.ConnectionTimeout );
			}
			catch (Exception ex)
			{
				throw new MySqlException("Unable to connect to any of the specified MySQL hosts", ex);
			}

			if (stream == null) 
				throw new MySqlException("Unable to connect to any of the specified MySQL hosts");

			writer = new BufferedStream( stream );
			// read off the welcome packet and parse out it's values
			Packet packet = ReadPacket();
			protocol = packet.ReadByte();
			versionString = packet.ReadString();
			serverVersion = DBVersion.Parse( versionString );
			threadID = (uint)packet.ReadInteger(4);
			encryptionSeed = packet.ReadString();

			// read in Server capabilities if they are provided
			serverCaps = 0;
			if (packet.HasMoreData)
				serverCaps = (int)packet.ReadInteger(2);

			Authenticate( settings.UserId, settings.Password, settings.UseCompression );

			// if we are using compression, then we use our CompressedStream class
			// to hide the ugliness of managing the compression
			if (settings.UseCompression)
			{
				stream = new CompressedStream( stream );
				writer = new BufferedStream( stream );
			}

			isOpen = true;
		}

		private Packet CreatePacket( byte[] buf )
		{
			if (buf == null)
				return new Packet( serverVersion.isAtLeast(3, 22, 5) );
			return new Packet( buf, serverVersion.isAtLeast(3, 22, 5 ));
		}

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
//			if ( (serverCaps & (int)ClientParam.CLIENT_SECURE_CONNECTION ) != 0 && password.Length > 0 )
//				clientParam |= ClientParam.CLIENT_SECURE_CONNECTION;

			int packetLength = userid.Length + 16 + 6 + 4;  // Passwords can be 16 chars long

			Packet packet = CreatePacket(null);

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
				// pad zeros out to packetLength for auth
				for (int i=0; i < (packetLength-packet.Length); i++)
					packet.WriteByte(0);
				SendPacket(packet);
			}

			packet = ReadPacket();
			if ((clientParam & ClientParam.CLIENT_COMPRESS) != 0)
				useCompression = true;
		}

		/// <summary>
		/// AuthenticateSecurity implements the new 4.1 authentication scheme
		/// </summary>
		/// <param name="packet">The in-progress packet we use to complete the authentication</param>
		/// <param name="password">The password of the user to use</param>
		private void AuthenticateSecurely( Packet packet, string password )
		{
			packet.WriteString("xxxxxxxx", encoding );
			SendPacket(packet);

			packet = ReadPacket();

			// compute pass1 hash
			string newPass = password.Replace(" ","").Replace("\t","");
			SHA1 sha = new SHA1CryptoServiceProvider(); 
			byte[] firstPassBytes = sha.ComputeHash( System.Text.Encoding.Default.GetBytes(newPass));

			byte[] salt = packet.GetBuffer();
			byte[] input = new byte[ firstPassBytes.Length + 4 ];
			salt.CopyTo( input, 0 );
			firstPassBytes.CopyTo( input, 4 );
			byte[] outPass = new byte[100];
			byte[] secondPassBytes = sha.ComputeHash( input );

			byte[] cryptSalt = new byte[20];
			Security.ArrayCrypt( salt, 4, cryptSalt, 0, secondPassBytes, 20 );

			Security.ArrayCrypt( cryptSalt, 0, firstPassBytes, 0, firstPassBytes, 20 );

			// send the packet
			packet = CreatePacket(null);
			packet.Write( firstPassBytes, 0, 20 );
			SendPacket(packet);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Packet PeekPacket()
		{
			if (peekedPacket != null)
				return peekedPacket;

			peekedPacket = ReadPacket();
			return peekedPacket;
		}

		/// <summary>
		/// ReadBuffer continuously loops until it has read the entire
		/// requested data
		/// </summary>
		/// <param name="buf">Buffer to read data into</param>
		/// <param name="offset">Offset to place the data</param>
		/// <param name="length">Number of bytes to read</param>
		private void ReadBuffer( byte[] buf, int offset, int length )
		{
			while (length > 0)
			{
				int amountRead = stream.Read( buf, offset, length );
				if (amountRead == 0)
					throw new MySqlException("Unexpected end of data encountered");
				length -= amountRead;
				offset += amountRead;
			}
		}

		private Packet ReadPacketFromServer()
		{
			int len = stream.ReadByte() + (stream.ReadByte() << 8) +
				(stream.ReadByte() << 16);
			byte seq = (byte)stream.ReadByte();
			byte[] buf = new byte[ len ];
			ReadBuffer( buf, 0, len );

			if (seq != packetSeq) 
				throw new MySqlException("Unknown transmission status: sequence out of order");
			packetSeq++;

			Packet p = CreatePacket(buf);
			p.Encoding = this.Encoding;
			if (p.Length == MAX_PACKET_SIZE && serverVersion.isAtLeast(4,0,0)) 
				p.Append( ReadPacketFromServer() );
			return p;
		}

		/// <summary>
		/// Reads a single packet off the stream
		/// </summary>
		/// <returns></returns>
		public Packet ReadPacket()
		{
			// if we have peeked at a packet, then return it
			if (peekedPacket != null)
			{
				Packet packet = peekedPacket;
				peekedPacket = null;
				return packet;
			}

			Packet p = ReadPacketFromServer();

			// if this is an error packet, then throw the exception
			if (p[0] == 0xff)
			{
				p.ReadByte();
				int errorCode = (int)p.ReadInteger(2);
				string msg = p.ReadString();
				throw new MySqlException( msg, errorCode );
			}
			
			return p;
		}

		protected MemoryStream CompressBuffer(byte[] buf, int index, int length)
		{

			if (length < MIN_COMPRESS_LENGTH) return null;

			MemoryStream ms = new MemoryStream(buf.Length);
			DeflaterOutputStream dos = new DeflaterOutputStream(ms);

			dos.WriteByte( (byte)(length & 0xff ));
			dos.WriteByte( (byte)((length >> 8) & 0xff ));
			dos.WriteByte( (byte)((length >> 16) & 0xff ));
			dos.WriteByte( 0 );

			dos.Write( buf, index, length );
			dos.Finish();
			if (ms.Length > length+4) return null;
			return ms;
		}

		private void WriteInteger( int v, int numbytes )
		{
			int val = v;

			if (numbytes < 1 || numbytes > 4) 
				throw new ArgumentOutOfRangeException("Wrong byte count for WriteInteger");

			for (int x=0; x < numbytes; x++)
			{
				writer.WriteByte( (byte)(val&0xff) );
				val >>= 8;
			}
		}

		/// <summary>
		/// Send a buffer to the server in a compressed form
		/// </summary>
		/// <param name="buf">Byte buffer to send</param>
		/// <param name="index">Location in buffer to start sending</param>
		/// <param name="length">Amount of data to send</param>
		protected void SendCompressedBuffer(byte[] buf, int index, int length)
		{
			MemoryStream compressed_bytes = CompressBuffer(buf, index, length);
			int comp_len = compressed_bytes == null ? length+HEADER_LEN : (int)compressed_bytes.Length;
			int ucomp_len = compressed_bytes == null ? 0 : length+HEADER_LEN;

			WriteInteger( comp_len, 3 );
			writer.WriteByte( packetSeq++ );
			WriteInteger( ucomp_len, 3 );
			if (compressed_bytes != null)
				writer.Write( compressed_bytes.GetBuffer(), 0, (int)compressed_bytes.Length );
			else 
			{
				WriteInteger( length, 3 );	
				writer.WriteByte( 0 );
				writer.Write( buf, index, length );
			}
			stream.Flush();
		}

		protected void SendBuffer( byte[] buf, int offset, int length )
		{
			while (length > 0)
			{
				int amount = Math.Min( 1024, length );
				writer.Write( buf, offset, amount );
				writer.Flush();
				offset += amount;
				length -= amount;
			}
		}

		/// <summary>
		/// Send a single packet to the server.
		/// </summary>
		/// <param name="packet">Packet to send to the server</param>
		/// <remarks>This method will send a single packet to the server
		/// possibly breaking the packet up into smaller packets that are
		/// smaller than max_allowed_packet.  This method will always send at
		/// least one packet to the server</remarks>
        protected void SendPacket(Packet packet)
		{
			byte[]	buf = packet.GetBuffer();
			int		len = packet.Length;
			int		index = 0;
			bool	oneSent = false;

			// make sure we are not trying to send too much
			if (packet.Length > maxPacketSize && maxPacketSize > 0)
				throw new MySqlException("Packet size too large.  This MySQL server cannot accept rows larger than " + maxPacketSize + " bytes.");

			try 
			{
				while (len > 0 || ! oneSent) 
				{
					int lenToSend = Math.Min( len, MAX_PACKET_SIZE );

					// send the data
					if (useCompression)
						SendCompressedBuffer( buf, index, lenToSend );
					else 
					{
						WriteInteger( lenToSend, 3 );
						writer.WriteByte( packetSeq++ );
						writer.Write( buf, index, lenToSend );
						writer.Flush();
					}

					len -= lenToSend;
					index += lenToSend;
					oneSent = true;
				}
				writer.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine( ex.Message );
			}
		}


		public void Close() 
		{
			if (stream != null)
				stream.Close();
		}


		/// <summary>
		/// Sends the specified command to the database
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="text">Text attribute of command</param>
		/// <returns>Result packet returned from database server</returns>
		public void Send( DBCmd command, String text ) 
		{
			CommandResult result = Send( command, this.Encoding.GetBytes( text ) );
			if (result.IsResultSet)
				throw new MySqlException("SendCommand failed for command " + text );
		}

		public CommandResult Send( DBCmd cmd, byte[] bytes )
		{
//			string s = Encoding.GetString( bytes );

			Packet packet = CreatePacket(null);
			packetSeq = 0;
			packet.WriteByte( (byte)cmd );
			if (bytes != null)
				packet.Write( bytes, 0, bytes.Length );

			SendPacket( packet );
			packet = ReadPacket();

			// first check to see if this is a LOAD DATA LOCAL callback
			// if so, send the file and then read the results
			long fieldcount = packet.ReadLenInteger();
			if (fieldcount == Packet.NULL_LEN)
			{
				string filename = packet.ReadString();
				SendFileToServer( filename );
				packet = ReadPacket();
			}
			else
				packet.Position = 0;

			return new CommandResult(packet, this);
		}

		/// <summary>
		/// Sends the specified file to the server. 
		/// This supports the LOAD DATA LOCAL INFILE
		/// </summary>
		/// <param name="filename"></param>
		private void SendFileToServer( string filename )
		{
			Packet		p = CreatePacket(null);
			byte[]		buffer = new byte[4092];
			FileStream	fs = null;
			try 
			{
				fs = new FileStream( filename, FileMode.Open );
				int count = fs.Read( buffer, 0, buffer.Length );
				while (count != 0) 
				{
					if ((p.Length + count) > MAX_PACKET_SIZE)
					{
						SendPacket( p );
						p.Clear();
					}
					p.Write( buffer, 0, count );
					count = fs.Read( buffer, 0, buffer.Length );
				}
				fs.Close();

				// send any remaining data
				if (p.Length > 0) 
				{
					SendPacket(p);
					p.Clear();
				}
			}
			catch (Exception ex)
			{
				throw new MySqlException("Error during LOAD DATA LOCAL INFILE", ex);
			}
			finally 
			{
				if (fs != null)
					fs.Close();
				// empty packet signals end of file
				p.Clear();
				SendPacket(p);
			}
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
		/// Encrypts a password using the MySql encryption scheme
		/// </summary>
		/// <param name="password">The password to encrypt</param>
		/// <param name="message">The encryption seed the server gave us</param>
		/// <param name="new_ver">Indicates if we should use the old or new encryption scheme</param>
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
