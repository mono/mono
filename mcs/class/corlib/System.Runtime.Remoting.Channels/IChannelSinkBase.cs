//
// System.Runtime.Remoting.Channels.IChannelSinkBase.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels {

	public interface IChannelSinkBase
	{
		IDictionary Properties { get; }
	}
}
