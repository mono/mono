//
// System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		IServerChannelSinkProvider next = null;
		BinaryCore _binaryCore;
		
#if NET_1_0
		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding" };
#endif
#if NET_1_1
		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding", "typeFilterLevel" };
#endif

		public BinaryServerFormatterSinkProvider ()
		{
			_binaryCore = BinaryCore.DefaultInstance;
		}

		public BinaryServerFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
		{
			_binaryCore = new BinaryCore (this, properties, allowedProperties);
		}

		public IServerChannelSinkProvider Next
		{
			get {
				return next;
			}

			set {
				next = value;
			}
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink next_sink = null;
			BinaryServerFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel);
			
			result = new BinaryServerFormatterSink (BinaryServerFormatterSink.Protocol.Other,
								next_sink, channel);

			result.BinaryCore = _binaryCore;
			return result;
		}

		[MonoTODO]
		public void GetChannelData (IChannelDataStore channelData)
		{
			// no idea why we need this
		}
	}
}
