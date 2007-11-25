using System;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace System.Net
{
	[Serializable]
	internal abstract class HttpProvider
	{
		#region Fields

		protected static int _defaultMaxResponseHeadersLength;
		protected static int _defaultMaxRedirectsNum = 50;
		
		protected Uri _originalUri;
		protected WebHeaderCollection _headers;
		protected bool _allowAutoRedirect;
		protected bool _allowWriteStreamBuffering = true;
		protected X509CertificateCollection _certificates;
		protected string _connectionGroupName;
		protected HttpContinueDelegate _continueDelegate;
		protected CookieContainer _cookieContainer;
		protected ICredentials _credentials;
		protected bool _keepAlive = true;
		protected int _maxResponseHeadersLength = _defaultMaxResponseHeadersLength;
		protected int _maxAutoRedirections = _defaultMaxRedirectsNum;
		protected int _readWriteTimeout = 300000;
		protected string _mediaType = string.Empty;
		protected string _methodName = "GET";
		protected bool _pipelined = true;
		protected bool _preAuthenticate;
		protected Version _version = HttpVersion.Version11;
		protected IWebProxy _proxy;
		protected bool _sendChunked;
		protected ServicePoint _servicePoint;
		protected int _timeout = 100000;

		protected bool _isAborted;
		protected long _contentLength = -1L;

		
		

		#endregion /* Fields */

		#region Constructors and Factory Methods
		protected HttpProvider(Uri uri)
		{
			_originalUri = uri;
			_headers = new WebHeaderCollection(true);
			_allowAutoRedirect = true;
			_proxy = GlobalProxySelection.Select;
		}

		public static HttpProvider GetHttpProvider(Uri uri)
		{
			return new VMWHttpProvider(uri);
		}

		public static HttpProvider  GetHttpProvider(string provider, Uri uri)
		{
			Type type = Type.GetType(provider, true);
			if(type != null)
				return GetHttpProvider(type, uri);
			//log it as an error
			return new VMWHttpProvider(uri);
		}

		public static HttpProvider GetHttpProvider(Type provider, Uri uri)
		{
			try
			{
				return (HttpProvider)Activator.CreateInstance(provider, 
					new object[]{uri});			
			}
			catch
			{
				//log it as an error
				return new VMWHttpProvider(uri);
			}
		}

		#endregion

		#region Properties
		internal virtual WebHeaderCollection Headers
		{
			get{return _headers;}
			set
			{
				if(IsRequestStarted ())
					throw new InvalidOperationException("Connection already opened");
				WebHeaderCollection newHeaders = new WebHeaderCollection (true);
				int count = value.Count;
				for (int i = 0; i < count; i++) 
					newHeaders.Add (value.GetKey (i), value.Get (i));

				_headers = newHeaders;
			}
		}

		internal virtual bool AllowAutoRedirect
		{
			get{return _allowAutoRedirect;}
			set{_allowAutoRedirect = value;}
		}

		internal virtual bool AllowWriteStreamBuffering
		{
			get{return _allowWriteStreamBuffering;}
			set{_allowWriteStreamBuffering = value;}
		}

		internal virtual string ConnectionGroupName
		{
			get{return _connectionGroupName;}
			set{_connectionGroupName = value;}
		}

		internal virtual HttpContinueDelegate ContinueDelegate
		{
			get{return _continueDelegate;}
			set{_continueDelegate = value;}
		}

		internal virtual CookieContainer CookieContainer
		{
			get{return _cookieContainer;}
			set{_cookieContainer = value;}
		}

		internal virtual ICredentials Credentials
		{
			get{return _credentials;}
			set{_credentials = value;}
		}
		internal static int DefaultMaxResponseHeadersLength
		{
			get{return _defaultMaxResponseHeadersLength;}
			set
			{
				if (value < 0 && value != -1)
					throw new ArgumentOutOfRangeException("Argument should be positive");
				_defaultMaxResponseHeadersLength = value;
			}
		}

		internal virtual bool KeepAlive
		{
			get{return _keepAlive;}
			set{_keepAlive = value;}
		}

		internal virtual int MaxAutoRedirections
		{
			get{return _maxAutoRedirections;}
			set
			{
				if (value <= 0)
					throw new ArgumentException("Must be > 0", "value");
				_maxAutoRedirections = value;
			}
		}

		internal virtual int MaximumResponseHeadersLength
		{
			get{return _maxResponseHeadersLength;}
			set
			{
				if (IsRequestStarted())
				{
					throw new InvalidOperationException("Request has been already submitted.");
				}
				if (value < 0 && value != -1)
					throw new ArgumentOutOfRangeException("The argument must be positive or -1");
				_maxResponseHeadersLength = value;
			}
		}
		
		internal virtual string MediaType
		{
			get{return _mediaType;}
			set{_mediaType = value;}
		}

		internal virtual string MethodName
		{
			get{return _methodName;}
			set
			{
				if (value == null || value.Trim () == "")
					throw new ArgumentException ("not a valid method");

				_methodName = value;
			}
		}
		internal virtual bool Pipelined
		{
			get{return _pipelined;}
			set{_pipelined = value;}
		}

		internal virtual bool PreAuthenticate 
		{ 
			get { return _preAuthenticate; }
			set { _preAuthenticate = value; }
		}

		internal virtual Version ProtocolVersion
		{
			get{return _version;}
			set
			{
				if (value != HttpVersion.Version10 && value != HttpVersion.Version11)
					throw new ArgumentException ("value");

				_version = value; 
			}
		}
		internal virtual IWebProxy Proxy
		{
			get{return _proxy;}
			set
			{
				if(IsRequestStarted())
					throw new InvalidOperationException("Request already has been submitted");
				if(value == null)
					throw new ArgumentNullException("value");
				if(!(value is WebProxy))
					throw new NotImplementedException("The supported proxy objects only of type System.Net.WebProxy");
				_proxy = value;
			}
		}
		internal virtual int ReadWriteTimeout
		{
			get{return _readWriteTimeout;}
			set
			{
				if (IsRequestStarted())
					throw new InvalidOperationException("Request has been submitted.");

				if (value < 0 && value != -1)
					throw new ArgumentOutOfRangeException("value");

				_readWriteTimeout = value;
			}
		}

		internal virtual bool SendChunked
		{
			get{return _sendChunked;}
			set
			{
				if(IsRequestStarted ())
					throw new InvalidOperationException("Request has been submitted.");
				_sendChunked = value;
			}
		}

		internal virtual ServicePoint ServicePoint
		{
			get{return _servicePoint;}
		}

		internal virtual int Timeout
		{
			get{return _timeout;}
			set
			{
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				_timeout = value;
			}
		}

		internal virtual long ContentLength
		{
			get{return _contentLength;}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException("value", "The Content-Length property value must be positive");
				_contentLength = value;
			}
		}



		#endregion

		#region Methods

		public virtual Uri GetOriginalAddress()
		{
			return _originalUri;
		}

		public virtual X509CertificateCollection GetX509Certificates()
		{
			if(_certificates == null)
				_certificates = new X509CertificateCollection();
			return _certificates;
		}

		public abstract bool IsRequestStarted();

		public abstract Uri GetAddress();

		public abstract bool IsHaveResponse();

		public abstract void Abort();

		public abstract Stream GetRequestStream();

		public abstract WebResponse GetResponse();

		public abstract IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state);

		public abstract Stream EndGetRequestStream(IAsyncResult asyncResult);

		public abstract IAsyncResult BeginGetResponse(AsyncCallback callback, object state);

		public abstract WebResponse EndGetResponse(IAsyncResult asyncResult);


		#endregion


	}
}
