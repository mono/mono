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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;


namespace System.Runtime.Remoting.Channels.Http
{


	public class HttpServerChannel : IChannel,
		IChannelReceiver, IChannelReceiverHook
	{
		private int                 _channelPriority = 1;  // priority of channel (default=1)
		private String            _channelName = "http"; // channel name
		private String            _machineName = null;   // machine name
		private int               _port = -1;            // port to listen on
		private ChannelDataStore  _channelData = null;   // channel data
		
		private String  _forcedMachineName = null; // an explicitly configured machine name
		private bool _bUseIpAddress = true; // by default, we'll use the ip address.
		private IPAddress _bindToAddr = IPAddress.Any; // address to bind to.
		private bool _bSuppressChannelData = false; // should we hand out null for our channel data
        
		private IServerChannelSinkProvider _sinkProvider = null;
		private HttpServerTransportSink    _transportSink = null;
		private IServerChannelSink         _sinkChain = null;

		private bool _wantsToListen = true;
		private bool _bHooked = false; // has anyone hooked into the channel?       
        
        
		private TcpListener _tcpListener;
		private Thread      _listenerThread;
		private bool        _bListening = false; // are we listening at the moment?
		private Exception   _startListeningException = null; // if an exception happens on the listener thread when attempting
		//   to start listening, that will get set here.
		private AutoResetEvent  _waitForStartListening = new AutoResetEvent(false);



		public HttpServerChannel() : base()
		{
			SetupChannel(null);
		
		}

		public HttpServerChannel(int port) : base()
		{
			_port = port;
			SetupChannel(null);
		
		} 
    
		
		public HttpServerChannel(String name, int port) : base()
		{
            _channelName = name;
			_port = port;
			SetupChannel(null);

		} 

		
		public HttpServerChannel(String name, int port, IServerChannelSinkProvider sinkProvider) : base()
		{
			//enter the name later ya gameeel
			
			_port = port;
			SetupChannel(sinkProvider);
	
		} 


		
		public HttpServerChannel(IDictionary properties, IServerChannelSinkProvider sinkProvider) : base()
		{      
				
			if(properties != null)
			foreach(DictionaryEntry Dict in properties)
			{
				switch((string)Dict.Key)
				{
					case "name":
						_channelName = (string)Dict.Value;
						break;
					case "bindTo": 
						_bindToAddr = IPAddress.Parse((string)Dict.Value); 
						break;
					case "listen": 
						_wantsToListen = Boolean.Parse((string)Dict.Value);; 
						break; 
					case "machineName": 
						_machineName = (string)Dict.Value; 
						break; 
					case "port": 
						_port =(int) Dict.Value; 
						break;
					case "suppressChannelData": 
						_bSuppressChannelData = Boolean.Parse((string)Dict.Value); 
						break;
					case "useIpAddress": 
						_bUseIpAddress = Boolean.Parse((string)Dict.Value); 
						break;
				}
			}

			SetupChannel(sinkProvider);
			
		} 


		void SetupChannel(IServerChannelSinkProvider sinkProvider)
		{
			
			SetupMachineName();
			
			_sinkProvider = sinkProvider;
			
			
			String[] urls = { this.GetChannelUri() };

			// needed for CAOs
			_channelData = new ChannelDataStore(urls);
			
			
			if(_sinkProvider == null)
				_sinkProvider = new SoapServerFormatterSinkProvider();

			
			
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
			

			_transportSink = new HttpServerTransportSink(snk);

			if(_port >= 0)
			{
				_tcpListener = new TcpListener( _bindToAddr,_port);
				// start to listen
				this.StartListening(null);
			}
		}

		public void Listen()
		{
			while(true)
			{
				Socket socket = _tcpListener.AcceptSocket();
				RequestArguments reqArg = new RequestArguments(socket,_transportSink);
				HttpThread httpThread = new HttpThread(reqArg);
			
			}

		} 

		public void StartListening(Object data)
		{
			if(!_bListening)
				_tcpListener.Start();

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
				_tcpListener.Stop();
			}

			_bListening = false;
		
		}

		
		void SetupMachineName()
		{
			if (_forcedMachineName != null)
			{
				// an explicitly configured machine name was used
				//_machineName = CoreChannel.DecodeMachineName(_forcedMachineName);
				if (_forcedMachineName.Equals("$hostName"))
				{
					_machineName =  Dns.GetHostName();
					if(_machineName == null)
							throw new ArgumentNullException("hostName");
					
				}

				else _machineName = _forcedMachineName;
			}
			else
			{
				if (!_bUseIpAddress)
					_machineName = HttpHelper.GetMachineName();
				else
				{
					if (_bindToAddr == IPAddress.Any)
						_machineName = HttpHelper.GetMachineIp();
					else
						_machineName = _bindToAddr.ToString();
				}
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
			if ((_channelData != null) && (_channelData.ChannelUris != null))
			{
				return _channelData.ChannelUris[0];
			}
			else
			{
				return "http://" + _machineName + ":" + _port;
			}
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
				return _channelData;
			}
		} // ChannelData

		public String ChannelScheme { get { return "http"; } }


		public bool WantsToListen 
		{ 
			get { return _wantsToListen; } 
			set { _wantsToListen = value; }
		} 

		
		public IServerChannelSink ChannelSinkChain { get { return _sinkChain; } }

		
		public void AddHookChannelUri(String channelUri)
		{
		} 
        
		public Object this[Object key]
		{
			get { return null; }
        
			set
			{
				switch((string)key)
				{
					case "":
						break;
				}
			}
		} 
    
		
		public ICollection Keys
		{
			get
			{
				return new ArrayList(); 
			}
		}

		

	} // HttpServerChannel

    


	internal class HttpServerTransportSink : IServerChannelSink
	{
		private static String s_serverHeader =
			"mono .NET Remoting, mono .NET CLR " + System.Environment.Version.ToString();
    
		// sink state
		private IServerChannelSink _nextSink;
        

		public HttpServerTransportSink(IServerChannelSink nextSink)
		{
			_nextSink = nextSink;

		} // IServerChannelSink
        
	
    
		internal void ServiceRequest(Socket socket , Stream requestStream , ITransportHeaders headers)
		{          
			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
			sinkStack.Push(this, socket);

			IMessage responseMessage;
			ITransportHeaders responseHeaders;
			Stream responseStream;

			ServerProcessing processing= ServerProcessing.Complete;
			try
			{
				processing =
					_nextSink.ProcessMessage(sinkStack, null, headers, requestStream,
					out responseMessage,
					out responseHeaders, out responseStream);


			
				switch (processing)
				{                    
					case ServerProcessing.Complete:
						sinkStack.Pop(this);
						if(!HttpServer.SendResponse(socket,200,responseHeaders,responseStream))
						{
							//ooops couldnot send response !!!!!! and error occured
						}
						break;

					case ServerProcessing.OneWay:				
						if(!HttpServer.SendResponse(socket,200,null,null))
						{
							//ooops couldnot send response !!!!!! and error occured
						}
						break;

					case ServerProcessing.Async:
						break;
				}
			}
			catch(Exception )
			{
			}

} 
      



		//
		// IServerChannelSink implementation
		//

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack,
			IMessage requestMsg,
			ITransportHeaders requestHeaders, Stream requestStream,
			out IMessage responseMsg, out ITransportHeaders responseHeaders,
			out Stream responseStream)
		{
		
			throw new NotSupportedException();
		
		} // ProcessMessage
           

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, Object state,
			IMessage msg, ITransportHeaders headers, Stream stream)                 
		{
			Socket socket = (Socket) state;
			
			if(!HttpServer.SendResponse(socket,200,headers,stream))
			{
				//Ooops could not send response!!!!!!!!!1
			}
		} // AsyncProcessResponse


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
			get { return null; }
		} // Properties
        

		internal static String ServerHeader
		{
			get { return s_serverHeader; }
		}
        
        
	} // HttpServerTransportSink



	internal class ErrorMessage: IMethodCallMessage
	{

		// IMessage
		public IDictionary Properties     { get{ return null;} }

		// IMethodMessage
		public String Uri                      { get{ return m_URI; } }
		public String MethodName               { get{ return m_MethodName; }}
		public String TypeName                 { get{ return m_TypeName; } }
		public Object MethodSignature          { get { return m_MethodSignature;} }
		public MethodBase MethodBase           { get { return null; }}
		public int ArgCount                    { get { return m_ArgCount;} }
		public String GetArgName(int index)    { return m_ArgName; }
		public Object GetArg(int argNum)       { return null;}
		public Object[] Args                   { get { return null;} }

		public bool HasVarArgs                 { get { return false;} }
		public LogicalCallContext LogicalCallContext { get { return null; }}


		// IMethodCallMessage
		public int InArgCount                  { get { return m_ArgCount;} }
		public String GetInArgName(int index)   { return null; }
		public Object GetInArg(int argNum)      { return null;}
		public Object[] InArgs                { get { return null; }}

		String m_URI = "Exception";
		String m_MethodName = "Unknown";
		String m_TypeName = "Unknown";
		Object m_MethodSignature = null;
		int m_ArgCount = 0;
		String m_ArgName = "Unknown";
	} // ErrorMessage




} // namespace System.Runtime.Remoting.Channels.Http
