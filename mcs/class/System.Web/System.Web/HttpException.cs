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
using System.Text;
using System.Runtime.InteropServices;
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
			FormatReader (builder, new StringReader (e.ToString ()), 0, false);
			builder.Append ("</td></tr>\n</table>\n<p>\n");

			builder.Append ("<hr>\n</body>\n</html>\n");
			builder.AppendFormat ("<!--\n{0}\n-->\n", HtmlEncode (this.ToString ()));

			return builder.ToString ();
		}

		static string HtmlEncode (string s)
		{
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

			if (exc is ParseException)
				builder.Append ("<b>File name: </b>" + exc.FileName);
			else if (exc is CompilationException) {
				builder.Append ("<table summary=\"Source file\" width=\"100%\" bgcolor=\"#ffffc\">\n<tr><td>");
				FormatReader (builder, ((CompilationException) exc).File, 0);
				builder.Append ("</td></tr>\n</table>\n<p>\n");
			}
			/*
			if (exc.HaveSourceError) {
				builder.Append ("<b>Source Error: </b>\n<p>\n");
				builder.Append ("<table summary=\"Source error\" width=\"100%\" bgcolor=\"#ffffc\">\n<tr><td>");
				FormatReader (builder, exc.SourceError, exc.SourceErrorLine);
				builder.Append ("</td></tr>\n</table>\n<p>\n");
			}

			builder.AppendFormat ("<b>Source File: </b>{0}", exc.FileName);
			if (exc.SourceErrorLine != -1)
				builder.AppendFormat ("&nbsp;&nbsp;&nbsp;&nbsp;<b>Line:</b>{0}", exc.SourceErrorLine);

			builder.Append ("\n<p>\n");

			if (exc.HaveSourceFile) {
				builder.Append ("<table summary=\"Source file\" width=\"100%\" bgcolor=\"#ffffc\">\n<tr><td>");
				FormatReader (builder, exc.SourceFile, 0);
				builder.Append ("</td></tr>\n</table>\n<p>\n");
			}
			
			*/
			builder.Append ("<hr>\n</body>\n</html>\n");
			builder.AppendFormat ("<!--\n{0}\n-->\n", HtmlEncode (exc.ToString ()));
			return builder.ToString ();
		}

		static void FormatReader (StringBuilder builder, string text, int errorLine)
		{
			FormatReader (builder, new StringReader (text), errorLine, true);
		}
		
		static void FormatReader (StringBuilder builder, TextReader reader, int errorLine)
		{
			FormatReader (builder, reader, errorLine, true);
		}
		
		static void FormatReader (StringBuilder builder, TextReader reader, int errorLine, bool lines)
		{
			int current = (errorLine > 0) ? errorLine : 1;
			string s;
			builder.Append ("<code><pre>\n");
			while ((s = reader.ReadLine ()) != null) {
				if (current == errorLine)
					builder.Append ("<span style=\"color: red\">");

				if (lines)
					builder.AppendFormat ("Line {0}: {1}\n", current++, HtmlEncode (s));
				else
					builder.AppendFormat ("{1}\n", current++, HtmlEncode (s));

				if (current == errorLine + 1)
					builder.Append ("</span>");
			}

			builder.Append ("</pre></code>\n");
		}

		[MonoTODO("Check error type and Set the correct error code")]
		public int GetHttpCode ()
		{
			return _HttpCode;
		}

/*		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int GetLastError_internal ();*/

		public static HttpException CreateFromLastError (string message)
		{
			WebTrace.WriteLine ("CreateFromLastError");
			//return new HttpException (message, GetLastError_internal ());
			return new HttpException (message, 0);
		}
	}
}

