//
// System.Runtime.Remoting.Channels.ChannelDataStore.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	[Serializable]
	public class ChannelDataStore : IChannelDataStore
	{
		string[] _channelURIs;
		DictionaryEntry[] _extraData;
		
		public ChannelDataStore (string[] uris)
		{
			_channelURIs = uris;
		}

		public string[] ChannelUris
		{
			get {
				return _channelURIs;
			}
			set {
				_channelURIs = value;
			}
		}

		public object this[object key]
		{
			get {
				if (_extraData == null) return null;

				foreach (DictionaryEntry entry in _extraData)
					if (entry.Key.Equals (key)) return entry.Value;

				return null;
			}

			set {
				if (_extraData == null)
				{
					_extraData = new DictionaryEntry [] { new DictionaryEntry (key, value) };
				}
				else
				{
					DictionaryEntry[] tmpData = new DictionaryEntry [_extraData.Length + 1];
					_extraData.CopyTo (tmpData, 0);
					tmpData [_extraData.Length] = new DictionaryEntry (key, value);
					_extraData = tmpData;
				}
			}
		}
	}
}
