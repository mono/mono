//
// System.Runtime.Remoting.Channels.CrossAppDomainChannel.cs
//
// Author: Patrik Torstensson (totte_mono@yahoo.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.IO;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;   
using System.Runtime.Remoting.Channels; 
using System.Runtime.Remoting.Contexts; 
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace System.Runtime.Remoting.Channels 
{

	// Holds the cross appdomain channel data (used to get/create the correct sink)
	[Serializable]
	internal class CrossAppDomainData 
	{
		// TODO: Add context support
		// Required for .NET compatibility
#pragma warning disable 0414
		private object _ContextID;
#pragma warning restore
		private int _DomainID;
		private string _processGuid;

		internal CrossAppDomainData(int domainId) 
		{
			_ContextID = (int) 0;
			_DomainID = domainId;
			_processGuid = RemotingConfiguration.ProcessId;
		}

		internal int DomainID 
		{  
			get { return _DomainID;	}
		}

		internal string ProcessID
		{
			get { return _processGuid; }
		}
	}

	// Responsible for marshalling objects between appdomains
	[Serializable]
	internal class CrossAppDomainChannel : IChannel, IChannelSender, IChannelReceiver 
	{
		private const String _strName = "MONOCAD";
		
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
			get { return new CrossAppDomainData(Thread.GetDomainID()); }
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
            
			if (data != null) 
			{
				// Get the data and then get the sink
				CrossAppDomainData cadData = data as CrossAppDomainData;
				if (cadData != null && cadData.ProcessID == RemotingConfiguration.ProcessId)
					// GetSink creates a new sink if we don't have any (use contexts here later)
					return CrossAppDomainSink.GetSink(cadData.DomainID);
			} 
			if (url != null && url.StartsWith(_strName)) 
				throw new NotSupportedException("Can't create a named channel via crossappdomain");

			return null;
		}
	}
	
	[MonoTODO("Handle domain unloading?")]
	internal class CrossAppDomainSink : IMessageSink 
	{
		private static Hashtable s_sinks = new Hashtable();

		private static MethodInfo processMessageMethod =
			typeof (CrossAppDomainSink).GetMethod ("ProcessMessageInDomain", BindingFlags.NonPublic|BindingFlags.Static);


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
		
		internal int TargetDomainId {
			get { return _domainID; }
		}

		private struct ProcessMessageRes {
			public byte[] arrResponse;
			public CADMethodReturnMessage cadMrm;
		}

#pragma warning disable 169
		private static ProcessMessageRes ProcessMessageInDomain (
			byte[] arrRequest,
			CADMethodCallMessage cadMsg)
	    {
			ProcessMessageRes res = new ProcessMessageRes ();

			try 
			{
				AppDomain.CurrentDomain.ProcessMessageInDomain (arrRequest, cadMsg, out res.arrResponse, out res.cadMrm);
			}
			catch (Exception e) 
			{
				IMessage errorMsg = new MethodResponse (e, new ErrorMessage());
				res.arrResponse = CADSerializer.SerializeMessage (errorMsg).GetBuffer(); 
			}
			return res;
		}
#pragma warning restore 169

		public virtual IMessage SyncProcessMessage(IMessage msgRequest) 
		{
			IMessage retMessage = null;

			try 
			{
				// Time to transit into the "our" domain
				byte [] arrResponse = null;
				byte [] arrRequest = null; 
				
				CADMethodReturnMessage cadMrm = null;
				CADMethodCallMessage cadMsg;
				
				cadMsg = CADMethodCallMessage.Create (msgRequest);
				if (null == cadMsg) {
					// Serialize the request message
					MemoryStream reqMsgStream = CADSerializer.SerializeMessage(msgRequest);
					arrRequest = reqMsgStream.GetBuffer();
				}

				Context currentContext = Thread.CurrentContext;

				try {
					// InternalInvoke can't handle out arguments, this is why
					// we return the results in a structure
					ProcessMessageRes res = (ProcessMessageRes)AppDomain.InvokeInDomainByID (_domainID, processMessageMethod, null, new object [] { arrRequest, cadMsg });
					arrResponse = res.arrResponse;
					cadMrm = res.cadMrm;
				} finally {
					AppDomain.InternalSetContext (currentContext);
				}					

				
				if (null != arrResponse) {
					// Time to deserialize the message
					MemoryStream respMsgStream = new MemoryStream(arrResponse);

					// Deserialize the response message
					retMessage = CADSerializer.DeserializeMessage(respMsgStream, msgRequest as IMethodCallMessage);
				} else
					retMessage = new MethodResponse (msgRequest as IMethodCallMessage, cadMrm);
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

		public virtual IMessageCtrl AsyncProcessMessage (IMessage reqMsg, IMessageSink replySink) 
		{
			AsyncRequest req = new AsyncRequest (reqMsg, replySink);
			ThreadPool.QueueUserWorkItem (new WaitCallback (SendAsyncMessage), req);
			return null;
		}
		
		public void SendAsyncMessage (object data)
		{
			AsyncRequest req = (AsyncRequest)data;
			IMessage response = SyncProcessMessage (req.MsgRequest);
			req.ReplySink.SyncProcessMessage (response);
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

			if (msg == null)
				return (IMessage) serializer.Deserialize(mem, null);
			else
				return (IMessage) serializer.DeserializeMethodResponse(mem, null, msg);
		}
		
		internal static MemoryStream SerializeMessage(IMessage msg)
		{
			MemoryStream mem = new MemoryStream ();
			BinaryFormatter serializer = new BinaryFormatter ();                

			serializer.SurrogateSelector = new RemotingSurrogateSelector ();
			serializer.Serialize (mem, msg);

			mem.Position = 0;

			return mem;
		}

		internal static MemoryStream SerializeObject(object obj)
		{
			MemoryStream mem = new MemoryStream ();
			BinaryFormatter serializer = new BinaryFormatter ();                

			serializer.SurrogateSelector = new RemotingSurrogateSelector ();
			serializer.Serialize (mem, obj);

			mem.Position = 0;

			return mem;
		}

		internal static object DeserializeObject(MemoryStream mem)
		{
			BinaryFormatter serializer = new BinaryFormatter();                

			serializer.SurrogateSelector = null;
			mem.Position = 0;

			return serializer.Deserialize (mem);
		}
	}
	
	internal class AsyncRequest
	{
		internal IMessageSink ReplySink;
		internal IMessage MsgRequest;
		
		public AsyncRequest (IMessage msgRequest, IMessageSink replySink)
		{
			ReplySink = replySink;
			MsgRequest = msgRequest;
		}
	}

}
