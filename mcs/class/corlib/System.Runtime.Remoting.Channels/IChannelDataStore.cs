//
// System.Runtime.Remoting.Channels.IChannelDataStore.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	public interface IChannelDataStore
	{
		string [] ChannelUris { get;}

		object this [object key] { get; set; }
	}
}
