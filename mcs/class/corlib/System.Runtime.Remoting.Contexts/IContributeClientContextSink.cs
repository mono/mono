//
// System.Runtime.Remoting.Contexts.IContributeClientContextSink.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts {

	public interface IContributeClientContextSink
	{
		IMessageSink GetClientContextSink (IMessageSink nextSink);
	}
}
