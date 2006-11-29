using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web.Configuration;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections.Specialized;

namespace MonoTests.SystemWeb.Framework
{
	internal class MyHost : MarshalByRefObject
	{
		public const string INVOKER_HEADER = "NunitWebInvoker";
		public const string USER_HEADER = "NunitWebUserData";
		public const string EXCEPTION_HEADER = "NunitWebException";
		private const string CURRENT_WEBTEST = "NunitWebCurrentTest";
		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }
		
		public static string Serialize (object o)
		{
			if (o == null)
				return string.Empty;
			using (MemoryStream ms = new MemoryStream ()) {
				SoapFormatter f = new SoapFormatter ();
				try {
					f.Serialize (ms, o);
				}
				catch (Exception ex) {
					Exception inner = o as Exception;
					if (inner != null)
						RethrowException (inner);
					else
						throw;
				}
				return HttpUtility.UrlEncode (ms.ToArray());
			}
		}

		public static object Deserialize (string s)
		{
			if (s == null || s == string.Empty)
				return null;
			using (MemoryStream ms = new MemoryStream ()) {
				byte [] ba = HttpUtility.UrlDecodeToBytes (s);
				ms.Write (ba, 0, ba.Length);
				ms.Position = 0;
				SoapFormatter b = new SoapFormatter ();
				try {
					return b.Deserialize (ms);
				}
				catch (Exception e) {
					throw;
				}
			}
		}

		public WebTest Run (WebTest t)
		{
			NameValueCollection headers = new NameValueCollection ();
			headers.Add (INVOKER_HEADER, Serialize (t.Invoker));
			headers.Add (USER_HEADER, Serialize (t.UserData));
			WebRequest wr = t.Request.CreateWebRequest (
#if TARGET_JVM
				new Uri ("http://localhost:8090/MainsoftWebApp20/"),
#else
new Uri ("http://localhost:59598/NunitWebTest/"),
#endif
				headers);


			WebResponse response = null;
			try {
				try {
					response = wr.GetResponse ();
				}
				catch (WebException we) {
					//StreamReader sr = new StreamReader (we.Response.GetResponseStream ());
					//throw new WebException (we.Message + Environment.NewLine
					//        + "Response:" + Environment.NewLine + sr.ReadToEnd (), we);
					response = we.Response;
					if (((HttpWebResponse) response).StatusCode == HttpStatusCode.OK)
						throw;//so we don't have a false positive test
				}

				Exception e = (Exception) Deserialize (
					response.Headers[EXCEPTION_HEADER]);
				if (e != null)
					RethrowException (e);

				t.UserData = Deserialize (response.Headers[USER_HEADER]);
					t.Response = t.Request.ExtractResponse (response);
			}
			finally {
				if (response != null)
					response.Close ();
			}

			return t;
		}

		public void SendHeaders (WebTest t)
		{
			HttpContext.Current.Response.AppendHeader (USER_HEADER, Serialize(t.UserData));
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

		public static WebTest GetCurrentTest ()
		{
			WebTest wt = HttpContext.Current.Items[CURRENT_WEBTEST] as WebTest;
			if (wt != null)
				return wt;
			wt = new WebTest ();
			wt.Invoker = (BaseInvoker) Deserialize (
				HttpContext.Current.Request.Headers [INVOKER_HEADER]);
			if (wt.Invoker == null)
				return null;
			wt.UserData = Deserialize (
				HttpContext.Current.Request.Headers [USER_HEADER]);
			HttpContext.Current.Items[CURRENT_WEBTEST] = wt;
			return wt;
		}

		public void RegisterException (Exception ex)
		{
			if (ex == null)
				return;
			if (HttpContext.Current.Items[EXCEPTION_HEADER] != null)
				return; //register only the first exception
			HttpContext.Current.Response.AddHeader (EXCEPTION_HEADER,
				Serialize (ex));
			HttpContext.Current.Items[EXCEPTION_HEADER] = ex;
		}
	}
}
