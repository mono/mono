//
// System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		IServerChannelSinkProvider next = null;
		BinaryCore _binaryCore;
		IDictionary _properties;
		
#if NET_1_0
		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding" };
#else
		static string[] allowedProperties = new string [] { "includeVersions", "strictBinding", "typeFilterLevel" };
#endif

		public BinaryServerFormatterSinkProvider ()
		{
			_binaryCore = BinaryCore.DefaultInstance;
		}

		public BinaryServerFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
		{
			_properties = properties;
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

#if NET_1_1
		public TypeFilterLevel TypeFilterLevel
		{
			get { return _binaryCore.TypeFilterLevel; }
			set 
			{
				if (_properties == null) _properties = new Hashtable ();
				_properties ["typeFilterLevel"] = value;
				_binaryCore = new BinaryCore (this, _properties, allowedProperties);
			}
		}
#endif

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

		public void GetChannelData (IChannelDataStore channelData)
		{
			// Nothing to add here
		}
	}
}
