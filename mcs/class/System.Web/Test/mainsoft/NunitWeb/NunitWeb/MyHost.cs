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
		public const string CALL_CONTEXT_METHOD = "MyHostMethod";
		public const string CALL_CONTEXT_PARAM = "MyHostParam";
		public const string CALL_CONTEXT_EXCEPTION = "MyHostException";
		public const string HELPER_INSTANCE_NAME = "mainsoft/NunitWeb/Helper";
		
		public void Initialize (Helper h)
		{
			AppDomain.CurrentDomain.SetData (HELPER_INSTANCE_NAME, h);
		}

		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }

		public string DoRun (string url, Delegate method, object param)
		{
			CallContext.SetData (CALL_CONTEXT_METHOD, method);
			CallContext.SetData (CALL_CONTEXT_PARAM, param);
			try {
				using (StringWriter tw = new StringWriter ()) {
					SimpleWorkerRequest sr = new SimpleWorkerRequest (
						url, null, tw);
					HttpRuntime.ProcessRequest (sr);
					tw.Close ();
					string res = tw.ToString ();
					Exception inner = CallContext.GetData (CALL_CONTEXT_EXCEPTION) as Exception;
					RethrowException (inner);
					if (CallContext.GetData (CALL_CONTEXT_METHOD) != null)
						throw new Exception ("ProcessRequest did not reach RunDelegate");
										
					return res;
				}
			}
			finally {
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_METHOD);
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_PARAM);
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_EXCEPTION);
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
			object param = CallContext.GetData (MyHost.CALL_CONTEXT_PARAM);
			try {
				Delegate method = (Delegate) CallContext.GetData (CALL_CONTEXT_METHOD);
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_METHOD);
				if (method == null)
					return;
				Helper.AnyMethodInPage amip = method as Helper.AnyMethodInPage;
				if (amip != null) {
					amip (context, page, param);
					return;
				}

				Helper.AnyMethod am = method as Helper.AnyMethod;
				if (am != null) {
					am (context, param);
					return;
				}

				throw new ArgumentException ("method must be AnyMethod or AnyMethodInPage");
			}
			catch (Exception ex) {
				CallContext.SetData (MyHost.CALL_CONTEXT_EXCEPTION, ex);
			}
		}
	}
}
#endif
