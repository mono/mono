//
// System.Runtime.Remoting.Contexts.IContributeServerContentSink.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts {
	
	public interface IContributeServerContentSink
	{
		IMessageSink GetServerContentSink (IMessageSink nextSink);
	}
}
