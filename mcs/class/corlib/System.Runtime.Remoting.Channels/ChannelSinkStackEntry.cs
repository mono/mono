//
// System.Runtime.Remoting.ChanelSinkStackEntry.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;

namespace System.Runtime.Remoting.Channels
{
	/// <summary>
	/// Used to store sink information in a SinkStack
	/// </summary>
	internal class ChanelSinkStackEntry
	{
		public IChannelSinkBase Sink;
		public object State;
		public ChanelSinkStackEntry Next;

		public ChanelSinkStackEntry(IChannelSinkBase sink, object state, ChanelSinkStackEntry next)
		{
			Sink = sink;
			State = state;
			Next = next;
		}
	}
}
