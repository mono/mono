//
// System.Runtime.Remoting.Channels.CrossDomainChannel.cs
//
// Author: Patrik Torstensson (totte_mono@yahoo.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;   
using System.Runtime.Remoting.Channels; 
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels 
{

	// Holds the cross appdomain channel data (used to get/create the correct sink)
	[Serializable]
	internal class CrossAppDomainChannelData 
	{
		// TODO: Add context support
		private int _domainId;

		internal CrossAppDomainChannelData(int domainId) 
		{
			_domainId = domainId;
		}

		internal int DomainID 
		{  
			get { return _domainId;	}
		}
	}

	// Responsible for marshalling objects between appdomains
	[Serializable]
	internal class CrossAppDomainChannel : IChannel, IChannelSender, IChannelReceiver 
	{
		private const String _strName = "MONOCAD";
		private const String _strBaseURI = "MONOCADURI";
		
		private static Object s_lock = new Object();

		internal static void RegisterCrossAppDomainChannel() 
		{
			lock (s_lock) 
			{
				// todo: make singleton
				CrossAppDomainChannel monocad = new CrossAppDomainChannel();
				ChannelServices.RegisterChannel ((IChannel) monocad);
			}
		}		

		// IChannel implementation
		public virtual String ChannelName 
		{
			get { return _strName; }
		}
    
		public virtual int ChannelPriority 
		{
			get { return 100; }
		}
		
		public String Parse(String url, out String objectURI) 
		{
			objectURI = url;
			return null;
		}	

		// IChannelReceiver
		public virtual Object ChannelData 
		{
			get { return new CrossAppDomainChannelData(Thread.GetDomainID()); }
		}	
		
		public virtual String[] GetUrlsForUri(String objectURI) 
		{
			throw new NotSupportedException("CrossAppdomain channel dont support UrlsForUri");
		}	
		
		// Dummies
		public virtual void StartListening(Object data) {}
		public virtual void StopListening(Object data) {}	

		// IChannelSender
		public virtual IMessageSink CreateMessageSink(String url, Object data, out String uri) 
		{
			uri = null;
			IMessageSink sink = null;
            
			if (url == null && data != null) 
			{
				// Get the data and then get the sink
				CrossAppDomainChannelData cadData = data as CrossAppDomainChannelData;
				if (cadData != null) 
					// GetSink creates a new sink if we don't have any (use contexts here later)
					sink = CrossAppDomainSink.GetSink(cadData.DomainID);
			} 
			else 
			{
				if (url != null && data == null) 
				{
					if (url.StartsWith(_strName)) 
					{
						throw new NotSupportedException("Can't create a named channel via crossappdomain");
					}
				}
			}

			return sink;
		}

	}
	
	[MonoTODO("Handle domain unloading?")]
	internal class CrossAppDomainSink : IMessageSink 
	{
		private static Hashtable s_sinks = new Hashtable();

		private int _domainID;

		internal CrossAppDomainSink(int domainID) 
		{
			_domainID = domainID;
		}
		
		internal static CrossAppDomainSink GetSink(int domainID) 
		{
			// Check if we have a sink for the current domainID
			// note, locking is not to bad here, very few class to GetSink
			lock (s_sinks.SyncRoot) 
			{
				if (s_sinks.ContainsKey(domainID)) 
					return (CrossAppDomainSink) s_sinks[domainID];
				else 
				{
					CrossAppDomainSink sink = new CrossAppDomainSink(domainID);
					s_sinks[domainID] = sink;

					return sink;
				}
			}
		}

		public virtual IMessage SyncProcessMessage(IMessage msgRequest) 
		{
			IMessage retMessage = null;

			try 
			{
				// Serialize the request message
				MemoryStream reqMsgStream = CADSerializer.SerializeMessage(msgRequest);

				// Time to transit into the "our" domain
				byte [] arrResponse = null;
				byte [] arrRequest = reqMsgStream.GetBuffer();
				
				// TODO: Enable again when we have support in the runtime
				//AppDomain currentDomain = AppDomain.EnterDomain ( _domainID );
				try 
				{
					IMessage reqDomMsg = CADSerializer.DeserializeMessage (new MemoryStream(arrRequest), null);

					IMessage retDomMsg = ChannelServices.SyncDispatchMessage (reqDomMsg);

					arrResponse = CADSerializer.SerializeMessage (retDomMsg).GetBuffer();
				}
				catch (Exception e) 
				{
					IMessage errorMsg = new ReturnMessage (e, new ErrorMessage());
					arrResponse = CADSerializer.SerializeMessage (errorMsg).GetBuffer(); 
				}   
				finally 
				{
					// TODO: Enable again when we have support in the runtime
					// AppDomain.EnterDomain (AppDomain.getIDFromDomain (currentDomain));
				}

				if (null != arrResponse) 
				{
					// Time to deserialize the message
					MemoryStream respMsgStream = new MemoryStream(arrResponse);

					// Deserialize the response message
					retMessage = CADSerializer.DeserializeMessage(respMsgStream, msgRequest as IMethodCallMessage);
				}
			}
			catch (Exception e) 
			{
				try
				{
					retMessage = new ReturnMessage (e, msgRequest as IMethodCallMessage);
				}
				catch (Exception)
				{
					// this is just to be sure
				}
			}

	    		return retMessage;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink) 
		{
			throw new NotSupportedException();
		}
		
		public IMessageSink NextSink { get { return null; } }
	}

	internal class CADSerializer 
	{
		internal static IMessage DeserializeMessage(MemoryStream mem, IMethodCallMessage msg)
		{
			BinaryFormatter serializer = new BinaryFormatter();                

			serializer.SurrogateSelector = null;
			mem.Position = 0;

			return (IMessage) serializer.Deserialize(mem);
		}
		
		internal static MemoryStream SerializeMessage(IMessage msg)
		{
			MemoryStream mem = new MemoryStream();
			BinaryFormatter serializer = new BinaryFormatter();                

			serializer.SurrogateSelector = new RemotingSurrogateSelector();
			serializer.Serialize(mem, msg);

			mem.Position = 0;

			return mem;
		}
	}
}
