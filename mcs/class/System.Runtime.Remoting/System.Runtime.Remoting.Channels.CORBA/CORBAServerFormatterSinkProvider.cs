//
// System.Runtime.Remoting.Channels.CORBA.CORBAServerFormatterSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		IServerChannelSinkProvider next = null;

		public CORBAServerFormatterSinkProvider ()
		{
		}

		[MonoTODO]
		public CORBAServerFormatterSinkProvider (IDictionary properties,
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
			CORBAServerFormatterSink result;
			
			if (next != null)
				next_sink = next.CreateSink (channel);
			
			result = new CORBAServerFormatterSink (next_sink);

			// set properties on result
			
			return result;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			// no idea why we need this
		}
	}
}
