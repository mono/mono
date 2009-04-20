// 
// DuplexSessionChannelBase.cs
// 
// Author:
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Channels
{
	internal abstract class DuplexChannelBase : ChannelBase, IDuplexChannel
	{
		ChannelFactoryBase channel_factory_base;
		ChannelListenerBase channel_listener_base;
		EndpointAddress local_address;
		EndpointAddress remote_address;
		Uri via;
		
		public DuplexChannelBase (ChannelFactoryBase factory, EndpointAddress remoteAddress, Uri via) : base (factory)
		{
			channel_factory_base = factory;
			remote_address = remoteAddress;
			this.via = via;
		}
		
		public DuplexChannelBase (ChannelListenerBase listener) : base (listener)
		{
			channel_listener_base = listener;
		}

		public abstract EndpointAddress LocalAddress { get; }
		
		public EndpointAddress RemoteAddress {
			get { return remote_address; }
		}

		public Uri Via {
			get { return via; }
		}
		
		public virtual IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			return BeginSend (message, this.DefaultSendTimeout, callback, state);
		}
		
		public abstract IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state);
		
		public abstract void EndSend (IAsyncResult result);
		
		public virtual void Send (Message message)
		{
			Send (message, this.DefaultSendTimeout);
		}

		public virtual void Send (Message message, TimeSpan timeout)
		{
			EndSend (BeginSend (message, timeout, null, null));
		}

		public virtual IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return BeginReceive (this.DefaultReceiveTimeout, callback, state);
		}

		public abstract IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state);
		
		public abstract IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state);
		
		public abstract IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state);
		
		public abstract Message EndReceive (IAsyncResult result);
		
		public abstract bool EndTryReceive (IAsyncResult result, out Message message);
		
		public abstract bool EndWaitForMessage (IAsyncResult result);
		
		public virtual Message Receive ()
		{
			return Receive (this.DefaultReceiveTimeout);
		}

		public virtual Message Receive (TimeSpan timeout)
		{
			return EndReceive (BeginReceive (timeout, null, null));
		}
		
		public virtual bool TryReceive (TimeSpan timeout, out Message message)
		{
			return EndTryReceive (BeginTryReceive (timeout, null, null), out message);
		}

		public virtual bool WaitForMessage (TimeSpan timeout)
		{
			return EndWaitForMessage (BeginWaitForMessage (timeout, null, null));
		}
	}
}
