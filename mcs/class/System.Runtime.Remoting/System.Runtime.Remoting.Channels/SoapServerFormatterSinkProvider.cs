//
// System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Channels
{
	public class SoapServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		private IServerChannelSinkProvider _next;
		SoapCore _soapCore;

#if NET_1_0
		internal static string[] AllowedProperties = new string [] { "includeVersions", "strictBinding" };
#else
		internal static string[] AllowedProperties = new string [] { "includeVersions", "strictBinding", "typeFilterLevel" };
#endif

		public SoapServerFormatterSinkProvider ()
		{
			_soapCore = SoapCore.DefaultInstance;
		}

		public SoapServerFormatterSinkProvider (IDictionary properties,
							ICollection providerData)
		{
			_soapCore = new SoapCore (this, properties, AllowedProperties);
		}

		public IServerChannelSinkProvider Next
		{
			get { return _next;	}

			set { _next = value; }
		}

#if NET_1_1
		[ComVisible(false)]
		public TypeFilterLevel TypeFilterLevel
		{
			get { return _soapCore.TypeFilterLevel; }
			set 
			{
				IDictionary props = (IDictionary) ((ICloneable)_soapCore.Properties).Clone ();
				props ["typeFilterLevel"] = value;
				_soapCore = new SoapCore (this, props, AllowedProperties);
			}
		}
#endif

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink chain = _next.CreateSink(channel);
			SoapServerFormatterSink sinkFormatter = new SoapServerFormatterSink(SoapServerFormatterSink.Protocol.Http, chain, channel);
			sinkFormatter.SoapCore = _soapCore;
			
			return sinkFormatter;
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			if(_next != null)
				_next.GetChannelData(channelData);
		}
	}
}
