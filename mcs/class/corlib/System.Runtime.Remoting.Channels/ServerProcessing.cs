//
// System.Runtime.Remoting.Channels.ServerProcessing.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	[Serializable]
	public enum ServerProcessing
	{
		Complete = 0,
		OneWay = 1,
		Async = 2,
	}
}
