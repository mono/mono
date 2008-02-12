//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Web.Util;
using System.Web.J2EE;
using System.Collections;
using System.Web;
using javax.servlet;
using javax.servlet.http;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Hosting;
using vmw.common;
using InputStream=java.io.InputStream;
using java.security;

namespace Mainsoft.Web.Hosting {
	[MonoTODO("Implement security demands on the path usage functions (and review)")]
	[ComVisible (false)]
	public abstract class BaseWorkerRequest : HttpWorkerRequest, IHttpExtendedWorkerRequest, IServiceProvider
	{
		OutputStreamWrapper _OutputStream;

		readonly string _contextPath;
		readonly string _requestUri;
		readonly string _pathInfo;

		static readonly StringDictionary _srvVarsToHeaderMap;

		private string [][] unknownHeaders;
		string _rawUrl;

		private HttpWorkerRequest.EndOfSendNotification _endOfSendCallback;
		private object _endOfSendArgs;

		enum KnownServerVariable {
			AUTH_TYPE,
			CONTENT_LENGTH,
			CONTENT_TYPE,
			QUERY_STRING,
			REMOTE_ADDR,
			REMOTE_HOST,
			REMOTE_USER,
			REQUEST_METHOD,
			REQUEST_URI,
			SCRIPT_NAME,
			SERVER_NAME,
			SERVER_PORT,
			SERVER_PROTOCOL,
			SERVER_SOFTWARE,
			PATH_INFO
		};

		static readonly Hashtable KnownServerVariableMap;

		static BaseWorkerRequest() {
			_srvVarsToHeaderMap = new StringDictionary();
			_srvVarsToHeaderMap.Add("HTTP_ACCEPT", "Accept");
			_srvVarsToHeaderMap.Add("HTTP_REFERER", "Referer");
			_srvVarsToHeaderMap.Add("HTTP_ACCEPT_LANGUAGE", "Accept-Language");
			_srvVarsToHeaderMap.Add("HTTP_ACCEPT_ENCODING", "Accept-Encoding");
			_srvVarsToHeaderMap.Add("HTTP_CONNECTION", "Connection");
			_srvVarsToHeaderMap.Add("HTTP_HOST", "Host");
			_srvVarsToHeaderMap.Add("HTTP_USER_AGENT", "User-Agent");
			_srvVarsToHeaderMap.Add("HTTP_SOAPACTION", "SOAPAction");

			string[] knownServerVariableNames = Enum.GetNames(typeof(KnownServerVariable));
			KnownServerVariableMap = CollectionsUtil.CreateCaseInsensitiveHashtable(knownServerVariableNames.Length);
			for (int i = 0; i < knownServerVariableNames.Length; i++)
				KnownServerVariableMap[knownServerVariableNames[i]] = (KnownServerVariable)i;
		}

		public BaseWorkerRequest (string contextPath, string servletPath, string requestURI) {


			_contextPath = contextPath;

			//string contextPath = req.getContextPath();
			//string servletPath = req.getServletPath ();
			//string requestURI = req.getRequestURI ();
			// servletPath - Returns the part of this request's URL that calls the servlet.
			//		so it contains default page.
			// requestURI - Returns the part of this request's URL from the protocol name up to the query string in the first line of the HTTP request.
			//		so it contains what the user passed.
			//
			// the one containing more information wins.
			if (contextPath.Length + servletPath.Length > requestURI.Length)
				requestURI = contextPath + servletPath;
			else { 
				int contextPos = requestURI.IndexOf(contextPath, StringComparison.Ordinal);
				if (contextPos > 0)
					requestURI = requestURI.Substring (contextPos);
			}

			_requestUri = Uri.UnescapeDataString(requestURI);
			const int dotInvokeLength = 7; //".invoke".Length
			if (_requestUri.Length > dotInvokeLength &&
				String.CompareOrdinal(".invoke", 0, _requestUri, 
				_requestUri.Length - dotInvokeLength, dotInvokeLength) == 0) {

				_requestUri = _requestUri.Substring(0, _requestUri.Length - dotInvokeLength);
				
				int paramNameStart = _requestUri.LastIndexOf('/');
				_pathInfo = _requestUri.Substring(paramNameStart, _requestUri.Length - paramNameStart);
			}

			const int aspnetconfigLength = 12; //"aspnetconfig".Length
			int endingSlash = _requestUri [_requestUri.Length - 1] == '/' ? 1 : 0;
			if (_requestUri.Length > aspnetconfigLength &&
				String.CompareOrdinal ("aspnetconfig", 0, _requestUri,
				_requestUri.Length - aspnetconfigLength - endingSlash, aspnetconfigLength) == 0) {

				if (endingSlash == 0)
					_requestUri += "/";
				_requestUri += "Default.aspx";
			}
		}

		static readonly Type typeOfWriter = typeof (java.io.Writer);
		public virtual object GetService (Type serviceType)
		{
			if (serviceType == typeOfWriter)
				return CreateOutputStream (false);
			return null;
		}
		
		[MonoTODO("Implement security")]
		public override string MachineInstallDirectory {
			get {
				return ".";
			}
		}

		public override string MachineConfigPath {
			get { return "."; }
		}

		public override void EndOfRequest () {
			if (_endOfSendCallback != null)
				_endOfSendCallback(this, _endOfSendArgs);
			_OutputStream = null;
		}

		public override void FlushResponse (bool finalFlush) {
			//kostat
			//IPortletActionResponse resp =_HttpServletResponse as IPortletActionResponse;
			//if (_OutputStream == null || resp != null && resp.isRedirected())
			//	return;
			if (_OutputStream == null)
				return;

			_OutputStream.flush();
			if (finalFlush)
				_OutputStream.close();
		}

		public override string GetAppPath () {
			return _contextPath;
		}

		public override string GetFilePath () {
			string uri = GetUriPath();
			string pathInfo = GetPathInfo();
			if (pathInfo != null && pathInfo.Length > 0)
				uri = uri.Substring (0, uri.Length - pathInfo.Length);

			return uri;
		}

		public override string GetFilePathTranslated () {
			string page = GetFilePath ();

			if (Path.DirectorySeparatorChar != '/')
				page = page.Replace ('/', Path.DirectorySeparatorChar);

			if (page [0] == Path.DirectorySeparatorChar)
				page = page.Substring (1);
			
			return Path.Combine (GetAppPathTranslated (), page);
		}

		public override string GetPathInfo () {
			return _pathInfo;
		}		

		public override string GetRawUrl () {
			if (_rawUrl == null) {
				StringBuilder builder = new StringBuilder();
				builder.Append(GetUriPath());
				string pathInfo = GetPathInfo();
				string query = GetQueryString();
				if (query != null && query.Length > 0) {
					builder.Append('?');
					builder.Append(query);
				}

				_rawUrl = builder.ToString();
			}

			return _rawUrl;
		}

		public override string GetServerVariable(string name) {
			// FIXME: We need to make a proper mapping between the standard server
			// variables and java equivalent. probably we have to have a configuration file 
			// which associates between the two. Pay a special attention on GetUnknownRequestHeader/s
			// while implementing. Ensure that system web "common" code correctly calls each method.

			string headerName = _srvVarsToHeaderMap[name];

			if (headerName != null)
				return getHeader( headerName );

			object knownVariable = KnownServerVariableMap[name];
			if (knownVariable != null)
				return GetKnownServerVariable((KnownServerVariable)knownVariable);

			return getHeader( name );
		}

		public abstract string GetAuthType ();
		protected abstract int getContentLength ();
		protected abstract string getContentType ();
		public abstract string GetRemoteUser ();
		protected abstract string getHeader (string name);
		protected abstract java.util.Enumeration getHeaderNames ();
		protected abstract InputStream getInputStream ();
		public abstract ServletContext GetContext ();
		protected abstract OutputStreamWrapper CreateOutputStream (bool binary);

		public abstract HttpSession GetSession (bool create);
		public abstract bool IsRequestedSessionIdValid ();
		public abstract string GetRequestedSessionId ();
		public abstract bool IsUserInRole (string name);
		public abstract Principal GetUserPrincipal ();

		string GetKnownServerVariable(KnownServerVariable index) {
			switch (index) {
				case KnownServerVariable.AUTH_TYPE : return GetAuthType();
				case KnownServerVariable.CONTENT_LENGTH : return Convert.ToString(getContentLength());
				case KnownServerVariable.CONTENT_TYPE : return getContentType();
				case KnownServerVariable.QUERY_STRING : return GetQueryString();
				case KnownServerVariable.REMOTE_ADDR : return GetRemoteAddress();
				case KnownServerVariable.REMOTE_HOST : return GetRemoteName();
				case KnownServerVariable.REMOTE_USER : return GetRemoteUser();
				case KnownServerVariable.REQUEST_METHOD : return GetHttpVerbName ();
				case KnownServerVariable.REQUEST_URI : return GetUriPath();
				case KnownServerVariable.SCRIPT_NAME : return GetFilePath ();
				case KnownServerVariable.SERVER_NAME : return GetServerName();
				case KnownServerVariable.SERVER_PORT : return Convert.ToString(GetLocalPort());
				case KnownServerVariable.SERVER_PROTOCOL : return GetHttpVersion ();
				case KnownServerVariable.SERVER_SOFTWARE : return GetContext().getServerInfo();
				case KnownServerVariable.PATH_INFO : return GetPathInfo();
				default: throw new IndexOutOfRangeException("index");
			}
		}

		public override string GetUriPath() {
			return _requestUri;
		}

		public override IntPtr GetUserToken() {
			return IntPtr.Zero;
		}

		public override string GetAppPathTranslated () {
			return J2EEUtils.GetApplicationRealPath (GetContext ());
		}

		public override string MapPath (string virtualPath) {
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			ServletContext context = GetContext ();

			string contextPath = GetAppPath ();
			if ((virtualPath.Length > contextPath.Length && virtualPath [contextPath.Length] != '/') ||
				string.Compare (contextPath, 0, virtualPath, 0, contextPath.Length, StringComparison.OrdinalIgnoreCase) != 0) {

				for (int appVirtualPathIndex = 0; appVirtualPathIndex > 0 && virtualPath.Length > appVirtualPathIndex; ) {
					appVirtualPathIndex = virtualPath.IndexOf ('/', appVirtualPathIndex + 1);
					string crossContextPath = appVirtualPathIndex > 0 ?
						virtualPath.Remove (appVirtualPathIndex) : virtualPath;
					ServletContext other = context.getContext (crossContextPath);
					if (other != null) {
						string appVirtualPath = appVirtualPathIndex > 0 ?
							virtualPath.Substring (appVirtualPathIndex) : String.Empty;
						return other.getRealPath (appVirtualPath);
					}
				}

				throw new HttpException (
					String.Format ("MapPath: Mapping across applications is not allowed. ApplicationPath is '{0}', VirtualPath is '{1}'.",
					contextPath, virtualPath));
			}

			string thisAppVirtualPath = virtualPath.Length > contextPath.Length ? virtualPath.Substring (contextPath.Length) : String.Empty;
			return J2EEUtils.GetApplicationRealPath (context, thisAppVirtualPath);

		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length) {
			throw new NotSupportedException();
		}

		public override void SendResponseFromFile (string filename, long offset, long length) {
			using (FileStream fs = File.OpenRead (filename)) {
				byte [] buffer = new byte [4 * 1024];

				if (offset != 0)
					fs.Position = offset;

				long remain = length;
				int n;
				while (remain > 0 && (n = fs.Read (buffer, 0, (int) Math.Min (remain, buffer.Length))) != 0){
					remain -= n;
					SendResponseFromMemory(buffer, n);
				}
			}
		}

		//kostat
		//private OutputStreamWrapper CreateOutputStream (bool binary)
		//{
		//    IPortletActionResponse resp = _HttpServletResponse as IPortletActionResponse;
		//    if (resp != null)
		//        return null; // no output stream while processAction

		//    if (_OutputStream != null)
		//        return _OutputStream;

		//    if (_HttpServletResponse != null) {
		//        if (binary)
		//            _OutputStream = new OutputStreamWrapper (_HttpServletResponse.getOutputStream ());
		//        else
		//            _OutputStream = new OutputStreamWrapper (_HttpServletResponse.getWriter ());
		//    }

		//    return _OutputStream;
		//}

		public override void SendResponseFromMemory (byte [] data, int length) {
			_OutputStream = CreateOutputStream (true);

			if (_OutputStream == null)
				return;

			sbyte [] sdata = vmw.common.TypeUtils.ToSByteArray(data);
			_OutputStream.write(sdata, 0 , length);
		}

		public override void SendKnownResponseHeader (int index, string value) {
			SendUnknownResponseHeader (GetKnownResponseHeaderName (index), value);
		}

		public override string GetKnownRequestHeader (int index) {	
			return GetUnknownRequestHeader(GetKnownRequestHeaderName (index));
		}

		public override string GetUnknownRequestHeader (string name) {
			return getHeader(name);
		}

		public override string [][] GetUnknownRequestHeaders () {
			if (unknownHeaders == null) {
				ArrayList pairs = new ArrayList ();
				for (java.util.Enumeration he = getHeaderNames(); he.hasMoreElements() ;) {
					string key = (string) he.nextElement();
					int index = HttpWorkerRequest.GetKnownRequestHeaderIndex (key);
					if (index != -1)
						continue;
					pairs.Add (new string [] {key, getHeader(key)});
				}
				
				if (pairs.Count != 0) {
					unknownHeaders = new string [pairs.Count][];
					for (int i = 0; i < pairs.Count; i++)
						unknownHeaders [i] = (string []) pairs [i];
				}
			}
			if (unknownHeaders == null) unknownHeaders = new string [0][];

			return unknownHeaders;
		}

		public override int ReadEntityBody (byte [] buffer, int size) {
			if (buffer == null || size == 0)
				return 0;
			sbyte [] sbuffer = vmw.common.TypeUtils.ToSByteArray(buffer);
			InputStream inp = getInputStream();
			if (inp == null)
				return 0;
			int r = inp.read (sbuffer, 0, size);
			return r < 0 ? 0 : r;
		}

		public override void SetEndOfSendNotification(System.Web.HttpWorkerRequest.EndOfSendNotification callback, object extraData) {
			_endOfSendCallback = callback;
			_endOfSendArgs = extraData;
		}

		public abstract BaseHttpContext CreateContext (HttpContext context);
	}
}

