// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
// Copyright (C) 2003  PT Cakram Datalingga Duaribu (http://www.cdl2000.com)
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
using System.Threading;
#if __MonoCS__ 
using Mono.Posix;
#endif

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for API.
	/// </summary>
	internal class MySqlStream : MultiHostStream
	{
		private Socket socket;

		public MySqlStream( string hostList, int port, int readTimeOut, int connectTimeOut ) :
			base( hostList, port, readTimeOut, connectTimeOut )

		{
		}

		protected override void Error(string msg)
		{
			throw new MySqlException( msg, baseException );
		}

		protected override void TimeOut(MultiHostStreamErrorType error) 
		{
			switch (error) 
			{
				case MultiHostStreamErrorType.Connecting:
					throw new MySqlException("Timed out creating a new MySqlConnection");
				case MultiHostStreamErrorType.Reading:
					throw new MySqlException("Timed out reading from MySql");
			}
		}

		protected override bool CreateStream( IPAddress ip, string hostname, int port )
		{
			if (port == -1)
				return CreatePipeStream(ip, hostname);
			else
				return CreateSocketStream(ip, port);
		}

		protected override bool CreateStream (string filename)
		{
			return CreateUnixSocketStream (filename);
		}

		private bool CreatePipeStream( IPAddress ip, string hostname )
		{
			string pipeName;

			if (hostname.ToLower().Equals("localhost"))
				pipeName = @"\\.\pipe\MySql";
			else
				pipeName = String.Format(@"\\{0}\pipe\MySql", ip.ToString());

			try 
			{
				stream = new NamedPipeStream(pipeName, FileAccess.ReadWrite);
				return true;
			}
			catch (Exception ex) 
			{
				baseException = ex;
				return false;
			}
		}

		private bool CreateSocketStream( IPAddress ip, int port )
		{
			socket = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);

			try
			{
				//
				// Lets try to connect
				IPEndPoint endPoint	= new IPEndPoint( ip, port);
				socket.Connect(endPoint);
				socket.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1 );
				stream = new NetworkStream( socket, true );
				socket.Blocking = false;
				return true;
			}
			catch (Exception ex)
			{
				baseException = ex;
				return false;
			}
		}

		private bool CreateUnixSocketStream(string socketName)
		{
#if __MonoCS__ && !WINDOWS
			Socket socket = new Socket (AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

			try
			{
				UnixEndPoint endPoint = new UnixEndPoint (socketName);
				socket.Connect (endPoint);
				stream = new NetworkStream (socket, true);
				return true;
			}
			catch (Exception ex)
			{
				baseException = ex;
				return false;
			}
#else
			baseException = new PlatformNotSupportedException ("This is not a Unix");
			return false;
#endif
		}

		protected override bool DataAvailable
		{
			get 
			{
				if (stream is NetworkStream)
					return ((NetworkStream)stream).DataAvailable;
				else return (stream as NamedPipeStream).DataAvailable;
			}
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

	}
}


