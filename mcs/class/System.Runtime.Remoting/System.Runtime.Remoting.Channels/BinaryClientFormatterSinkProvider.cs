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

		// this is not used at the moment ??
		IDictionary properties;
		
		public BinaryClientFormatterSinkProvider ()
		{
			properties = new Hashtable ();
		}

		public BinaryClientFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
	        {
			this.properties = properties;
			// fixme: what shall we do with providerData?
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

			if (next != null)
				next_sink = next.CreateSink (channel, url, remoteChannelData);
			
			return new BinaryClientFormatterSink (next_sink);
		}		
	}
}
