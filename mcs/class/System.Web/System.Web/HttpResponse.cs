// 
// System.Web.HttpResponse
//
// Authors:
//	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Util;
using System.Web.Caching;

namespace System.Web
{
	public sealed class HttpResponse
	{
		// Chunked encoding static helpers
		static byte [] s_arrChunkSuffix = {13, 10};
		static byte [] s_arrChunkEnd = {48, 13, 10, 13, 10};
		static string s_sChunkedPrefix = "\r\n";

		ArrayList _Headers;
			
		bool _bClientDisconnected;
		bool _bSuppressHeaders;
		bool _bSuppressContent;
		bool _bChunked;
		bool _bEnded;
		bool _bBuffering;
		bool _bHeadersSent;
		bool _bFlushing;
		bool filtered;
		long _lContentLength;
		int _iStatusCode;
		
		int _expiresInMinutes;
		bool _expiresInMinutesSet;
		DateTime _expiresAbsolute;
		bool _expiresAbsoluteSet;

		bool _ClientDisconnected;
		bool closed;

		string	_sContentType;
		string	_sCacheControl;
		string	_sTransferEncoding;
		string	_sCharset;
		string	_sStatusDescription;

		HttpCookieCollection _Cookies;
		HttpCachePolicy _CachePolicy;

		Encoding _ContentEncoding;
			
		HttpContext _Context;
		HttpWriter _Writer;
		TextWriter _TextWriter;

		HttpWorkerRequest _WorkerRequest;

		ArrayList fileDependencies;
		CachedRawResponse cached_response;
		ArrayList cached_headers;
#if NET_1_1
		string redirectLocation;
#endif

		string app_path_mod = null;
                
		public HttpResponse (TextWriter output)
		{
			 _bBuffering = true;
			 _bFlushing = false;
			 _bHeadersSent = false;

			 _Headers = new ArrayList ();

			 _sContentType = "text/html";

			 _iStatusCode = 200;
			 _sCharset = null;
			 _sCacheControl = null;

			 _lContentLength = 0;
			 _bSuppressContent = false;
			 _bSuppressHeaders = false;
			 _bClientDisconnected = false;

			 _bChunked = false;

			 _TextWriter = output;
		}

		internal HttpResponse (HttpWorkerRequest WorkerRequest, HttpContext Context)
		{
			 _Context = Context;
			 _WorkerRequest = WorkerRequest;

			 _bBuffering = true;
			 _bFlushing = false;
			 _bHeadersSent = false;

			 _Headers = new ArrayList ();

			 _sContentType = "text/html";

			 _iStatusCode = 200;
			 _sCharset = null;
			 _sCacheControl = null;

			 _lContentLength = 0;
			 _bSuppressContent = false;
			 _bSuppressHeaders = false;
			 _bClientDisconnected = false;

			 _bChunked = false;
		}

		internal void InitializeWriter ()
		{
			// We cannot do this in the .ctor because HttpWriter uses configuration and
			// it may not be initialized
			if (_Writer == null) {
				 _Writer = new HttpWriter (this);
				 _TextWriter = _Writer;
			}
		}
		
		internal void FinalFlush ()
		{
			Flush (true);
		}

		internal void DoFilter (bool really)
		{
			if (really && null != _Writer) 
				_Writer.FilterData (true);

			filtered = true;
		}

		internal bool IsCached {
			get { return cached_response != null; }
		}

		internal CachedRawResponse GetCachedResponse () {
			cached_response.StatusCode = StatusCode;
			cached_response.StatusDescription = StatusDescription;
			return cached_response;
		}

		internal void SetCachedHeaders (ArrayList headers)
		{
			cached_headers = headers;
		}
		
		private ArrayList GenerateHeaders ()
		{
			ArrayList oHeaders = new ArrayList (_Headers.ToArray ());

			oHeaders.Add (new HttpResponseHeader ("X-Powered-By", "Mono"));
			// save culture info, we need us info here
			CultureInfo oSavedInfo = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			string date = DateTime.UtcNow.ToString ("ddd, d MMM yyyy HH:mm:ss ");
			HttpResponseHeader date_header = new HttpResponseHeader ("Date", date + "GMT");
			oHeaders.Add (date_header);
			
			if (IsCached)
				cached_response.DateHeader = date_header;

			Thread.CurrentThread.CurrentCulture = oSavedInfo;

			if (_lContentLength > 0) {
				oHeaders.Add (new HttpResponseHeader (HttpWorkerRequest.HeaderContentLength,
								      _lContentLength.ToString ()));
			}

			if (_sContentType != null) {
				if (_sContentType.IndexOf ("charset=") == -1) {
					if (Charset.Length == 0) {
						Charset = ContentEncoding.HeaderName;
					}

					// Time to build our string
					if (Charset.Length > 0) {
						_sContentType += "; charset=" + Charset;
					}
				}

				oHeaders.Add (new HttpResponseHeader (HttpWorkerRequest.HeaderContentType,
								      _sContentType));
			}

			if (_CachePolicy != null)
				_CachePolicy.SetHeaders (this, oHeaders);
			
			if (_sCacheControl != null) {
				oHeaders.Add (new HttpResponseHeader (HttpWorkerRequest.HeaderPragma,
								      _sCacheControl));
			}

			if (_sTransferEncoding != null) {
				oHeaders.Add (new HttpResponseHeader (HttpWorkerRequest.HeaderTransferEncoding,
								      _sTransferEncoding));
			}

			if (_Cookies != null) {
				int length = _Cookies.Count;
				for (int i = 0; i < length; i++) {
					oHeaders.Add (_Cookies.Get (i).GetCookieHeader ());
				}
			}
#if NET_1_1
			if (redirectLocation != null)
				oHeaders.Add (new HttpResponseHeader (HttpWorkerRequest.HeaderLocation,
								      redirectLocation));
#endif
			return oHeaders;
		}
		
		private void SendHeaders ()
		{
			_WorkerRequest.SendStatus (StatusCode, StatusDescription);
			
			ArrayList oHeaders;

			if (cached_headers != null)
				oHeaders = cached_headers;
			else
				oHeaders = GenerateHeaders ();

			if (cached_response != null)
				cached_response.SetHeaders (oHeaders);
			
			foreach (HttpResponseHeader oHeader in oHeaders)
				oHeader.SendContent (_WorkerRequest);
			
			_bHeadersSent = true;
		}

		public string Status
		{
			get {
				return String.Format ("{0} {1}", StatusCode, StatusDescription);
			}

			set {
				string sMsg = "OK";
				int iCode = 200;

				try {
					iCode = Int32.Parse (value.Substring (0, value.IndexOf (' ')));
					sMsg = value.Substring (value.IndexOf (' ') + 1);
				} catch (Exception) {
					throw new HttpException ("Invalid status string");
				}

				StatusCode = iCode;
				StatusDescription = sMsg;
			}
		}

		[MonoTODO()]
		public void AddCacheItemDependencies (ArrayList cacheKeys)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO()]
		public void AddCacheItemDependency(string cacheKey)
		{
			throw new NotImplementedException ();
		}

		public void AddFileDependencies (ArrayList filenames)
		{
			if (filenames == null || filenames.Count == 0)
				return;
			
			if (fileDependencies == null) {
				fileDependencies = (ArrayList) filenames.Clone ();
				return;
			}

			foreach (string fn in filenames)
				AddFileDependency (fn);
		}

		public void AddFileDependency (string filename)
		{
			if (fileDependencies == null)
				fileDependencies = new ArrayList ();

			fileDependencies.Add (filename);
		}

		public void AddHeader (string name, string value)
		{
			AppendHeader(name, value);
		}

		public void AppendCookie (HttpCookie cookie)
		{
			if (_bHeadersSent)
				throw new HttpException ("Cannot append cookies after HTTP headers have been sent");

			Cookies.Add (cookie);
		}

		[MonoTODO()]
		public void AppendToLog (string param)
		{
			throw new NotImplementedException ();
		}

		public string ApplyAppPathModifier (string virtualPath)
		{
			if (virtualPath == null)
				return null;

			if (virtualPath == "")
				return _Context.Request.RootVirtualDir;

			if (UrlUtils.IsRelativeUrl (virtualPath)) {
				virtualPath = UrlUtils.Combine (_Context.Request.RootVirtualDir, virtualPath);
			} else if (UrlUtils.IsRooted (virtualPath)) {
				virtualPath = UrlUtils.Reduce (virtualPath);
			}

			if (app_path_mod != null && virtualPath.IndexOf (app_path_mod) < 0) {
				string rvd = _Context.Request.RootVirtualDir;
				string basevd = rvd.Replace (app_path_mod, "");

				if (!virtualPath.StartsWith (basevd))
					return virtualPath;

				virtualPath = UrlUtils.Combine (rvd, virtualPath.Substring (basevd.Length));
			}

			return virtualPath;
		}

		internal void SetAppPathModifier (string app_path_mod)
		{
			this.app_path_mod = app_path_mod;
		}
		
		public bool Buffer
		{
			get {
				return BufferOutput;
			}

			set {
				BufferOutput = value;
			}
		}

		public bool BufferOutput
		{
			get {
				return _bBuffering;
			}
			
			set {
				if (_Writer != null)
					_Writer.Update ();

				_bBuffering = value;
			}
		}

		public HttpCachePolicy Cache
		{
			get {
				if (null == _CachePolicy) {
					_CachePolicy = new HttpCachePolicy ();
					_CachePolicy.CacheabilityUpdated += new CacheabilityUpdatedCallback (
						OnCacheabilityUpdated);
				}

				return _CachePolicy;
			}
		}

		private void OnCacheabilityUpdated (object sender, CacheabilityUpdatedEventArgs e)
		{
			if (e.Cacheability >= HttpCacheability.Server && !IsCached)
				cached_response = new CachedRawResponse (_CachePolicy);
			else if (e.Cacheability <= HttpCacheability.Private)
				cached_response = null;
		}
		
		[MonoTODO("Set status in the cache policy")]
		public string CacheControl
		{
			get {
				return _sCacheControl;
			}

			set {
				if (_bHeadersSent)
					throw new HttpException ("Headers has been sent to the client");

				_sCacheControl = value;
			}
		}

		public string Charset
		{
			get {
				if (null == _sCharset)
					_sCharset = ContentEncoding.WebName;

				return _sCharset;
			}

			set {
				if (_bHeadersSent)
					throw new HttpException ("Headers has been sent to the client");

				_sCharset = value;
			}
		}

		public Encoding ContentEncoding
		{
			get {
				if (_ContentEncoding == null)
					_ContentEncoding = WebEncoding.ResponseEncoding;

				return _ContentEncoding;
			}

			set {
				if (value == null)
					throw new ArgumentNullException ("Can't set a null as encoding");

				_ContentEncoding = value;

				if (_Writer != null)
					_Writer.Update ();
			}
		}

		public string ContentType
		{
			get {
				return _sContentType;
			}

			set {
				if (_bHeadersSent)
					throw new HttpException ("Headers has been sent to the client");

				_sContentType = value;
			}
		}

		public HttpCookieCollection Cookies
		{
			get {
				if (null == _Cookies)
					_Cookies = new HttpCookieCollection (this, false);

				return _Cookies;
			}
		}

		public int Expires
		{
			get {
				return _expiresInMinutes;
			}

			set {
				if (!_expiresInMinutesSet || (value < _expiresInMinutes))
				{
					_expiresInMinutes = value;
					Cache.SetExpires(_Context.Timestamp.Add(new TimeSpan(0, _expiresInMinutes, 0)));
 				}
				_expiresInMinutesSet = true;
			}
		}

		public DateTime ExpiresAbsolute
		{
			get {
				return _expiresAbsolute;
			}

			set {
				if (!_expiresAbsoluteSet || value.CompareTo(_expiresAbsolute)<0)
				{
					_expiresAbsolute = value;
					Cache.SetExpires(_expiresAbsolute); 
				}
				_expiresAbsoluteSet = true;
			}
		}

		public Stream Filter
		{
			get {
				if (_Writer != null)
					return _Writer.GetActiveFilter ();

				return null;
			}

			set {
				if (_Writer == null)
					throw new HttpException ("Filtering is not allowed");

				_Writer.ActivateFilter (value);
			}
		}

		public bool IsClientConnected
		{
			get {
				if (_ClientDisconnected)
					return false;

				if (null != _WorkerRequest && (!_WorkerRequest.IsClientConnected ())) {
					_ClientDisconnected = false;
					return false;
				}

				return true;
			}
		}
      
		public TextWriter Output
		{
			get {
				return _TextWriter;
			}
		}

		public Stream OutputStream
		{
			get {
				if (_Writer == null)
					throw new HttpException ("an Output stream not available when " +
								 "running with custom text writer");

				return _Writer.OutputStream;
			}
		}

#if NET_1_1
		public string RedirectLocation {
			get { return redirectLocation; }
			set { redirectLocation = value; }
		}
#endif
		
		public string StatusDescription
		{
			get {
				if (null == _sStatusDescription)
					_sStatusDescription =
						HttpWorkerRequest.GetStatusDescription (_iStatusCode);

				return _sStatusDescription;
			}

			set {
				if (_bHeadersSent)
					throw new HttpException ("Headers has been sent to the client");

				_sStatusDescription = value;
			}
		}
	
		public int StatusCode
		{
			get {
				return _iStatusCode;
			}

			set {
				if (_bHeadersSent)
					throw new HttpException ("Headers has been sent to the client");

				_sStatusDescription = null;
				_iStatusCode = value;
			}
		}

		public bool SuppressContent
		{
			get {
				return _bSuppressContent;
			}
			
			set {
				_bSuppressContent = true;
			}
		}

		HttpRequest Request
		{
			get {
				if (_Context == null)
					return null;

				return _Context.Request;
			}
		}

		internal void AppendHeader (int iIndex, string value)
		{
			if (_bHeadersSent)
				throw new HttpException ("Headers has been sent to the client");

			switch (iIndex) {
			case HttpWorkerRequest.HeaderContentLength:
				_lContentLength = Int64.Parse (value);
				break;
			case HttpWorkerRequest.HeaderContentEncoding:
				_sContentType = value;
				break;
			case HttpWorkerRequest.HeaderTransferEncoding:
				_sTransferEncoding = value;
				_bChunked = (value == "chunked");
				break;
			case HttpWorkerRequest.HeaderPragma:
				_sCacheControl = value;
				break;
			default:
				_Headers.Add (new HttpResponseHeader (iIndex, value));
				break;
			}
		}

		public void AppendHeader (string name, string value)
		{
			if (_bHeadersSent)
				throw new HttpException ("Headers has been sent to the client");

			switch (name.ToLower ()) {
			case "content-length":
				_lContentLength = Int64.Parse (value);
				break;
			case "content-type":
				_sContentType = value;
				break;
			case "transfer-encoding":
				_sTransferEncoding = value;
				_bChunked = (value == "chunked");
				break;
			case "pragma":
				_sCacheControl = value;
				break;
			default:
				_Headers.Add (new HttpResponseHeader (name, value));
				break;
			}
		}
	
		internal TextWriter SetTextWriter (TextWriter w)
		{
			TextWriter prev = _TextWriter;
			_TextWriter = w;
			return prev;
		}
		
		public void BinaryWrite (byte [] buffer)
		{
			OutputStream.Write (buffer, 0, buffer.Length);
		}

		internal void BinaryWrite (byte [] buffer, int start, int length)
		{
			OutputStream.Write (buffer, start, length);
		}
		
		public void Clear ()
		{
			if (_Writer != null)
				_Writer.Clear ();
		}

		public void ClearContent ()
		{
			Clear();
		}

		internal void SetHeadersSent (bool val)
		{
			_bHeadersSent = val;
		}
		
		public void ClearHeaders ()
		{
			if (_bHeadersSent)
				throw new HttpException ("Headers has been sent to the client");

			_sContentType = "text/html";

			_iStatusCode = 200;
			_sCharset = null;
			_Headers = new ArrayList ();
			_sCacheControl = null;
			_sTransferEncoding = null;

			_lContentLength = 0;
			_bSuppressContent = false;
			_bSuppressHeaders = false;
			_bClientDisconnected = false;
		}

		public void Close ()
		{
			if (closed && !_bClientDisconnected) {
				_bClientDisconnected = false;
				_WorkerRequest.CloseConnection ();
				_bClientDisconnected = true;
			}
		}

		internal void Dispose ()
		{
			if (_Writer != null) {
				_Writer.Dispose ();
				_Writer = null;
			}
		}

		[MonoTODO("Handle callbacks into before done with session, needs to have a non ended flush here")]
		internal void FlushAtEndOfRequest () 
		{
			Flush (true);
		}

		public void End ()
		{
			if (_bEnded)
				return;

			if (_Context.TimeoutPossible)
				Thread.CurrentThread.Abort (new StepCompleteRequest ());

			Flush ();
			_bEnded = true;
			_Context.ApplicationInstance.CompleteRequest ();
		}

		public void Flush ()
		{
			if (closed)
				throw new HttpException ("Response already finished.");

			Flush (false);
		}

		private void Flush (bool bFinish)
		{
			if (_bFlushing || closed)
				return;

			_bFlushing = true;

			if (_Writer == null) {
				_TextWriter.Flush ();
				_bFlushing = false;
				return;
			}

			try {
				if (_bClientDisconnected)
					return;

				long length = _Writer.BufferSize;
				if (!_bHeadersSent && !_bSuppressHeaders) {
					if (bFinish) {
						if (length == 0 && _lContentLength == 0)
							_sContentType = null;

						SendHeaders ();
						length = _Writer.BufferSize;
						if (length != 0)
							_WorkerRequest.SendCalculatedContentLength ((int) length);
					} else {
						if (_lContentLength == 0 && _iStatusCode == 200 &&
						   _sTransferEncoding == null) {
							// Check we are going todo chunked encoding
							string sProto = Request.ServerVariables ["SERVER_PROTOCOL"];
							if (sProto != null && sProto == "HTTP/1.1") {
								AppendHeader (
									HttpWorkerRequest.HeaderTransferEncoding,
									"chunked");
							}  else {
								// Just in case, the old browsers send a HTTP/1.0
								// request with Connection: Keep-Alive
								AppendHeader (
									HttpWorkerRequest.HeaderConnection,
									"Close");
							}
						}

						length = _Writer.BufferSize;
						SendHeaders ();
					}
				}

				if (!filtered) {
					_Writer.FilterData (false);
					length = _Writer.BufferSize;
				}

				if (length == 0) {
					if (bFinish && _bChunked) {
						_WorkerRequest.SendResponseFromMemory (s_arrChunkEnd,
										s_arrChunkEnd.Length);
					}

					_WorkerRequest.FlushResponse (bFinish);
					if (!bFinish)
						_Writer.Clear ();
					return;
				}

				if (!_bSuppressContent && Request.HttpMethod == "HEAD")
					_bSuppressContent = true;

				if (_bSuppressContent)
					_Writer.Clear ();

				if (!_bSuppressContent) {
					_bClientDisconnected = false;
					if (_bChunked) {
						Encoding oASCII = Encoding.ASCII;

						string chunk = Convert.ToString(_Writer.BufferSize, 16);
						byte [] arrPrefix = oASCII.GetBytes (chunk + s_sChunkedPrefix);

						_WorkerRequest.SendResponseFromMemory (arrPrefix,
										       arrPrefix.Length);

						_Writer.SendContent (_WorkerRequest);

						_WorkerRequest.SendResponseFromMemory (s_arrChunkSuffix,
										       s_arrChunkSuffix.Length);
						if (bFinish)
							_WorkerRequest.SendResponseFromMemory (
									s_arrChunkEnd, s_arrChunkEnd.Length);
					} else {
						_Writer.SendContent (_WorkerRequest);
					}
				}

				_WorkerRequest.FlushResponse (bFinish);
				if (IsCached) {
					cached_response.ContentLength = (int) length;
					cached_response.SetData (_Writer.GetBuffer ());
				}
				_Writer.Clear ();
			} finally {
				if (bFinish)
					closed = true;
				_bFlushing = false;
			}
		}

		public void Pics (string value)
		{
			AppendHeader ("PICS-Label", value);
		}


		public void Redirect (string url)
		{
			Redirect (url, true);
		}

		public void Redirect (string url, bool endResponse)
		{
			if (_bHeadersSent)
				throw new HttpException ("Headers has been sent to the client");

			Clear ();

			url = ApplyAppPathModifier (url);
			StatusCode = 302;
			AppendHeader (HttpWorkerRequest.HeaderLocation, url);

			// Text for browsers that can't handle location header
			Write ("<html><head><title>Object moved</title></head><body>\r\n");
			Write ("<h2>Object moved to <a href='" + url + "'>here</a></h2>\r\n");
			Write ("</body><html>\r\n");

			if (endResponse)
				End ();
		}

		internal bool RedirectCustomError (string errorPage)
		{
			if (_bHeadersSent)
				return false;

			if (Request.QueryString ["aspxerrorpath"] != null)
				return false; // Prevent endless loop

			Redirect (errorPage + "?aspxerrorpath=" + Request.Path, false);
			return true;
		}
		
		public void Write (char ch)
		{
			_TextWriter.Write(ch);
		}

		public void Write (object obj)
		{
			_TextWriter.Write(obj);
		}

		public void Write (string str)
		{
			_TextWriter.Write (str);
		}

		public void Write (char [] buffer, int index, int count)
		{
			_TextWriter.Write (buffer, index, count);
		}

		public static void RemoveOutputCacheItem (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			if (!UrlUtils.IsRooted (path))
				throw new ArgumentException ("Invalid path for HttpResponse.RemoveOutputCacheItem '" +
					path + "'. An absolute virtual path is expected.");

			Cache cache = HttpRuntime.Cache;
			cache.Remove (path);
		}

		public void SetCookie (HttpCookie cookie)
		{
			if (_bHeadersSent)
				throw new HttpException ("Cannot append cookies after HTTP headers have been sent");

			Cookies.Add (cookie);
		}

		private void WriteFromStream (Stream stream, long offset, long length, long bufsize)
		{
			if (offset < 0 || length <= 0)
				return;
			
			long stLength = stream.Length;
			if (offset + length > stLength)
				length = stLength - offset;

			if (offset > 0)
				stream.Seek (offset, SeekOrigin.Begin);

			byte [] fileContent = new byte [bufsize];
			int count = (int) Math.Min (Int32.MaxValue, bufsize);
			while (length > 0 && (count = stream.Read (fileContent, 0, count)) != 0) {
				_Writer.WriteBytes (fileContent, 0, count);
				length -= count;
				count = (int) Math.Min (length, fileContent.Length);
			}
		}

		public void WriteFile (string filename)
		{
			WriteFile (filename, false);
		}

		public void WriteFile (string filename, bool readIntoMemory)
		{
			FileStream fs = null;
			try {
				fs = File.OpenRead (filename);
				long size = fs.Length;
				if (readIntoMemory) {
					WriteFromStream (fs, 0, size, size);
				} else {
					WriteFromStream (fs, 0, size, 8192);
				}
			} finally {
				if (fs != null)
					fs.Close ();
			}
		}

		public void WriteFile (string filename, long offset, long size)
		{
			FileStream fs = null;
			try {
				fs = File.OpenRead (filename);
				WriteFromStream (fs, offset, size, 8192);
			} finally {
				if (fs != null)
					fs.Close ();
			}
		}

		public void WriteFile (IntPtr fileHandle, long offset, long size)
		{
			FileStream fs = null;
			try {
				fs = new FileStream (fileHandle, FileAccess.Read);
				WriteFromStream (fs, offset, size, 8192);
			} finally {
				if (fs != null)
					fs.Close ();
			}
		}   

		[MonoTODO()]
		internal void OnCookieAdd (HttpCookie cookie)
		{
		}

		[MonoTODO("Do we need this?")]
		internal void OnCookieChange (HttpCookie cookie)
		{
		}

		[MonoTODO()]
		internal void GoingToChangeCookieColl ()
		{
		}

		[MonoTODO()]
		internal void ChangedCookieColl ()
		{
		}
	}
}
