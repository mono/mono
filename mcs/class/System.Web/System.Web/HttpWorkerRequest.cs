//
// System.Web.HttoWorkerRequest.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

//
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ComVisible (false)]
	public abstract partial class HttpWorkerRequest
	{
		public delegate void EndOfSendNotification (HttpWorkerRequest wr, object extraData);

		public const int HeaderCacheControl = 0;
		public const int HeaderConnection = 1;
		public const int HeaderDate = 2;
		public const int HeaderKeepAlive = 3;
		public const int HeaderPragma = 4;
		public const int HeaderTrailer = 5;
		public const int HeaderTransferEncoding = 6;
		public const int HeaderUpgrade = 7;
		public const int HeaderVia = 8;
		public const int HeaderWarning = 9;
		public const int HeaderAllow = 10;
		public const int HeaderContentLength = 11;
		public const int HeaderContentType = 12;
		public const int HeaderContentEncoding = 13;
		public const int HeaderContentLanguage = 14;
		public const int HeaderContentLocation = 15;
		public const int HeaderContentMd5 = 16;
		public const int HeaderContentRange = 17;
		public const int HeaderExpires = 18;
		public const int HeaderLastModified = 19;
		public const int HeaderAccept = 20;
		public const int HeaderAcceptCharset = 21;
		public const int HeaderAcceptEncoding = 22;
		public const int HeaderAcceptLanguage = 23;
		public const int HeaderAuthorization = 24;
		public const int HeaderCookie = 25;
		public const int HeaderExpect = 26;
		public const int HeaderFrom = 27;
		public const int HeaderHost = 28;
		public const int HeaderIfMatch = 29;
		public const int HeaderIfModifiedSince = 30;
		public const int HeaderIfNoneMatch = 31;
		public const int HeaderIfRange = 32;
		public const int HeaderIfUnmodifiedSince = 33;
		public const int HeaderMaxForwards = 34;
		public const int HeaderProxyAuthorization = 35;
		public const int HeaderReferer = 36;
		public const int HeaderRange = 37;
		public const int HeaderTe = 38;
		public const int HeaderUserAgent = 39;
		public const int RequestHeaderMaximum = 40;

		public const int HeaderAcceptRanges = 20;
		public const int HeaderAge = 21;
		public const int HeaderEtag = 22;
		public const int HeaderLocation = 23;
		public const int HeaderProxyAuthenticate = 24;
		public const int HeaderRetryAfter = 25;
		public const int HeaderServer = 26;
		public const int HeaderSetCookie = 27;
		public const int HeaderVary = 28;
		public const int HeaderWwwAuthenticate = 29;
		public const int ResponseHeaderMaximum = 30;

		public const int ReasonResponseCacheMiss = 0;
		public const int ReasonFileHandleCacheMiss = 1;
		public const int ReasonCachePolicy = 2;
		public const int ReasonCacheSecurity = 3;
		public const int ReasonClientDisconnect = 4;
		public const int ReasonDefault = 0;
		static readonly Dictionary <string, int> RequestHeaderIndexer;
		static readonly Dictionary <string, int> ResponseHeaderIndexer;
		
		static HttpWorkerRequest ()
		{
			RequestHeaderIndexer = new Dictionary <string, int> (StringComparer.OrdinalIgnoreCase);			
			for (int i = 0; i < RequestHeaderMaximum; i++)
				RequestHeaderIndexer.Add (GetKnownRequestHeaderName(i), i);

			ResponseHeaderIndexer = new Dictionary <string, int> (StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < ResponseHeaderMaximum; i++)
				ResponseHeaderIndexer.Add (GetKnownResponseHeaderName(i), i);
		}

		bool started_internally;
		internal bool StartedInternally {
			get { return started_internally; }
			set { started_internally = value; }
		}

		public virtual string MachineConfigPath {
			get {
				return null;
			}
		}

		public virtual string MachineInstallDirectory {
			get {
				return null;
			}
		}

		public virtual Guid RequestTraceIdentifier {
			get { return Guid.Empty; }
		}

		public virtual string RootWebConfigPath {
			get { return null; }
		}

		public virtual void CloseConnection ()
		{
		}

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

		public virtual string GetFilePath ()
		{
			return null;
		}

		public virtual string GetFilePathTranslated ()
		{
			return null;
		}

		public virtual string GetKnownRequestHeader (int index)
		{
			return null;
		}

		public virtual string GetPathInfo ()
		{
			return "";
		}

		public virtual byte [] GetPreloadedEntityBody ()
		{
			return null;
		}

		public virtual int GetPreloadedEntityBody (byte[] buffer, int offset)
		{
			return 0;
		}

		public virtual int GetPreloadedEntityBodyLength ()
		{
			return 0;
		}

		public virtual string GetProtocol ()
		{
			if (IsSecure ())
				return "https";
			else
				return "http";
		}
		
		public virtual byte [] GetQueryStringRawBytes ()
		{
			return null;
		}

		public virtual string GetRemoteName ()
		{
			return GetRemoteAddress ();
		}
		
		public virtual int GetRequestReason ()
		{
			return 0;
		}

		public virtual string GetServerName ()
		{
			return GetLocalAddress ();
		}

		public virtual string GetServerVariable (string name)
		{
			return null;
		}

		public virtual int GetTotalEntityBodyLength ()
		{
			return 0;
		}

		public virtual string GetUnknownRequestHeader (string name)
		{
			return null;
		}

		[CLSCompliant (false)]
		public virtual string [][] GetUnknownRequestHeaders ()
		{
			return null;
		}

		public virtual IntPtr GetUserToken ()
		{
			return IntPtr.Zero;
		}

		public bool HasEntityBody ()
		{
			// TODO this is not virtual.
			
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

		public virtual int ReadEntityBody (byte [] buffer, int size)
		{
			return 0;
		}

		public virtual int ReadEntityBody (byte [] buffer, int offset, int size)
		{
			byte[] temp = new byte [size];
			int n = ReadEntityBody (temp, size);

			if(n > 0)
				Array.Copy (temp, 0, buffer, offset, n);

			return n;
		}

		public virtual void SendCalculatedContentLength (long contentLength)
		{
			SendCalculatedContentLength ((int)contentLength);
		}

		public virtual void SendCalculatedContentLength (int contentLength)
		{
			// apparently does nothing in MS.NET
		}

#if !TARGET_JVM
		public virtual void SendResponseFromMemory (IntPtr data, int length)
		{
			if (data != IntPtr.Zero) {
				byte [] copy = new byte [length];
				Marshal.Copy (data, copy, 0, length);
				SendResponseFromMemory (copy, length);
			}
		}
#endif

		public virtual void SetEndOfSendNotification (HttpWorkerRequest.EndOfSendNotification callback, object extraData)
		{
		}

#region Abstract methods
		public abstract void EndOfRequest ();
		public abstract void FlushResponse (bool finalFlush);
		public abstract string GetHttpVerbName ();
		public abstract string GetHttpVersion ();
		public abstract string GetLocalAddress ();
		public abstract int GetLocalPort ();
		public abstract string GetQueryString ();
		public abstract string GetRawUrl ();
		public abstract string GetRemoteAddress ();
		public abstract int GetRemotePort ();
		public abstract string GetUriPath ();
		public abstract void SendKnownResponseHeader (int index, string value);
		public abstract void SendResponseFromFile (IntPtr handle, long offset, long length);
		public abstract void SendResponseFromFile (string filename, long offset, long length);
		public abstract void SendResponseFromMemory (byte [] data, int length);
		public abstract void SendStatus (int statusCode, string statusDescription);
		public abstract void SendUnknownResponseHeader (string name, string value);
#endregion
		
#region Static methods
		
		public static int GetKnownRequestHeaderIndex (string header)
		{
			int index;
			if (RequestHeaderIndexer.TryGetValue (header, out index))
				return index;

			return -1;
		}

		public static string GetKnownRequestHeaderName (int index)
		{
			switch (index){
			case HeaderCacheControl: return "Cache-Control";
			case HeaderConnection: return "Connection";
			case HeaderDate: return "Date";
			case HeaderKeepAlive: return "Keep-Alive";
			case HeaderPragma: return "Pragma";
			case HeaderTrailer: return "Trailer";
			case HeaderTransferEncoding: return "Transfer-Encoding";
			case HeaderUpgrade: return "Upgrade";
			case HeaderVia: return "Via";
			case HeaderWarning: return "Warning";
			case HeaderAllow: return "Allow";
			case HeaderContentLength: return "Content-Length";
			case HeaderContentType: return "Content-Type";
			case HeaderContentEncoding: return "Content-Encoding";
			case HeaderContentLanguage: return "Content-Language";
			case HeaderContentLocation: return "Content-Location";
			case HeaderContentMd5: return "Content-MD5";
			case HeaderContentRange: return "Content-Range";
			case HeaderExpires: return "Expires";
			case HeaderLastModified: return "Last-Modified";
			case HeaderAccept: return "Accept";
			case HeaderAcceptCharset: return "Accept-Charset";
			case HeaderAcceptEncoding: return "Accept-Encoding";
			case HeaderAcceptLanguage: return "Accept-Language";
			case HeaderAuthorization: return "Authorization";
			case HeaderCookie: return "Cookie";
			case HeaderExpect: return "Expect";
			case HeaderFrom: return "From";
			case HeaderHost: return "Host";
			case HeaderIfMatch: return "If-Match";
			case HeaderIfModifiedSince: return "If-Modified-Since";
			case HeaderIfNoneMatch: return "If-None-Match";
			case HeaderIfRange: return "If-Range";
			case HeaderIfUnmodifiedSince: return "If-Unmodified-Since";
			case HeaderMaxForwards: return "Max-Forwards";
			case HeaderProxyAuthorization: return "Proxy-Authorization";
			case HeaderReferer: return "Referer";
			case HeaderRange: return "Range";
			case HeaderTe: return "TE";
			case HeaderUserAgent: return "User-Agent";
			}
			throw new IndexOutOfRangeException ("index");
				
		}

		public static int GetKnownResponseHeaderIndex (string header)
		{
			int index;
			if (ResponseHeaderIndexer.TryGetValue (header, out index))
				return index;

			return -1;
		}

		public static string GetKnownResponseHeaderName (int index)
		{
			switch (index){
			case HeaderCacheControl: return "Cache-Control";
			case HeaderConnection: return "Connection";
			case HeaderDate: return "Date";
			case HeaderKeepAlive: return "Keep-Alive";
			case HeaderPragma: return "Pragma";
			case HeaderTrailer: return "Trailer";
			case HeaderTransferEncoding: return "Transfer-Encoding";
			case HeaderUpgrade: return "Upgrade";
			case HeaderVia: return "Via";
			case HeaderWarning: return "Warning";
			case HeaderAllow: return "Allow";
			case HeaderContentLength: return "Content-Length";
			case HeaderContentType: return "Content-Type";
			case HeaderContentEncoding: return "Content-Encoding";
			case HeaderContentLanguage: return "Content-Language";
			case HeaderContentLocation: return "Content-Location";
			case HeaderContentMd5: return "Content-MD5";
			case HeaderContentRange: return "Content-Range";
			case HeaderExpires: return "Expires";
			case HeaderLastModified: return "Last-Modified";
			case HeaderAcceptRanges: return "Accept-Ranges";
			case HeaderAge: return "Age";
			case HeaderEtag: return "ETag";
			case HeaderLocation: return "Location";
			case HeaderProxyAuthenticate: return "Proxy-Authenticate";
			case HeaderRetryAfter: return "Retry-After";
			case HeaderServer: return "Server";
			case HeaderSetCookie: return "Set-Cookie";
			case HeaderVary: return "Vary";
			case HeaderWwwAuthenticate: return "WWW-Authenticate";
			}

			throw new IndexOutOfRangeException ("index");
		}
		
		public static string GetStatusDescription (int code)
		{
			switch (code){
			case 100: return "Continue";
			case 101: return "Switching Protocols";
			case 102: return "Processing";
			case 200: return "OK";
			case 201: return "Created";
			case 202: return "Accepted";
			case 203: return "Non-Authoritative Information";
			case 204: return "No Content";
			case 205: return "Reset Content";
			case 206: return "Partial Content";
			case 207: return "Multi-Status";
			case 300: return "Multiple Choices";
			case 301: return "Moved Permanently";
			case 302: return "Found";
			case 303: return "See Other";
			case 304: return "Not Modified";
			case 305: return "Use Proxy";
			case 307: return "Temporary Redirect";
			case 400: return "Bad Request";
			case 401: return "Unauthorized";
			case 402: return "Payment Required";
			case 403: return "Forbidden";
			case 404: return "Not Found";
			case 405: return "Method Not Allowed";
			case 406: return "Not Acceptable";
			case 407: return "Proxy Authentication Required";
			case 408: return "Request Timeout";
			case 409: return "Conflict";
			case 410: return "Gone";
			case 411: return "Length Required";
			case 412: return "Precondition Failed";
			case 413: return "Request Entity Too Large";
			case 414: return "Request-Uri Too Long";
			case 415: return "Unsupported Media Type";
			case 416: return "Requested Range Not Satisfiable";
			case 417: return "Expectation Failed";
			case 422: return "Unprocessable Entity";
			case 423: return "Locked";
			case 424: return "Failed Dependency";
			case 500: return "Internal Server Error";
			case 501: return "Not Implemented";
			case 502: return "Bad Gateway";
			case 503: return "Service Unavailable";
			case 504: return "Gateway Timeout";
			case 505: return "Http Version Not Supported";
			case 507: return "Insufficient Storage";
			}
			return "";
		}
#endregion

#region Internals
		//
		// These are exposed on the public API, but are for internal consumption
		//
		public virtual byte [] GetClientCertificate ()
		{
			return new byte [0];
		}
		
		public virtual byte [] GetClientCertificateBinaryIssuer ()
		{
			return new byte [0];
		}
		
		public virtual int GetClientCertificateEncoding  ()
		{
			return 0;
		}
		
		public virtual byte [] GetClientCertificatePublicKey ()
		{
			return new byte [0];
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

		public virtual long GetUrlContextID ()
		{
			return 0;
		}

		public virtual IntPtr GetVirtualPathToken ()
		{
			return IntPtr.Zero;
		}
#endregion
		
	}
}
