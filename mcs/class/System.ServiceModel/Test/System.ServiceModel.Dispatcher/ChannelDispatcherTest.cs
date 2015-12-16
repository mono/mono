using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class ChannelDispatcherTest
	{
		Uri CreateAvailableUri (string uriString)
		{
			Uri uri = new Uri (uriString);
			try {
				TcpListener t = new TcpListener (uri.Port);
				t.Start ();
				t.Stop ();
			} catch (Exception ex) {
				Assert.Fail (String.Format ("Port {0} is not open. It is likely previous tests have failed and the port is kept opened", uri.Port));
			}
			return uri;
		}

		[Test]
		public void ConstructorNullBindingName ()
		{
			new ChannelDispatcher (new MyChannelListener (new Uri ("urn:foo")), null);
			new ChannelDispatcher (new MyChannelListener (new Uri ("urn:foo")), null, null);
		}

		[Test]
		public void ServiceThrottle ()
		{
			ChannelDispatcher channelDispatcher = new ChannelDispatcher (new MyChannelListener<IReplyChannel> (new Uri ("urn:foo")));
			Assert.IsNull (channelDispatcher.ServiceThrottle, "#0");

			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());

			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				serviceHost.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
				serviceHost.ChannelDispatchers.Add (channelDispatcher);
				Assert.IsNull (channelDispatcher.ServiceThrottle, "#1");

				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				Assert.IsNull (endpointDispatcher.ChannelDispatcher, "#1-2");

				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract);
				channelDispatcher.Endpoints.Add (endpointDispatcher);
				Assert.AreEqual (channelDispatcher, endpointDispatcher.ChannelDispatcher, "#1-3");

				channelDispatcher.MessageVersion = MessageVersion.Default;

				channelDispatcher.Open (TimeSpan.FromSeconds (10));
				try {
					Assert.IsNull (channelDispatcher.ServiceThrottle, "#2");
					// so, can't really test actual slot values as it is null.
				} finally {
					channelDispatcher.Close (TimeSpan.FromSeconds (10));
				}
			}
		}

		[Test]			
		public void Collection_Add_Remove () {
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());

			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				serviceHost.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");

				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (new MyChannelListener (uri));

				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				Assert.IsTrue (myChannelDispatcher.Attached, "#1");

				serviceHost.ChannelDispatchers.Remove (myChannelDispatcher);
				Assert.IsFalse (myChannelDispatcher.Attached, "#2");

				serviceHost.ChannelDispatchers.Insert (0, myChannelDispatcher);
				Assert.IsTrue (myChannelDispatcher.Attached, "#3");

				serviceHost.ChannelDispatchers.Add (new MyChannelDispatcher (new MyChannelListener (uri)));
				serviceHost.ChannelDispatchers.Clear ();
				Assert.IsFalse (myChannelDispatcher.Attached, "#4");
			}
		}

		[Test]
		public void EndpointDispatcherAddTest ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (new MyChannelListener (uri));
			myChannelDispatcher.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] 
		public void EndpointDispatcherAddTest2 () {
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (new MyChannelListener (uri));
			myChannelDispatcher.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
			myChannelDispatcher.Open (); // the dispatcher must be attached.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest3 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (new MyChannelListener (uri));
				myChannelDispatcher.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				myChannelDispatcher.Open (); // missing MessageVersion
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest4 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				MyChannelListener myChannelListener = new MyChannelListener (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");

				Assert.IsNotNull (endpointDispatcher.DispatchRuntime, "#1");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#2");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceContextProvider, "#3");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#3.2");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#4");

				myChannelDispatcher.Endpoints.Add (endpointDispatcher);
				myChannelDispatcher.MessageVersion = MessageVersion.Default;
				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				// it misses DispatchRuntime.Type, which seems set
				// automatically when the dispatcher is created in
				// ordinal process but need to be set manually in this case.
				try {
					myChannelDispatcher.Open ();
					try {
						// should not reach here, but in case it didn't, it must be closed.
						myChannelDispatcher.Close (TimeSpan.FromSeconds (10));
					} catch {
					}
				} finally {
					Assert.AreEqual (CommunicationState.Opened, myChannelDispatcher.State, "#5");
				}
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest5 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				BasicHttpBinding binding = new BasicHttpBinding ();
				MyChannelListener myChannelDispatcher = new MyChannelListener (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				myChannelDispatcher.Endpoints.Add (endpointDispatcher);

				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract); // different from Test4

				myChannelDispatcher.MessageVersion = MessageVersion.Default;
				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				// It rejects "unrecognized type" of the channel myChannelDispatcher.
				// Test6 uses IChannelListener<IReplyChannel> and works.
				myChannelDispatcher.Open ();
				// should not reach here, but in case it didn't, it must be closed.
				myChannelDispatcher.Close (TimeSpan.FromSeconds (10));
			}
		}

		[Test]
		public void EndpointDispatcherAddTest6 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				BasicHttpBinding binding = new BasicHttpBinding ();
				MyChannelListener myChannelDispatcher = new MyChannelListener<IReplyChannel> (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				myChannelDispatcher.Endpoints.Add (endpointDispatcher);

				Assert.IsFalse (myChannelDispatcher.Attached, "#x1");

				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract);

				myChannelDispatcher.MessageVersion = MessageVersion.Default;
				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				Assert.IsTrue (myChannelDispatcher.Attached, "#x2");

				myChannelDispatcher.Open (); // At this state, it does *not* call AcceptChannel() yet.
				Assert.IsFalse (myChannelDispatcher.AcceptChannelTried, "#1");
				Assert.IsFalse (myChannelDispatcher.WaitForChannelTried, "#2");

				Assert.IsNotNull (endpointDispatcher.DispatchRuntime, "#3");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#4");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceContextProvider, "#5"); // it is not still set after ChannelDispatcher.Open().
				Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#5.2");
				Assert.IsNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#6");

				myChannelDispatcher.Close (); // we don't have to even close it.
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest7 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				BasicHttpBinding binding = new BasicHttpBinding ();
				MyChannelListener myChannelDispatcher = new MyChannelListener<IReplyChannel> (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				myChannelDispatcher.Endpoints.Add (endpointDispatcher);

				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract);

				myChannelDispatcher.MessageVersion = MessageVersion.Default;

				// add service endpoint to open the host (unlike all tests above).
				serviceHost.AddServiceEndpoint (typeof (TestContract), new BasicHttpBinding (), uri.ToString ());
				serviceHost.ChannelDispatchers.Clear ();

				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				myChannelDispatcher.Open (); // At this state, it does *not* call AcceptChannel() yet.

				// This rejects already-opened ChannelDispatcher.
				serviceHost.Open (TimeSpan.FromSeconds (10));
				// should not reach here, but in case it didn't, it must be closed.
				serviceHost.Close (TimeSpan.FromSeconds (10));
			}
		}

		[Test]
		[Category ("NotWorking")]
		// Validating duplicate listen URI causes this regression.
		// Since it is niche, I rather fixed ServiceHostBase to introduce validation.
		// It is probably because this code doesn't use ServiceEndpoint to build IChannelListener i.e. built without Binding.
		// We can add an extra field to ChannelDispatcher to indicate that it is from ServiceEndpoint (i.e. with Binding),
		// but it makes little sense especially for checking duplicate listen URIs. Duplicate listen URIs should be rejected anyways.
		public void EndpointDispatcherAddTest8 ()
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (typeof (TestContract), uri)) {
				MyChannelListener myChannelDispatcher = new MyChannelListener<IReplyChannel> (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				myChannelDispatcher.Endpoints.Add (endpointDispatcher);

				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract);

				myChannelDispatcher.MessageVersion = MessageVersion.Default;

				// add service endpoint to open the host (unlike all tests above).
				serviceHost.AddServiceEndpoint (typeof (TestContract), new BasicHttpBinding (), uri.ToString ());
				serviceHost.ChannelDispatchers.Clear ();

				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);

				Assert.AreEqual (serviceHost, myChannelDispatcher.Host, "#0");

				try {
					serviceHost.Open (TimeSpan.FromSeconds (10));
					Assert.AreEqual (3, serviceHost.ChannelDispatchers.Count, "#0"); // TestContract, myChannelDispatcher, mex
					Assert.IsTrue (myChannelDispatcher.BeginAcceptChannelTried, "#1"); // while it throws NIE ...
					Assert.IsFalse (myChannelDispatcher.WaitForChannelTried, "#2");
					Assert.IsNotNull (endpointDispatcher.DispatchRuntime, "#3");
					Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#4");
					Assert.IsNotNull (endpointDispatcher.DispatchRuntime.InstanceContextProvider, "#5"); // it was set after ServiceHost.Open().
					Assert.IsNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#6");
					/*
					var l = new HttpListener ();
					l.Prefixes.Add (uri.ToString ());
					l.Start ();
					l.Stop ();
					*/
				} finally {
					serviceHost.Close (TimeSpan.FromSeconds (10));
					serviceHost.Abort ();
				}
			}
		}

		// FIXME: this test itself indeed passes, but some weird conflict that blocks correspoding port happens between this and somewhere (probably above)
//		[Test]
		public void EndpointDispatcherAddTest9 () // test singleton service
		{
			Uri uri = CreateAvailableUri ("http://localhost:" + NetworkHelpers.FindFreePort ());
			using (ServiceHost serviceHost = new ServiceHost (new TestContract (), uri)) {
				serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute> ().InstanceContextMode = InstanceContextMode.Single;
				MyChannelListener myChannelDispatcher = new MyChannelListener<IReplyChannel> (uri);
				MyChannelDispatcher myChannelDispatcher = new MyChannelDispatcher (myChannelDispatcher);
				EndpointDispatcher endpointDispatcher = new EndpointDispatcher (new EndpointAddress (uri), "", "");
				myChannelDispatcher.Endpoints.Add (endpointDispatcher);
				endpointDispatcher.DispatchRuntime.Type = typeof (TestContract);
				myChannelDispatcher.MessageVersion = MessageVersion.Default;
				serviceHost.AddServiceEndpoint (typeof (TestContract), new BasicHttpBinding (), uri.ToString ());
				serviceHost.ChannelDispatchers.Clear ();
				Assert.IsNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#1");
				serviceHost.ChannelDispatchers.Add (myChannelDispatcher);
				Assert.IsNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#2");
				try {
					serviceHost.Open (TimeSpan.FromSeconds (10));
					Assert.IsNull (endpointDispatcher.DispatchRuntime.InstanceProvider, "#4");
					Assert.IsNotNull (endpointDispatcher.DispatchRuntime.InstanceContextProvider, "#5"); // it was set after ServiceHost.Open().
					Assert.IsNotNull (endpointDispatcher.DispatchRuntime.SingletonInstanceContext, "#6");
				} finally {
					serviceHost.Close (TimeSpan.FromSeconds (10));
					serviceHost.Abort ();
				}
			}
		}

		[ServiceContract]
		public class TestContract
		{
			[OperationContract]
			public void Process (string input) {
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

			protected override void Detach (ServiceHostBase host) {
				base.Detach (host);
				Attached = false;				
			}
		}

		class MyChannelListener<TChannel> : MyChannelListener, IChannelListener<TChannel> where TChannel : class, IChannel
		{
			public MyChannelListener (Uri uri)
				: base (uri)
			{
			}

			public bool AcceptChannelTried { get; set; }
			public bool BeginAcceptChannelTried { get; set; }

			public TChannel AcceptChannel ()
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public TChannel AcceptChannel (TimeSpan timeout)
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginAcceptChannel (AsyncCallback callback, object state)
			{
				BeginAcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
			{
				BeginAcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public TChannel EndAcceptChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}
		}

		class MyChannelListener : IChannelListener
		{
			public MyChannelListener (Uri uri)
			{
				Uri = uri;
			}

			public bool WaitForChannelTried { get; set; }

			public CommunicationState State { get; set; }

			#region IChannelListener Members

			public IAsyncResult BeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
			{
				WaitForChannelTried = true;
				throw new NotImplementedException ();
			}

			public bool EndWaitForChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			public T GetProperty<T> () where T : class
			{
				throw new NotImplementedException ();
			}

			public Uri Uri { get; set; }

			public bool WaitForChannel (TimeSpan timeout)
			{
				WaitForChannelTried = true;
				throw new NotImplementedException ();
			}

			#endregion

			#region ICommunicationObject Members

			public void Abort ()
			{
				State = CommunicationState.Closed;
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

			public void Close (TimeSpan timeout)
			{
				State = CommunicationState.Closed;
			}

			public void Close ()
			{
				State = CommunicationState.Closed;
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

			public void Open (TimeSpan timeout)
			{
				State = CommunicationState.Opened;
			}

			public void Open () 
			{
				State = CommunicationState.Opened;
			}

			public event EventHandler Opened;

			public event EventHandler Opening;

			#endregion
		}
	}
}
