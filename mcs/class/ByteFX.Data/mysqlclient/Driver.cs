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

namespace ByteFX.Data.MySQLClient
{
	/// <summary>
	/// Summary description for Driver.
	/// </summary>
	internal class Driver : IDisposable
	{
		protected const	int COMPRESS_HEADER_LEN = 3;
		protected const int HEADER_LEN = 4;
		protected const int MIN_COMPRESS_LEN = 50;

		public MemoryStream		_packet;
		protected Stream		_stream;
		protected Socket		_socket;
		protected int			m_Seq;
		protected int			m_BufIndex;
		protected byte			m_LastResult;
		protected byte[]		m_Buffer;
		protected int			m_Timeout;
		protected int			_port;

		int		m_Protocol;
		String	m_ServerVersion;
		int		m_ThreadID;
		String	m_EncryptionSeed;
		int		m_ServerCaps;
		bool	m_UseCompression = false;


		public Driver(int ConnectionTimeout)
		{
			m_Seq = -1;
			m_LastResult = 0xff;
			m_Timeout = ConnectionTimeout;
			m_BufIndex = 0;

			ResetPacket();
		}

		~Driver() 
		{
		}

		public byte LastResult 
		{
			get { return m_LastResult; }
		}

		public string ServerVersion 
		{
			get { return m_ServerVersion; }
		}

		public void Dispose() 
		{
		}

#if WINDOWS
		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		void CreatePipeStream( string host ) 
		{
			string _pipename;
			if (host.ToLower().Equals("localhost"))
				_pipename = @"\\.\pipe\MySQL";
			else
				_pipename = String.Format(@"\\{0}\pipe\MySQL", host);

			_stream = new ByteFX.Data.Common.NamedPipeStream(_pipename, FileAccess.ReadWrite);
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		void CreateSocketStream( string host, int port ) 
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPHostEntry he = Dns.GetHostByName(host);
			IPEndPoint _serverAddr = new IPEndPoint(he.AddressList[0], port);

			_socket.Connect(_serverAddr);
			_stream = new NetworkStream(_socket);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="userid"></param>
		/// <param name="password"></param>
		public void Open( String host, int port, String userid, String password, bool UseCompression ) 
		{
			_port = port;
#if WINDOWS
			if (-1 == port) 
			{
				CreatePipeStream(host);
			}
#endif
			
			if (-1 != port)
			{
				CreateSocketStream(host, port);
			}

			ReadPacket();

			// read off the protocol version
			m_Protocol = _packet.ReadByte();
			m_ServerVersion = ReadString();
			m_ThreadID = ReadInteger(4);
			m_EncryptionSeed = ReadString();

			// read in Server capabilities if they are provided
			m_ServerCaps = 0;
			if (_packet.CanRead)
				m_ServerCaps = ReadInteger(2);

			Authenticate( userid, password, UseCompression );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="password"></param>
		private void Authenticate( String userid, String password, bool UseCompression )
		{
			ClientParam clientParam = ClientParam.CLIENT_FOUND_ROWS | ClientParam.CLIENT_LONG_FLAG;

			if ((m_ServerCaps & (int)ClientParam.CLIENT_COMPRESS) != 0 && UseCompression)
			{
				clientParam |= ClientParam.CLIENT_COMPRESS;
			}

			clientParam |= ClientParam.CLIENT_LONG_PASSWORD;

			password = EncryptPassword(password, m_EncryptionSeed, m_Protocol > 9);
			// header_length = 4
			//int headerLength = (userid.Length + 16) + 6 + 4; // Passwords can be 16 chars long

			ResetPacket();
			WriteInteger( (int)clientParam, 2 );
			WriteInteger( 0, 3 ); 
			WriteString( userid );
			WriteString( password );
			WritePacket();

			CheckResult();

			if ((clientParam & ClientParam.CLIENT_COMPRESS) != 0)
				m_UseCompression = true;
		}

		public void ResetPacket()
		{
			_packet = new MemoryStream();
			_packet.SetLength(0);

			// hack for Mono 0.17 not handling length < position on MemoryStream
			_packet.Position = 0;
			WriteInteger(0, HEADER_LEN);

			if (m_UseCompression)
				_packet.Position += (COMPRESS_HEADER_LEN+HEADER_LEN);
		}

		protected bool CanReadStream()
		{
#if WINDOWS
			if (_port == -1)
			{
				return (_stream as ByteFX.Data.Common.NamedPipeStream).DataAvailable;
			}
#endif
			return (_stream as NetworkStream).DataAvailable;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public byte ReadStreamByte()
		{
			long start = DateTime.Now.Ticks;
			long timeout = m_Timeout * TimeSpan.TicksPerSecond;

			while ((DateTime.Now.Ticks - start) < timeout)
			{
				if (CanReadStream()) return (byte)_stream.ReadByte();
			}
			throw new MySQLException("Timeout waiting for response from server");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		protected void ReadStreamBytes(byte[] buf, int offset, int count)
		{
			long start = DateTime.Now.Ticks;
			long timeout = m_Timeout * TimeSpan.TicksPerSecond;
			long curoffset = offset;

			while (count > 0 && ((DateTime.Now.Ticks - start) < timeout))
			{
				if (CanReadStream()) 
				{
					int cnt = _stream.Read(buf, (int)curoffset, count);
					count -= cnt;
					curoffset += cnt;
				}
			}
			if (count > 0)
				throw new MySQLException("Timeout waiting for response from server");
		}

		/// <summary>
		/// 
		/// </summary>
		private void ReadServerDataBlock()
		{
			int b0 = (int)ReadStreamByte();
			int b1 = (int)ReadStreamByte();
			int b2 = (int)ReadStreamByte();

			if (b0 == -1 && b1 == -1 && b2 == -1) 
			{
				//TODO: close?
				throw new IOException("Unexpected end of input stream");
			}

			int packetLength = (int)(b0+ (256*b1) + (256*256*b2));
			int comp_len = 0;
			byte Seq = (byte)ReadStreamByte();
	
			// handle the stupid field swapping does if compression is used
			// If the block is compressed, then the first length field is the compressed
			// length and the second is the uncompressed.
			// If the block is uncompressed, even if compression is selected, the first
			// length field is the uncompressed size and the second field is zero
			if (m_UseCompression) 
			{
				int c0 = (int)ReadStreamByte();
				int c1 = (int)ReadStreamByte();
				int c2 = (int)ReadStreamByte();
				comp_len = (int)(c0 + (256*c1) + (256*256*c2));
				if (comp_len > 0) 
				{
					int temp = packetLength;
					packetLength = comp_len;
					comp_len = temp;
				}
			}

			if (m_UseCompression && comp_len > 0) 
			{
				m_Buffer = new Byte[packetLength];
				byte[] comp = new Byte[comp_len];
				// read in the compressed data
				ReadStreamBytes(comp, 0, comp_len);

				Inflater i = new Inflater();
				i.SetInput( comp );

				i.Inflate(m_Buffer);
				return;
			}

			if (!m_UseCompression) 
			{
				m_Buffer = new Byte[packetLength+4];
				ReadStreamBytes(m_Buffer, 4, packetLength);
				m_Buffer[0] = (byte)b0; m_Buffer[1] = (byte)b1; m_Buffer[2] = (byte)b2;
				m_Buffer[3] = (byte)Seq;
			}
			else 
			{
				m_Buffer = new Byte[packetLength];
				ReadStreamBytes(m_Buffer, 0, packetLength);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int ReadPacket()
		{
			if (_packet == null || m_Buffer == null || m_BufIndex == m_Buffer.Length)
			{
				ReadServerDataBlock();
				m_BufIndex = 0;
			}

			_packet = new MemoryStream(m_Buffer, m_BufIndex, m_Buffer.Length - m_BufIndex);
			int len = ReadInteger(3);
			int seq = (int)ReadByte();
			_packet.SetLength(len+HEADER_LEN);
			m_BufIndex += (int)_packet.Length;

			// if the sequence doesn't match up, then there must be some orphaned
			// packets so we just read them off
			if (seq != (m_Seq+1)) return ReadPacket();
			
			m_Seq = seq;
			return len;
}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected int CompressPacket()
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="useCompressionIfAvail"></param>
		protected void WritePacket()
		{
			if (m_UseCompression)
			{
				// store the length of the buffer we are going to compress
				long num_bytes = _packet.Length - (HEADER_LEN*2) - COMPRESS_HEADER_LEN;
				_packet.Position = HEADER_LEN + COMPRESS_HEADER_LEN;
				WriteInteger( (int) num_bytes, 3 );
				_packet.WriteByte(0);				// internal packet has 0 as seq if compressing

				// now compress it
				int compressed_size = CompressPacket();

				_packet.Position = 0;
				if (compressed_size == 0) 
				{
					WriteInteger( (int)num_bytes + HEADER_LEN, 3);
					_packet.WriteByte((byte)++m_Seq);
					WriteInteger( compressed_size, 3 );
				}
				else 
				{
					WriteInteger( compressed_size, 3 );
					_packet.WriteByte((byte)++m_Seq);
					WriteInteger( (int)num_bytes + HEADER_LEN, 3);
				}
			}
			else 
			{
				_packet.Position = 0;
				WriteInteger( (int)(_packet.Length - HEADER_LEN), 3 );
				_packet.WriteByte((byte)++m_Seq);
			}

			_stream.Write( _packet.ToArray(), 0, (int)_packet.Length );
			_stream.Flush();

			// reset the writeStream to empty
			ResetPacket();
		}


		protected void WriteString(string v)
		{
			WriteStringNoNull(v);
			_packet.WriteByte(0);
		}

		protected void WriteStringNoNull(string v)
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(v);
			_packet.Write(bytes, 0, bytes.Length);
		}

		public void Close() 
		{
			m_Seq = -1;
			_stream.Close();
			if (_socket != null)
				_socket.Close();
		}

		public string ReadString()
		{
			String str = new String('c',0);

			while (_packet.Position < _packet.Length) 
			{
				byte b = (byte)_packet.ReadByte();
				if (b == 0) break;
				str += Convert.ToChar(b);
			}
			return str;
		}

		protected void WriteInteger( int v, int numbytes )
		{
			int val = v;

			if (numbytes < 1 || numbytes > 4) 
				throw new Exception("Wrong byte count for WriteInteger");

			for (int x=0; x < numbytes; x++)
			{
				_packet.WriteByte( (byte)(val&0xff) );
				val >>= 8;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numbytes"></param>
		/// <returns></returns>
		public int ReadInteger(int numbytes)
		{
			int val = 0;
			int raise = 1;
			for (int x=0; x < numbytes; x++)
			{
				int b = (int)_packet.ReadByte();
				val += (b*raise);
				raise *= 256;
			}
			return val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int ReadLength()
		{
			byte c  = (byte)_packet.ReadByte();
			switch(c) 
			{
				case 251 : return (int) 0; 
				case 252 : return ReadInteger(2);
				case 253 : return ReadInteger(3);
				case 254 : return ReadInteger(4);
				default  : return (int) c;
			}
		}

		public byte ReadByte()
		{
			return (byte)_packet.ReadByte();
		}

		public int ReadNBytes()
		{
			byte c = (byte)_packet.ReadByte();
			if (c < 1 || c > 4) throw new MySQLException("Unexpected byte count received");
			return ReadInteger((int)c);
		}

		public string ReadLenString()
		{
			int len = ReadLength();

			byte[] buf = new Byte[len];
			_packet.Read(buf, 0, len);

			String s = new String('c', 0);
			for (int x=0; x < buf.Length; x++)
				s += Convert.ToChar(buf[x]);
			return s;
		}


		void CheckResult()
		{
			ReadPacket();

			m_LastResult = (byte)_packet.ReadByte();

			if (0xff == m_LastResult) 
			{
				int errno = ReadInteger(2);
				string msg = ReadString();
				throw new MySQLException(msg, errno);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsLastPacketSignal() 
		{
			byte b = (byte)_packet.ReadByte();
			_packet.Position--;

			if ((_packet.Length - HEADER_LEN) == 1 && b == 0xfe)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Read the byte data from the server for the next column
		/// </summary>
		/// <returns></returns>
		public byte[] ReadColumnData()
		{
			int		len;

			byte c = (byte)_packet.ReadByte(); 

			switch (c)
			{
				case 251:  return null; //new byte[1] { c }; 
				case 252:  len = ReadInteger(2); break;
				case 253:  len = ReadInteger(3); break;
				case 254:  len = ReadInteger(4); break;
				default:   len = c; break;
			}

			byte[] buf = new Byte[len];
			_packet.Read(buf, 0, len);
			return buf;
		}


		/// <summary>
		/// Sends the specified command to the database
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <param name="text">Text attribute of command</param>
		/// <returns>Result packet returned from database server</returns>
		public void SendCommand( DBCmd command, String text ) 
		{
			m_Seq = -1;
			ResetPacket();

			_packet.WriteByte( (byte)command );

			if (text != null && text.Length > 0)
				WriteStringNoNull(text);

			try 
			{
				WritePacket();

				if (command != DBCmd.QUIT)
					CheckResult();
			}
			catch (Exception e) 
			{
				throw e;
			}
		}

		public void SendQuery( byte[] sql )
		{
			try 
			{
				m_Seq = -1;
				ResetPacket();

				_packet.WriteByte( (byte)DBCmd.QUERY );
				_packet.Write( sql, 0, sql.Length );

				WritePacket();
				CheckResult();
			}
			catch (Exception e) 
			{
				throw e;
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
