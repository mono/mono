/**
 * Namespace: System.Web
 * Class:     HttpServerUtility
 *
 * Author:  Wictor Wilén
 * Contact: <wictor@ibizkit.se>, <patrik.torstensson@labs2.com>
 * Status:  ?%
 *
 * (C) Wictor Wilén (2002)
 * ---------------------------------------
 * 2002-03-27  Wictor      Started implementation
 * 2002-04-09  Patrik      Added HttpContext constructor
 * 2002-04-10  Patrik      Moved encoding to HttpUtility and
 *                         fixed all functions that used
 *                         HttpContext
 * 
 */
using System;
using System.IO;

namespace System.Web
{
	public sealed class HttpServerUtility
	{
		private static string _name = "";

		private HttpContext _Context;
		private HttpApplication _Application;

		internal HttpServerUtility (HttpContext Context)
		{
			_Context = Context;
		}

		internal HttpServerUtility (HttpApplication app)
		{
			_Application = app;
		}

		// Properties


		/// <summary>
		/// Gets the server's computer name.
		/// </summary>
		public string MachineName {
			get {
				if(_name.Length == 0)
					_name = Environment.MachineName;

				return _name;
			}
		}

		/// <summary>
		/// Gets and sets the request time-out in seconds.
		/// </summary>
		[MonoTODO()]
		public int ScriptTimeout {
			get {
				throw new NotImplementedException ();
			} 
			set {
				throw new NotImplementedException ();
			}
		}

		// Methods

		/// <summary>
		/// Clears the previous exception.
		/// </summary>
		public void ClearError ()
		{
			if (null != _Context) {
				_Context.ClearError ();
				return;
			}

			if (null != _Application) {
				_Application.ClearError ();
			}
		}


		/// <summary>
		/// Creates a server instance of a COM object identified by the object's Programmatic Identifier (ProgID).
		/// </summary>
		/// <param name="progID">The class or type of object to be instantiated. </param>
		/// <returns>The new object.</returns>
		public object CreateObject (string progID)
		{
			return CreateObject (Type.GetTypeFromProgID (progID));
		}


		/// <summary>
		/// Creates a server instance of a COM object identified by the object's type.
		/// </summary>
		/// <param name="type">A Type representing the object to create. </param>
		/// <returns>The new object.</returns>
		[MonoTODO()]
		public object CreateObject (Type type)
		{
			Object o;
			o = Activator.CreateInstance (type);

			// TODO: Call OnStartPage()

			return o;
		}


		/// <summary>
		/// Creates a server instance of a COM object identified by the object's class identifier (CLSID).
		/// </summary>
		/// <param name="clsid">The class identifier of the object to be instantiated. </param>
		/// <returns>The new object.</returns>
		public object CreateObjectFromClsid (string clsid)
		{
			Guid guid = new Guid (clsid);
			return CreateObject (Type.GetTypeFromCLSID (guid));
		}


		/// <summary>
		/// Executes a request to another page using the specified URL path to the page.
		/// </summary>
		/// <param name="path">The URL path of the new request. </param>
		[MonoTODO()]
		public void Execute (string path)
		{
			throw new NotImplementedException ();
		}


		/// <summary>
		/// Executes a request to another page using the specified URL path to the page.
		/// A TextWriter captures output from the page.
		/// </summary>
		/// <param name="path">The URL path of the new request. </param>
		/// <param name="writer">The TextWriter to capture the output. </param>
		[MonoTODO()]
		public void Execute (string path, TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Returns the previous exception.
		/// </summary>
		/// <returns>The previous exception that was thrown.</returns>
		[MonoTODO()]
		public Exception GetLastError ()
		{
			throw new NotImplementedException ();
		}



		/// <summary>
		/// Decodes an HTML-encoded string and returns the decoded string.
		/// </summary>
		/// <param name="s">The HTML string to decode. </param>
		/// <returns>The decoded text.</returns>
		[MonoTODO()]
		public string HtmlDecode (string s)
		{
			return HttpUtility.HtmlDecode (s);
		}


		/// <summary>
		/// Decodes an HTML-encoded string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The HTML string to decode</param>
		/// <param name="output">The TextWriter output stream containing the decoded string. </param>
		public void HtmlDecode (string s, TextWriter output)
		{
			output.Write (HttpUtility.HtmlDecode (s));
		}

		/// <summary>
		/// HTML-encodes a string and returns the encoded string.
		/// </summary>
		/// <param name="s">The text string to encode. </param>
		/// <returns>The HTML-encoded text.</returns>
		public string HtmlEncode (string s)
		{
			return HttpUtility.HtmlEncode (s);
		}

		/// <summary>
		/// HTML-encodes a string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The string to encode. </param>
		/// <param name="output">The TextWriter output stream containing the encoded string. </param>
		public void HtmlEncode (string s, TextWriter output)
		{
			output.Write (HtmlEncode (s));
		}


		/// <summary>
		/// Returns the physical file path that corresponds to the specified virtual path on the Web server.
		/// </summary>
		/// <param name="path">The virtual path on the Web server. </param>
		/// <returns>The physical file path that corresponds to path.</returns>
		public string MapPath (string path)
		{
			if (null == _Context)
				throw new HttpException ("MapPath is not available");

			return _Context.Request.MapPath (path);
		}

		/// <summary>
		/// Terminates execution of the current page and begins execution of a new page using the specified
		/// URL path to the page.
		/// </summary>
		/// <param name="path">The URL path of the new page on the server to execute. </param>
		[MonoTODO()]
		public void Transfer (string path)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Terminates execution of the current page and begins execution of a new page using the specified
		/// URL path to the page. Specifies whether to clear the QueryString and Form collections.
		/// </summary>
		/// <param name="path">The URL path of the new page on the server to execute. </param>
		/// <param name="preserveForm">If true, the QueryString and Form collections are preserved. If false,
		/// they are cleared. The default is false. </param>
		[MonoTODO()]
		public void Transfer(string path, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// URL-decodes a string and returns the decoded string.
		/// </summary>
		/// <param name="s">The text string to decode. </param>
		/// <returns>The decoded text.</returns>
		/// <remarks>
		/// Post/html encoding @ ftp://ftp.isi.edu/in-notes/rfc1866.txt
		/// Uncomment the line marked with RFC1738 to get pure RFC1738
		/// and it will also consider the RFC1866 (ftp://ftp.isi.edu/in-notes/rfc1866.txt)
		/// `application/x-www-form-urlencoded' format
		/// </remarks>
		public string UrlDecode (string s)
		{
			return HttpUtility.UrlDecode (s);
		}

		/// <summary>
		/// Decodes an HTML string received in a URL and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="output"></param>
		public void UrlDecode (string s, TextWriter output)
		{
			output.Write (UrlDecode (s));
		}

		/// <summary>
		/// URL-encodes a string and returns the encoded string.
		/// </summary>
		/// <param name="s">The text to URL-encode. </param>
		/// <returns>The URL encoded text.</returns>
		public string UrlEncode (string s)
		{
			return HttpUtility.UrlEncode (s);
		}

		/// <summary>
		/// URL encodes a string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The text string to encode. </param>
		/// <param name="output">The TextWriter output stream containing the encoded string. </param>
		public void UrlEncode (string s, TextWriter output)
		{
			output.Write (UrlEncode (s));
		}

		/// <summary>
		/// URL-encodes the path portion of a URL string and returns the encoded string.
		/// </summary>
		/// <param name="s">The text to URL-encode.</param>
		/// <returns>The URL encoded text.</returns>
		/// <remarks>Does not do any browser specific adjustments, just encode everything</remarks>
		public string UrlPathEncode (string s)
		{
			// find the path portion (?)
			int idx = s.IndexOf ("?");
			string s2 = s.Substring (0, idx-1);
			s2 = UrlEncode (s2);
			s2 += s.Substring (idx);

			return s2;
		}
	}
}

