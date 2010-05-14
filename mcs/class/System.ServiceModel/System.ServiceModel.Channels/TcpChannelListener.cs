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
		TcpListener tcp_listener;
		
		public TcpChannelListener (TcpTransportBindingElement source, BindingContext context)
			: base (context)
		{
			MessageEncoder encoder = null;
			XmlDictionaryReaderQuotas quotas = null;

			foreach (BindingElement be in context.Binding.Elements) {
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
		
		List<ManualResetEvent> accept_handles = new List<ManualResetEvent> ();
		List<TChannel> accepted_channels = new List<TChannel> ();

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;

			// Close channels that are incorrectly kept open first.
			var l = new List<TcpDuplexSessionChannel> ();
			foreach (var tch in accepted_channels) {
				var dch = tch as TcpDuplexSessionChannel;
				if (dch != null && dch.TcpClient != null && !dch.TcpClient.Connected)
					l.Add (dch);
			}
			foreach (var dch in l)
				dch.Close (timeout - (DateTime.Now - start));

			TcpClient client = AcceptTcpClient (timeout - (DateTime.Now - start));
			if (client == null)
				return null; // onclose

			TChannel ch;

			if (typeof (TChannel) == typeof (IDuplexSessionChannel))
				ch = (TChannel) (object) new TcpDuplexSessionChannel (this, info, client);
			else if (typeof (TChannel) == typeof (IReplyChannel))
				ch = (TChannel) (object) new TcpReplyChannel (this, info, client);
			else
				throw new InvalidOperationException (String.Format ("Channel type {0} is not supported.", typeof (TChannel).Name));

			((ChannelBase) (object) ch).Closed += delegate {
				accepted_channels.Remove (ch);
				};
			accepted_channels.Add (ch);

			return ch;
		}

		// TcpReplyChannel requires refreshed connection after each request processing.
		internal TcpClient AcceptTcpClient (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;

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

			// This may be optional though ...
			if (client != null) {
				foreach (var ch in accepted_channels) {
					var dch = ch as TcpDuplexSessionChannel;
					if (dch == null || dch.TcpClient == null && !dch.TcpClient.Connected)
						continue;
					if (((IPEndPoint) dch.TcpClient.Client.RemoteEndPoint).Equals (client.Client.RemoteEndPoint))
						// ... then it should be handled in another BeginTryReceive/EndTryReceive loop in ChannelDispatcher.
						return AcceptTcpClient (timeout - (DateTime.Now - start));
				}
			}

			return client;
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
			IPHostEntry entry = Dns.GetHostEntry (Uri.Host);
			
			if (entry.AddressList.Length ==0)
				throw new ArgumentException (String.Format ("Invalid listen URI: {0}", Uri));
			
			int explicitPort = Uri.Port;
			tcp_listener = new TcpListener (entry.AddressList [0], explicitPort <= 0 ? TcpTransportBindingElement.DefaultPort : explicitPort);
			tcp_listener.Start ();
		}
	}
}
