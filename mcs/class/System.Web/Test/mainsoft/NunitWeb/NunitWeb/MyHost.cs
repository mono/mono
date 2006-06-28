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
			public WebTest currentTest;
			public Exception exception;
		}
		#endregion
		
		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }
		
		public WebTest Run (WebTest t)
		{
			HttpWorkerRequest wr = t.Request.CreateWorkerRequest ();
			MyData data = GetMyData (wr);
			data.currentTest = t;
			data.exception = null;

			HttpRuntime.ProcessRequest (wr);
			t.Response = t.Request.ExtractResponse (wr);
			
			if (data.exception != null)
				RethrowException (data.exception);

			return t;
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

		private static HttpWorkerRequest GetMyWorkerRequest ()
		{
			IServiceProvider isp = HttpContext.Current as IServiceProvider;
			if (isp == null)
				return null;
			return (HttpWorkerRequest) (isp.GetService (typeof (HttpWorkerRequest)));
		}

		private static MyData GetMyData (HttpWorkerRequest wr)
		{
			IForeignData fd = (IForeignData) wr;
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
	}
}
