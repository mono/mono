// 
// System.Web.HttpException
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Patrik Torstensson
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Web.Util;
using System.Web.Compilation;

namespace System.Web
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	[Serializable]
#endif
	public class HttpException : ExternalException
	{
		int http_code = 500;
		const string errorStyleFonts = "\"Verdana\",\"DejaVu Sans\",sans-serif";
		
		public HttpException ()
		{
		}

		public HttpException (string message)
			: base (message)
		{
		}

		public HttpException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public HttpException (int httpCode, string message) : base (message)
		{
			http_code = httpCode;
		}

#if NET_2_0
		protected HttpException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			http_code = info.GetInt32 ("_httpCode");
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("_httpCode", http_code);
		}
#endif

		public HttpException (int httpCode, string message, int hr) 
			: base (message, hr)
		{
			http_code = httpCode;
		}

		public HttpException (string message, int hr)
			: base (message, hr)
		{
		}
	
		public HttpException (int httpCode, string message, Exception innerException)
			: base (message, innerException)
		{
			http_code = httpCode;
		}

		public string GetHtmlErrorMessage ()
		{
			if (HttpContext.Current.IsCustomErrorEnabled)
				return GetCustomErrorDefaultMessage ();
			
			if (!(this.InnerException is HtmlizedException))
				return GetDefaultErrorMessage ();

			return GetHtmlizedErrorMessage ();
		}

		internal virtual string Description {
			get { return "Error processing request."; }
		}

		void WriteFileTop (StringBuilder builder, string title)
		{
			builder.Append ("<?xml version=\"1.0\" ?>\n");
			builder.Append ("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			builder.AppendFormat ("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><title>{0}</title><style type=\"text/css\">", title);
			builder.AppendFormat (
				@"body {{font-family:{0};font-weight:normal;font-size: .7em;color:black;background-color: white}}
p {{font-family:{0};font-weight:normal;color:black;margin-top: -5px}}
b {{font-family:{0};font-weight:bold;color:black;margin-top: -5px}}
h1 {{ font-family:{0};font-weight:normal;font-size:18pt;color:red }}
h2 {{ font-family:{0};font-weight:normal;font-size:14pt;color:maroon }}
pre {{font-family:""Lucida Console"",""DejaVu Sans Mono"",	monospace;font-size: 1.2em}}
div.bodyText {{font-family: {0}}}
table.sampleCode {{width: 100%; background-color: #ffffcc; }}
.errorText {{color: red; font-weight: bold}}
.marker {{font-weight: bold; color: black;text-decoration: none;}}
.version {{color: gray;}}
.error {{margin-bottom: 10px;}}
.expandable {{ text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }}", errorStyleFonts);
			builder.AppendFormat (
				"</style></head><body><h1>Server Error in '{0}' Application</h1><hr style=\"color: silver\"/>",
				HtmlEncode (HttpRuntime.AppDomainAppVirtualPath));
		}

		void WriteFileBottom (StringBuilder builder, string trace1, string trace2)
		{
			if (trace1 != null)
				builder.AppendFormat ("<![CDATA[\r\n{0}\r\n]]>\r\n", HttpUtility.HtmlEncode (trace1));
			if (trace2 != null)
				builder.AppendFormat ("<![CDATA[\r\n{0}\r\n]]>\r\n", HttpUtility.HtmlEncode (trace2));
			builder.AppendFormat ("<hr style=\"color: silver\"/>{0}</body></html>\r\n", DateTime.UtcNow);
		}

		string GetCustomErrorDefaultMessage ()
		{
			StringBuilder builder = new StringBuilder ();
			WriteFileTop (builder, "Runtime Error");
			builder.Append (@"<p><strong>Description:</strong> An application error occurred on the server. The current custom error settings for this application prevent the details of the application error from being viewed remotely (for security reasons). It could, however, be viewed by browsers running on the local server machine.</p>
<p><strong>Details:</strong> To enable the details of this specific error message to be viewable on remote machines, please create a &lt;customErrors&gt; tag within a &quot;web.config&quot; configuration file located in the root directory of the current web application. This &lt;customErrors&gt; tag should then have its &quot;mode&quot; attribute set to &quot;Off&quot;.</p>
<table class=""sampleCode""><tr><td><pre>

&lt;!-- Web.Config Configuration File --&gt;

&lt;configuration&gt;
    &lt;system.web&gt;

        &lt;customErrors mode=&quot;Off&quot;/&gt;
    &lt;/system.web&gt;
&lt;/configuration&gt;</pre>
    </td></tr></table><br/>
<p><strong>Notes:</strong> The current error page you are seeing can be replaced by a custom error page by modifying the &quot;defaultRedirect&quot; attribute of the application's &lt;customErrors&gt; configuration tag to point to a custom error page URL.</p>
<table class=""sampleCode""><tr><td><pre>
&lt;!-- Web.Config Configuration File --&gt;

&lt;configuration&gt;
    &lt;system.web&gt;
        &lt;customErrors mode=&quot;RemoteOnly&quot; defaultRedirect=&quot;mycustompage.htm&quot;/&gt;

    &lt;/system.web&gt;
&lt;/configuration&gt;</pre></td></tr></table>");
			WriteFileBottom (builder, null, null);
			return builder.ToString ();
		}
		
		string GetDefaultErrorMessage ()
		{
			Exception ex = InnerException;
			if (ex == null)
				ex = this;

			StringBuilder builder = new StringBuilder ();
			WriteFileTop (builder, String.Format ("Error{0}", http_code != 0 ? " " + http_code : String.Empty));
			builder.AppendFormat ("<h2><em>{0}</em></h2>\r\n", HtmlEncode (ex.Message));
			builder.AppendFormat ("<p><strong>Description: </strong>{0}</p>\r\n", HtmlEncode (Description));
			builder.Append ("<p><strong>Error Message: </strong>");
			if (http_code != 0)
				builder.AppendFormat ("HTTP {0}. ", http_code);
			builder.AppendFormat ("{0}: {1}\r\n</p>\r\n", ex.GetType ().FullName, HtmlEncode (ex.Message));

			if (InnerException != null) {
				builder.AppendFormat ("<p><strong>Stack Trace: </strong></p>");
				builder.Append ("<table summary=\"Stack Trace\" class=\"sampleCode\">\r\n<tr><td>");
				WriteTextAsCode (builder, InnerException.ToString ());
				builder.Append ("</td></tr>\r\n</table>\r\n");
			}
			WriteFileBottom (builder,
					 this.ToString (),
					 null
			);
			
			return builder.ToString ();
		}

		static string HtmlEncode (string s)
		{
			if (s == null)
				return s;

			string res = HttpUtility.HtmlEncode (s);
			return res.Replace ("\r\n", "<br />");
		}

		string GetHtmlizedErrorMessage ()
		{
			StringBuilder builder = new StringBuilder ();
			HtmlizedException exc = (HtmlizedException) this.InnerException;
			WriteFileTop (builder, exc.Title);
			builder.AppendFormat ("<h2><em>{0}</em></h2>\r\n", exc.Title);
			builder.AppendFormat ("<p><strong>Description: </strong>{0}\r\n</p>\r\n", HtmlEncode (exc.Description));
			string errorMessage = HtmlEncode (exc.ErrorMessage).Replace ("\n", "<br/>");
			builder.AppendFormat ("<p><strong>Error message: </strong></p><p>{0}</p>", errorMessage);

			if (exc.FileName != null)
				builder.AppendFormat ("<p><strong>File name: </strong> {0}</p>", HtmlEncode (exc.FileName));

			if (exc.FileText != null) {
				if (exc.SourceFile != exc.FileName)
					builder.AppendFormat ("<p><strong>Source File: </strong>{0}</p>", exc.SourceFile);

				if (exc is ParseException) {
					builder.Append ("<p>&nbsp;&nbsp;&nbsp;&nbsp;<strong>Line: </strong>");
					builder.Append (exc.ErrorLines [0]);
					builder.Append ("</p>");
				}

				if (exc is ParseException) {
					builder.Append ("<strong>Source Error: </strong>\r\n");
					builder.Append ("<table summary=\"Source error\" class=\"sampleCode\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n");
				} else {
					builder.Append ("<table summary=\"Source file\" class=\"sampleCode\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n");
				}
			}			

			WriteFileBottom (
				builder,
				exc.ToString (),
				null
			);
			
			return builder.ToString ();
		}

		static void WriteTextAsCode (StringBuilder builder, string text)
		{
			builder.AppendFormat ("<pre>{0}</pre>", HtmlEncode (text));
		}

#if TARGET_J2EE
		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<pre>");
			WritePageSource (builder, e);
			builder.Append ("</pre>\r\n");
		}

#else
		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<pre>");
			if (e is CompilationException)
				WriteCompilationSource (builder, e);
			else
				WritePageSource (builder, e);

			builder.Append ("</pre>\r\n");
		}
#endif
		
		static void WriteCompilationSource (StringBuilder builder, HtmlizedException e)
		{
			int [] a = e.ErrorLines;
			string s;
			int line = 0;
			int index = 0;
			int errline = 0;

			if (a != null && a.Length > 0)
				errline = a [0];
			
			TextReader reader = new StringReader (e.FileText);
			while ((s = reader.ReadLine ()) != null) {
				line++;

				if (errline == line)
					builder.Append ("<span style=\"color: red\">");

				builder.AppendFormat ("Line {0}: {1}\r\n", line, HtmlEncode (s));

				if (line == errline) {
					builder.Append ("</span>");
					errline = (++index < a.Length) ? a [index] : 0;
				}
			}
		}

		static void WritePageSource (StringBuilder builder, HtmlizedException e)
		{
			string s;
			int line = 0;
			int beginerror = e.ErrorLines [0];
			int enderror = e.ErrorLines [1];
			int begin = beginerror - 3;
			int end = enderror + 3;
			if (begin <= 0)
				begin = 1;
			
			TextReader reader = new StringReader (e.FileText);
			while ((s = reader.ReadLine ()) != null) {
				line++;
				if (line < begin)
					continue;

				if (line > end)
					break;

				if (beginerror == line)
					builder.Append ("<span style=\"color: red\">");

				builder.AppendFormat ("{0}\r\n", HtmlEncode (s));

				if (enderror <= line) {
					builder.Append ("</span>");
					enderror = end + 1; // one shot
				}
			}
		}
		
		public int GetHttpCode ()
		{
			return http_code;
		}

		public static HttpException CreateFromLastError (string message)
		{
			WebTrace.WriteLine ("CreateFromLastError");
			return new HttpException (message, 0);
		}
	}
}

