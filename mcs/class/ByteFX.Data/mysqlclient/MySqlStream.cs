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
using System.IO;
using System.Net;
using System.Net.Sockets;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for API.
	/// </summary>
	internal class MySqlStream : Stream
	{
		Stream	stream;
		Socket	socket;
		int		timeOut;

		public MySqlStream( string host, int port, int timeout )
		{
			if (port == -1)
				Create( host );
			else
				Create( host, port );
			timeOut = timeout;
		}

		private void Create( string host, int port )
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPHostEntry he = Dns.GetHostByName( host );
			IPEndPoint serverAddr = new IPEndPoint(he.AddressList[0], port);

			socket.Connect(serverAddr);
			stream = new NetworkStream(socket, true);
		}

		private void Create( string host )
		{
			string pipeName;

			if (host.ToLower().Equals("localhost"))
				pipeName = @"\\.\pipe\MySql";
			else
				pipeName = String.Format(@"\\{0}\pipe\MySql", host);

			stream = new ByteFX.Data.Common.NamedPipeStream(pipeName, FileAccess.ReadWrite);
		}

		public bool DataAvailable
		{
			get 
			{
				if (stream is NetworkStream)
					return ((NetworkStream)stream).DataAvailable;
				else return (stream as NamedPipeStream).DataAvailable;
			}
	}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position 
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override void Flush() 
		{
			stream.Flush();
		}

		public override int ReadByte()
		{
			long start = Environment.TickCount;
			long timeout_ticks = timeOut * TimeSpan.TicksPerSecond;

			while (((Environment.TickCount - start) < timeout_ticks))
			{
				if (DataAvailable)
				{
					int b = stream.ReadByte();
					return b;
				}
			}
			throw new Exception("Timeout waiting for response from server");
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			long start = Environment.TickCount;
			int  numToRead = count;
			long timeout_ticks = timeOut * TimeSpan.TicksPerSecond;

			while (numToRead > 0 && ((Environment.TickCount - start) < timeout_ticks))
			{
				if (DataAvailable)
				{
					int bytes_read = stream.Read(buffer, offset, numToRead);
					offset += bytes_read;
					numToRead -= bytes_read;
				}
			}
			if (numToRead > 0)
				throw new Exception("Timeout waiting for response from server");
			return count;
		}

		public int ReadInt24()
		{
			byte[] bytes = new byte[3];
			Read( bytes, 0, 3 );
			return (bytes[0] + (bytes[1]*256) + (bytes[2]*256*256));
		}

		public override void Close()
		{
			stream.Close();
		}

		public override void SetLength(long length)
		{
			stream.SetLength( length );
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write( buffer, offset, count );
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			return stream.Seek( offset, origin );
		}
	}
}


