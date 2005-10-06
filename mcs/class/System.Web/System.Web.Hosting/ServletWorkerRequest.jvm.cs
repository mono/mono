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
using vmw.common;
using System.Web.J2EE;
using System.Collections;
using System.Web;
using javax.servlet;
using javax.servlet.http;

namespace System.Web.Hosting
{
	[MonoTODO("Implement security demands on the path usage functions (and review)")]
	[ComVisible (false)]
	internal class ServletWorkerRequest : HttpWorkerRequest
	{
		private string _Page;
		private string _Query;
		private string _PathInfo = String.Empty;
		private string _AppVirtualPath;
		private string _AppPhysicalPath;
		private string _AppInstallPath;
		private bool _HasInstallInfo;
		
		private static string SLASH = "/";

		private ServletOutputStream _ServletOutputStream;
		private HttpServlet _HttpServlet;
		private HttpServletRequest _HttpServletRequest;
		private HttpServletResponse _HttpServletResponse;
		private string [][] unknownHeaders;

		private HttpWorkerRequest.EndOfSendNotification _endOfSendCallback;
		private object _endOfSendArgs;


		private ServletWorkerRequest ()
		{
		}

		public ServletWorkerRequest (HttpServlet servlet, HttpServletRequest req, HttpServletResponse resp, ServletOutputStream output)
			:this(String.Empty, String.Empty ,String.Empty, String.Empty ,null)
		{
#if DEBUG
			Console.WriteLine("Constructor 1 of ServletWorkerRequest!! -->");
#endif
			_HttpServlet = servlet;
			_HttpServletRequest = req;
			_HttpServletResponse = resp;
			_ServletOutputStream = output;

			string contextPath = req.getContextPath();
			string requestURI = req.getRequestURI();
			if (requestURI.Equals(contextPath) ||
				((requestURI.Length - contextPath.Length) == 1) && requestURI[requestURI.Length-1] == '/' && requestURI.StartsWith(contextPath))
				requestURI = contextPath + req.getServletPath();
		
			_Page = requestURI.Substring(contextPath.Length);
			
			if (_Page.StartsWith("/"))
				_Page = _Page.Substring(1);

			_Query = req.getQueryString();
			//_PathInfo = req.getPathInfo();
			_AppVirtualPath = req.getContextPath();
			_AppPhysicalPath = J2EEUtils.GetApplicationRealPath(servlet.getServletConfig());
#if DEBUG
			LogCurrentPageLocation();
#endif
		}

		public ServletWorkerRequest (string Page, string Query, ServletOutputStream output)
			:this(Page, Query)
		{
#if DEBUG
			Console.WriteLine("Constructor 2 of ServletWorkerRequest!! -->");
#endif
			//_Page = Page;
			ParsePathInfo ();

			//_Query = Query;
			AppDomain current = AppDomain.CurrentDomain;
			object o = current.GetData (".appPath");
			if (o == null)
				throw new HttpException ("Cannot get .appPath");
			_AppPhysicalPath = (string)current.GetData(IAppDomainConfig.WEB_APP_DIR);

			o = current.GetData (".hostingVirtualPath");
			if (o == null)
				throw new HttpException ("Cannot get .hostingVirtualPath");
			_AppVirtualPath = CheckAndAddVSlash (o.ToString ());

			o = current.GetData (".hostingInstallDir");
			if (o == null)
				throw new HttpException ("Cannot get .hostingInstallDir");
			_AppInstallPath = o.ToString ();
			_ServletOutputStream = output;

			if (_AppPhysicalPath == null)
				throw new HttpException ("Invalid app domain");

			_HasInstallInfo = true;
#if DEBUG
			LogCurrentPageLocation();
#endif
		}

		public ServletWorkerRequest (string AppVirtualPath,
					  string AppPhysicalPath,
					  string Page,
					  string Query,
					  ServletOutputStream output) : this (Page, Query)
		{
#if DEBUG
			Console.WriteLine("Constructor 3 of ServletWorkerRequest!! -->");
#endif
			if (AppDomain.CurrentDomain.GetData (".appPath") == null)
				throw new HttpException ("Invalid app domain");

			//_Page = Page;
			ParsePathInfo ();
			//_Query = Query;
			_AppVirtualPath = AppVirtualPath;
			_AppPhysicalPath = CheckAndAddSlash (AppPhysicalPath);
			_ServletOutputStream = output;
			_HasInstallInfo = false;
#if DEBUG
			LogCurrentPageLocation();
#endif
		}

		public ServletWorkerRequest (string Page, string Query)
		{
			_Page = Page;

			_Query = Query;
			AppDomain current = AppDomain.CurrentDomain;
			object o = current.GetData (".appPath");
			if (o == null)
				throw new HttpException ("Cannot get .appPath");
			_AppPhysicalPath = o.ToString ();

			o = current.GetData (".hostingVirtualPath");
			if (o == null)
				throw new HttpException ("Cannot get .hostingVirtualPath");
			_AppVirtualPath = o.ToString ();

			o = current.GetData (".hostingInstallDir");
			if (o == null)
				throw new HttpException ("Cannot get .hostingInstallDir");
			_AppInstallPath = o.ToString ();

			if (_AppPhysicalPath == null)
				throw new HttpException ("Invalid app domain");

			_HasInstallInfo = true;

			ExtractPagePathInfo();
		}

		public ServletOutputStream ServletOutputStream
		{
			get	
			{
				return _ServletOutputStream;
			}
		}

		public HttpServlet Servlet
		{
			get {
				return _HttpServlet;
			}
		}
		
		public HttpServletRequest ServletRequest
		{
			get{
				return _HttpServletRequest;
			}
		}

		public HttpServletResponse ServletResponse
		{
			get{
				return _HttpServletResponse;
			}
		}
		
		[MonoTODO("Implement security")]
		public override string MachineInstallDirectory
		{
			get {
				if (_HasInstallInfo)
					return _AppInstallPath;

				return ICalls.GetMachineInstallDirectory ();
			}
		}

		public override string MachineConfigPath
		{
			get { return ICalls.GetMachineConfigPath (); }
		}

		public override void EndOfRequest ()
		{
			_ServletOutputStream = null;
			_HttpServlet = null;
			_HttpServletRequest = null;
			_HttpServletResponse = null;
			if (_endOfSendCallback != null)
				_endOfSendCallback(this, _endOfSendArgs);
		}

		public override void FlushResponse (bool finalFlush)
		{
			_ServletOutputStream.flush();
			if (finalFlush)
				_ServletOutputStream.close();
		}

		public override string GetAppPath ()
		{
			return _AppVirtualPath;
		}

		public override string GetAppPathTranslated ()
		{
			return _AppPhysicalPath;
		}

		public override string GetFilePath ()
		{
                        return CreatePath (false);
		}

		public override string GetFilePathTranslated ()
		{
                        string page = _Page;

			if (Path.DirectorySeparatorChar != '/')
                                page = _Page.Replace ('/', Path.DirectorySeparatorChar);

			if (page [0] == Path.DirectorySeparatorChar)
				page = page.Substring (1);
			
			return (Path.Combine (_AppPhysicalPath, page));
		}

		public override string GetHttpVerbName ()
		{
			return _HttpServletRequest.getMethod();
		}

		public override string GetHttpVersion ()
		{
			return _HttpServletRequest.getProtocol();
		}

		public override string GetLocalAddress ()
		{
			return _HttpServletRequest.getServerName();
		}

		public override int GetLocalPort ()
		{
			return _HttpServletRequest.getServerPort();
		}

		public override string GetPathInfo ()
		{
			return (null != _PathInfo) ? _PathInfo : String.Empty;
		}

		public override string GetQueryString ()
		{
			return _Query;
		}

		public override string GetRawUrl ()
		{
                        string path = CreatePath (true);
                        if (null != _Query && _Query.Length > 0)
				return path + "?" + _Query;

			return path;
		}

		public override string GetRemoteAddress()
		{
			return _HttpServletRequest.getRemoteAddr();
		}

		public override int GetRemotePort()
		{
			try
			{
				return _HttpServletRequest.getRemotePort();
			}
			catch(Exception e) //should catch also java.lang.Throwable
			{
				//if servlet API is 2.3 and below - there is no
				//method getRemotePort in ServletRequest interface...
				//should be described as limitation.
				return 0;
			}
		}

		public override string GetServerVariable(string name)
		{
			return String.Empty;
		}

		public override string GetUriPath()
		{
			return CreatePath (true);
		}

		public override IntPtr GetUserToken()
		{
			return IntPtr.Zero;
		}

		public override string MapPath (string path)
		{
			if (path.StartsWith(_AppVirtualPath))
			{
				path = path.Remove(0,_AppVirtualPath.Length);
				if (path.StartsWith("/"))
					path = path.Remove(0,1);
			}
			//string realPath = Servlet.getServletContext().getRealPath(path);
//			if (Path.IsPathRooted(path))
//				return path;
//			if (!path.StartsWith(IAppDomainConfig.WAR_ROOT_SYMBOL)&& 
//				!path.StartsWith("/") && !path.StartsWith("\\")&& !Path.IsPathRooted(path))            
//				return IAppDomainConfig.WAR_ROOT_SYMBOL + "/" + path;
//			else if (!path.StartsWith(IAppDomainConfig.WAR_ROOT_SYMBOL)&& !Path.IsPathRooted(path))
//				return IAppDomainConfig.WAR_ROOT_SYMBOL + path;
//			else 
//				return path;

			if (path.StartsWith(IAppDomainConfig.WAR_ROOT_SYMBOL))
			{
				return path;
			}

			string retVal =  IAppDomainConfig.WAR_ROOT_SYMBOL;

			if (!path.StartsWith("/") && !path.StartsWith("\\"))
				retVal += "/";

			retVal += path;

			return retVal;
		}

		public override void SendResponseFromFile (IntPtr handle, long offset, long length)
		{
		}

		public override void SendResponseFromFile (string filename, long offset, long length)
		{
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

		public override void SendResponseFromMemory (byte [] data, int length)
		{
			sbyte [] sdata = vmw.common.TypeUtils.ToSByteArray(data);
			_ServletOutputStream.write(sdata, 0 , length);
		}

		public override void SendStatus(int statusCode, string statusDescription)
		{
			_HttpServletResponse.setStatus(statusCode, statusDescription);
		}

		public override void SendUnknownResponseHeader(string name, string value)
		{
			_HttpServletResponse.addHeader(name, value);
		}

		public override bool HeadersSent ()
		{
			return false;
		}

		public override void SendCalculatedContentLength (int contentLength)
		{
			//FIXME: Should we ignore this for apache2?
			SendUnknownResponseHeader ("Content-Length", contentLength.ToString ());
		}

		public override void SendKnownResponseHeader (int index, string value)
		{
			if (HeadersSent ())
				return;

			string headerName = HttpWorkerRequest.GetKnownResponseHeaderName (index);
			SendUnknownResponseHeader (headerName, value);
		}
		
		// Create's a path string
		private string CheckAndAddSlash(string sPath)
		{
			if (null == sPath)
				return null;

			if (String.Empty == sPath)
				return SLASH;
				
			if (!sPath.EndsWith ("" + Path.DirectorySeparatorChar))
				return sPath + Path.DirectorySeparatorChar;

			return sPath;
		}

		// Creates a path string
		private string CheckAndAddVSlash(string sPath)
		{
			if (null == sPath)
				return null;

			if (!sPath.EndsWith ("/"))
				return sPath + "/";

			return sPath;
		}

		// Create's a path string
                private string CreatePath (bool bIncludePathInfo)
                {
                        string sPath = Path.Combine (_AppVirtualPath, _Page);

                        if (bIncludePathInfo && null != _PathInfo)
                        {
                                sPath += _PathInfo;
                        }

                        return sPath;
		}

		//  "The extra path information, as given by the client. In
		//  other words, scripts can be accessed by their virtual
		//  pathname, followed by extra information at the end of this
		//  path. The extra information is sent as PATH_INFO."
		private void ExtractPagePathInfo ()
		{
			if (_Page == null || _Page == String.Empty)
				return;

			string FullPath = GetFilePathTranslated ();
			int PathInfoLength = 0;
			string LastFile = String.Empty;

			while (PathInfoLength < _Page.Length) {
				if (LastFile.Length > 0) {
					// increase it by the length of the file plus 
					// a "/"
					//
					PathInfoLength += LastFile.Length + 1;
				}

				if (File.Exists (FullPath) == true)
					break;

				if (Directory.Exists (FullPath) == true) {
					PathInfoLength -= (LastFile.Length + 1);
					break;
				}

				LastFile = Path.GetFileName (FullPath);
				FullPath = Path.GetDirectoryName (FullPath);
			}

			if (PathInfoLength <= 0 || PathInfoLength > _Page.Length)
				return;

			_PathInfo = _Page.Substring (_Page.Length - PathInfoLength);
			_Page = _Page.Substring (0, _Page.Length - PathInfoLength);
		}
		
		// Parses out the string after / known as the "path info"
		private void ParsePathInfo ()
		{
			int iPos = _Page.LastIndexOf("/");
			if (iPos >= 0) {
				_PathInfo = _Page.Substring (iPos);
				_Page = _Page.Substring (0, iPos);
			}
		}

		public override string GetKnownRequestHeader (int index)
		{
			if (_HttpServletRequest == null)
				return null;

			string headerName = HttpWorkerRequest.GetKnownRequestHeaderName (index);
			
			return _HttpServletRequest.getHeader(headerName);
		}

		public override string GetUnknownRequestHeader (string name)
		{
			if (_HttpServletRequest == null)
				return null;

			return _HttpServletRequest.getHeader(name);
		}

		public override string [][] GetUnknownRequestHeaders ()
		{
			if (unknownHeaders == null) {
				ArrayList pairs = new ArrayList ();
				for (java.util.Enumeration he = _HttpServletRequest.getHeaderNames(); he.hasMoreElements() ;) {
					string key = (string) he.nextElement();
					int index = HttpWorkerRequest.GetKnownRequestHeaderIndex (key);
					if (index != -1)
						continue;
					pairs.Add (new string [] {key, _HttpServletRequest.getHeader(key)});
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

		public override int ReadEntityBody (byte [] buffer, int size)
		{
			if (buffer == null || size == 0)
				return 0;
			sbyte [] sbuffer = new sbyte [size];
			int r = _HttpServletRequest.getInputStream().read(sbuffer, 0, size);
			for (int i=0; i < r; i++) buffer[i] = (byte) sbuffer[i];			
			return (r==-1)?0:r;
		}

		public override void SetEndOfSendNotification(System.Web.HttpWorkerRequest.EndOfSendNotification callback, object extraData)
		{
			_endOfSendCallback = callback;
			_endOfSendArgs = extraData;
		}
		
		// Prints some stats about the current _Page.
		private void LogCurrentPageLocation()
		{
#if DEBUG
			Console.WriteLine(" relpath=" + _AppVirtualPath);
			Console.WriteLine(" physical path=" + _AppPhysicalPath);
			Console.WriteLine(" page=" + _Page);
#endif
		}
		
	}
}

