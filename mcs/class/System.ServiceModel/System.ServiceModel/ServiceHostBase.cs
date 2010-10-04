//
// ServiceHostBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Reflection;
using System.Threading;

namespace System.ServiceModel
{
	public abstract partial class ServiceHostBase
		: CommunicationObject, IExtensibleObject<ServiceHostBase>, IDisposable
	{
		// It is used for mapping a ServiceHostBase to HttpChannelListener precisely.
		internal static ServiceHostBase CurrentServiceHostHack;

		ServiceCredentials credentials;
		ServiceDescription description;
		UriSchemeKeyedCollection base_addresses;
		TimeSpan open_timeout, close_timeout, instance_idle_timeout;
		List<InstanceContext> contexts;
		ReadOnlyCollection<InstanceContext> exposed_contexts;
		ChannelDispatcherCollection channel_dispatchers;
		IDictionary<string,ContractDescription> contracts;
		int flow_limit = int.MaxValue;
		IExtensionCollection<ServiceHostBase> extensions;

		protected ServiceHostBase ()
		{
			open_timeout = DefaultOpenTimeout;
			close_timeout = DefaultCloseTimeout;

			credentials = new ServiceCredentials ();
			contexts = new List<InstanceContext> ();
			exposed_contexts = new ReadOnlyCollection<InstanceContext> (contexts);
			channel_dispatchers = new ChannelDispatcherCollection (this);
		}

		public event EventHandler<UnknownMessageReceivedEventArgs>
			UnknownMessageReceived;

		internal void OnUnknownMessageReceived (Message message)
		{
			if (UnknownMessageReceived != null)
				UnknownMessageReceived (this, new UnknownMessageReceivedEventArgs (message));
			else
				// FIXME: better be logged
				throw new EndpointNotFoundException (String.Format ("The request message has the target '{0}' with action '{1}' which is not reachable in this service contract", message.Headers.To, message.Headers.Action));
		}

		public ReadOnlyCollection<Uri> BaseAddresses {
			get {
				if (base_addresses == null)
					base_addresses = new UriSchemeKeyedCollection ();
				return new ReadOnlyCollection<Uri> (base_addresses.InternalItems);
			}
		}

		internal Uri CreateUri (string scheme, Uri relativeUri)
		{
			Uri baseUri = base_addresses.Contains (scheme) ? base_addresses [scheme] : null;

			if (relativeUri == null)
				return baseUri;
			if (relativeUri.IsAbsoluteUri)
				return relativeUri;
			if (baseUri == null)
				return null;
			var s = relativeUri.ToString ();
			if (s.Length == 0)
				return baseUri;
			var l = baseUri.LocalPath;
			var r = relativeUri.ToString ();

			if (l.Length > 0 && l [l.Length - 1] != '/' && r [0] != '/')
				return new Uri (String.Concat (baseUri.ToString (), "/", r));
			else
				return new Uri (String.Concat (baseUri.ToString (), r));
		}

		public ChannelDispatcherCollection ChannelDispatchers {
			get { return channel_dispatchers; }
		}

		public ServiceAuthorizationBehavior Authorization {
			get;
			private set;
		}

		public ServiceCredentials Credentials {
			get { return credentials; }
		}

		public ServiceDescription Description {
			get { return description; }
		}

		protected internal IDictionary<string,ContractDescription> ImplementedContracts {
			get { return contracts; }
		}

		public IExtensionCollection<ServiceHostBase> Extensions {
			get {
				if (extensions == null)
					extensions = new ExtensionCollection<ServiceHostBase> (this);
				return extensions;
			}
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return DefaultCommunicationTimeouts.Instance.CloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return DefaultCommunicationTimeouts.Instance.OpenTimeout; }
		}

		public TimeSpan CloseTimeout {
			get { return close_timeout; }
			set { close_timeout = value; }
		}

		public TimeSpan OpenTimeout {
			get { return open_timeout; }
			set { open_timeout = value; }
		}

		public int ManualFlowControlLimit {
			get { return flow_limit; }
			set { flow_limit = value; }
		}

		protected void AddBaseAddress (Uri baseAddress)
		{
			if (base_addresses == null)
				throw new InvalidOperationException ("Base addresses must be added before the service description is initialized");
			base_addresses.Add (baseAddress);
		}

		public ServiceEndpoint AddServiceEndpoint (
			string implementedContract, Binding binding, string address)
		{
			return AddServiceEndpoint (implementedContract,
				binding,
				new Uri (address, UriKind.RelativeOrAbsolute));
		}

		public ServiceEndpoint AddServiceEndpoint (
			string implementedContract, Binding binding,
			string address, Uri listenUri)
		{
			Uri uri = new Uri (address, UriKind.RelativeOrAbsolute);
			return AddServiceEndpoint (
				implementedContract, binding, uri, listenUri);
		}

		public ServiceEndpoint AddServiceEndpoint (
			string implementedContract, Binding binding,
			Uri address)
		{
			return AddServiceEndpoint (implementedContract, binding, address, null);
		}

		public ServiceEndpoint AddServiceEndpoint (
			string implementedContract, Binding binding,
			Uri address, Uri listenUri)
		{
			EndpointAddress ea = new EndpointAddress (BuildAbsoluteUri (address, binding));
			ContractDescription cd = GetContract (implementedContract, binding.Namespace == "http://schemas.microsoft.com/ws/2005/02/mex/bindings");
			if (cd == null)
				throw new InvalidOperationException (String.Format ("Contract '{0}' was not found in the implemented contracts in this service host.", implementedContract));
			return AddServiceEndpointCore (cd, binding, ea, listenUri);
		}

#if NET_4_0
		public virtual void AddServiceEndpoint (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");

			ThrowIfDisposedOrImmutable ();

			if (endpoint.Address == null)
				throw new ArgumentException ("Address on the argument endpoint is null");
			if (endpoint.Contract == null)
				throw new ArgumentException ("Contract on the argument endpoint is null");
			if (endpoint.Binding == null)
				throw new ArgumentException ("Binding on the argument endpoint is null");

			if (!ImplementedContracts.Values.Any (cd => cd.ContractType == endpoint.Contract.ContractType) &&
			    endpoint.Binding.Namespace != "http://schemas.microsoft.com/ws/2005/02/mex/bindings") // special case
				throw new InvalidOperationException (String.Format ("Contract '{0}' is not implemented in this service '{1}'", endpoint.Contract.Name, Description.Name));

			Description.Endpoints.Add (endpoint);
		}
#endif

		Type PopulateType (string typeName)
		{
			Type type = Type.GetType (typeName);
			if (type != null)
				return type;
			foreach (ContractDescription cd in ImplementedContracts.Values) {
				type = cd.ContractType.Assembly.GetType (typeName);
				if (type != null)
					return type;
			}
			return null;
		}

		ContractDescription mex_contract, help_page_contract;

		ContractDescription GetContract (string name, bool mexBinding)
		{
			// FIXME: not sure if they should really be special cases.
			switch (name) {
			case "IHttpGetHelpPageAndMetadataContract":
				if (help_page_contract == null)
					help_page_contract = ContractDescription.GetContract (typeof (IHttpGetHelpPageAndMetadataContract));
				return help_page_contract;
			case "IMetadataExchange":
				// this is certainly looking special (or we may 
				// be missing something around ServiceMetadataExtension).
				// It seems .NET WCF has some "infrastructure"
				// endpoints. .NET ServiceHost fails to Open()
				// if it was added only IMetadataExchange 
				// endpoint (and you'll see the word
				// "infrastructure" in the exception message).
				if (mexBinding && Description.Behaviors.Find<ServiceMetadataBehavior> () == null)
					break;
				if (mex_contract == null)
					mex_contract = ContractDescription.GetContract (typeof (IMetadataExchange));
				return mex_contract;
			}

			Type type = PopulateType (name);
			if (type == null)
				return null;

			foreach (ContractDescription cd in ImplementedContracts.Values) {
				// This check is a negative side effect of the above match-by-name design.
				if (cd.ContractType == typeof (IMetadataExchange))
					continue;

				if (cd.ContractType == type ||
				    cd.ContractType.IsSubclassOf (type) ||
				    type.IsInterface && cd.ContractType.GetInterface (type.FullName) == type)
					return cd;
			}
			return null;
		}

		internal Uri BuildAbsoluteUri (Uri address, Binding binding)
		{
			if (!address.IsAbsoluteUri) {
				// Find a Base address with matching scheme,
				// and build new absolute address
				if (!base_addresses.Contains (binding.Scheme))
					throw new InvalidOperationException (String.Format ("Could not find base address that matches Scheme {0} for endpoint {1}", binding.Scheme, binding.Name));

				Uri baseaddr = base_addresses [binding.Scheme];

				if (!baseaddr.AbsoluteUri.EndsWith ("/") && address.OriginalString.Length > 0) // with empty URI it should not add '/' to possible file name of the absolute URI
					baseaddr = new Uri (baseaddr.AbsoluteUri + "/");
				address = new Uri (baseaddr, address);
			}
			return address;
		}

		internal ServiceEndpoint AddServiceEndpointCore (
			ContractDescription cd, Binding binding, EndpointAddress address, Uri listenUri)
		{
			if (listenUri != null)
				listenUri = BuildAbsoluteUri (listenUri, binding);

			foreach (ServiceEndpoint e in Description.Endpoints)
				if (e.Contract == cd && e.Binding == binding && e.Address == address && e.ListenUri.Equals (listenUri))
					return e;
			ServiceEndpoint se = new ServiceEndpoint (cd, binding, address);
			// FIXME: should we reject relative ListenUri?
			se.ListenUri = listenUri ?? address.Uri;
			Description.Endpoints.Add (se);
			return se;
		}

		protected virtual void ApplyConfiguration ()
		{
			if (Description == null)
				throw new InvalidOperationException ("ApplyConfiguration requires that the Description property be initialized. Either provide a valid ServiceDescription in the CreateDescription method or override the ApplyConfiguration method to provide an alternative implementation");

			ServiceElement service = GetServiceElement ();
			
			if (service != null)
				ApplyServiceElement (service);
#if NET_4_0
			// simplified configuration
			AddServiceBehaviors (String.Empty, false);
#endif
			// TODO: consider commonBehaviors here

			// ensure ServiceAuthorizationBehavior
			Authorization = Description.Behaviors.Find<ServiceAuthorizationBehavior> ();
			if (Authorization == null) {
				Authorization = new ServiceAuthorizationBehavior ();
				Description.Behaviors.Add (Authorization);
			}

			// ensure ServiceDebugBehavior
			ServiceDebugBehavior debugBehavior = Description.Behaviors.Find<ServiceDebugBehavior> ();
			if (debugBehavior == null) {
				debugBehavior = new ServiceDebugBehavior ();
				Description.Behaviors.Add (debugBehavior);
			}
		}

		void AddServiceBehaviors (string configurationName, bool throwIfNotFound)
		{
#if NET_4_0
			if (configurationName == null)
				return;
#else
			if (String.IsNullOrEmpty (configurationName))
				return;
#endif
			ServiceBehaviorElement behavior = ConfigUtil.BehaviorsSection.ServiceBehaviors [configurationName];
			if (behavior == null) {
				if (throwIfNotFound)
					throw new ArgumentException (String.Format ("Service behavior configuration '{0}' was not found", configurationName));
				return;
			}

			KeyedByTypeCollection<IServiceBehavior> behaviors = Description.Behaviors;
			foreach (var bxe in behavior) {
				IServiceBehavior b = (IServiceBehavior) bxe.CreateBehavior ();
				if (behaviors.Contains (b.GetType ()))
					continue;
				behaviors.Add (b);
			}
		}
		
		void ApplyServiceElement (ServiceElement service)
		{
			//base addresses
			HostElement host = service.Host;
			foreach (BaseAddressElement baseAddress in host.BaseAddresses) {
				AddBaseAddress (new Uri (baseAddress.BaseAddress));
			}

			// behaviors
			AddServiceBehaviors (service.BehaviorConfiguration, true);

			// services
			foreach (ServiceEndpointElement endpoint in service.Endpoints) {
				ServiceEndpoint se;

#if NET_4_0
				var binding = String.IsNullOrEmpty (endpoint.Binding) ? null : ConfigUtil.CreateBinding (endpoint.Binding, endpoint.BindingConfiguration);

				if (!String.IsNullOrEmpty (endpoint.Kind)) {
					var contract = String.IsNullOrEmpty (endpoint.Contract) ? null : GetContract (endpoint.Contract, false);
					se = ConfigUtil.ConfigureStandardEndpoint (contract, endpoint);
					if (se.Binding == null)
						se.Binding = binding;
					if (se.Address == null && se.Binding != null) // standard endpoint might have empty address
						se.Address = new EndpointAddress (CreateUri (se.Binding.Scheme, endpoint.Address));
					if (se.Binding == null && se.Address != null) // look for protocol mapping
						se.Binding = GetBindingByProtocolMapping (se.Address.Uri);

					AddServiceEndpoint (se);
				}
				else {
					if (binding == null && endpoint.Address != null) // look for protocol mapping
						binding = GetBindingByProtocolMapping (endpoint.Address);
					se = AddServiceEndpoint (endpoint.Contract, binding, endpoint.Address);
				}
#else
				var binding = ConfigUtil.CreateBinding (endpoint.Binding, endpoint.BindingConfiguration);
				se = AddServiceEndpoint (endpoint.Contract, binding, endpoint.Address);
#endif

				// endpoint behaviors
				EndpointBehaviorElement epbehavior = ConfigUtil.BehaviorsSection.EndpointBehaviors [endpoint.BehaviorConfiguration];
				if (epbehavior != null)
					foreach (var bxe in epbehavior) {
						IEndpointBehavior b = (IEndpointBehavior) bxe.CreateBehavior ();
						se.Behaviors.Add (b);
				}
			}
		}

#if NET_4_0
		Binding GetBindingByProtocolMapping (Uri address)
		{
			ProtocolMappingElement el = ConfigUtil.ProtocolMappingSection.ProtocolMappingCollection [address.Scheme];
			if (el == null)
				return null;
			return ConfigUtil.CreateBinding (el.Binding, el.BindingConfiguration);
		}
#endif

		private ServiceElement GetServiceElement() {
			Type serviceType = Description.ServiceType;
			if (serviceType == null)
				return null;

			return ConfigUtil.ServicesSection.Services [serviceType.FullName];			
		}

		protected abstract ServiceDescription CreateDescription (
			out IDictionary<string,ContractDescription> implementedContracts);

		protected void InitializeDescription (UriSchemeKeyedCollection baseAddresses)
		{
			this.base_addresses = baseAddresses;
			IDictionary<string,ContractDescription> retContracts;
			description = CreateDescription (out retContracts);
			contracts = retContracts;

			ApplyConfiguration ();
		}

		protected virtual void InitializeRuntime ()
		{
			//First validate the description, which should call all behaviors
			//'Validate' method.
			ValidateDescription ();
			
			//Build all ChannelDispatchers, one dispatcher per user configured EndPoint.
			//We must keep thet ServiceEndpoints as a seperate collection, since the user
			//can change the collection in the description during the behaviors events.
			ServiceEndpoint[] endPoints = new ServiceEndpoint[Description.Endpoints.Count];
			Description.Endpoints.CopyTo (endPoints, 0);
			var builder = new DispatcherBuilder (this);
			foreach (ServiceEndpoint se in endPoints) {
				var commonParams = new BindingParameterCollection ();
				foreach (IServiceBehavior b in Description.Behaviors)
					b.AddBindingParameters (Description, this, Description.Endpoints, commonParams);

				var channel = builder.BuildChannelDispatcher (Description.ServiceType, se, commonParams);
				if (!ChannelDispatchers.Contains (channel))
					ChannelDispatchers.Add (channel);
			}

			//After the ChannelDispatchers are created, and attached to the service host
			//Apply dispatching behaviors.
			//
			// This behavior application order is tricky: first only
			// ServiceDebugBehavior and ServiceMetadataBehavior are
			// applied, and then other service behaviors are applied.
			// It is because those two behaviors adds ChannelDispatchers
			// and any other service behaviors must be applied to
			// those newly populated dispatchers.
			foreach (IServiceBehavior b in Description.Behaviors)
				if (b is ServiceMetadataBehavior || b is ServiceDebugBehavior)
					b.ApplyDispatchBehavior (Description, this);
			foreach (IServiceBehavior b in Description.Behaviors)
				if (!(b is ServiceMetadataBehavior || b is ServiceDebugBehavior))
					b.ApplyDispatchBehavior (Description, this);

			builder.ApplyDispatchBehaviors ();
		}

		private void ValidateDescription ()
		{
			foreach (IServiceBehavior b in Description.Behaviors)
				b.Validate (Description, this);
			foreach (ServiceEndpoint endPoint in Description.Endpoints)
				endPoint.Validate ();

#if NET_4_0
			// In 4.0, it seems that if there is no configured ServiceEndpoint, infer them from the service type.
			if (Description.Endpoints.Count == 0) {
				foreach (Type iface in Description.ServiceType.GetInterfaces ())
					if (iface.GetCustomAttributes (typeof (ServiceContractAttribute), true).Length > 0)
						foreach (var baddr in BaseAddresses) {
							if (!baddr.IsAbsoluteUri)
								continue;
							var binding = GetBindingByProtocolMapping (baddr);
							if (binding == null)
								continue;
							AddServiceEndpoint (iface.FullName, binding, baddr);
						}
			}
#endif

			if (Description.Endpoints.FirstOrDefault (e => e.Contract != mex_contract && !e.IsSystemEndpoint) == null)
				throw new InvalidOperationException ("The ServiceHost must have at least one application endpoint (that does not include metadata exchange endpoint) defined by either configuration, behaviors or call to AddServiceEndpoint methods.");
		}

		[MonoTODO]
		protected void LoadConfigurationSection (ServiceElement element)
		{
			ServicesSection services = ConfigUtil.ServicesSection;
		}

		[MonoTODO]
		protected override sealed void OnAbort ()
		{
		}

		Action<TimeSpan> close_delegate;
		Action<TimeSpan> open_delegate;

		protected override sealed IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate != null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override sealed IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			ReleasePerformanceCounters ();
			List<ChannelDispatcherBase> l = new List<ChannelDispatcherBase> (ChannelDispatchers);
			foreach (ChannelDispatcherBase e in l) {
				try {
					TimeSpan ts = timeout - (DateTime.Now - start);
					if (ts < TimeSpan.Zero)
						e.Abort ();
					else
						e.Close (ts);
				} catch (Exception ex) {
					Console.WriteLine ("ServiceHostBase failed to close the channel dispatcher:");
					Console.WriteLine (ex);
				}
			}
		}

		protected override sealed void OnOpen (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			InitializeRuntime ();
			for (int i = 0; i < ChannelDispatchers.Count; i++) {
				// Skip ServiceMetadataExtension-based one. special case.
				for (int j = i + 1; j < ChannelDispatchers.Count; j++) {
					var cd1 = ChannelDispatchers [i];
					var cd2 = ChannelDispatchers [j];
					if (cd1.IsMex || cd2.IsMex)
						continue;
					// surprisingly, some ChannelDispatcherBase implementations have null Listener property.
					if (cd1.Listener != null && cd2.Listener != null && cd1.Listener.Uri.Equals (cd2.Listener.Uri))
						throw new InvalidOperationException ("Two or more service endpoints with different Binding instance are bound to the same listen URI.");
				}
			}

			var waits = new List<ManualResetEvent> ();
			foreach (var cd in ChannelDispatchers) {
				var wait = new ManualResetEvent (false);
				cd.Opened += delegate { wait.Set (); };
				waits.Add (wait);
				cd.Open (timeout - (DateTime.Now - start));
			}

			WaitHandle.WaitAll (waits.ToArray ());
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			if (close_delegate == null)
				throw new InvalidOperationException ("Async close operation has not started");
			close_delegate.EndInvoke (result);
		}

		protected override sealed void OnEndOpen (IAsyncResult result)
		{
			if (open_delegate == null)
				throw new InvalidOperationException ("Aync open operation has not started");
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpened ()
		{
			base.OnOpened ();
		}

		[MonoTODO]
		protected void ReleasePerformanceCounters ()
		{
		}

		void IDisposable.Dispose ()
		{
			Close ();
		}
	}

	/// <summary>
	///  Builds ChannelDispatchers as appropriate to service the service endpoints. 
	/// </summary>
	partial class DispatcherBuilder
	{
		ServiceHostBase host;

		public DispatcherBuilder (ServiceHostBase host)
		{
			this.host = host;
		}

		Dictionary<Binding,ChannelDispatcher> built_dispatchers = new Dictionary<Binding,ChannelDispatcher> ();
		Dictionary<ServiceEndpoint, EndpointDispatcher> ep_to_dispatcher_ep = new Dictionary<ServiceEndpoint, EndpointDispatcher> ();

		internal static Action<ChannelDispatcher> ChannelDispatcherSetter;

		internal ChannelDispatcher BuildChannelDispatcher (Type serviceType, ServiceEndpoint se, BindingParameterCollection commonParams)
		{
			//Let all behaviors add their binding parameters
			AddBindingParameters (commonParams, se);
			
			// See if there's an existing channel that matches this endpoint
			var version = se.Binding.GetProperty<MessageVersion> (commonParams);
			if (version == null)
				throw new InvalidOperationException ("At least one BindingElement in the Binding must override GetProperty method to return a MessageVersion and no prior binding element should return null instead of calling GetInnerProperty method on BindingContext.");

			ChannelDispatcher cd = FindExistingDispatcher (se);
			EndpointDispatcher ep;
			if (cd != null) {
				ep = cd.InitializeServiceEndpoint (serviceType, se);
			} else {
				// Use the binding parameters to build the channel listener and Dispatcher.
				lock (HttpTransportBindingElement.ListenerBuildLock) {
					ServiceHostBase.CurrentServiceHostHack = host;
					IChannelListener lf = BuildListener (se, commonParams);
					cd = new ChannelDispatcher (lf, se.Binding.Name);
					cd.MessageVersion = version;
					if (ChannelDispatcherSetter != null) {
						ChannelDispatcherSetter (cd);
						ChannelDispatcherSetter = null;
					}
					ServiceHostBase.CurrentServiceHostHack = null;
				}
				ep = cd.InitializeServiceEndpoint (serviceType, se);
				built_dispatchers.Add (se.Binding, cd);
			}
			ep_to_dispatcher_ep[se] = ep;
			return cd;
		}
		
		ChannelDispatcher FindExistingDispatcher (ServiceEndpoint se)
		{
			return built_dispatchers.FirstOrDefault ((KeyValuePair<Binding,ChannelDispatcher> p) => se.Binding == p.Key).Value;
		}

		internal void ApplyDispatchBehaviors ()
		{
			foreach (KeyValuePair<ServiceEndpoint, EndpointDispatcher> val in ep_to_dispatcher_ep)
				ApplyDispatchBehavior (val.Value, val.Key);
		}
		
		private void ApplyDispatchBehavior (EndpointDispatcher ed, ServiceEndpoint endPoint)
		{
			foreach (IContractBehavior b in endPoint.Contract.Behaviors)
				b.ApplyDispatchBehavior (endPoint.Contract, endPoint, ed.DispatchRuntime);
			foreach (IEndpointBehavior b in endPoint.Behaviors)
				b.ApplyDispatchBehavior (endPoint, ed);
			foreach (OperationDescription operation in endPoint.Contract.Operations) {
				foreach (IOperationBehavior b in operation.Behaviors)
					b.ApplyDispatchBehavior (operation, ed.DispatchRuntime.Operations [operation.Name]);
			}

		}

		private void AddBindingParameters (BindingParameterCollection commonParams, ServiceEndpoint endPoint) {

			commonParams.Add (ChannelProtectionRequirements.CreateFromContract (endPoint.Contract));

			foreach (IContractBehavior b in endPoint.Contract.Behaviors)
				b.AddBindingParameters (endPoint.Contract, endPoint, commonParams);
			foreach (IEndpointBehavior b in endPoint.Behaviors)
				b.AddBindingParameters (endPoint, commonParams);
			foreach (OperationDescription operation in endPoint.Contract.Operations) {
				foreach (IOperationBehavior b in operation.Behaviors)
					b.AddBindingParameters (operation, commonParams);
			}
		}

		static IChannelListener BuildListener (ServiceEndpoint se,
			BindingParameterCollection pl)
		{
			Binding b = se.Binding;
			if (b.CanBuildChannelListener<IReplySessionChannel> (pl))
				return b.BuildChannelListener<IReplySessionChannel> (se.ListenUri, "", se.ListenUriMode, pl);
			if (b.CanBuildChannelListener<IReplyChannel> (pl))
				return b.BuildChannelListener<IReplyChannel> (se.ListenUri, "", se.ListenUriMode, pl);
			if (b.CanBuildChannelListener<IInputSessionChannel> (pl))
				return b.BuildChannelListener<IInputSessionChannel> (se.ListenUri, "", se.ListenUriMode, pl);
			if (b.CanBuildChannelListener<IInputChannel> (pl))
				return b.BuildChannelListener<IInputChannel> (se.ListenUri, "", se.ListenUriMode, pl);

			if (b.CanBuildChannelListener<IDuplexChannel> (pl))
				return b.BuildChannelListener<IDuplexChannel> (se.ListenUri, "", se.ListenUriMode, pl);
			if (b.CanBuildChannelListener<IDuplexSessionChannel> (pl))
				return b.BuildChannelListener<IDuplexSessionChannel> (se.ListenUri, "", se.ListenUriMode, pl);
			throw new InvalidOperationException ("None of the listener channel types is supported");
		}
	}
}
