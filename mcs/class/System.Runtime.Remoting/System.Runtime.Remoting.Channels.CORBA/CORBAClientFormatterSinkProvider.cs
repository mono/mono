//
// System.Runtime.Remoting.Channels.CORBA.CORBAClientFormatterSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAClientFormatterSinkProvider :
		IClientFormatterSinkProvider, IClientChannelSinkProvider
	{
		IClientChannelSinkProvider next = null;

		// add any sink properties here (private fields)
		
		public CORBAClientFormatterSinkProvider ()
		{
			// nothing to do
		}

		public CORBAClientFormatterSinkProvider (IDictionary properties,
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
			CORBAClientFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel, url, remoteChannelData);
			
			result = new CORBAClientFormatterSink (next_sink);

			// set properties on the newly creates sink

			return result;
		}		
	}
}
