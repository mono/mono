//
// Microsoft.Web.Services.Messaging.SoapTcpListener.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Web.Services.Messaging
{
	public class SoapTcpListener : TcpListener
	{

		private int _refs = 0;

		private delegate Socket AcceptSocket ();

		private AcceptSocket _acceptSocket;
	
		public SoapTcpListener (IPEndPoint endpoint) : base (endpoint)
		{
			if(Server == null) {
				Server.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
			}
		}

		public SoapTcpListener (IPAddress address, int port) : base (address, port)
		{
			if(Server == null) {
				Server.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
			}
		}

		public SoapTcpListener (int port) : base (port)
		{
			if(Server == null) {
				Server.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
			}
		}

		public void AddReference ()
		{
			_refs++;
		}

		public int ReleaseReference ()
		{
			return --_refs;
		}

		public IAsyncResult BeginAcceptSocket (AsyncCallback callback, object state)
		{
			if(_acceptSocket == null) {
				_acceptSocket = new AcceptSocket (base.AcceptSocket);
			}
			return _acceptSocket.BeginInvoke (callback, state);
		}

		public Socket EndAcceptSocket (IAsyncResult result)
		{
			return _acceptSocket.EndInvoke (result);
		}

		public bool IsListening {
			get { return Active; }
		}
	}
}
