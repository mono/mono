//
// DuplexChannelFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marcos Cobena (marcoscobena@gmail.com)
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
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
using System.ServiceModel.MonoInternal;

namespace System.ServiceModel
{
	public class DuplexChannelFactory<TChannel> : ChannelFactory<TChannel>
	{
		InstanceContext callback_instance;
		Type callback_instance_type;

		public DuplexChannelFactory (Type callbackInstanceType)
			: base ()
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			string endpointConfigurationName)
			: base (endpointConfigurationName)
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
			: base (endpointConfigurationName, remoteAddress)
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			ServiceEndpoint endpoint)
			: base (endpoint)
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			Binding binding)
			: base (binding)
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			Binding binding,
			string remoteAddress)
			: base (binding, new EndpointAddress (remoteAddress))
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (Type callbackInstanceType,
			Binding binding,
			EndpointAddress remoteAddress)
			: base (binding, remoteAddress)
		{
			callback_instance_type = callbackInstanceType;
		}

		public DuplexChannelFactory (object callbackInstance)
			: this (new InstanceContext (callbackInstance))
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			string endpointConfigurationName)
			: this (new InstanceContext (callbackInstance), endpointConfigurationName)
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
			: this (new InstanceContext (callbackInstance), endpointConfigurationName, remoteAddress)
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			ServiceEndpoint endpoint)
			: this (new InstanceContext (callbackInstance), endpoint)
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			Binding binding)
			: this (new InstanceContext (callbackInstance), binding)
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			Binding binding,
			string remoteAddress)
			: this (callbackInstance, binding, new EndpointAddress (remoteAddress))
		{
		}

		public DuplexChannelFactory (object callbackInstance,
			Binding binding,
			EndpointAddress remoteAddress)
			: this (new InstanceContext (callbackInstance), binding, remoteAddress)
		{
		}

		public DuplexChannelFactory (InstanceContext callbackInstance)
			: base ()
		{
			callback_instance = callbackInstance;
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			Binding binding)
			: base (binding)
		{
			callback_instance = callbackInstance;
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			Binding binding,
			string remoteAddress)
			: this (callbackInstance, binding, new EndpointAddress (remoteAddress))
		{
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			Binding binding,
			EndpointAddress remoteAddress)
			: base (binding, remoteAddress)
		{
			callback_instance = callbackInstance;
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
			: base (endpointConfigurationName, remoteAddress)
		{
			callback_instance = callbackInstance;
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			string endpointConfigurationName)
			: base (endpointConfigurationName)
		{
			callback_instance = callbackInstance;
		}

		public DuplexChannelFactory (InstanceContext callbackInstance,
			ServiceEndpoint endpoint)
			: base (endpoint)
		{
			callback_instance = callbackInstance;
		}

		// CreateChannel() instance methods

		public TChannel CreateChannel (InstanceContext callbackInstance)
		{
			return CreateChannel (callbackInstance, Endpoint.Address, null);
		}

		public override TChannel CreateChannel (EndpointAddress address, Uri via)
		{
			return CreateChannel (callback_instance, address, via);
		}

		public TChannel CreateChannel (InstanceContext callbackInstance, EndpointAddress address)
		{
			return CreateChannel (callbackInstance, address, null);
		}

		public virtual TChannel CreateChannel (InstanceContext callbackInstance, EndpointAddress address, Uri via)
		{
			if (callbackInstance == null)
				throw new ArgumentNullException ("callbackInstance");

			EnsureOpened ();
#if DISABLE_REAL_PROXY
			Type type = ClientProxyGenerator.CreateProxyType (typeof (TChannel), Endpoint.Contract, true);
			// in .NET and SL2, it seems that the proxy is RealProxy.
			// But since there is no remoting in SL2 (and we have
			// no special magic), we have to use different approach
			// that should work either.
			object proxy = Activator.CreateInstance (type, new object [] {Endpoint, this, address, via});
#else
			object proxy = new ClientRealProxy (typeof (TChannel), new DuplexClientRuntimeChannel (Endpoint, this, address, via), true).GetTransparentProxy ();
#endif

			((IDuplexContextChannel) proxy).CallbackInstance = callbackInstance;

			return (TChannel) proxy;
		}

		// CreateChannel() factory methods

		static TChannel CreateChannelCore (DuplexChannelFactory<TChannel> cf, Func<DuplexChannelFactory<TChannel>,TChannel> f)
		{
			var ch = f (cf);
			((CommunicationObject) (object) ch).Closed += delegate { cf.Close (); };
			return ch;
		}

		public static TChannel CreateChannel (object callbackObject, string endpointConfigurationName)
		{
			return CreateChannel (new InstanceContext (callbackObject), endpointConfigurationName);
		}

		public static TChannel CreateChannel (InstanceContext callbackInstance, string endpointConfigurationName)
		{
			return new DuplexChannelFactory<TChannel> (callbackInstance, endpointConfigurationName).CreateChannel (callbackInstance);
		}

		public static TChannel CreateChannel (object callbackObject, Binding binding, EndpointAddress endpointAddress)
		{
			return CreateChannel (new InstanceContext (callbackObject), binding, endpointAddress);
		}

		public static TChannel CreateChannel (InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress)
		{
			return new DuplexChannelFactory<TChannel> (callbackInstance, binding, endpointAddress).CreateChannel (callbackInstance);
		}

		public static TChannel CreateChannel (object callbackObject, Binding binding, EndpointAddress endpointAddress, Uri via)
		{
			return CreateChannel (new InstanceContext (callbackObject), binding, endpointAddress, via);
		}

		public static TChannel CreateChannel (InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress, Uri via)
		{
			return new DuplexChannelFactory<TChannel> (callbackInstance, binding).CreateChannel (callbackInstance, endpointAddress, via);
		}

	}
}
