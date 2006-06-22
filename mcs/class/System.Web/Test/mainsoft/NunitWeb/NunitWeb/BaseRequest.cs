using System;
using System.IO;
using System.Web;
using System.Collections;

namespace MonoTests.SystemWeb.Framework
{
	public enum HttpVerb
	{
		Get, Post, Put, Delete
	}

	[Serializable]
	public class BaseRequest
	{
		string _url;

		public BaseRequest ()
		{
		}

		public BaseRequest (string url)
			: this ()
		{
			this._url = url;
		}

		public virtual string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>A new HttpWorkerRequest, which must implement also IDictionary</returns>
		/// 
		public virtual HttpWorkerRequest CreateWorkerRequest ()
		{
			StringWriter wr = new StringWriter ();
			BaseWorkerRequest br = CreateBaseWorkerRequest (wr);
			((IDictionary) br) [GetType ()] = wr;
			return br;
		}

		protected virtual BaseWorkerRequest CreateBaseWorkerRequest (StringWriter wr)
		{
			return new BaseWorkerRequest (Url, GetQueryString (), wr);
		}

		protected virtual string GetQueryString ()
		{
			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request">this must be the same request that was returned by
		/// CreateWorkerRequest</param>
		/// <returns></returns>
		public virtual Response ExtractResponse (HttpWorkerRequest request)
		{
			BaseWorkerRequest br = (BaseWorkerRequest) request;
			IDictionary d = (IDictionary) br;
			TextWriter wr = (TextWriter) d[GetType ()];
			d.Remove (GetType ());
			wr.Close ();
			Response r = new Response ();
			r.Body = wr.ToString ();
			return r;
		}
	}
}
