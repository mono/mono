//
// System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSinkProvider :
		IClientFormatterSinkProvider, IClientChannelSinkProvider
	{
		IClientChannelSinkProvider next = null;
		BinaryCore _binaryCore;

		public BinaryClientFormatterSinkProvider ()
		{
			_binaryCore = BinaryCore.DefaultInstance;
		}

		public BinaryClientFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
		{
			_binaryCore = new BinaryCore (this, properties);
		}

		public IClientChannelSinkProvider Next
		{
			get {
				return next;
			}
			
			set {
				next = value;
			}
		}

		public IClientChannelSink CreateSink (IChannelSender channel,
						      string url,
						      object remoteChannelData)
		{
			IClientChannelSink next_sink = null;
			BinaryClientFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel, url, remoteChannelData);
			
			result = new BinaryClientFormatterSink (next_sink);
			result.BinaryCore = _binaryCore;

			return result;
		}		
	}
}
