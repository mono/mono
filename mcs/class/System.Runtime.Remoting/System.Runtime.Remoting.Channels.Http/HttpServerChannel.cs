//
// System.Runtime.Remoting.Channels.Http.HttpServerChannel
//
// Summary:    Implements a client channel that transmits method calls over HTTP.
//
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
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.MetadataServices;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;


namespace System.Runtime.Remoting.Channels.Http
{

	public class HttpServerChannel : BaseChannelWithProperties, IChannel,
		IChannelReceiver, IChannelReceiverHook
	{
		private int               _channelPriority = 1;  // priority of channel (default=1)
		private String            _channelName = "http"; // channel name
		private String            _machineName = null;   // machine name
		private int               _port = 0;            // port to listen on
		private ChannelDataStore  _channelData = null;   // channel data
		
		private bool _bUseIpAddress = true; // by default, we'll use the ip address.
		private IPAddress _bindToAddr = IPAddress.Any; // address to bind to.
		private bool _bSuppressChannelData = false; // should we hand out null for our channel data
        
		private IServerChannelSinkProvider _sinkProvider = null;
		private HttpServerTransportSink    _transportSink = null;

		private bool _wantsToListen = true;
        
        
		private TcpListener _tcpListener;
		private Thread      _listenerThread;
		private bool        _bListening = false; // are we listening at the moment?
		//   to start listening, that will get set here.
		private AutoResetEvent  _waitForStartListening = new AutoResetEvent(false);

		public HttpServerChannel() : base()
		{
			SetupChannel(null,null);
		}

		public HttpServerChannel(int port) : base()
		{
			_port = port;
			SetupChannel(null,null);
		} 
		
		public HttpServerChannel(String name, int port) : base()
		{
            _channelName = name;
			_port = port;
			_wantsToListen = false;
			SetupChannel(null,null);
		} 

		public HttpServerChannel(String name, int port, IServerChannelSinkProvider sinkProvider) : base()
		{
			//enter the name later ya gameeel
			
			_port = port;
			_wantsToListen = false;
			SetupChannel(sinkProvider,null);
		} 

		public HttpServerChannel (IDictionary properties, IServerChannelSinkProvider sinkProvider) : base()
		{
			if (properties != null)
			foreach(DictionaryEntry Dict in properties)
			{
				switch((string)Dict.Key)
				{
					case "name":
						_channelName = (string)Dict.Value;
						break;
					case "priority":
						_channelPriority = Convert.ToInt32 (Dict.Value);
						break;
					case "bindTo": 
						_bindToAddr = IPAddress.Parse ((string)Dict.Value); 
						break;
					case "listen": 
						_wantsToListen = Convert.ToBoolean (Dict.Value);; 
						break; 
					case "machineName": 
						_machineName = (string)Dict.Value; 
						break; 
					case "port": 
						_wantsToListen = false;
						_port = Convert.ToInt32 (Dict.Value);
						break;
					case "suppressChannelData": 
						_bSuppressChannelData = Convert.ToBoolean (Dict.Value); 
						break;
					case "useIpAddress": 
						_bUseIpAddress = Convert.ToBoolean (Dict.Value); 
						break;
				}
			}

			SetupChannel (sinkProvider, properties);
		} 


		void SetupChannel (IServerChannelSinkProvider sinkProvider, IDictionary properties)
		{
			if (properties == null) properties = new Hashtable ();
			
			SetupMachineName();
			
			_sinkProvider = sinkProvider;
			
			String[] urls = { this.GetChannelUri() };

			// needed for CAOs
			_channelData = new ChannelDataStore(urls);
			
			if(_sinkProvider == null)
			{
				_sinkProvider = new SdlChannelSinkProvider();
				_sinkProvider.Next = new SoapServerFormatterSinkProvider();
			}

			// collect channel data from all providers
			IServerChannelSinkProvider provider = _sinkProvider;
			while (provider != null) 
			{
				provider.GetChannelData(_channelData);
				provider = provider.Next;
			}			

			// create the sink chain
			IServerChannelSink snk = 
				ChannelServices.CreateServerChannelSinkChain(_sinkProvider,this);

			_transportSink = new HttpServerTransportSink (snk, properties);
			SinksWithProperties = _transportSink;
		}

		internal void Listen()
		{
			while(true)
			{
				Socket socket = _tcpListener.AcceptSocket();
				RequestArguments reqArg = new RequestArguments (socket, _transportSink);
				ThreadPool.QueueUserWorkItem (new WaitCallback (HttpServer.ProcessRequest), reqArg);
			}

		} 

		public void StartListening (Object data)
		{
			_tcpListener = new TcpListener (_bindToAddr, _port);
			
			if(!_bListening)
				_tcpListener.Start();

			if (_port == 0) {
				_port = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
				String[] uris = { this.GetChannelUri() };
				_channelData.ChannelUris = uris;
			}
				
			if(_listenerThread == null)
			{
				ThreadStart t = new ThreadStart(this.Listen);
				_listenerThread = new Thread(t);
				_listenerThread.IsBackground = true;
			}
			
			if(!_listenerThread.IsAlive)
				_listenerThread.Start();
			
			_bListening = true;
		} 

		public void StopListening(Object data)
		{
			if( _bListening)
			{
				_listenerThread.Abort ();
				_tcpListener.Stop();
			}

			_bListening = false;
		}

		
		void SetupMachineName()
		{
			if (_machineName == null)
			{
				if (_bUseIpAddress) {
					IPHostEntry he = Dns.Resolve (Dns.GetHostName());
					if (he.AddressList.Length == 0) throw new RemotingException ("IP address could not be determined for this host");
					_machineName = he.AddressList [0].ToString ();
				}
				else
					_machineName = Dns.GetHostByName(Dns.GetHostName()).HostName;
			}
			
		} // SetupMachineName


		public int ChannelPriority
		{
			get { return _channelPriority; }
		}

		public String ChannelName
		{
			get { return _channelName; }
		}
	
		public String GetChannelUri()
		{
			return "http://" + _machineName + ":" + _port;
		} 
	
		public virtual String[] GetUrlsForUri(String objectUri)
		{
			String[] retVal = new String[1];

			if (!objectUri.StartsWith("/"))
				objectUri = "/" + objectUri;
			retVal[0] = GetChannelUri() + objectUri;
			return retVal;

		}

		public String Parse(String url,out String objectURI)
		{   
			return HttpHelper.Parse(url,out objectURI);
		} 
		
		public Object ChannelData
		{
			get
			{
				if (_bSuppressChannelData) return null;
				else return _channelData;
			}
		}

		public String ChannelScheme 
		{
			get { return "http"; } 
		}

		public bool WantsToListen 
		{ 
			get { return _wantsToListen; } 
			set { _wantsToListen = value; }
		} 
		
		public IServerChannelSink ChannelSinkChain 
		{ 
			get { return _transportSink.NextChannelSink; } 
		}
		
		public void AddHookChannelUri (String channelUri)
		{
			string [] uris = _channelData.ChannelUris;
			
			string [] newUris = new string[1] { channelUri };
			_channelData.ChannelUris = newUris;
			_wantsToListen = false;
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

	} // HttpServerChannel


	internal class HttpServerTransportSink : IServerChannelSink, IChannelSinkBase
	{
		private static String s_serverHeader =
			"mono .NET Remoting, mono .NET CLR " + System.Environment.Version.ToString();
    
		// sink state
		private IServerChannelSink _nextSink;
		private IDictionary _properties;
        

		public HttpServerTransportSink (IServerChannelSink nextSink, IDictionary properties)
		{
			_nextSink = nextSink;
			_properties = properties;

		} // IServerChannelSink

		internal void ServiceRequest (RequestArguments reqArg, Stream requestStream, ITransportHeaders headers)
		{          
			ITransportHeaders responseHeaders;
			Stream responseStream;

			ServerProcessing processing;
			try
			{
				processing = DispatchRequest (requestStream, headers, out responseStream, out responseHeaders);

				switch (processing)
				{                    
					case ServerProcessing.Complete:
						HttpServer.SendResponse (reqArg, 200, responseHeaders, responseStream);
						break;

					case ServerProcessing.OneWay:				
						HttpServer.SendResponse (reqArg, 200, null, null);
						break;

					case ServerProcessing.Async:
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
			}
		}

		internal ServerProcessing DispatchRequest (Stream requestStream, ITransportHeaders headers, out Stream responseStream, out ITransportHeaders responseHeaders)
		{          
			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();

			IMessage responseMessage;

			return _nextSink.ProcessMessage (sinkStack, null, headers, requestStream,
						out responseMessage,
						out responseHeaders, out responseStream);
		}
		
		//
		// IServerChannelSink implementation
		//

		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
			IMessage requestMsg,
			ITransportHeaders requestHeaders, Stream requestStream,
			out IMessage responseMsg, out ITransportHeaders responseHeaders,
			out Stream responseStream)
		{
		
			throw new NotSupportedException();
		
		} // ProcessMessage
           

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, Object state,
			IMessage msg, ITransportHeaders headers, Stream stream)                 
		{
			// Never called
			throw new NotSupportedException ();
		}


		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, Object state,
			IMessage msg, ITransportHeaders headers)
		{
			return null;
		} // GetResponseStream


		public IServerChannelSink NextChannelSink
		{
			get { return _nextSink; }
		}


		public IDictionary Properties
		{
			get { return _properties; }
		} // Properties
        

		internal static String ServerHeader
		{
			get { return s_serverHeader; }
		}
	} // HttpServerTransportSink


} // namespace System.Runtime.Remoting.Channels.Http
