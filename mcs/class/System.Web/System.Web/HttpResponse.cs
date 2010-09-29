//
// System.Web.HttpResponse.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Web.UI;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web.Caching;
using System.Threading;
using System.Web.Util;
using System.Web.Configuration;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Hosting;
using System.Web.SessionState;

#if NET_4_0
using System.Web.Routing;
#endif

namespace System.Web
{	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed partial class HttpResponse
	{
		internal HttpWorkerRequest WorkerRequest;
		internal HttpResponseStream output_stream;
		internal bool buffer = true;

		ArrayList fileDependencies;
		
		HttpContext context;
		TextWriter writer;
		HttpCachePolicy cache_policy;
		Encoding encoding;
		HttpCookieCollection cookies;
		
		int status_code = 200;
		string status_description = "OK";

		string content_type = "text/html";
		string charset;
		bool charset_set;
		CachedRawResponse cached_response;
		string user_cache_control = "private";
		string redirect_location;
		string version_header;
		bool version_header_checked;
		
		//
		// Negative Content-Length means we auto-compute the size of content-length
		// can be overwritten with AppendHeader ("Content-Length", value)
		//
		long content_length = -1;

		//
		// The list of the headers that we will send back to the client, except
		// the headers that we compute here.
		//

		HttpHeaderCollection headers;
		bool headers_sent;
		NameValueCollection cached_headers;

		//
		// Transfer encoding state
		//
		string transfer_encoding;
		internal bool use_chunked;
		
		bool closed;
		internal bool suppress_content;

		//
		// Session State
		//
		string app_path_mod;
		bool is_request_being_redirected;
		Encoding headerEncoding;

		internal HttpResponse ()
		{
			output_stream = new HttpResponseStream (this);
			writer = new HttpWriter (this);
		}

		public HttpResponse (TextWriter writer) : this ()
		{

			this.writer = writer;
		}

		internal HttpResponse (HttpWorkerRequest worker_request, HttpContext context) : this ()
		{
			WorkerRequest = worker_request;
			this.context = context;

#if !TARGET_J2EE
			if (worker_request != null)
				use_chunked = (worker_request.GetHttpVersion () == "HTTP/1.1");
#endif
			writer = new HttpWriter (this);
		}

		internal TextWriter SetTextWriter (TextWriter writer)
		{
			TextWriter prev = this.writer;
			this.writer = writer;
			return prev;
		}

		internal string VersionHeader {
			get {
				if (!version_header_checked && version_header == null) {
					version_header_checked = true;
					HttpRuntimeSection config = WebConfigurationManager.GetWebApplicationSection ("system.web/httpRuntime") as HttpRuntimeSection;
					if (config != null && config.EnableVersionHeader)
						version_header = Environment.Version.ToString (3);
				}

				return version_header;
			}
		}

		internal HttpContext Context {
			get { return context; }
			set { context = value; }
		}
			
		internal string[] FileDependencies {
			get {
				if (fileDependencies == null || fileDependencies.Count == 0)
					return new string[0] {};
				return (string[]) fileDependencies.ToArray (typeof (string));
			}
		}
		
		ArrayList FileDependenciesArray {
			get {
				if (fileDependencies == null)
					fileDependencies = new ArrayList ();
				return fileDependencies;
			}
		}
		
		public bool Buffer {
			get {
				return buffer;
			}

			set {
				buffer = value;
			}
		}

		public bool BufferOutput {
			get {
				return buffer;
			}

			set {
				buffer = value;
			}
		}

		//
		// Use the default from <globalization> section if the client has not set the encoding
		//
		public Encoding ContentEncoding {
			get {
				if (encoding == null) {
					if (context != null) {
						string client_content_type = context.Request.ContentType;
						string parameter = HttpRequest.GetParameter (client_content_type, "; charset=");
						if (parameter != null) {
							try {
								// Do what the #1 web server does
								encoding = Encoding.GetEncoding (parameter);
							} catch {
							}
						}
					}
					if (encoding == null)
						encoding = WebEncoding.ResponseEncoding;
				}
				return encoding;
			}

			set {
				if (value == null)
					throw new ArgumentException ("ContentEncoding can not be null");

				encoding = value;
				HttpWriter http_writer = writer as HttpWriter;
				if (http_writer != null)
					http_writer.SetEncoding (encoding);
			}
		}
		
		public string ContentType {
			get {
				return content_type;
			}

			set {
				content_type = value;
			}
		}

		public string Charset {
			get {
				if (charset == null)
					charset = ContentEncoding.WebName;
				
				return charset;
			}

			set {
				charset_set = true;
				charset = value;
			}
		}
		
		public HttpCookieCollection Cookies {
			get {
				if (cookies == null)
					cookies = new HttpCookieCollection (true, false);
				return cookies;
			}
		}
		
		public int Expires {
			get {
				if (cache_policy == null)
					return 0;

				return cache_policy.ExpireMinutes ();
			}

			set {
				Cache.SetExpires (DateTime.Now + new TimeSpan (0, value, 0));
			}
		}
		
		public DateTime ExpiresAbsolute {
			get {
				return Cache.Expires;
			}

			set {
				Cache.SetExpires (value);
			}
		}

		public Stream Filter {
			get {
				if (WorkerRequest == null || output_stream == null)
					return null;
				
				return output_stream.Filter;
			}

			set {
				if (output_stream != null)
					output_stream.Filter = value;
			}
		}

		public Encoding HeaderEncoding {
			get {
				if (headerEncoding == null) {
					GlobalizationSection gs = WebConfigurationManager.SafeGetSection ("system.web/globalization", typeof (GlobalizationSection)) as GlobalizationSection;

					if (gs == null)
						headerEncoding = Encoding.UTF8;
					else {
						headerEncoding = gs.ResponseHeaderEncoding;
						if (headerEncoding == Encoding.Unicode)
							throw new HttpException ("HeaderEncoding must not be Unicode");
					}
				}
				return headerEncoding;
			}
			set {
				if (headers_sent)
					throw new HttpException ("headers have already been sent");
				if (value == null)
					throw new ArgumentNullException ("HeaderEncoding");
				if (value == Encoding.Unicode)
					throw new HttpException ("HeaderEncoding must not be Unicode");
				headerEncoding = value;
			}
		}

		public NameValueCollection Headers {
			get {
				if (headers == null)
					headers = new HttpHeaderCollection ();

				return headers;
			}
		}

		
		public bool IsClientConnected {
			get {
				if (WorkerRequest == null)
					return true; // yep that's true

				return WorkerRequest.IsClientConnected ();
			}
		}

		public bool IsRequestBeingRedirected {
			get { return is_request_being_redirected; }
		}

		public TextWriter Output {
			get {
				return writer;
			}
#if NET_4_0
			set { writer = value; }
#endif
		}

		public Stream OutputStream {
			get {
				return output_stream;
			}
		}
		
		public string RedirectLocation {
			get {
				return redirect_location;
			}

			set {
				redirect_location = value;
			}
		}
		
		public string Status {
			get { return String.Concat (status_code.ToString (), " ", StatusDescription); }

			set {
				int p = value.IndexOf (' ');
				if (p == -1)
					throw new HttpException ("Invalid format for the Status property");

				string s = value.Substring (0, p);
				
				if (!Int32.TryParse (s, out status_code))
					throw new HttpException ("Invalid format for the Status property");
				
				status_description = value.Substring (p+1);
			}
		}

		// We ignore the two properties on Mono as they are for use with IIS7, but there is
		// no point in throwing PlatformNotSupportedException. We might find a use for them
		// some day.
		public int SubStatusCode {
			get;
			set;
		}

		public bool TrySkipIisCustomErrors {
			get;
			set;
		}
		
		public int StatusCode {
			get {
				return status_code;
			}

			set {
				if (headers_sent)
					throw new HttpException ("headers have already been sent");
				
				status_code = value;
				status_description = null;
			}
		}

		public string StatusDescription {
			get {
				if (status_description == null)
					status_description = HttpWorkerRequest.GetStatusDescription (status_code);

				return status_description;
			}

			set {
				if (headers_sent)
					throw new HttpException ("headers have already been sent");
				
				status_description = value;
			}
		}
		
		public bool SuppressContent {
			get {
				return suppress_content;
			}

			set {
				suppress_content = value;
			}
		}

		[MonoTODO ("Not implemented")]
		public void AddCacheDependency (CacheDependency[] dependencies)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public void AddCacheItemDependencies (string[] cacheKeys)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Currently does nothing")]
		public void AddCacheItemDependencies (ArrayList cacheKeys)
		{
			// TODO: talk to jackson about the cache
		}

		[MonoTODO("Currently does nothing")]
		public void AddCacheItemDependency (string cacheKey)
		{
			// TODO: talk to jackson about the cache
		}
		
		public void AddFileDependencies (ArrayList filenames)
		{
			if (filenames == null || filenames.Count == 0)
				return;
			FileDependenciesArray.AddRange (filenames);
		}

		public void AddFileDependencies (string[] filenames)
		{
			if (filenames == null || filenames.Length == 0)
				return;
			FileDependenciesArray.AddRange (filenames);
		}

		public void AddFileDependency (string filename)
		{
			if (filename == null || filename == String.Empty)
				return;
			FileDependenciesArray.Add (filename);
		}

		public void AddHeader (string name, string value)
		{
			AppendHeader (name, value);
		}

		public void AppendCookie (HttpCookie cookie)
		{
			Cookies.Add (cookie);
		}

		//
		// AppendHeader:
		//    Special case for Content-Length, Content-Type, Transfer-Encoding and Cache-Control
		//
		//
		public void AppendHeader (string name, string value)
		{
			if (headers_sent)
				throw new HttpException ("Headers have been already sent");
#if !TARGET_J2EE
			if (String.Compare (name, "content-length", true, Helpers.InvariantCulture) == 0){
				content_length = (long) UInt64.Parse (value);
				use_chunked = false;
				return;
			}
#endif

			if (String.Compare (name, "content-type", true, Helpers.InvariantCulture) == 0){
				ContentType = value;
				return;
			}

#if !TARGET_J2EE
			if (String.Compare (name, "transfer-encoding", true, Helpers.InvariantCulture) == 0){
				transfer_encoding = value;
				use_chunked = false;
				return;
			}
#endif

			if (String.Compare (name, "cache-control", true, Helpers.InvariantCulture) == 0){
				user_cache_control = value;
				return;
			}

			Headers.Add (name, value);
		}

		[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
		public void AppendToLog (string param)
		{
			Console.Write ("System.Web: ");
			Console.WriteLine (param);
		}
		
		public string ApplyAppPathModifier (string virtualPath)
		{
			if (virtualPath == null || context == null)
				return null;
		
			if (virtualPath.Length == 0)
				return context.Request.RootVirtualDir;

			if (UrlUtils.IsRelativeUrl (virtualPath)) {
				virtualPath = UrlUtils.Combine (context.Request.RootVirtualDir, virtualPath);
			} else if (UrlUtils.IsRooted (virtualPath)) {
				virtualPath = UrlUtils.Canonic (virtualPath);
			}

			bool cookieless = false;
			SessionStateSection config = WebConfigurationManager.GetWebApplicationSection ("system.web/sessionState") as SessionStateSection;
			cookieless = SessionStateModule.IsCookieLess (context, config);

			if (!cookieless)
				return virtualPath;

			if (app_path_mod != null && virtualPath.IndexOf (app_path_mod) < 0) {
				if (UrlUtils.HasSessionId (virtualPath))
					virtualPath = UrlUtils.RemoveSessionId (VirtualPathUtility.GetDirectory (virtualPath), virtualPath);
				return UrlUtils.InsertSessionId (app_path_mod, virtualPath);
			}
		
			return virtualPath;
		}

		public void BinaryWrite (byte [] buffer)
		{
			if (output_stream != null)
				output_stream.Write (buffer, 0, buffer.Length);
		}

		internal void BinaryWrite (byte [] buffer, int start, int len)
		{
			if (output_stream != null)
				output_stream.Write (buffer, start, len);
		}

		public void Clear ()
		{
			ClearContent ();
		}

		public void ClearContent ()
		{
			if (output_stream != null)
				output_stream.Clear ();
			content_length = -1;
		}

		public void ClearHeaders ()
		{
			if (headers_sent)
				throw new HttpException ("headers have been already sent");

			// Reset the special case headers.
			content_length = -1;
			content_type = "text/html";
			transfer_encoding = null;
			user_cache_control = "private";
			if (cache_policy != null)
				cache_policy.Cacheability = HttpCacheability.Private;

			if (headers != null)
				headers.Clear ();
		}

		internal bool HeadersSent {
			get {
				return headers_sent;
			}
		}

		public void Close ()
		{
			if (closed)
				return;
			if (WorkerRequest != null)
				WorkerRequest.CloseConnection ();
			closed = true;
		}

		public void DisableKernelCache ()
		{
			// does nothing in Mono
		}
		
		public void End ()
		{
			if (context == null)
				return;
			
			if (context.TimeoutPossible) {
				Thread.CurrentThread.Abort (FlagEnd.Value);
			} else {
				// If this is called from an async event, signal the completion
				// but don't throw.
				HttpApplication app_instance = context.ApplicationInstance;
				if (app_instance != null)
					app_instance.CompleteRequest ();
			}
		}

		// Generate:
		//   Content-Length
		//   Content-Type
		//   Transfer-Encoding (chunked)
		//   Cache-Control
		//   X-AspNet-Version
		void AddHeadersNoCache (NameValueCollection write_headers, bool final_flush)
		{
#if !TARGET_J2EE
			//
			// Transfer-Encoding
			//
			if (use_chunked)
				write_headers.Add ("Transfer-Encoding", "chunked");
			else if (transfer_encoding != null)
				write_headers.Add ("Transfer-Encoding", transfer_encoding);
#endif
			if (redirect_location != null)
				write_headers.Add ("Location", redirect_location);
			
#if !TARGET_J2EE
			string vh = VersionHeader;
			if (vh != null)
				write_headers.Add ("X-AspNet-Version", vh);

			//
			// If Content-Length is set.
			//
			if (content_length >= 0) {
				write_headers.Add (HttpWorkerRequest.GetKnownResponseHeaderName (HttpWorkerRequest.HeaderContentLength),
						   content_length.ToString (Helpers.InvariantCulture));
			} else if (BufferOutput) {
				if (final_flush) {					
					//
					// If we are buffering and this is the last flush, not a middle-flush,
					// we know the content-length.
					//
					if (output_stream != null)
						content_length = output_stream.total;
					write_headers.Add (HttpWorkerRequest.GetKnownResponseHeaderName (HttpWorkerRequest.HeaderContentLength),
							   content_length.ToString (Helpers.InvariantCulture));
				} else {
					//
					// We are buffering, and this is a flush in the middle.
					// If we are not chunked, we need to set "Connection: close".
					//
					if (use_chunked){
						write_headers.Add (HttpWorkerRequest.GetKnownResponseHeaderName (HttpWorkerRequest.HeaderConnection), "close");
					}
				}
			} else {
				//
				// If the content-length is not set, and we are not buffering, we must
				// close at the end.
				//
				if (use_chunked){
					write_headers.Add (HttpWorkerRequest.GetKnownResponseHeaderName (HttpWorkerRequest.HeaderConnection), "close");
				}
			}
#endif

			//
			// Cache Control, the cache policy takes precedence over the cache_control property.
			//
			if (cache_policy != null)
				cache_policy.SetHeaders (this, headers);
			else
				write_headers.Add ("Cache-Control", CacheControl);
			
			//
			// Content-Type
			//
			if (content_type != null){
				string header = content_type;

				if (charset_set || header == "text/plain" || header == "text/html") {
					if (header.IndexOf ("charset=") == -1) {
						if (charset == null || charset == "")
							charset = ContentEncoding.HeaderName;
						header += "; charset=" + charset;
					}
				}
				
				write_headers.Add ("Content-Type", header);
			}

			if (cookies != null && cookies.Count != 0){
				int n = cookies.Count;
				for (int i = 0; i < n; i++)
					write_headers.Add ("Set-Cookie", cookies.Get (i).GetCookieHeaderValue ());
#if TARGET_J2EE
				// For J2EE Portal support emulate cookies by storing them in the session.
				context.Request.SetSessionCookiesForPortal (cookies);
#endif
			}
		}

		internal void WriteHeaders (bool final_flush)
		{
			if (headers_sent)
				return;

			//
			// Flush
			//
			if (context != null) {
				HttpApplication app_instance = context.ApplicationInstance;
				if (app_instance != null)
					app_instance.TriggerPreSendRequestHeaders ();
			}

			headers_sent = true;

			if (cached_response != null)
				cached_response.SetHeaders (headers);

			// If this page is cached use the cached headers
			// instead of the standard headers	
			NameValueCollection write_headers;
			if (cached_headers != null)
				write_headers = cached_headers;
			else {
				write_headers = Headers;
				AddHeadersNoCache (write_headers, final_flush);
			}
			
			if (WorkerRequest != null)
				WorkerRequest.SendStatus (status_code, StatusDescription);

			if (WorkerRequest != null) {
				string header_name;
				string[] values;
				int header_index;
				
				for (int i = 0; i < write_headers.Count; i++) {
					header_name = write_headers.GetKey (i);
					header_index = HttpWorkerRequest.GetKnownResponseHeaderIndex (header_name);
					values = write_headers.GetValues (i);
					if (values == null)
						continue;
					
					foreach (string val in values) {
						if (header_index > -1)
							WorkerRequest.SendKnownResponseHeader (header_index, val);
						else
							WorkerRequest.SendUnknownResponseHeader (header_name, val);
					}
				}
			}
		}

		internal void DoFilter (bool close)
		{
			if (output_stream != null && output_stream.HaveFilter && context != null && context.Error == null)
				output_stream.ApplyFilter (close);
		}

		internal void Flush (bool final_flush)
		{
			DoFilter (final_flush);
			if (!headers_sent){
				if (final_flush || status_code != 200)
					use_chunked = false;
			}

			bool head = ((context != null) && (context.Request.HttpMethod == "HEAD"));
			if (suppress_content || head) {
				if (!headers_sent)
					WriteHeaders (true);
				output_stream.Clear ();
				if (WorkerRequest != null && output_stream != null)
					output_stream.Flush (WorkerRequest, true); // ignore final_flush here.
				return;
			}

			if (!headers_sent)
				WriteHeaders (final_flush);

			if (context != null) {
				HttpApplication app_instance = context.ApplicationInstance;
				if (app_instance != null)
					app_instance.TriggerPreSendRequestContent ();
			}

			if (IsCached)
				cached_response.SetData (output_stream.GetData ());

			if (WorkerRequest != null && output_stream != null)
				output_stream.Flush (WorkerRequest, final_flush);
		}

		public void Flush ()
		{
			Flush (false);
		}

		public void Pics (string value)
		{
			AppendHeader ("PICS-Label", value);
		}

		void Redirect (string url, bool endResponse, int code)
		{
			if (url == null)
				throw new ArgumentNullException ("url");
			
			if (headers_sent)
				throw new HttpException ("Headers have already been sent");

			if (url.IndexOf ('\n') != -1)
				throw new ArgumentException ("Redirect URI cannot contain newline characters.", "url");
			
			is_request_being_redirected = true;
			ClearHeaders ();
			ClearContent ();
			
			StatusCode = code;
			url = ApplyAppPathModifier (url);

			bool isFullyQualified;
			if (StrUtils.StartsWith (url, "http:", true) ||
			    StrUtils.StartsWith (url, "https:", true) ||
			    StrUtils.StartsWith (url, "file:", true) ||
			    StrUtils.StartsWith (url, "ftp:", true))
				isFullyQualified = true;
			else
				isFullyQualified = false;

			if (!isFullyQualified) {
				HttpRuntimeSection config = WebConfigurationManager.GetWebApplicationSection ("system.web/httpRuntime") as HttpRuntimeSection;
				if (config != null && config.UseFullyQualifiedRedirectUrl) {
					var ub = new UriBuilder (context.Request.Url);
					ub.Path = url;
					ub.Fragment = null;
					ub.Password = null;
					ub.Query = null;
					ub.UserName = null;
					url = ub.Uri.ToString ();
				}
			}
			
			redirect_location = url;

			// Text for browsers that can't handle location header
			Write ("<html><head><title>Object moved</title></head><body>\r\n");
			Write ("<h2>Object moved to <a href=\"" + url + "\">here</a></h2>\r\n");
			Write ("</body><html>\r\n");
			
			if (endResponse)
				End ();
			is_request_being_redirected = false;
		}
		
		public void Redirect (string url)
		{
			Redirect (url, true);
		}

		public void Redirect (string url, bool endResponse)
		{
			Redirect (url, endResponse, 302);
		}
#if NET_4_0
		public void RedirectPermanent (string url)
		{
			RedirectPermanent (url, true);
		}

		public void RedirectPermanent (string url, bool endResponse)
		{
			Redirect (url, endResponse, 301);
		}

		public void RedirectToRoute (object routeValues)
		{
			RedirectToRoute ("RedirectToRoute", null, new RouteValueDictionary (routeValues), 302, true);
		}

		public void RedirectToRoute (RouteValueDictionary routeValues)
		{
			RedirectToRoute ("RedirectToRoute", null, routeValues, 302, true);
		}

		public void RedirectToRoute (string routeName)
		{
			RedirectToRoute ("RedirectToRoute", routeName, null, 302, true);
		}

		public void RedirectToRoute (string routeName, object routeValues)
		{
			RedirectToRoute ("RedirectToRoute", routeName, new RouteValueDictionary (routeValues), 302, true);
		}

		public void RedirectToRoute (string routeName, RouteValueDictionary routeValues)
		{
			RedirectToRoute ("RedirectToRoute", routeName, routeValues, 302, true);
		}

		public void RedirectToRoutePermanent (object routeValues)
		{
			RedirectToRoute ("RedirectToRoutePermanent", null, new RouteValueDictionary (routeValues), 301, false);
		}

		public void RedirectToRoutePermanent (RouteValueDictionary routeValues)
		{
			RedirectToRoute ("RedirectToRoutePermanent", null, routeValues, 301, false);
		}

		public void RedirectToRoutePermanent (string routeName)
		{
			RedirectToRoute ("RedirectToRoutePermanent", routeName, null, 301, false);
		}

		public void RedirectToRoutePermanent (string routeName, object routeValues)
		{
			RedirectToRoute ("RedirectToRoutePermanent", routeName, new RouteValueDictionary (routeValues), 301, false);
		}		

		public void RedirectToRoutePermanent (string routeName, RouteValueDictionary routeValues)
		{
			RedirectToRoute ("RedirectToRoutePermanent", routeName, routeValues, 301, false);
		}
		
		void RedirectToRoute (string callerName, string routeName, RouteValueDictionary routeValues, int redirectCode, bool endResponse)
		{
			HttpContext ctx = context ?? HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			
			if (req == null)
				// Let's emulate .NET
				throw new NullReferenceException ();
			
			VirtualPathData vpd = RouteTable.Routes.GetVirtualPath (req.RequestContext, routeName, routeValues);
			string redirectUrl = vpd != null ? vpd.VirtualPath : null;
			if (String.IsNullOrEmpty (redirectUrl))
				throw new InvalidOperationException ("No matching route found for RedirectToRoute");

			Redirect (redirectUrl, true, redirectCode);
		}

		public static void RemoveOutputCacheItem (string path, string providerName)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Length > 0 && path [0] != '/')
				throw new ArgumentException ("Invalid path for HttpResponse.RemoveOutputCacheItem: '" + path + "'. An absolute virtual path is expected");

			OutputCache.RemoveFromProvider (path, providerName);
		}
#endif
		public static void RemoveOutputCacheItem (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Length == 0)
				return;

			if (path [0] != '/')
				throw new ArgumentException ("'" + path + "' is not an absolute virtual path.");

#if NET_4_0
			RemoveOutputCacheItem (path, OutputCache.DefaultProviderName);
#else
			HttpContext context = HttpContext.Current;
			HttpApplication app_instance = context != null ? context.ApplicationInstance : null;
			HttpModuleCollection modules = app_instance != null ? app_instance.Modules : null;
			OutputCacheModule ocm = modules != null ? modules.Get ("OutputCache") as OutputCacheModule : null;
			OutputCacheProvider internalProvider = ocm != null ? ocm.InternalProvider : null;
			if (internalProvider == null)
				return;

			internalProvider.Remove (path);
#endif
		}

		public void SetCookie (HttpCookie cookie)
		{
			AppendCookie (cookie);
		}

		public void Write (char ch)
		{
			TextWriter writer = Output;
#if NET_4_0
			// Emulating .NET
			if (writer == null)
				throw new NullReferenceException (".NET 4.0 emulation. A null value was found where an object was required.");
#endif
			writer.Write (ch);
		}

		public void Write (object obj)
		{
			TextWriter writer = Output;
#if NET_4_0
			// Emulating .NET
			if (writer == null)
				throw new NullReferenceException (".NET 4.0 emulation. A null value was found where an object was required.");
#endif
			if (obj == null)
				return;
			
			writer.Write (obj.ToString ());
		}
		
		public void Write (string s)
		{
			TextWriter writer = Output;
#if NET_4_0
			// Emulating .NET
			if (writer == null)
				throw new NullReferenceException (".NET 4.0 emulation. A null value was found where an object was required.");
#endif
			writer.Write (s);
		}
		
		public void Write (char [] buffer, int index, int count)
		{
			TextWriter writer = Output;
#if NET_4_0
			// Emulating .NET
			if (writer == null)
				throw new NullReferenceException (".NET 4.0 emulation. A null value was found where an object was required.");
#endif
			writer.Write (buffer, index, count);
		}

		internal void WriteFile (FileStream fs, long offset, long size)
		{
			if (output_stream == null)
				return;
			
			byte [] buffer = new byte [32*1024];

			if (offset != 0)
				fs.Position = offset;

			long remain = size;
			int n;
			while (remain > 0 && (n = fs.Read (buffer, 0, (int) Math.Min (remain, 32*1024))) != 0){
				remain -= n;
				output_stream.Write (buffer, 0, n);
			}
		}
		
		public void WriteFile (string filename)
		{
			WriteFile (filename, false);
		}

		public void WriteFile (string filename, bool readIntoMemory)
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			if (readIntoMemory){
				using (FileStream fs = File.OpenRead (filename))
					WriteFile (fs, 0, fs.Length);
			} else {
				FileInfo fi = new FileInfo (filename);
				if (output_stream != null)
					output_stream.WriteFile (filename, 0, fi.Length);
			}
			if (buffer)
				return;

			if (output_stream != null)
				output_stream.ApplyFilter (false);
			Flush ();
		}

#if TARGET_JVM
		public void WriteFile (IntPtr fileHandle, long offset, long size) {
			throw new PlatformNotSupportedException("IntPtr not supported");
		}
#else
		public void WriteFile (IntPtr fileHandle, long offset, long size)
		{
			if (offset < 0)
				throw new ArgumentNullException ("offset can not be negative");
			if (size < 0)
				throw new ArgumentNullException ("size can not be negative");

			if (size == 0)
				return;

			// Note: this .ctor will throw a SecurityException if the caller 
			// doesn't have the UnmanagedCode permission
			using (FileStream fs = new FileStream (fileHandle, FileAccess.Read))
				WriteFile (fs, offset, size);

			if (buffer)
				return;
			if (output_stream != null)
				output_stream.ApplyFilter (false);
			Flush ();
		}
#endif

		public void WriteFile (string filename, long offset, long size)
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");
			if (offset < 0)
				throw new ArgumentNullException ("offset can not be negative");
			if (size < 0)
				throw new ArgumentNullException ("size can not be negative");

			if (size == 0)
				return;
			
			FileStream fs = File.OpenRead (filename);
			WriteFile (fs, offset, size);

			if (buffer)
				return;

			if (output_stream != null)
				output_stream.ApplyFilter (false);
			Flush ();
		}

		public void WriteSubstitution (HttpResponseSubstitutionCallback callback)
		{
			// Emulation of .NET behavior
			if (callback == null)
				throw new NullReferenceException ();

			object target = callback.Target;
			if (target != null && target.GetType () == typeof (Control))
				throw new ArgumentException ("callback");

			string s = callback (context);
			if (!IsCached) {
				Write (s);
				return;
			}

			Cache.Cacheability = HttpCacheability.Server;
			Flush ();
			if (WorkerRequest == null)
				Write (s); // better this than nothing
			else {
				byte[] bytes = WebEncoding.ResponseEncoding.GetBytes (s);
				WorkerRequest.SendResponseFromMemory (bytes, bytes.Length);
			}
			
			cached_response.SetData (callback);
		}

		//
		// Like WriteFile, but never buffers, so we manually Flush here
		//
		public void TransmitFile (string filename) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			TransmitFile (filename, false);
		}

		internal void TransmitFile (string filename, bool final_flush)
		{
			FileInfo fi = new FileInfo (filename);
			using (Stream s = fi.OpenRead ()); // Just check if we can read.
			if (output_stream != null) {
				output_stream.WriteFile (filename, 0, fi.Length);
				output_stream.ApplyFilter (final_flush);
			}
			
			Flush (final_flush);
		}

		public void TransmitFile (string filename, long offset, long length)
		{
			if (output_stream != null) {
				output_stream.WriteFile (filename, offset, length);
				output_stream.ApplyFilter (false);
			}
			
			Flush (false);
		}
		
		internal void TransmitFile (VirtualFile vf)
		{
			TransmitFile (vf, false);
		}

		const int bufLen = 65535;
		internal void TransmitFile (VirtualFile vf, bool final_flush)
		{
			if (vf == null)
				throw new ArgumentNullException ("vf");

			if (vf is DefaultVirtualFile) {
				TransmitFile (HostingEnvironment.MapPath (vf.VirtualPath), final_flush);
				return;
			}

			byte[] buf = new byte [bufLen];
			using (Stream s = vf.Open ()) {
				int readBytes;
				if (output_stream != null) {
					while ((readBytes = s.Read (buf, 0, bufLen)) > 0) {
						output_stream.Write (buf, 0, readBytes);
						output_stream.ApplyFilter (final_flush);
						Flush (false);
					}
				}
				
				if (final_flush)
					Flush (true);
			}
		}
		
#region Session state support
		internal void SetAppPathModifier (string app_modifier)
		{
			app_path_mod = app_modifier;
		}
#endregion
		
#region Cache Support
		internal void SetCachedHeaders (NameValueCollection headers)
		{
			cached_headers = headers;
			
		}

		internal bool IsCached {
			get { return cached_response != null; }
			set {
				if (value)
					cached_response = new CachedRawResponse (cache_policy);
				else
					cached_response = null;
			}
		}

		public HttpCachePolicy Cache {
			get {
				if (cache_policy == null)
					cache_policy = new HttpCachePolicy ();
				
				return cache_policy;
			}
		}		

		internal CachedRawResponse GetCachedResponse ()
		{
			if (cached_response != null) {
				cached_response.StatusCode = StatusCode;
				cached_response.StatusDescription = StatusDescription;
			}
			
			return cached_response;
		}

		//
		// This is one of the old ASP compatibility methods, the real cache
		// control is in the Cache property, and this is a second class citizen
		//
		public string CacheControl {
			set {
				if (value == null || value == "") {
					Cache.SetCacheability (HttpCacheability.NoCache);
					user_cache_control = null;
				} else if (String.Compare (value, "public", true, Helpers.InvariantCulture) == 0) {
					Cache.SetCacheability (HttpCacheability.Public);
					user_cache_control = "public";
				} else if (String.Compare (value, "private", true, Helpers.InvariantCulture) == 0) {
					Cache.SetCacheability (HttpCacheability.Private);
					user_cache_control = "private";
				} else if (String.Compare (value, "no-cache", true, Helpers.InvariantCulture) == 0) {
					Cache.SetCacheability (HttpCacheability.NoCache);
					user_cache_control = "no-cache";
				} else
					throw new ArgumentException ("CacheControl property only allows `public', " +
								     "`private' or no-cache, for different uses, use " +
								     "Response.AppendHeader");
			}

			get { return (user_cache_control != null) ? user_cache_control : "private"; }
		}
#endregion

		internal int GetOutputByteCount ()
		{
			if (output_stream != null)
				return output_stream.GetTotalLength ();
			else
				return -1;
		}

		internal void ReleaseResources ()
		{
			if (output_stream != null) {
				output_stream.ReleaseResources (true);
				output_stream = null;
			}
		}
	}

#if TARGET_J2EE
	public 
#endif	
	static class FlagEnd
	{
		public static readonly object Value = new object ();
	}
}

