//
// System.Runtime.Remoting.Channels.ChannelServices.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
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
			IServerChannelSinkProvider provider,
			IChannelReceiver channel)
	        {
			IServerChannelSinkProvider tmp = provider;
			while (tmp.Next != null) tmp = tmp.Next;
			tmp.Next = new ServerDispatchSinkProvider ();

			return  provider.CreateSink (channel);
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
			// fixme: sort it by priority
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
