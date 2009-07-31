//
// PeerDuplexChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Threading;

namespace System.ServiceModel.Channels
{

	// PeerDuplexChannel can be created either from PeerChannelFactory
	// (as IOutputChannel) or PeerChannelListener (as IInputChannel).
	//
	// PeerNode has to be created before Open() (at least at client side).
	// On open, it tries to resolve the nodes in the mesh (and do something
	// - but what?). Then registers itself to the mesh and refreshes it.

	internal class PeerDuplexChannel : DuplexChannelBase
	{
		IChannelFactory<IDuplexSessionChannel> client_factory;
		PeerTransportBindingElement binding;
		PeerResolver resolver;
		PeerNode node;
		IChannelListener<IDuplexSessionChannel> channel_listener;
		TcpChannelInfo info;
		List<PeerNodeAddress> peers = new List<PeerNodeAddress> ();

		public PeerDuplexChannel (IPeerChannelManager factory, EndpointAddress address, Uri via, PeerResolver resolver)
			: base ((ChannelFactoryBase) factory, address, via)
		{
			binding = factory.Source;
			this.resolver = factory.Resolver;
			info = new TcpChannelInfo (binding, factory.MessageEncoder, null); // FIXME: fill properties correctly.

			// It could be opened even with empty list of PeerNodeAddresses.
			// So, do not create PeerNode per PeerNodeAddress, but do it with PeerNodeAddress[].
			node = new PeerNodeImpl (RemoteAddress.Uri.Host, factory.Source.ListenIPAddress, factory.Source.Port);
		}

		public PeerDuplexChannel (IPeerChannelManager listener)
			: base ((ChannelListenerBase) listener)
		{
			binding = listener.Source;
			this.resolver = listener.Resolver;
			info = new TcpChannelInfo (binding, listener.MessageEncoder, null); // FIXME: fill properties correctly.

			node = new PeerNodeImpl (((ChannelListenerBase) listener).Uri.Host, listener.Source.ListenIPAddress, listener.Source.Port);
		}

		public override T GetProperty<T> ()
		{
			if (typeof (T).IsInstanceOfType (node))
				return (T) (object) node;
			return base.GetProperty<T> ();
		}

		// DuplexChannelBase

		IDuplexSessionChannel CreateInnerClient (PeerNodeAddress pna)
		{
			// FIXME: pass more setup parameters
			if (client_factory == null) {
				var binding = new NetTcpBinding ();
				binding.Security.Mode = SecurityMode.None;
				client_factory = binding.BuildChannelFactory<IDuplexSessionChannel> (new object [0]);
				client_factory.Open ();
			}

			return client_factory.CreateChannel (pna.EndpointAddress, Via);
		}

		public override void Send (Message message, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			DateTime start = DateTime.Now;
			
			// rewriter message header to net.p2p endpoint.
			//
			// FIXME: I'm not sure rewriting it here is the right
			// answer. It is possible that it should be done on
			// creating internal tcp channel factory creating,
			// using Via uri.
			message.Headers.To = (RemoteAddress ?? LocalAddress).Uri;

			foreach (var pna in peers) {
				var inner = CreateInnerClient (pna);
				inner.Open (timeout - (DateTime.Now - start));
				inner.Send (message, timeout);
			}
		}

		public override Message Receive (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();
			DateTime start = DateTime.Now;

			var ch = channel_listener.AcceptChannel (timeout);
			ch.Open (timeout - (DateTime.Now - start));
			try {
				return ch.Receive (timeout - (DateTime.Now - start));
			} finally {
				ch.Close ();
			}
		}

		public override bool WaitForMessage (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			throw new NotImplementedException ();
		}
		
		// CommunicationObject
		
		protected override void OnAbort ()
		{
			if (client_factory != null) {
				client_factory.Abort ();
				client_factory = null;
			}
			OnClose (TimeSpan.Zero);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			if (client_factory != null)
				client_factory.Close (timeout - (DateTime.Now - start));
			peers.Clear ();
			resolver.Unregister (node.RegisteredId, timeout - (DateTime.Now - start));
			node.SetOffline ();
			if (channel_listener != null)
				channel_listener.Close (timeout - (DateTime.Now - start));
			node.RegisteredId = null;
		}


		protected override void OnOpen (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;

			// FIXME: supply maxAddresses
			peers.AddRange (resolver.Resolve (node.MeshId, 3, timeout));

			// FIXME: pass more configuration
			var binding = new NetTcpBinding ();
			binding.Security.Mode = SecurityMode.None;

			int port = 0;
			var rnd = new Random ();
			for (int i = 0; i < 1000; i++) {
				if (DateTime.Now - start > timeout)
					throw new TimeoutException ();
				try {
					port = rnd.Next (50000, 51000);
					var t = new TcpListener (port);
					t.Start ();
					t.Stop ();
					break;
				} catch (SocketException) {
					continue;
				}
			}

			string name = Dns.GetHostName ();
			var uri = new Uri ("net.tcp://" + name + ":" + port + "/PeerChannelEndpoints/" + Guid.NewGuid ());

			channel_listener = binding.BuildChannelListener<IDuplexSessionChannel> (uri, new object [0]);
			channel_listener.Open (timeout - (DateTime.Now - start));

			var nid = new Random ().Next (0, int.MaxValue);
			var ea = new EndpointAddress (uri);
			var pna = new PeerNodeAddress (ea, new ReadOnlyCollection<IPAddress> (Dns.GetHostEntry (name).AddressList));
			node.RegisteredId = resolver.Register (node.MeshId, pna, timeout - (DateTime.Now - start));
			node.NodeId = nid;

			// Add itself to the local list as well.
			// FIXME: it might become unnecessary once it implemented new node registration from peer resolver service.
			peers.Add (pna);

			node.SetOnline ();
		}
	}
}
