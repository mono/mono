//
// System.Runtime.Remoting.Channels.ChannelServices.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez Gual (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	internal class ChannelInfoStore : IChannelInfo
	{
		object [] data = null;

		public ChannelInfoStore ()
		{
			this.data = ChannelServices.GetCurrentChannelInfo ();
		}
		
		public object[] ChannelData {

			get {
				return data;
			}
			
			set {
				data = value;
			}
		}
	}
	
	public sealed class ChannelServices
	{
		private static ArrayList registeredChannels = new ArrayList ();
		
		private ChannelServices ()
		{
		}

		public static IMessageSink CreateClientChannelSinkChain(string url, object remoteChannelData, out string objectUri)
		{
			// Locate a channel that can parse the url. This channel will be used to
			// create the sink chain.

			foreach (IChannel c in registeredChannels) 
			{
				IChannelSender sender = c as IChannelSender;
				
				if (sender != null) 
				{
					IMessageSink sink = sender.CreateMessageSink (url, remoteChannelData, out objectUri);
					if (sink != null) return sink;		// URL is ok, this is the channel and the sink
				}
			}
			objectUri = null;
			return null;
		}
		
		public static IChannel[] RegisteredChannels
		{
			get {
				IChannel[] channels = new IChannel[registeredChannels.Count];

				for (int i = 0; i < registeredChannels.Count; i++)
					channels[i] = (IChannel) registeredChannels[i];

				return channels;
			}
		}

		[MonoTODO]
		public static IMessageCtrl AsyncDispatchMessage (IMessage msg,
								 IMessageSink replySink)
		{
			throw new NotImplementedException ();
		}

		public static IServerChannelSink CreateServerChannelSinkChain (
			IServerChannelSinkProvider provider, IChannelReceiver channel)
	    {
			IServerChannelSinkProvider tmp = provider;
			while (tmp.Next != null) tmp = tmp.Next;
			tmp.Next = new ServerDispatchSinkProvider ();

			// Every provider has to call CreateSink() of its next provider
			return  provider.CreateSink (channel);
		}

		[MonoTODO]
		public static ServerProcessing DispatchMessage (
			IServerChannelSinkStack sinkStack,
			IMessage msg,
			out IMessage replyMsg)
		{
			// TODO: put Identity in message
			// TODO: Async processing

			IMethodMessage call = (IMethodMessage)msg;
			Identity identity = RemotingServices.GetIdentityForUri(call.Uri);
			if (identity == null) throw new RemotingException ("No receiver for uri " + call.Uri);

			replyMsg = identity.Context.GetServerContextSinkChain().SyncProcessMessage (msg);

			return ServerProcessing.Complete;
		}

		[MonoTODO]
		public static IChannel GetChannel (string name)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDictionary GetChannelSinkProperties (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string[] GetUrlsForObject (MarshalByRefObject obj)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterChannel (IChannel chnl)
		{
			// Put the channel in the correct place according to its priority.
			// Since there are not many channels, a linear search is ok.

			for (int n = 0; n < registeredChannels.Count; n++) {
				if ( ((IChannel)registeredChannels[n]).ChannelPriority < chnl.ChannelPriority)
				{
					registeredChannels.Insert (n, chnl);
					return;
				}
			}
			registeredChannels.Add (chnl);
		}

		[MonoTODO]
		public static IMessage SyncDispatchMessage (IMessage msg)
		{
			throw new NotImplementedException ();
		}

		public static void UnregisterChannel (IChannel chnl)
		{
			if (chnl == null)
				throw new ArgumentNullException ();
			if (!registeredChannels.Contains ((object) chnl))
				throw new RemotingException ();

			registeredChannels.Remove ((object) chnl);
		}

		internal static object [] GetCurrentChannelInfo ()
		{
			ArrayList list = new ArrayList ();
			
			foreach (object chnl_obj in registeredChannels) {
				IChannelReceiver chnl = chnl_obj as IChannelReceiver;
			
				if (chnl != null) {
					object chnl_data = chnl.ChannelData;
					if (chnl_data != null)
						list.Add (chnl_data);
				}
			}

			return  list.ToArray ();
		}
	}
}
