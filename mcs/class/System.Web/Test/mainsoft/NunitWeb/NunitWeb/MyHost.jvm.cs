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

		public static string Serialize (object o) {
			return Serialize (o, new SoapFormatter ());
		}
		public static string SerializeBinary (object o) {
			return Serialize (o, new SoapFormatter ());
		}
		public static string Serialize (object o, IFormatter f)
		{
			if (o == null)
				return string.Empty;
			using (MemoryStream ms = new MemoryStream ()) {
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
				return HttpUtility.UrlEncode (ms.ToArray ());
			}
		}

		public static object Deserialize (string s) {
			return Deserialize (s, new SoapFormatter ());
		}
		public static object DeserializeBinary (string s) {
			return Deserialize (s, new SoapFormatter ());
		}
		public static object Deserialize (string s, IFormatter b)
		{
			if (s == null || s == string.Empty)
				return null;
			using (MemoryStream ms = new MemoryStream ()) {
				byte [] ba = HttpUtility.UrlDecodeToBytes (s);
				ms.Write (ba, 0, ba.Length);
				ms.Position = 0;
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
				new Uri ("http://localhost:8080/MainsoftWebApp20/"),
#else
new Uri ("http://localhost:59598/NunitWebTest/"),
#endif
				headers);


			WebResponse response = null;
			try {
				
				response = wr.GetResponse ();
				HttpStatusCode status = ((HttpWebResponse) response).StatusCode;
				if (status != HttpStatusCode.OK && status != HttpStatusCode.Found)
					throw new WebException (((HttpWebResponse) response).StatusCode.ToString ());

				t.Response = t.Request.ExtractResponse (response);
				string etype = response.Headers [EXCEPTION_HEADER];

				if (!String.IsNullOrEmpty (etype)) {
					Exception e;
					string data = t.Response.Body;
					int start = data.IndexOf (EXCEPTION_HEADER);
					if (start >= 0) {
						start += EXCEPTION_HEADER.Length;
						int end = data.IndexOf (EXCEPTION_HEADER, start);
						int length = int.Parse(data.Substring(start, end - start));

						string serialized = data.Substring (end + EXCEPTION_HEADER.Length, length);
						e = (Exception) DeserializeBinary (serialized);
					}
					else
						e = (Exception) Activator.CreateInstance (Type.GetType(etype));

					if (e != null)
						RethrowException (e);
				}


				t.UserData = Deserialize (response.Headers [USER_HEADER]);
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
			throw inner;
		}

		public static WebTest GetCurrentTest ()
		{
			if (HttpContext.Current == null)
				return null;

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
			//bug #6946
			if (ex is TargetInvocationException)
				ex = ex.InnerException;
			if (HttpContext.Current.Items[EXCEPTION_HEADER] != null)
				return; //register only the first exception
			HttpContext.Current.Response.AddHeader (EXCEPTION_HEADER,
				ex.GetType().FullName);
			HttpContext.Current.Response.Write (EXCEPTION_HEADER);
			string serialized = SerializeBinary (ex);
			HttpContext.Current.Response.Write (serialized.Length);
			HttpContext.Current.Response.Write (EXCEPTION_HEADER);
			HttpContext.Current.Response.Write (serialized);
			HttpContext.Current.Items[EXCEPTION_HEADER] = ex;
		}
	}
}
