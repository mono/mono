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

		// add any sink properties here (private fields)
		
		public BinaryClientFormatterSinkProvider ()
		{
			// nothing to do
		}

		public BinaryClientFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
	        {
			// copy the contained properties to private fields
			
			// add a check that there is no providerData 
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

			// set properties on the newly creates sink

			return result;
		}		
	}
}
