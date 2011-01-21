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
		ExceptionPageTemplate pageTemplate;

		ExceptionPageTemplate PageTemplate {
			get {
				if (pageTemplate == null)
					pageTemplate = GetPageTemplate ();
				return pageTemplate;
			}
		}
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

		[MonoTODO ("For now just the default template is created. Means of user-provided templates are to be implemented yet.")]
		ExceptionPageTemplate GetPageTemplate ()
		{
			ExceptionPageTemplate template = new DefaultExceptionPageTemplate ();
			template.Init ();

			return template;
		}
		
		public string GetHtmlErrorMessage ()
		{
			var values = new ExceptionPageTemplateValues ();
			ExceptionPageTemplate template = PageTemplate;

			try {
				values.Add (ExceptionPageTemplate.Template_RuntimeVersionInformationName, RuntimeHelpers.MonoVersion);
				values.Add (ExceptionPageTemplate.Template_AspNetVersionInformationName, Environment.Version.ToString ());
				
				HttpContext ctx = HttpContext.Current;
				ExceptionPageTemplateType pageType = ExceptionPageTemplateType.Standard;

				if (ctx != null && ctx.IsCustomErrorEnabled) {
					if (http_code != 404 && http_code != 403) {
						FillDefaultCustomErrorValues (values);
						pageType = ExceptionPageTemplateType.CustomErrorDefault;
					} else
						FillDefaultErrorValues (false, false, null, values);
				} else {
					Exception ex = GetBaseException ();
					if (ex == null)
						ex = this;

					values.Add (ExceptionPageTemplate.Template_FullStackTraceName, FormatFullStackTrace ());
					HtmlizedException htmlException = ex as HtmlizedException;
					if (htmlException == null)
						FillDefaultErrorValues (true, true, ex, values);
					else {
						pageType = ExceptionPageTemplateType.Htmlized;
						FillHtmlizedErrorValues (values, htmlException, ref pageType);
					}
				}
				
				return template.Render (values, pageType);
			} catch (Exception ex) {
				Console.Error.WriteLine ("An exception has occurred while generating HttpException page:");
				Console.Error.WriteLine (ex);
				Console.Error.WriteLine ();
				Console.Error.WriteLine ("The actual exception which was being reported was:");
				Console.Error.WriteLine (this);

				// we need the try/catch block in case the
				// problem was with MapPath, which will cause
				// IsCustomErrorEnabled to throw an exception
				try {
					FillDefaultCustomErrorValues (values);
					return template.Render (values, ExceptionPageTemplateType.CustomErrorDefault);
				} catch {
					return DoubleFaultExceptionMessage;
				}
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
		
		string FormatFullStackTrace ()
		{
			Exception ex = this;
			var builder = new StringBuilder ("\r\n<!--");
			string trace;
			string message;
			bool haveTrace, first = true;
			
			while (ex != null) {
				trace = ex.StackTrace;
				message = ex.Message;
				haveTrace = !String.IsNullOrEmpty (trace);
				
				if (!haveTrace && String.IsNullOrEmpty (message)) {
					ex = ex.InnerException;
					continue;
				}

				if (first)
					first = false;
				else
					builder.Append ("\r\n");
				
				builder.Append ("\r\n[" + ex.GetType () + "]: " + HtmlEncode (message) + "\r\n");
				if (haveTrace)
					builder.Append (ex.StackTrace);
				
				ex = ex.InnerException;
			}
			builder.Append ("\r\n-->\r\n");

			return builder.ToString ();
		}

		void FillHtmlizedErrorValues (ExceptionPageTemplateValues values, HtmlizedException exc, ref ExceptionPageTemplateType pageType)
		{
#if TARGET_J2EE
			bool isParseException = false;
			bool isCompileException = false;
#else
			bool isParseException = exc is ParseException;
			bool isCompileException = (!isParseException && exc is CompilationException);
#endif
			values.Add (ExceptionPageTemplate.Template_PageTitleName, HtmlEncode (exc.Title));
			values.Add (ExceptionPageTemplate.Template_DescriptionName, HtmlEncode (exc.Description));
			values.Add (ExceptionPageTemplate.Template_StackTraceName, HtmlEncode (exc.StackTrace));
			values.Add (ExceptionPageTemplate.Template_ExceptionTypeName, exc.GetType ().ToString ());
			values.Add (ExceptionPageTemplate.Template_ExceptionMessageName, HtmlEncode (exc.Message));
			values.Add (ExceptionPageTemplate.Template_DetailsName, HtmlEncode (exc.ErrorMessage));

			string origin;
			if (isParseException)
				origin = "Parser";
			else if (isCompileException)
				origin = "Compiler";
			else
				origin = "Other";
			values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionOriginName, origin);
			if (exc.FileText != null) {
				pageType |= ExceptionPageTemplateType.SourceError;
				StringBuilder shortSource = new StringBuilder ();
				StringBuilder longSource;
				
				if (isCompileException)
					longSource = new StringBuilder ();
				else
					longSource = null;
				FormatSource (shortSource, longSource, exc);
				values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionShortSourceName, shortSource.ToString ());
				values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionLongSourceName, longSource != null ? longSource.ToString () : null);
				
				if (exc.SourceFile != exc.FileName)
					values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionSourceFileName, FormatSourceFile (exc.SourceFile));
				else
					values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionSourceFileName, FormatSourceFile (exc.FileName));
				if (isParseException || isCompileException) {
					int[] errorLines = exc.ErrorLines;
					int numErrors = errorLines != null ? errorLines.Length : 0;
					var lines = new StringBuilder ();
					for (int i = 0; i < numErrors; i++) {
						if (i > 0)
							lines.Append (", ");
						lines.Append (errorLines [i]);
					}
					values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionErrorLinesName, lines.ToString ());
				}
			} else
				values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionSourceFileName, FormatSourceFile (exc.FileName));

#if !TARGET_J2EE
			if (isCompileException) {
				CompilationException cex = exc as CompilationException;
				StringCollection output = cex.CompilerOutput;

				if (output != null && output.Count > 0) {
					pageType |= ExceptionPageTemplateType.CompilerOutput;
					var sb = new StringBuilder ();
					bool first = true;
					foreach (string s in output) {
						sb.Append (HtmlEncode (s));
						if (first) {
							sb.Append ("<br/>");
							first = false;
						}
						sb.Append ("<br/>");
					}
					
					values.Add (ExceptionPageTemplate.Template_HtmlizedExceptionCompilerOutputName, sb.ToString ());
				}
			}
#endif
		}
		
		void FillDefaultCustomErrorValues (ExceptionPageTemplateValues values)
		{
			values.Add (ExceptionPageTemplate.Template_PageTitleName, "Runtime Error");
			values.Add (ExceptionPageTemplate.Template_ExceptionTypeName, "Runtime Error");
			values.Add (ExceptionPageTemplate.Template_ExceptionMessageName, "A runtime error has occurred");
			values.Add (ExceptionPageTemplate.Template_DescriptionName, "An application error occurred on the server. The current custom error settings for this application prevent the details of the application error from being viewed (for security reasons).");
			values.Add (ExceptionPageTemplate.Template_DetailsName, "To enable the details of this specific error message to be viewable, please create a &lt;customErrors&gt; tag within a &quot;web.config&quot; configuration file located in the root directory of the current web application. This &lt;customErrors&gt; tag should then have its &quot;mode&quot; attribute set to &quot;Off&quot;.");
		}
		
		void FillDefaultErrorValues (bool showTrace, bool showExceptionType, Exception baseEx, ExceptionPageTemplateValues values)
		{
			if (baseEx == null)
				baseEx = this;
			
			values.Add (ExceptionPageTemplate.Template_PageTitleName, String.Format ("Error{0}", http_code != 0 ? " " + http_code : String.Empty));
			values.Add (ExceptionPageTemplate.Template_ExceptionTypeName, showExceptionType ? baseEx.GetType ().ToString () : "Runtime error");
			values.Add (ExceptionPageTemplate.Template_ExceptionMessageName, http_code == 404 ? "The resource cannot be found." : HtmlEncode (baseEx.Message));

			string tmp = http_code != 0 ? "HTTP " + http_code + "." : String.Empty;
			values.Add (ExceptionPageTemplate.Template_DescriptionName, tmp + (http_code == 404 ? ERROR_404_DESCRIPTION : HtmlEncode (Description)));

			if (!String.IsNullOrEmpty (resource_name))
				values.Add (ExceptionPageTemplate.Template_DetailsName, "Requested URL: " + HtmlEncode (resource_name));
			else if (http_code == 404)
				values.Add (ExceptionPageTemplate.Template_DetailsName, "No virtual path information available.");
			else if (baseEx is HttpException) {
				tmp = ((HttpException)baseEx).Description;
				values.Add (ExceptionPageTemplate.Template_DetailsName, !String.IsNullOrEmpty (tmp) ? HtmlEncode (tmp) : "Web exception occurred but no additional error description given.");
			} else {
				var sb = new StringBuilder ("Non-web exception.");

				tmp = baseEx.Source;
				if (!String.IsNullOrEmpty (tmp))
					sb.AppendFormat (" Exception origin (name of application or object): {0}.", HtmlEncode (tmp));
				tmp = baseEx.HelpLink;
				if (!String.IsNullOrEmpty (tmp))
					sb.AppendFormat (" Additional information is available at {0}", HtmlEncode (tmp));
				
				values.Add (ExceptionPageTemplate.Template_DetailsName, sb.ToString ());
			}
			
			if (showTrace)
				values.Add (ExceptionPageTemplate.Template_StackTraceName, HtmlEncode (baseEx.StackTrace.ToString ()));
		}
		
		static string HtmlEncode (string s)
		{
			if (String.IsNullOrEmpty (s))
				return s;

			string res = HttpUtility.HtmlEncode (s);
			return res.Replace ("\r\n", "<br />");
		}

		string FormatSourceFile (string filename)
		{
			if (filename == null || filename.Length == 0)
				return String.Empty;

			if (filename.StartsWith ("@@"))
				return "[internal] <!-- " + HttpUtility.HtmlEncode (filename) + " -->";

			return HttpUtility.HtmlEncode (filename);
		}
		
		static void FormatSource (StringBuilder builder, StringBuilder longVersion, HtmlizedException e)
		{
#if !TARGET_J2EE
			if (e is CompilationException)
				WriteCompilationSource (builder, longVersion, e);
			else
#endif
				WritePageSource (builder, e);
		}

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
							longVersion.AppendFormat ("{0}: {1}\r\n", line, HtmlEncode (s));
						continue;
					}
				
					if (errline == line) {
						if (longVersion != null)
							longVersion.Append ("<span class=\"sourceErrorLine\">");
						builder.Append ("<span class=\"sourceErrorLine\">");
					}
					
					tmp = String.Format ("{0}: {1}\r\n", line, HtmlEncode (s));
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
					builder.Append ("<span class=\"sourceErrorLine\">");

				builder.AppendFormat ("{0}: {1}\r\n", line, HtmlEncode (s));

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

		// Putting this at the end so that the code above isn't bloated
		const string DoubleFaultExceptionMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<style type=""text/css"">
body { background-color: #FFFFFF; font-size: .75em; font-family: Verdana, Helvetica, Sans-Serif; margin: 0; padding: 0; color: #696969; }
a:link { color: #000000; text-decoration: underline; }
a:visited { color: #000000; }
a:hover { color: #000000; text-decoration: none; }
a:active { color: #12eb87; }
p, ul { margin-bottom: 20px; line-height: 1.6em; }
pre { font-size: 1.2em; margin-left: 20px; margin-top: 0px; }
h1, h2, h3, h4, h5, h6 { font-size: 1.6em; color: #000; font-family: Arial, Helvetica, sans-serif; }
h1 { font-weight: bold; margin-bottom: 0; margin-top: 0; padding-bottom: 0; }
h2 { font-size: 1em; padding: 0 0 0px 0; color: #696969; font-weight: normal; margin-top: 0; margin-bottom: 20px; }
h3 { font-size: 1.2em; }
h4 { font-size: 1.1em; }
h5, h6 { font-size: 1em; }
#header { position: relative; margin-bottom: 0px; color: #000; padding: 0; background-color: #5c87b2; height: 38px; padding-left: 10px; }
#header h1 { font-weight: bold; padding: 5px 0; margin: 0; color: #fff; border: none; line-height: 2em; font-family: Arial, Helvetica, sans-serif; font-size: 32px !important; }
#header-image { float: left; padding: 3px; margin-left: 1px; margin-right: 1px; }
#header-text { color: #fff; font-size: 1.4em; line-height: 38px; font-weight: bold; }
#main { padding: 20px 20px 15px 20px; background-color: #fff; _height: 1px; }
#footer { color: #999; padding: 5px 0; text-align: left; line-height: normal; margin: 20px 0px 0px 0px; font-size: .9em; border-top: solid 1px #5C87B2; }
#footer-powered-by { float: right; }
.details { font-family: monospace; border: solid 1px #e8eef4; white-space: pre; font-size: 1.2em; overflow: auto; padding: 6px; margin-top: 6px }
.details-wrapped { white-space: normal }
.details-header { margin-top: 1.5em }
.details-header a { font-weight: bold; text-decoration: none }
p { margin-bottom: 0.3em; margin-top: 0.1em }
.sourceErrorLine { color: #770000; font-weight: bold; }
</style>

<title>Double fault in exception reporting code</title>
</head>
<body>
<h1>Double fault in exception reporting code</h1>
<p>While generating HTML with exception report, a double fault has occurred. Please consult your server's console and/or log file to see the actual exception.</p>
</body>
</html>
";
	}
}

