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
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web
{
	public class HttpException : ExternalException
	{
		int _HttpCode;
		int _HR;
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
			_HttpCode = iHttpCode;
		}

		public HttpException (int iHttpCode, string sMessage, int iHR) : base (sMessage)
		{
			_HttpCode = iHttpCode;
			_HR = iHR;
		}

		public HttpException (string sMessage, int iHR) : base (sMessage)
		{
			_HR = iHR;
		}

		public HttpException (int iHttpCode,
				      string sMessage,
				      Exception InnerException)
			: base (sMessage, InnerException)
		{
			_HttpCode = iHttpCode;
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
			StringBuilder builder = new StringBuilder ("<html>\n<title>");
			builder.Append ("Error"); //FIXME
			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Error in '{0}' " + 
					      "Application</font></h1><hr>\n",
					      HttpRuntime.AppDomainAppVirtualPath);

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\n", "Error"); //FIXME
			builder.AppendFormat ("<b>Description: </b>{0}\n<p>\n", "Error processing request.");
			builder.AppendFormat ("<b>Error Message: </b>{0}\n<p>\n", HtmlEncode (this.Message));
			builder.AppendFormat ("<b>Stack Trace: </b>");

			Exception e = (InnerException != null) ? InnerException : this;
			builder.Append ("<table summary=\"Stack Trace\" width=\"100%\" bgcolor=\"#ffffc\">\n<tr><td>");
			WriteTextAsCode (builder, e.ToString ());
			builder.Append ("</td></tr>\n</table>\n<p>\n");

			builder.Append ("<hr>\n</body>\n</html>\n");
			builder.AppendFormat ("<!--\n{0}\n-->\n", HttpUtility.HtmlEncode (this.ToString ()));

			return builder.ToString ();
		}

		static string HtmlEncode (string s)
		{
			if (s == null)
				return s;

			string res = HttpUtility.HtmlEncode (s);
			return res.Replace ("\n", "<br />");
		}
		
		string GetHtmlizedErrorMessage ()
		{
			StringBuilder builder = new StringBuilder ("<html>\n<title>");
			HtmlizedException exc = (HtmlizedException) this.InnerException;
			builder.Append (exc.Title);
			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Server Error in '{0}' " + 
					      "Application</font></h1><hr>\n",
					      HttpRuntime.AppDomainAppVirtualPath);

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\n", exc.Title);
			builder.AppendFormat ("<b>Description: </b>{0}\n<p>\n", HtmlEncode (exc.Description));
			builder.AppendFormat ("<b>Error message: </b>{0}\n<p>\n", HtmlEncode (exc.ErrorMessage));

			if (exc.FileName != null)
				builder.AppendFormat ("<b>File name: </b> {0}", HtmlEncode (exc.FileName));

			if (exc.FileText != null) {
				if (exc.SourceFile != exc.FileName)
					builder.AppendFormat ("<p><b>Source File: </b>{0}", exc.SourceFile);

				if (exc is ParseException) {
					builder.Append ("&nbsp;&nbsp;&nbsp;&nbsp;<b>Line: <b>");
					builder.Append (exc.ErrorLines [0]);
				}

				builder.Append ("\n<p>\n");

				if (exc is ParseException) {
					builder.Append ("<b>Source Error: </b>\n");
					builder.Append ("<table summary=\"Source error\" width=\"100%\"" +
							" bgcolor=\"#ffffc\">\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\n</table>\n<p>\n");
				} else {
					builder.Append ("<table summary=\"Source file\" width=\"100%\" " +
							"bgcolor=\"#ffffc\">\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\n</table>\n<p>\n");
				}
			}
			
			builder.Append ("<hr>\n</body>\n</html>\n");
			builder.AppendFormat ("<!--\n{0}\n-->\n", HtmlEncode (exc.ToString ()));
			return builder.ToString ();
		}

		static void WriteTextAsCode (StringBuilder builder, string text)
		{
			builder.Append ("<code><pre>\n");
			builder.AppendFormat ("{0}", HtmlEncode (text));
			builder.Append ("</pre></code>\n");
		}

		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			if (e is CompilationException)
				WriteCompilationSource (builder, e);
			else
				WritePageSource (builder, e);

			builder.Append ("</pre></code>\n");
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

				builder.AppendFormat ("Line {0}: {1}\n", line, HtmlEncode (s));

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

				builder.AppendFormat ("{0}\n", HtmlEncode (s));

				if (enderror <= line) {
					builder.Append ("</span>");
					enderror = end + 1; // one shot
				}
			}
		}
		
		[MonoTODO("Check error type and Set the correct error code")]
		public int GetHttpCode ()
		{
			return _HttpCode;
		}

		public static HttpException CreateFromLastError (string message)
		{
			WebTrace.WriteLine ("CreateFromLastError");
			return new HttpException (message, 0);
		}
	}
}

