//
// System.Runtime.Remoting.Contexts.IContributeObjectSink.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts {
	
	public interface IContributeObjectSink
	{
		IMessageSink GetObjectSink (MarshalByRefObject obj, IMessageSink nextSink);
	}
}
