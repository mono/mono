//
// System.Runtime.Remoting.Channels.Http.HttpClientChannel
//
// Summary:    Implements a client channel that transmits method calls over HTTP.
//
// Classes:    public HttpClientChannel
//             internal HttpClientTransportSink
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Ahmad Tantawy (popsito82@hotmail.com)
//      Ahmad Kadry (kadrianoz@hotmail.com)
//      Hussein Mehanna (hussein_mehanna@hotmail.com)
//
// (C) 2003 Martin Willemoes Hansen
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
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Text;



namespace System.Runtime.Remoting.Channels.Http
{
	public class HttpClientChannel : BaseChannelWithProperties, IChannelSender,IChannel
	{
		// Property Keys (purposely all lower-case)
		private const String ProxyNameKey = "proxyname";
		private const String ProxyPortKey = "proxyport";

		// Settings
		private int    _channelPriority = 1;  // channel priority
		private String _channelName = "http"; // channel name

		// Proxy settings (_proxyObject gets recreated when _proxyName and _proxyPort are updated)
		//private IWebProxy _proxyObject = WebProxy.GetDefaultProxy(); // proxy object for request, can be overridden in transport sink
		private IWebProxy _proxyObject = null;
		private String    _proxyName = null;
		private int       _proxyPort = -1;

		private int _clientConnectionLimit = 0; // bump connection limit to at least this number (only meaningful if > 0)
		private bool _bUseDefaultCredentials = false; // should default credentials be used?
        
		private IClientChannelSinkProvider _sinkProvider = null; // sink chain provider

		public HttpClientChannel()
		{
			SetupProvider (null,null);
		} 
		
		public HttpClientChannel(String name, IClientChannelSinkProvider sinkProvider)
		{
			if(name != null)
				_channelName = name;
			
			SetupProvider (sinkProvider, null);
		}

		// constructor used by config file
		public HttpClientChannel(IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{	
			if (properties != null) 
			{
				foreach(DictionaryEntry Dict in properties)
				{
					switch(Dict.Key.ToString())
					{
						case "name":
							_channelName = Dict.Value.ToString();
							break;
						case "priority":
							_channelPriority = Convert.ToInt32(Dict.Value);
							break;
						case "clientConnectionLimit":
							_clientConnectionLimit = Convert.ToInt32(Dict.Value);
							break;
						case "proxyName":
							_proxyName = Dict.Value.ToString();
							break;
						case "proxyPort":
							_proxyPort = Convert.ToInt32(Dict.Value);
							break;
						case "useDefaultCredentials":
							_bUseDefaultCredentials = Convert.ToBoolean(Dict.Value);
							break;
					}
				}
			}
			
			SetupProvider (sinkProvider, properties);
		} 
        
		public int ChannelPriority
		{
			get { return _channelPriority; }    
		}

		public String ChannelName
		{
			get { return _channelName; }
		}

		// returns channelURI and places object uri into out parameter
		
		public String Parse(String url, out String objectURI)
		{            
			return HttpHelper.Parse(url,out objectURI);
		}

		//
		// end of IChannel implementation
		// 

		//
		// IChannelSender implementation
		//
		

		public virtual IMessageSink CreateMessageSink(String url, Object remoteChannelData, out String objectURI)
		{
			if (url == null && remoteChannelData != null && remoteChannelData as IChannelDataStore != null )
			{
				IChannelDataStore ds = (IChannelDataStore) remoteChannelData;
				url = ds.ChannelUris[0];
			}

			if(url != null && HttpHelper.StartsWithHttp(url))
			{
				HttpHelper.Parse(url, out objectURI);
				IMessageSink msgSink = (IMessageSink) _sinkProvider.CreateSink(this,url,remoteChannelData); 
				
				if(msgSink !=null )
					SetServicePoint(url);

				return msgSink;
			}
			else
			{
				objectURI = null;
				return null;
			}
		}

		private void UpdateProxy()
		{
			// If the user values for the proxy object are valid , then the proxy
			// object will be created based on these values , if not it'll have the
			// value given when declared , as a default proxy object
			if(_proxyName!=null && _proxyPort !=-1)
				_proxyObject = new WebProxy(_proxyName,_proxyPort);
					
			// Either it's default or not it'll have this property
			((WebProxy)_proxyObject).BypassProxyOnLocal = true;
		} 

		private void SetServicePoint(string channelURI)
		{
			// Find a ServicePoint for the given url and assign the connection limit 
			// to the user given value only if it valid
			ServicePoint sp = ServicePointManager.FindServicePoint(channelURI,ProxyObject);
			if(_clientConnectionLimit> 0)
				sp.ConnectionLimit = _clientConnectionLimit;
		}		

		internal IWebProxy ProxyObject { get { return _proxyObject; } }
		internal bool UseDefaultCredentials { get { return _bUseDefaultCredentials; } }

		private void SetupProvider (IClientChannelSinkProvider sinkProvider, IDictionary properties)
		{
			if (properties == null) properties = new Hashtable ();
			HttpClientTransportSinkProvider httpSink = new HttpClientTransportSinkProvider (properties);
			SinksWithProperties = httpSink;
			
			if(sinkProvider == null)
			{
				_sinkProvider = new SoapClientFormatterSinkProvider();
				_sinkProvider.Next = httpSink;
			}
			else
			{
				IClientChannelSinkProvider dummySinkProvider;
				dummySinkProvider = sinkProvider;
				_sinkProvider = sinkProvider;
				while(dummySinkProvider.Next != null)
				{
					dummySinkProvider = dummySinkProvider.Next;
				}

				dummySinkProvider.Next = httpSink;
			} 
		}
		
		public override object this [object key]
		{
			get { return Properties[key]; }
			set { Properties[key] = value; }
		}
		
		public override ICollection Keys 
		{
			get { return Properties.Keys; }
		}
	} 


	internal class HttpClientTransportSinkProvider : IClientChannelSinkProvider, IChannelSinkBase
	{
		IDictionary _properties;
		
		internal HttpClientTransportSinkProvider (IDictionary properties)
		{
			_properties = properties;
		}    
   
		public IClientChannelSink CreateSink(IChannelSender channel, String url, 
			Object remoteChannelData)
		{
			// url is set to the channel uri in CreateMessageSink        
			return new HttpClientTransportSink((HttpClientChannel)channel, url);
		}

		public IClientChannelSinkProvider Next
		{
			get { return null; }
			set { throw new NotSupportedException(); }
		}
		
		public IDictionary Properties
		{
			get { return _properties; }
		}
		
	} // class HttpClientTransportSinkProvider




	// transport sender sink used by HttpClientChannel
	internal class HttpClientTransportSink : BaseChannelSinkWithProperties, IClientChannelSink
	{
		private const String s_defaultVerb = "POST";

		private static String s_userAgent =
			"Mono Remoting Client (Mono CLR " + System.Environment.Version.ToString() + ")";
        
		// Property keys (purposely all lower-case)
		private const String UserNameKey = "username";
		private const String PasswordKey = "password";
		private const String DomainKey = "domain";
		private const String PreAuthenticateKey = "preauthenticate";
		private const String CredentialsKey = "credentials";
		private const String ClientCertificatesKey = "clientcertificates";
		private const String ProxyNameKey = "proxyname";
		private const String ProxyPortKey = "proxyport";
		private const String TimeoutKey = "timeout";
		private const String AllowAutoRedirectKey = "allowautoredirect";

		// If above keys get modified be sure to modify, the KeySet property on this
		// class.
		private static ICollection s_keySet = null;

		// Property values
		private String _securityUserName = null;
		private String _securityPassword = null;
		private String _securityDomain = null;
		private bool   _bSecurityPreAuthenticate = false;
		private ICredentials _credentials = null; // this overrides all of the other security settings

		private int  _timeout = System.Threading.Timeout.Infinite; // timeout value in milliseconds (only used if greater than 0)
		private bool _bAllowAutoRedirect = false;

		// Proxy settings (_proxyObject gets recreated when _proxyName and _proxyPort are updated)
		private IWebProxy _proxyObject = null; // overrides channel proxy object if non-null
		private String    _proxyName = null;
		private int       _proxyPort = -1;

		// Other members
		private HttpClientChannel _channel; // channel that created this sink
		private String            _channelURI; // complete url to remote object        

		// settings
		private bool _useChunked = false; 
//		private bool _useKeepAlive = true;

		internal HttpClientTransportSink(HttpClientChannel channel, String channelURI) : base()
		{
			string dummy;
			_channel = channel;
   			_channelURI = HttpHelper.Parse(channelURI,out dummy);
		} 
        

		public void ProcessMessage(IMessage msg,
			ITransportHeaders requestHeaders, Stream requestStream,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			string url = null;
			string uri = ((IMethodCallMessage)msg).Uri;
			requestHeaders [CommonTransportKeys.RequestUri] = uri;
			CreateUrl(uri,out url);

			HttpWebRequest httpWebRequest = CreateWebRequest(url,requestHeaders,requestStream);

			SendAndRecieve(httpWebRequest,out responseHeaders,out responseStream);
		}


		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg,
			ITransportHeaders headers, Stream stream)
		{
			string url = null;
			string uri = ((IMethodCallMessage)msg).Uri;
			headers [CommonTransportKeys.RequestUri] = uri;
			CreateUrl(uri,out url);

			HttpWebRequest httpWebRequest = CreateWebRequest(url,headers,stream);
			RequestState reqState = new RequestState(httpWebRequest,sinkStack);

			httpWebRequest.BeginGetResponse(new AsyncCallback(AsyncRequestHandler),reqState);
		}

		private void AsyncRequestHandler(IAsyncResult ar)
		{
			HttpWebResponse httpWebResponse = null;

			RequestState reqState = (RequestState) ar.AsyncState;
			HttpWebRequest httpWebRequest = reqState.webRquest;
			IClientChannelSinkStack sinkStack = reqState.sinkStack;

			try
			{
				httpWebResponse = (HttpWebResponse) httpWebRequest.EndGetResponse(ar);
			}
			catch (WebException ex)
			{
				httpWebResponse = ex.Response as HttpWebResponse;
				if (httpWebResponse == null) sinkStack.DispatchException (ex);
			}

			Stream responseStream;
			ITransportHeaders responseHeaders;

			try
			{
				ReceiveResponse (httpWebResponse, out responseHeaders, out responseStream);
				sinkStack.AsyncProcessResponse(responseHeaders,responseStream);
			}
			catch (Exception ex)
			{
				sinkStack.DispatchException (ex);
			}
		}


		public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, Object state,
			ITransportHeaders headers, Stream stream)
		{
			// We don't have to implement this since we are always last in the chain.
		} // AsyncProcessRequest


        
		public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
		{
			return null; 
		} // GetRequestStream


		public IClientChannelSink NextChannelSink
		{
			get { return null; }
		}
    

		public override Object this[Object key]
		{
			get
			{
				String keyStr = key as String;
				if (keyStr == null)
					return null;
            
				switch (keyStr.ToLower())
				{
					case UserNameKey: return _securityUserName; 
					case PasswordKey: return null; // Intentionally refuse to return password.
					case DomainKey: return _securityDomain;
					case PreAuthenticateKey: return _bSecurityPreAuthenticate; 
					case CredentialsKey: return _credentials;
					case ClientCertificatesKey: return null; // Intentionally refuse to return certificates
					case ProxyNameKey: return _proxyName; 
					case ProxyPortKey: return _proxyPort; 
					case TimeoutKey: return _timeout;
					case AllowAutoRedirectKey: return _bAllowAutoRedirect;
				} // switch (keyStr.ToLower())

				return null; 
			}
        
			set
			{
				String keyStr = key as String;
				if (keyStr == null)
					return;
    
				switch (keyStr.ToLower())
				{
					case UserNameKey: _securityUserName = (String)value; break;
					case PasswordKey: _securityPassword = (String)value; break;    
					case DomainKey: _securityDomain = (String)value; break;                
					case PreAuthenticateKey: _bSecurityPreAuthenticate = Convert.ToBoolean(value); break;
					case CredentialsKey: _credentials = (ICredentials)value; break;
					case ProxyNameKey: _proxyName = (String)value; UpdateProxy(); break;
					case ProxyPortKey: _proxyPort = Convert.ToInt32(value); UpdateProxy(); break;

					case TimeoutKey: 
					{
						if (value is TimeSpan)
							_timeout = (int)((TimeSpan)value).TotalMilliseconds;
						else
							_timeout = Convert.ToInt32(value); 
						break;
					} // case TimeoutKey

					case AllowAutoRedirectKey: _bAllowAutoRedirect = Convert.ToBoolean(value); break;
                
				} // switch (keyStr.ToLower())
			}
		} // this[]   
        
		public override ICollection Keys
		{
			get
			{
				if (s_keySet == null)
				{
					// No need for synchronization
					ArrayList keys = new ArrayList(6);
					keys.Add(UserNameKey);
					keys.Add(PasswordKey);
					keys.Add(DomainKey);
					keys.Add(PreAuthenticateKey);
					keys.Add(CredentialsKey);
					keys.Add(ClientCertificatesKey);
					keys.Add(ProxyNameKey);
					keys.Add(ProxyPortKey);
					keys.Add(TimeoutKey);
					keys.Add(AllowAutoRedirectKey);                    

					s_keySet = keys;
				}

				return s_keySet;
			}
		} 

		private void UpdateProxy()
			{
				// If the user values for the proxy object are valid , then the proxy
				// object will be created based on these values , if not it'll have the
				// value given when declared , as a default proxy object
				if(_proxyName!=null && _proxyPort !=-1)
					_proxyObject = new WebProxy(_proxyName,_proxyPort);
					
				// Either it's default or not it'll have this property
				((WebProxy)_proxyObject).BypassProxyOnLocal = true;
			} 
		
		
		internal static String UserAgent
		{
			get { return s_userAgent; }
		}

		private void CreateUrl(string uri, out string fullURL)
		{
            		if(HttpHelper.StartsWithHttp(uri)) //this is a full url
			{
				fullURL = uri;
				return;
			}

			if(_channelURI.EndsWith("/") && uri.StartsWith("/"))
			{
				fullURL = _channelURI + uri.Substring(1);
				return;
			}
			else
				if(_channelURI.EndsWith("/") && !uri.StartsWith("/") ||
				!_channelURI.EndsWith("/") && uri.StartsWith("/") )
			{
				fullURL = _channelURI  +uri;
				return;
			}
			else
			{
				fullURL = _channelURI +'/'+ uri;
				return;
			}

		}

		private HttpWebRequest CreateWebRequest(string url, ITransportHeaders requestHeaders, Stream requestStream)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);;
			request.AllowAutoRedirect = _bAllowAutoRedirect;
			request.ContentLength = requestStream.Length;
			request.Credentials = GetCredenentials();
			//request.Expect = "100-Continue";
			
			//This caused us some troubles with the HttpWebResponse class
			//maybe its fixed now. TODO
			//request.KeepAlive = _useKeepAlive;
			request.KeepAlive = false;;
			
			request.Method = s_defaultVerb;
			request.Pipelined = false;
			request.SendChunked = _useChunked;
			request.UserAgent = s_userAgent;


			// write the remoting headers			
			IEnumerator headerenum = requestHeaders.GetEnumerator();
			while (headerenum.MoveNext()) 
			{
				DictionaryEntry entry = (DictionaryEntry) headerenum.Current;
				String key = entry.Key as String;
				if(key == "Content-Type")
				{
					request.ContentType = entry.Value.ToString();
					continue;
				}

				if (key == null || key.StartsWith("__")) 
				{
					continue;
				}

				request.Headers.Add(entry.Key.ToString(),entry.Value.ToString());
			}

			Stream reqStream = request.GetRequestStream();
			if (requestStream is MemoryStream)
			{
				MemoryStream memStream = (MemoryStream)requestStream;
				reqStream.Write (memStream.GetBuffer(), 0, (int)memStream.Length);
			}
			else
				HttpHelper.CopyStream(requestStream, reqStream);

			reqStream.Close();
			
			return request;
		}       
        
		private void SendAndRecieve(HttpWebRequest httpRequest,out ITransportHeaders responseHeaders,out Stream responseStream)
		{
			responseStream = null;
			responseHeaders = null;
			HttpWebResponse httpWebResponse = null;

			try
			{
				httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();
			}
			catch (WebException ex)
			{
				httpWebResponse = ex.Response as HttpWebResponse;
				if (httpWebResponse == null || httpWebResponse.StatusCode == HttpStatusCode.InternalServerError) throw ex;
			}

			ReceiveResponse (httpWebResponse, out responseHeaders, out responseStream);
		}

		private void ReceiveResponse (HttpWebResponse httpWebResponse, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			responseHeaders = new TransportHeaders();

			try
			{
				Stream webStream = httpWebResponse.GetResponseStream();

				if (httpWebResponse.ContentLength != -1)
				{
					byte[] buffer = new byte [httpWebResponse.ContentLength];
					int nr = 0;
					while (nr < buffer.Length)
						nr += webStream.Read (buffer, nr, buffer.Length - nr);
					responseStream = new MemoryStream (buffer);
				}
				else
				{
					responseStream = new MemoryStream();
					HttpHelper.CopyStream(webStream, responseStream);
				}

				//Use the two commented lines below instead of the 3 below lines when HttpWebResponse
				//class is fully implemented in order to support custom headers
				//for(int i=0; i < httpWebResponse.Headers.Count; ++i)
				//	responseHeaders[httpWebResponse.Headers.Keys[i].ToString()] = httpWebResponse.Headers[i].ToString();

				responseHeaders["Content-Type"] = httpWebResponse.ContentType;
				responseHeaders["Server"] = httpWebResponse.Server;
				responseHeaders["Content-Length"] = httpWebResponse.ContentLength;
			}
			finally
			{
				if(httpWebResponse!=null)
					httpWebResponse.Close();
			}
		}

		private void ProcessErrorCode()
		{
		}

		private ICredentials GetCredenentials()
		{
			if(_credentials!=null)
				return _credentials;

			//Now use the username , password and domain if provided
			if(_securityUserName==null ||_securityUserName=="")
				if(_channel.UseDefaultCredentials)
					return CredentialCache.DefaultCredentials;
				else
					return null;

			return new NetworkCredential(_securityUserName,_securityPassword,_securityDomain);

		}

                
	} // class HttpClientTransportSink


	internal class RequestState
	{
		public HttpWebRequest webRquest;
		public IClientChannelSinkStack sinkStack;

		public RequestState(HttpWebRequest wr,IClientChannelSinkStack ss)
		{
			webRquest = wr;
			sinkStack = ss;
		}
	}


} // namespace System.Runtime.Remoting.Channels.Http
