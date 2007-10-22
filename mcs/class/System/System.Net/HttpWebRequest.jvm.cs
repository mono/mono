
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Net;

namespace System.Net
{
	[Serializable]
	public class HttpWebRequest : WebRequest
	{
		#region Fields


		private static readonly int DEFAULT_MAX_RESP_HEADER_LEN = 64;

		private static int _defaultMaxResponseHeadersLength = DEFAULT_MAX_RESP_HEADER_LEN;


		private HttpProvider _provider;

		#endregion

		#region Constructors

		internal HttpWebRequest(Uri uri)
		{
			_provider = HttpProvider.GetHttpProvider(uri);
//			Console.WriteLine("uri to string: " + uri.ToString());
		}


		#endregion

		#region Properties


		public string Accept
		{
			get{return Headers["Accept"];}
			set
			{
				if(_provider.IsRequestStarted ())
					throw new InvalidOperationException ("request started");
				_provider.Headers.RemoveAndAdd ("Accept", value);
			}
		}

		public Uri Address
		{
			get{return _provider.GetAddress();}
		}

		public bool AllowAutoRedirect
		{
			get{return _provider.AllowAutoRedirect;}
			set{_provider.AllowAutoRedirect = value;}
		}

		public bool AllowWriteStreamBuffering
		{
			get{return _provider.AllowWriteStreamBuffering;}
			set{_provider.AllowWriteStreamBuffering = value;}
		}

		[MonoTODO] //documentation related
		public X509CertificateCollection ClientCertificates
		{
			[MonoTODO]
			get{return _provider.GetX509Certificates();}
			[MonoNotSupported("")]
			set { throw new NotImplementedException (); }
		}

		public string Connection
		{
			get { return Headers["Connection"]; }
			set
			{
				if(_provider.IsRequestStarted())
					throw new InvalidOperationException ("request started");

				string val = value;
				if (val != null)
					val = val.Trim ().ToLower (CultureInfo.InvariantCulture);

				if (val == null || val.Length == 0)
				{
					Headers.RemoveInternal ("Connection");
					return;
				}

				if (val == "keep-alive" || val == "close")
					throw new ArgumentException ("Keep-Alive and Close may not be set with this property");

//				if (this.KeepAlive && val.IndexOf ("keep-alive") == -1)
//					value = value + ", Keep-Alive";

				Headers.RemoveAndAdd ("Connection", value);
			}
		}

		public override string ConnectionGroupName
		{
			get{return _provider.ConnectionGroupName;}
			set{_provider.ConnectionGroupName = value;}
		}

		public override long ContentLength
		{
			get{return _provider.ContentLength;}
			set
			{
				if(_provider.IsRequestStarted())
					throw new InvalidOperationException("Connection already opened");
				_provider.ContentLength = value;
			}
		}

		public override string ContentType
		{
			get { return Headers["Content-Type"]; }
			set
			{
				if (value == null || value.Trim().Length == 0)
				{
					Headers.RemoveInternal ("Content-Type");
					return;
				}
				Headers.RemoveAndAdd ("Content-Type", value);
			}
		}
		[MonoTODO] //needed for automatic documentation tools,
			//since currently we don't support this feature
		public HttpContinueDelegate ContinueDelegate
		{
			[MonoTODO]
			get{return _provider.ContinueDelegate;}
			[MonoTODO]
			set{_provider.ContinueDelegate = value;}
		}

		public CookieContainer CookieContainer
		{
			get{return _provider.CookieContainer;}
			set{_provider.CookieContainer = value;}
		}

		public override ICredentials Credentials
		{
			get{return _provider.Credentials;}
			set{_provider.Credentials = value;}
		}

		public static int DefaultMaximumResponseHeadersLength
		{
			get{return HttpProvider.DefaultMaxResponseHeadersLength;}
			set{HttpProvider.DefaultMaxResponseHeadersLength = value;}
		}

		public string Expect
		{
			get{return Headers["Expect"];}
			set
			{
				if(_provider.IsRequestStarted ())
					throw new InvalidOperationException("Connection already opened");
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower (CultureInfo.InvariantCulture);

				if (val == null || val.Length == 0)
				{
					Headers.RemoveInternal ("Expect");
					return;
				}

				if (val == "100-continue")
					throw new ArgumentException ("100-Continue cannot be set with this property.",
						"value");
				Headers.RemoveAndAdd ("Expect", value);
			}
		}

		public bool HaveResponse
		{
			get{return _provider.IsHaveResponse();}
		}

		public override WebHeaderCollection Headers
		{
			get{return _provider.Headers;}
			set{_provider.Headers = value;}
		}

		public DateTime IfModifiedSince
		{
			get
			{
				string str = Headers["If-Modified-Since"];
				if (str == null)
					return DateTime.Now;
				try
				{
					return MonoHttpDate.Parse (str);
				}
				catch (Exception)
				{
					return DateTime.Now;
				}
			}
			set
			{
				if(_provider.IsRequestStarted ())
					throw new InvalidOperationException("Connection already started");
				// rfc-1123 pattern
				Headers.SetInternal ("If-Modified-Since",
					value.ToUniversalTime ().ToString ("r", null));
				// TODO: check last param when using different locale
			}
		}

		public bool KeepAlive
		{
			get{return _provider.KeepAlive;}
			set{_provider.KeepAlive = value;}
		}

		public int MaximumAutomaticRedirections
		{
			get{return _provider.MaxAutoRedirections;}
			set{_provider.MaxAutoRedirections = value;}
		}

		[MonoTODO] //documentation
		public int MaximumResponseHeadersLength
		{
			[MonoTODO]
			get{return _provider.MaximumResponseHeadersLength;}
			[MonoTODO]
			set{_provider.MaximumResponseHeadersLength = value;}
		}

		public string MediaType
		{
			get{return _provider.MediaType;}
			set{_provider.MediaType = value;}
		}

		public override string Method
		{
			get{return _provider.MethodName;}
			set{_provider.MethodName = value;}
		}
		[MonoTODO] //for documentation related - limited.
		public bool Pipelined
		{
			[MonoTODO]
			get{return _provider.Pipelined;}
			[MonoTODO]
			set{_provider.Pipelined = value;}
		}

		public override bool PreAuthenticate
		{
			get{return _provider.PreAuthenticate;}
			set{_provider.PreAuthenticate = value;}
		}

		public Version ProtocolVersion
		{
			get{return _provider.ProtocolVersion;}
			set{_provider.ProtocolVersion = value;}
		}

		public override IWebProxy Proxy
		{
			get{return _provider.Proxy;}
			set{_provider.Proxy = value;}
		}

		public int ReadWriteTimeout
		{
			get{return _provider.ReadWriteTimeout;}
			set{_provider.ReadWriteTimeout = value;}
		}

		public string Referer
		{
			get {return Headers["Referer"];}
			set
			{
				if(_provider.IsRequestStarted ())
					throw new InvalidOperationException("Connection already opened");
				if (value == null || value.Trim().Length == 0)
				{
					Headers.RemoveInternal ("Referer");
					return;
				}
				Headers.SetInternal ("Referer", value);
			}
		}
		internal Uri AuthUri
		{
			get { return RequestUri; }
		}
		public override Uri RequestUri
		{
			get{return _provider.GetOriginalAddress();}
		}

		public bool SendChunked
		{
			get{return _provider.SendChunked;}
			set{_provider.SendChunked = value;}
		}

		public ServicePoint ServicePoint
		{
			get{return _provider.ServicePoint;}
		}
		[MonoTODO] //once again - needed since our impl. still
			//doesn't support this feature we need document it..
		public override int Timeout
		{
			[MonoTODO]
			get{return _provider.Timeout;}
			[MonoTODO]
			set{_provider.Timeout = value;}
		}


		public string TransferEncoding
		{
			get { return Headers ["Transfer-Encoding"]; }
			set
			{
				if(_provider.IsRequestStarted ())
				{
					throw new InvalidOperationException("Connection has been already opened");
				}
				string val = value;
				if (val != null)
					val = val.Trim ().ToLower (CultureInfo.InvariantCulture);

				if (val == null || val.Length == 0)
				{
					Headers.RemoveInternal ("Transfer-Encoding");
					return;
				}

				if (val == "chunked")
					throw new ArgumentException ("Chunked encoding must be set with the SendChunked property");

				if (!this.SendChunked)
					throw new InvalidOperationException ("SendChunked must be True");

				Headers.RemoveAndAdd ("Transfer-Encoding", value);
			}
		}


		public bool UnsafeAuthenticatedConnectionSharing
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string UserAgent
		{
			get { return Headers ["User-Agent"]; }
			set { Headers.SetInternal ("User-Agent", value); }
		}




		#endregion

		#region Methods

		//todo
		public override void Abort()
		{
			_provider.Abort();
//			_connection.disconnect();
//			_haveResponse = true;
//			//aborted = true;
//			if (_asyncWrite != null)
//			{
//				GHWebAsyncResult r = _asyncWrite;
//				WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled);
//				r.SetCompleted (false, wexc);
//				r.DoCallback ();
//				_asyncWrite = null;
//			}
//
//			if (_asyncRead != null)
//			{
//				GHWebAsyncResult r = _asyncRead;
//				WebException wexc = new WebException ("Aborted.", WebExceptionStatus.RequestCanceled);
//				r.SetCompleted (false, wexc);
//				r.DoCallback ();
//				_asyncRead = null;
//			}
//
////			if (abortHandler != null)
////			{
////				try
////				{
////					abortHandler (this, EventArgs.Empty);
////				}
////				catch {}
////				abortHandler = null;
////			}
//
//			if (_writeStream != null)
//			{
//				try
//				{
//					_writeStream.Close ();
//					_writeStream = null;
//				}
//				catch {}
//			}
//
//			if (_response != null)
//			{
//				try
//				{
//					_response.Close ();
//					_response = null;
//				}
//				catch {}
//			}
		}

		public void AddRange (int range)
		{
			AddRange ("bytes", range);
		}

		public void AddRange (int from, int to)
		{
			AddRange ("bytes", from, to);
		}

		public void AddRange (string rangeSpecifier, int range)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			string value = Headers ["Range"];
			if (value == null || value.Length == 0)
				value = rangeSpecifier + "=";
			else if (value.StartsWith (rangeSpecifier.ToLower () + "=", StringComparison.InvariantCultureIgnoreCase))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			Headers.RemoveAndAdd ("Range", value + range + "-");
		}

		public void AddRange (string rangeSpecifier, int from, int to)
		{
			if (rangeSpecifier == null)
				throw new ArgumentNullException ("rangeSpecifier");
			if (from < 0 || to < 0 || from > to)
				throw new ArgumentOutOfRangeException ();
			string value = Headers ["Range"];
			if (value == null || value.Length == 0)
				value = rangeSpecifier + "=";
			else if (value.StartsWith (rangeSpecifier.ToLower () + "=", StringComparison.InvariantCultureIgnoreCase))
				value += ",";
			else
				throw new InvalidOperationException ("rangeSpecifier");
			Headers.RemoveAndAdd ("Range", value + from + "-" + to);
		}

		public override Stream GetRequestStream()
		{
			return _provider.GetRequestStream();
//			lock(this)
//			{
//				Type t = Type.GetType("System.IO.ConsoleWriteStream", true);
//				_connection.setDoOutput(true);
//
//
////				Console.WriteLine("Request is sent with following headers:");
////				java.util.Map map = _connection.getRequestProperties();
////				for(java.util.Iterator iter = map.keySet().iterator(); iter.hasNext();)
////				{
////					string key = (string) iter.next();
////					Console.WriteLine(key + ": " + map.get(key));
////				}
//
//				foreach(string k in Headers)
//				{
//					string val = Headers[k];
//					val = (val == null) ? "" : val;
//					_connection.setRequestProperty(k, val);
//				}
//
//				_writeStream = (Stream) Activator.CreateInstance(t, new object[]{_connection.getOutputStream()});
//				_haveRequest = true;
//				return _writeStream;
//			}
		}

		public override WebResponse GetResponse()
		{
			return _provider.GetResponse();
		}
		/*
		private void CommonChecks (bool putpost)
		{
			string method = _connection.getRequestMethod();

			if (method == null)
				throw new ProtocolViolationException ("Method is null.");

			bool keepAlive = _headers["Keep-Alive"] == null;
			bool allowBuffering = true;
			bool sendChunked = true;
			long contentLength = _connection.getContentLength();

			if (putpost && ((!keepAlive || (contentLength == -1 && !sendChunked)) && !allowBuffering))
				throw new ProtocolViolationException ("Content-Length not set");

			string transferEncoding = TransferEncoding;
			if (!sendChunked && transferEncoding != null && transferEncoding.Trim () != "")
				throw new ProtocolViolationException ("SendChunked should be true.");
		}
		*/

		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			return _provider.BeginGetRequestStream(callback, state);
		}

		public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			return _provider.EndGetRequestStream(asyncResult);
		}

		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			//todo check input, http headers etc.

			return	_provider.BeginGetResponse(callback, state);
		}

		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			return _provider.EndGetResponse(asyncResult);
		}




		#endregion

		#region Inner Classes

//		#region JavaHeaders class
//		[Serializable]
//			internal sealed class JavaHeaders  : WebHeaderCollection
//		{
//			private java.net.HttpURLConnection _connection;
//
//			internal JavaHeaders(java.net.HttpURLConnection con)
//			{
//				_connection = con;
//			}
//
//			public string this[string key]
//			{
//				get
//				{
//					return _connection.getHeaderField(key);
//				}
//				set
//				{
//					_connection.addRequestProperty(key, value);
//				}
//			}
//		}
//		#endregion




		#endregion
#if NET_2_0
                public DecompressionMethods AutomaticDecompression
                {
                        get {
                                throw new NotSupportedException ();
                        }
                        set {
                                throw new NotSupportedException ();
                        }
                }
#endif

	}
}
