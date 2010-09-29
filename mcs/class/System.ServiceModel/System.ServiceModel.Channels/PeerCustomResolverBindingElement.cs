//
// PeerCustomResolverBindingElement.cs
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public class PeerCustomResolverBindingElement : PeerResolverBindingElement
	{
		public PeerCustomResolverBindingElement ()
		{
			settings = new PeerCustomResolverSettings ();
		}

		public PeerCustomResolverBindingElement (PeerCustomResolverBindingElement other)
			: base (other)
		{
			ReferralPolicy = other.ReferralPolicy;
			settings = other.settings.Clone ();
		}

		public PeerCustomResolverBindingElement (BindingContext context, PeerCustomResolverSettings settings)
			: this (settings)
		{
			this.context = context;
		}

		public PeerCustomResolverBindingElement (PeerCustomResolverSettings settings)
		{
			this.settings = settings;
		}

		BindingContext context;
		PeerCustomResolverSettings settings;

		public EndpointAddress Address {
			get { return settings.Address; }
			set { settings.Address = value; }
		}

		public Binding Binding {
			get { return settings.Binding; }
			set { settings.Binding = value; }
		}

		[MonoTODO]
		public override PeerReferralPolicy ReferralPolicy { get; set; }

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelFactory<TChannel> ();
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			var cf = context.BuildInnerChannelFactory<TChannel> ();
			var pcf = cf as PeerChannelFactory<TChannel>;
			if (pcf != null)
				pcf.Resolver = CreatePeerResolver ();
			return cf;
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			var cl = context.BuildInnerChannelListener<TChannel> ();
			var pcl = cl as PeerChannelListener<TChannel>;
			if (pcl != null)
				pcl.Resolver = CreatePeerResolver ();
			return cl;
		}

		public override BindingElement Clone ()
		{
			return new PeerCustomResolverBindingElement (this);
		}

		public override PeerResolver CreatePeerResolver ()
		{
			if (settings != null && settings.Resolver != null)
				return settings.Resolver;

			var se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IPeerResolverContract)), settings.Binding, settings.Address);
			return new PeerCustomResolver (se);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}

	internal interface ICustomPeerResolverClient : IPeerResolverContract, IClientChannel
	{
	}

	internal class PeerCustomResolver : PeerResolver
	{
		Guid client_id = Guid.NewGuid ();
		ICustomPeerResolverClient client;
		string preserved_mesh_id;

		public PeerCustomResolver (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			client = new ChannelFactory<ICustomPeerResolverClient> (endpoint).CreateChannel ();
		}

		public override bool CanShareReferrals {
			get { return false; }
		}

		public override object Register (string meshId,
			PeerNodeAddress nodeAddress, TimeSpan timeout)
		{
			if (String.IsNullOrEmpty (meshId))
				throw new ArgumentNullException ("meshId");
			if (nodeAddress == null)
				throw new ArgumentNullException ("nodeAddress");
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			client.OperationTimeout = timeout;
			preserved_mesh_id = meshId;
			return client.Register (new RegisterInfo (client_id, meshId, nodeAddress)).RegistrationId;
		}

		public override ReadOnlyCollection<PeerNodeAddress> Resolve (
			string meshId, int maxAddresses, TimeSpan timeout)
		{
			if (String.IsNullOrEmpty (meshId))
				throw new ArgumentNullException ("meshId");
			if (maxAddresses <= 0)
				throw new ArgumentOutOfRangeException ("maxAddresses must be positive integer");
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			client.OperationTimeout = timeout;
			return new ReadOnlyCollection<PeerNodeAddress> (client.Resolve (new ResolveInfo (client_id, meshId, maxAddresses)).Addresses ?? new PeerNodeAddress [0]);
		}

		public override void Unregister (object registrationId,
			TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			client.OperationTimeout = timeout;
			preserved_mesh_id = null;
			client.Unregister (new UnregisterInfo (preserved_mesh_id, (Guid) registrationId));
		}

		public override void Update (object registrationId,
			PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			client.OperationTimeout = timeout;
			client.Update (new UpdateInfo ((Guid) registrationId, client_id, preserved_mesh_id, updatedNodeAddress));
		}
	}
}
