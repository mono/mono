//
// System.Web.HttpRequest
//
// Authors:
//   	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2001, 2002 Patrick Torstensson
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web {
	[MonoTODO("Review security in all path access function")]
	public sealed class HttpRequest {
		private string []	_arrAcceptTypes;
		private string []	_arrUserLanguages;

		private byte [] _arrRawContent;
		private int	_iContentLength;

		private string	_sContentType;
		private string	_sHttpMethod;
		private string	_sRawUrl;
		private string	_sUserAgent;
		private string	_sUserHostAddress;
		private string	_sUserHostName;
		private string	_sPath;
		private string	_sPathInfo;
		private string _sFilePath;
		private string baseVirtualDir;
		private string _sPathTranslated;
		private string _sQueryStringRaw;
		private string _sRequestType;
		private string _sRequestRootVirtualDir;

		private Encoding _oContentEncoding;

		private Uri _oUriReferrer;
		private Uri _oUrl;

		private int	_iTotalBytes;

		private HttpContext	_oContext;

		private HttpWorkerRequest _WorkerRequest;
		private HttpRequestStream	_oInputStream;
		private HttpClientCertificate _ClientCert;

		private HttpValueCollection _oServerVariables;
		private HttpValueCollection _oHeaders;
		private HttpValueCollection _oQueryString;
		private HttpValueCollection _oFormData;
		private HttpValueCollection _oParams;

		private HttpBrowserCapabilities _browser;

		private HttpCookieCollection cookies;
		Stream userFilter;
		HttpRequestStream requestFilter;
		string clientTarget;
		string currentExePath;
#if NET_1_1
		bool validateCookies;
		bool validateForm;
		bool validateQueryString;
#endif

		public HttpRequest(string Filename, string Url, string Querystring) {
			_iContentLength = -1;
			_iTotalBytes = -1;

			_WorkerRequest = null;
			_sPathTranslated = Filename;
			_sRequestType = "GET";

			_oUrl = new Uri(Url);
			_sPath = _oUrl.AbsolutePath;

			_sQueryStringRaw = Querystring;
			_oQueryString = new HttpValueCollection(Querystring, true, Encoding.ASCII);
		}

		internal HttpRequest(HttpWorkerRequest WorkRequest, HttpContext Context) {
			_WorkerRequest = WorkRequest;
			_oContext = Context;

			_iContentLength = -1;
			_iTotalBytes = -1;
		}

		static private string MakeServerVariableFromHeader(string header) {
			return "HTTP_" + header.ToUpper().Replace("-", "_");
		}

		internal string GetAllHeaders(bool Raw) {
			StringBuilder oData;

			if (null == _WorkerRequest) {
				return null;
			}

			oData = new StringBuilder(512);

			string sHeaderValue;
			string sHeaderName;
			int iCount = 0;

			// Add all know headers
			for (; iCount != 40; iCount++) {
				sHeaderValue = _WorkerRequest.GetKnownRequestHeader(iCount);
				if (null != sHeaderValue && sHeaderValue.Length > 0) {
					sHeaderName = _WorkerRequest.GetKnownRequestHeader(iCount);
					if (null != sHeaderName && sHeaderName.Length > 0) {
						oData.Append(sHeaderName);
						oData.Append(": ");
						oData.Append(sHeaderValue);
						oData.Append("\r\n");
					}
				}
			}

			// Get all other headers
			string [][] arrUnknownHeaders = _WorkerRequest.GetUnknownRequestHeaders();
			if (null != arrUnknownHeaders) {
				for (iCount = 0; iCount != arrUnknownHeaders.Length; iCount++) {
					oData.Append(arrUnknownHeaders[iCount][0]);
					oData.Append(": ");
					oData.Append(arrUnknownHeaders[iCount][1]);
					oData.Append("\r\n");
				}
			}

			return oData.ToString();
		}
      
		[MonoTODO("We need to handly 'dynamic' variables like AUTH_USER, that can be changed during runtime... special collection")]
		private void ParseServerVariables() {
			if (null == _WorkerRequest) {
				return;
			}

			if (_oServerVariables == null){ 
				_oServerVariables = new ServerVariablesCollection (this);            
 				_oServerVariables.MakeReadOnly ();
			}
		}

		private void ParseFormData ()
		{
			string contentType = ContentType;
			if (0 == String.Compare (contentType, "application/x-www-form-urlencoded", true)) {
				byte [] arrData = GetRawContent ();
				Encoding enc = ContentEncoding;
				string data = enc.GetString (arrData);
				_oFormData = new HttpValueCollection (data, true, enc);
				return;
			}
			if (!ContentType.StartsWith ("multipart/form-data")) {
				if (contentType.Length > 0)
					Console.WriteLine ("Content-Type -> {0} not supported", contentType);
				_oFormData = new HttpValueCollection ();
				return;
			}
			
			MultipartContentElement [] parts = GetMultipartFormData ();
			_oFormData = new HttpValueCollection ();
			if (parts == null) return;
				
			foreach (MultipartContentElement p in parts) {
				if (!p.IsFormItem) continue;
				_oFormData.Add (p.Name, p.GetString (ContentEncoding));
			}

		}

		[MonoTODO("void Dispose")]
		internal void Dispose() {			
		}

		private byte [] GetRawContent ()
		{
			if (_arrRawContent != null)
				return _arrRawContent;

			if (null == _WorkerRequest) {
				if (QueryStringRaw == null)
					return null;
				char [] q = QueryStringRaw.ToCharArray ();
				_arrRawContent = new byte [q.Length];
				for (int i = 0; i < q.Length; i++)
					_arrRawContent [i] = (byte) q [i];
				return _arrRawContent;
			}

			_arrRawContent = _WorkerRequest.GetPreloadedEntityBody ();
			if (_arrRawContent == null)
				_arrRawContent = new byte [0];

			int length = ContentLength;
			HttpRuntimeConfig cfg = (HttpRuntimeConfig) _oContext.GetConfig ("system.web/httpRuntime");
			int maxRequestLength = cfg.MaxRequestLength * 1024;
			if (ContentLength > maxRequestLength)
				throw new HttpException (400, "Maximum request length exceeded.");
				
			if (_WorkerRequest.IsEntireEntityBodyIsPreloaded () || length <= _arrRawContent.Length)
				return _arrRawContent;

			byte [] arrBuffer = new byte [Math.Min (16384, length)];
			MemoryStream ms = new MemoryStream (arrBuffer.Length);
			ms.Write (_arrRawContent, 0, _arrRawContent.Length);
			int read = 0;
			int bufLength = arrBuffer.Length;
			for (int loaded = _arrRawContent.Length; loaded < length; loaded += read) {
				if (length - loaded < bufLength)
					bufLength = length - loaded;

				read = _WorkerRequest.ReadEntityBody (arrBuffer, bufLength);
				if (read == 0 ||read == -1 )
					break;

				if (ContentLength > maxRequestLength || ms.Length + read > maxRequestLength)
					throw new HttpException (400, "Maximum request length exceeded.");

				ms.Write (arrBuffer, 0, read);
			}

			byte [] msBuffer = ms.GetBuffer ();
			if (msBuffer.Length == length)
				_arrRawContent = msBuffer;
			else
				_arrRawContent = ms.ToArray ();

			if (userFilter != null) {
				requestFilter.Set (_arrRawContent, 0, _arrRawContent.Length);
				int userLength = Convert.ToInt32 (userFilter.Length - userFilter.Position);
				byte [] filtered = new byte [userLength];
				userFilter.Read (filtered, 0, userLength);
				_arrRawContent = filtered;
			}

			return _arrRawContent;
		}
		
		internal HttpContext Context {
			get { return _oContext; }
		}

		internal HttpWorkerRequest WorkerRequest {
			get { return _WorkerRequest; }
		}

		public string [] AcceptTypes {
			get {
				if (null == _arrAcceptTypes && null != _WorkerRequest) {
					_arrAcceptTypes = HttpHelper.ParseMultiValueHeader(_WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderAccept));
				} 

				return _arrAcceptTypes;
				
			}
		}

		public string ApplicationPath {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.GetAppPath();
				}

				return null;
			}
		}

		public HttpBrowserCapabilities Browser {
			get {
				if (_browser == null) {
					_browser = (HttpBrowserCapabilities)
						    HttpCapabilitiesBase.GetConfigCapabilities (null, this);
				}

				return _browser;
			}

			set { _browser = value; }
		}

		public HttpClientCertificate ClientCertificate {
			get {
				if (null == _ClientCert) {
					_ClientCert = new HttpClientCertificate(_oContext);
				}

				return _ClientCert;
			}
		}

		private string GetValueFromHeader (string header, string attr)
		{
			int where = header.IndexOf (attr + '=');
			if (where == -1)
				return null;

			where += attr.Length + 1;
			int max = header.Length;
			if (where >= max)
				return String.Empty;

			char ending = header [where];
			if (ending != '"')
				ending = ' ';

			int end = header.Substring (where + 1).IndexOf (ending);
			if (end == -1)
				return (ending == '"') ? null : header.Substring (where);

			return header.Substring (where, end);
		}
		
		public Encoding ContentEncoding
		{
			get {
				if (_oContentEncoding == null) {
					if (_WorkerRequest != null && 
					    (!_WorkerRequest.HasEntityBody () || ContentType != String.Empty)) {
						_oContentEncoding = WebEncoding.RequestEncoding;
					} else  {
						string charset;
						charset = GetValueFromHeader (_sContentType, "charset");
						try {
							_oContentEncoding = Encoding.GetEncoding (charset);
						} catch {
							_oContentEncoding = WebEncoding.RequestEncoding;
						}
					}
				}

				return _oContentEncoding;
			}

			set {
				_oContentEncoding = value;
			}
		}

		public int ContentLength {
			get {
				if (_iContentLength == -1 && null != _WorkerRequest) {
					string sLength = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
					if (sLength != null) {
						try {
							_iContentLength = Int32.Parse(sLength);
						}
						catch(Exception) {
						}
					} 
				}

				if (_iContentLength < 0) {
					_iContentLength = 0;
				}

				return _iContentLength;
			}
		}

		public string ContentType {
			get {
				if (null == _sContentType) {
					if (null != _WorkerRequest) {
						_sContentType = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);
					}

					if (null == _sContentType) {
						_sContentType = string.Empty;
					}
				}

				return _sContentType;
			}
#if NET_1_1
			set { _sContentType = value; }
#endif
		}

		static private string GetCookieValue (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';')
				k++;

			i = k;
			return str.Substring (begin, i - begin).Trim ();
		}

		static private string GetCookieName (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';' &&  str [k] != '=')
				k++;

			i = k + 1;
			return str.Substring (begin, k - begin).Trim ();
		}

		private void GetCookies ()
		{
			string header = _WorkerRequest.GetKnownRequestHeader (HttpWorkerRequest.HeaderCookie);
			if (header == null || header.Length == 0)
				return;

			/* RFC 2109
			 *	cookie          =       "Cookie:" cookie-version
			 *				   1*((";" | ",") cookie-value)
			 *	cookie-value    =       NAME "=" VALUE [";" path] [";" domain]
			 *	cookie-version  =       "$Version" "=" value
			 *	NAME            =       attr
			 *	VALUE           =       value
			 *	path            =       "$Path" "=" value
			 *	domain          =       "$Domain" "=" value
			 *
			 *	MS ignores $Version! 
			 *	',' as a separator produces errors.
			 */

			string [] name_values = header.Trim ().Split (';');
			int length = name_values.Length;
			HttpCookie cookie = null;
			int pos;
			for (int i = 0; i < length; i++) {
				pos = 0;
				string name_value = name_values [i].Trim ();
				string name = GetCookieName (name_value, name_value.Length, ref pos);
				string value = GetCookieValue (name_value, name_value.Length, ref pos);
				if (cookie != null) {
					if (name == "$Path") {
						cookie.Path = value;
						continue;
					} else if (name == "$Domain") {
						cookie.Domain = value;
						continue;
					} else {
						cookies.Add (cookie);
						cookie = null;
					}
				}
				cookie = new HttpCookie (name, value);
			}

			if (cookie != null)
				cookies.Add (cookie);
		}

		public HttpCookieCollection Cookies
		{
			get {
				if (cookies == null) {
					cookies = new HttpCookieCollection (null, false);
					if (_WorkerRequest != null)
						GetCookies ();
#if NET_1_1
					if (validateCookies)
						ValidateCookieCollection (cookies);
#endif
				}

				return cookies;
			}
		}

		public string CurrentExecutionFilePath {
			get {
				if (currentExePath != null)
					return currentExePath;

				return FilePath;
			}
		}

		public string FilePath {
			get {
				if (null == _sFilePath && null != _WorkerRequest) {
					_sFilePath = _WorkerRequest.GetFilePath();
					try {
						_sFilePath = UrlUtils.Reduce (_sFilePath);
					} catch (Exception) {
						throw new HttpException (403, "Forbidden");
					}
				}

				return _sFilePath;
			}
		}

		HttpFileCollection files;
		public HttpFileCollection Files {
			get {
				if (files != null)
					return files;
				
				files = new HttpFileCollection ();
				FillPostedFiles ();
				return files;
				
			}
		}
		
		void FillPostedFiles ()
		{
			if (!ContentType.StartsWith ("multipart/form-data")) return;
			
			MultipartContentElement [] parts = GetMultipartFormData ();
			if (parts == null) return;
				
			foreach (MultipartContentElement p in parts) {
				if (!p.IsFile) continue;
				files.AddFile (p.Name, p.GetFile ());
			}
		}
		
		MultipartContentElement [] multipartContent;
		MultipartContentElement [] GetMultipartFormData ()
		{
			if (multipartContent != null) return multipartContent;
			
			byte [] raw = GetRawContent ();
			byte [] boundary = Encoding.ASCII.GetBytes (("--" + GetValueFromHeader (ContentType, "boundary")));
			return multipartContent = HttpMultipartContentParser.Parse (raw, boundary, ContentEncoding);
		}

		public Stream Filter {
			get {
				if (userFilter != null)
					return userFilter;

				if (requestFilter == null)
					requestFilter = new HttpRequestStream ();

				// This is an empty stream. It will not contain data until GetRawContent
				return requestFilter;
			}

			set {
				if (requestFilter == null)
					throw new HttpException ("Invalid request filter.");

				userFilter = value;
			}
		}

		public NameValueCollection Form {
			get {
				if (_oFormData == null) {
					ParseFormData ();
#if NET_1_1
					if (validateForm)
						ValidateNameValueCollection ("Form", _oFormData);
#endif
				}

				return _oFormData;
			}
		}

		public NameValueCollection Headers {
			get {
				if (_oHeaders == null) {
					_oHeaders = new HttpValueCollection();

					if (null != _WorkerRequest) {
						string sHeaderValue;
						string sHeaderName;
						int iCount = 0;

						// Add all know headers
						for (; iCount != 40; iCount++) {
							sHeaderValue = _WorkerRequest.GetKnownRequestHeader(iCount);
							if (null != sHeaderValue && sHeaderValue.Length > 0) {
								sHeaderName = HttpWorkerRequest.GetKnownRequestHeaderName(iCount);
								if (null != sHeaderName && sHeaderName.Length > 0) {
									_oHeaders.Add(sHeaderName, sHeaderValue);
								}
							}
						}

						// Get all other headers
						string [][] arrUnknownHeaders = _WorkerRequest.GetUnknownRequestHeaders();
						if (null != arrUnknownHeaders) {
							for (iCount = 0; iCount != arrUnknownHeaders.Length; iCount++) {
								_oHeaders.Add(arrUnknownHeaders[iCount][0], arrUnknownHeaders[iCount][1]);
							}
						}
					}

					// Make headers read-only
					_oHeaders.MakeReadOnly();
				}

				return (NameValueCollection) _oHeaders;
			}
		}

		public string HttpMethod {
			get {
				if (null == _sHttpMethod) {
					if (null != _WorkerRequest) {
						_sHttpMethod = _WorkerRequest.GetHttpVerbName().ToUpper();
					}
               
					if (_sHttpMethod == null) {
						if (RequestType != null)
							_sHttpMethod = RequestType;
						else
							_sHttpMethod = "GET";
					}
				}

				return _sHttpMethod;
			}
		}

		public Stream InputStream {
			get {
				if (_oInputStream == null) {
					byte [] arrInputData = GetRawContent ();

					if (null != arrInputData) {
						_oInputStream = new HttpRequestStream(arrInputData, 0, arrInputData.Length);
					} else {
						_oInputStream = new HttpRequestStream(null, 0, 0);
					}
				}

				return _oInputStream;
			} 
		}

		public bool IsAuthenticated {
			get {
				if (_oContext != null && _oContext.User != null && _oContext.User.Identity != null) {
					return _oContext.User.Identity.IsAuthenticated;
				}

				return false;
			}
		}

		public bool IsSecureConnection {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.IsSecure();
				}

				return false;
			}
		}

		public string this [string sKey] {
			get {
				string result = QueryString [sKey];
				if (result != null)
					return result;

				result = Form [sKey];
				if (result != null)
					return result;

				HttpCookie cookie = Cookies [sKey];
				if (cookie != null)
					return cookie.Value;

				return ServerVariables [sKey];
			}
		}

		public NameValueCollection Params {
			get {
				if (_oParams == null) {
					_oParams = new HttpValueCollection();
					
					_oParams.Merge(QueryString);
					_oParams.Merge(Form);
					_oParams.Merge(ServerVariables);
					string [] cookies = Cookies.AllKeys;
					foreach (string k in cookies)
						_oParams.Add (k, Cookies [k].ToString ());
					_oParams.MakeReadOnly();
				}

				return (NameValueCollection) _oParams;
			}
		}
		
		public string Path {
			get {
				if (_sPath == null) {
					if (null != _WorkerRequest) {
						_sPath = _WorkerRequest.GetUriPath();
					}

					if (_sPath == null) {
						_sPath = string.Empty;
					}
				}

				return _sPath;
			}
		}
		
		public string PathInfo {
			get {
				if (_sPathInfo == null) {
					if (null != _WorkerRequest) {
						_sPathInfo = _WorkerRequest.GetPathInfo();
					}

					if (_sPathInfo == null) {
						_sPathInfo = string.Empty;
					}
				}
				
				return _sPathInfo;
			}
		}

		public string PhysicalApplicationPath {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.GetAppPathTranslated();
				}

				return null;
			}
		}

		public string PhysicalPath {
			get {
				if (_sPathTranslated == null && _WorkerRequest != null)
					_sPathTranslated = _WorkerRequest.MapPath (CurrentExecutionFilePath);

				return _sPathTranslated;
			}
		}

		public NameValueCollection QueryString {
			get {
				if (_oQueryString == null) {
					try {
						_oQueryString = new HttpValueCollection(QueryStringRaw, true,
											ContentEncoding);
					} catch {
						_oQueryString = new HttpValueCollection(QueryStringRaw, true,
											Encoding.ASCII);
					}
#if NET_1_1
					if (validateQueryString)
						ValidateNameValueCollection ("QueryString", _oQueryString);
#endif
				}

				return _oQueryString;
			}
		}

		// Used to parse the querystring
		internal string QueryStringRaw {
			get {
				if (_sQueryStringRaw == null && null != _WorkerRequest) {
					byte [] arrQuerystringBytes = _WorkerRequest.GetQueryStringRawBytes();
					if (null != arrQuerystringBytes && arrQuerystringBytes.Length > 0) {
						_sQueryStringRaw = ContentEncoding.GetString(arrQuerystringBytes);
					} else {
						_sQueryStringRaw = _WorkerRequest.GetQueryString();   
					}
				}

				if (_sQueryStringRaw == null) {
					_sQueryStringRaw = string.Empty;
				}

				return _sQueryStringRaw;
			}
			
			set {
				_sQueryStringRaw = value;
				_oQueryString = null;
				_arrRawContent = null;
				_sRawUrl = null;
			}
		}

		public string RawUrl {
			get {
				if (null == _sRawUrl) {
					if (null != _WorkerRequest) {
						_sRawUrl = _WorkerRequest.GetRawUrl();
					} else {
						_sRawUrl = Path;
						if (QueryStringRaw != null && QueryStringRaw.Length > 0) {
							_sRawUrl = _sRawUrl + "?" + QueryStringRaw;
						}
					}
				}

				return _sRawUrl;
			}
		}

		public string RequestType {
			get {
				if (null == _sRequestType) {
					return HttpMethod;
				}
         
				return _sRequestType;
			}

			set {
				_sRequestType = value;
			}
		}
		
      
		public NameValueCollection ServerVariables {
			get {
				ParseServerVariables();

				return (NameValueCollection) _oServerVariables;
			}
		}      

		public int TotalBytes {
			get {
				if (_iTotalBytes == -1) {
					if (null != InputStream) {
						_iTotalBytes = (int) InputStream.Length;
					} else {
						_iTotalBytes = 0;
					}
				}

				return _iTotalBytes;
			}
		}

		public Uri Url {
			get {
				if (_oUrl != null || _WorkerRequest == null)
					return _oUrl;

				string qs = QueryStringRaw;
				if (qs == null)
					qs = "";
				else
					qs = "?" + qs;

				UriBuilder ub = new UriBuilder (_WorkerRequest.GetProtocol (),
								_WorkerRequest.GetServerName (),
								_WorkerRequest.GetLocalPort (),
								Path,
								qs);

				_oUrl = ub.Uri;
				return _oUrl;
			}
		}

		public Uri UrlReferrer {
			get {
				if (null == _oUriReferrer && null != _WorkerRequest) {
					string sReferrer = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderReferer);
					if (null != sReferrer && sReferrer.Length > 0) {
						try {
							if (sReferrer.IndexOf("://") >= 0) {
								_oUriReferrer = new Uri(sReferrer);
							} else {
								_oUriReferrer = new Uri(this.Url, sReferrer);
							}
						}
						catch (Exception) {
						}
					}
				}

				return _oUriReferrer;
			}
		}

		public string UserAgent {
			get {
				if (_sUserAgent == null && _WorkerRequest != null) {
					_sUserAgent = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderUserAgent);
				}

				if (_sUserAgent == null) {
					_sUserAgent = string.Empty;
				}

				return _sUserAgent;
			}
		}

		public string UserHostAddress {
			get {
				if (_sUserHostAddress == null && null != _WorkerRequest) {
					_sUserHostAddress = _WorkerRequest.GetRemoteAddress();
				}

				if (_sUserHostAddress == null || _sUserHostAddress.Length == 0) {
					_sUserHostAddress = "127.0.0.1";
				}

				return _sUserHostAddress;
			}
		}
		
		public string UserHostName {
			get {
				if (_sUserHostName == null && null != _WorkerRequest) {
					_sUserHostName = _WorkerRequest.GetRemoteName();
				}

				if (_sUserHostName == null || _sUserHostName.Length == 0) {
					_sUserHostName = UserHostAddress;
				}

				return _sUserHostName;
			}
		}
		
		public string [] UserLanguages {
			get {
				if (_arrUserLanguages == null && null != _WorkerRequest) {
					_arrUserLanguages = HttpHelper.ParseMultiValueHeader(_WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderAcceptLanguage));
				}

				return _arrUserLanguages;
			}
		}

		internal string RootVirtualDir {
			get {
				if (_sRequestRootVirtualDir == null) {
					_sRequestRootVirtualDir = FilePath;
					int pos = _sRequestRootVirtualDir.LastIndexOf ('/');
					if (pos == -1 || pos == 0)
						_sRequestRootVirtualDir = "/";
					else
						_sRequestRootVirtualDir = _sRequestRootVirtualDir.Substring (0, pos);
				}

				return _sRequestRootVirtualDir;
			}
		}
		
		internal string BaseVirtualDir {
			get {
				if (baseVirtualDir == null)
					baseVirtualDir = UrlUtils.GetDirectory (FilePath);

				return baseVirtualDir;
			}
		}

                internal bool IsLocal {
                        get {
                                return _WorkerRequest.GetLocalAddress () == "127.0.0.1";
                        }
                }
                
		public byte [] BinaryRead(int count) {
			int iSize = TotalBytes;
			if (iSize == 0) {
				throw new ArgumentException();
			}

			byte [] arrData = new byte[iSize];
			
			int iRetSize = InputStream.Read(arrData, 0, iSize);
			if (iRetSize != iSize) {
				byte [] tmpData = new byte[iRetSize];
				if (iRetSize > 0) {
					Array.Copy(arrData, 0, tmpData, 0, iRetSize);
				}

				arrData = tmpData;
			}

			return arrData;
		}

		public int [] MapImageCoordinates(string ImageFieldName) {
			NameValueCollection oItems;

			if (HttpMethod == "GET" || HttpMethod == "HEAD") {
				oItems = QueryString;
			} else if (HttpMethod == "POST") {
				oItems = Form;
			} else {
				return null;
			}

			int [] arrRet = null;
			try {
				string sX = oItems.Get(ImageFieldName + ".x");
				string sY = oItems.Get(ImageFieldName + ".y");

				if (null != sX && null != sY) {
					int [] arrTmp = new Int32[2];
					arrRet[0] = Int32.Parse(sX);
					arrRet[1] = Int32.Parse(sY);

					arrRet = arrTmp;
				}
			}
			catch (Exception) {
			}

			return arrRet;
		}

		public string MapPath (string VirtualPath)
		{
			return MapPath (VirtualPath, BaseVirtualDir, true);
		}

		public string MapPath (string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			if (_WorkerRequest == null)
				throw new HttpException ("No HttpWorkerRequest!!!");

			if (virtualPath == null || virtualPath.Length == 0)
				virtualPath = ".";
			else
				virtualPath = virtualPath.Trim ();

			if (virtualPath.IndexOf (':') != -1)
				throw new ArgumentException ("Invalid path -> " + virtualPath);

			if (System.IO.Path.DirectorySeparatorChar != '/')
				virtualPath = virtualPath.Replace (System.IO.Path.DirectorySeparatorChar, '/');

			if (UrlUtils.IsRooted (virtualPath)) {
				virtualPath = UrlUtils.Reduce (virtualPath);
			} else {
				if (baseVirtualDir == null) {
					virtualPath = UrlUtils.Combine (RootVirtualDir, virtualPath);
				} else {
					virtualPath = UrlUtils.Combine (baseVirtualDir, virtualPath);
				}
			}

			if (!allowCrossAppMapping) {
				if (!virtualPath.ToLower ().StartsWith (RootVirtualDir.ToLower ()))
					throw new HttpException ("Mapping across applications not allowed.");

				if (RootVirtualDir.Length > 1 && virtualPath.Length > 1 && virtualPath [0] != '/')
					throw new HttpException ("Mapping across applications not allowed.");
			}
			
			return _WorkerRequest.MapPath (virtualPath);
		}

		public void SaveAs(string filename, bool includeHeaders) {
			FileStream oFile;
			TextWriter oWriter;
			HttpRequestStream oData;

			oFile = new FileStream(filename, FileMode.CreateNew);
			if (includeHeaders) {
				oWriter = new StreamWriter(oFile);
				oWriter.Write(HttpMethod + " " + Path);

				if (QueryStringRaw != null && QueryStringRaw.Length > 0)
					oWriter.Write("?" + QueryStringRaw);
				if (_WorkerRequest != null) {
					oWriter.Write(" " + _WorkerRequest.GetHttpVersion() + "\r\n");
					oWriter.Write(GetAllHeaders(true));
				} else {
					oWriter.Write("\r\n");
				}

				oWriter.Write("\r\n");
				oWriter.Flush();
			}

			oData = (HttpRequestStream) InputStream;

			if (oData.DataLength > 0) {
				oFile.Write(oData.Data, oData.DataOffset, oData.DataLength);
			}

			oFile.Flush();
			oFile.Close();
		}

#if NET_1_1
		public void ValidateInput ()
		{
			validateCookies = true;
			validateQueryString = true;
			validateForm = true;
		}
#endif
		
		internal void SetCurrentExePath (string filePath)
		{
			currentExePath = filePath;
			_sRequestRootVirtualDir = null;
			baseVirtualDir = null;
			_sPathTranslated = null;
		}

		internal void SetPathInfo (string pathInfo)
		{
			_sPathInfo = pathInfo;
		}

		internal void SetForm (HttpValueCollection form)
		{
			_oFormData = form;
		}

		internal void SetHeader (string name, string value)
		{
			HttpValueCollection headers = (HttpValueCollection) Headers;
			headers.MakeReadWrite ();
			headers [name] = value;
			headers.MakeReadOnly ();
		}

		internal string ClientTarget {
			get { return clientTarget; }
			set {
				if (value != clientTarget) {
					clientTarget = value;
					_browser = null;
				}
			}
		}

#if NET_1_1
		static void ValidateNameValueCollection (string name, NameValueCollection coll)
		{
			if (coll == null)
				return;

			foreach (string key in coll.Keys) {
				string val = coll [key];
				if (CheckString (val))
					ThrowValidationException (name, key, val);
			}
		}

		static void ValidateCookieCollection (HttpCookieCollection cookies)
		{
			if (cookies == null)
				return;

			int size = cookies.Count;
			HttpCookie cookie;
			for (int i = 0 ; i < size ; i++) {
				cookie = cookies[i];
				if (CheckString (cookie.Value))
					ThrowValidationException ("Cookies", cookie.Name, cookie.Value);
			}
		}

		static void ThrowValidationException (string name, string key, string value)
		{
			string v = "\"" + value + "\"";
			if (v.Length > 20)
				v = v.Substring (16) + "...\"";

			string msg = String.Format ("A potentially dangerous Request.{0} value was " +
						    "detected from the client ({1}={2}).", name, key, v);

			throw new HttpRequestValidationException (msg);
		}

		static char [] dangerousChars = "<>".ToCharArray ();
		static bool CheckString (string val)
		{
			if (val == null)
				return false;

			//TODO: More checks
			if (val.IndexOfAny (dangerousChars) != -1 && val.Length > 1) {
				foreach (char c in val)
					if (Array.IndexOf (dangerousChars, c) != -1)
						return true;
			}

			return false;
		}
#endif
	}
}

