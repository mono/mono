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
		
		public void Initialize ()
		{
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
					Exception e = CallContext.GetData (CALL_CONTEXT_EXCEPTION) as Exception;
					if (e != null) {
						Exception outer = (Exception) Activator.CreateInstance (e.GetType (), 
							e.Message, e);
						throw outer;
					}
					tw.Close ();
					return tw.ToString ();
				}
			}
			finally {
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_METHOD);
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_PARAM);
				CallContext.FreeNamedDataSlot (CALL_CONTEXT_EXCEPTION);
			}
		}

		static public void RunDelegate (HttpContext context, Page page)
		{
			object param = CallContext.GetData (MyHost.CALL_CONTEXT_PARAM);
			try {
				Delegate method = (Delegate) CallContext.GetData (CALL_CONTEXT_METHOD);
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
