//
// Microsoft.Web.Services.Messaging.SoapTcpChannel.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Web.Services.Messaging
{

	public class SoapTcpChannel : SoapChannel
	{

		private bool _active = false;
		private bool _disposed = false;
		private AddressFamily _addrFam = AddressFamily.InterNetwork;
		private DateTime _lastActivity = DateTime.Now;
		private int _port = 0;
		private Socket _socket = null;
		private NetworkStream _stream = null;
		private Uri _destination = null;
		private string _hostname = null;

		public SoapTcpChannel (Socket sock, ISoapFormatter format) : base (format)
		{
			_socket = sock;
			_stream = new NetworkStream (sock, false);
			_active = true;

			IPEndPoint ep = sock.RemoteEndPoint as IPEndPoint;

			_destination = new Uri ("soap.tcp://" + ep.Address.ToString () + ":" + ep.Port);
		}

		public SoapTcpChannel (Uri uri, ISoapFormatter format) : base (format)
		{
			if(uri == null) {
				throw new ArgumentNullException ("to");
			}
			if(uri.Scheme != "soap.tcp") {
				throw new ArgumentException ("Invalid Scheme");
			}
			_hostname = uri.Host;
			_port = uri.Port < 0 ? 8081 : uri.Port;
		}

		public override void Close ()
		{
			lock(SyncRoot) {
				try {
					_active = false;
					if(_socket != null || !_socket.Connected) {
						_socket.Close ();
					}
				} catch {
					_socket = null;
				}
			}
		}
		
		public void Connect ()
		{
			if(_disposed) {
				throw new ObjectDisposedException (GetType ().FullName);
			}
			if(_active) {
				return;
			}
			lock(SyncRoot) {
				if(_active) {
					return;
				}
				IPHostEntry host = Dns.Resolve (_hostname);

				IPAddress[] ip_addrs = host.AddressList;

				Exception exception = null;

				for (int i = 0; i < ip_addrs.Length; i++) {
					IPAddress addy = ip_addrs[i];

					_addrFam = addy.AddressFamily;
					_socket = new Socket (_addrFam, SocketType.Stream, ProtocolType.Tcp);
					_active = false;

					try {
						Connect ( new IPEndPoint (addy, _port) );
						break;
					} catch (Exception e) {
						_socket.Close ();
						_socket = null;
						exception = e;
					}
				}

				if(_active == false) {
					if(exception != null) {
						throw exception;
					}

					throw new Exception ("Not Connected");
				}
				_stream = new NetworkStream (_socket, false);
			}
		}

		public void Connect (IPEndPoint endpoint)
		{
			if(_disposed) {
				throw new ObjectDisposedException (GetType ().FullName);
			}
			if(endpoint == null) {
				throw new ArgumentNullException ("endpoint");
			}

			_socket.Connect (endpoint);
			_active = true;
			UpdateLastActivity ();
		}


		~SoapTcpChannel ()
		{
			if(_active == false) {
				Close ();
				_disposed = true;
			}
		}


		public override SoapEnvelope Receive ()
		{
			if(!_active) {
				Connect ();
			}

			SoapEnvelope env = DeserializeMessage (_stream);

			if(env != null) {
				env.Context.Channel = this;
			}

			UpdateLastActivity ();
			
			return env;
		}

		public override void Send (SoapEnvelope env)
		{
			lock(SyncRoot) {
				if(!_active) {
					Connect ();
				}
				SerializeMessage (env, _stream);
				UpdateLastActivity ();
			}
		}

		public void UpdateLastActivity ()
		{
			lock (SyncRoot) {
				_lastActivity = DateTime.Now;
			}
		}

		public override bool Active {
			get { return _active; }
		}

		public Uri Destination {
			get { return _destination; }
		}

		public DateTime LastActivity {
			get { return _lastActivity; }
		}

		public override string Scheme {
			get { return "soap.tcp"; }
		}
		
	}

}
