//
// generic ChannelFactory_1.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	// I really dislike those TChannels which are rather contract.
	// If I were to design WCF, I'd rather name them as TContract.
	public class ChannelFactory<TChannel>
		: ChannelFactory, IChannelFactory<TChannel>
	{
		public ChannelFactory ()
			: this ("*")
		{
		}

		protected ChannelFactory (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (!type.IsInterface)
				throw new InvalidOperationException ("The type argument to the generic ChannelFactory constructor must be an interface type.");

			InitializeEndpoint (CreateDescription ());
		}

		public ChannelFactory (string endpointConfigurationName)
		{
			if (endpointConfigurationName == null)
				throw new ArgumentNullException ("endpointConfigurationName");

			InitializeEndpoint (endpointConfigurationName, null);
		}

		public ChannelFactory (string endpointConfigurationName,
			EndpointAddress remoteAddress)
		{
			if (endpointConfigurationName == null)
				throw new ArgumentNullException ("endpointConfigurationName");

			InitializeEndpoint (endpointConfigurationName, remoteAddress);
		}

		public ChannelFactory (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("serviceEndpoint");

			InitializeEndpoint (endpoint);
		}

		public ChannelFactory (Binding binding, string remoteAddress)
			: this (binding, new EndpointAddress (remoteAddress))
		{
		}

		public ChannelFactory (Binding binding)
			: this (binding, (EndpointAddress) null)
		{
		}

		public ChannelFactory (Binding binding, EndpointAddress remoteAddress)
			: this (typeof (TChannel))
		{
			if (binding == null)
				throw new ArgumentNullException ();

			Endpoint.Binding = binding;
			Endpoint.Address = remoteAddress;
		}

		[MonoTODO]
		public TChannel CreateChannel ()
		{
			return CreateChannel (Endpoint.Address);
		}

		public TChannel CreateChannel (EndpointAddress address)
		{
			return CreateChannel (address, null);
		}

		public static TChannel CreateChannel (Binding binding, EndpointAddress address)
		{
			return new ChannelFactory<TChannel> (binding, address).CreateChannel ();
		}

		public static TChannel CreateChannel (Binding binding, EndpointAddress address, Uri via)
		{
			return new ChannelFactory<TChannel> (binding).CreateChannel (address, via);
		}

		[MonoTODO]
		public virtual TChannel CreateChannel (EndpointAddress address, Uri via)
		{
			EnsureOpened ();
			Type type = ClientProxyGenerator.CreateProxyType (Endpoint.Contract);
			object proxy = Activator.CreateInstance (type,
				new object [] {CreateRuntime (Endpoint), this});
			return (TChannel) proxy;
		}

		protected static TChannel CreateChannel (string endpointConfigurationName)
		{
			return new ChannelFactory<TChannel> (endpointConfigurationName).CreateChannel ();
		}

		protected override ServiceEndpoint CreateDescription ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (TChannel));
			ServiceEndpoint ep = new ServiceEndpoint (cd);
#if !NET_2_1
			ep.Behaviors.Add (new ClientCredentials ());
#endif
			return ep;
		}

		static ClientRuntime CreateRuntime (ServiceEndpoint se)
		{
			ClientRuntime proxy = new ClientRuntime (se);
			proxy.ContractClientType = typeof (TChannel);

			foreach (OperationDescription od in se.Contract.Operations)
				if (!proxy.Operations.Contains (od.Name))
					PopulateClientOperation (proxy, od);

#if !NET_2_1
			foreach (IEndpointBehavior b in se.Behaviors)
				b.ApplyClientBehavior (se, proxy);

			foreach (IContractBehavior b in se.Contract.Behaviors)
				b.ApplyClientBehavior (se.Contract, se, proxy);
			foreach (OperationDescription od in se.Contract.Operations)
				foreach (IOperationBehavior ob in od.Behaviors)
					ob.ApplyClientBehavior (od, proxy.Operations [od.Name]);
#endif

			return proxy;
		}

		static void PopulateClientOperation (ClientRuntime proxy, OperationDescription od)
		{
			string reqA = null, resA = null;
			foreach (MessageDescription m in od.Messages) {
				if (m.Direction == MessageDirection.Input)
					reqA = m.Action;
				else
					resA = m.Action;
			}
			ClientOperation o =
				od.IsOneWay ?
				new ClientOperation (proxy, od.Name, reqA) :
				new ClientOperation (proxy, od.Name, reqA, resA);
			foreach (MessageDescription md in od.Messages) {
				if (md.Direction == MessageDirection.Input &&
				    md.Body.Parts.Count == 1 &&
				    md.Body.Parts [0].Type == typeof (Message))
					o.SerializeRequest = false;
				if (md.Direction == MessageDirection.Output &&
				    md.Body.ReturnValue != null &&
				    md.Body.ReturnValue.Type == typeof (Message))
					o.DeserializeReply = false;
			}
			proxy.Operations.Add (o);
		}
	}
}
