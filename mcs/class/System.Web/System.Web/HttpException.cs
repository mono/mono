// 
// System.Web.HttpException
//
// Author:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Text;
//using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.Util;

namespace System.Web
{
	public class HttpException : ExternalException
	{
		int _HttpCode;
		int _HR;

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

		[MonoTODO("Format messages")]
		public string GetHtmlErrorMessage ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<html>\n<title>");
			sb.Append (HttpUtility.HtmlEncode (Message));
			sb.Append ("</title><body><h1>Error</h1>\n<pre>");
			sb.Append (HttpUtility.HtmlEncode (ToString ()));
			sb.Append ("</pre></body>\n</html>\n");
			return sb.ToString ();
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

