//
// System.Runtime.Remoting.Channels.ChannelDataStore.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels
{
	public class ChannelDataStore : IChannelDataStore
	{
		private string[] channelUris;
		
		public ChannelDataStore (string[] uris)
		{
			channelUris = uris;
		}

		public string[] ChannelUris
		{
			get {
				return channelUris;
			}
			set {
				channelUris = value;
			}
		}

		public object this[object key]
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
	}
}
