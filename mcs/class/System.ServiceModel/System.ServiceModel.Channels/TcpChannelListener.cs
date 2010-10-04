// 
// TcpChannelListener.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
//     Atsushi Enomoto  (atsushi@ximian.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// Copyright 2009-2010 Novell, Inc (http://www.novell.com/)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
			XmlDictionaryReaderQuotas quotas = null;

			foreach (BindingElement be in context.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					quotas = mbe.GetProperty<XmlDictionaryReaderQuotas> (context);
					break;
				}
			}
			
			if (MessageEncoder == null)
				MessageEncoder = new BinaryMessageEncoder ();

			info = new TcpChannelInfo (source, MessageEncoder, quotas);
		}
		
		SynchronizedCollection<ManualResetEvent> accept_handles = new SynchronizedCollection<ManualResetEvent> ();
		Queue<TcpClient> accepted_clients = new Queue<TcpClient> ();
		SynchronizedCollection<TChannel> accepted_channels = new SynchronizedCollection<TChannel> ();

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

			TcpClient client = accepted_clients.Count == 0 ? null : accepted_clients.Dequeue ();
			if (client == null) {
				var wait = new ManualResetEvent (false);
				accept_handles.Add (wait);
				if (!wait.WaitOne (timeout)) {
					accept_handles.Remove (wait);
					return null;
				}
				accept_handles.Remove (wait);
				// recurse with new timeout, or return null if it's either being closed or timed out.
				timeout -= (DateTime.Now - start);
				return State == CommunicationState.Opened && timeout > TimeSpan.Zero ? AcceptTcpClient (timeout) : null;
			}

			// There might be bettwe way to exclude those TCP clients though ...
			foreach (var ch in accepted_channels) {
				var dch = ch as TcpDuplexSessionChannel;
				if (dch == null || dch.TcpClient == null && !dch.TcpClient.Connected)
					continue;
				if (((IPEndPoint) dch.TcpClient.Client.RemoteEndPoint).Equals (client.Client.RemoteEndPoint))
					// ... then it should be handled in another BeginTryReceive/EndTryReceive loop in ChannelDispatcher.
					return AcceptTcpClient (timeout - (DateTime.Now - start));
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
			ProcessClose (TimeSpan.Zero);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (State == CommunicationState.Closed)
				return;
			ProcessClose (timeout);
		}

		void ProcessClose (TimeSpan timeout)
		{
			if (tcp_listener == null)
				throw new InvalidOperationException ("Current state is " + State);
			//tcp_listener.Client.Close (Math.Max (50, (int) timeout.TotalMilliseconds));
			tcp_listener.Stop ();
			var l = new List<ManualResetEvent> (accept_handles);
			foreach (var wait in l) // those handles will disappear from accepted_handles
				wait.Set ();
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
			tcp_listener.BeginAcceptTcpClient (TcpListenerAcceptedClient, tcp_listener);
		}

		void TcpListenerAcceptedClient (IAsyncResult result)
		{
			var listener = (TcpListener) result.AsyncState;
			try {
				var client = listener.EndAcceptTcpClient (result);
				if (client != null) {
					accepted_clients.Enqueue (client);
					if (accept_handles.Count > 0)
						accept_handles [0].Set ();
				}
			} catch {
				/* If an accept fails, just ignore it. Maybe the remote peer disconnected already */
			} finally {
				if (State == CommunicationState.Opened) {
					try {
						listener.BeginAcceptTcpClient (TcpListenerAcceptedClient, listener);
					} catch {
						/* If this fails, we must have disposed the listener */
					}
				}
			}
		}
	}
}

