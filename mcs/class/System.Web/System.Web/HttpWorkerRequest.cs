// 
// System.Web.HttpWorkerRequest
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   	(constants from Bob Smith (bob@thestuff.net))
//   	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Patrik Torstensson
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Web
{
	public abstract class HttpWorkerRequest : IHttpMapPath
	{
		public delegate void EndOfSendNotification (HttpWorkerRequest wr, object extraData);

		public const int HeaderAccept = 20;
		public const int HeaderAcceptCharset = 21;
		public const int HeaderAcceptEncoding = 22;
		public const int HeaderAcceptLanguage = 23;
		public const int HeaderAcceptRanges = 20;
		public const int HeaderAge = 21;
		public const int HeaderAllow = 10;
		public const int HeaderAuthorization = 24;
		public const int HeaderCacheControl = 0;
		public const int HeaderConnection = 1;
		public const int HeaderContentEncoding = 13;
		public const int HeaderContentLanguage = 14;
		public const int HeaderContentLength = 11;
		public const int HeaderContentLocation = 15;
		public const int HeaderContentMd5 = 16;
		public const int HeaderContentRange = 17;
		public const int HeaderContentType = 12;
		public const int HeaderCookie = 25;
		public const int HeaderDate = 2;
		public const int HeaderEtag = 22;
		public const int HeaderExpect = 26;
		public const int HeaderExpires = 18;
		public const int HeaderFrom = 27;
		public const int HeaderHost = 28;
		public const int HeaderIfMatch = 29;
		public const int HeaderIfModifiedSince = 30;
		public const int HeaderIfNoneMatch = 31;
		public const int HeaderIfRange = 32;
		public const int HeaderIfUnmodifiedSince = 33;
		public const int HeaderKeepAlive = 3;
		public const int HeaderLastModified = 19;
		public const int HeaderLocation = 23;
		public const int HeaderMaxForwards = 34;
		public const int HeaderPragma = 4;
		public const int HeaderProxyAuthenticate = 24;
		public const int HeaderProxyAuthorization = 35;
		public const int HeaderRange = 37;
		public const int HeaderReferer = 36;
		public const int HeaderRetryAfter = 25;
		public const int HeaderServer = 26;
		public const int HeaderSetCookie = 27;
		public const int HeaderTe = 38;
		public const int HeaderTrailer = 5;
		public const int HeaderTransferEncoding = 6;
		public const int HeaderUpgrade = 7;
		public const int HeaderUserAgent = 39;
		public const int HeaderVary = 28;
		public const int HeaderVia = 8;
		public const int HeaderWarning = 9;
		public const int HeaderWwwAuthenticate = 29;
		public const int ReasonCachePolicy = 2;
		public const int ReasonCacheSecurity = 3;
		public const int ReasonClientDisconnect = 4;
		public const int ReasonDefault = 0;
		public const int ReasonFileHandleCacheMiss = 1;
		public const int ReasonResponseCacheMiss = 0;
		public const int RequestHeaderMaximum = 40;
		public const int ResponseHeaderMaximum = 30;

		static string [][] s_HttpStatusDescriptions;
		static string [] s_HttpRequestHeaderNames;
		static string [] s_HttpResponseHeaderNames;

		static Hashtable s_HttpResponseHeadersTable;
		static Hashtable s_HttpRequestHeaderTable;

		static HttpWorkerRequest ()
		{
			string [] sSubCodes;

			s_HttpStatusDescriptions = new string [6][];

			sSubCodes = new String [3];
			sSubCodes [0] = "Continue";
			sSubCodes [1] = "Switching Protocols";
			sSubCodes [2] = "Processing";
			s_HttpStatusDescriptions [1] = sSubCodes;

			sSubCodes = new String [8];
			sSubCodes [0] = "OK";
			sSubCodes [1] = "Created";
			sSubCodes [2] = "Accepted";
			sSubCodes [3] = "Non-Authoritative Information";
			sSubCodes [4] = "No Content";
			sSubCodes [5] = "Reset Content";
			sSubCodes [6] = "Partial Content";
			sSubCodes [7] = "Multi-Status";
			s_HttpStatusDescriptions [2] = sSubCodes;

			sSubCodes = new String [8];
			sSubCodes [0] = "Multiple Choices";
			sSubCodes [1] = "Moved Permanently";
			sSubCodes [2] = "Found";
			sSubCodes [3] = "See Other";
			sSubCodes [4] = "Not Modified";
			sSubCodes [5] = "Use Proxy";
			sSubCodes [6] = String.Empty;
			sSubCodes [7] = "Temporary Redirect";
			s_HttpStatusDescriptions [3] = sSubCodes;

			sSubCodes = new String [24];
			sSubCodes [0] = "Bad Request";
			sSubCodes [1] = "Unauthorized";
			sSubCodes [2] = "Payment Required";
			sSubCodes [3] = "Forbidden";
			sSubCodes [4] = "Not Found";
			sSubCodes [5] = "Method Not Allowed";
			sSubCodes [6] = "Not Acceptable";
			sSubCodes [7] = "Proxy Authentication Required";
			sSubCodes [8] = "Request Timeout";
			sSubCodes [9] = "Conflict";
			sSubCodes [10] = "Gone";
			sSubCodes [11] = "Length Required";
			sSubCodes [12] = "Precondition Failed";
			sSubCodes [13] = "Request Entity Too Large";
			sSubCodes [14] = "Request-Uri Too Long";
			sSubCodes [15] = "Unsupported Media Type";
			sSubCodes [16] = "Requested Range Not Satisfiable";
			sSubCodes [17] = "Expectation Failed";
			sSubCodes [18] = String.Empty;
			sSubCodes [19] = String.Empty;
			sSubCodes [20] = String.Empty;
			sSubCodes [21] = "Unprocessable Entity";
			sSubCodes [22] = "Locked";
			sSubCodes [23] = "Failed Dependency";
			s_HttpStatusDescriptions [4] = sSubCodes;

			sSubCodes = new String [8];
			sSubCodes [0] = "Internal Server Error";
			sSubCodes [1] = "Not Implemented";
			sSubCodes [2] = "Bad Gateway";
			sSubCodes [3] = "Service Unavailable";
			sSubCodes [4] = "Gateway Timeout";
			sSubCodes [5] = "Http Version Not Supported";
			sSubCodes [6] = String.Empty;
			sSubCodes [7] = "Insufficient Storage";
			s_HttpStatusDescriptions [5] = sSubCodes;     

			InitLookupTables ();
		}

		protected HttpWorkerRequest ()
		{
		}

		static private void InitLookupTables ()
		{
			// Performance arrays
			s_HttpRequestHeaderNames = new string [40];
			s_HttpResponseHeaderNames = new string [30];

			// Lookup tables (name -> id)
			s_HttpRequestHeaderTable = new Hashtable ();
			s_HttpResponseHeadersTable = new Hashtable ();

			AddHeader (true, true, 0, "Cache-Control");
			AddHeader (true, true, 1, "Connection");
			AddHeader (true, true, 2, "Date");
			AddHeader (true, true, 3, "Keep-Alive");
			AddHeader (true, true, 4, "Pragma");
			AddHeader (true, true, 5, "Trailer");
			AddHeader (true, true, 6, "Transfer-Encoding");
			AddHeader (true, true, 7, "Upgrade");
			AddHeader (true, true, 8, "Via");
			AddHeader (true, true, 9, "Warning");
			AddHeader (true, true, 10, "Allow");
			AddHeader (true, true, 11, "Content-Length");
			AddHeader (true, true, 12, "Content-Type");
			AddHeader (true, true, 13, "Content-Encoding");
			AddHeader (true, true, 14, "Content-Language");
			AddHeader (true, true, 15, "Content-Location");
			AddHeader (true, true, 16, "Content-MD5");
			AddHeader (true, true, 17, "Content-Range");
			AddHeader (true, true, 18, "Expires");
			AddHeader (true, true, 19, "Last-Modified");
			AddHeader (true, false, 20, "Accept");
			AddHeader (true, false, 21, "Accept-Charset");
			AddHeader (true, false, 22, "Accept-Encoding");
			AddHeader (true, false, 23, "Accept-Language");
			AddHeader (true, false, 24, "Authorization");
			AddHeader (true, false, 25, "Cookie");
			AddHeader (true, false, 26, "Expect");
			AddHeader (true, false, 27, "From");
			AddHeader (true, false, 28, "Host");
			AddHeader (true, false, 29, "If-Match");
			AddHeader (true, false, 30, "If-Modified-Since");
			AddHeader (true, false, 31, "If-None-Match");
			AddHeader (true, false, 32, "If-Range");
			AddHeader (true, false, 33, "If-Unmodified-Since");
			AddHeader (true, false, 34, "Max-Forwards");
			AddHeader (true, false, 35, "Proxy-Authorization");
			AddHeader (true, false, 36, "Referer");
			AddHeader (true, false, 37, "Range");
			AddHeader (true, false, 38, "TE");
			AddHeader (true, false, 39, "User-Agent");
			AddHeader (false, true, 20, "Accept-Ranges");
			AddHeader (false, true, 21, "Age");
			AddHeader (false, true, 22, "ETag");
			AddHeader (false, true, 23, "Location");
			AddHeader (false, true, 24, "Proxy-Authenticate");
			AddHeader (false, true, 25, "Retry-After");
			AddHeader (false, true, 26, "Server");
			AddHeader (false, true, 27, "Set-Cookie");
			AddHeader (false, true, 28, "Vary");
			AddHeader (false, true, 29, "WWW-Authenticate");
		}

		static private void AddHeader(bool bRequest, bool bResponse, int iID, string sHeader)
		{
			if (bResponse) {
				s_HttpResponseHeaderNames [iID] = sHeader;
				s_HttpResponseHeadersTable.Add (sHeader, iID);
			}

			if (bRequest) {
				s_HttpRequestHeaderNames [iID] = sHeader;
				s_HttpRequestHeaderTable.Add (sHeader, iID);
			}
		}

		public virtual void CloseConnection ()
		{
		}
		
		public abstract void EndOfRequest ();
		public abstract void FlushResponse (bool finalFlush);

		public virtual string GetAppPath ()
		{
			return null;
		}

		public virtual string GetAppPathTranslated ()
		{
			return null;
		}

		public virtual string GetAppPoolID ()
		{
			return null;
		}

		public virtual long GetBytesRead ()
		{
			return 0;
		}

		public virtual byte[] GetClientCertificate ()
		{
			return new byte[0];
		}

		public virtual byte[] GetClientCertificateBinaryIssuer ()
		{
			return new byte[0];
		}

		public virtual int GetClientCertificateEncoding ()
		{
			return 0;
		}

		public virtual byte[] GetClientCertificatePublicKey ()
		{
			return new byte[0];
		}

		public virtual DateTime GetClientCertificateValidFrom ()
		{
			return DateTime.Now;
		}

		public virtual DateTime GetClientCertificateValidUntil ()
		{
			return DateTime.Now;
		}

		public virtual long GetConnectionID ()
		{
			return 0;
		}

		public virtual string GetFilePath ()
		{
			return GetUriPath();
		}

		public virtual string GetFilePathTranslated ()
		{
			return null;
		}

		public abstract string GetHttpVerbName ();
		public abstract string GetHttpVersion ();

		public virtual string GetKnownRequestHeader (int index)
		{
			return null;
		}

		public static int GetKnownRequestHeaderIndex (string header)
		{
			object index;
			index = s_HttpRequestHeaderTable [header];
			if (null != index)
				return (Int32) index;

			return -1;
		}

		public static string GetKnownRequestHeaderName (int index)
		{
			return s_HttpRequestHeaderNames [index];
		}

		public static int GetKnownResponseHeaderIndex (string header)
		{
			object index;

			index = s_HttpResponseHeadersTable [header];
			if (null != index)
				return (Int32) index;

			return -1;
		}

		public static string GetKnownResponseHeaderName(int index)
		{
			return s_HttpResponseHeaderNames [index];
		}

		public abstract string GetLocalAddress ();
		public abstract int GetLocalPort ();

		public virtual string GetPathInfo ()
		{
			return String.Empty;
		}

		public virtual byte [] GetPreloadedEntityBody ()
		{
			return null;
		}

		public virtual string GetProtocol ()
		{
			return (IsSecure ()) ? "https" : "http";
		}

		public abstract string GetQueryString ();

		public virtual byte[] GetQueryStringRawBytes ()
		{
			return null;
		}

		public abstract string GetRawUrl ();
		public abstract string GetRemoteAddress ();

		public virtual string GetRemoteName ()
		{
			return GetRemoteAddress();
		}

		public abstract int GetRemotePort ();

		public virtual int GetRequestReason ()
		{
			return 0;
		}

		public virtual string GetServerName ()
		{
			return GetLocalAddress();  
		}

		public virtual string GetServerVariable (string name)
		{
			return null;
		}

		public static string GetStatusDescription (int code)
		{
			if (code>= 100 && code < 600) {
				int iMajor = code / 100;
				int iMinor = code % 100;
				if (iMinor < (int) s_HttpStatusDescriptions [iMajor].Length)
					return s_HttpStatusDescriptions [iMajor][iMinor];
			}

			return String.Empty;
		}

		public virtual string GetUnknownRequestHeader (string name)
		{
			return null;
		}

		[CLSCompliant(false)]
		public virtual string [][] GetUnknownRequestHeaders ()
		{
			return null;
		}

		public abstract string GetUriPath ();

		public virtual long GetUrlContextID ()
		{
			return 0;
		}

		public virtual IntPtr GetUserToken ()
		{
			throw new NotSupportedException ();
		}

		public virtual IntPtr GetVirtualPathToken ()
		{
			throw new NotSupportedException ();
		}

		public bool HasEntityBody ()
		{
			string sContentLength = GetKnownRequestHeader (HeaderContentLength);
			if (null != sContentLength && sContentLength != "0")
				return true;

			if (null != GetKnownRequestHeader (HeaderTransferEncoding))
				return true;

			if (null != GetPreloadedEntityBody () || IsEntireEntityBodyIsPreloaded ())
				return true;

			return false;
		}

		public virtual bool HeadersSent ()
		{
			return true;
		}

		public virtual bool IsClientConnected ()
		{
			return true;
		}

		public virtual bool IsEntireEntityBodyIsPreloaded ()
		{
			return false;
		}

		public virtual bool IsSecure () 
		{
			return false;
		}

		public virtual string MapPath (string virtualPath)
		{
			return null;
		}

		public virtual int ReadEntityBody (byte[] buffer, int size)
		{
			return 0;
		}

		public virtual void SendCalculatedContentLength (int contentLength)
		{
		}

		public abstract void SendKnownResponseHeader (int index, string value);
		public abstract void SendResponseFromFile (IntPtr handle, long offset, long length);
		public abstract void SendResponseFromFile (string filename, long offset, long length);
		public abstract void SendResponseFromMemory (byte [] data, int length);

		public virtual void SendResponseFromMemory (IntPtr data, int length)
		{
			if (length <= 0)
				return;

			byte [] dataBytes = new byte [length];
			Marshal.Copy (data, dataBytes, 0, length);
			SendResponseFromMemory (dataBytes, length);
		}

		public abstract void SendStatus (int statusCode, string statusDescription);
		public abstract void SendUnknownResponseHeader (string name, string value);

		public virtual void SetEndOfSendNotification (EndOfSendNotification callback, object extraData)
		{
		}

		public virtual string MachineConfigPath
		{
			get {
				return null;
			}
		}

		public virtual string MachineInstallDirectory
		{
			get {
				return null;
			}
		}
	}
}

