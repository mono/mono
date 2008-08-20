using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class ChannelDispatcherTest
	{
		[Test]			
		public void Collection_Add_Remove () {
			Console.WriteLine ("STart test Collection_Add_Remove");
			ServiceHost h = new ServiceHost (typeof (TestContract), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener ());
			h.ChannelDispatchers.Add (d);
			Assert.IsTrue (d.Attached, "#1");
			h.ChannelDispatchers.Remove (d);
			Assert.IsFalse (d.Attached, "#2");
			h.ChannelDispatchers.Insert (0, d);
			Assert.IsTrue (d.Attached, "#3");
			h.ChannelDispatchers.Add (new MyChannelDispatcher (new MyChannelListener ()));
			h.ChannelDispatchers.Clear ();
			Assert.IsFalse (d.Attached, "#4");
		}

		[Test]
		[Category("NotWorking")]
		public void EndpointDispatcherAddTest () {
			ServiceHost h = new ServiceHost (typeof (TestContract), new Uri ("http://localhost:8080"));
			h.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener ());			
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (""), "", ""));
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
	}
}
