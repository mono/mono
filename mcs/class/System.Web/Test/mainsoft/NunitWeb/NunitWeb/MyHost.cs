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

namespace MonoTests.SystemWeb.Framework
{
	internal class MyHost : MarshalByRefObject
	{
		#region MyData
		class MyData
		{
			public BaseInvoker invoker;
			public Exception exception;
			public bool invoked;
		}
		#endregion
		
		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }
		
		public Response Run (BaseInvoker invoker, BaseRequest request)
		{
			HttpWorkerRequest wr = request.CreateWorkerRequest ();
			IDictionary d = (IDictionary) wr;

			MyData data = new MyData ();
			data.invoker = invoker;
			data.exception = null;
			data.invoked = false;
			d[GetType ()] = data;

			HttpRuntime.ProcessRequest (wr);
			Response res = request.ExtractResponse (wr);

			if (data.exception != null)
				RethrowException (data.exception);

			if (!data.invoked)
				throw new Exception ("internal error: ProcessRequest did not reach WebTest.Invoke; response was: " + res.Body);

			return res;
		}

		public void Invoke (object param)
		{
			HttpWorkerRequest wr = GetMyWorkerRequest (HttpContext.Current);
			MyData data = (MyData) ((IDictionary) wr)[GetType ()];
			data.invoked = true;
			try {
				 data.invoker.DoInvoke (param);
			}
			catch (Exception ex) {
				data.exception = ex;
				throw;
			}
		}

		private static void RethrowException (Exception inner)
		{
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

		private static HttpWorkerRequest GetMyWorkerRequest (HttpContext c)
		{
			IServiceProvider isp = (IServiceProvider) c;
			return (HttpWorkerRequest) (isp.GetService (typeof (HttpWorkerRequest)));
		}
	}
}
