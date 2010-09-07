//
// ServiceHostBaseTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using SMMessage = System.ServiceModel.Channels.Message;
using System.ServiceModel.Channels;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceHostBaseTest
	{
		class Poker : ServiceHostBase
		{
			public event EventHandler OnApplyConfiguration;

			protected override ServiceDescription CreateDescription (out IDictionary<string, ContractDescription> implementedContracts) {
				implementedContracts = new Dictionary<string, ContractDescription> ();
				ServiceDescription description = new ServiceDescription ();
				description.ServiceType = typeof (MyService);
				description.Behaviors.Add (new ServiceBehaviorAttribute ());
				return description;
			}

			protected override void ApplyConfiguration () {
				if (OnApplyConfiguration != null)
					OnApplyConfiguration (this, EventArgs.Empty);
				base.ApplyConfiguration ();
			}

			public void CallInitializeDescription () {
				InitializeDescription (new UriSchemeKeyedCollection ());
			}

			protected override void InitializeRuntime () {
				base.InitializeRuntime ();
			}

			public void CallInitializeRuntime () {
				InitializeRuntime ();
			}

			public void DoAddBaseAddress (Uri uri)
			{
				AddBaseAddress (uri);
			}
		}

		[Test]
		public void Ctor () {
			Poker host = new Poker ();

			Assert.AreEqual (null, host.Description, "Description");
			Assert.AreEqual (null, host.Authorization, "Authorization");
		}

		[Test]
		public void DefaultConfiguration () {
			Poker host = new Poker ();
			host.OnApplyConfiguration += delegate (object sender, EventArgs e) {
				Assert.AreEqual (1, host.Description.Behaviors.Count, "Description.Behaviors.Count #1");
			};
			host.CallInitializeDescription ();

			Assert.AreEqual (true, host.Description.Behaviors.Count > 1, "Description.Behaviors.Count #2");

			Assert.IsNotNull (host.Description.Behaviors.Find<ServiceDebugBehavior> (), "ServiceDebugBehavior");
			Assert.IsNotNull (host.Description.Behaviors.Find<ServiceAuthorizationBehavior> (), "ServiceDebugBehavior");
			Assert.IsNotNull (host.Authorization, "Authorization #1");

			Assert.AreEqual (host.Description.Behaviors.Find<ServiceAuthorizationBehavior> (), host.Authorization, "Authorization #2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ApplyConfigurationNoDescription () {
			CustomServiceHost customHost = new CustomServiceHost ();
			customHost.ApplyConfiguration ();
		}

		class CustomServiceHost : ServiceHostBase
		{

			public CustomServiceHost () {

			}

			public new void ApplyConfiguration () {
				base.ApplyConfiguration ();
			}

			protected override ServiceDescription CreateDescription (out IDictionary<string, ContractDescription> implementedContracts) {
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void InitializeRuntime () {
			Poker host = new Poker ();
			host.CallInitializeDescription ();
			EndpointAddress address = new EndpointAddress ("http://localhost:8090/");
			ContractDescription contract = ContractDescription.GetContract (typeof (IMyContract));
			ServiceEndpoint endpoint = new ServiceEndpoint (contract, new BasicHttpBinding (), address);
			endpoint.ListenUri = address.Uri;
			host.Description.Endpoints.Add (endpoint);

			Assert.AreEqual (0, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");

			host.CallInitializeRuntime ();

			Assert.AreEqual (1, host.ChannelDispatchers.Count, "ChannelDispatchers.Count #1");
			Assert.AreEqual (CommunicationState.Created, host.ChannelDispatchers [0].State, "ChannelDispatchers.Count #1");
		}

		[ServiceContract]
		interface IMyContract
		{
			[OperationContract]
			string GetData ();
		}

		class MyService : IMyContract
		{
			public string GetData () {
				return "Hello World";
			}
		}

		[Test]
		public void ChannelDispatchers_NoDebug () {
			ServiceHost h = new ServiceHost (typeof (AllActions), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (AllActions).FullName, new BasicHttpBinding (), "address");

			ServiceDebugBehavior b = h.Description.Behaviors.Find<ServiceDebugBehavior> ();
			b.HttpHelpPageEnabled = false;						

			h.Open ();
			try {
			Assert.AreEqual (h.ChannelDispatchers.Count, 1);
			ChannelDispatcher channelDispatcher =  h.ChannelDispatchers[0] as ChannelDispatcher;
			Assert.IsNotNull (channelDispatcher, "#1");
			Assert.IsTrue (channelDispatcher.Endpoints.Count == 1, "#2");
			EndpointAddressMessageFilter filter = channelDispatcher.Endpoints [0].AddressFilter as EndpointAddressMessageFilter;
			Assert.IsNotNull (filter, "#3");
			Assert.IsTrue (filter.Address.Equals (new EndpointAddress ("http://localhost:8080/address")), "#4");
			Assert.IsFalse (filter.IncludeHostNameInComparison, "#5");
			Assert.IsTrue (channelDispatcher.Endpoints [0].ContractFilter is MatchAllMessageFilter, "#6");
			} finally {
			h.Close ();
			}
		}

		[Test]
		public void ChannelDispatchers_WithDebug () {
			ServiceHost h = new ServiceHost (typeof (AllActions), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (AllActions).FullName, new BasicHttpBinding (), "address");
			ServiceMetadataBehavior b = new ServiceMetadataBehavior ();
			b.HttpGetEnabled = true;
			b.HttpGetUrl = new Uri( "http://localhost:8080" );
			h.Description.Behaviors.Add (b);
			h.Open ();

			Assert.AreEqual (h.ChannelDispatchers.Count, 2, "#1");
			ChannelDispatcher channelDispatcher = h.ChannelDispatchers[1] as ChannelDispatcher;
			Assert.IsNotNull (channelDispatcher, "#2");
			Assert.IsTrue (channelDispatcher.Endpoints.Count == 1, "#3");
			EndpointAddressMessageFilter filter = channelDispatcher.Endpoints [0].AddressFilter as EndpointAddressMessageFilter;
			Assert.IsNotNull (filter, "#4");
			Assert.IsTrue (filter.Address.Equals (new EndpointAddress ("http://localhost:8080")), "#5");
			Assert.IsFalse (filter.IncludeHostNameInComparison, "#6");
			Assert.IsTrue (channelDispatcher.Endpoints [0].ContractFilter is MatchAllMessageFilter, "#7");
			h.Close ();
		}

		[Test]
		public void SpecificActionTest ()
		{
			//EndpointDispatcher d = new EndpointDispatcher(
			ServiceHost h = new ServiceHost (typeof (SpecificAction), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (Action1Interface), new BasicHttpBinding (), "address");
						
			h.Open ();
			ChannelDispatcher d = h.ChannelDispatchers [0] as ChannelDispatcher;
			EndpointDispatcher ed = d.Endpoints [0] as EndpointDispatcher;
			ActionMessageFilter actionFilter = ed.ContractFilter as ActionMessageFilter;
			Assert.IsNotNull (actionFilter, "#1");
			Assert.IsTrue (actionFilter.Actions.Count == 1, "#2");
			h.Close();
		}

		[Test]
		public void InitializeRuntimeBehaviors1 () {
			HostState st = new HostState ();
			ServiceHost h = new ServiceHost (typeof (SpecificAction2), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (SpecificAction2), new BasicHttpBinding (), "temp");			

			h.Description.Behaviors.Add (new MyServiceBehavior (st, h));

			h.Description.Endpoints [0].Behaviors.Add (new MyEndpointBehavior (st, h));
			h.Description.Endpoints [0].Contract.Behaviors.Add (new MyContractBehavior (st, h));
			h.Description.Endpoints [0].Contract.Operations [0].Behaviors.Add (new MyOperationBehavior (st, h));
			
			h.Open ();
			h.Close ();
			
			string expected = "Start, IServiceBehavior.Validate, IContractBehavior.Validate, IEndpointBehavior.Validate, IOperationBehavior.ApplyDispatchBehavior, IServiceBehavior.AddBindingParameters, IContractBehavior.AddBindingParameters, IEndpointBehavior.AddBindingParameters, IOperationBehavior.AddBindingParameters, IServiceBehavior.ApplyDispatchBehavior, IContractBehavior.ApplyDispatchBehavior, IEndpointBehavior.ApplyDispatchBehavior, IOperationBehavior.ApplyDispatchBehavior";
			Assert.AreEqual (expected, st.CurrentStage);
		}

		[Test]
		public void InitializeRuntimeBehaviors2 () {
			HostState st = new HostState ();
			ServiceHost h = new ServiceHost (typeof (SpecificAction), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (Action1Interface), new BasicHttpBinding (), "temp");
			h.AddServiceEndpoint (typeof (Action2Interface), new BasicHttpBinding (), "temp2");

			h.Description.Behaviors.Add (new MyServiceBehavior (st, h));			
			
			h.Description.Endpoints [0].Behaviors.Add (new MyEndpointBehavior (st, h));
			h.Description.Endpoints [0].Contract.Behaviors.Add (new MyContractBehavior (st, h));
			h.Description.Endpoints [0].Contract.Operations [0].Behaviors.Add (new MyOperationBehavior (st, h));

			h.Description.Endpoints [1].Behaviors.Add (new MyEndpointBehavior (st, h));
			h.Description.Endpoints [1].Contract.Behaviors.Add (new MyContractBehavior (st, h));
			h.Description.Endpoints [1].Contract.Operations [0].Behaviors.Add (new MyOperationBehavior (st, h));
			h.Open ();
			h.Close ();

			string expected = "Start, IServiceBehavior.Validate, IContractBehavior.Validate, IEndpointBehavior.Validate, IOperationBehavior.ApplyDispatchBehavior, IContractBehavior.Validate, IEndpointBehavior.Validate, IOperationBehavior.ApplyDispatchBehavior, IServiceBehavior.AddBindingParameters, IContractBehavior.AddBindingParameters, IEndpointBehavior.AddBindingParameters, IOperationBehavior.AddBindingParameters, IServiceBehavior.AddBindingParameters, IContractBehavior.AddBindingParameters, IEndpointBehavior.AddBindingParameters, IOperationBehavior.AddBindingParameters, IServiceBehavior.ApplyDispatchBehavior, IContractBehavior.ApplyDispatchBehavior, IEndpointBehavior.ApplyDispatchBehavior, IOperationBehavior.ApplyDispatchBehavior, IContractBehavior.ApplyDispatchBehavior, IEndpointBehavior.ApplyDispatchBehavior, IOperationBehavior.ApplyDispatchBehavior";
			Assert.AreEqual (expected, st.CurrentStage);
		}

		[Test]
		public void AddBaseAddress ()
		{
			var host = new Poker ();
			Assert.AreEqual (0, host.BaseAddresses.Count, "#1");
			host.DoAddBaseAddress (new Uri ("http://localhost:37564"));
			Assert.AreEqual (1, host.BaseAddresses.Count, "#1");
			host.DoAddBaseAddress (new Uri ("net.tcp://localhost:893"));
			Assert.AreEqual (2, host.BaseAddresses.Count, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddBaseAddress2 ()
		{
			var host = new Poker ();
			Assert.AreEqual (0, host.BaseAddresses.Count, "#1");
			host.DoAddBaseAddress (new Uri ("http://localhost:37564"));
			// http base address is already added.
			host.DoAddBaseAddress (new Uri ("http://localhost:893"));
		}

		[Test]
		public void AddServiceEndpointUri ()
		{
			var host = new ServiceHost (typeof (AllActions),
				new Uri ("http://localhost:37564"));
			var se = host.AddServiceEndpoint (typeof (AllActions),
				new BasicHttpBinding (), "foobar");
			Assert.AreEqual ("http://localhost:37564/foobar", se.Address.Uri.AbsoluteUri, "#1");
			Assert.AreEqual ("http://localhost:37564/foobar", se.ListenUri.AbsoluteUri, "#2");
		}

		[Test]
		public void AddServiceEndpointUri2 ()
		{
			var host = new ServiceHost (typeof (AllActions),
				new Uri ("http://localhost:37564"));
			var se = host.AddServiceEndpoint (typeof (AllActions),
				new BasicHttpBinding (), String.Empty);
			Assert.AreEqual ("http://localhost:37564/", se.Address.Uri.AbsoluteUri, "#1");
			Assert.AreEqual ("http://localhost:37564/", se.ListenUri.AbsoluteUri, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpointOnlyMex ()
		{
			var host = new ServiceHost (typeof (AllActions),
				new Uri ("http://localhost:37564"));
			host.Description.Behaviors.Add (new ServiceMetadataBehavior ());
			host.AddServiceEndpoint ("IMetadataExchange",
				new BasicHttpBinding (), "/wsdl");
			host.Open ();
			try {
				// to make sure that throwing IOE from here does not count.
				host.Close ();
			} catch {
			}
			Assert.Fail ("should not open");
		}

		[Test]
		public void RunDestinationUnreachableTest ()
		{
			RunDestinationUnreachableTest ("BasicHttp", new BasicHttpBinding ());
		}

		[Test]
		public void RunDestinationUnreachableTest2 ()
		{
			RunDestinationUnreachableTest ("CustomSoap12", new CustomBinding (new HttpTransportBindingElement ()));
		}

		void RunDestinationUnreachableTest (string label, Binding binding)
		{
			string address = "http://localhost:37564/";
			var host = OpenHost (address, binding);
			
			try {
				var client = new DestinationUnreachableClient (binding, address);
				client.NotImplementedOperation ();
				Assert.Fail (label + " ActionNotSupportedException is expected");
			} catch (ActionNotSupportedException) {
				// catching it instead of ExpectedException to distinguish errors at service side.
			} finally {
				host.Close ();
			}
		}
		
		ServiceHost OpenHost (string address, Binding binding)
		{
			var baseAddresses = new Uri[] { new Uri(address) };

			var host = new ServiceHost (typeof (DummyService), baseAddresses);
			host.AddServiceEndpoint (typeof (IDummyService), binding, new Uri ("", UriKind.Relative));
			host.Open ();
			return host;
		}

#if NET_4_0
		[Test]
		public void AddServiceEndpoint_Directly ()
		{
			var host = new ServiceHost (typeof (DummyService));
			var address = new EndpointAddress ("http://localhost:8080");
			var binding = new BasicHttpBinding ();
			var contract = ContractDescription.GetContract (typeof (IDummyService));
			host.AddServiceEndpoint (new ServiceEndpoint (contract, binding, address));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceEndpoint_Directly_ContractMismatch ()
		{
			var host = new ServiceHost (typeof (DummyService));
			var address = new EndpointAddress ("http://localhost:8080");
			var binding = new BasicHttpBinding ();
			var contract = ContractDescription.GetContract (typeof (INotImplementedService));
			host.AddServiceEndpoint (new ServiceEndpoint (contract, binding, address));
		}
#endif

		#region helpers

		public enum Stage
		{
		}

		public class HostState
		{
			public string CurrentStage = "Start";
		}

		public class MyServiceBehavior : IServiceBehavior
		{
			#region IServiceBehavior Members

			HostState _state;
			ServiceHost _host;
			public MyServiceBehavior (HostState state, ServiceHost h) {
				_state = state;
				_host = h;
			}

			public void AddBindingParameters (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, global::System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {				
				_state.CurrentStage += ", IServiceBehavior.AddBindingParameters";				
				bindingParameters.Add (this);
			}

			public void ApplyDispatchBehavior (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {				
				_state.CurrentStage += ", IServiceBehavior.ApplyDispatchBehavior";				
			}

			public void Validate (ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
				_state.CurrentStage += ", IServiceBehavior.Validate";
				Assert.AreEqual (_host.ChannelDispatchers.Count, 0);
			}

			#endregion
		}

		public class MyEndpointBehavior : IEndpointBehavior
		{
			#region IEndpointBehavior Members
			HostState _state;
			ServiceHost _host;
			public MyEndpointBehavior (HostState state, ServiceHost h) {
				_state = state;
				_host = h;
			}

			public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {
				Console.WriteLine ("IEndpointBehavior - AddBindingParameters " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IEndpointBehavior.AddBindingParameters";				
				bindingParameters.Add (this);
			}

			public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
				Console.WriteLine ("IEndpointBehavior - ApplyClientBehavior " + _host.ChannelDispatchers.Count);
			}

			public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) {
				Console.WriteLine ("IEndpointBehavior - ApplyDispatchBehavior " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IEndpointBehavior.ApplyDispatchBehavior";				
			}

			public void Validate (ServiceEndpoint endpoint) {
				Console.WriteLine ("IEndpointBehavior - Validate " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IEndpointBehavior.Validate";
				Assert.AreEqual (_host.ChannelDispatchers.Count, 0);				
			}

			#endregion
		}

		public class MyContractBehavior : IContractBehavior
		{
			#region IContractBehavior Members
			HostState _state;
			ServiceHost _host;
			public MyContractBehavior (HostState state, ServiceHost h) {
				_state = state;
				_host = h;
			}

			public void AddBindingParameters (ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {
				Console.WriteLine ("Contract - AddBindingParameters " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IContractBehavior.AddBindingParameters";				
				bindingParameters.Add (this);
			}

			public void ApplyClientBehavior (ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
				Console.WriteLine ("Contract - ApplyClientBehavior " + _host.ChannelDispatchers.Count);
			}

			public void ApplyDispatchBehavior (ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime) {
				Console.WriteLine ("Contract - ApplyDispatchBehavior " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IContractBehavior.ApplyDispatchBehavior";				
			}

			public void Validate (ContractDescription contractDescription, ServiceEndpoint endpoint) {
				Console.WriteLine ("Contract - Validate " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IContractBehavior.Validate";		
				Assert.AreEqual (_host.ChannelDispatchers.Count, 0);				
			}

			#endregion
		}

		public class MyOperationBehavior : IOperationBehavior
		{
			#region IOperationBehavior Members
			HostState _state;
			ServiceHost _host;
			public MyOperationBehavior (HostState state, ServiceHost h) {
				_state = state;
				_host = h;
			}

			public void AddBindingParameters (OperationDescription operationDescription, BindingParameterCollection bindingParameters) {
				Console.WriteLine ("IOperationBehavior - AddBindingParameters " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IOperationBehavior.AddBindingParameters";					
				bindingParameters.Add (this);
			}

			public void ApplyClientBehavior (OperationDescription operationDescription, ClientOperation clientOperation) {
				Console.WriteLine ("IOperationBehavior - ApplyClientBehavior " + _host.ChannelDispatchers.Count);
			}

			public void ApplyDispatchBehavior (OperationDescription operationDescription, DispatchOperation dispatchOperation) {
				Console.WriteLine ("IOperationBehavior - ApplyDispatchBehavior " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IOperationBehavior.ApplyDispatchBehavior";				
			}

			public void Validate (OperationDescription operationDescription) {
				Console.WriteLine ("IOperationBehavior - Validate " + _host.ChannelDispatchers.Count);
				_state.CurrentStage += ", IOperationBehavior.ApplyDispatchBehavior";
				Assert.AreEqual (_host.ChannelDispatchers.Count, 0);
			}

			#endregion
		}

		[ServiceContract]
		class AllActions
		{
			[OperationContract (Action = "*", ReplyAction = "*")]
			public SMMessage Get (SMMessage req) {
				return null;
			}
		}

		[ServiceContract]
		interface Action1Interface
		{
			[OperationContract (Action = "Specific1", ReplyAction = "*")]
			SMMessage GetMessage1 (SMMessage req);
		}

		[ServiceContract]
		interface Action2Interface
		{
			[OperationContract (Action = "Specific2", ReplyAction = "*")]
			SMMessage GetMessage2 (SMMessage req);
		}
		
		class SpecificAction : Action1Interface, Action2Interface
		{			
			public SMMessage GetMessage1 (SMMessage req) {
				return null;
			}

			public SMMessage GetMessage2 (SMMessage req) {
				return null;
			}
		}

		[ServiceContract]
		class SpecificAction2
		{
			[OperationContract (Action = "Specific", ReplyAction = "*")]
			public SMMessage GetMessage1 (SMMessage req) {
				return null;
			}
		}

		class MyChannelDispatcher : ChannelDispatcher
		{
			public bool Attached = false;

			public MyChannelDispatcher (IChannelListener l) : base (l) { }
			protected override void Attach (ServiceHostBase host) {
				base.Attach (host);
				Attached = true;
			}
		}

		class MyChannelListener : IChannelListener
		{
			#region IChannelListener Members

			public IAsyncResult BeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public bool EndWaitForChannel (IAsyncResult result) {
				throw new NotImplementedException ();
			}

			public T GetProperty<T> () where T : class {
				throw new NotImplementedException ();
			}

			public Uri Uri {
				get { throw new NotImplementedException (); }
			}

			public bool WaitForChannel (TimeSpan timeout) {
				throw new NotImplementedException ();
			}

			#endregion

			#region ICommunicationObject Members

			public void Abort () {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginClose (TimeSpan timeout, AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginClose (AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginOpen (TimeSpan timeout, AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginOpen (AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public void Close (TimeSpan timeout) {
				throw new NotImplementedException ();
			}

			public void Close () {
				throw new NotImplementedException ();
			}

			public event EventHandler Closed;

			public event EventHandler Closing;

			public void EndClose (IAsyncResult result) {
				throw new NotImplementedException ();
			}

			public void EndOpen (IAsyncResult result) {
				throw new NotImplementedException ();
			}

			public event EventHandler Faulted;

			public void Open (TimeSpan timeout) {
				throw new NotImplementedException ();
			}

			public void Open () {
				throw new NotImplementedException ();
			}

			public event EventHandler Opened;

			public event EventHandler Opening;

			public CommunicationState State {
				get { throw new NotImplementedException (); }
			}

			#endregion
		}

		[ServiceContract]
		public interface IDummyService
		{
			[OperationContract]
			void DummyOperation ();
		}
		public class DummyService : IDummyService
		{
			public void DummyOperation ()
			{
				// Do nothing
			}
		}
		[ServiceContract]
		public interface INotImplementedService
		{
			[OperationContract]
			void NotImplementedOperation ();
		}
		public class DestinationUnreachableClient : ClientBase<INotImplementedService>, INotImplementedService
		{
			public void NotImplementedOperation ()
			{
				Channel.NotImplementedOperation ();
			}
		
			public DestinationUnreachableClient (Binding binding, string address) 
				: base (binding, new EndpointAddress (address))
			{
			}
		}

		#endregion
	}
}
