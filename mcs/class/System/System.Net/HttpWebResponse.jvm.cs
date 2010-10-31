using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace System.Net
{
	[Serializable]
	public class HttpWebResponse : WebResponse,  IDisposable
	{
		#region Fields
		private mainsoft.apache.commons.httpclient.HttpMethod _httpMethod;
		private CookieCollection _cookies;
		private WebHeaderCollection _headers;
		private string _statusDescription;
		private Version _version = null;
		private Uri _uri;
		private string _method;
		private Stream _responseStream;
		private mainsoft.apache.commons.httpclient.HttpState _state;
		private HttpStateCache _stateCache;
		private HttpStatusCode _statusCode;
		private bool _isStatusCodeInitialized = false;

		private Type _httpStatusCodeType = typeof(HttpStatusCode);

		private sbyte [] _responseBody;
		private bool _isHttpMethodClosed = false;
		#endregion

		#region Constructors

		internal HttpWebResponse(mainsoft.apache.commons.httpclient.HttpMethod httpMethod,
			mainsoft.apache.commons.httpclient.HttpState state,
			HttpStateCache stateCache,
			Uri uri, string method)
		{
			_httpMethod = httpMethod;
			_uri = uri;
			_method = method;
			_state = state;
			_stateCache = stateCache;
		}

		#endregion

		#region Properties

		public CookieCollection Cookies
		{
			get
			{
				if(_cookies == null)
				{
					_cookies = new CookieCollection();
					FillCookies();

				}
				return _cookies;
			}

			set
			{
				_cookies = value;
			}
		}

		public override WebHeaderCollection Headers
		{
			get
			{
				if(_headers == null)
				{
					_headers = new WebHeaderCollection();
					FillHeaders();
				}
				return _headers;
			}
		}
		public override long ContentLength
		{
			get
			{
				string val = Headers["Content-Length"];
				if(val == null || val.Trim().Equals(""))
					return -1L;
				try
				{
					return Int64.Parse(val);
				}
				catch
				{
					return -1L;
				}
			}
		}


		public string ContentEncoding
		{
			get
			{
				return Headers["Content-Encoding"];
			}
		}

		public override string ContentType
		{
			get
			{
				return	Headers["Content-Type"];
			}
		}

		public string CharacterSet
		{
			get
			{
				string contentType = ContentType;
				if (contentType == null)
					return "ISO-8859-1";
				int pos = contentType.IndexOf ("charset=", StringComparison.OrdinalIgnoreCase);
				if (pos == -1)
					return "ISO-8859-1";
				pos += 8;
				int pos2 = contentType.IndexOf (';', pos);
				return (pos2 == -1)
					? contentType.Substring (pos) 
					: contentType.Substring (pos, pos2 - pos);
			}
		}

		public string Server
		{
			get
			{
				return Headers ["Server"];
			}
		}

		public DateTime LastModified
		{
			get
			{
				try
				{
					string val =  Headers["Last-Modified"];
					return MonoHttpDate.Parse(val);
				}
				catch
				{
					return DateTime.Now;
				}
			}
		}

		public HttpStatusCode StatusCode
		{
			get
			{
				if(_isStatusCodeInitialized == false)
				{
					int status = _httpMethod.getStatusCode();
					_statusCode = (HttpStatusCode)Enum.Parse(_httpStatusCodeType, 
						Enum.GetName(_httpStatusCodeType, status));
					_isStatusCodeInitialized = true;
				}

				return _statusCode;
			}
		}

		public string StatusDescription
		{
			get
			{
				if(_statusDescription == null)
					_statusDescription = _httpMethod.getStatusText();
				return _statusDescription;
			}
		}

		public Version ProtocolVersion
		{
			get
			{
				if(_version == null)
					ParseVersion();
				return _version;
			}
		}

		public override Uri ResponseUri
		{
			get
			{
				return _uri;
			}
		}

		public string Method
		{
			get
			{
				return _method;
			}
		}
		#endregion

		#region Methods

		internal void ReadAllAndClose()
		{
			if (_responseBody != null)
				return;

			object temp = null;
			if(_cookies == null)
			{
                temp = this.Cookies;
			}
			if(_headers == null)
			{
				temp = this.Headers;
			}
			if(_isStatusCodeInitialized == false)
			{
				temp = this.StatusCode;
			}
			if(_statusDescription == null)
			{
				temp = this.StatusDescription;
			}
			if(_version == null)
			{
				temp = this.ProtocolVersion;
			}

			_responseBody = _httpMethod.getResponseBody();
#if DEBUG
			Console.WriteLine("The response body as string == {0}", System.Text.Encoding.UTF8.GetString((byte[])vmw.common.TypeUtils.ToByteArray(_responseBody)));
#endif
			this.Close();
		}

		public override Stream GetResponseStream()
		{
			try
			{
				if(_responseStream == null)
				{
					Type t = Type.GetType("System.IO.ConsoleReadStream", true);
					object [] param = null;
					if (_responseBody == null)
					{
						param = new object[]{_httpMethod.getResponseBodyAsStream()};
					}
					else
					{
						param = new object[]{new java.io.ByteArrayInputStream(_responseBody)};
					}
					_responseStream = (Stream) Activator.CreateInstance(t, param);
				}
				return _responseStream;
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception caught!");
				Console.WriteLine(e.GetType() + ":" + e.Message + "\n" + e.StackTrace);
				throw e;
			}
		}

		public override void Close()
		{
			try
			{
				if(_responseStream != null)
					_responseStream.Close();
			}
			finally
			{
				_responseStream = null;

				if (!_isHttpMethodClosed)
				{
					_httpMethod.releaseConnection();
					if(_stateCache != null && _state != null)
					{
						_stateCache.ReleaseHttpState(_state);
						_state = null;
						_stateCache = null;
					}
					_isHttpMethodClosed = true;
				}
			}
		}

		public string GetResponseHeader(string headerName)
		{
			return Headers[headerName];
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}


		protected virtual void Dispose(bool disposing)
		{
			try
			{
				if(_responseStream != null)
					_responseStream.Close();
			}
			finally
			{
				_responseStream = null;

				if (!_isHttpMethodClosed)
				{
					_httpMethod.releaseConnection();
					_isHttpMethodClosed = true;
				}
			}
		}

		private void FillHeaders()
		{
			mainsoft.apache.commons.httpclient.Header[] respHeaders =
				_httpMethod.getResponseHeaders();
			if(respHeaders == null)
				return;
			for(int i = 0; i < respHeaders.Length; i++)
			{
				
				mainsoft.apache.commons.httpclient.HeaderElement[] elements = respHeaders[i].getElements();	
				for(int j = 0; j < elements.Length; j++)
				{
					string key = elements[j].getName();
					string val = elements[j].getValue();
					string pair = (key == null) ? ((val == null) ? "" : val) : ((val==null) ? key : key + "=" + val);
					_headers.Add(respHeaders[i].getName(), pair);							
				}

			}
			ParseVersion();
		}

		private void ParseVersion()
		{
			mainsoft.apache.commons.httpclient.StatusLine statusLine =
				_httpMethod.getStatusLine();
			string ver = statusLine.getHttpVersion().Trim().ToUpper();
			if(ver == "HTTP/1.1")
			       _version = HttpVersion.Version11;
			else if(ver == "HTTP/1.0")
				_version = HttpVersion.Version10;
			else
				_version = null;
		}

		private mainsoft.apache.commons.httpclient.Cookie FindCookie (mainsoft.apache.commons.httpclient.Cookie [] cookies, string name) {
			for (int i = 0; i < cookies.Length; ++i)
				if (cookies [i].getName () == name)
					return cookies [i];
			return null;
		}

		private mainsoft.apache.commons.httpclient.Cookie [] FetchResponseCookies (mainsoft.apache.commons.httpclient.Header [] headers,
																				mainsoft.apache.commons.httpclient.Cookie [] stateCookies) {
			System.Collections.ArrayList list = new System.Collections.ArrayList();
			foreach (mainsoft.apache.commons.httpclient.Header h in headers) {
				foreach (mainsoft.apache.commons.httpclient.HeaderElement element in h.getValues ()) {
					mainsoft.apache.commons.httpclient.Cookie c = FindCookie (stateCookies, element.getName ());
					if (c != null)
						list.Add(c);
				}
			}

			return (mainsoft.apache.commons.httpclient.Cookie[]) list.ToArray(typeof(mainsoft.apache.commons.httpclient.Cookie));
		}

		private void FillCookies ()
		{
			if(_state == null)
				return;

			mainsoft.apache.commons.httpclient.Cookie[] javaCookies =
				_state.getCookies();

			if(javaCookies == null)
				return;

			mainsoft.apache.commons.httpclient.Header [] headers = _httpMethod.getResponseHeaders ("Set-Cookie");
			if (headers != null)
				javaCookies = FetchResponseCookies (headers, javaCookies);						

			for(int i = 0; i < javaCookies.Length; i++)
			{
				bool httpsProtocol = _httpMethod.getURI().ToString().StartsWith("https");
				if(!httpsProtocol && javaCookies[i].getSecure())
					continue;
				Cookie c = new Cookie(javaCookies[i].getName(), 
					javaCookies[i].getValue(), 
					(javaCookies[i].getPath() == null) ? "" : javaCookies[i].getPath(),
					(javaCookies[i].getDomain() == null) ? "" : javaCookies[i].getDomain());
				java.util.Calendar jCalendar = java.util.Calendar.getInstance();
				java.util.Date jDate = javaCookies[i].getExpiryDate();
				if(jDate != null)
				{
					jCalendar.setTime(javaCookies[i].getExpiryDate());
					c.Expires = (DateTime) vmw.common.DateTimeUtils.CalendarToDateTime(jCalendar);
				}
				
				_cookies.Add(c);
			}

		}

		//todo remove unused methods
		private void FillCookies_old ()
		{
			
			string val = Headers["Set-Cookie"];
			if (val != null && val.Trim () != "")
				SetCookie (val);

			val = Headers["Set-Cookie2"];
			if (val != null && val.Trim () != "")
				SetCookie2 (val);
		}

		static string [] SplitValue (string input)
		{
			string [] result = new string [2];
			int eq = input.IndexOf ('=');
			if (eq == -1) 
			{
				result [0] = "invalid";
			} 
			else 
			{
				result [0] = input.Substring (0, eq).Trim ().ToUpper ();
				result [1] = input.Substring (eq + 1);
			}
			
			return result;
		}

		private void SetCookie(string val)
		{
//			Console.WriteLine("in set cookie 1 - got value : " + val);
			string[] parts = null;
			Collections.Queue options = null;
			Cookie cookie = null;

			options = new Collections.Queue (val.Split (';'));
			parts = SplitValue ((string) options.Dequeue()); // NAME=VALUE must be first

			cookie = new Cookie (parts[0], parts[1]);

			while (options.Count > 0) 
			{
				parts = SplitValue ((string) options.Dequeue());
				switch (parts [0]) 
				{
					case "COMMENT":
						if (cookie.Comment == null)
							cookie.Comment = parts[1];
						break;
					case "COMMENTURL":
						if (cookie.CommentUri == null)
							cookie.CommentUri = new Uri(parts[1]);
						break;
					case "DISCARD":
						cookie.Discard = true;
						break;
					case "DOMAIN":
						if (cookie.Domain == "")
							cookie.Domain = parts[1];
						break;
					case "MAX-AGE": // RFC Style Set-Cookie2
						if (cookie.Expires == DateTime.MinValue)
							cookie.Expires = cookie.TimeStamp.AddSeconds (Int32.Parse (parts[1]));
						break;
					case "EXPIRES": // Netscape Style Set-Cookie
						if (cookie.Expires == DateTime.MinValue) 
						{
							//FIXME: Does DateTime parse something like: "Sun, 17-Jan-2038 19:14:07 GMT"?
							//cookie.Expires = DateTime.ParseExact (parts[1]);
							cookie.Expires = DateTime.Now.AddDays (1);
						}
						break;
					case "PATH":
						cookie.Path = parts[1];
						break;
					case "PORT":
						if (cookie.Port == null)
							cookie.Port = parts[1];
						break;
					case "SECURE":
						cookie.Secure = true;
						break;
					case "VERSION":
						cookie.Version = Int32.Parse (parts[1]);
						break;
				} // switch
			} // while

			if (_cookies == null)
				_cookies = new CookieCollection();

			if (cookie.Domain == "")
				cookie.Domain = _uri.Host;

//			Console.WriteLine("adding cookie " + cookie + " to collection");
			_cookies.Add (cookie);
//			Console.WriteLine("exit from method...");
		}		
		
		private void SetCookie2 (string cookies_str)
		{
			string [] cookies = cookies_str.Split (',');
			foreach (string cookie_str in cookies)
				SetCookie (cookie_str);
		}
		#endregion
	}
}
