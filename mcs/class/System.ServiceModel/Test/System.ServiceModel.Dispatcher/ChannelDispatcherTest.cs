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

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class ChannelDispatcherTest
	{
		Uri CreateAvailableUri (string uriString)
		{
			var uri = new Uri (uriString);
			try {
				var t = new TcpListener (uri.Port);
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
			var cd = new ChannelDispatcher (new MyChannelListener<IReplyChannel> (new Uri ("urn:foo")));
			var st = cd.ServiceThrottle;
			Assert.IsNull (st, "#0");

			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			h.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
			h.ChannelDispatchers.Add (cd);
			Assert.IsNull (st, "#1");
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			Assert.IsNull (ed.ChannelDispatcher, "#1-2");
			ed.DispatchRuntime.Type = typeof (TestContract);
			cd.Endpoints.Add (ed);
			Assert.AreEqual (cd, ed.ChannelDispatcher, "#1-3");
			cd.MessageVersion = MessageVersion.Default;

			{
				cd.Open (TimeSpan.FromSeconds (10));
				try {
					Assert.IsNull (st, "#2");
					// so, can't really test actual slot values as it is null.
				} finally {
					cd.Close (TimeSpan.FromSeconds (10));
				}
				return;
			}
		}

		[Test]			
		public void Collection_Add_Remove () {
			Console.WriteLine ("STart test Collection_Add_Remove");
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			h.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			h.ChannelDispatchers.Add (d);
			Assert.IsTrue (d.Attached, "#1");
			h.ChannelDispatchers.Remove (d);
			Assert.IsFalse (d.Attached, "#2");
			h.ChannelDispatchers.Insert (0, d);
			Assert.IsTrue (d.Attached, "#3");
			h.ChannelDispatchers.Add (new MyChannelDispatcher (new MyChannelListener (uri)));
			h.ChannelDispatchers.Clear ();
			Assert.IsFalse (d.Attached, "#4");
		}

		[Test]
		public void EndpointDispatcherAddTest ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] 
		public void EndpointDispatcherAddTest2 () {
			var uri = CreateAvailableUri ("http://localhost:37564");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
			d.Open (); // the dispatcher must be attached.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest3 ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
			h.ChannelDispatchers.Add (d);
			d.Open (); // missing MessageVersion
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest4 ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var listener = new MyChannelListener (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			Assert.IsNotNull (ed.DispatchRuntime, "#1");
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#2");
			Assert.IsNull (ed.DispatchRuntime.InstanceContextProvider, "#3");
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#3.2");
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#4");
			d.Endpoints.Add (ed);
			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			// it misses DispatchRuntime.Type, which seems set
			// automatically when the dispatcher is created in
			// ordinal process but need to be set manually in this case.
			try {
				d.Open ();
				try {
					// should not reach here, but in case it didn't, it must be closed.
					d.Close (TimeSpan.FromSeconds (10));
				} catch {
				}
			} finally {
				Assert.AreEqual (CommunicationState.Opened, listener.State, "#5");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest5 ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract); // different from Test4

			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			// It rejects "unrecognized type" of the channel listener.
			// Test6 uses IChannelListener<IReplyChannel> and works.
			d.Open ();
			// should not reach here, but in case it didn't, it must be closed.
			d.Close (TimeSpan.FromSeconds (10));
		}

		[Test]
		public void EndpointDispatcherAddTest6 ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);
			Assert.IsFalse (d.Attached, "#x1");

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			Assert.IsTrue (d.Attached, "#x2");
			d.Open (); // At this state, it does *not* call AcceptChannel() yet.
			Assert.IsFalse (listener.AcceptChannelTried, "#1");
			Assert.IsFalse (listener.WaitForChannelTried, "#2");

			Assert.IsNotNull (ed.DispatchRuntime, "#3");
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#4");
			Assert.IsNull (ed.DispatchRuntime.InstanceContextProvider, "#5"); // it is not still set after ChannelDispatcher.Open().
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#5.2");
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#6");

			d.Close (); // we don't have to even close it.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest7 ()
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;

			// add service endpoint to open the host (unlike all tests above).
			h.AddServiceEndpoint (typeof (TestContract),
				new BasicHttpBinding (), uri.ToString ());
			h.ChannelDispatchers.Clear ();

			h.ChannelDispatchers.Add (d);
			d.Open (); // At this state, it does *not* call AcceptChannel() yet.

			// This rejects already-opened ChannelDispatcher.
			h.Open (TimeSpan.FromSeconds (10));
			// should not reach here, but in case it didn't, it must be closed.
			h.Close (TimeSpan.FromSeconds (10));
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
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;

			// add service endpoint to open the host (unlike all tests above).
			h.AddServiceEndpoint (typeof (TestContract),
				new BasicHttpBinding (), uri.ToString ());
			h.ChannelDispatchers.Clear ();

			h.ChannelDispatchers.Add (d);

			Assert.AreEqual (h, d.Host, "#0");

			try {
				h.Open (TimeSpan.FromSeconds (10));
				Assert.AreEqual (3, h.ChannelDispatchers.Count, "#0"); // TestContract, d, mex
				Assert.IsTrue (listener.BeginAcceptChannelTried, "#1"); // while it throws NIE ...
				Assert.IsFalse (listener.WaitForChannelTried, "#2");
				Assert.IsNotNull (ed.DispatchRuntime, "#3");
				Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#4");
				Assert.IsNotNull (ed.DispatchRuntime.InstanceContextProvider, "#5"); // it was set after ServiceHost.Open().
				Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#6");
				/*
				var l = new HttpListener ();
				l.Prefixes.Add (uri.ToString ());
				l.Start ();
				l.Stop ();
				*/
			} finally {
				h.Close (TimeSpan.FromSeconds (10));
				h.Abort ();
			}
		}

		// FIXME: this test itself indeed passes, but some weird conflict that blocks correspoding port happens between this and somewhere (probably above)
//		[Test]
		public void EndpointDispatcherAddTest9 () // test singleton service
		{
			var uri = CreateAvailableUri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (new TestContract (), uri);
			h.Description.Behaviors.Find<ServiceBehaviorAttribute> ().InstanceContextMode = InstanceContextMode.Single;
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);
			ed.DispatchRuntime.Type = typeof (TestContract);
			d.MessageVersion = MessageVersion.Default;
			h.AddServiceEndpoint (typeof (TestContract), new BasicHttpBinding (), uri.ToString ());
			h.ChannelDispatchers.Clear ();
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#1");
			h.ChannelDispatchers.Add (d);
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#2");
			try {
				h.Open (TimeSpan.FromSeconds (10));
				Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#4");
				Assert.IsNotNull (ed.DispatchRuntime.InstanceContextProvider, "#5"); // it was set after ServiceHost.Open().
				Assert.IsNotNull (ed.DispatchRuntime.SingletonInstanceContext, "#6");
			} finally {
				h.Close (TimeSpan.FromSeconds (10));
				h.Abort ();
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
