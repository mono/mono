//
// PeerNode.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2009 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
	public abstract class PeerNode : IOnlineStatus
	{
		internal PeerNode (string meshId, int port)
		{
			MeshId = meshId;
			Port = port;
		}

		public event EventHandler Offline;
		public event EventHandler Online;

		public bool IsOnline { get; internal set; }

		internal string MeshId { get; private set; }

		internal int NodeId { get; set; }

		internal abstract bool IsOpen { get; }

		public int Port { get; private set; }

		public abstract PeerMessagePropagationFilter MessagePropagationFilter { get; set; }

		internal abstract void Open (TimeSpan timeout);
		internal abstract void Close (TimeSpan timeout);

		public void RefreshConnection ()
		{
		}

		public override string ToString ()
		{
			return String.Format ("MeshId: {0}, Node ID: {1}, Online: {2}, Opened:{3}, Port: {4}", MeshId, NodeId, IsOnline, IsOpen, Port);
		}

		internal void SetOnline ()
		{
			IsOnline = true;
			if (Online != null)
				Online (this, EventArgs.Empty);
		}

		internal void SetOffline ()
		{
			IsOnline = false;
			if (Offline != null)
				Offline (this, EventArgs.Empty);
		}
	}

	internal class PeerNodeImpl : PeerNode
	{
		class NodeInfo
		{
			public int Id { get; set; }
			public PeerNodeAddress Address { get; set; }
		}

		Dictionary<string,NodeInfo> mesh_map = new Dictionary<string,NodeInfo> ();

		internal PeerNodeImpl (PeerResolver resolver, EndpointAddress remoteAddress, int port)
			: base (remoteAddress.Uri.Host, port)
		{
			this.resolver = resolver;
			this.remote_address = remoteAddress;
		}

		PeerResolver resolver;
		EndpointAddress remote_address;
		object registered_id;
		TcpListener listener; // FIXME: not sure if it is actually used ...

		// FIXME: implement
		public override PeerMessagePropagationFilter MessagePropagationFilter { get; set; }

		internal override bool IsOpen {
			get { return registered_id != null; }
		}

		internal override void Open (TimeSpan timeout)
		{
			DateTime startTime = DateTime.Now;

			int maxAddresses = 3; // FIXME: get it from somewhere

			NodeInfo info;
			if (!mesh_map.TryGetValue (MeshId, out info)) {
				var rnd = new Random ();
				// FIXME: not sure how I should handle addresses
				foreach (var address in resolver.Resolve (MeshId, maxAddresses, timeout)) {
					info = new NodeInfo () { Id = rnd.Next (), Address = address };
					break;
				}
				if (info == null) { // there was no resolved IP for the MeshId, so create a new peer ...
					int p = rnd.Next (50000, 60000);
					while (p < 60000) {
						try {
							listener = new TcpListener (p);
							break;
						} catch {
						}
					}
					if (listener == null)
						throw new Exception ("No port is available for a peer node to listen");
					listener.Start ();
					var ep = (IPEndPoint) listener.LocalEndpoint;
					string name = Dns.GetHostName ();
					info = new NodeInfo () { Id = new Random ().Next (0, int.MaxValue), Address = new PeerNodeAddress (new EndpointAddress ("net.tcp://" + name + ":" + ep.Port + "/PeerChannelEndpoints/" + Guid.NewGuid ()), new ReadOnlyCollection<IPAddress> (Dns.GetHostEntry (name).AddressList)) };
				}
			}
			registered_id = resolver.Register (MeshId, info.Address, timeout - (DateTime.Now - startTime));
			mesh_map [MeshId] = info;
			NodeId = info.Id;
			SetOnline ();
		}

		internal override void Close (TimeSpan timeout)
		{
			resolver.Unregister (registered_id, timeout);
			if (listener != null)
				listener.Stop ();
			registered_id = null;
		}
	}
}
