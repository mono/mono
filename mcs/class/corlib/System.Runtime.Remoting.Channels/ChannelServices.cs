//
// System.Runtime.Remoting.Channels.ChannelServices.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public sealed class ChannelServices
	{
		private static ArrayList registeredChannels = new ArrayList ();
		
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

		[MonoTODO]
		public static IServerChannelSink CreateServerChannelSinkChain (
			IServerChannelSinkProvider provider,
			IChannelReceiver channel)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServerProcessing DispatchMessage (
			IServerChannelSinkStack sinkStack,
			IMessage msg,
			out IMessage replyMsg)
		{
			throw new NotImplementedException ();
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
			registeredChannels.Add ((object) chnl);
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
	}
}
