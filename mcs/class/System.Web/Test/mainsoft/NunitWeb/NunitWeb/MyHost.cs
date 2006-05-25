#if NET_2_0
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

namespace NunitWeb
{
	public class MyHost: MarshalByRefObject
	{
		public const string HELPER_INSTANCE_NAME = "mainsoft/NunitWeb/Helper";

		public void Initialize (Helper h)
		{
			AppDomain.CurrentDomain.SetData (HELPER_INSTANCE_NAME, h);
		}

		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }

		public string DoRun (string url, Delegate method, object param)
		{
			using (StringWriter tw = new StringWriter ()) {
				MyWorkerRequest wr = new MyWorkerRequest (method, param, url, null, tw);
				HttpRuntime.ProcessRequest (wr);
				tw.Close ();
				string res = tw.ToString ();
				Exception inner = wr.Exception;
				RethrowException (inner);
				if (wr.DelegateInvoked)
					throw new Exception ("ProcessRequest did not reach RunDelegate");
				return res;
			}
		}

		private static void RethrowException (Exception inner)
		{
			if (inner == null)
				return;

			Exception outer;
			try { //Try create a similar exception and keep the inner intact
				outer = (Exception) Activator.CreateInstance (inner.GetType (),
					inner.Message, inner);
			}
			catch { //Failed to create a similar, fallback to the inner, ruining the call stack
				throw inner;
			}
			throw outer;
		}

		static public void RunDelegate (HttpContext context, Page page)
		{
			MyWorkerRequest wr = (MyWorkerRequest) ((IServiceProvider) context).GetService (
				typeof (HttpWorkerRequest));
			try {
				if (wr.Method == null)
					return;
				Helper.AnyMethodInPage amip = wr.Method as Helper.AnyMethodInPage;
				if (amip != null) {
					amip (context, page, wr.Param);
					return;
				}

				Helper.AnyMethod am = wr.Method as Helper.AnyMethod;
				if (am != null) {
					am (context, wr.Param);
					return;
				}

				throw new ArgumentException ("method must be AnyMethod or AnyMethodInPage");
			}
			catch (Exception ex) {
				wr.Exception = ex;
			}
		}
	}
}
#endif
