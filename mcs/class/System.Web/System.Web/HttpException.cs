// 
// System.Web.HttpException
//
// Author:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Runtime.InteropServices;

namespace System.Web
{
	[MonoTODO("This class contains a lot of windows specific methods, solve this.. :)")]
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

		[MonoTODO("Should get an correct html message depending on error type")]
		public string GetHtmlErrorMessage ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Check error type and Set the correct error code")]
		public int GetHttpCode ()
		{
			return _HttpCode;
		}

		[MonoTODO("Get the last error code")]
		public static HttpException CreateFromLastError (string Message)
		{
			return new HttpException (Message);
		}
	}
}

