using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;
using System.IO;
using System.Configuration;
using System.Web.Configuration;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.SystemWeb.Framework
{
	internal class MyHost : MarshalByRefObject
	{
		AutoResetEvent _done;
		AutoResetEvent _doNext;
		WebTest _currentTest;
		Exception _e;

		#region MyData
		class MyData
		{
			public WebTest currentTest;
			public Exception exception;
		}
		#endregion
		
		public MyHost ()
		{
			_done = new AutoResetEvent (false);
			_doNext = new AutoResetEvent (false);
			ThreadPool.QueueUserWorkItem (new WaitCallback (param => {
				try {
					AsyncRun (param);
				} catch {}
				}), null);
		}

		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }
		
		public WebTest Run (WebTest t)
		{
			_currentTest = t;
			_doNext.Set ();
			_done.WaitOne ();
			if (_e != null) {
				Exception e = _e;
				_e = null;
				throw e;
			}
			return t;
		}

		void AsyncRun (object param)
		{
			for (;;) {
			_doNext.WaitOne ();
			try {
			WebTest t = _currentTest;
			HttpWorkerRequest wr = t.Request.CreateWorkerRequest ();
			MyData data = GetMyData (wr);
			data.currentTest = t;
			data.exception = null;

			HttpRuntime.ProcessRequest (wr);
			t.Response = t.Request.ExtractResponse (wr);
			
			if (data.exception != null)
				RethrowException (data.exception);
			} catch (Exception e) {
				_e = e;
			}

			_done.Set ();
			}
		}

		private static void RethrowException (Exception inner)
		{
			Exception serializableInner = FindSerializableInner (inner);
			if (serializableInner != inner) {
				throw new Exception ("Cannot serialize exception of type " + inner.GetType ().Name,
					serializableInner);
			}
			Exception outer;
			try { //Try create a similar exception and keep the inner intact
				outer = (Exception) Activator.CreateInstance (inner.GetType (),
					new object []{inner.ToString (), inner});
			}
			catch { //Failed to create a similar, fallback to the inner, ruining the call stack
				throw inner;
			}
			throw outer;
		}

		private static Exception FindSerializableInner (Exception inner)
		{
			//FIXME: what can be a less expansive, but equally reliable
			//check that exception can pass remoting?
			Exception ex = inner;
			Exception mostInner = null;
			while (ex != null) {
				try {
					BinaryFormatter f = new BinaryFormatter ();
					f.Serialize (new MemoryStream (), ex);
					//serialization succeeded, return it
					return ex;
				}
				catch (SerializationException) {
					mostInner = ex;
					ex = ex.InnerException;
				}
			}
			//no inner exceptions remain, create one with message and stack of the most inner
			ex = new Exception (mostInner.Message + " Call stack: " + mostInner.StackTrace);
			return ex;
		}

		private static HttpWorkerRequest GetMyWorkerRequest ()
		{
			IServiceProvider isp = HttpContext.Current as IServiceProvider;
			if (isp == null)
				return null;
			return (HttpWorkerRequest) (isp.GetService (typeof (HttpWorkerRequest)));
		}

		private static MyData GetMyData (HttpWorkerRequest wr)
		{
			IForeignData fd = wr as IForeignData;
			if (fd == null)
				throw new ArgumentException ("Invalid worker request. Probable reason is using WebTest.Invoke from a real web application");
			MyData d = (MyData) fd[typeof (MyHost)];
			if (d == null) {
				d = new MyData ();
				fd[typeof (MyHost)] = d;
			}
			return d;
		}

		public static WebTest GetCurrentTest ()
		{
			HttpWorkerRequest hwr = GetMyWorkerRequest ();
			if (hwr == null)
				return null;
			return GetMyData (hwr).currentTest;
		}

		public void RegisterException (Exception ex)
		{
			MyData data = GetMyData (GetMyWorkerRequest());
			data.exception = ex;
		}

		internal void SendHeaders (WebTest webTest)
		{
			//nothing to do in this host
		}
	}
}
