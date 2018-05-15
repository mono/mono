//
// System.Web.HttpRequest.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//      Marek Habersack <mhabersack@novell.com>
//

//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011-2012 Xamarin, Inc (http://xamarin.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.Configuration;
using System.Web.Management;
using System.Web.UI;
using System.Web.Util;
using System.Globalization;

using System.Security.Authentication.ExtendedProtection;
using System.Web.Routing;

namespace System.Web
{	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed partial class HttpRequest
	{
		HttpWorkerRequest worker_request;
		HttpContext context;
		WebROCollection query_string_nvc;

		//
		//string filename;
		string orig_url = null;
		UriBuilder url_components;

		string client_target;

		//
		// On-demand computed values
		//
		HttpBrowserCapabilities browser_capabilities;
		string file_path, base_virtual_dir, root_virtual_dir, client_file_path;
		string content_type;
		int content_length = -1;
		Encoding encoding;
		string current_exe_path;
		string physical_path;
		string unescaped_path;
		string original_path;
		string path_info;
        	string path_info_unvalidated;
		string raw_url;
        	string raw_url_unvalidated;
		WebROCollection all_params;
		NameValueCollection headers;
        	WebROCollection headers_unvalidated;
		Stream input_stream;
		InputFilterStream input_filter;
		Stream filter;
		HttpCookieCollection cookies;
        	HttpCookieCollection cookies_unvalidated;
		string http_method;

		WebROCollection form;
		HttpFileCollection files;
		
		ServerVariablesCollection server_variables;
		HttpClientCertificate client_cert;
		
		string request_type;
		string [] accept_types;
		string [] user_languages;
		Uri cached_url;
		TempFileStream request_file;

		readonly static System.Net.IPAddress [] host_addresses;
		
		// Validations
		bool validate_cookies, validate_query_string, validate_form;
		bool checked_cookies, checked_query_string, checked_form;
		static readonly UrlMappingCollection urlMappings;
		readonly static char [] queryTrimChars = {'?'};
		bool lazyFormValidation;
		bool lazyQueryStringValidation;
		bool inputValidationEnabled;
		RequestContext requestContext;
		BufferlessInputStream bufferlessInputStream;
		
		static bool validateRequestNewMode;
		internal static bool ValidateRequestNewMode {
			get { return validateRequestNewMode; }
		}

		internal bool InputValidationEnabled {
			get { return inputValidationEnabled; }
		}
		
		private static char[] RequestPathInvalidCharacters {
			get; set;
		}

		private static char[] CharsFromList (string list)
		{
			// List format is very strict and enforced by the Configuration	
			// there must be a single char separated by commas with no trailing comma
			// whitespace is allowed though and should be trimmed.
			
			string [] pieces = list.Split (',');

			char [] chars = new char [pieces.Length];
			for (int i = 0; i < chars.Length; i++) {
				string trimmed = pieces [i].Trim ();
				if (trimmed.Length != 1) {
					// This should have been caught by System.Web.Configuration
					// and throw a configuration error. This is just here for sanity
					throw new System.Configuration.ConfigurationErrorsException ();
				}

				chars [i] = trimmed [0];
			}

			return chars;
		}

		static HttpRequest ()
		{
			try {
				UrlMappingsSection ums = WebConfigurationManager.GetWebApplicationSection ("system.web/urlMappings") as UrlMappingsSection;
				if (ums != null && ums.IsEnabled) {
					urlMappings = ums.UrlMappings;
					if (urlMappings.Count == 0)
						urlMappings = null;
				}

				Version validationMode = HttpRuntime.Section.RequestValidationMode;

				if (validationMode >= new Version (4, 0)) {
					validateRequestNewMode = true;
					string invalidChars = HttpRuntime.Section.RequestPathInvalidCharacters;
					if (!String.IsNullOrEmpty (invalidChars))
						RequestPathInvalidCharacters = CharsFromList (invalidChars);
				}
			} catch {
				// unlikely to happen
			}
			
			host_addresses = GetLocalHostAddresses ();
		}
		
		public HttpRequest (string filename, string url, string queryString)
		{
			// warning 169: what are we supposed to do with filename?
			
			//this.filename = filename;

			orig_url = url;
			url_components = new UriBuilder (url);
			url_components.Query = queryString;
			
			query_string_nvc = new WebROCollection ();
			if (queryString != null)
				HttpUtility.ParseQueryString (queryString, Encoding.Default, query_string_nvc);
			query_string_nvc.Protect ();
		}

		internal HttpRequest (HttpWorkerRequest worker_request, HttpContext context)
		{
			this.worker_request = worker_request;
			this.context = context;
		}
		
		internal UriBuilder UrlComponents {
			get {
				if (url_components == null) {
					string query;
					byte[] queryStringRaw = worker_request.GetQueryStringRawBytes();
					if(queryStringRaw != null)
						query = ContentEncoding.GetString(queryStringRaw);
					else
						query = worker_request.GetQueryString();
					
					BuildUrlComponents (ApplyUrlMapping (worker_request.GetUriPath ()), query);
				}
				return url_components;
			}
		}

		void BuildUrlComponents (string path, string query)
		{
			if (url_components != null)
				return;
			url_components = new UriBuilder ();
			url_components.Scheme = worker_request.GetProtocol ();
			url_components.Host = worker_request.GetServerName ();
			url_components.Port = worker_request.GetLocalPort ();
			url_components.Path = path;
			if (query != null && query.Length > 0)
				url_components.Query = query.TrimStart (queryTrimChars);
		}

		internal string ApplyUrlMapping (string url)
		{
			if (urlMappings == null)
				return url;

			string relUrl = VirtualPathUtility.ToAppRelative (url);
			UrlMapping um = null;
			
			foreach (UrlMapping u in urlMappings) {
				if (u == null)
					continue;
				if (String.Compare (relUrl, u.Url, StringComparison.Ordinal) == 0) {
					um = u;
					break;
				}
			}

			if (um == null)
				return url;

			string rawUrl = VirtualPathUtility.ToAbsolute (um.MappedUrl.Trim ());
			Uri newUrl = new Uri ("http://host.com" + rawUrl);

			if (url_components != null) {
				url_components.Path = newUrl.AbsolutePath;
				url_components.Query = newUrl.Query.TrimStart (queryTrimChars);
				query_string_nvc = new WebROCollection ();
				HttpUtility.ParseQueryString (newUrl.Query, Encoding.Default, query_string_nvc);
				query_string_nvc.Protect ();
			} else
				BuildUrlComponents (newUrl.AbsolutePath, newUrl.Query);

			return url_components.Path;
		}

		string [] SplitHeader (int header_index)
		{
			string [] result = null;
			string header = worker_request.GetKnownRequestHeader (header_index);
			if (header != null && header != "" && header.Trim () != "") {
				result = header.Split (',');
				for (int i = result.Length - 1; i >= 0; i--)
					result [i] = result [i].Trim ();
			}
			return result;
		}

		public string [] AcceptTypes {
			get {
				if (worker_request == null)
					return null;

				if (accept_types == null)
					accept_types = SplitHeader (HttpWorkerRequest.HeaderAccept);

				return accept_types;
			}
		}

		public WindowsIdentity LogonUserIdentity {
			get { throw new NotImplementedException (); }
		}
		
		string anonymous_id;
		public string AnonymousID {
			get {
				return anonymous_id;
			}
			internal set {
				anonymous_id = value;
			}
		}

		public string ApplicationPath {
			get {
				if (worker_request == null)
					return null;
				return worker_request.GetAppPath ();
			}
		}

		public HttpBrowserCapabilities Browser {
			get {
				if (browser_capabilities == null)
					browser_capabilities = HttpCapabilitiesBase.BrowserCapabilitiesProvider.GetBrowserCapabilities (this);

				return browser_capabilities;
			}

			set {
				browser_capabilities = value;
			}
		}

		internal bool BrowserMightHaveSpecialWriter {
			get {
				return (browser_capabilities != null 
					|| HttpApplicationFactory.AppBrowsersFiles.Length > 0);
			}
		}

		internal bool BrowserMightHaveAdapters {
			get {
				return (browser_capabilities != null 
					|| HttpApplicationFactory.AppBrowsersFiles.Length > 0);
			}
		}

		public HttpClientCertificate ClientCertificate {
			get {
				if (client_cert == null)
					client_cert = new HttpClientCertificate (worker_request);
				return client_cert;
			}
		}

		static internal string GetParameter (string header, string attr)
		{
			int ap = header.IndexOf (attr);
			if (ap == -1)
				return null;

			ap += attr.Length;
			if (ap >= header.Length)
				return null;
			
			char ending = header [ap];
			if (ending != '"')
				ending = ' ';
			
			int end = header.IndexOf (ending, ap+1);
			if (end == -1)
				return (ending == '"') ? null : header.Substring (ap);

			return header.Substring (ap+1, end-ap-1);
		}

		public Encoding ContentEncoding {
			get {
				if (encoding == null){
					if (worker_request == null)
						throw HttpException.NewWithCode ("No HttpWorkerRequest", WebEventCodes.RuntimeErrorRequestAbort);
					
					string content_type = ContentType;
					string parameter = GetParameter (content_type, "; charset=");
					if (parameter == null) {
						encoding = WebEncoding.RequestEncoding;
					} else {
						try {
							// Do what the #1 web server does
							encoding = Encoding.GetEncoding (parameter);
						} catch {
							encoding = WebEncoding.RequestEncoding;
						}
					}
				}
				return encoding;
			}

			set {
				encoding = value;
			}
		}

		public int ContentLength {
			get {
				if (content_length == -1){
					if (worker_request == null)
						return 0;

					string cl = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderContentLength);

					if (cl != null) {
						try {
							content_length = Int32.Parse (cl);
						} catch { }
					}
				}

				// content_length will still be < 0, but we know we gotta read from the client
				if (content_length < 0)
					return 0;

				return content_length;
			}
		}

		public string ContentType {
			get {
				if (content_type == null){
					if (worker_request != null)
						content_type = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderContentType);

					if (content_type == null)
						content_type = String.Empty;
				}
				
				return content_type;
			}

			set {
				content_type = value;
			}
		}

		internal HttpCookieCollection CookiesNoValidation {
			get {
				if (cookies_unvalidated == null) {
					if (worker_request == null) {
						cookies_unvalidated = new HttpCookieCollection ();
					} else {
						string cookie_hv = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderCookie);
						cookies_unvalidated = new HttpCookieCollection (cookie_hv);
					}
				}

				return cookies_unvalidated;
			}
		}

		public HttpCookieCollection Cookies {
			get {
				if (cookies == null) {
					cookies = CookiesNoValidation;
				}

				bool needValidation = validate_cookies;
				needValidation |= validateRequestNewMode;
				if (needValidation && !checked_cookies) {
					// Setting this before calling the validator prevents
					// possible endless recursion
					checked_cookies = true;
					ValidateCookieCollection (cookies);
				}

				return cookies;
			}

		}

		public string CurrentExecutionFilePath {
			get {
				if (current_exe_path != null)
					return current_exe_path;

				return FilePath;
			}
		}
		public string CurrentExecutionFilePathExtension {
			get { return global::System.IO.Path.GetExtension (CurrentExecutionFilePath); }
		}
		public string AppRelativeCurrentExecutionFilePath {
			get {
				return VirtualPathUtility.ToAppRelative (CurrentExecutionFilePath);
			}
		}

		public string FilePath {
			get {
				if (worker_request == null)
					return "/"; // required for 2.0

				if (file_path == null)
					file_path = UrlUtils.Canonic (ApplyUrlMapping (worker_request.GetFilePath ()));

				return file_path;
			}
		}

		internal string ClientFilePath {
			get {
				if (client_file_path == null) {
					if (worker_request == null)
						return "/";
					
					return UrlUtils.Canonic (ApplyUrlMapping (worker_request.GetFilePath ()));
				}
				
				return client_file_path;
			}

			set {
				if (value == null || value.Length == 0)
					client_file_path = null;
				else
					client_file_path = value;
			}
		}
		
		internal string BaseVirtualDir {
			get {
				if (base_virtual_dir == null){
					base_virtual_dir = FilePath;
					if (UrlUtils.HasSessionId (base_virtual_dir))
						base_virtual_dir = UrlUtils.RemoveSessionId (VirtualPathUtility.GetDirectory (base_virtual_dir), base_virtual_dir);
					
					int p = base_virtual_dir.LastIndexOf ('/');
					if (p != -1) {
						if (p == 0)
							p = 1;
						base_virtual_dir = base_virtual_dir.Substring (0, p);
					} else
						base_virtual_dir = "/";
				}
				return base_virtual_dir;
			}
		}
		
		public HttpFileCollection Files {
			get {
				if (files == null) {
					files = new HttpFileCollection ();
					if ((worker_request != null) && IsContentType ("multipart/form-data", true)) {
						form = new WebROCollection ();
						LoadMultiPart ();
						form.Protect ();
					}
				}
				return files;
			}
		}

		public Stream Filter {
			get {
				if (filter != null)
					return filter;

				if (input_filter == null)
					input_filter = new InputFilterStream ();

				return input_filter;
			}

			set {
				// This checks that get_ was called before.
				if (input_filter == null)
					throw new HttpException ("Invalid filter");

				filter = value;
			}
		}

		// GetSubStream returns a 'copy' of the InputStream with Position set to 0.
		static Stream GetSubStream (Stream stream)
		{
			if (stream is IntPtrStream)
				return new IntPtrStream (stream);

			if (stream is MemoryStream) {
				MemoryStream other = (MemoryStream) stream;
				return new MemoryStream (other.GetBuffer (), 0, (int) other.Length, false, true);
			}

			if (stream is TempFileStream) {
				((TempFileStream) stream).SavePosition ();
				return stream;
			}

			throw new NotSupportedException ("The stream is " + stream.GetType ());
		}

		static void EndSubStream (Stream stream)
		{
			if (stream is TempFileStream) {
				((TempFileStream) stream).RestorePosition ();
			}
		}

		//
		// Loads the data on the form for multipart/form-data
		//
		void LoadMultiPart ()
		{
			string boundary = GetParameter (ContentType, "; boundary=");
			if (boundary == null)
				return;

			Stream input = GetSubStream (InputStream);
			HttpMultipart multi_part = new HttpMultipart (input, boundary, ContentEncoding);

			HttpMultipart.Element e;
			while ((e = multi_part.ReadNextElement ()) != null) {
				if (e.Filename == null){
					byte [] copy = new byte [e.Length];
				
					input.Position = e.Start;
					input.Read (copy, 0, (int) e.Length);

					form.Add (e.Name, ContentEncoding.GetString (copy));
				} else {
					//
					// We use a substream, as in 2.x we will support large uploads streamed to disk,
					//
					HttpPostedFile sub = new HttpPostedFile (e.Filename, e.ContentType, input, e.Start, e.Length);
					files.AddFile (e.Name, sub);
				}
			}
			EndSubStream (input);
		}

		//
		// Adds the key/value to the form, and sets the argumets to empty
		//
		void AddRawKeyValue (StringBuilder key, StringBuilder value)
		{
			string decodedKey = HttpUtility.UrlDecode (key.ToString (), ContentEncoding);
			form.Add (decodedKey,
				  HttpUtility.UrlDecode (value.ToString (), ContentEncoding));

			key.Length = 0;
			value.Length = 0;
		}

		//
		// Loads the form data from on a application/x-www-form-urlencoded post
		// 
		void LoadWwwForm ()
		{
			using (Stream input = GetSubStream (InputStream)) {
				using (StreamReader s = new StreamReader (input, ContentEncoding)) {
					StringBuilder key = new StringBuilder ();
					StringBuilder value = new StringBuilder ();
					int c;

					while ((c = s.Read ()) != -1){
						if (c == '='){
							value.Length = 0;
							while ((c = s.Read ()) != -1){
								if (c == '&'){
									AddRawKeyValue (key, value);
									break;
								} else
									value.Append ((char) c);
							}
							if (c == -1){
								AddRawKeyValue (key, value);
								return;
							}
						} else if (c == '&')
							AddRawKeyValue (key, value);
						else
							key.Append ((char) c);
					}
					if (c == -1)
						AddRawKeyValue (key, value);

					EndSubStream (input);
				}
			}
		}

		bool IsContentType (string ct, bool starts_with)
		{
			if (starts_with)
				return StrUtils.StartsWith (ContentType, ct, true);

			return String.Compare (ContentType, ct, true, Helpers.InvariantCulture) == 0;
		}

		internal WebROCollection FormUnvalidated {
			get {
				if (form == null){
					form = new WebROCollection ();
					files = new HttpFileCollection ();

					if (IsContentType ("multipart/form-data", true))
						LoadMultiPart ();
					else if (
						IsContentType ("application/x-www-form-urlencoded", true))
						LoadWwwForm ();

					form.Protect ();
				}

				return form;
			}
		}
		
		public NameValueCollection Form {
			get {
				NameValueCollection form = FormUnvalidated;
				if (validateRequestNewMode && !checked_form) {
					if (!lazyFormValidation) {
						// Setting this before calling the validator prevents
						// possible endless recursion
						checked_form = true;
						ValidateNameValueCollection ("Form", form, RequestValidationSource.Form);
					}
				} else
					if (validate_form && !checked_form){
						checked_form = true;
						ValidateNameValueCollection ("Form", form);
					}
				
				return form;
			}
		}

        	internal NameValueCollection HeadersNoValidation {
			get {
				if (headers_unvalidated == null) {
					headers_unvalidated = new HeadersCollection (this);
				}
			
                		return headers_unvalidated;
			}
		}

		public NameValueCollection Headers {
			get {
				if (headers == null) {
					headers = HeadersNoValidation;
					if (validateRequestNewMode) {
						RequestValidator validator = RequestValidator.Current;
						int validationFailureIndex;

						foreach (string hkey in headers.AllKeys) {
							string value = headers [hkey];
							
							if (!validator.IsValidRequestString (HttpContext.Current, value, RequestValidationSource.Headers, hkey, out validationFailureIndex))
								ThrowValidationException ("Headers", hkey, value);
						}
					}
				}
				
				return headers;
			}
		}

		public string HttpMethod {
			get {
				if (http_method == null){
					if (worker_request != null)
						http_method = worker_request.GetHttpVerbName ();
					else
						http_method = "GET";
				}
				return http_method;
			}
		}

		void DoFilter (byte [] buffer)
		{
			if (input_filter == null || filter == null)
				return;

			if (buffer.Length < 1024)
				buffer = new byte [1024];

			// Replace the input with the filtered input
			input_filter.BaseStream = input_stream;
			MemoryStream ms = new MemoryStream ();
			while (true) {
				int n = filter.Read (buffer, 0, buffer.Length);
				if (n <= 0)
					break;
				ms.Write (buffer, 0, n);
			}
			// From now on input_stream has the filtered input
			input_stream = new MemoryStream (ms.GetBuffer (), 0, (int) ms.Length, false, true);
		}

		const int INPUT_BUFFER_SIZE = 32*1024;

		TempFileStream GetTempStream ()
		{
			string tempdir = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
			TempFileStream f = null;
			string path;
			Random rnd = new Random ();
			int num;
			do {
				num = rnd.Next ();
				num++;
				path = System.IO.Path.Combine (tempdir, "tmp" + num.ToString("x") + ".req");

				try {
					f = new TempFileStream (path);
				} catch (SecurityException) {
					// avoid an endless loop
					throw;
				} catch { }
			} while (f == null);

			return f;
		}

		void MakeInputStream ()
		{
			if (input_stream != null)
				return;

			if (worker_request == null) {
				input_stream = new MemoryStream (new byte [0], 0, 0, false, true);
				DoFilter (new byte [1024]);
				return;
			}

			//
			// Use an unmanaged memory block as this might be a large
			// upload
			//
			int content_length = ContentLength;
			int content_length_kb = content_length / 1024;
			HttpRuntimeSection config = HttpRuntime.Section;
			if (content_length_kb > config.MaxRequestLength)
				throw HttpException.NewWithCode (400, "Upload size exceeds httpRuntime limit.", WebEventCodes.RuntimeErrorPostTooLarge);

			int total = 0;
			byte [] buffer;
			buffer = worker_request.GetPreloadedEntityBody ();
			// we check the instance field 'content_length' here, not the local var.
			if (this.content_length == 0 || worker_request.IsEntireEntityBodyIsPreloaded ()) {
				if (buffer == null || content_length == 0) {
					input_stream = new MemoryStream (new byte [0], 0, 0, false, true);
				} else {
					input_stream = new MemoryStream (buffer, 0, buffer.Length, false, true);
				}
				DoFilter (new byte [1024]);
				return;
			}

			if (buffer != null)
				total = buffer.Length;

			if (content_length > 0 && content_length_kb >= config.RequestLengthDiskThreshold) {
				// Writes the request to disk
				total = Math.Min (content_length, total);
				request_file = GetTempStream ();
				Stream output = request_file;
				if (total > 0)
					output.Write (buffer, 0, total);

				if (total < content_length) {
					buffer = new byte [Math.Min (content_length, INPUT_BUFFER_SIZE)];
					do {
						int n;
						int min = Math.Min (content_length - total, INPUT_BUFFER_SIZE);
						n = worker_request.ReadEntityBody (buffer, min);
						if (n <= 0)
							break;
						output.Write (buffer, 0, n);
						total += n;
					} while (total < content_length);
				}

				request_file.SetReadOnly ();
				input_stream = request_file;
			} else if (content_length > 0) {
				// Buffers the request in an IntPtrStream
				total = Math.Min (content_length, total);
				IntPtr content = Marshal.AllocHGlobal (content_length);
				if (content == (IntPtr) 0)
					throw HttpException.NewWithCode (
						String.Format ("Not enough memory to allocate {0} bytes.", content_length),
						WebEventCodes.WebErrorOtherError);

				if (total > 0)
					Marshal.Copy (buffer, 0, content, total);

				if (total < content_length) {
					buffer = new byte [Math.Min (content_length, INPUT_BUFFER_SIZE)];
					do {
						int n;
						int min = Math.Min (content_length - total, INPUT_BUFFER_SIZE);
						n = worker_request.ReadEntityBody (buffer, min);
						if (n <= 0)
							break;
						Marshal.Copy (buffer, 0, (IntPtr) ((long)content + total), n);
						total += n;
					} while (total < content_length);
				}

				input_stream = new IntPtrStream (content, total);
			} else {
				// Buffers the request in a MemoryStream or writes to disk if threshold exceeded
				MemoryStream ms = new MemoryStream ();
				Stream output = ms;
				if (total > 0)
					ms.Write (buffer, 0, total);

				buffer = new byte [INPUT_BUFFER_SIZE];
				long maxlength = config.MaxRequestLength * 1024L;
				long disk_th = config.RequestLengthDiskThreshold * 1024L;
				int n;
				while (true) {
					n = worker_request.ReadEntityBody (buffer, INPUT_BUFFER_SIZE);
					if (n <= 0)
						break;
					total += n;
					if (total < 0 || total > maxlength)
						throw HttpException.NewWithCode (400, "Upload size exceeds httpRuntime limit.", WebEventCodes.RuntimeErrorPostTooLarge);

					if (ms != null && total > disk_th) {
						// Swith to on-disk file.
						request_file = GetTempStream ();
						ms.WriteTo (request_file);
						ms = null;
						output = request_file;
					}
					output.Write (buffer, 0, n);
				}

				if (ms != null) {
					input_stream = new MemoryStream (ms.GetBuffer (), 0, (int) ms.Length, false, true);
				} else {
					request_file.SetReadOnly ();
					input_stream = request_file;
				}
			}
			DoFilter (buffer);

			if (total < content_length)
				throw HttpException.NewWithCode (411, "The request body is incomplete.", WebEventCodes.WebErrorOtherError);
		}

		internal void ReleaseResources ()
		{
			Stream stream;
			if (input_stream != null){
				stream = input_stream;
				input_stream = null;
				try {
					stream.Close ();
				} catch {}
			}

			if (request_file != null) {
				stream = request_file;
				request_file = null;
				try {
					stream.Close ();
				} catch {}
			}
		}
		public RequestContext RequestContext {
			get {
				if (requestContext == null)
					requestContext = new RequestContext (new HttpContextWrapper (this.context ?? HttpContext.Current), new RouteData ());

				return requestContext;
			}
			
			internal set { requestContext = value; }	
		}

		public ChannelBinding HttpChannelBinding {
			get {
				throw new PlatformNotSupportedException ("This property is not supported.");
			}
		}

		public Stream GetBufferedInputStream ()
		{
			return input_stream;
		}

		public Stream GetBufferlessInputStream ()
		{
			if (bufferlessInputStream == null) {
				if (input_stream != null)
					throw new HttpException ("Input stream has already been created");

				// we don't need to hook up the filter here, because the raw stream should be returned
				bufferlessInputStream = new BufferlessInputStream (this);
			}

			return bufferlessInputStream;
		}

		public Stream GetBufferlessInputStream (bool disableMaxRequestLength)
		{
			return GetBufferlessInputStream ();
		}

		//
		// Stream that returns the data as it is read, without buffering
		//
		class BufferlessInputStream : Stream {
			HttpRequest request;

			// cached, the request content-length
			int content_length;

			// buffer that holds preloaded data
			byte [] preloadedBuffer;

			// indicates if we already served the whole preloaded buffer
			bool preloaded_served;

			// indicates if we already checked the request content-length against httpRuntime limit
			bool checked_maxRequestLength;

			// our stream position
			long position;

			//
			// @request: the containing request that created us, used to find out content length
			public BufferlessInputStream (HttpRequest request)
			{
				this.request = request;
				content_length = request.ContentLength;
			}

			public override bool CanRead {
				get { return true; }
			}

			public override bool CanSeek {
				get { return false; }
			}

			public override bool CanWrite {
				get { return false; }
			}

			public override long Length {
				get {
					return content_length;
				}
			}

			public override long Position {
				get {
					return position;
				}
				set {
					throw new NotSupportedException ("This is a readonly stream");
				}
			}

			public override void Flush ()
			{
			}

			public override int Read (byte [] buffer, int offset, int count)
			{
				if (buffer == null)
					throw new ArgumentNullException ("buffer");

				if (offset < 0 || count < 0)
					throw new ArgumentOutOfRangeException ("offset or count less than zero.");

				if (buffer.Length - offset < count )
					throw new ArgumentException ("offset+count",
								     "The size of the buffer is less than offset + count.");

				if (count == 0 || request.worker_request == null)
					return 0;

				if (!checked_maxRequestLength) {
					int content_length_kb = content_length / 1024;
					HttpRuntimeSection config = HttpRuntime.Section;
					if (content_length_kb > config.MaxRequestLength)
						throw HttpException.NewWithCode (400, "Upload size exceeds httpRuntime limit.", WebEventCodes.RuntimeErrorPostTooLarge);
					else
						checked_maxRequestLength = true;
				}

				// Serve the bytes we might have preloaded already.
				if (!preloaded_served) {
					if (preloadedBuffer == null)
						preloadedBuffer = request.worker_request.GetPreloadedEntityBody ();

					if (preloadedBuffer != null) {
						long bytes_left = preloadedBuffer.Length-position;
						int n = (int) Math.Min (count, bytes_left);
						Array.Copy (preloadedBuffer, position, buffer, offset, n);
						position += n;

						if (n == bytes_left)
							preloaded_served = true;

						return n;
					}
					else
						preloaded_served = true;
				}

				// serve bytes from worker request if available
				if (position < content_length) {
					long bytes_left = content_length-position;
					int n = count;

					if (bytes_left < count)
						n = (int) bytes_left;

					int bytes_read = request.worker_request.ReadEntityBody (buffer, offset, n);
					position += bytes_read;
					return bytes_read;
				}

				return 0;
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotSupportedException ("Can not seek on the HttpRequest.BufferlessInputStream");
			}

			public override void SetLength (long value)
			{
				throw new NotSupportedException ("Can not set length on the HttpRequest.BufferlessInputStream");
			}

			public override void Write (byte [] buffer, int offset, int count)
			{
				throw new NotSupportedException ("Can not write on the HttpRequest.BufferlessInputStream");
			}

			//
			// TODO: explicitly support the async methods if there is a convenient way of doing it
			//
		}
		public Stream InputStream {
			get {
				if (input_stream == null)
					MakeInputStream ();

				return input_stream;
			}
		}

		public bool IsAuthenticated {
			get {
				if (context.User == null || context.User.Identity == null)
					return false;
				return context.User.Identity.IsAuthenticated;
			}
		}

		public bool IsSecureConnection {
			get {
				if (worker_request == null)
					return false;
				return worker_request.IsSecure ();
			}
		}

		public string this [string key] {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				// "The QueryString, Form, Cookies, or ServerVariables collection member
				// specified in the key parameter."
				string val = QueryString [key];
				if (val == null)
					val = Form [key];
				if (val == null) {
					HttpCookie cookie = Cookies [key];
					if (cookie != null)
						val = cookie.Value;
				}
				if (val == null)
					val = ServerVariables [key];

				return val;
			}
		}

		public NameValueCollection Params {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				if (all_params == null)
					all_params = new HttpParamsCollection (QueryString, Form, ServerVariables, Cookies);

				return all_params;
			}
		}

		internal string PathNoValidation {
			get {
				if (original_path == null) {
					if (url_components != null)
						// use only if it's already been instantiated, so that we can't go into endless
						// recursion in some scenarios
						original_path = UrlComponents.Path;
					else
						original_path = ApplyUrlMapping (worker_request.GetUriPath ());
				}

				return original_path;
			}
		}
		
		public string Path {
			get {
				if (unescaped_path == null) {
					unescaped_path = PathNoValidation;
					if (validateRequestNewMode) {
						RequestValidator validator = RequestValidator.Current;
						int validationFailureIndex;
						
						if (!validator.IsValidRequestString (HttpContext.Current, unescaped_path, RequestValidationSource.Path, null, out validationFailureIndex))
							ThrowValidationException ("Path", "Path", unescaped_path);
					}
				}
				
				return unescaped_path;
			}
		}

        	internal string PathInfoNoValidation {
        		get {
        			if (path_info_unvalidated == null) {
					if (worker_request == null)
						return String.Empty;

					path_info_unvalidated = worker_request.GetPathInfo () ?? String.Empty;
                		}

                		return path_info_unvalidated;
        		}
        	}

		public string PathInfo {
			get {
				if (path_info == null) {
					path_info = PathInfoNoValidation;
					if (validateRequestNewMode) {
						RequestValidator validator = RequestValidator.Current;
						int validationFailureIndex;
						
						if (!validator.IsValidRequestString (HttpContext.Current, path_info, RequestValidationSource.PathInfo, null, out validationFailureIndex))
							ThrowValidationException ("PathInfo", "PathInfo", path_info);
					}
				}

				return path_info;
			}
		}

		public string PhysicalApplicationPath {
			get {
				if (worker_request == null)
					throw new ArgumentNullException (); // like 2.0, 1.x throws TypeInitializationException

				string path = HttpRuntime.AppDomainAppPath;
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public string PhysicalPath {
			get {
				if (worker_request == null)
					return String.Empty; // don't check security with an empty string!

				if (physical_path == null) {
					// Don't call HttpRequest.MapPath here, as that one *trims* the input
					physical_path = worker_request.MapPath (FilePath);
				}

				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, physical_path).Demand ();
				}
				return physical_path;
			}
		}

		internal string RootVirtualDir {
			get {
				if (root_virtual_dir == null){
					string fp = FilePath;
					int p = fp.LastIndexOf ('/');

					if (p < 1)
						root_virtual_dir = "/";
					else
						root_virtual_dir = fp.Substring (0, p);
				}
				return root_virtual_dir;
			}
		}

		internal WebROCollection QueryStringUnvalidated {
			get {
				if (query_string_nvc == null) {
					query_string_nvc = new WebROCollection ();
					string q = UrlComponents.Query;
					if (q != null) {
						if (q.Length != 0)
							q = q.Remove(0, 1);
					
						HttpUtility.ParseQueryString (q, ContentEncoding, query_string_nvc);
					}
					
					query_string_nvc.Protect();
				}

				return query_string_nvc;
			}
		}
		
		public NameValueCollection QueryString {
			get {
				NameValueCollection query_string_nvc = QueryStringUnvalidated;
				if (validateRequestNewMode && !checked_query_string) {
					if (!lazyQueryStringValidation) {
						// Setting this before calling the validator prevents
						// possible endless recursion
						checked_query_string = true;
						ValidateNameValueCollection ("QueryString", query_string_nvc, RequestValidationSource.QueryString);
					}
				} else
					if (validate_query_string && !checked_query_string) {
						// Setting this before calling the validator prevents
						// possible endless recursion
						checked_query_string = true;
						ValidateNameValueCollection ("QueryString", query_string_nvc);
					}
				
				return query_string_nvc;
			}
		}

		internal string RawUrlUnvalidated {
			get {
				if (raw_url_unvalidated == null) {
					if (worker_request != null)
						raw_url_unvalidated = worker_request.GetRawUrl ();
					else
						raw_url_unvalidated = UrlComponents.Path + UrlComponents.Query;
					
					if (raw_url_unvalidated == null)
						raw_url_unvalidated = String.Empty;
				}

				return raw_url_unvalidated;
			}
		}

		public string RawUrl {
			get {
				if (raw_url == null) {
					raw_url = RawUrlUnvalidated;
					if (validateRequestNewMode) {
						RequestValidator validator = RequestValidator.Current;
						int validationFailureIndex;

						if (!validator.IsValidRequestString (HttpContext.Current, raw_url, RequestValidationSource.RawUrl, null, out validationFailureIndex))
							ThrowValidationException ("RawUrl", "RawUrl", raw_url);
					}
				}
				
				return raw_url;
			}
		}

		//
		// "GET" or "SET"
		//
		public string RequestType {
			get {
				if (request_type == null){
					if (worker_request != null) {
						request_type = worker_request.GetHttpVerbName ();
						http_method = request_type;
					} else {
						request_type = "GET";
					}
				}
				return request_type;
			}

			set {
				request_type = value;
			}
		}

		public NameValueCollection ServerVariables {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				if (server_variables == null)
					server_variables = new ServerVariablesCollection (this);

				return server_variables;
			}
		}

		public CancellationToken TimedOutToken {
			get {
				throw new NotImplementedException ();
			}
		}

		public int TotalBytes {
			get {
				Stream ins = InputStream;
				return (int) ins.Length;
			}
		}

		public UnvalidatedRequestValues Unvalidated { 
			get {
				var vals = new UnvalidatedRequestValues ();
				
				vals.Cookies = CookiesNoValidation;
				vals.Files = Files;
				vals.Form = FormUnvalidated;
				vals.Headers = HeadersNoValidation;
				vals.Path = PathNoValidation;
				vals.PathInfo = PathInfoNoValidation;
				vals.QueryString = QueryStringUnvalidated;
				vals.RawUrl = RawUrlUnvalidated;
				vals.Url = Url;
				
				return vals;
			}
		}

		public Uri Url {
			get {
				if (cached_url == null) {
					if (orig_url == null)
						cached_url = UrlComponents.Uri;
					else
						cached_url = new Uri (orig_url);
				}

				return cached_url;			
			}
		}

		public Uri UrlReferrer {
			get {
				if (worker_request == null)
					return null;

				string hr = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderReferer);
				if (hr == null)
					return null;

				Uri uri = null;
				try {
					uri = new Uri (hr);
				} catch (UriFormatException) {}
				return uri;
			}
		}

		public string UserAgent {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderUserAgent);
			}
		}

		public string UserHostAddress {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetRemoteAddress ();
			}
		}

		public string UserHostName {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetRemoteName ();
			}
		}

		public string [] UserLanguages {
			get {
				if (worker_request == null)
					return null;

				if (user_languages == null)
					user_languages = SplitHeader (HttpWorkerRequest.HeaderAcceptLanguage);

				return user_languages;
			}
		}

		public byte [] BinaryRead (int count)
		{
			if (count < 0)
				throw new ArgumentException ("count is < 0");

			Stream s = InputStream;
			byte [] ret = new byte [count];
			if (s.Read (ret, 0, count) != count)
				throw new ArgumentException (
					String.Format ("count {0} exceeds length of available input {1}",
						count, s.Length - s.Position));
			return ret;
		}

		public int [] MapImageCoordinates (string imageFieldName)
		{
			string[] parameters = GetImageCoordinatesParameters (imageFieldName);
			if (parameters == null)
				return null;
			int [] result = new int [2];
			try {
				result [0] = Int32.Parse (parameters [0]);
				result [1] = Int32.Parse (parameters [1]);
			} catch {
				return null;
			}

			return result;
		}

		public double [] MapRawImageCoordinates (string imageFieldName)
		{
			string[] parameters = GetImageCoordinatesParameters (imageFieldName);
			if (parameters == null)
				return null;
			double [] result = new double [2];
			try {
				result [0] = Double.Parse (parameters [0]);
				result [1] = Double.Parse (parameters [1]);
			} catch {
				return null;
			}

			return result;
		}

		string [] GetImageCoordinatesParameters (string imageFieldName)
		{
			string method = HttpMethod;
			NameValueCollection coll = null;
			if (method == "HEAD" || method == "GET")
				coll = QueryString;
			else if (method == "POST")
				coll = Form;

			if (coll == null)
				return null;

			string x = coll [imageFieldName + ".x"];
			if (x == null || x == "")
				return null;

			string y = coll [imageFieldName + ".y"];
			if (y == null || y == "")
				return null;
			string[] result = new string [] { x, y };

			return result;
		}

		public string MapPath (string virtualPath)
		{
			if (worker_request == null)
				return null;

			return MapPath (virtualPath, BaseVirtualDir, true);
		}

		public string MapPath (string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			if (worker_request == null)
				throw HttpException.NewWithCode ("No HttpWorkerRequest", WebEventCodes.RuntimeErrorRequestAbort);

			if (virtualPath == null)
				virtualPath = "~";
			else {
				virtualPath = virtualPath.Trim ();
				if (virtualPath.Length == 0)
					virtualPath = "~";
			}

			if (!VirtualPathUtility.IsValidVirtualPath (virtualPath))
				throw HttpException.NewWithCode (String.Format ("'{0}' is not a valid virtual path.", virtualPath), WebEventCodes.RuntimeErrorRequestAbort);

			string appVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
			if (!VirtualPathUtility.IsRooted (virtualPath)) {
				if (StrUtils.IsNullOrEmpty (baseVirtualDir))
					baseVirtualDir = appVirtualPath;
				virtualPath = VirtualPathUtility.Combine (VirtualPathUtility.AppendTrailingSlash (baseVirtualDir), virtualPath);
				if (!VirtualPathUtility.IsAbsolute (virtualPath))
					virtualPath = VirtualPathUtility.ToAbsolute (virtualPath, false);
			} else if (!VirtualPathUtility.IsAbsolute (virtualPath))
				virtualPath = VirtualPathUtility.ToAbsolute (virtualPath, false);

			bool isAppVirtualPath = String.Compare (virtualPath, appVirtualPath, RuntimeHelpers.StringComparison) == 0;
			appVirtualPath = VirtualPathUtility.AppendTrailingSlash (appVirtualPath);
			if (!allowCrossAppMapping){
				if (!StrUtils.StartsWith (virtualPath, appVirtualPath, true))
					throw new ArgumentException ("MapPath: Mapping across applications not allowed");
				if (appVirtualPath.Length > 1 && virtualPath.Length > 1 && virtualPath [0] != '/')
					throw HttpException.NewWithCode ("MapPath: Mapping across applications not allowed", WebEventCodes.RuntimeErrorRequestAbort);
			}
			
			if (!isAppVirtualPath && !virtualPath.StartsWith (appVirtualPath, RuntimeHelpers.StringComparison))
				throw new InvalidOperationException (String.Format ("Failed to map path '{0}'", virtualPath));
			string path = worker_request.MapPath (virtualPath);
			if (virtualPath [virtualPath.Length - 1] != '/' && path [path.Length - 1] == System.IO.Path.DirectorySeparatorChar)
				path = path.TrimEnd (System.IO.Path.DirectorySeparatorChar);
			return path;
		}

		public void SaveAs (string filename, bool includeHeaders)
		{
			Stream output = new FileStream (filename, FileMode.Create);
			if (includeHeaders) {
				StringBuilder sb = new StringBuilder ();
				string version = String.Empty;
				string path = "/";
				if (worker_request != null) {
					version = worker_request.GetHttpVersion ();
					path = UrlComponents.Path;
				}
				string qs = UrlComponents.Query;

				sb.AppendFormat ("{0} {1}{2} {3}\r\n", HttpMethod, path, qs, version);
				NameValueCollection coll = Headers;
				foreach (string k in coll.AllKeys) {
					sb.Append (k);
					sb.Append (':');
					sb.Append (coll [k]);
					sb.Append ("\r\n");
				}
				sb.Append ("\r\n");
				// latin1
				byte [] bytes = Encoding.GetEncoding (28591).GetBytes (sb.ToString ());
				output.Write (bytes, 0, bytes.Length);
			}

			// More than 1 call to SaveAs works fine on MS, so we "copy" the stream
			// to keep InputStream in its state.
			Stream input = GetSubStream (InputStream);
			try {
				long len = input.Length;
				int buf_size = (int) Math.Min ((len < 0 ? 0 : len), 8192);
				byte [] data = new byte [buf_size];
				int count = 0;
				while (len > 0 && (count = input.Read (data, 0, buf_size)) > 0) {
					output.Write (data, 0, count);
					len -= count;
				}
			} finally {
				output.Flush ();
				output.Close ();
				EndSubStream (input);
			}
		}

		public void ValidateInput ()
		{
			validate_cookies = true;
			validate_query_string = true;
			validate_form = true;
			inputValidationEnabled = true;
		}
		internal void Validate ()
		{
			var cfg = HttpRuntime.Section;
			string query = UrlComponents.Query;
			
			if (query != null && query.Length > cfg.MaxQueryStringLength)
				throw new HttpException (400, "The length of the query string for this request exceeds the configured maxQueryStringLength value.");
			
			string path = PathNoValidation;
			if (path != null) {
				if (path.Length > cfg.MaxUrlLength)
					throw new HttpException (400, "The length of the URL for this request exceeds the configured maxUrlLength value.");
				
				char[] invalidChars = RequestPathInvalidCharacters;
				if (invalidChars != null) {
					int idx = path.IndexOfAny (invalidChars);
					if (idx != -1)
						throw HttpException.NewWithCode (
							String.Format ("A potentially dangerous Request.Path value was detected from the client ({0}).", path [idx]),
							WebEventCodes.RuntimeErrorValidationFailure
						);
				}
			}

			if (validateRequestNewMode)
				ValidateInput ();
		}
#region internal routines
		internal string ClientTarget {
			get {
				return client_target;
			}

			set {
				client_target = value;
			}
		}
		
		public bool IsLocal {
			get {
				string address = worker_request.GetRemoteAddress ();

				if (StrUtils.IsNullOrEmpty (address))
					return false;

				if (address == "127.0.0.1")
					return true;

				System.Net.IPAddress remoteAddr = System.Net.IPAddress.Parse (address);
				if (System.Net.IPAddress.IsLoopback (remoteAddr))
					return true;

				for (int i = 0; i < host_addresses.Length; i++)
					if (remoteAddr.Equals (host_addresses [i]))
						return true;

				return false;
			}
		}

		internal void SetFilePath (string path)
		{
			file_path = path;
			physical_path = null;
			original_path = null;
		}

		internal void SetCurrentExePath (string path)
		{
			cached_url = null;
			current_exe_path = path;
			UrlComponents.Path = path + PathInfo;
			// recreated on demand
			root_virtual_dir = null;
			base_virtual_dir = null;
			physical_path = null;
			unescaped_path = null;
			original_path = null;
		}

		internal void SetPathInfo (string pi)
		{
			cached_url = null;
			path_info = pi;
			original_path = null;

			string path = UrlComponents.Path;
			UrlComponents.Path = path + PathInfo;
		}
		internal void SetFormCollection (WebROCollection coll, bool lazyValidation)
		{
			if (coll == null)
				return;
			form = coll;
			lazyFormValidation = lazyValidation;
		}

		internal void SetQueryStringCollection (WebROCollection coll, bool lazyValidation)
		{
			if (coll == null)
				return;
			query_string_nvc = coll;
			lazyQueryStringValidation = lazyValidation;
		}
		// Headers is ReadOnly, so we need this hack for cookie-less sessions.
		internal void SetHeader (string name, string value)
		{
			WebROCollection h = (WebROCollection) Headers;
			h.Unprotect ();
			h [name] = value;
			h.Protect ();
		}

		// Notice: there is nothing raw about this querystring.
		internal string QueryStringRaw {
			get {
				UriBuilder urlComponents = UrlComponents;

				if (urlComponents == null) {
					string ret = worker_request.GetQueryString ();

					if (ret == null || ret.Length == 0)
						return String.Empty;

					if (ret [0] == '?')
						return ret;

					return "?" + ret;
				}
				
				return UrlComponents.Query;
			}

			set {
				UrlComponents.Query = value;
				cached_url = null;
				query_string_nvc = null;
			}
		}

		// Internal, dont know what it does, so flagged as public so we can see it.
		internal void SetForm (WebROCollection coll)
		{
			form = coll;
		}

		internal HttpWorkerRequest WorkerRequest {
			get {
				return worker_request;
			}
		}

		internal HttpContext Context {
			get { return context; }
			set { context = value; }
		}

		static void ValidateNameValueCollection (string name, NameValueCollection coll)
		{
			if (coll == null)
				return;
		
			foreach (string key in coll.Keys) {
				string val = coll [key];
				if (val != null && val.Length > 0 && IsInvalidString (val))
					ThrowValidationException (name, key, val);
			}
		}
		static void ValidateNameValueCollection (string name, NameValueCollection coll, RequestValidationSource source)
		{
			if (coll == null)
				return;

			RequestValidator validator = RequestValidator.Current;
			int validationFailureIndex;
			HttpContext context = HttpContext.Current;

			foreach (string key in coll.Keys) {
				string val = coll [key];
				if (val != null && val.Length > 0 && !validator.IsValidRequestString (context, val, source, key, out validationFailureIndex))
					ThrowValidationException (name, key, val);
			}
		}

		[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
		public void InsertEntityBody ()
		{
			throw new PlatformNotSupportedException ("This method is not supported.");
		}

		[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
		public void InsertEntityBody (byte[] buffer, int offset, int count)
		{
			throw new PlatformNotSupportedException ("This method is not supported.");
		}
		static void ValidateCookieCollection (HttpCookieCollection cookies)
		{
			if (cookies == null)
				return;
		
			int size = cookies.Count;
			HttpCookie cookie;
			RequestValidator validator = RequestValidator.Current;
			int validationFailureIndex;
			HttpContext context = HttpContext.Current;
			bool invalid;
			
			for (int i = 0 ; i < size ; i++) {
				cookie = cookies[i];
				if (cookie == null)
					continue;
				
				string value = cookie.Value;
				string name = cookie.Name;

				if (!String.IsNullOrEmpty (value)) {
					if (validateRequestNewMode)
						invalid = !validator.IsValidRequestString (context, value, RequestValidationSource.Cookies, name, out validationFailureIndex);
					else
						invalid = IsInvalidString (value);

					if (invalid)
						ThrowValidationException ("Cookies", name, value);
				}
			}
		}

		static void ThrowValidationException (string name, string key, string value)
		{
			string v = "\"" + value + "\"";
			if (v.Length > 20)
				v = v.Substring (0, 16) + "...\"";
		
			string msg = String.Format ("A potentially dangerous Request.{0} value was " +
						    "detected from the client ({1}={2}).", name, key, v);
		
			throw new HttpRequestValidationException (msg);
		}
		internal static void ValidateString (string key, string value, RequestValidationSource source)
		{
			if (String.IsNullOrEmpty (value))
				return;
#pragma warning disable 219
			int ignore;
#pragma warning restore 219
			if (IsInvalidString (value, out ignore))
				ThrowValidationException (source.ToString (), key, value);
		}
		internal static bool IsInvalidString (string val)
		{
#pragma warning disable 219
			int validationFailureIndex;
#pragma warning restore 219
			return IsInvalidString (val, out validationFailureIndex);
		}

		internal static bool IsInvalidString (string val, out int validationFailureIndex)
		{
			validationFailureIndex = 0;

			int len = val.Length;
			if (len < 2)
				return false;

			char current = val [0];
			for (int idx = 1; idx < len; idx++) {
				char next = val [idx];
				// See http://secunia.com/advisories/14325
				if (current == '<' || current == '\xff1c') {
					if (next == '!' || next < ' '
					    || (next >= 'a' && next <= 'z')
					    || (next >= 'A' && next <= 'Z')) {
						validationFailureIndex = idx - 1;
						return true;
					}
				} else if (current == '&' && next == '#') {
					validationFailureIndex = idx - 1;
					return true;
				}

				current = next;
			}

			return false;
		}
		
		static System.Net.IPAddress [] GetLocalHostAddresses ()
		{
			try {
				string hostName = System.Net.Dns.GetHostName ();
				System.Net.IPAddress [] ipaddr = System.Net.Dns.GetHostAddresses (hostName);
				return ipaddr;
			} catch {
				return new System.Net.IPAddress[0];
			}
		}
	}
#endregion

#region Helper classes

	//
	// Stream-based multipart handling.
	//
	// In this incarnation deals with an HttpInputStream as we are now using
	// IntPtr-based streams instead of byte [].   In the future, we will also
	// send uploads above a certain threshold into the disk (to implement
	// limit-less HttpInputFiles). 
	//
	
	class HttpMultipart {

		public class Element {
			public string ContentType;
			public string Name;
			public string Filename;
			public long Start;
			public long Length;
			
			public override string ToString ()
			{
				return "ContentType " + ContentType + ", Name " + Name + ", Filename " + Filename + ", Start " +
					Start.ToString () + ", Length " + Length.ToString ();
			}
		}
		
		Stream data;
		string boundary;
		byte [] boundary_bytes;
		byte [] buffer;
		bool at_eof;
		Encoding encoding;
		StringBuilder sb;
		
		const byte HYPHEN = (byte) '-', LF = (byte) '\n', CR = (byte) '\r';
		
		// See RFC 2046 
		// In the case of multipart entities, in which one or more different
		// sets of data are combined in a single body, a "multipart" media type
		// field must appear in the entity's header.  The body must then contain
		// one or more body parts, each preceded by a boundary delimiter line,
		// and the last one followed by a closing boundary delimiter line.
		// After its boundary delimiter line, each body part then consists of a
		// header area, a blank line, and a body area.  Thus a body part is
		// similar to an RFC 822 message in syntax, but different in meaning.
		
		public HttpMultipart (Stream data, string b, Encoding encoding)
		{
			this.data = data;
			boundary = b;
			boundary_bytes = encoding.GetBytes (b);
			buffer = new byte [boundary_bytes.Length + 2]; // CRLF or '--'
			this.encoding = encoding;
			sb = new StringBuilder ();
		}

		string ReadLine ()
		{
			// CRLF or LF are ok as line endings.
			bool got_cr = false;
			int b = 0;
			sb.Length = 0;
			while (true) {
				b = data.ReadByte ();
				if (b == -1) {
					return null;
				}

				if (b == LF) {
					break;
				}
				got_cr = (b == CR);
				sb.Append ((char) b);
			}

			if (got_cr)
				sb.Length--;

			return sb.ToString ();

		}

		static string GetContentDispositionAttribute (string l, string name)
		{
			int idx = l.IndexOf (name + "=");
			if (idx < 0)
				return null;
			int begin = idx + name.Length + "=".Length;
			int end;
			if (l.Length > begin && l [begin] == '"') {
				begin++;
				end = l.IndexOf ('"', begin);
			} else {
				end = l.IndexOf (';', begin);
				if (end == -1)
					end = l.Length;
			}
			if (end < 0)
				return null;
			if (begin == end)
				return "";
			return l.Substring (begin, end - begin);
		}

		string GetContentDispositionAttributeWithEncoding (string l, string name)
		{
			int idx = l.IndexOf (name + "=");
			if (idx < 0)
				return null;
			int begin = idx + name.Length + "=".Length;
			int end;
			if (l.Length > begin && l [begin] == '"') {
				begin++;
				end = l.IndexOf ('"', begin);
			} else {
				end = l.IndexOf (';', begin);
				if (end == -1)
					end = l.Length;
			}
			if (end < 0)
				return null;
			if (begin == end)
				return "";

			string temp = l.Substring (begin, end - begin);
			byte [] source = new byte [temp.Length];
			for (int i = temp.Length - 1; i >= 0; i--)
				source [i] = (byte) temp [i];

			return encoding.GetString (source);
		}

		bool ReadBoundary ()
		{
			try {
				string line = ReadLine ();
				while (line == "")
					line = ReadLine ();
				if (line [0] != '-' || line [1] != '-')
					return false;

				if (!StrUtils.EndsWith (line, boundary, false))
					return true;
			} catch {
			}

			return false;
		}

		string ReadHeaders ()
		{
			string s = ReadLine ();
			if (s == "")
				return null;

			return s;
		}

		bool CompareBytes (byte [] orig, byte [] other)
		{
			for (int i = orig.Length - 1; i >= 0; i--)
				if (orig [i] != other [i])
					return false;

			return true;
		}

		long MoveToNextBoundary ()
		{
			long retval = 0;
			bool got_cr = false;

			int state = 0;
			int c = data.ReadByte ();
			while (true) {
				if (c == -1)
					return -1;

				if (state == 0 && c == LF) {
					retval = data.Position - 1;
					if (got_cr)
						retval--;
					state = 1;
					c = data.ReadByte ();
				} else if (state == 0) {
					got_cr = (c == CR);
					c = data.ReadByte ();
				} else if (state == 1 && c == '-') {
					c = data.ReadByte ();
					if (c == -1)
						return -1;

					if (c != '-') {
						state = 0;
						got_cr = false;
						continue; // no ReadByte() here
					}

					int nread = data.Read (buffer, 0, buffer.Length);
					int bl = buffer.Length;
					if (nread != bl)
						return -1;

					if (!CompareBytes (boundary_bytes, buffer)) {
						state = 0;
						data.Position = retval + 2;
						if (got_cr) {
							data.Position++;
							got_cr = false;
						}
						c = data.ReadByte ();
						continue;
					}

					if (buffer [bl - 2] == '-' && buffer [bl - 1] == '-') {
						at_eof = true;
					} else if (buffer [bl - 2] != CR || buffer [bl - 1] != LF) {
						state = 0;
						data.Position = retval + 2;
						if (got_cr) {
							data.Position++;
							got_cr = false;
						}
						c = data.ReadByte ();
						continue;
					}
					data.Position = retval + 2;
					if (got_cr)
						data.Position++;
					break;
				} else {
					// state == 1
					state = 0; // no ReadByte() here
				}
			}

			return retval;
		}

		public Element ReadNextElement ()
		{
			if (at_eof || ReadBoundary ())
				return null;

			Element elem = new Element ();
			string header;
			while ((header = ReadHeaders ()) != null) {
				if (StrUtils.StartsWith (header, "Content-Disposition:", true)) {
					elem.Name = GetContentDispositionAttribute (header, "name");
					elem.Filename = StripPath (GetContentDispositionAttributeWithEncoding (header, "filename"));
				} else if (StrUtils.StartsWith (header, "Content-Type:", true)) {
					elem.ContentType = header.Substring ("Content-Type:".Length).Trim ();
				}
			}

			long start = data.Position;
			elem.Start = start;
			long pos = MoveToNextBoundary ();
			if (pos == -1)
				return null;

			elem.Length = pos - start;
			return elem;
		}

		static string StripPath (string path)
		{
			if (path == null || path.Length == 0)
				return path;
			
			if (path.IndexOf (":\\") != 1 && !path.StartsWith ("\\\\"))
				return path;
			return path.Substring (path.LastIndexOf ('\\') + 1);
		}
	}
#endregion
}

