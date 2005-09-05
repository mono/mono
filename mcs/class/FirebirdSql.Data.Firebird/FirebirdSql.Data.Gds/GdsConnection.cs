/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Gds
{
	internal sealed class GdsConnection
	{
		#region Fields

		private Socket			socket;
		private NetworkStream	networkStream;
		private XdrStream		send;
		private XdrStream		receive;
		private int				operation;

		#endregion

		#region Internal Properties

		internal XdrStream Receive
		{
			get { return this.receive; }
		}

		internal XdrStream Send
		{
			get { return this.send; }
		}

		#endregion

		#region Constructors

		public GdsConnection()
		{
			this.operation = -1;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region Methods

		public void Connect(string dataSource, int port)
		{
			this.Connect(dataSource, port, 8192, Charset.DefaultCharset);
		}

		public void Connect(string dataSource, int port, int packetSize, Charset charset)
		{
			try
			{
				IPAddress hostadd = Dns.Resolve(dataSource).AddressList[0];
				IPEndPoint EPhost = new IPEndPoint(hostadd, port);

				this.socket = new Socket(
					AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.Tcp);

#if (!NETCF)

				// Set Receive Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer,
					packetSize);

				// Set Send	Buffer size.
				this.socket.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer,
					packetSize);
#endif

				// Disables	the	Nagle algorithm	for	send coalescing.
				this.socket.SetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay,
					1);

				// Make	the	socket to connect to the Server
				this.socket.Connect(EPhost);
				this.networkStream = new NetworkStream(this.socket, true);

#if	(NETCF)
				this.send	 = new XdrStream(this.networkStream, charset);
				this.receive = new XdrStream(this.networkStream, charset);
#else
				this.send = new XdrStream(new BufferedStream(this.networkStream), charset);
				this.receive = new XdrStream(new BufferedStream(this.networkStream), charset);
#endif

				GC.SuppressFinalize(this.socket);
				GC.SuppressFinalize(this.networkStream);
				GC.SuppressFinalize(this.send);
				GC.SuppressFinalize(this.receive);
			}
			catch (SocketException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_network_error, dataSource);
			}
		}

		public void Disconnect()
		{
			try
			{
				if (this.receive != null)
				{
					this.receive.Close();
				}
				if (this.send != null)
				{
					this.send.Close();
				}
				if (this.networkStream != null)
				{
					this.networkStream.Close();
				}
				if (this.socket != null)
				{
					this.socket.Close();
				}

				this.receive		= null;
				this.send			= null;
				this.socket			= null;
				this.networkStream	= null;
			}
			catch (IOException)
			{
				throw;
			}
		}

		#endregion

		#region Internal Methods

		internal int ReadOperation()
		{
			int op = (this.operation >= 0) ? this.operation : this.NextOperation();
			this.operation = -1;

			return op;
		}

		internal int NextOperation()
		{
			do
			{
				/* loop	as long	as we are receiving	dummy packets, just
				 * throwing	them away--note	that if	we are a server	we won't
				 * be receiving	them, but it is	better to check	for	them at
				 * this	level rather than try to catch them	in all places where
				 * this	routine	is called 
				 */
				this.operation = this.receive.ReadInt32();
			} while (this.operation == IscCodes.op_dummy);

			return this.operation;
		}

		internal GdsResponse ReadGenericResponse()
		{
			try
			{
				if (this.ReadOperation() == IscCodes.op_response)
				{
					GdsResponse r = new GdsResponse(
						this.receive.ReadInt32(),
						this.receive.ReadInt64(),
						this.receive.ReadBuffer());

					r.Warning = this.ReadStatusVector();

					return r;
				}
				else
				{
					return null;
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_net_read_err);
			}
		}

		internal IscException ReadStatusVector()
		{
			IscException exception = null;
			bool eof = false;

			try
			{
				while (!eof)
				{
					int arg = this.receive.ReadInt32();

					switch (arg)
					{
						case IscCodes.isc_arg_gds:
							int er = this.receive.ReadInt32();
							if (er != 0)
							{
								if (exception == null)
								{
									exception = new IscException();
								}
								exception.Errors.Add(arg, er);
							}
							break;

						case IscCodes.isc_arg_end:
							if (exception != null && exception.Errors.Count != 0)
							{
								exception.BuildExceptionMessage();
							}
							eof = true;
							break;

						case IscCodes.isc_arg_interpreted:
						case IscCodes.isc_arg_string:
							exception.Errors.Add(arg, this.receive.ReadString());
							break;

						case IscCodes.isc_arg_number:
							exception.Errors.Add(arg, this.receive.ReadInt32());
							break;

						default:
							{
								int e = this.receive.ReadInt32();
								if (e != 0)
								{
									if (exception == null)
									{
										exception = new IscException();
									}
									exception.Errors.Add(arg, e);
								}
							}
							break;
					}
				}
			}
			catch (IOException)
			{
				throw new IscException(IscCodes.isc_arg_gds, IscCodes.isc_net_read_err);
			}

			if (exception != null && !exception.IsWarning)
			{
				throw exception;
			}

			return exception;
		}

		internal void SetOperation(int operation)
		{
			this.operation = operation;
		}

		#endregion
	}
}
