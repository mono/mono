// 
// System.Web.HttpException
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Patrik Torstensson
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web
{
#if NET_1_2
	[Serializable]
#endif
	public class HttpException : ExternalException
	{
		int http_code;
		int hr;
		string fileName;

		public HttpException () : base ()
		{
		}

		public HttpException (string sMessage) : base (sMessage)
		{
		}

		public HttpException (string sMessage, Exception InnerException)
			: base (sMessage, InnerException)
		{
		}

		public HttpException (int iHttpCode, string sMessage) : base (sMessage)
		{
			http_code = iHttpCode;
		}

#if NET_1_2
		protected HttpException (SerializationInfo info, StreamingContext sc) : base (info, sc)
		{
			http_code = info.GetInt32 ("_httpCode");
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("_httpCode", http_code);
		}
#endif

		public HttpException (int iHttpCode, string sMessage, int iHR) : base (sMessage)
		{
			http_code = iHttpCode;
			hr = iHR;
		}

		public HttpException (string sMessage, int iHR) : base (sMessage)
		{
			hr = iHR;
		}
	
		public HttpException (int iHttpCode,
				      string sMessage,
				      Exception InnerException)
			: base (sMessage, InnerException)
		{
			http_code = iHttpCode;
		}

		internal HttpException (string message, string fileName)
			: base (message)
		{
			this.fileName = fileName;
			
		}
		
		public string GetHtmlErrorMessage ()
		{
			if (!(this.InnerException is HtmlizedException))
				return GetDefaultErrorMessage ();

			return GetHtmlizedErrorMessage ();
		}

		string GetDefaultErrorMessage ()
		{
			StringBuilder builder = new StringBuilder ("<html>\r\n<title>");
			builder.Append ("Error");
			if (http_code != 0)
				builder.Append (" " + http_code);

			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Server error in '{0}' " + 
					      "application</font></h1><hr>\r\n",
					      HttpRuntime.AppDomainAppVirtualPath);

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\r\n", Message);
			builder.AppendFormat ("<b>Description: </b>{0}\r\n<p>\r\n", "Error processing request.");
			builder.Append ("<b>Error Message: </b>");
			if (http_code != 0)
				builder.AppendFormat ("HTTP {0}. ", http_code);

			builder.AppendFormat ("{0}\r\n<p>\r\n", HtmlEncode (this.Message));

			if (InnerException != null) {
				builder.AppendFormat ("<b>Stack Trace: </b>");
				builder.Append ("<table summary=\"Stack Trace\" width=\"100%\" " +
						"bgcolor=\"#ffffc\">\r\n<tr><td>");
				WriteTextAsCode (builder, InnerException.ToString ());
				builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
			}

			builder.Append ("<hr>\r\n</body>\r\n</html>\r\n");
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HttpUtility.HtmlEncode (this.ToString ()));

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
			StringBuilder builder = new StringBuilder ("<html>\r\n<title>");
			HtmlizedException exc = (HtmlizedException) this.InnerException;
			builder.Append (exc.Title);
			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Server Error in '{0}' " + 
					      "Application</font></h1><hr>\r\n",
					      HttpRuntime.AppDomainAppVirtualPath);

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\r\n", exc.Title);
			builder.AppendFormat ("<b>Description: </b>{0}\r\n<p>\r\n", HtmlEncode (exc.Description));
			builder.AppendFormat ("<b>Error message: </b>{0}\r\n<p>\r\n", HtmlEncode (exc.ErrorMessage));

			if (exc.FileName != null)
				builder.AppendFormat ("<b>File name: </b> {0}", HtmlEncode (exc.FileName));

			if (exc.FileText != null) {
				if (exc.SourceFile != exc.FileName)
					builder.AppendFormat ("<p><b>Source File: </b>{0}", exc.SourceFile);

				if (exc is ParseException) {
					builder.Append ("&nbsp;&nbsp;&nbsp;&nbsp;<b>Line: <b>");
					builder.Append (exc.ErrorLines [0]);
				}

				builder.Append ("\r\n<p>\r\n");

				if (exc is ParseException) {
					builder.Append ("<b>Source Error: </b>\r\n");
					builder.Append ("<table summary=\"Source error\" width=\"100%\"" +
							" bgcolor=\"#ffffc\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
				} else {
					builder.Append ("<table summary=\"Source file\" width=\"100%\" " +
							"bgcolor=\"#ffffc\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
				}
			}
			
			builder.Append ("<hr>\r\n</body>\r\n</html>\r\n");
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HtmlEncode (exc.ToString ()));
			return builder.ToString ();
		}

		static void WriteTextAsCode (StringBuilder builder, string text)
		{
			builder.Append ("<code><pre>\r\n");
			builder.AppendFormat ("{0}", HtmlEncode (text));
			builder.Append ("</pre></code>\r\n");
		}

		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			if (e is CompilationException)
				WriteCompilationSource (builder, e);
			else
				WritePageSource (builder, e);

			builder.Append ("</pre></code>\r\n");
		}
		
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
		
		[MonoTODO("Check error type and Set the correct error code")]
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

