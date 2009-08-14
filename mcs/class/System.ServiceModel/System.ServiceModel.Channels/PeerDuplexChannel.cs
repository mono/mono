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
using System.ServiceModel.PeerResolvers;
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
		enum RemotePeerStatus
		{
			None,
			Connected,
			Error,
		}

		class RemotePeerConnection
		{
			public RemotePeerConnection (PeerNodeAddress address)
			{
				Address = address;
			}

			public PeerNodeAddress Address { get; private set; }
			public RemotePeerStatus Status { get; set; }
			public IPeerConnectorClient Channel { get; set; }
		}

		class LocalPeerReceiver : IPeerReceiverContract
		{
			public LocalPeerReceiver (PeerDuplexChannel owner)
			{
				this.owner = owner;
			}

			PeerDuplexChannel owner;

			public void Connect (ConnectInfo connect)
			{
				if (connect == null)
					throw new ArgumentNullException ("connect");
try {
				var ch = OperationContext.Current.GetCallbackChannel<IPeerConnectorContract> ();
				// FIXME: check and reject if inappropriate.
				ch.Welcome (new WelcomeInfo () { NodeId = connect.NodeId });

} catch (Exception ex) {
Console.WriteLine ("Exception during Connect()");
Console.WriteLine (ex);
throw;
}

			}

			public void Welcome (WelcomeInfo welcome)
			{
			}

			public void Refuse (RefuseInfo refuse)
			{
			}

			public void SendMessage (Message msg)
			{
				owner.EnqueueMessage (msg);
			}
		}

		interface IPeerConnectorClient : IClientChannel, IPeerConnectorContract
		{
		}

		IChannelFactory<IDuplexSessionChannel> client_factory;
		ChannelFactory<IPeerConnectorClient> channel_factory;
		PeerTransportBindingElement binding;
		PeerResolver resolver;
		PeerNode node;
		ServiceHost listener_host;
		TcpChannelInfo info;
		List<RemotePeerConnection> peers = new List<RemotePeerConnection> ();

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

		IPeerConnectorClient CreateInnerClient (PeerNodeAddress pna)
		{
			// FIXME: pass more setup parameters
			if (channel_factory == null) {
				var binding = new NetTcpBinding ();
				binding.Security.Mode = SecurityMode.None;
				channel_factory = new ChannelFactory<IPeerConnectorClient> (binding);
			}

			return channel_factory.CreateChannel (new EndpointAddress ("net.p2p://" + node.MeshId), pna.EndpointAddress.Uri);
		}

		public override void Send (Message message, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			DateTime start = DateTime.Now;
			
			foreach (var pc in peers) {
				if (pc.Status == RemotePeerStatus.None) {
					var inner = CreateInnerClient (pc.Address);
					pc.Channel = inner;
					inner.Open (timeout - (DateTime.Now - start));
					inner.OperationTimeout = timeout - (DateTime.Now - start);
					inner.Connect (new ConnectInfo () { PeerNodeAddress = pc.Address, NodeId = (uint) node.NodeId });

					// FIXME: wait for Welcome or Reject and take further action.
					throw new NotImplementedException ();
				}

				pc.Channel.OperationTimeout = timeout - (DateTime.Now - start);
				pc.Channel.SendMessage (message);
			}
		}

		internal void EnqueueMessage (Message message)
		{
Console.WriteLine ("###########################");
var mb = message.CreateBufferedCopy (0x10000);
Console.WriteLine (mb.CreateMessage ());
message = mb.CreateMessage ();
			queue.Enqueue (message);
			receive_handle.Set ();
		}

		Queue<Message> queue = new Queue<Message> ();
		AutoResetEvent receive_handle = new AutoResetEvent (false);

		public override Message Receive (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();
			DateTime start = DateTime.Now;

			if (queue.Count > 0)
				return queue.Dequeue ();
			receive_handle.WaitOne ();
			return queue.Dequeue ();
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
			if (listener_host != null)
				listener_host.Close (timeout - (DateTime.Now - start));
			node.RegisteredId = null;
		}


		protected override void OnOpen (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;

			// FIXME: supply maxAddresses
			foreach (var a in resolver.Resolve (node.MeshId, 3, timeout))
				peers.Add (new RemotePeerConnection (a));

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

			var peer_receiver = new LocalPeerReceiver (this);
			listener_host = new ServiceHost (peer_receiver);
			var sba = listener_host.Description.Behaviors.Find<ServiceBehaviorAttribute> ();
			sba.InstanceContextMode = InstanceContextMode.Single;
			sba.IncludeExceptionDetailInFaults = true;

			var se = listener_host.AddServiceEndpoint (typeof (IPeerReceiverContract), binding, "net.p2p://" + node.MeshId);
			se.ListenUri = uri;
			listener_host.Open (timeout - (DateTime.Now - start));

			var nid = new Random ().Next (0, int.MaxValue);
			var ea = new EndpointAddress (uri);
			var pna = new PeerNodeAddress (ea, new ReadOnlyCollection<IPAddress> (Dns.GetHostEntry (name).AddressList));
			node.RegisteredId = resolver.Register (node.MeshId, pna, timeout - (DateTime.Now - start));
			node.NodeId = nid;

			// Add itself to the local list as well.
			// FIXME: it might become unnecessary once it implemented new node registration from peer resolver service.
			peers.Add (new RemotePeerConnection (pna));

			node.SetOnline ();
		}
	}
}
