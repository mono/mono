using System;
using System.IO;
using System.Web;
using System.Collections;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// The base request container. Provides access to <see cref="Url"/> and
	/// <see cref="UserAgent"/> and creates <see cref="BaseWorkerRequest"/>
	/// in web appdomain.
	/// </summary>
	/// <seealso cref="Url"/>
	/// <seealso cref="UserAgent"/>
	/// <seealso cref="BaseWorkerRequest"/>
	[Serializable]
	public class BaseRequest
	{
		string _url;

		/// <summary>
		/// The default constructor. Does nothing.
		/// </summary>
		public BaseRequest ()
		{
		}

		/// <summary>
		/// Creates instance of <see cref="BaseRequest"/> and initializes
		/// <see cref="Url"/> property.
		/// </summary>
		/// <param name="url">The initial value of <see cref="Url"/> property.</param>
		/// <seealso cref="Url"/>
		public BaseRequest (string url)
			: this ()
		{
			this._url = url;
		}

		/// <summary>
		/// The URL to make the request to.
		/// </summary>
		public virtual string Url
		{
			get { return _url; }
			set { _url = value; }
		}

		string _userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)";
		/// <summary>
		/// The user-agent HTTP header string. By default is initialized by the same 
		/// string as sent by Internet Explorer 6.
		/// </summary>
		public virtual string UserAgent
		{
			get { return _userAgent; }
			set { _userAgent = value; }
		}

		/// <summary>
		/// Create a new <see cref="HttpWorkerRequest"/> from data contained in this
		/// <see cref="BaseRequest"/>. The returned object must also implement
		/// <see cref="IForeignData"/> interface. <see cref="BaseRequest"/> stores
		/// the TextWriter in foreign data of the worker request, to use it later in
		/// <see cref="ExtractResponse"/>.
		/// </summary>
		/// <returns>A new <see cref="HttpWorkerRequest"/>.</returns>
		/// <seealso cref="HttpWorkerRequest"/>
		/// <seealso cref="IForeignData"/>
		/// <seealso cref="HttpWorkerRequest"/>
		/// <seealso cref="ExtractResponse"/>
		public virtual HttpWorkerRequest CreateWorkerRequest ()
		{
			StringWriter wr = new StringWriter ();
			BaseWorkerRequest br = CreateBaseWorkerRequest (wr);
			((IForeignData) br) [GetType ()] = wr;
			return br;
		}

		/// <summary>
		/// This function is used by subclasses of <see cref="BaseRequest"/> to create a
		/// subclass of <see cref="BaseWorkerRequest"/>.
		/// </summary>
		/// <param name="wr">TextWriter that must be passed to <see cref="BaseWorkerRequest"/>.</param>
		/// <returns>A new instance of <see cref="BaseWorkerRequest"/>, created
		/// with <see cref="Url"/>, <see cref="QueryString"/> and <see cref="UserAgent"/>.</returns>
		/// <seealso cref="BaseWorkerRequest"/>
		/// <seealso cref="UserAgent"/>
		/// <seealso cref="Url"/>
		protected virtual BaseWorkerRequest CreateBaseWorkerRequest (TextWriter wr)
		{
			return new BaseWorkerRequest (Url, QueryString, wr, UserAgent);
		}

		/// <summary>
		/// The query string, passed to the constructor of <see cref="BaseWorkerRequest"/>.
		/// </summary>
		/// <seealso cref="BaseWorkerRequest"/>
		protected virtual string QueryString
		{
			get { return ""; }
		}

		/// <summary>
		/// Extracts the response from the completed <see cref="System.Web.HttpWorkerRequest"/>
		/// and returns a new <see cref="Response"/> instance.
		/// </summary>
		/// <param name="request">this must be the same request that was returned by
		/// CreateWorkerRequest</param>
		/// <returns>New <see cref="Response"/> instance, containing the results of the 
		/// request.</returns>
		/// <seealso cref="System.Web.HttpWorkerRequest"/>
		/// <seealso cref="Response"/>
		public virtual Response ExtractResponse (HttpWorkerRequest request)
		{
			BaseWorkerRequest br = (BaseWorkerRequest) request;
			IForeignData d = (IForeignData) br;
			TextWriter wr = (TextWriter) d[GetType ()];
			d[GetType ()] = null;
			wr.Close ();
			Response r = new Response ();
			r.Body = wr.ToString ();
			return r;
		}
	}
}
