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
		string[] channelUris;
		Hashtable hash;
		
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
			get {
				if (hash == null)
					hash = new Hashtable ();
				
				return hash [key];
			}

			set {
				if (hash == null)
					hash = new Hashtable ();
				
				hash [key] = value;
			}
		}
	}
}
