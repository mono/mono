
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
/**
 * Namespace: System.Web
 * Class:     HttpServerUtility
 *
 * Authosr:
 *	Wictor Wilén (wictor@ibizkit.se)
 *	Patrik Torstensson (patrik.torstensson@labs2.com)
 *	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 *
 * (c) Wictor Wilén (2002)
 * (c) 2002 Patrik Torstensson
 * (c) 2003 Ximian, Inc. (http://www.ximian.com)
 *
 * (This log is no longer maintained)
 * ---------------------------------------
 * 2002-03-27  Wictor      Started implementation
 * 2002-04-09  Patrik      Added HttpContext constructor
 * 2002-04-10  Patrik      Moved encoding to HttpUtility and
 *                         fixed all functions that used
 *                         HttpContext
 * 
 */
using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Hosting;
using System.Web.Util;

namespace System.Web
{
	public sealed class HttpServerUtility
	{
		private static string _name;

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
				if(_name == null)
					_name = Environment.MachineName;

				return _name;
			}
		}

		public int ScriptTimeout {
			get {
				return (int) _Context.ConfigTimeout.TotalSeconds;
			} 
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				_Context.ConfigTimeout = new TimeSpan (0, 0, value);
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
		public object CreateObject (Type type)
		{
			return Activator.CreateInstance (type);
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
		public void Execute (string path)
		{
			Execute (path, null);
		}


		/// <summary>
		/// Executes a request to another page using the specified URL path to the page.
		/// A TextWriter captures output from the page.
		/// </summary>
		/// <param name="path">The URL path of the new request. </param>
		/// <param name="writer">The TextWriter to capture the output. </param>
		public void Execute (string path, TextWriter writer)
		{
			Execute (path, writer, false);
		}

#if NET_2_0
		public
#else
		internal
#endif
		void Execute (string path, TextWriter writer, bool preserveQuery)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (_Context == null)
				throw new HttpException ("No context available.");

			if (path.IndexOf (':') != -1)
				throw new ArgumentException ("Invalid path.");

			int qmark = path.IndexOf ('?');
			string query;
			if (qmark != -1) {
				query = path.Substring (qmark + 1);
				path = path.Substring (0, qmark);
			} else {
				query = "";
			}
			
			string filePath = _Context.Request.MapPath (path);
			HttpResponse response = _Context.Response;
			TextWriter output = writer;
			if (output == null)
			 	output = response.Output;

			HttpRequest request = _Context.Request;
			string oldFilePath = request.FilePath;
			request.SetFilePath (UrlUtils.Combine (_Context.Request.BaseVirtualDir, path));
			string oldQuery = request.QueryStringRaw;
			
			if (!preserveQuery)	request.QueryStringRaw = query;
			IHttpHandler handler = _Context.ApplicationInstance.CreateHttpHandler (_Context,
											       request.RequestType,
											       path,
											       filePath);
			TextWriter previous = null;
			try {
				previous = response.SetTextWriter (output);
				if (!(handler is IHttpAsyncHandler)) {
					handler.ProcessRequest (_Context);
				} else {
					IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler) handler;
					IAsyncResult ar = asyncHandler.BeginProcessRequest (_Context, null, null);
					ar.AsyncWaitHandle.WaitOne ();
					asyncHandler.EndProcessRequest (ar);
				}
			} finally {
				request.SetFilePath (oldFilePath);
				request.QueryStringRaw = oldQuery;
				response.SetTextWriter (previous);
			}

		}

		/// <summary>
		/// Returns the previous exception.
		/// </summary>
		/// <returns>The previous exception that was thrown.</returns>
		public Exception GetLastError ()
		{
			if (_Context == null)
				return null;

			return _Context.Error;
		}



		/// <summary>
		/// Decodes an HTML-encoded string and returns the decoded string.
		/// </summary>
		/// <param name="s">The HTML string to decode. </param>
		/// <returns>The decoded text.</returns>
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
		public void Transfer (string path)
		{
			Transfer (path, true);
		}

		/// <summary>
		/// Terminates execution of the current page and begins execution of a new page using the specified
		/// URL path to the page. Specifies whether to clear the QueryString and Form collections.
		/// </summary>
		/// <param name="path">The URL path of the new page on the server to execute. </param>
		/// <param name="preserveForm">If true, the QueryString and Form collections are preserved. If false,
		/// they are cleared. The default is false. </param>
		public void Transfer (string path, bool preserveForm)
		{
			HttpValueCollection oldForm = null;
			if (!preserveForm) {
				oldForm = _Context.Request.Form as HttpValueCollection;
				_Context.Request.SetForm (new HttpValueCollection ());
			}

			Execute (path, null, preserveForm);
			if (!preserveForm)
				_Context.Request.SetForm (oldForm);

			_Context.Response.End ();
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
			if (s != null)
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
			if (s != null)
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
			if (s == null)
				return null;

			int idx = s.IndexOf ("?");
			string s2 = null;
			if (idx != -1) {
				s2 = s.Substring (0, idx-1);
				s2 = UrlEncode (s2) + s.Substring (idx);
			} else {
				s2 = UrlEncode (s);
			}

			return s2;
		}
	}
}
