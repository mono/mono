using System;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Collections;
using System.Reflection;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Overrides <seealso cref="System.Web.Hosting.SimpleWorkerRequest"/> to provide
	/// access to user-agent header and to implement <seealso cref="IForeignData"/>
	/// interface.
	/// </summary>
	public class BaseWorkerRequest : SimpleWorkerRequest, IForeignData
	{
		string _userAgent;
		/// <summary>
		/// Create worker request with given page, query, writer and user agent.
		/// </summary>
		/// <param name="page">The URL of the page.</param>
		/// <param name="query">The request query string.</param>
		/// <param name="writer">The <seealso cref="System.IO.TextWriter"/> used to write HTTP response.</param>
		/// <param name="userAgent">The value of the user-agent HTTP header.</param>
		public BaseWorkerRequest (string page, string query, TextWriter writer, string userAgent)
			: base (page, query, writer)
		{
			_userAgent = userAgent;
		}

		/// <summary>
		/// Overriden to return the custom user-agent.
		/// </summary>
		/// <param name="index">Header index, as defined by <seealso cref="System.Web.HttpWorkerRequest"/></param>
		/// <returns></returns>
		public override string GetKnownRequestHeader(int index) {
			switch (index) {
			case HttpWorkerRequest.HeaderUserAgent:
				return _userAgent;
			}
			return base.GetKnownRequestHeader (index);
		}

		Hashtable foreignData = new Hashtable ();
		object IForeignData.this [Type type] {
			get {return foreignData[type];}
			set {
				if (value == null)
					foreignData.Remove (type);
				else
					foreignData[type] = value;
			}
		}
	}
}
