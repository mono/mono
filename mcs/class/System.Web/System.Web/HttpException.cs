// 
// System.Web.HttpException
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Patrik Torstensson
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.Web.Management;
using System.Collections.Specialized;

namespace System.Web
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[Serializable]
	public class HttpException : ExternalException
	{
		const string DEFAULT_DESCRIPTION_TEXT = "Error processing request.";
		const string ERROR_404_DESCRIPTION = "The resource you are looking for (or one of its dependencies) could have been removed, had its name changed, or is temporarily unavailable.  Please review the following URL and make sure that it is spelled correctly.";

		int webEventCode = WebEventCodes.UndefinedEventCode;
		int http_code = 500;
		string resource_name;
		string description;
		
		const string errorStyleFonts = "\"Verdana\",\"DejaVu Sans\",sans-serif";

#if NET_4_0
		public
#else
		internal
#endif
		int WebEventCode 
		{
			get { return webEventCode; }
		}
		
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

		internal HttpException (int httpCode, string message, string resourceName) : this (httpCode, message)
		{
			resource_name = resourceName;
		}

		internal HttpException (int httpCode, string message, string resourceName, string description) : this (httpCode, message, resourceName)
		{
			this.description = description;
		}
		
		protected HttpException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			http_code = info.GetInt32 ("_httpCode");
			webEventCode = info.GetInt32 ("_webEventCode");
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("_httpCode", http_code);
			info.AddValue ("_webEventCode", webEventCode);
		}

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

		internal HttpException (int httpCode, string message, Exception innerException, string resourceName)
			: this (httpCode, message, innerException)
		{
			resource_name = resourceName;
		}
		
		public string GetHtmlErrorMessage ()
		{
			try {
				HttpContext ctx = HttpContext.Current;
				if (ctx != null && ctx.IsCustomErrorEnabled) {
					if (http_code != 404 && http_code != 403)
						return GetCustomErrorDefaultMessage ();
					else
						return GetDefaultErrorMessage (false, null);
				}
				
				Exception ex = GetBaseException ();
				if (ex == null)
					ex = this;
				
				HtmlizedException htmlException = ex  as HtmlizedException;
				if (htmlException == null)
					return GetDefaultErrorMessage (true, ex);
				
				return GetHtmlizedErrorMessage (htmlException);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				
				// we need the try/catch block in case the
				// problem was with MapPath, which will cause
				// IsCustomErrorEnabled to throw an exception
				return GetCustomErrorDefaultMessage ();
			}
		}

		internal virtual string Description {
			get {
				if (description != null)
					return description;

				return DEFAULT_DESCRIPTION_TEXT;
			}
			
			set {
				if (value != null && value.Length > 0)
					description = value;
				else
					description = DEFAULT_DESCRIPTION_TEXT;
			}
		}

		internal static HttpException NewWithCode (string message, int webEventCode)
		{
			var ret = new HttpException (message);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}

		internal static HttpException NewWithCode (string message, Exception innerException, int webEventCode)
		{
			var ret = new HttpException (message, innerException);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}

		internal static HttpException NewWithCode (int httpCode, string message, int webEventCode)
		{
			var ret = new HttpException (httpCode, message);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}
		
		internal static HttpException NewWithCode (int httpCode, string message, Exception innerException, string resourceName, int webEventCode)
		{
			var ret = new HttpException (httpCode, message, innerException, resourceName);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}

		internal static HttpException NewWithCode (int httpCode, string message, string resourceName, int webEventCode)
		{
			var ret = new HttpException (httpCode, message, resourceName);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}

		internal static HttpException NewWithCode (int httpCode, string message, Exception innerException, int webEventCode)
		{
			var ret = new HttpException (httpCode, message, innerException);
			ret.SetWebEventCode (webEventCode);

			return ret;
		}
		
		internal void SetWebEventCode (int webEventCode)
		{
			this.webEventCode = webEventCode;
		}
		
		void WriteFileTop (StringBuilder builder, string title)
		{
#if TARGET_J2EE
			builder.AppendFormat ("<html><head><title>{0}</title><style type=\"text/css\">", title);
			builder.AppendFormat (
				@"body {{font-family:{0};font-weight:normal;font-size: 9pt;color:black;background-color: white}}
p {{font-family:{0};font-weight:normal;color:black;margin-top: -5px}}
b {{font-family:{0};font-weight:bold;color:black;margin-top: -5px}}
h1 {{ font-family:{0};font-weight:normal;font-size:18pt;color:red }}
h2 {{ font-family:{0};font-weight:normal;font-size:14pt;color:maroon }}
pre {{font-family:""Lucida Console"",""DejaVu Sans Mono"",monospace;font-size: 10pt}}
div.bodyText {{font-family: {0}}}
table.sampleCode {{width: 100%; background-color: #ffffcc; }}
.errorText {{color: red; font-weight: bold}}
.marker {{font-weight: bold; color: black;text-decoration: none;}}
.version {{color: gray;}}
.error {{margin-bottom: 10px;}}
.expandable {{ text-decoration:underline; font-weight:bold; color:navy; cursor:pointer; }}", errorStyleFonts);
#else
			builder.Append ("<?xml version=\"1.0\" ?>\n");
			builder.Append ("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			builder.AppendFormat ("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><title>{0}</title><style type=\"text/css\">", title);
			builder.AppendFormat (
				@"body {{font-family:{0};font-weight:normal;font-size: .7em;color:black;background-color: white}}
p {{font-family:{0};font-weight:normal;color:black;margin-top: -5px}}
b {{font-family:{0};font-weight:bold;color:black;margin-top: -5px}}
h1 {{ font-family:{0};font-weight:normal;font-size:18pt;color:red }}
h2 {{ font-family:{0};font-weight:normal;font-size:14pt;color:maroon }}
pre,code {{font-family:""Lucida Console"",""DejaVu Sans Mono"",monospace;font-size: 0.9em,white-space: pre-line}}
div.bodyText {{font-family: {0}}}
table.sampleCode {{width: 100%; background-color: #ffffcc; }}
.errorText {{color: red; font-weight: bold}}
.marker {{font-weight: bold; color: black;text-decoration: none;}}
.version {{color: gray;}}
.error {{margin-bottom: 10px;}}
.expandable {{ text-decoration:underline; font-weight:bold; color:navy; cursor:pointer; }}", errorStyleFonts);
#endif

			builder.AppendFormat (
				"</style></head><body><h1>Server Error in '{0}' Application</h1><hr style=\"color: silver\"/>",
				HtmlEncode (HttpRuntime.AppDomainAppVirtualPath));
		}
		
		void WriteFileBottom (StringBuilder builder, bool showTrace)
		{
			if (showTrace) {
				builder.Append ("<hr style=\"color: silver\"/>");
				builder.AppendFormat ("<strong>Version information: </strong> Mono Runtime Version: <tt>{0}</tt>; ASP.NET Version: <tt>{1}</tt></body></html>\r\n",
						      RuntimeHelpers.MonoVersion, Environment.Version);
			
				string trace, message;
				bool haveTrace;
				Exception ex = this;

				builder.Append ("\r\n<!--");
				while (ex != null) {
					trace = ex.StackTrace;
					message = ex.Message;
					haveTrace = (trace != null && trace.Length > 0);
				
					if (!haveTrace && (message == null || message.Length == 0)) {
						ex = ex.InnerException;
						continue;
					}

					builder.Append ("\r\n[" + ex.GetType () + "]: " + HtmlEncode (message) + "\r\n");
					if (haveTrace)
						builder.Append (ex.StackTrace);
				
					ex = ex.InnerException;
				}
			
				builder.Append ("\r\n-->");
			} else
				builder.Append ("</body></html>\r\n");
		}

		string GetCustomErrorDefaultMessage ()
		{
			StringBuilder builder = new StringBuilder ();
			WriteFileTop (builder, "Runtime Error");
#if TARGET_J2EE //on portal we cannot know if we run locally
			if (!HttpContext.Current.IsServletRequest)
				builder.Append (
@"<p><strong>Description:</strong> An application error occurred on the server. The current custom error settings for this application prevent the details of the application error from being viewed (for security reasons).</p>
<p><strong>Details:</strong> To enable the details of this specific error message to be viewable, please create a &lt;customErrors&gt; tag within a &quot;web.config&quot; configuration file located in the root directory of the current web application. This &lt;customErrors&gt; tag should then have its &quot;mode&quot; attribute set to &quot;Off&quot;.</p>
<table class=""sampleCode""><tr><td><pre>

&lt;!-- Web.Config Configuration File --&gt;

&lt;configuration&gt;
    &lt;system.web&gt;

        &lt;customErrors mode=&quot;Off&quot;/&gt;
    &lt;/system.web&gt;
&lt;/configuration&gt;</pre>
    </td></tr></table><br/>
");
			else
#endif
			builder.Append (@"<p><strong>Description:</strong> An application error occurred on the server. The current custom error settings for this application prevent the details of the application error from being viewed remotely (for security reasons)." + (
				" It could, however, be viewed by browsers running on the local server machine.") 
				+ @"</p>
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
			WriteFileBottom (builder, false);
			return builder.ToString ();
		}
		
		string GetDefaultErrorMessage (bool showTrace, Exception baseEx)
		{
			Exception ex;
			ex = baseEx;
			if (ex == null)
				ex = this;

			StringBuilder builder = new StringBuilder ();
			WriteFileTop (builder, String.Format ("Error{0}", http_code != 0 ? " " + http_code : String.Empty));
			builder.Append ("<h2><em>");
			if (http_code == 404)
				builder.Append ("The resource cannot be found.");
			else
				builder.Append (HtmlEncode (ex.Message));
			builder.Append ("</em></h2>\r\n<p><strong>Description: </strong>");
			
			if (http_code != 0)
				builder.AppendFormat ("HTTP {0}. ", http_code);
			builder.Append (http_code == 404 ? ERROR_404_DESCRIPTION : HtmlEncode (Description));
			builder.Append ("</p>\r\n");

			if (resource_name != null && resource_name.Length > 0)
				builder.AppendFormat ("<p><strong>Requested URL: </strong>{0}</p>\r\n", resource_name);
			
			if (showTrace && baseEx != null && http_code != 404 && http_code != 403) {
				builder.Append ("<p><strong>Stack Trace: </strong></p>");
				builder.Append ("<table summary=\"Stack Trace\" class=\"sampleCode\">\r\n<tr><td>");
				WriteTextAsCode (builder, baseEx.ToString ());
				builder.Append ("</td></tr>\r\n</table>\r\n");
			}
			WriteFileBottom (builder, showTrace);
			
			return builder.ToString ();
		}

		static string HtmlEncode (string s)
		{
			if (s == null)
				return s;

			string res = HttpUtility.HtmlEncode (s);
			return res.Replace ("\r\n", "<br />");
		}

		string FormatSourceFile (string filename)
		{
			if (filename == null || filename.Length == 0)
				return String.Empty;

			if (filename.StartsWith ("@@"))
				return "[internal] <!-- " + filename + " -->";

			return filename;
		}
		
		string GetHtmlizedErrorMessage (HtmlizedException exc)
		{
			StringBuilder builder = new StringBuilder ();
#if TARGET_J2EE
			bool isParseException = false;
			bool isCompileException = false;
#else
			bool isParseException = exc is ParseException;
			bool isCompileException = (!isParseException && exc is CompilationException);
#endif
			
			WriteFileTop (builder, exc.Title);
			builder.AppendFormat ("<h2><em>{0}</em></h2>\r\n", exc.Title);
			builder.AppendFormat ("<p><strong>Description: </strong>{0}\r\n</p>\r\n", HtmlEncode (exc.Description));
			string errorMessage = HtmlEncode (exc.ErrorMessage);
			
			builder.Append ("<p><strong>");
			if (isParseException)
				builder.Append ("Parser ");
			else if (isCompileException)
				builder.Append ("Compiler ");
			
			builder.Append ("Error Message: </strong>");
			builder.AppendFormat ("<code>{0}</code></p>", errorMessage);

			StringBuilder longCodeVersion = null;
			
			if (exc.FileText != null) {
				if (isParseException || isCompileException) {
					builder.Append ("<p><strong>Source Error: </strong></p>\r\n");
					builder.Append ("<table summary=\"Source error\" class=\"sampleCode\">\r\n<tr><td>");

					if (isCompileException)
						longCodeVersion = new StringBuilder ();
					WriteSource (builder, longCodeVersion, exc);
					builder.Append ("</pre></code></td></tr>\r\n</table>\r\n");
				} else {
					builder.Append ("<table summary=\"Source file\" class=\"sampleCode\">\r\n<tr><td>");
					WriteSource (builder, null, exc);
					builder.Append ("</pre></code></td></tr>\r\n</table>\r\n");
				}

				builder.Append ("<br/><p><strong>Source File: </strong>");
				if (exc.SourceFile != exc.FileName)
					builder.Append (FormatSourceFile (exc.SourceFile));
				else
					builder.Append (FormatSourceFile (exc.FileName));

				if (isParseException || isCompileException) {
					int[] errorLines = exc.ErrorLines;
					int numErrors = errorLines != null ? errorLines.Length : 0;
					if (numErrors > 0) {
						builder.AppendFormat ("&nbsp;&nbsp;<strong>Line{0}: </strong>", numErrors > 1 ? "s" : String.Empty);
						for (int i = 0; i < numErrors; i++) {
							if (i > 0)
								builder.Append (", ");
							builder.Append (exc.ErrorLines [i]);
						}
					}
				}
				builder.Append ("</p>");
			} else if (exc.FileName != null)
				builder.AppendFormat ("{0}</p>", HtmlEncode (exc.FileName));

			bool needToggleJS = false;
			
#if !TARGET_J2EE
			if (isCompileException) {
				CompilationException cex = exc as CompilationException;
				StringCollection output = cex.CompilerOutput;

				if (output != null && output.Count > 0) {
					needToggleJS = true;
					StringBuilder sb = new StringBuilder ();
					foreach (string s in output)
						sb.Append (s + "\r\n");
					WriteExpandableBlock (builder, "compilerOutput", "Show Detailed Compiler Output", sb.ToString ());
				}
			}
#endif
			
			if (longCodeVersion != null && longCodeVersion.Length > 0) {
				WriteExpandableBlock (builder, "fullCode", "Show Complete Compilation Source", longCodeVersion.ToString ());
				needToggleJS = true;
			}

			if (needToggleJS)
				builder.Append ("<script type=\"text/javascript\">\r\n" +
						"function ToggleVisible (id)\r\n" +
						"{\r\n" +
						"\tvar e = document.getElementById (id);\r\n" +
						"\tif (e.style.display == 'none')\r\n" +
						"\t{\r\n" +
						"\t\te.style.display = '';\r\n" +
						"\t} else {\r\n" +
						"\t\te.style.display = 'none';\r\n" +
						"\t}\r\n" +
						"}\r\n" +
						"</script>\r\n");
			
			WriteFileBottom (builder, true);
			
			return builder.ToString ();
		}

		static void WriteExpandableBlock (StringBuilder builder, string id, string title, string contents)
		{
			builder.AppendFormat ("<br><div class=\"expandable\" onclick=\"ToggleVisible ('{1}')\">{0}:</div><br/>" +
					      "<div id=\"{1}\" style=\"display: none\"><table summary=\"Details\" class=\"sampleCode\"><tr><td>" +
					      "<code><pre>\r\n", title, id);
			builder.Append (contents);
			builder.Append ("</pre></code></td></tr></table></div>");
		}
		
		static void WriteTextAsCode (StringBuilder builder, string text)
		{
			builder.AppendFormat ("<pre>{0}</pre>", HtmlEncode (text));
		}

#if TARGET_J2EE
		static void WriteSource (StringBuilder builder, StringBuilder longVersion, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			WritePageSource (builder, e);
			builder.Append ("</code></pre>\r\n");
		}

#else
		static void WriteSource (StringBuilder builder, StringBuilder longVersion, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			if (e is CompilationException)
				WriteCompilationSource (builder, longVersion, e);
			else
				WritePageSource (builder, e);

			builder.Append ("<code></pre>\r\n");
		}
#endif
		
		static void WriteCompilationSource (StringBuilder builder, StringBuilder longVersion, HtmlizedException e)
		{
			int [] a = e.ErrorLines;
			string s;
			int line = 0;
			int index = 0;
			int errline = 0;

			if (a != null && a.Length > 0)
				errline = a [0];

			int begin = errline - 2;
			int end = errline + 2;

			if (begin < 0)
				begin = 0;

			string tmp;			
			using (TextReader reader = new StringReader (e.FileText)) {
				while ((s = reader.ReadLine ()) != null) {
					line++;
					if (line < begin || line > end) {
						if (longVersion != null)
							longVersion.AppendFormat ("Line {0}: {1}\r\n", line, HtmlEncode (s));
						continue;
					}
				
					if (errline == line) {
						if (longVersion != null)
							longVersion.Append ("<span style=\"color: red\">");
						builder.Append ("<span style=\"color: red\">");
					}
					
					tmp = String.Format ("Line {0}: {1}\r\n", line, HtmlEncode (s));
					builder.Append (tmp);
					if (longVersion != null)
						longVersion.Append (tmp);
					
					if (line == errline) {
						builder.Append ("</span>");
						if (longVersion != null)
							longVersion.Append ("</span>");
						errline = (++index < a.Length) ? a [index] : 0;
					}
				}
			}			
		}

		static void WritePageSource (StringBuilder builder, HtmlizedException e)
		{
			string s;
			int line = 0;
			int beginerror = e.ErrorLines [0];
			int enderror = e.ErrorLines [1];
			int begin = beginerror - 2;
			int end = enderror + 2;
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

				builder.AppendFormat ("Line {0}: {1}\r\n", line, HtmlEncode (s));

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

