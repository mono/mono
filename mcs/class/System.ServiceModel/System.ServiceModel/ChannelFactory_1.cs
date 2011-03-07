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
using System.Reflection;
using System.Runtime.Remoting;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MonoInternal;

namespace System.ServiceModel
{
	// LAMESPEC: TChannel should have been defined as "where TChannel : IClientChannel".
	// The returned channel is actually used as IClientChannel.
	// (That's also likely why the type parameter name is TChannel, not TContract.)
	public class ChannelFactory<TChannel>
		: ChannelFactory, IChannelFactory<TChannel>
	{
		public ChannelFactory ()
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

		internal object OwnerClientBase { get; set; }

		public TChannel CreateChannel ()
		{
			EnsureOpened ();

			return CreateChannel (Endpoint.Address);
		}

		public TChannel CreateChannel (EndpointAddress address)
		{
			return CreateChannel (address, null);
		}

		static TChannel CreateChannelCore (ChannelFactory<TChannel> cf, Func<ChannelFactory<TChannel>, TChannel> f)
		{
			var ch = f (cf);
			((ICommunicationObject) (object) ch).Closed += delegate {
				if (cf.State == CommunicationState.Opened)
					cf.Close ();
			};
			return ch;
		}

		public static TChannel CreateChannel (Binding binding, EndpointAddress address)
		{
			return CreateChannelCore (new ChannelFactory<TChannel> (binding, address), f => f.CreateChannel ());
		}

		public static TChannel CreateChannel (Binding binding, EndpointAddress address, Uri via)
		{
			return CreateChannelCore (new ChannelFactory<TChannel> (binding), f => f.CreateChannel (address, via));
		}

		public virtual TChannel CreateChannel (EndpointAddress address, Uri via)
		{
#if MONOTOUCH
			throw new InvalidOperationException ("MonoTouch does not support dynamic proxy code generation. Override this method or its caller to return specific client proxy instance");
#else
			var existing = Endpoint.Address;
			try {

			Endpoint.Address = address;
			EnsureOpened ();
			Endpoint.Validate ();
#if DISABLE_REAL_PROXY
			Type type = ClientProxyGenerator.CreateProxyType (typeof (TChannel), Endpoint.Contract, false);
			// in .NET and SL2, it seems that the proxy is RealProxy.
			// But since there is no remoting in SL2 (and we have
			// no special magic), we have to use different approach
			// that should work either.
			var proxy = (IClientChannel) Activator.CreateInstance (type, new object [] {Endpoint, this, address ?? Endpoint.Address, via});
#else
			var proxy = (IClientChannel) new ClientRealProxy (typeof (TChannel), new ClientRuntimeChannel (Endpoint, this, address ?? Endpoint.Address, via), false).GetTransparentProxy ();
#endif
			proxy.Opened += delegate {
				OpenedChannels.Add (proxy);
			};
			proxy.Closing += delegate {
				OpenedChannels.Remove (proxy);
			};

			return (TChannel) proxy;
			} catch (TargetInvocationException ex) {
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;
			} finally {
				Endpoint.Address = existing;
			}
#endif
		}

		protected static TChannel CreateChannel (string endpointConfigurationName)
		{
			return CreateChannelCore (new ChannelFactory<TChannel> (endpointConfigurationName), f => f.CreateChannel ());
		}

		protected override ServiceEndpoint CreateDescription ()
		{
			ContractDescription cd = ContractDescription.GetContract (typeof (TChannel));
			ServiceEndpoint ep = new ServiceEndpoint (cd);
			ep.Behaviors.Add (new ClientCredentials ());
			return ep;
		}
	}

	class DummyClientBase<T> : ClientBase<T> where T : class
	{
		public DummyClientBase (ChannelFactory<T> factory)
			: base (factory)
		{
		}
	}
}
