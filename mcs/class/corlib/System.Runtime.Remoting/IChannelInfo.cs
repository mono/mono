//
// System.Runtime.Remoting.IChannelInfo.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting {

	public interface IChannelInfo
	{
		object[] ChannelData { get; set; }
	}
}
