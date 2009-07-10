// 
// TcpChannelListener.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class TcpChannelListener<TChannel> : InternalChannelListenerBase<TChannel> 
		where TChannel : class, IChannel
	{
		BindingContext context;
		TcpChannelInfo info;
		IDuplexSession session;
		Uri listen_uri;
		TcpListener tcp_listener;
		
		public TcpChannelListener (TcpTransportBindingElement source, 
		                           BindingContext context) : base (context.Binding)
		{
			MessageEncoder encoder = null;
			XmlDictionaryReaderQuotas quotas = null;

			if (context.ListenUriMode == ListenUriMode.Explicit)
				listen_uri =
					context.ListenUriRelativeAddress != null ?
					new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress) :
					context.ListenUriBaseAddress;
			else
				throw new NotImplementedException ();
			
			foreach (BindingElement be in context.RemainingBindingElements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					encoder = CreateEncoder<TChannel> (mbe);
					quotas = mbe.GetProperty<XmlDictionaryReaderQuotas> (context);
					break;
				}
			}
			
			if (encoder == null)
				encoder = new BinaryMessageEncoder ();

			info = new TcpChannelInfo (source, encoder, quotas);
		}
		
		public override Uri Uri {
			get { return listen_uri; }
		}

		List<ManualResetEvent> accept_handles = new List<ManualResetEvent> ();

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			TcpClient client = null;
			if (tcp_listener.Pending ()) {
				client = tcp_listener.AcceptTcpClient ();
			} else {
				var wait = new ManualResetEvent (false);
				tcp_listener.BeginAcceptTcpClient (delegate (IAsyncResult result) {
					client = tcp_listener.EndAcceptTcpClient (result);
					wait.Set ();
					accept_handles.Remove (wait);
				}, null);
				if (State == CommunicationState.Closing)
					return null;
				accept_handles.Add (wait);
				wait.WaitOne (timeout);
			}
			if (client == null)
				return null; // onclose

			if (typeof (TChannel) == typeof (IDuplexSessionChannel))
				return (TChannel) (object) new TcpDuplexSessionChannel (this, info, client);

			// FIXME: To implement more.
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		// CommunicationObject
		
		protected override void OnAbort ()
		{
			if (State == CommunicationState.Closed)
				return;
			ProcessClose ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (State == CommunicationState.Closed)
				return;
			ProcessClose ();
		}

		void ProcessClose ()
		{
			if (tcp_listener == null)
				throw new InvalidOperationException ("Current state is " + State);
			lock (accept_handles) {
				foreach (var wait in accept_handles)
					wait.Set ();
			}
			tcp_listener.Stop ();
			tcp_listener = null;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			IPHostEntry entry = Dns.GetHostEntry (listen_uri.Host);
			
			if (entry.AddressList.Length ==0)
				throw new ArgumentException (String.Format ("Invalid listen URI: {0}", listen_uri));
			
			int explicitPort = listen_uri.Port;
			tcp_listener = new TcpListener (entry.AddressList [0], explicitPort <= 0 ? TcpTransportBindingElement.DefaultPort : explicitPort);
			tcp_listener.Start ();
		}
	}
}
