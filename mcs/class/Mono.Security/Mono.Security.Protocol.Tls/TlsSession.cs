/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

using Mono.Security.Protocol.Tls.Alerts;

namespace Mono.Security.Protocol.Tls
{
	public sealed class TlsSession
	{
		#region EVENTS

		public event TlsWarningAlertEventHandler WarningAlert;

		#endregion

		#region FIELDS
		
		private byte[]						sessionId;
		private TlsSessionContext			context;
		private bool						helloDone;
		private	bool						handshakeFinished;
		private TlsSessionSettings			settings;
		private TlsCipherSuiteCollection	supportedCiphers;
		private TlsSocket					socket;
		private TlsNetworkStream			networkStream;
		private bool						isSecure;
		private TlsSessionState				state;

		#endregion

		#region PROPERTIES

		public byte[] SessionId
		{
			get { return sessionId; }
		}

		public TlsNetworkStream NetworkStream
		{
			get { return networkStream; }
		}

		public TlsSessionState State
		{
			get { return state; }
		}

		#endregion

		#region INTERNAL_PROPERTIES

		internal TlsSessionContext Context
		{
			get { return context; }
		}

		internal TlsCipherSuiteCollection SupportedCiphers
		{
			get { return supportedCiphers; }
		}
				
		internal bool HelloDone
		{
			get { return helloDone; }
			set { helloDone = value; }
		}

		internal bool HandshakeFinished
		{
			get { return handshakeFinished; }
			set { handshakeFinished = value; }
		}

		internal bool IsSecure
		{
			get { return isSecure; }
			set { isSecure = value; }
		}

		internal TlsSessionSettings Settings
		{
			get { return settings; }
		}

		internal short MaxFragmentSize
		{
			get { return (short)Math.Pow(2, 14); }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsSession(TlsSessionSettings settings)
		{
			this.supportedCiphers	= TlsCipherSuiteCollection.GetSupportedCipherSuiteCollection();
			this.settings			= settings;
			this.context			= new TlsSessionContext();
			this.sessionId			= new byte[0];
						
			// Initialize socket for connection
			this.initializeSocket();
		}

		#endregion

		#region EXCEPTION_METHODS

		internal TlsException CreateException(TlsAlertLevel alertLevel, TlsAlertDescription alertDesc)
		{
			return CreateException(TlsAlert.GetAlertMessage(alertDesc));
		}

		internal TlsException CreateException(string format, params object[] args)
		{
			StringBuilder message = new StringBuilder();
			message.AppendFormat(format, args);

			return CreateException(message.ToString());
		}

		internal TlsException CreateException(string message)
		{
			this.state = TlsSessionState.Broken;

			// Throw an exception will made the connection unavailable
			// for this both streams will be closed
			closeStreams();

			return new TlsException(message);
		}

		#endregion

		#region METHODS

		public void Open()
		{
			try
			{
				this.context.Protocol	= settings.Protocol;
				this.state				= TlsSessionState.OpeningSecure;
				this.socket.DoHandshake();
			}
			catch (TlsException ex)
			{
				this.state = TlsSessionState.Broken;
				throw ex;
			}
			catch (Exception ex)
			{
				this.state = TlsSessionState.Broken;
				this.closeStreams();
				throw ex;
			}
		}

		public void Close()
		{
			try
			{
				this.state = TlsSessionState.Closing;

				if (isSecure)
				{
					TlsCloseNotifyAlert alert = new TlsCloseNotifyAlert(this);

					// Write close notify
					this.socket.SendAlert(alert);

					// Check that the session is finished by the client and by server
					if (!this.context.ConnectionEnd)
					{
						throw new TlsException("Invalid session termination");
					}
				}				
			}
			catch (Exception ex)
			{
				this.state = TlsSessionState.Broken;
				throw ex;
			}
			finally
			{
				// Close streams
				closeStreams();

				this.state = TlsSessionState.Closed;
			}
		}

		#endregion

		#region INTERNAL_METHODS

		internal void RaiseWarningAlert(TlsAlertLevel level, TlsAlertDescription description)
		{
			if (WarningAlert != null)
			{
				WarningAlert(this, new TlsWarningAlertEventArgs(level, description));
			}
		}

		internal void SetSessionId(byte[] sessionId)
		{
			this.sessionId = sessionId;
		}

		#endregion

		#region PRIVATE_METHODS

		private void initializeSocket()
		{
			try
			{
				this.state = TlsSessionState.Opening;

				// Initialize socket
				IPAddress	hostadd = Dns.Resolve(settings.ServerName).AddressList[0];
				IPEndPoint	EPhost	= new IPEndPoint(hostadd, settings.ServerPort);

				// Create the socket
				socket = new TlsSocket(
					this,
					AddressFamily.InterNetwork,
					SocketType.Stream,
					ProtocolType.IP);

				// Make the socket to connect to the Server
				socket.Connect(EPhost);					
				networkStream = new TlsNetworkStream(socket, true);

				this.state = TlsSessionState.Open;
			}
			catch (Exception ex)
			{
				this.state = TlsSessionState.Broken;
				throw ex;
			}			
		}

		private void closeStreams()
		{
			// Reset session state
			this.context.IsActual = false;

			// Close the socket and the networkStream
			this.networkStream.Close();

			// Reset session information
			this.isSecure			= false;
			this.helloDone			= false;
			this.handshakeFinished	= false;
			this.context			= new TlsSessionContext();
			this.sessionId			= new byte[0];			
		}

		#endregion
	}
}