//
// ChannelFactory.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Xml;

namespace System.ServiceModel
{
	public abstract class ChannelFactory : CommunicationObject,
		IChannelFactory, ICommunicationObject, IDisposable
	{
		// instance members

		ServiceEndpoint service_endpoint;
		IChannelFactory factory;
		List<IClientChannel> opened_channels = new List<IClientChannel> ();

		protected ChannelFactory ()
		{
		}

		internal IChannelFactory OpenedChannelFactory {
			get {
				if (factory == null) {
					factory = CreateFactory ();
					factory.Open ();
				}

				return factory;
			}
			private set {
				factory = value;
			}
		}

		internal List<IClientChannel> OpenedChannels {
			get { return opened_channels; }
		}

		public ServiceEndpoint Endpoint {
			get { return service_endpoint; }
		}

		public ClientCredentials Credentials {
			get { return Endpoint.Behaviors.Find<ClientCredentials> (); }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return Endpoint.Binding.CloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return Endpoint.Binding.OpenTimeout; }
		}

		protected virtual void ApplyConfiguration (string endpointConfig)
		{
			if (endpointConfig == null)
				return;

#if NET_2_1
			try {
				// It should automatically use XmlXapResolver
				var cfg = new SilverlightClientConfigLoader ().Load (XmlReader.Create ("ServiceReferences.ClientConfig"));

				SilverlightClientConfigLoader.ServiceEndpointConfiguration se = null;
				if (endpointConfig == "*")
					se = cfg.GetServiceEndpointConfiguration (Endpoint.Contract.Name);
				if (se == null)
					se = cfg.GetServiceEndpointConfiguration (endpointConfig);

				if (se.Binding != null && Endpoint.Binding == null)
					Endpoint.Binding = se.Binding;
				else // ignore it
					Console.WriteLine ("WARNING: Configured binding not found in configuration {0}", endpointConfig);
				if (se.Address != null && Endpoint.Address == null)
					Endpoint.Address = se.Address;
				else // ignore it
					Console.WriteLine ("WARNING: Configured endpoint address not found in configuration {0}", endpointConfig);
			} catch (Exception) {
				// ignore it.
				Console.WriteLine ("WARNING: failed to load endpoint configuration for {0}", endpointConfig);
			}
#else

			string contractName = Endpoint.Contract.ConfigurationName;
			ClientSection client = ConfigUtil.ClientSection;
			ChannelEndpointElement endpoint = null;

			foreach (ChannelEndpointElement el in client.Endpoints) {
				if (el.Contract == contractName && (endpointConfig == el.Name || endpointConfig == "*")) {
					if (endpoint != null)
						throw new InvalidOperationException (String.Format ("More then one endpoint matching contract {0} was found.", contractName));
					endpoint = el;
				}
			}

			if (endpoint == null)
				throw new InvalidOperationException (String.Format ("Client endpoint configuration '{0}' was not found in {1} endpoints.", endpointConfig, client.Endpoints.Count));

#if NET_4_0
			var binding = String.IsNullOrEmpty (endpoint.Binding) ? null : ConfigUtil.CreateBinding (endpoint.Binding, endpoint.BindingConfiguration);
			var contract = String.IsNullOrEmpty (endpoint.Contract) ? Endpoint.Contract : ContractDescription.GetContract (ConfigUtil.GetTypeFromConfigString (endpoint.Contract));

			if (!String.IsNullOrEmpty (endpoint.Kind)) {
				var se = ConfigUtil.ConfigureStandardEndpoint (contract, endpoint);
				if (se.Binding == null)
					se.Binding = binding;
				if (se.Address == null && se.Binding != null) // standard endpoint might have empty address
					se.Address = new EndpointAddress (endpoint.Address);
				if (se.Binding == null && se.Address != null) // look for protocol mapping
					se.Binding = ConfigUtil.GetBindingByProtocolMapping (se.Address.Uri);

				service_endpoint = se;
			} else {
				if (binding == null && endpoint.Address != null) // look for protocol mapping
					Endpoint.Binding = ConfigUtil.GetBindingByProtocolMapping (endpoint.Address);
			}
#endif
			if (Endpoint.Binding == null)
				Endpoint.Binding = ConfigUtil.CreateBinding (endpoint.Binding, endpoint.BindingConfiguration);
			if (Endpoint.Address == null)
				Endpoint.Address = new EndpointAddress (endpoint.Address);

			if (endpoint.BehaviorConfiguration != "")
				ApplyBehavior (endpoint.BehaviorConfiguration);
#endif
		}

#if !NET_2_1
		private void ApplyBehavior (string behaviorConfig)
		{
			BehaviorsSection behaviorsSection = ConfigUtil.BehaviorsSection;
			EndpointBehaviorElement behaviorElement = behaviorsSection.EndpointBehaviors [behaviorConfig];
			int i = 0;
			foreach (BehaviorExtensionElement el in behaviorElement) {
				IEndpointBehavior behavior = (IEndpointBehavior) el.CreateBehavior ();
				Endpoint.Behaviors.Remove (behavior.GetType ());
				Endpoint.Behaviors.Add (behavior);
			}
		}
#endif

		protected virtual IChannelFactory CreateFactory ()
		{
			bool isOneWay = true; // check OperationDescription.IsOneWay
			foreach (var od in Endpoint.Contract.Operations)
				if (!od.IsOneWay) {
					isOneWay = false;
					break;
				}

			BindingParameterCollection pl = CreateBindingParameters ();

			// the assumption on the type of created channel could
			// be wrong, but would mostly fit the actual 
			// requirements. No books have explained how it is done.

			// try duplex
			switch (Endpoint.Contract.SessionMode) {
			case SessionMode.Required:
				if (Endpoint.Binding.CanBuildChannelFactory<IDuplexSessionChannel> (pl))
					return Endpoint.Binding.BuildChannelFactory<IDuplexSessionChannel> (pl);
				break;
			case SessionMode.Allowed:
				if (Endpoint.Binding.CanBuildChannelFactory<IDuplexChannel> (pl))
					return Endpoint.Binding.BuildChannelFactory<IDuplexChannel> (pl);
				if (Endpoint.Binding.CanBuildChannelFactory<IDuplexSessionChannel> (pl))
					return Endpoint.Binding.BuildChannelFactory<IDuplexSessionChannel> (pl);
				break;
			default:
				if (Endpoint.Binding.CanBuildChannelFactory<IDuplexChannel> (pl))
					return Endpoint.Binding.BuildChannelFactory<IDuplexChannel> (pl);
				break;
			}

			if (Endpoint.Contract.CallbackContractType != null)
				throw new InvalidOperationException ("The binding does not support duplex channel types that the contract requies for CallbackContractType.");

			if (isOneWay) {
				switch (Endpoint.Contract.SessionMode) {
				case SessionMode.Required:
					if (Endpoint.Binding.CanBuildChannelFactory<IOutputSessionChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IOutputSessionChannel> (pl);
					if (Endpoint.Binding.CanBuildChannelFactory<IDuplexSessionChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IDuplexSessionChannel> (pl);
					break;
				case SessionMode.Allowed:
					if (Endpoint.Binding.CanBuildChannelFactory<IOutputChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IOutputChannel> (pl);
					if (Endpoint.Binding.CanBuildChannelFactory<IDuplexChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IDuplexChannel> (pl);
					goto case SessionMode.Required;
				default:
					if (Endpoint.Binding.CanBuildChannelFactory<IOutputChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IOutputChannel> (pl);
					if (Endpoint.Binding.CanBuildChannelFactory<IDuplexChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IDuplexChannel> (pl);
					break;
				}
			}
			// both OneWay and non-OneWay contracts fall into here.
			{
				switch (Endpoint.Contract.SessionMode) {
				case SessionMode.Required:
					if (Endpoint.Binding.CanBuildChannelFactory<IRequestSessionChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IRequestSessionChannel> (pl);
					break;
				case SessionMode.Allowed:
					if (Endpoint.Binding.CanBuildChannelFactory<IRequestChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IRequestChannel> (pl);
					if (Endpoint.Binding.CanBuildChannelFactory<IRequestSessionChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IRequestSessionChannel> (pl);
					break;
				default:
					if (Endpoint.Binding.CanBuildChannelFactory<IRequestChannel> (pl))
						return Endpoint.Binding.BuildChannelFactory<IRequestChannel> (pl);
					break;
				}
			}
			throw new InvalidOperationException (String.Format ("The binding does not support any of the channel types that the contract '{0}' allows.", Endpoint.Contract.Name));
		}

		BindingParameterCollection CreateBindingParameters ()
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();

			ContractDescription cd = Endpoint.Contract;
#if !NET_2_1
			pl.Add (ChannelProtectionRequirements.CreateFromContract (cd));

			foreach (IEndpointBehavior behavior in Endpoint.Behaviors)
				behavior.AddBindingParameters (Endpoint, pl);
#endif

			return pl;
		}

		protected abstract ServiceEndpoint CreateDescription ();

		void IDisposable.Dispose ()
		{
			Close ();
		}

		public T GetProperty<T> () where T : class
		{
			if (OpenedChannelFactory != null)
				OpenedChannelFactory.GetProperty<T> ();
			return null;
		}

		protected void EnsureOpened ()
		{
			if (Endpoint == null)
				throw new InvalidOperationException ("A service endpoint must be configured for this channel factory");
			if (Endpoint.Contract == null)
				throw new InvalidOperationException ("A service Contract must be configured for this channel factory");
			if (Endpoint.Binding == null)
				throw new InvalidOperationException ("A Binding must be configured for this channel factory");

			if (State != CommunicationState.Opened)
				Open ();
		}

		protected void InitializeEndpoint (
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
		{
			InitializeEndpoint (CreateDescription ());
			if (remoteAddress != null)
				service_endpoint.Address = remoteAddress;
			ApplyConfiguration (endpointConfigurationName);
		}

		protected void InitializeEndpoint (Binding binding,
			EndpointAddress remoteAddress)
		{
			InitializeEndpoint (CreateDescription ());
			if (binding != null)
				service_endpoint.Binding = binding;
			if (remoteAddress != null)
				service_endpoint.Address = remoteAddress;
		}

		protected void InitializeEndpoint (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			service_endpoint = endpoint;
		}

		protected override void OnAbort ()
		{
			if (OpenedChannelFactory != null)
				OpenedChannelFactory.Abort ();
		}

		Action<TimeSpan> close_delegate;
		Action<TimeSpan> open_delegate;


		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnClose);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			if (close_delegate == null)
				throw new InvalidOperationException ("Async close operation has not started");
			close_delegate.EndInvoke (result);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			if (open_delegate == null)
				throw new InvalidOperationException ("Async close operation has not started");
			open_delegate.EndInvoke (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			foreach (var ch in opened_channels.ToArray ())
				ch.Close (timeout - (DateTime.Now - start));
			if (OpenedChannelFactory != null)
				OpenedChannelFactory.Close (timeout - (DateTime.Now - start));
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override void OnOpening ()
		{
			base.OnOpening ();
			OpenedChannelFactory = CreateFactory ();
		}

		protected override void OnOpened ()
		{
			base.OnOpened ();
			OpenedChannelFactory.Open ();
		}
	}

#if obsolete
	[ServiceContract]
	interface UninitializedContract
	{
		[OperationContract]
		void ItShouldReallyGone ();
	}
#endif
}
