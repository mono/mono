//
// System.Runtime.Remoting.Channels.Simple.SimpleServerFormatterSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels.Simple
{
	public class SimpleServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		IServerChannelSinkProvider next = null;

		public SimpleServerFormatterSinkProvider ()
		{
		}

		[MonoTODO]
		public SimpleServerFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
	        {
			throw new NotImplementedException ();
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
			SimpleServerFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel);
			
			result = new SimpleServerFormatterSink (next_sink);

			// set properties on result
			
			return result;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			// no idea why we need this
		}
	}
}
